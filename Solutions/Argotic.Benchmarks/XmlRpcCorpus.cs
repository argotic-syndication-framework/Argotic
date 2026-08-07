using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Net;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds XML-RPC payloads, one per branch of <c>XmlRpcClient.TryParseValue</c>.
/// </summary>
/// <remarks>
/// <para>
/// There is no real-world XML-RPC corpus and there cannot easily be one: an XML-RPC response is the
/// body of an HTTP POST to a weblog ping server, not something a publisher leaves at a URL to be
/// fetched. Every document this class produces is therefore synthetic, and modelled on the
/// examples in the XML-RPC 1.0 specification at <c>xmlrpc.com/spec</c> plus the <c>metaWeblog</c> and
/// <c>weblogUpdates</c> shapes the library was written to talk to.
/// </para>
/// <para>
/// The point of generating one payload per scalar type is that <c>TryParseValue</c> is a linear
/// <c>if</c>/<c>else if</c> chain over nine element names, in this order: <c>i4</c>, <c>int</c>,
/// <c>boolean</c>, <c>string</c>, <c>double</c>, <c>dateTime.iso8601</c>, <c>base64</c>,
/// <c>struct</c>, <c>array</c>. A value's position in that chain is its dispatch cost — an
/// <c>array</c> pays nine ordinal-ignore-case string comparisons before it reaches its own branch,
/// an <c>i4</c> pays one. A corpus containing only <c>i4</c> can neither see that nor refute it.
/// </para>
/// <para>
/// Every generator here emits pure 7-bit ASCII with no byte-order mark, which is what an XML-RPC
/// server actually sends and also a limitation worth stating: nothing here exercises the sanitiser's
/// rebuild path or a non-UTF-8 declaration.
/// </para>
/// </remarks>
internal static class XmlRpcCorpus
{
    private const string ResponsePrefix = "<?xml version=\"1.0\"?><methodResponse><params><param>";

    private const string ResponseSuffix = "</param></params></methodResponse>";

    /// <summary>
    /// Wraps a <c>value</c> element in the <c>methodResponse</c> envelope a server returns.
    /// </summary>
    /// <param name="valueXml">The <c>value</c> element, as produced by the generators below.</param>
    /// <returns>A complete XML-RPC method response document.</returns>
    public static string MethodResponse(string valueXml) =>
        string.Concat(ResponsePrefix, valueXml, ResponseSuffix);

    /// <summary>
    /// Returns the same document as the byte sequence it would arrive as over the wire.
    /// </summary>
    /// <param name="valueXml">The <c>value</c> element to wrap.</param>
    /// <returns>The response document as UTF-8 bytes.</returns>
    public static byte[] MethodResponseUtf8(string valueXml) =>
        Encoding.UTF8.GetBytes(MethodResponse(valueXml));

    /// <summary>
    /// Builds a <c>value</c> element carrying an explicitly typed scalar.
    /// </summary>
    /// <param name="typeName">The XML-RPC type element name, e.g. <c>i4</c> or <c>dateTime.iso8601</c>.</param>
    /// <param name="text">The scalar's text content.</param>
    /// <returns>The <c>value</c> element.</returns>
    public static string TypedScalar(string typeName, string text) =>
        string.Concat("<value><", typeName, ">", text, "</", typeName, ">", "</value>");

    /// <summary>
    /// Builds a <c>value</c> element with no type element, which the specification defines as a string.
    /// </summary>
    /// <param name="text">The scalar's text content.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     <para>
    ///     XML-RPC 1.0 is explicit: "If no type is indicated, the type is string." Real ping servers
    ///     emit this for short strings, and <c>TryParseValue</c> cannot parse it — measured, not
    ///     inferred. It returns <see langword="false"/> and a null value.
    ///     </para>
    ///     <para>
    ///     The mechanism, because it also condemns a block of code as unreachable: text is a child
    ///     node in the XPath data model, so <c>source.HasChildren</c> is <see langword="true"/> for
    ///     <c>&lt;value&gt;text&lt;/value&gt;</c>. Control therefore enters the <c>if</c>,
    ///     <c>MoveToFirstChild</c> lands on the text node whose <c>Name</c> is the empty
    ///     string, all nine name comparisons fail, and the method falls out through
    ///     <c>value = null; return false;</c>. The <c>else if (!string.IsNullOrEmpty(source.Value))</c>
    ///     tail that was written to handle this shape is only reached when the element has no children
    ///     at all — and an element with no children has an empty <c>Value</c>, so its own guard then
    ///     fails too. That tail cannot execute for any input.
    ///     </para>
    /// </remarks>
    public static string UntypedScalar(string text) => string.Concat("<value>", text, "</value>");

    /// <summary>
    /// Builds a base64 scalar whose decoded payload is <paramref name="byteCount"/> bytes.
    /// </summary>
    /// <param name="byteCount">The number of bytes the encoded text decodes to.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     The size is a parameter because <c>Convert.FromBase64String</c> is the only scalar branch
    ///     whose cost is proportional to its content rather than constant. Measuring it at one byte
    ///     would price the branch at its dispatch overhead and call that the answer.
    /// </remarks>
    public static string Base64Scalar(int byteCount)
    {
        byte[] payload = new byte[byteCount];
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(i % 251);
        }

        return TypedScalar("base64", Convert.ToBase64String(payload));
    }

    /// <summary>
    /// Builds a flat <c>struct</c> of <paramref name="memberCount"/> string-valued members.
    /// </summary>
    /// <param name="memberCount">The number of <c>member</c> elements.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     The shape a <c>metaWeblog.getPost</c> response takes: a record of named fields. Each member
    ///     costs one <c>XmlRpcStructureMember.Load</c>, two <c>SelectChildElement</c> calls and one
    ///     recursive <c>TryParseValue</c>, so the per-member cost is what this measures.
    /// </remarks>
    public static string StructValue(int memberCount)
    {
        StringBuilder builder = new(capacity: 64 + (memberCount * 96));
        builder.Append("<value><struct>");

        for (int i = 0; i < memberCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("<member><name>field").Append(ordinal).Append("</name>");
            builder.Append("<value><string>The value of field ").Append(ordinal).Append("</string></value></member>");
        }

        builder.Append("</struct></value>");
        return builder.ToString();
    }

    /// <summary>
    /// Builds a flat <c>array</c> of <paramref name="valueCount"/> integer values.
    /// </summary>
    /// <param name="valueCount">The number of <c>value</c> elements inside <c>data</c>.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     The shape a <c>system.listMethods</c> or <c>pingback</c> enumeration takes. Unlike the
    ///     struct, every element here dispatches through the first branch of the chain, so
    ///     the struct-versus-array delta is not confounded by dispatch position.
    /// </remarks>
    public static string ArrayValue(int valueCount)
    {
        StringBuilder builder = new(capacity: 64 + (valueCount * 32));
        builder.Append("<value><array><data>");

        for (int i = 0; i < valueCount; i++)
        {
            builder.Append("<value><i4>").Append(i.ToString(CultureInfo.InvariantCulture)).Append("</i4></value>");
        }

        builder.Append("</data></array></value>");
        return builder.ToString();
    }

    /// <summary>
    /// Builds a <c>struct</c> whose members are arrays, holding <paramref name="leafCount"/> leaves in total.
    /// </summary>
    /// <param name="leafCount">The total number of scalar values across every array.</param>
    /// <param name="memberCount">The number of array-valued members to spread them over.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     Two levels of recursion at a leaf count identical to the flat generators', so the delta
    ///     against them is the cost of recursing rather than the cost of parsing more values. This is
    ///     the shape a <c>metaWeblog.getRecentPosts</c> response takes.
    /// </remarks>
    public static string StructOfArrays(int leafCount, int memberCount)
    {
        int perMember = Math.Max(1, leafCount / Math.Max(1, memberCount));
        StringBuilder builder = new(capacity: 128 + (leafCount * 32) + (memberCount * 96));
        builder.Append("<value><struct>");

        for (int member = 0; member < memberCount; member++)
        {
            builder.Append("<member><name>group").Append(member.ToString(CultureInfo.InvariantCulture)).Append("</name>");
            builder.Append("<value><array><data>");

            for (int i = 0; i < perMember; i++)
            {
                builder.Append("<value><i4>").Append(i.ToString(CultureInfo.InvariantCulture)).Append("</i4></value>");
            }

            builder.Append("</data></array></value></member>");
        }

        builder.Append("</struct></value>");
        return builder.ToString();
    }

    /// <summary>
    /// Builds a chain of arrays nested <c>leafCount / leavesPerLevel</c> levels deep.
    /// </summary>
    /// <param name="leafCount">The total number of scalar leaves, spread evenly down the chain.</param>
    /// <param name="leavesPerLevel">The number of scalars held at each level of the chain.</param>
    /// <returns>The <c>value</c> element.</returns>
    /// <remarks>
    ///     <para>
    ///     Nesting depth grows linearly with <paramref name="leafCount"/> while the leaf count stays
    ///     equal to the flat generators', which is what separates recursion depth from element count.
    ///     </para>
    ///     <para>
    ///     Depth is deliberately bounded here and is not bounded in the library.
    ///     <c>TryParseValue</c> recurses into <c>XmlRpcArrayValue.Load</c>, which calls
    ///     <c>TryParseValue</c> again, with no depth limit anywhere on the path. A hostile or broken
    ///     server can therefore send a payload that overflows the stack, and the process cannot catch
    ///     it. Keeping <paramref name="leavesPerLevel"/> at ten caps this corpus at a depth of a
    ///     hundred, which is safe to measure; the unbounded case is a defect to report, not to
    ///     benchmark.
    ///     </para>
    /// </remarks>
    public static string ArrayChain(int leafCount, int leavesPerLevel)
    {
        int depth = Math.Max(1, leafCount / Math.Max(1, leavesPerLevel));
        StringBuilder builder = new(capacity: 128 + (leafCount * 32) + (depth * 64));

        for (int level = 0; level < depth; level++)
        {
            builder.Append("<value><array><data>");

            for (int i = 0; i < leavesPerLevel; i++)
            {
                builder.Append("<value><i4>").Append(i.ToString(CultureInfo.InvariantCulture)).Append("</i4></value>");
            }
        }

        for (int level = 0; level < depth; level++)
        {
            builder.Append("</data></array></value>");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a complete <c>methodResponse</c> carrying a fault rather than a parameter.
    /// </summary>
    /// <returns>The fault response document.</returns>
    /// <remarks>
    ///     The fault path is a different branch of <c>XmlRpcResponse.Load</c> from the parameter path
    ///     and reaches <c>XmlRpcStructureValue.Load</c> directly. Both were at zero coverage; a
    ///     benchmark that only ever sends a successful call measures half of what a client meets.
    /// </remarks>
    public static string FaultResponse() =>
        string.Concat(
            "<?xml version=\"1.0\"?><methodResponse><fault><value><struct>",
            "<member><name>faultCode</name><value><int>4</int></value></member>",
            "<member><name>faultString</name><value><string>Too many parameters.</string></value></member>",
            "</struct></value></fault></methodResponse>");

    /// <summary>
    /// Builds a <c>methodCall</c> request body with <paramref name="parameterCount"/> string parameters.
    /// </summary>
    /// <param name="methodName">The remote method name.</param>
    /// <param name="parameterCount">The number of parameters to carry.</param>
    /// <returns>The message the client would serialise and POST.</returns>
    public static XmlRpcMessage MethodCall(string methodName, int parameterCount)
    {
        XmlRpcMessage message = new(methodName);

        for (int i = 0; i < parameterCount; i++)
        {
            message.Parameters.Add(new XmlRpcScalarValue($"parameter {i.ToString(CultureInfo.InvariantCulture)}"));
        }

        return message;
    }

    /// <summary>
    /// Builds a navigator positioned on the <c>value</c> node, exactly where the library positions one.
    /// </summary>
    /// <param name="valueXml">The <c>value</c> element to wrap and navigate to.</param>
    /// <returns>A navigator on <c>/methodResponse/params/param/value</c>.</returns>
    /// <remarks>
    ///     Document construction is deliberately outside the measured region: the subject of the value
    ///     benchmarks is the walk from an <see cref="XPathNavigator"/> into the object model, and
    ///     <c>XmlRpcResponse.CreateAsync</c> is the arm that prices the document build on top of it.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The generated document has no value node, which indicates a defective generator.
    /// </exception>
    public static XPathNavigator ValueNavigator(string valueXml)
    {
        // Through the same reader settings the library's own load path uses - DTD resolution off, no
        // XmlResolver - rather than the XPathDocument(TextReader) overload, which leaves both open.
        using StringReader stringReader = new(MethodResponse(valueXml));
        using XmlReader reader = XmlReader.Create(stringReader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(reader);

        return document.CreateNavigator().SelectSingleNode("/methodResponse/params/param/value")
            ?? throw new InvalidOperationException(
                "The generated XML-RPC document has no /methodResponse/params/param/value node, so the benchmark would measure a null parse rather than a value parse.");
    }
}