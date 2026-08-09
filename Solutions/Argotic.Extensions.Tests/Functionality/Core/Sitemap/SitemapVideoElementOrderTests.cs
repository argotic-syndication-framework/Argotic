namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Pins the order in which <see cref="SitemapVideo.WriteTo(XmlWriter, string)"/> emits child elements
/// against the <c>xsd:sequence</c> declared by <c>sitemap-video-1.1.xsd</c>.
/// </summary>
/// <remarks>
///     <para>
///     The video schema declares a <c>sequence</c>, not an <c>all</c>, so element order is part of the
///     contract rather than a matter of taste. This library wrote <c>tag</c> last and grouped
///     <c>uploader</c>, <c>platform</c> and <c>restriction</c> together after <c>live</c>; the schema puts
///     <c>tag</c> directly after <c>publication_date</c>, <c>restriction</c> after
///     <c>family_friendly</c>, and <c>platform</c> and <c>live</c> near the end. Every video sitemap this
///     library emitted carrying a tag, restriction, platform or uploader was therefore rejected by
///     Google's own schema.
///     </para>
///     <para>
///     Nothing found it for years because nothing validated. The round-trip tests pass either way — the
///     reader accepts any order, so save-then-load is symmetric over a defect that only a validating
///     parser can see. That is the same shape as the APML conformance defects recorded in
///     <c>ApmlSchemaConformanceTests</c>: reading the specification did not find them, and running it did.
///     </para>
///     <para>
///     This test asserts relative order rather than an exact element list, so adding a new optional
///     element does not break it — but moving one across another does, which is the only thing that can
///     reintroduce the defect.
///     </para>
/// </remarks>
[TestClass]
public class SitemapVideoElementOrderTests
{
    /// <summary>
    /// The child element order declared by <c>sitemap-video-1.1.xsd</c>, restricted to the elements this
    /// library writes.
    /// </summary>
    private static readonly string[] SchemaOrder =
    [
        "thumbnail_loc",
        "title",
        "description",
        "content_loc",
        "player_loc",
        "duration",
        "expiration_date",
        "rating",
        "view_count",
        "publication_date",
        "tag",
        "family_friendly",
        "restriction",
        "requires_subscription",
        "uploader",
        "platform",
        "live",
        "id",
    ];

    /// <summary>
    /// A video populated on every element this library writes emits them in the schema's declared order.
    /// </summary>
    [TestMethod]
    public void AVideoCarryingEveryElement_IsWrittenInTheOrderTheSchemaDeclares()
    {
        // Arrange
        SitemapVideo video = new(new Uri("https://res.cloudinary.com/endjin/thumb.jpg"), "Rx.NET v7.0 Released", "Ian Griffiths on the Rx.NET 7.0 package split.")
        {
            ContentLocation = new Uri("https://endjin.com/videos/rx7.mp4"),
            PlayerLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
            Duration = 1612,
            ExpirationDate = new DateTime(2027, 7, 29, 9, 0, 0, DateTimeKind.Utc),
            Rating = 4.8m,
            ViewCount = 1234,
            PublicationDate = new DateTime(2026, 7, 29, 9, 0, 0, DateTimeKind.Utc),
            FamilyFriendly = false,
            Restriction = "GB IE",
            RestrictionRelationship = SitemapVideoRelationship.Allow,
            RequiresSubscription = true,
            Uploader = "endjin",
            UploaderInfo = new Uri("https://www.youtube.com/endjin"),
            Platform = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile,
            PlatformRelationship = SitemapVideoRelationship.Allow,
            Live = true,
        };

        video.Tags.Add("Rx.NET");
        video.Tags.Add(".NET");

        // Act
        List<string> written = WrittenElementNames(video);

        // Assert
        List<string> expected = [.. SchemaOrder.Where(written.Contains)];
        written.Distinct().ToList().ShouldBe(expected);
    }

    /// <summary>
    /// Writes the video and returns its child element local names, in document order, duplicates
    /// collapsed to first appearance so repeated <c>tag</c> elements do not obscure the sequence.
    /// </summary>
    private static List<string> WrittenElementNames(SitemapVideo video)
    {
        StringBuilder builder = new();
        XmlWriterSettings settings = new() { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment };

        using (XmlWriter writer = XmlWriter.Create(builder, settings))
        {
            video.WriteTo(writer, "http://www.google.com/schemas/sitemap-video/1.1");
        }

        List<string> names = [];
        using XmlReader reader = XmlReader.Create(new StringReader(builder.ToString()), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1)
            {
                names.Add(reader.LocalName);
            }
        }

        return names;
    }
}