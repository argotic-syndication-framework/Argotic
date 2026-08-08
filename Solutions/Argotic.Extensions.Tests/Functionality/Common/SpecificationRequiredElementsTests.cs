using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Asserts that the feeds this library writes carry the elements their specifications make mandatory.
/// </summary>
/// <remarks>
///     <para>
///     <b>The formats with no schema still have requirements.</b> RSS 2.0 is a prose specification and
///     RFC 4287 Appendix B is RELAX NG, which .NET cannot validate and for which this container has no
///     <c>xmllint</c>, <c>jing</c> or <c>java</c> — so neither format can be schema-checked the way the
///     sitemaps are. What both specifications do state outright is which elements must be present, and
///     that is checkable with a navigator and nothing else.
///     </para>
///     <para>
///     This is narrower than validation and should not be mistaken for it. It says the mandatory
///     elements are written; it says nothing about cardinality beyond that, ordering, or the content
///     model of anything optional. The gap is recorded in <c>.endjin/build-warnings.md</c> §12.5, and
///     the integration tier closes part of it by putting the same documents through the W3C Feed
///     Validator, which is the canonical checker for both formats.
///     </para>
/// </remarks>
[TestClass]
public class SpecificationRequiredElementsTests
{
    private const string AtomNamespace = "http://www.w3.org/2005/Atom";

    /// <summary>
    /// An RSS channel carries the three elements the RSS 2.0 specification requires of it.
    /// </summary>
    /// <remarks>
    ///     The RSS 2.0 specification names title, link and description as required channel elements.
    ///     They are the only three it requires, which is why a reader cannot lean on anything else being
    ///     present.
    /// </remarks>
    [TestMethod]
    public void AnRssChannel_CarriesTheThreeElementsTheSpecificationRequires()
    {
        RssFeed feed = new();
        feed.Channel.Title = "endjin blog";
        feed.Channel.Link = new Uri("https://endjin.com/blog/");
        feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

        feed.Channel.Items.Add(new RssItem
        {
            Title = "Rx.NET v7.0 Released",
            Link = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            Description = "The Rx.NET 7.0 release and its UI package split.",
        });

        XPathNavigator channel = NavigateTo(Save(feed), "/rss/channel");

        foreach (string required in (string[])["title", "link", "description"])
        {
            channel.SelectSingleNode(required).ShouldNotBeNull(
                $"the RSS 2.0 specification requires a channel-level <{required}> and Argotic did not write one");
        }
    }

    /// <summary>
    /// An Atom feed carries the three elements RFC 4287 section 4.1.1 requires of it.
    /// </summary>
    [TestMethod]
    public void AnAtomFeed_CarriesTheThreeElementsRfc4287Requires()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("endjin blog"),
            UpdatedOn = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
        };

        XPathNavigator root = NavigateTo(Save(feed), "/atom:feed");

        // Section 4.1.1: a feed element MUST contain exactly one atom:id, exactly one atom:title and
        // exactly one atom:updated.
        foreach (string required in (string[])["id", "title", "updated"])
        {
            SelectAtom(root, required).Count.ShouldBe(
                1,
                $"RFC 4287 section 4.1.1 requires exactly one atom:{required} on a feed");
        }
    }

    /// <summary>
    /// An Atom entry carries the three elements RFC 4287 section 4.1.2 requires of it.
    /// </summary>
    /// <remarks>
    ///     The entry requirements are separate from the feed's and are easy to satisfy on the feed while
    ///     missing on the entry, which is why this is asserted on the entry element rather than by
    ///     counting across the document.
    /// </remarks>
    [TestMethod]
    public void AnAtomEntry_CarriesTheThreeElementsRfc4287Requires()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("endjin blog"),
            UpdatedOn = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
        };

        feed.Entries.Add(new AtomEntry
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Rx.NET v7.0 Released"),
            UpdatedOn = new DateTime(2026, 7, 29, 10, 0, 0, DateTimeKind.Utc),
        });

        XPathNavigator entry = NavigateTo(Save(feed), "/atom:feed/atom:entry");

        foreach (string required in (string[])["id", "title", "updated"])
        {
            SelectAtom(entry, required).Count.ShouldBe(
                1,
                $"RFC 4287 section 4.1.2 requires exactly one atom:{required} on an entry");
        }
    }

    /// <summary>
    /// A stand-alone Atom entry document carries the same three required elements.
    /// </summary>
    /// <remarks>
    ///     The entry requirements do not soften when the entry is its own document — that is the media
    ///     type the Atom Publishing Protocol posts and returns, and a client has nothing else to read.
    /// </remarks>
    [TestMethod]
    public void AStandAloneAtomEntry_CarriesTheThreeElementsRfc4287Requires()
    {
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Rx.NET v7.0 Released"),
            UpdatedOn = new DateTime(2026, 7, 29, 10, 0, 0, DateTimeKind.Utc),
        };

        XPathNavigator root = NavigateTo(Save(entry), "/atom:entry");

        foreach (string required in (string[])["id", "title", "updated"])
        {
            SelectAtom(root, required).Count.ShouldBe(
                1,
                $"RFC 4287 section 4.1.2 requires exactly one atom:{required} on a stand-alone entry");
        }
    }

    /// <summary>
    /// The navigation used by the other tests finds nothing when the element is absent.
    /// </summary>
    /// <remarks>
    ///     <b>The check on the checker.</b> Every test above asserts a node is found; all of them would
    ///     pass against a query that matched everything, and none would fail against one that silently
    ///     matched the wrong namespace. Asserting that a deliberately absent element is reported missing
    ///     is what distinguishes those cases.
    /// </remarks>
    [TestMethod]
    public void TheRequiredElementQuery_FindsNothingWhenTheElementIsAbsent()
    {
        const string feedWithNoUpdated = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>
              <title>endjin blog</title>
            </feed>
            """;

        XPathNavigator root = NavigateTo(feedWithNoUpdated, "/atom:feed");

        SelectAtom(root, "updated").Count.ShouldBe(0);
        SelectAtom(root, "title").Count.ShouldBe(1);
    }

    private static XPathNodeIterator SelectAtom(XPathNavigator navigator, string localName)
    {
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("atom", AtomNamespace);
        return navigator.Select($"atom:{localName}", manager);
    }

    private static XPathNavigator NavigateTo(string document, string xpath)
    {
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(document);

        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("atom", AtomNamespace);

        return navigator.SelectSingleNode(xpath, manager)
            ?? throw new InvalidOperationException($"'{xpath}' matched nothing in the saved document.");
    }

    /// <summary>
    /// Saves a resource and returns the text, without the byte order mark.
    /// </summary>
    /// <param name="resource">The resource to save.</param>
    /// <returns>The saved document.</returns>
    /// <remarks>
    ///     <c>Save</c> writes a UTF-8 BOM, and decoding the bytes to a string keeps it as a leading
    ///     U+FEFF that a reader then treats as content. The failure is "Data at the root level is
    ///     invalid. Line 1, position 1", which reads as a malformed document when the document is
    ///     perfectly well formed. Every test in this class hit it at once, and the only one that did not
    ///     was the one reading from a literal — which is how the cause was obvious rather than
    ///     mysterious.
    /// </remarks>
    private static string Save(ISyndicationResource resource)
    {
        using MemoryStream stream = new();
        resource.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray()).TrimStart('﻿');
    }
}