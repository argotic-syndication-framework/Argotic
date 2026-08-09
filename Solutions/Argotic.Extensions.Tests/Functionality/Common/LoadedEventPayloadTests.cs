namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Asserts what the load events actually carry, rather than only that they fired.
/// </summary>
/// <remarks>
///     <para>
///     Around thirty tests subscribe to <see cref="ISyndicationResource.Loaded"/>, and almost all are
///     shaped <c>Loaded += (_, _) =&gt; raised = true;</c> - the payload is discarded. Two inspect it
///     and only one asserts anything on it. Nothing anywhere asserted
///     <see cref="SyndicationResourceLoadedEventArgs.Data"/>.
///     </para>
///     <para>
///     <see cref="ISyndicationExtension.Loaded"/> had no subscriber at all. Every extension load
///     constructs a <see cref="SyndicationExtensionLoadedEventArgs"/>, dispatches it to an empty
///     invocation list and drops it, which is why that type sat at zero branch coverage.
///     </para>
/// </remarks>
[TestClass]
public class LoadedEventPayloadTests
{
    /// <summary>
    /// The resource loaded event carries a navigator over the document that was parsed.
    /// </summary>
    [TestMethod]
    public void TheResourceLoadedEvent_CarriesANavigatorOverTheParsedDocument()
    {
        RssFeed feed = new();
        SyndicationResourceLoadedEventArgs? captured = null;
        feed.Loaded += (_, args) => captured = args;

        feed.Load(ToStream(FeedTestData.RssWithItems));

        captured.ShouldNotBeNull("the Loaded event did not fire");
        captured.Data.ShouldNotBeNull("the Loaded event fired with no navigator");
        captured.Data.OuterXml.ShouldContain("rss");
    }

    /// <summary>
    /// The resource loaded event reports no source when the resource came from a stream.
    /// </summary>
    [TestMethod]
    public void TheResourceLoadedEvent_ReportsNoSourceForALocalLoad()
    {
        RssFeed feed = new();
        SyndicationResourceLoadedEventArgs? captured = null;
        feed.Loaded += (_, args) => captured = args;

        feed.Load(ToStream(FeedTestData.RssWithItems));

        captured.ShouldNotBeNull();
        captured.Source.ShouldBeNull("a stream load has no originating URI");
    }

    /// <summary>
    /// An Atom feed raises the event with a navigator too.
    /// </summary>
    [TestMethod]
    public void AnAtomFeedLoad_RaisesTheEventWithANavigator()
    {
        AtomFeed feed = new();
        SyndicationResourceLoadedEventArgs? captured = null;
        feed.Loaded += (_, args) => captured = args;

        feed.Load(ToStream(FeedTestData.AtomWithEntries));

        captured.ShouldNotBeNull();
        captured.Data.ShouldNotBeNull();
        captured.Data.OuterXml.ShouldContain("feed");
    }

    /// <summary>
    /// An extension raises its own loaded event, carrying itself and the data it parsed.
    /// </summary>
    /// <remarks>
    ///     The first subscriber this event has ever had in the suite. Without it, the event silently
    ///     ceasing to fire would go unnoticed.
    /// </remarks>
    [TestMethod]
    public void AnExtensionLoad_RaisesItsEventCarryingItselfAndItsData()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <item xmlns:slash="http://purl.org/rss/1.0/modules/slash/">
              <slash:section>articles</slash:section>
              <slash:comments>42</slash:comments>
            </item>
            """;

        SiteSummarySlashSyndicationExtension extension = new();
        SyndicationExtensionLoadedEventArgs? captured = null;
        extension.Loaded += (_, args) => captured = args;

        using StringReader stringReader = new(xml);
        System.Xml.XPath.XPathDocument document = new(stringReader);

        System.Xml.XPath.XPathNavigator item = document.CreateNavigator().SelectSingleNode("//item")!;

        extension.Load(item).ShouldBeTrue();

        captured.ShouldNotBeNull("ISyndicationExtension.Loaded did not fire");
        captured.Extension.ShouldBeSameAs(extension);
        captured.Data.ShouldNotBeNull();
    }

    /// <summary>
    /// An extension that finds nothing to load still raises its event.
    /// </summary>
    /// <remarks>
    ///     Pins the contract: the event reports that a load was attempted, not that it succeeded. A
    ///     subscriber that assumed otherwise would miss the empty case.
    /// </remarks>
    [TestMethod]
    public void AnExtensionThatLoadsNothing_StillRaisesItsEvent()
    {
        const string xml = """<?xml version="1.0" encoding="utf-8"?><item><title>No extension data here</title></item>""";

        SiteSummarySlashSyndicationExtension extension = new();
        bool raised = false;
        extension.Loaded += (_, _) => raised = true;

        using StringReader stringReader = new(xml);
        System.Xml.XPath.XPathDocument document = new(stringReader);

        System.Xml.XPath.XPathNavigator item = document.CreateNavigator().SelectSingleNode("//item")!;

        extension.Load(item).ShouldBeFalse();
        raised.ShouldBeTrue();
    }

    private static MemoryStream ToStream(string xml) => new(System.Text.Encoding.UTF8.GetBytes(xml));
}