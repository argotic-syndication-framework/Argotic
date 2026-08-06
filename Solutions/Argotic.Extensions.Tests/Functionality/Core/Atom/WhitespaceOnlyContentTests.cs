using System.Text;

using Argotic.Common;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers elements whose content is whitespace, which trimming setters turn into the empty string.
/// </summary>
/// <remarks>
///     <para>
///     Found by auditing the Azure Weekly corpus for round-trip fixed points: <c>save(load(x))</c>
///     must equal <c>save(load(save(load(x))))</c>. Two of 456 live feeds failed, both emitting
///     <c>&lt;category&gt;&lt;![CDATA[  ]]&gt;&lt;/category&gt;</c> — a Ghost/Jekyll idiom for a field
///     the author left blank.
///     </para>
///     <para>
///     The cause is a guard and a setter disagreeing about what empty means. <c>Load</c> asks
///     <c>IsNullOrEmpty</c>, which is <b>false</b> for <c>"  "</c>, so the element loads; the setter
///     then trims it to <c>""</c>. Saving writes <c>&lt;category&gt;&lt;/category&gt;</c>, and
///     reloading <i>that</i> asks <c>IsNullOrEmpty</c> of <c>""</c> — now <b>true</b> — so nothing
///     loads and the element is dropped. One pass through the model deletes an element the previous
///     pass kept.
///     </para>
///     <para>
///     The fix is the same shape as the empty-author fix: <c>wasLoaded</c> reports the element's
///     <b>presence</b>, and the assignment is guarded so a whitespace-only value simply leaves the
///     property at its default. The element survives, identically, on every pass.
///     </para>
/// </remarks>
[TestClass]
public sealed class WhitespaceOnlyContentTests
{
    private const string RssWithBlankCategory = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
            <channel>
                <title>Blank fields</title>
                <link>https://blank.invalid/</link>
                <description>d</description>
                <item>
                    <title>Post</title>
                    <link>https://blank.invalid/1</link>
                    <description>b</description>
                    <category><![CDATA[  ]]></category>
                </item>
            </channel>
        </rss>
        """;

    private const string AtomWithBlankTitle = """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <id>https://blank.invalid/feed</id>
            <title><![CDATA[  ]]></title>
            <updated>2026-01-02T03:04:05Z</updated>
            <entry>
                <id>https://blank.invalid/1</id>
                <title>Post</title>
                <updated>2026-01-02T03:04:05Z</updated>
            </entry>
        </feed>
        """;

    private static string Cycle(ISyndicationResource resource, string document)
    {
        using MemoryStream input = new(Encoding.UTF8.GetBytes(document), writable: false);
        resource.Load(input, null);

        using MemoryStream output = new();
        resource.Save(output, null);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    /// <summary>
    /// A whitespace-only RSS category survives every cycle identically.
    /// </summary>
    /// <remarks>
    ///     The field case, reduced. Two cycles are the minimum that can see this: the first save is
    ///     where the whitespace becomes empty, and the second is where the old code dropped the
    ///     element entirely.
    /// </remarks>
    [TestMethod]
    public void ABlankRssCategory_IsAFixedPoint()
    {
        string once = Cycle(new RssFeed(), RssWithBlankCategory);
        string twice = Cycle(new RssFeed(), once);

        once.ShouldContain("<category", Case.Sensitive, "the element is kept, not deleted by a pass through the model");
        twice.ShouldBe(once, "save(load(x)) must equal save(load(save(load(x))))");
    }

    /// <summary>
    /// A whitespace-only Atom title survives every cycle identically.
    /// </summary>
    /// <remarks>
    ///     Dropping it would be worse than losing whitespace: RFC 4287 §4.1.1 requires a feed to carry
    ///     exactly one <c>atom:title</c>, so a reader that deleted the element would turn a conformant
    ///     document into one that violates a MUST — the same argument that settled the empty author.
    /// </remarks>
    [TestMethod]
    public void ABlankAtomTitle_IsAFixedPointAndKeepsTheRequiredElement()
    {
        string once = Cycle(new AtomFeed(), AtomWithBlankTitle);
        string twice = Cycle(new AtomFeed(), once);

        once.ShouldContain("<title", Case.Sensitive, "§4.1.1 requires the element, blank or not");
        twice.ShouldBe(once);
    }

    /// <summary>
    /// Real content still survives both cycles unchanged.
    /// </summary>
    /// <remarks>
    ///     The control. Without it, the two rows above are equally consistent with "categories and
    ///     titles are now always emitted empty".
    /// </remarks>
    [TestMethod]
    public void RealContent_IsUnaffected()
    {
        string once = Cycle(
            new RssFeed(),
            RssWithBlankCategory.Replace("<![CDATA[  ]]>", "Azure", StringComparison.Ordinal));
        string twice = Cycle(new RssFeed(), once);

        once.ShouldContain("<category>Azure</category>", Case.Sensitive);
        twice.ShouldBe(once);
    }
}