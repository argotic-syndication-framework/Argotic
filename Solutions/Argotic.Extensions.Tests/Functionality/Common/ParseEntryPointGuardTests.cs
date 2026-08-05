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
    /// Row 29 — an empty stream reports a parameter the caller never supplied.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>CreateSafeNavigator(Stream)</c> has one parameter and it is called <c>stream</c>. Handed an
    ///     empty one it throws <see cref="ArgumentException"/> naming <c>content</c> — which is the
    ///     parameter of <c>GetXmlEncoding(string)</c>, four calls down a chain the caller cannot see:
    ///     <c>GetStreamBytes</c> returns an empty array, <c>GetXmlEncoding(byte[])</c> forwards to the
    ///     <c>Stream</c> overload, which reads it to an empty string, which hits the string overload's
    ///     guard.
    ///     </para>
    ///     <para>
    ///     Rows 29a and 29b are the same defect wearing a different name: they leak <c>xml</c>, the
    ///     parameter of the <c>string</c> overload they delegate to. All three disappear when the
    ///     delegation chain does, and all three become <see cref="XmlException"/> instead. Pinned so
    ///     that it is a recorded behaviour change rather than something noticed later.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnEmptyInput_LeaksThePrivateParameterNameOfSomethingItDelegatesTo()
    {
        using MemoryStream empty = new();
        Should.Throw<ArgumentException>(() => SyndicationEncodingUtility.CreateSafeNavigator(empty))
            .ParamName.ShouldBe("content", "row 29: the caller's parameter is called 'stream'");

        using MemoryStream emptyAgain = new();
        Should.Throw<ArgumentException>(() => SyndicationEncodingUtility.CreateSafeNavigator(emptyAgain, Encoding.UTF8))
            .ParamName.ShouldBe("xml", "row 29a: the caller's parameters are 'stream' and 'encoding'");

        using StringReader emptyReader = new(string.Empty);
        Should.Throw<ArgumentException>(() => SyndicationEncodingUtility.CreateSafeNavigator(emptyReader))
            .ParamName.ShouldBe("xml", "row 29b: the caller's parameter is called 'reader'");
    }

    /// <summary>
    /// Row 30 — two overloads that look symmetrical are not, about whose stream they close.
    /// </summary>
    /// <remarks>
    ///     <c>CreateSafeNavigator(Stream, Encoding)</c> wraps the caller's stream in a
    ///     <see cref="StreamReader"/> it owns and disposes, which closes the caller's stream as a side
    ///     effect. <c>CreateSafeNavigator(Stream)</c> copies the bytes out first and only ever disposes
    ///     its own <see cref="MemoryStream"/>, so the caller's survives. Nothing documents the
    ///     difference and nothing observed it until now.
    /// </remarks>
    [TestMethod]
    public void OnlyTheEncodingOverload_ClosesTheCallersStream()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(Document);

        using MemoryStream survives = new(bytes, writable: false);
        SyndicationEncodingUtility.CreateSafeNavigator(survives);
        survives.CanRead.ShouldBeTrue("CreateSafeNavigator(Stream) must not close a stream it did not open");

        using MemoryStream closed = new(bytes, writable: false);
        SyndicationEncodingUtility.CreateSafeNavigator(closed, Encoding.UTF8);
        closed.CanRead.ShouldBeFalse("CreateSafeNavigator(Stream, Encoding) closes the caller's stream today");
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