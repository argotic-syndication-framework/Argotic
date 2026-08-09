namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Verifies that the real sample documents are deployed alongside the test assembly and parse.
/// </summary>
/// <remarks>
///     This is infrastructure, not behaviour: it fails if the <c>None Include</c> link in the project
///     file stops copying the samples, which would otherwise surface as every fixture-based test
///     failing with a file-not-found somewhere less obvious.
/// </remarks>
[TestClass]
public class SampleFeedFixtureTests
{
    /// <summary>
    /// Gets each linked sample document name as a data row.
    /// </summary>
    public static IEnumerable<object[]> EverySample => SampleFeeds.All.Select(name => new object[] { name });

    /// <summary>
    /// Every linked sample document is deployed to the test output directory.
    /// </summary>
    /// <param name="fileName">The document under test.</param>
    [TestMethod]
    [DynamicData(nameof(EverySample))]
    public void EverySampleDocument_IsDeployedBesideTheTestAssembly(string fileName)
    {
        File.Exists(SampleFeeds.PathTo(fileName)).ShouldBeTrue(
            $"{fileName} was not copied to the output directory - check the None Include link in Argotic.Extensions.Tests.csproj");
    }

    /// <summary>
    /// Every linked sample document parses through the framework's own navigator factory.
    /// </summary>
    /// <param name="fileName">The document under test.</param>
    [TestMethod]
    [DynamicData(nameof(EverySample))]
    public void EverySampleDocument_ParsesToANavigatorWithARootElement(string fileName)
    {
        using FileStream stream = SampleFeeds.Open(fileName);

        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        navigator.MoveToFirstChild().ShouldBeTrue($"{fileName} has no root element");
        navigator.LocalName.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// The hand-maintained sample list names exactly the documents that are deployed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>This test exists because the list silently stopped matching.</b> Four samples were added
    ///     in one pass — <c>PodcastFeed.xml</c>, <c>sitemap_hreflang.xml</c>,
    ///     <c>AtomServiceDocument.xml</c> and <c>AtomCategoryDocument.xml</c> — and none was added to
    ///     <see cref="SampleFeeds.All"/>. Every guard in this class iterates that list, so all four
    ///     escaped the deployment and parse checks completely, and nothing failed. The suite was green
    ///     and four fixtures were unguarded.
    ///     </para>
    ///     <para>
    ///     It compares in <b>both</b> directions on purpose. A list that is too short loses coverage
    ///     silently, which is what happened; a list that is too long fails later and less clearly, as a
    ///     file-not-found from whichever test happened to ask for the missing name first.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheSampleList_MatchesTheSampleDirectory()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "SampleData");

        List<string> deployed = [.. Directory.EnumerateFiles(directory, "*.xml")
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)];

        List<string> listed = [.. SampleFeeds.All.OrderBy(name => name, StringComparer.Ordinal)];

        listed.Except(deployed, StringComparer.Ordinal).ShouldBeEmpty(
            "SampleFeeds.All names documents that are not deployed - remove them or restore the files");

        deployed.Except(listed, StringComparer.Ordinal).ShouldBeEmpty(
            "SampleData holds documents SampleFeeds.All does not name, so no fixture guard covers them - add them to the list");
    }

    /// <summary>
    /// The extension-bearing RSS sample really is extension-bearing.
    /// </summary>
    /// <remarks>
    ///     Guards the assumption the rest of the suite is about to rest on. If this file were ever
    ///     swapped for a minimal one, the tests that read extension data off it would keep passing while
    ///     silently proving nothing.
    /// </remarks>
    [TestMethod]
    public void RssFeedWithExtensions_DeclaresTheExtensionNamespacesTheSuiteReliesOn()
    {
        using FileStream stream = SampleFeeds.Open(SampleFeeds.RssFeedWithExtensions);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);
        navigator.MoveToChild("rss", string.Empty).ShouldBeTrue();

        Dictionary<string, string> namespaces =
            (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

        // The inline corpus in FeedTestData declares none of these.
        foreach (string prefix in (string[])["itunes", "media", "dc", "dcterms", "content", "slash", "sy", "wfw", "geo", "georss", "lj"])
        {
            namespaces.ShouldContainKey(prefix, $"RssFeedWithExtensions.xml no longer declares xmlns:{prefix}");
        }

        namespaces.Count.ShouldBeGreaterThanOrEqualTo(
            20,
            "RssFeedWithExtensions.xml is the suite's only richly-extended document; it declared 21 namespaces once georss was added");
    }

    /// <summary>
    /// The extension-bearing Atom sample really is extension-bearing.
    /// </summary>
    [TestMethod]
    public void AtomFeedWithExtensions_DeclaresExtensionNamespaces()
    {
        using FileStream stream = SampleFeeds.Open(SampleFeeds.AtomFeedWithExtensions);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);
        navigator.MoveToChild("feed", "http://www.w3.org/2005/Atom").ShouldBeTrue();

        Dictionary<string, string> namespaces =
            (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

        namespaces.Count.ShouldBeGreaterThanOrEqualTo(
            9,
            "AtomFeedWithExtensions.xml declared 10 namespaces once georss was added");
    }
}