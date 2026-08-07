using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Publishing;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// Permanent guards over two places in the Atom Publishing Protocol implementation where the
/// specification-correct behaviour reads like a defect, and has twice been catalogued as one.
/// </summary>
/// <remarks>
///     Neither of these tests inverts anything. They exist because the code they cover looks wrong to a
///     reader who has not opened RFC 5023, and each records the clause that makes it right, so a future
///     "fix" fails a named test instead of shipping.
/// </remarks>
[TestClass]
public sealed class AtomPublishingSpecGuardTests
{
    private static XmlReader Reader(string xml) =>
        XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    private static string Save(AtomServiceDocument document)
    {
        using MemoryStream stream = new();
        document.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Returns just the <c>service</c> start tag from a saved document.
    /// </summary>
    /// <param name="saved">The whole saved document.</param>
    /// <returns>The characters from <c>&lt;service</c> up to and including the closing angle bracket.</returns>
    /// <remarks>
    ///     The XML declaration carries <c>version="1.0"</c> of its own, so a search of the whole document
    ///     for <c>version=</c> can never distinguish the two. The subject is the element, so the subject
    ///     is what gets sliced out.
    /// </remarks>
    private static string ServiceStartTag(string saved)
    {
        int start = saved.IndexOf("<service", StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, "the control: a service element was written at all");

        int end = saved.IndexOf('>', start);
        end.ShouldBeGreaterThan(start, "and its start tag is closed");

        return saved[start..(end + 1)];
    }

    /// <summary>
    /// An empty <c>app:accept</c> loads successfully, because empty and absent are opposites.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     RFC 5023 §8.3.4: a collection with <b>no</b> <c>app:accept</c> element accepts Atom entry
    ///     documents, and one carrying an <c>app:accept</c> with an empty value accepts <b>nothing</b> —
    ///     it does not support member creation at all. <c>AtomMemberResources.Accepts</c> says the same
    ///     thing in prose.
    ///     </para>
    ///     <para>
    ///     So <c>Load</c> must report success here. Making it return <see langword="false"/> — the
    ///     obvious-looking tidy-up, since nothing was parsed out of the element — would stop
    ///     <c>AtomMemberResources</c> adding the range, leaving an empty <c>Accepts</c> list that a
    ///     consumer must read as "accepts Atom entries": the exact inverse of what the server published.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnEmptyAcceptElement_LoadsAndMeansTheCollectionAcceptsNothing()
    {
        XPathDocument fragment = new(new StringReader("""<accept xmlns="http://www.w3.org/2007/app" />"""));
        XPathNavigator navigator = fragment.CreateNavigator();
        navigator.MoveToChild(XPathNodeType.Element).ShouldBeTrue();

        AtomAcceptedMediaRange range = new();

        range.Load(navigator).ShouldBeTrue("RFC 5023 §8.3.4: an empty accept is a statement, not a parse failure");
        range.MediaRange.ShouldBe(string.Empty, "and the statement it makes is 'nothing'");
    }

    /// <summary>
    /// An empty <c>app:accept</c> survives into the collection that carries it.
    /// </summary>
    /// <remarks>
    ///     The end-to-end half of the guard above: <c>AtomMemberResources</c> adds a range only when its
    ///     <c>Load</c> reports success, so this is the assertion that actually fails if the return value
    ///     is ever flipped.
    /// </remarks>
    [TestMethod]
    public void ACollectionWithAnEmptyAccept_KeepsItRatherThanFallingBackToTheEntryDefault()
    {
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
              <workspace>
                <atom:title>W</atom:title>
                <collection href="http://example.org/readonly"><atom:title>Read only</atom:title><accept /></collection>
              </workspace>
            </service>
            """;

        AtomServiceDocument document = new();
        using XmlReader reader = Reader(xml);
        document.Load(reader);

        AtomMemberResources collection = document.Workspaces.Single().Collections.Single();

        collection.Accepts.ShouldHaveSingleItem().MediaRange
            .ShouldBe(string.Empty, "an empty Accepts list would mean the opposite — that entries may be posted");
    }

    /// <summary>
    /// A saved service document carries no <c>version</c> attribute, because RFC 5023 defines none.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     RFC 5023 §8.3.1 gives <c>app:service</c> exactly two permitted attribute groups — the common
    ///     <c>xml:base</c>/<c>xml:lang</c> pair and foreign-namespace attributes. There is no
    ///     <c>version</c>, so emitting one produces a document that does not conform to the schema the
    ///     specification prints.
    ///     </para>
    ///     <para>
    ///     <b>This is the test that stops the symmetry argument.</b> <c>BlogMLDocument</c> does write a
    ///     version attribute, and the line that would do the same here sat commented out in <c>Save</c>
    ///     for years, inviting exactly that reasoning. BlogML is a genuinely different case: its own
    ///     schema declares the attribute. Atom Publishing's does not, and a format that versions its
    ///     documents is not evidence about a format that versions itself through its namespace URI.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ASavedServiceDocument_CarriesNoVersionAttribute()
    {
        AtomServiceDocument document = new();
        document.Workspaces.Add(new AtomWorkspace(new AtomTextConstruct("Main")));

        string startTag = ServiceStartTag(Save(document));

        startTag.ShouldContain("http://www.w3.org/2007/app", Case.Sensitive, "the control: this is the app:service element");
        startTag.ShouldNotContain("version", Case.Sensitive, "RFC 5023 §8.3.1 defines no version attribute on app:service");
    }
}