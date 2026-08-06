using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

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
            19,
            "RssFeedWithExtensions.xml is the suite's only richly-extended document; it declared 20 namespaces when linked in");
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
            8,
            "AtomFeedWithExtensions.xml declared 9 namespaces when linked in");
    }
}