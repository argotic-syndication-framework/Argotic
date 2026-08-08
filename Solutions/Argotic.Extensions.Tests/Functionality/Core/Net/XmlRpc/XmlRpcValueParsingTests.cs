using Argotic.Net;
namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Covers <see cref="XmlRpcClient.TryParseValue"/> against the spellings XML-RPC 1.0 actually defines.
/// </summary>
/// <remarks>
///     <para>
///     Written because building <c>XmlRpcValueParsingBenchmarks</c> found two spec-legal payloads the
///     parser rejects outright. Both are in the specification's own text, and neither had a test.
///     </para>
///     <para>
///     The untyped case is the sharper of the two, because the branch written to handle it could never
///     run. <c>&lt;value&gt;hi&lt;/value&gt;</c> <em>does</em> have children — a text node is a child —
///     so the typed dispatch is entered, <c>MoveToFirstChild</c> lands on the text node whose
///     <c>Name</c> is the empty string, nothing matches, and the method falls out returning
///     <see langword="false"/>. The untyped fallback sat in the <c>else</c> of
///     <c>if (source.HasChildren)</c>, which is reachable only when the element is empty, which its own
///     <c>!string.IsNullOrEmpty</c> guard then rejects. <b>Unreachable for every possible input.</b>
///     </para>
/// </remarks>
[TestClass]
public sealed class XmlRpcValueParsingTests
{
    private static XPathNavigator Value(string xml) =>
        new XPathDocument(new StringReader(xml)).CreateNavigator()!.SelectSingleNode("//value")!;

    /// <summary>
    /// An untyped value is a string, which is what XML-RPC 1.0 says it is.
    /// </summary>
    /// <remarks>
    ///     XML-RPC 1.0, on <c>&lt;value&gt;</c>: "If no type is indicated, the type is string." Real
    ///     ping servers emit this spelling.
    /// </remarks>
    [TestMethod]
    public void AnUntypedValue_IsAString()
    {
        XmlRpcClient.TryParseValue(Value("<value>hello</value>"), out IXmlRpcValue? value).ShouldBeTrue();

        value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe("hello");
    }

    /// <summary>
    /// The explicitly typed spelling of the same value is unchanged.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The control. Without it, the test above passing is equally consistent with "everything is now
    ///     a string", which would break every other branch of the dispatch.
    ///     </para>
    ///     <para>
    ///     The trailing text after <c>&lt;/string&gt;</c> is what makes the control able to fail. With a
    ///     bare <c>&lt;value&gt;&lt;string&gt;hello&lt;/string&gt;&lt;/value&gt;</c>,
    ///     <see cref="System.Xml.XPath.XPathNavigator.Value"/> on the outer element is also
    ///     <c>"hello"</c> — so deleting the <c>string</c> arm of the dispatch and letting the input fall
    ///     through to the untyped path produced the identical result and the control passed. Here the
    ///     element's string-value is <c>"hellotail"</c> and only the typed arm answers <c>"hello"</c>.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnExplicitlyTypedString_IsStillAString()
    {
        XmlRpcClient.TryParseValue(Value("<value><string>hello</string>tail</value>"), out IXmlRpcValue? value)
            .ShouldBeTrue();

        value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe("hello");
    }

    /// <summary>
    /// The typed branches still answer with their own types, not with strings.
    /// </summary>
    /// <param name="xml">A <c>value</c> element wrapping an integer, in one of its two legal spellings.</param>
    /// <param name="expected">The <c>int</c> the parsed scalar must hold.</param>
    /// <remarks>
    ///     The second control, and the one that matters: making an untyped value parse means loosening
    ///     the dispatch, and the way to get that wrong is to catch typed payloads on the way past. Each
    ///     row asserts the parsed CLR type, so a value that fell through to the string path fails here
    ///     rather than passing as a plausible-looking string.
    /// </remarks>
    [TestMethod]
    [DataRow("<value><i4>42</i4></value>", 42)]
    [DataRow("<value><int>42</int></value>", 42)]
    public void ATypedIntegerValue_IsStillAnInteger(string xml, int expected)
    {
        XmlRpcClient.TryParseValue(Value(xml), out IXmlRpcValue? value).ShouldBeTrue();

        value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(expected);
    }

    /// <summary>
    /// A boolean and a double keep their own types too.
    /// </summary>
    [TestMethod]
    public void ATypedBooleanAndDouble_KeepTheirTypes()
    {
        XmlRpcClient.TryParseValue(Value("<value><boolean>1</boolean></value>"), out IXmlRpcValue? boolean)
            .ShouldBeTrue();
        boolean.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(true);

        XmlRpcClient.TryParseValue(Value("<value><double>3.5</double></value>"), out IXmlRpcValue? number)
            .ShouldBeTrue();
        number.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(3.5d);
    }

    /// <summary>
    /// A date is read in the spelling XML-RPC 1.0 uses for it, and in RFC 3339.
    /// </summary>
    /// <param name="spelling">The text content of the <c>dateTime.iso8601</c> element, one spelling per row.</param>
    /// <param name="kind">The <c>DateTimeKind</c> that spelling must yield: <c>Unspecified</c> where it carries no offset.</param>
    /// <param name="label">The row's description, passed to every assertion so a failure names the spelling that broke.</param>
    /// <remarks>
    ///     <para>
    ///     The element is named <c>dateTime.iso8601</c> and XML-RPC 1.0's own example spells it
    ///     <c>19980717T14:08:55</c> — no hyphens, no offset. That value was routed to an RFC 3339
    ///     pattern table which cannot match it, so the specification's own example failed to parse.
    ///     </para>
    ///     <para>
    ///     <b>The <c>Kind</c> assertions are the load-bearing part.</b> A zoneless spelling carries no
    ///     offset, so the only honest answer is <see cref="DateTimeKind.Unspecified"/>; claiming
    ///     <see cref="DateTimeKind.Utc"/> would invent an offset the wire never carried. This is the
    ///     §2.37 lesson — a suite running on a UTC machine cannot tell <c>Utc</c> from <c>Local</c> by
    ///     value, so the assertion has to name the <c>Kind</c>.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("19980717T14:08:55", DateTimeKind.Unspecified, "the XML-RPC 1.0 spelling")]
    [DataRow("19980717T140855", DateTimeKind.Unspecified, "fully basic ISO 8601")]
    [DataRow("1998-07-17T14:08:55Z", DateTimeKind.Utc, "RFC 3339, which already worked")]

    // Z is offset zero, so on the row above the rebase to UTC is the identity: a parser that read the
    // wall-clock digits and stamped Kind = Utc without applying the offset produced the same DateTime.
    // The two zoneless rows carry no offset either, so before this row nothing in the suite reached
    // XmlRpcClient's Iso8601OffsetFormats at all. +02:00 makes the rebase observable: an implementation
    // that ignored the offset returns hour 16, and one that dropped AdjustToUniversal returns Local.
    [DataRow("19980717T16:08:55+02:00", DateTimeKind.Utc, "XML-RPC basic form carrying a real offset")]
    public void ADate_IsReadInEverySpellingTheProtocolUses(string spelling, DateTimeKind kind, string label)
    {
        string xml = $"<value><dateTime.iso8601>{spelling}</dateTime.iso8601></value>";

        XmlRpcClient.TryParseValue(Value(xml), out IXmlRpcValue? value).ShouldBeTrue(label);

        object? parsed = value.ShouldBeOfType<XmlRpcScalarValue>().Value;
        DateTime moment = parsed.ShouldBeOfType<DateTime>();

        moment.Year.ShouldBe(1998, label);
        moment.Month.ShouldBe(7, label);
        moment.Day.ShouldBe(17, label);
        moment.Hour.ShouldBe(14, label);
        moment.Minute.ShouldBe(8, label);
        moment.Second.ShouldBe(55, label);
        moment.Kind.ShouldBe(kind, label);
    }

    /// <summary>
    /// A value that is neither typed nor carries text is still refused.
    /// </summary>
    /// <param name="xml">An empty <c>value</c> element, in each of the two ways XML lets it be written.</param>
    /// <remarks>
    ///     The boundary control. The untyped fallback the fix makes reachable was written with a
    ///     <c>!string.IsNullOrEmpty</c> guard, and that guard is kept: an empty element is not a
    ///     successfully parsed empty string. Pinned so the choice is deliberate rather than incidental.
    /// </remarks>
    [TestMethod]
    [DataRow("<value></value>")]
    [DataRow("<value />")]
    public void AnEmptyValue_IsRefused(string xml)
    {
        XmlRpcClient.TryParseValue(Value(xml), out IXmlRpcValue? value).ShouldBeFalse();

        value.ShouldBeNull();
    }

    /// <summary>
    /// A node that is not a value element is refused.
    /// </summary>
    [TestMethod]
    public void ANodeThatIsNotAValue_IsRefused()
    {
        XPathNavigator navigator = new XPathDocument(new StringReader("<param>text</param>"))
            .CreateNavigator()!.SelectSingleNode("//param")!;

        XmlRpcClient.TryParseValue(navigator, out IXmlRpcValue? value).ShouldBeFalse();

        value.ShouldBeNull();
    }

    /// <summary>
    /// A composite value still loads through its own type.
    /// </summary>
    /// <remarks>
    ///     The last control: <c>struct</c> and <c>array</c> are the two branches that recurse back into
    ///     this method, so a change to the dispatch reaches them indirectly.
    /// </remarks>
    [TestMethod]
    public void ACompositeValue_StillLoadsThroughItsOwnType()
    {
        const string Structure = """
            <value><struct>
              <member><name>a</name><value><i4>1</i4></value></member>
            </struct></value>
            """;
        const string Array = """
            <value><array><data>
              <value><i4>1</i4></value>
              <value>untyped</value>
            </data></array></value>
            """;

        XmlRpcClient.TryParseValue(Value(Structure), out IXmlRpcValue? structure).ShouldBeTrue();
        structure.ShouldBeOfType<XmlRpcStructureValue>().Members.Count.ShouldBe(1);

        XmlRpcClient.TryParseValue(Value(Array), out IXmlRpcValue? array).ShouldBeTrue();
        array.ShouldBeOfType<XmlRpcArrayValue>().Values.Count
            .ShouldBe(2, "an untyped member of an array is a string, not a reason to drop it");
    }
}