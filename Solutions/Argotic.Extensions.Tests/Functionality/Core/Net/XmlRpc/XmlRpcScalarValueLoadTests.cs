using System.Xml.XPath;

using Argotic.Net;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Covers <see cref="XmlRpcScalarValue.Load(XPathNavigator)"/>, which carried the same defect as
/// <c>XmlRpcClient.TryParseValue</c> in a type that no product code calls.
/// </summary>
/// <remarks>
///     <para>
///     Found by auditing every <c>MoveToFirstChild</c> in the three product assemblies after §2.42
///     fixed the first one. There were two, and this is the other: identical shape, identical
///     unreachable untyped branch, and <b>no caller inside the library</b> —
///     <c>TryParseValue</c> constructs its scalars directly rather than routing through this
///     <c>Load</c>. It is public, so a consumer reaches it even though nothing here does, which is
///     exactly the kind of member a suite that exercises only the library's own paths never sees.
///     </para>
///     <para>
///     §2.39's lesson, applied a second time: auditing every instance of a construct beats fixing the
///     one that was reported.
///     </para>
/// </remarks>
[TestClass]
public sealed class XmlRpcScalarValueLoadTests
{
    private static XPathNavigator Value(string xml) =>
        new XPathDocument(new StringReader(xml)).CreateNavigator()!.SelectSingleNode("//value")!;

    /// <summary>
    /// An untyped value loads, and keeps its text.
    /// </summary>
    [TestMethod]
    public void AnUntypedValue_Loads()
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value("<value>hello</value>")).ShouldBeTrue();

        scalar.Value.ShouldBe("hello");
    }

    /// <summary>
    /// An untyped value keeps <see cref="XmlRpcScalarValueType.None"/>, so that it round-trips untyped.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Deliberate, and the assertion is on the mechanism rather than on the field. <c>WriteTo</c>
    ///     branches on <c>ValueType</c>: <c>None</c> emits a bare <c>&lt;value&gt;text&lt;/value&gt;</c>,
    ///     anything else emits a typed wrapper. Setting <c>String</c> here would be defensible in
    ///     isolation and would silently rewrite every untyped value a client read into a typed one when
    ///     it wrote the document back.
    ///     </para>
    ///     <para>
    ///     The round-trip assertion is what pins it. An assertion on <c>ValueType</c> alone would pass
    ///     for a change that broke <c>WriteTo</c> instead.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnUntypedValue_RoundTripsUntyped()
    {
        XmlRpcScalarValue scalar = new();
        scalar.Load(Value("<value>hello</value>")).ShouldBeTrue();

        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.None);
        scalar.ToString().ShouldNotContain("<string>", Case.Insensitive);
        scalar.ToString().ShouldContain("hello");
    }

    /// <summary>
    /// A typed value still loads through its declared type.
    /// </summary>
    /// <remarks>
    ///     The controls. Each asserts the parsed <see cref="XmlRpcScalarValueType"/> as well as the
    ///     value, because a value that fell through to the untyped path would still carry the right
    ///     text and would differ only in its type.
    /// </remarks>
    [TestMethod]
    [DataRow("<value><i4>1138</i4></value>", XmlRpcScalarValueType.Integer)]
    [DataRow("<value><int>1138</int></value>", XmlRpcScalarValueType.Integer)]
    [DataRow("<value><string>1138</string></value>", XmlRpcScalarValueType.String)]
    [DataRow("<value><boolean>1</boolean></value>", XmlRpcScalarValueType.Boolean)]
    [DataRow("<value><double>-12.53</double></value>", XmlRpcScalarValueType.Double)]
    public void ATypedValue_KeepsItsDeclaredType(string xml, XmlRpcScalarValueType expected)
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value(xml)).ShouldBeTrue();

        scalar.ValueType.ShouldBe(expected);
    }

    /// <summary>
    /// A typed value surrounded by insignificant whitespace still takes the typed path.
    /// </summary>
    /// <remarks>
    ///     The shape that made the original defect subtle. Whether a pretty-printed payload works at all
    ///     depended on whether the <see cref="IXPathNavigable"/> the caller supplied strips whitespace-only
    ///     text nodes — <see cref="XPathDocument"/> does, <see cref="System.Xml.XmlDocument"/> does not —
    ///     so the same bytes behaved differently depending on how they had been read.
    ///     Asking for a child <em>element</em> removes that dependence.
    /// </remarks>
    [TestMethod]
    public void APrettyPrintedTypedValue_StillTakesTheTypedPath()
    {
        System.Xml.XmlDocument document = new() { PreserveWhitespace = true };
        document.LoadXml("<value>\n  <i4>1138</i4>\n</value>");

        XmlRpcScalarValue scalar = new();

        scalar.Load(document.CreateNavigator()!.SelectSingleNode("//value")!).ShouldBeTrue();

        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.Integer);
        scalar.Value.ShouldBe(1138);
    }

    /// <summary>
    /// An empty value does not load.
    /// </summary>
    /// <remarks>
    ///     The boundary control, matching <c>XmlRpcClient.TryParseValue</c>: an empty element is not a
    ///     successfully loaded empty string.
    /// </remarks>
    [TestMethod]
    [DataRow("<value></value>")]
    [DataRow("<value />")]
    public void AnEmptyValue_DoesNotLoad(string xml)
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value(xml)).ShouldBeFalse();
    }

    /// <summary>
    /// A value carrying an unrecognised type element does not load.
    /// </summary>
    /// <remarks>
    ///     The control that stops the untyped path from swallowing everything. A child element the
    ///     dispatch does not know is a failure, not text: falling back would turn
    ///     <c>&lt;value&gt;&lt;i8&gt;1&lt;/i8&gt;&lt;/value&gt;</c> into the string "1".
    /// </remarks>
    [TestMethod]
    public void AValueWithAnUnrecognisedTypeElement_DoesNotLoad()
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value("<value><i8>1138</i8></value>")).ShouldBeFalse();
    }
}