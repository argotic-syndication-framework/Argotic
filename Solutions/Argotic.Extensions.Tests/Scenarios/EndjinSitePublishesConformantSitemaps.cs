using System.Xml.Schema;

using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Checks that the documents endjin.com publishes conform to the schemas they declare.
/// </summary>
/// <remarks>
///     <para>
///     <b>This monitors a website, not this library.</b> It is separated from
///     <see cref="ValidateAgainstPublishedSchemas"/> and carries its own category for one reason: a
///     content problem on endjin.com is not fixable from this repository, and a build that goes red for
///     it teaches developers to ignore a red build. The library-side question — can Argotic read what
///     endjin actually serves — is asserted hard in that class and stays there.
///     </para>
///     <para>
///     It is still worth having, and it earned its place immediately. The first run found
///     <c>endjin.com/sitemap-video.xml</c> carrying 17 <c>video:description</c> elements longer than the
///     2,048 characters <c>sitemap-video-1.1.xsd</c> permits, the longest at 3,985 — descriptions Google
///     is entitled to truncate or reject, on 17 of 100 talks. Nothing else in the estate was looking.
///     </para>
///     <para>
///     Run it deliberately:
///     <c>dotnet test --project … --filter "TestCategory=SiteMonitoring"</c>. It is excluded from the
///     offline gate by <c>TestCategory!=Integration</c> along with the rest of the live tier.
///     </para>
/// </remarks>
[TestClass]
[TestCategory("Integration")]
[TestCategory("SiteMonitoring")]
public class EndjinSitePublishesConformantSitemaps
{
    /// <summary>
    /// Gets each published endjin sitemap with the extension schemas needed to validate it.
    /// </summary>
    /// <remarks>
    ///     The news sitemap declares the image namespace as well as the news one and carries an
    ///     <c>image:image</c> in every <c>url</c>, so validating it needs both schemas — a document may
    ///     carry more than one extension, and this one does.
    /// </remarks>
    public static IEnumerable<object[]> PublishedSitemaps =>
    [
        ["https://endjin.com/sitemap.xml", Array.Empty<string>()],
        ["https://endjin.com/sitemap-news.xml", new[] { LiveSchemaSource.NewsSchemaUrl, LiveSchemaSource.ImageSchemaUrl }],
        ["https://endjin.com/sitemap-video.xml", new[] { LiveSchemaSource.VideoSchemaUrl }],
    ];

    /// <summary>
    /// A sitemap endjin publishes conforms to the schemas it declares.
    /// </summary>
    /// <param name="url">The sitemap to fetch.</param>
    /// <param name="schemaUrls">The extension schemas needed to validate it, if any.</param>
    [TestMethod]
    [DynamicData(nameof(PublishedSitemaps))]
    public void APublishedSitemap_ConformsToTheSchemasItDeclares(string url, string[] schemaUrls)
    {
        ArgumentNullException.ThrowIfNull(schemaUrls);

        string? document = LiveSchemaSource.TryFetch(url);

        if (document is null)
        {
            Assert.Inconclusive($"{url} could not be reached.");
        }

        XmlSchemaSet schemas = ConformanceSchemas.Sitemap;

        if (schemaUrls.Length > 0)
        {
            List<string> fetched = [];

            foreach (string schemaUrl in schemaUrls)
            {
                string? schema = LiveSchemaSource.TryFetch(schemaUrl);

                if (schema is null)
                {
                    Assert.Inconclusive($"{schemaUrl} could not be fetched, so {url} was not validated against it.");
                }

                fetched.Add(schema);
            }

            schemas = ConformanceSchemas.SitemapWith([.. fetched]);
        }

        ConformanceSchemas.Describe(document, schemas).ShouldBeEmpty(
            $"{url} does not conform to the schemas it declares. This is a website content problem, not a library one - the document needs correcting at source.");
    }
}