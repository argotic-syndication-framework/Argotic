using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins what each parse entry point rejects, what it names when it does, and what it closes.
/// </summary>
/// <remarks>
///     <para>
///     Ten guard tests already existed and not one of them asserted a parameter name. That is the
///     difference between "it threw" and "it threw the exception the caller can act on" — and one of
///     the names below is a <b>leak</b>, the private plumbing of a call chain surfacing in a public
///     API's contract. The rewrite deletes that chain, so the leak goes with it. Pinning it first is
///     what makes that a deliberate change rather than an incidental one.
///     </para>
///     <para>
///     Ownership is here for the same reason. Two overloads that look symmetrical differ in whether
///     they close the caller's stream, nothing documents it, and no test observed it.
///     </para>
/// </remarks>
[TestClass]
public sealed class ParseEntryPointGuardTests
{
    private const string Document = """<?xml version="1.0" encoding="utf-8"?><r><title>text</title></r>""";

    /// <summary>
    /// Rows 26 to 28 — every guard names the parameter the caller passed.
    /// </summary>
    [TestMethod]
    public void EveryGuard_NamesItsParameter()
    {
        Should.Throw<ArgumentNullException>(() => SyndicationEncodingUtility.CreateSafeNavigator((string)null!))
            .ParamName.ShouldBe("xml");
        Should.Throw<ArgumentException>(() => SyndicationEncodingUtility.CreateSafeNavigator(string.Empty))
            .ParamName.ShouldBe("xml");

        Should.Throw<ArgumentNullException>(() => SyndicationEncodingUtility.CreateSafeNavigator((Stream)null!))
            .ParamName.ShouldBe("stream");
        Should.Throw<ArgumentNullException>(() => SyndicationEncodingUtility.CreateSafeNavigator((TextReader)null!))
            .ParamName.ShouldBe("reader");

        // Not previously tested at all, in either direction: the two-argument stream overload has two
        // guards and the suite exercised neither.
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false);
        Should.Throw<ArgumentNullException>(() => SyndicationEncodingUtility.CreateSafeNavigator(null!, Encoding.UTF8))
            .ParamName.ShouldBe("stream");
        Should.Throw<ArgumentNullException>(() => SyndicationEncodingUtility.CreateSafeNavigator(stream, null!))
            .ParamName.ShouldBe("encoding");
    }

    /// <summary>
    /// Rows 29, 29a and 29b — an empty input is a parse error, not a guard about a foreign parameter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     All three used to report a parameter the caller never supplied. <c>CreateSafeNavigator(Stream)</c>
    ///     has one parameter, called <c>stream</c>, and threw <see cref="ArgumentException"/> naming
    ///     <c>content</c> — the parameter of <c>GetXmlEncoding(string)</c>, four calls down a chain the
    ///     caller could not see. The other two leaked <c>xml</c>, the parameter of the string overload
    ///     they delegated to.
    ///     </para>
    ///     <para>
    ///     All three inverted as their delegation disappeared: the reader at Phase 2, the stream and the
    ///     stream-with-encoding at Phase 3. An empty document has no root element, which is what it
    ///     always was, and now that is what the caller is told.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnEmptyInput_ProducesAParseErrorRatherThanALeakedParameterName()
    {
        using MemoryStream empty = new();
        Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator(empty))
            .Message.ShouldBe("Root element is missing.", "row 29, inverted at Phase 3");

        using MemoryStream emptyAgain = new();
        Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator(emptyAgain, Encoding.UTF8))
            .Message.ShouldBe("Root element is missing.", "row 29a, inverted at Phase 3");

        using StringReader emptyReader = new(string.Empty);
        Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator(emptyReader))
            .Message.ShouldBe("Root element is missing.", "row 29b, inverted at Phase 2");
    }

    /// <summary>
    /// Row 30 — neither stream overload closes the stream it was handed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     They used to disagree. <c>CreateSafeNavigator(Stream, Encoding)</c> wrapped the caller's
    ///     stream in a <see cref="StreamReader"/> it owned and disposed, closing the stream as a side
    ///     effect, while <c>CreateSafeNavigator(Stream)</c> copied the bytes out first and only disposed
    ///     its own buffer. Two overloads that read identically at the call site, behaving differently,
    ///     with nothing documenting it and no test observing it.
    ///     </para>
    ///     <para>
    ///     Inverted at Phase 3: both now pass <c>leaveOpen</c>, so neither closes what it did not open.
    ///     Asserting both directions is what makes this a rule rather than a coincidence.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void NeitherStreamOverload_ClosesTheCallersStream()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(Document);

        using MemoryStream withoutEncoding = new(bytes, writable: false);
        SyndicationEncodingUtility.CreateSafeNavigator(withoutEncoding);
        withoutEncoding.CanRead.ShouldBeTrue("CreateSafeNavigator(Stream) must not close a stream it did not open");

        using MemoryStream withEncoding = new(bytes, writable: false);
        SyndicationEncodingUtility.CreateSafeNavigator(withEncoding, Encoding.UTF8);
        withEncoding.CanRead.ShouldBeTrue("row 30 INVERTED at Phase 3: nor must the encoding overload");
    }

    /// <summary>
    /// Row 30, reader half — the caller's reader is drained but never disposed.
    /// </summary>
    [TestMethod]
    public void TheReaderOverload_DrainsTheReaderWithoutDisposingIt()
    {
        using StringReader reader = new(Document);

        SyndicationEncodingUtility.CreateSafeNavigator(reader);

        Should.NotThrow(() => reader.Peek()).ShouldBe(-1, "drained to the end, but still usable");
    }

    /// <summary>
    /// Row 33 — entity expansion is bounded, proved by driving a document through rather than by
    /// reading the setting back.
    /// </summary>
    /// <remarks>
    ///     The existing test asserts <c>MaxCharactersFromEntities &gt; 0</c> and never parses anything,
    ///     so it would pass against a parser that ignored the setting entirely. This one expands to
    ///     roughly 10^9 characters against a 10^7 cap.
    /// </remarks>
    [TestMethod]
    public void EntityExpansion_IsBoundedWhenADocumentActuallyExpands()
    {
        const string BillionLaughs = """
            <?xml version="1.0"?>
            <!DOCTYPE lolz [
              <!ENTITY lol "lol">
              <!ENTITY lol1 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;">
              <!ENTITY lol2 "&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;">
              <!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">
              <!ENTITY lol4 "&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;">
              <!ENTITY lol5 "&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;">
              <!ENTITY lol6 "&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;">
              <!ENTITY lol7 "&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;">
              <!ENTITY lol8 "&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;">
            ]>
            <lolz>&lol8;</lolz>
            """;

        Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator(BillionLaughs));
    }

    /// <summary>
    /// Row 34 — the reader settings reach the parser, proved by their effect on a document.
    /// </summary>
    /// <remarks>
    ///     Three existing tests read <c>IgnoreComments</c>, <c>IgnoreWhitespace</c> and
    ///     <c>IgnoreProcessingInstructions</c> back off a settings object. That answers whether the
    ///     settings are set, not whether the chain that builds the parser still passes them — which is
    ///     the question the rewrite raises.
    /// </remarks>
    [TestMethod]
    public void TheReaderSettings_StillReachTheParser()
    {
        const string Noisy = """<?xml version="1.0"?><?target instruction?><r>  <!-- comment -->text</r>""";

        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(Noisy);
        navigator.MoveToRoot();

        navigator.OuterXml.ShouldNotContain("comment", Case.Sensitive, "IgnoreComments must still apply");
        navigator.OuterXml.ShouldNotContain("instruction", Case.Sensitive, "IgnoreProcessingInstructions must still apply");
        navigator.OuterXml.ShouldBe("<r>text</r>", "IgnoreWhitespace must still apply");
    }
}