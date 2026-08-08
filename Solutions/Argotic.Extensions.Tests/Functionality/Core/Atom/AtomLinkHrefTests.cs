namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Pins what an <see cref="AtomLink"/> carrying no usable <c>href</c> does on the way in and on the way out.
/// </summary>
/// <remarks>
///     <para>
///     RFC 4287 §4.2.7.1 requires <c>href</c> and types it as <c>atomUri</c> — plain text in the
///     Appendix B schema. The empty string is a well-formed IRI reference, so <c>href=""</c> is not
///     invalid Atom; it is a <i>same-document reference</i> (<a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986</a> §4.4).
///     That is worse than invalid: a validator passes it and a consumer silently resolves the link to
///     the containing feed's own address, so the library asserted something false rather than
///     something detectable.
///     </para>
///     <para>
///     Both ends move together on purpose. Guarding only the write would trade the false statement for
///     an element missing a REQUIRED attribute, so <c>Load</c> refuses a link it cannot give an href
///     and every caller — all four adapter sites gate on the return value — drops it.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomLinkHrefTests
{
    /// <summary>The characters an empty href attribute puts on the wire.</summary>
    private const string EmptyHrefAttribute = "href=\"\"";

    private static AtomFeed Load(string entryChildren)
    {
        AtomFeed feed = new();
        string xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry>
                    <id>https://conformance.invalid/1</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                    {entryChildren}
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);
        return feed;
    }

    private static string Save(AtomFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// A link with no href is not a link, so it is neither kept nor written.
    /// </summary>
    /// <remarks>
    ///     <c>&lt;link rel="self"/&gt;</c> is the reachable case: the <c>rel</c> branch used to report
    ///     success on its own, so a hrefless link entered the model from real input and left it as a
    ///     same-document reference.
    /// </remarks>
    [TestMethod]
    public void ALinkWithNoHref_IsRefusedRatherThanWrittenAsASameDocumentReference()
    {
        AtomFeed feed = Load("""<link rel="self"/>""");

        feed.Entries.First().Links.ShouldBeEmpty("INVERTED: Load refuses a link it cannot give an href");

        Save(feed).ShouldNotContain(EmptyHrefAttribute, Case.Sensitive, "INVERTED: nothing on the wire claims to point at the feed itself");
    }

    /// <summary>
    /// A link that does carry an href keeps loading, exactly as written.
    /// </summary>
    /// <remarks>INVARIANT — the guard is on the absent href, not on links generally.</remarks>
    [TestMethod]
    public void ALinkWithAnHref_StillLoadsAndKeepsItsOtherAttributes()
    {
        AtomFeed feed = Load("""<link rel="self" href="https://conformance.invalid/feed" type="application/atom+xml"/>""");

        AtomLink link = feed.Entries.First().Links.ShouldHaveSingleItem();
        link.Uri.ShouldBe(new Uri("https://conformance.invalid/feed"));
        link.Relation.ShouldBe("self");
        link.ContentType.ShouldBe("application/atom+xml");
    }

    /// <summary>
    /// A default-constructed link omits the attribute rather than asserting an empty one.
    /// </summary>
    /// <remarks>
    ///     The remaining element is missing a REQUIRED attribute, which is the honest report of a link
    ///     a caller never gave a target. It is detectable; <c>href=""</c> was not.
    /// </remarks>
    [TestMethod]
    public void ADefaultConstructedLink_StringifiesWithNoHrefAtAll()
    {
        new AtomLink().ToString().ShouldNotContain(EmptyHrefAttribute, Case.Sensitive, "INVERTED: the attribute is written only when there is a target");
    }
}