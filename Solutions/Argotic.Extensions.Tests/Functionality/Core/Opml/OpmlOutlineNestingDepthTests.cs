using System.Text;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

/// <summary>
/// Covers the bound on how deeply <see cref="OpmlOutline"/> elements may nest.
/// </summary>
/// <remarks>
///     <para>
///     <see cref="OpmlOutline.Load(System.Xml.XPath.XPathNavigator)"/> and its settings-taking sibling are
///     self-recursive, one frame per nesting level, and had nothing bounding depth. They are reachable from
///     <see cref="OpmlDocument.Load(Stream)"/>, whose content allowance is 8 MiB — a <i>larger</i> budget
///     than the XML-RPC parser that has the same defect. An <c>&lt;outline&gt;</c> start tag costs 9 bytes,
///     so that allowance buys hundreds of thousands of levels, and the recursion is not tail-recursive:
///     the call sits inside <c>while (iterator.MoveNext())</c> with work after it.
///     </para>
///     <para>
///     The failure mode is what makes this worth bounding rather than tolerating. A stack overflow in .NET
///     is not a catchable exception — it terminates the process. So a hostile or merely broken subscription
///     list kills the host application outright, with no <c>catch</c> that can intervene.
///     </para>
///     <para>
///     None of these tests actually overflows a stack. That is deliberate: a test that did would kill the
///     test host and take the rest of the suite with it. They exercise the cap at its boundary instead —
///     exactly at it, and one past it — which is where an off-by-one in the guard would show.
///     </para>
/// </remarks>
[TestClass]
public class OpmlOutlineNestingDepthTests
{
    /// <summary>
    /// A document nested exactly to the cap loads every level.
    /// </summary>
    [TestMethod]
    public void ADocumentNestedExactlyToTheCap_LoadsEveryLevel()
    {
        OpmlDocument document = Load(NestedOutlines(OpmlOutline.MaxOutlineNestingDepth));

        document.Outlines.Count.ShouldBe(1);
        ChainLength(document.Outlines[0]).ShouldBe(OpmlOutline.MaxOutlineNestingDepth);
    }

    /// <summary>
    /// A document nested one level past the cap loads up to the cap and drops the level beyond it, rather
    /// than throwing.
    /// </summary>
    /// <remarks>
    ///     Skipping matches how every other unusable value on this path is handled — an unparseable
    ///     attribute is skipped, not raised — and a malformed depth should not get a louder failure than a
    ///     malformed integer.
    /// </remarks>
    [TestMethod]
    public void ADocumentNestedOneLevelPastTheCap_LoadsUpToTheCapAndSkipsTheRest()
    {
        OpmlDocument document = Load(NestedOutlines(OpmlOutline.MaxOutlineNestingDepth + 1));

        document.Outlines.Count.ShouldBe(1);
        ChainLength(document.Outlines[0]).ShouldBe(OpmlOutline.MaxOutlineNestingDepth);
    }

    /// <summary>
    /// A document nested far past the cap is still bounded at the cap, and still loads rather than throwing.
    /// </summary>
    [TestMethod]
    public void ADocumentNestedFarPastTheCap_IsStillBoundedAtTheCap()
    {
        OpmlDocument document = Load(NestedOutlines(2000));

        document.Outlines.Count.ShouldBe(1);
        ChainLength(document.Outlines[0]).ShouldBe(OpmlOutline.MaxOutlineNestingDepth);
    }

    /// <summary>
    /// The cap applies to the settings-taking overload too, which is the one the OPML adapter calls and
    /// therefore the one every document load actually reaches.
    /// </summary>
    [TestMethod]
    public void ADocumentLoadedWithSettings_IsBoundedByTheSameCap()
    {
        OpmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(NestedOutlines(OpmlOutline.MaxOutlineNestingDepth + 8)));
        document.Load(stream, new Argotic.Common.SyndicationResourceLoadSettings());

        document.Outlines.Count.ShouldBe(1);
        ChainLength(document.Outlines[0]).ShouldBe(OpmlOutline.MaxOutlineNestingDepth);
    }

    /// <summary>
    /// Ordinary documents are nowhere near the cap, so bounding it costs real subscription lists nothing.
    /// </summary>
    [TestMethod]
    public void AnOrdinarilyNestedDocument_IsUnaffectedByTheCap()
    {
        OpmlOutline.MaxOutlineNestingDepth.ShouldBe(256);

        OpmlDocument document = Load(NestedOutlines(8));

        ChainLength(document.Outlines[0]).ShouldBe(8);
    }

    /// <summary>
    /// Walks the first-child chain and returns its length, iteratively — a recursive walk here would hit
    /// the same stack the product code does, and this test exists to talk about that stack.
    /// </summary>
    /// <param name="outline">The outermost outline.</param>
    /// <returns>The number of outlines in the chain, counting <paramref name="outline"/> itself.</returns>
    private static int ChainLength(OpmlOutline outline)
    {
        int length = 0;
        OpmlOutline? current = outline;
        while (current is not null)
        {
            length++;
            current = current.Outlines.Count > 0 ? current.Outlines[0] : null;
        }

        return length;
    }

    /// <summary>
    /// Builds an OPML document whose body holds a single chain of nested outlines.
    /// </summary>
    /// <param name="depth">The number of nested <c>outline</c> elements to emit.</param>
    /// <returns>The document text.</returns>
    private static string NestedOutlines(int depth)
    {
        StringBuilder builder = new();
        builder.Append("""
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0"><head><title>Deep</title></head><body>
            """);

        for (int i = 0; i < depth; i++)
        {
            builder.Append("<outline text=\"d").Append(i).Append("\">");
        }

        for (int i = 0; i < depth; i++)
        {
            builder.Append("</outline>");
        }

        builder.Append("</body></opml>");
        return builder.ToString();
    }

    /// <summary>
    /// Loads a document from the supplied text.
    /// </summary>
    /// <param name="xml">The document text.</param>
    /// <returns>The loaded document.</returns>
    private static OpmlDocument Load(string xml)
    {
        OpmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        document.Load(stream);
        return document;
    }
}