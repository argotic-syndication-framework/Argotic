using System.Xml.Schema;
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
///     <c>dotnet test --project … --filter "TestCategory=SiteMonitoring"</c>. Two separate filters keep
///     it out of the builds that must not see it, and both are needed. The local gate excludes the whole
///     live tier with <c>TestCategory!=Integration</c>; CI keeps the rest of that tier and excludes this
///     class alone, via <c>$AdditionalTestArgs</c> in <c>.zf/config.ps1</c>.
///     </para>
///     <para>
///     <b>The CI filter was missing when this class was written, and the build went red for exactly the
///     reason the first paragraph gives.</b> The category existed, the intent was documented here, and
///     the pipeline was never told - so two runs failed on the 2,048-character finding below. A category
///     no runner filters on is a comment, not a control.
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