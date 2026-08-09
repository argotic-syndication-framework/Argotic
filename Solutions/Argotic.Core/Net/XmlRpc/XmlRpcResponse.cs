using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents the response to an XML remote procedure call.
/// </summary>
/// <remarks>
///     A <c>&lt;methodResponse&gt;</c> carries either one <see cref="Parameter"/> or one
///     <see cref="Fault"/>, never both, and a fault still arrives over a <c>200 OK</c> — the HTTP status
///     says only that the server was reachable. Both properties are <see langword="null"/> on a document
///     that held neither, which is what a response from a server that does not speak XML-RPC looks like
///     once it has been parsed.
/// </remarks>
/// <seealso cref="XmlRpcClient.SendAsync(XmlRpcMessage, CancellationToken)"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs" language="cs" title="The following code example demonstrates the usage of the XmlRpcResponse class." />
/// </example>
public class XmlRpcResponse : IComparable<XmlRpcResponse>, IEquatable<XmlRpcResponse>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class.
    /// </summary>
    public XmlRpcResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="IXmlRpcValue"/>.
    /// </summary>
    /// <param name="parameter">A <see cref="IXmlRpcValue"/> that represents the response value that was returned for the remote procedure call.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="parameter"/> is <see langword="null"/>.</exception>
    public XmlRpcResponse(IXmlRpcValue parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        Parameter = parameter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="XmlRpcStructureValue"/>.
    /// </summary>
    /// <param name="fault">A <see cref="XmlRpcStructureValue"/> that represents the response to the remote procedure call.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="fault"/> is <see langword="null"/>.</exception>
    public XmlRpcResponse(XmlRpcStructureValue fault)
    {
        ArgumentNullException.ThrowIfNull(fault);

        Fault = fault;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied fault code and message.
    /// </summary>
    /// <param name="faultCode">Machine-readable code that identifies the reason the remote procedure call failed. XML-RPC defines no codes of its own; they are the server's to choose.</param>
    /// <param name="faultMessage">Human-readable information about the reason the remote procedure call failed.</param>
    /// <remarks>
    ///     Builds the conventional two-member fault structure — <c>faultCode</c> as an <c>int</c> and
    ///     <c>faultString</c> as a <c>string</c> — and assigns it to <see cref="Fault"/>. Use it when
    ///     writing a response rather than reading one.
    /// </remarks>
    public XmlRpcResponse(int faultCode, string faultMessage)
    {
        XmlRpcStructureValue faultStructure = new();
        XmlRpcStructureMember codeMember = new("faultCode", new XmlRpcScalarValue(faultCode));
        XmlRpcStructureMember stringMember = new("faultString", new XmlRpcScalarValue(faultMessage));
        faultStructure.Members.Add(codeMember);
        faultStructure.Members.Add(stringMember);

        Fault = faultStructure;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="XmlRpcResponse"/> class asynchronously using the supplied <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="response">The XML-RPC server's response to the remote procedure call. Its media type must be <c>text/xml</c>, and its content length must not be explicitly <c>0</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result is the parsed <see cref="XmlRpcResponse"/>. A well-formed document with no
    ///     <c>methodResponse</c> element yields an instance with both <see cref="Parameter"/> and
    ///     <see cref="Fault"/> <see langword="null"/> rather than an exception.
    /// </returns>
    /// <remarks>
    ///     An absent <c>Content-Length</c> is accepted. XML-RPC 1.0 requires the header, but it predates
    ///     chunked transfer encoding, which omits it and which live servers use; only an explicit
    ///     <c>0</c> is rejected.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> media type is not <c>text/xml</c>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> declares a content length of <c>0</c>.</exception>
    /// <exception cref="XmlException">The <paramref name="response"/> body does not represent a valid XML document, or an error was encountered in the XML data.</exception>
    public static async Task<XmlRpcResponse> CreateAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        string? contentType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(contentType, "text/xml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"The HttpResponseMessage content type is invalid. Content type of the response was {contentType}", nameof(response));
        }

        // Note: The original XML-RPC spec (xmlrpc.com/spec.md) requires Content-Length,
        // but this predates HTTP/1.1 chunked transfer encoding (RFC 7230). Modern servers
        // commonly use Transfer-Encoding: chunked without Content-Length (-1 here).
        // We only reject explicitly empty responses (0) which would be invalid XML.
        long contentLength = response.Content.Headers.ContentLength ?? -1;
        if (contentLength == 0)
        {
            throw new ArgumentException($"The HttpResponseMessage content length is invalid. Content length was {contentLength}. ", nameof(response));
        }

        // These are public and take a response the caller may have obtained under headers-read, in
        // which case the stream below is the socket and XmlReader.Create would read it synchronously.
        // Their own callers use content-read, so this is not a live defect - it is a public method that
        // stops being able to become one.
        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using Stream stream = body.AsStream();
        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(reader);
        XPathNavigator source = document.CreateNavigator();

        XmlRpcResponse result = new();
        XPathNavigator? methodResponseNavigator = source.SelectChildElement("methodResponse");
        if (methodResponseNavigator is not null)
        {
            result.Load(methodResponseNavigator);
        }

        return result;
    }

    /// <summary>
    /// Gets the fault information that was returned for the remote procedure call.
    /// </summary>
    /// <value>
    ///     The fault structure, conventionally holding <c>faultCode</c> and <c>faultString</c> members;
    ///     <see langword="null"/> if the call executed without error, or if the response was not an
    ///     XML-RPC document at all.
    /// </value>
    /// <seealso cref="XmlRpcResponse(XmlRpcStructureValue)"/>
    /// <seealso cref="XmlRpcResponse(int, string)"/>
    public XmlRpcStructureValue? Fault { get; private set; }

    /// <summary>
    /// Gets the response information that was returned for the remote procedure call.
    /// </summary>
    /// <value>
    ///     The single value the method returned; <see langword="null"/> if the call faulted, in which
    ///     case <see cref="Fault"/> carries the reason. XML-RPC returns exactly one value, so a method
    ///     with several results returns an array or a structure holding them.
    /// </value>
    /// <seealso cref="XmlRpcResponse(IXmlRpcValue)"/>
    public IXmlRpcValue? Parameter { get; private set; }

    /// <summary>
    /// Loads this <see cref="XmlRpcResponse"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcResponse"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcResponse"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? parametersNavigator = source.SelectChildElement("params");
            XPathNavigator? faultNavigator = source.SelectChildElement("fault");

            if (parametersNavigator is not null)
            {
                XPathNavigator? valueNavigator = parametersNavigator.SelectSingleNode("param/value");
                if (valueNavigator is not null)
                {
                    if (XmlRpcClient.TryParseValue(valueNavigator, out IXmlRpcValue? value))
                    {
                        Parameter = value;
                        wasLoaded = true;
                    }
                }
            }

            if (faultNavigator is not null)
            {
                XPathNavigator? structNavigator = faultNavigator.SelectChildElement("value");
                if (structNavigator is not null)
                {
                    XmlRpcStructureValue structure = new();
                    if (structure.Load(structNavigator))
                    {
                        Fault = structure;
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcResponse"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <remarks>
    ///     Writes whichever of <see cref="Parameter"/> and <see cref="Fault"/> is set. An instance with
    ///     both set writes both, which the specification does not allow — it is the caller's business not
    ///     to build one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("methodResponse");

        if (this.Parameter is not null)
        {
            writer.WriteStartElement("params");
            writer.WriteStartElement("param");
            this.Parameter.WriteTo(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        if (this.Fault is not null)
        {
            writer.WriteStartElement("fault");
            this.Fault.WriteTo(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcResponse"/>.
    /// </summary>
    /// <returns>The <c>&lt;methodResponse&gt;</c> XML for the current instance, written as a fragment — no XML declaration.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(XmlRpcResponse? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = 0;

        result = (this.Fault, other.Fault) switch
        {
            (XmlRpcStructureValue fault, XmlRpcStructureValue otherFault) => fault.CompareTo(otherFault),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = (this.Parameter, other.Parameter) switch
        {
            (IXmlRpcValue parameter, IXmlRpcValue otherParameter) => string.Compare(parameter.ToString(), otherParameter.ToString(), StringComparison.Ordinal),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcResponse"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcResponse"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="XmlRpcResponse"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(XmlRpcResponse? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is XmlRpcResponse other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Fault), HashCodeUtility.Component(this.Parameter?.ToString()));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(XmlRpcResponse? first, XmlRpcResponse? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(XmlRpcResponse? first, XmlRpcResponse? second) => !(first == second);
}