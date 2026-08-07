using System.Xml;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Reading and re-emitting the body of an Atom entry.
/// </summary>
/// <remarks>
///     <para>
///     <c>atom:content</c> is how Atom carries a post body, and neither of
///     <see cref="AtomContent"/>'s two real methods had ever run. The proof was one line pair in the
///     adapter: at <c>Atom10SyndicationResourceAdapter.cs:286</c> the predicate
///     <c>if (contentNavigator is not null)</c> recorded a hit while its body recorded none. It is
///     evaluated on every Atom 1.0 entry load and had never once been true, because no Atom fixture in
///     the repository contained a <c>content</c> element.
///     </para>
///     <para>
///     The four variants below are the ones RFC 4287 section 4.1.3 distinguishes, and each takes a
///     different branch of <c>AtomContent.Load</c> and <c>WriteTo</c>.
///     </para>
/// </remarks>
[TestClass]
public class ReadAtomEntryContent
{
    /// <summary>
    /// Plain text content is read from an entry.
    /// </summary>
    [TestMethod]
    public void AnEntryWithTextContent_ExposesTheText()
    {
        AtomEntry entry = LoadSingleEntry("""<content type="text">A plain text body</content>""");

        entry.Content.ShouldNotBeNull();
        entry.Content.ContentType.ShouldBe("text");
        entry.Content.Content.ShouldBe("A plain text body");
    }

    /// <summary>
    /// Escaped HTML content is read from an entry.
    /// </summary>
    [TestMethod]
    public void AnEntryWithHtmlContent_ExposesTheMarkupAsText()
    {
        AtomEntry entry = LoadSingleEntry("""<content type="html">&lt;p&gt;An HTML body&lt;/p&gt;</content>""");

        entry.Content.ShouldNotBeNull();
        entry.Content.ContentType.ShouldBe("html");
        entry.Content.Content.ShouldBe("<p>An HTML body</p>");
    }

    /// <summary>
    /// Inline XHTML content is read from the wrapping div.
    /// </summary>
    /// <remarks>
    ///     The xhtml branch is the only one that reaches into a child element and takes its
    ///     <see cref="System.Xml.XPath.XPathNavigator.InnerXml"/> rather than the element's own value.
    /// </remarks>
    [TestMethod]
    public void AnEntryWithXhtmlContent_ExposesTheMarkupInsideTheDiv()
    {
        AtomEntry entry = LoadSingleEntry(
            """<content type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">An XHTML body</div></content>""");

        entry.Content.ShouldNotBeNull();
        entry.Content.ContentType.ShouldBe("xhtml");
        entry.Content.Content.ShouldBe("An XHTML body");
    }

    /// <summary>
    /// Out-of-line content exposes its source and carries no body.
    /// </summary>
    [TestMethod]
    public void AnEntryWithOutOfLineContent_ExposesTheSourceAndNoBody()
    {
        AtomEntry entry = LoadSingleEntry(
            """<content type="application/pdf" src="http://example.com/paper.pdf" />""");

        entry.Content.ShouldNotBeNull();
        entry.Content.ContentType.ShouldBe("application/pdf");
        entry.Content.Source.ShouldBe(new Uri("http://example.com/paper.pdf"));
        entry.Content.Content.ShouldBeNullOrEmpty();
    }

    /// <summary>
    /// Content carrying xml:base and xml:lang exposes both.
    /// </summary>
    [TestMethod]
    public void AnEntryWithContentCarryingCommonAttributes_ExposesThem()
    {
        AtomEntry entry = LoadSingleEntry(
            """<content type="text" xml:base="http://example.com/" xml:lang="en-GB">A body</content>""");

        entry.Content.ShouldNotBeNull();
        entry.Content.BaseUri.ShouldBe(new Uri("http://example.com/"));
        entry.Content.Language.ShouldNotBeNull().Name.ShouldBe("en-GB");
    }

    /// <summary>
    /// Each content variant survives being written and read back.
    /// </summary>
    /// <param name="contentType">The content type under test.</param>
    /// <param name="body">The body to round trip.</param>
    [TestMethod]
    [DataRow("text", "A plain text body")]
    [DataRow("html", "<p>An HTML body</p>")]
    [DataRow("xhtml", "An XHTML body")]
    public void EntryContent_SurvivesBeingWrittenAndReadBack(string contentType, string body)
    {
        AtomFeed written = MinimalFeed();
        written.Entries.Add(new AtomEntry(
            new AtomId(new Uri("urn:example:entry:1")),
            new AtomTextConstruct("An entry"),
            new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc))
        {
            Content = new AtomContent(body, contentType),
        });

        AtomFeed read = SaveAndReload(written);

        AtomContent content = read.Entries.Single().Content.ShouldNotBeNull();
        content.ContentType.ShouldBe(contentType);
        content.Content.ShouldBe(body);
    }

    /// <summary>
    /// Out-of-line content survives being written and read back.
    /// </summary>
    [TestMethod]
    public void OutOfLineEntryContent_SurvivesBeingWrittenAndReadBack()
    {
        AtomFeed written = MinimalFeed();
        written.Entries.Add(new AtomEntry(
            new AtomId(new Uri("urn:example:entry:1")),
            new AtomTextConstruct("An entry"),
            new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc))
        {
            Content = new AtomContent
            {
                ContentType = "application/pdf",
                Source = new Uri("http://example.com/paper.pdf"),
            },
        });

        AtomFeed read = SaveAndReload(written);

        AtomContent content = read.Entries.Single().Content.ShouldNotBeNull();
        content.ContentType.ShouldBe("application/pdf");
        content.Source.ShouldBe(new Uri("http://example.com/paper.pdf"));
    }

    /// <summary>
    /// Writing xhtml content declares the xhtml namespace on the content element.
    /// </summary>
    /// <remarks>
    ///     <c>AtomContent.WriteTo</c> emits the declaration only when
    ///     <see cref="XmlWriter.LookupPrefix"/> reports the namespace is not already in scope. That rule
    ///     had never executed, and it is the likeliest place in the type for a silent serialisation bug.
    /// </remarks>
    [TestMethod]
    public void WritingXhtmlContent_DeclaresTheXhtmlNamespace()
    {
        AtomFeed feed = MinimalFeed();
        feed.Entries.Add(new AtomEntry(
            new AtomId(new Uri("urn:example:entry:1")),
            new AtomTextConstruct("An entry"),
            new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc))
        {
            Content = new AtomContent("An XHTML body", "xhtml"),
        });

        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldContain("http://www.w3.org/1999/xhtml");
        xml.ShouldContain("div");
    }

    private static AtomFeed MinimalFeed() => new(
        new AtomId(new Uri("urn:example:feed")),
        new AtomTextConstruct("A feed"),
        new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc));

    private static AtomFeed SaveAndReload(AtomFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        AtomFeed read = new();
        read.Load(stream);
        return read;
    }

    private static AtomEntry LoadSingleEntry(string contentElement)
    {
        string xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <id>urn:example:feed</id>
              <title>A feed</title>
              <updated>2024-01-20T12:00:00Z</updated>
              <entry>
                <id>urn:example:entry:1</id>
                <title>An entry</title>
                <updated>2024-01-20T12:00:00Z</updated>
                {contentElement}
              </entry>
            </feed>
            """;

        using StringReader stringReader = new(xml);
        using XmlReader reader = XmlReader.Create(stringReader);
        AtomFeed feed = new();
        feed.Load(reader);

        return feed.Entries.Single();
    }
}