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
    /// <param name="xml">A <c>value</c> element wrapping one of the type elements XML-RPC 1.0 defines.</param>
    /// <param name="expected">The <c>XmlRpcScalarValueType</c> that spelling must parse to.</param>
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
    /// <param name="xml">An empty <c>value</c> element, in each of the two ways XML lets it be written.</param>
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
    /// A typed value whose text will not parse fails the load rather than throwing out of a <c>bool</c>.
    /// </summary>
    /// <param name="xml">A <c>value</c> element whose declared type and text disagree.</param>
    /// <remarks>
    ///     <para>
    ///     This method's contract is a <see cref="bool"/>, and four of its arms reached it through
    ///     <c>int.Parse</c>, <c>double.Parse</c>, <c>Convert.FromBase64String</c> and
    ///     <c>ParseRfc3339DateTime</c> — every one of which throws. A caller reading a
    ///     <c>&lt;bool&gt;Load</c> as a total function got a <see cref="FormatException"/> from an
    ///     ordinary malformed document.
    ///     </para>
    ///     <para>
    ///     The boolean row is the same defect wearing different clothes: it did not throw, it stored the
    ///     empty string under <c>ValueType.Boolean</c> and reported success.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("<value><i4>abc</i4></value>")]
    [DataRow("<value><int>abc</int></value>")]
    [DataRow("<value><double>abc</double></value>")]
    [DataRow("<value><base64>not*valid*base64</base64></value>")]
    [DataRow("<value><dateTime.iso8601>the day before yesterday</dateTime.iso8601></value>")]
    [DataRow("<value><boolean>maybe</boolean></value>")]
    public void ATypedValueWhoseTextWillNotParse_DoesNotLoad(string xml)
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value(xml)).ShouldBeFalse("INVERTED: a bool-returning parse reports failure by returning false");
    }

    /// <summary>
    /// The date spelling XML-RPC 1.0 uses in its own example loads.
    /// </summary>
    /// <remarks>
    ///     <c>19980717T14:08:55</c> is the specification's own example: a basic-format date, an
    ///     extended-format time, no offset. It cannot match a hyphenated RFC 3339 pattern, so the
    ///     parser this method used could only throw on it. <c>XmlRpcClient</c> has carried the two
    ///     zoneless XML-RPC spellings all along; the fix is to stop having a second answer.
    /// </remarks>
    [TestMethod]
    public void TheSpecificationsOwnDateSpelling_Loads()
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value("<value><dateTime.iso8601>19980717T14:08:55</dateTime.iso8601></value>"))
            .ShouldBeTrue("INVERTED: the spelling the specification prints is the one it could not read");

        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.DateTime);
        scalar.Value.ShouldBe(new DateTime(1998, 7, 17, 14, 8, 55, DateTimeKind.Unspecified));
    }

    /// <summary>
    /// An RFC 3339 date still loads, which is what most live servers send.
    /// </summary>
    /// <remarks>
    ///     The control on the row above. RFC 3339 is tried first and is unchanged, so nothing that
    ///     parsed before parses differently now.
    /// </remarks>
    [TestMethod]
    public void AnRfc3339Date_StillLoads()
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value("<value><dateTime.iso8601>1998-07-17T14:08:55Z</dateTime.iso8601></value>"))
            .ShouldBeTrue();

        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.DateTime);
        ((DateTime)scalar.Value!).Kind.ShouldBe(DateTimeKind.Utc);
    }

    /// <summary>
    /// A string value keeps the whitespace the document gave it, and is trimmed when written.
    /// </summary>
    /// <remarks>
    ///     A consequence of routing both parse paths through one parser rather than two: this method
    ///     used to trim on the way in and <c>XmlRpcClient.TryParseValue</c> did not, so the same
    ///     document produced two different <c>Value</c> strings depending on which entry point read it.
    ///     Trimming still happens, on the way out, where <c>ValueAsString</c> has always done it — so
    ///     the wire form is unchanged and only the in-memory value differs.
    /// </remarks>
    [TestMethod]
    public void AStringValue_KeepsItsWhitespaceAndIsTrimmedOnTheWayOut()
    {
        XmlRpcScalarValue scalar = new();

        scalar.Load(Value("<value><string>  padded  </string></value>")).ShouldBeTrue();

        scalar.Value.ShouldBe("  padded  ", "INVERTED: one parser, one answer");
        scalar.ToString().ShouldContain("<string>padded</string>", Case.Sensitive);
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