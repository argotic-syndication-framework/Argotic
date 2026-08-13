using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the specialized-document benchmark inputs: BlogML exports and APML attention profiles
/// at controllable scale.
/// </summary>
/// <remarks>
/// <para>
/// The committed samples are small (a 4-post BlogML export, a 3-profile APML document), so
/// cost-versus-size for these formats did not exist — §19 names the 1,000-post blog export as the
/// realistic missing scenario, because BlogML is an engine-to-engine migration format and a real
/// export is as large as the blog it moves. Shapes are modelled on the committed
/// <c>BlogMLDocument.xml</c> and <c>ApmlDocument.xml</c>, including the deliberate approval-status
/// mix the sample's own comment explains: "approved" is exactly the field an importer must not
/// get wrong, because getting it wrong publishes something the author did not.
/// </para>
/// <para>
/// Every sixteenth post carries an <i>embedded</i> base64 attachment (~1 KB) rather than the
/// sample's linked one — embedded attachments are where a migration document's decode cost
/// lives, and a corpus without one leaves that path priced at zero.
/// </para>
/// </remarks>
internal static class SpecializedDocumentCorpus
{
    private const string EmbeddedPayloadSeed = "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAJUlEQVR4nGP8//8/AzGAiSitUYWjCkcVjiocVUg1hQAAAP//AwB4";

    /// <summary>
    /// Generates a BlogML 2.0 export with the requested number of posts.
    /// </summary>
    /// <param name="postCount">The number of <c>post</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     Three authors (one unapproved), a category hierarchy with <c>parentref</c>, and per
    ///     post: HTML content, two category refs, two comments (plus an unapproved third on every
    ///     fourth post), a trackback on every eighth, and an embedded base64 attachment on every
    ///     sixteenth.
    /// </remarks>
    public static byte[] GenerateBlogMLUtf8(int postCount)
    {
        string embeddedPayload = string.Concat(Enumerable.Repeat(EmbeddedPayloadSeed, 10));

        StringBuilder builder = new(capacity: 2048 + (postCount * 1280));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<blog root-url=\"https://example.com/blog/\" date-created=\"2026-01-05T08:00:00Z\" xmlns=\"http://www.blogml.com/2006/09/BlogML\">\n");
        builder.Append("  <title type=\"text\">Synthetic Export</title>\n");
        builder.Append("  <sub-title type=\"html\">&lt;em&gt;Synthetic&lt;/em&gt; blog export at scale</sub-title>\n");
        builder.Append("  <authors>\n");
        builder.Append("    <author id=\"author-1\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-08-07T12:00:00Z\" approved=\"true\" email=\"alice@example.com\">\n");
        builder.Append("      <title type=\"text\">Alice Author</title>\n    </author>\n");
        builder.Append("    <author id=\"author-2\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-08-06T10:00:00Z\" approved=\"true\" email=\"bob@example.com\">\n");
        builder.Append("      <title type=\"text\">Bob Blogger</title>\n    </author>\n");
        builder.Append("    <author id=\"author-3\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-07-29T09:00:00Z\" approved=\"false\" email=\"carol@example.com\">\n");
        builder.Append("      <title type=\"text\">Carol Contributor</title>\n    </author>\n");
        builder.Append("  </authors>\n");
        builder.Append("  <extended-properties>\n");
        builder.Append("    <property name=\"CommentModeration\" value=\"true\"/>\n");
        builder.Append("    <property name=\"BlogEngine\" value=\"synthetic\"/>\n");
        builder.Append("  </extended-properties>\n");
        builder.Append("  <categories>\n");
        builder.Append("    <category id=\"cat-1\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-08-07T12:00:00Z\" approved=\"true\" parentref=\"0\">\n");
        builder.Append("      <title type=\"text\">Engineering</title>\n    </category>\n");
        builder.Append("    <category id=\"cat-2\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-08-05T11:00:00Z\" approved=\"true\" parentref=\"cat-1\">\n");
        builder.Append("      <title type=\"text\">Benchmarks</title>\n    </category>\n");
        builder.Append("    <category id=\"cat-3\" date-created=\"2026-01-05T08:00:00Z\" date-modified=\"2026-08-06T10:00:00Z\" approved=\"true\" parentref=\"cat-1\">\n");
        builder.Append("      <title type=\"text\">Syndication</title>\n    </category>\n");
        builder.Append("  </categories>\n");
        builder.Append("  <posts>\n");

        for (int i = 0; i < postCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("    <post id=\"post-").Append(ordinal)
                   .Append("\" date-created=\"2026-08-07T10:00:00Z\" date-modified=\"2026-08-07T12:00:00Z\" approved=\"true\" post-url=\"https://example.com/blog/post")
                   .Append(ordinal).Append("\" type=\"normal\" hasexcerpt=\"true\" views=\"")
                   .Append(((i * 13) % 5000).ToString(CultureInfo.InvariantCulture)).Append("\">\n");
            builder.Append("      <title type=\"text\">Post ").Append(ordinal).Append("</title>\n");
            builder.Append("      <excerpt type=\"html\">&lt;p&gt;Excerpt of post ").Append(ordinal).Append(".&lt;/p&gt;</excerpt>\n");
            builder.Append("      <content type=\"html\">&lt;p&gt;The full body of post ").Append(ordinal)
                   .Append(", long enough to carry the markup a real post carries: &lt;strong&gt;emphasis&lt;/strong&gt;, a &lt;a href=\"https://example.com\"&gt;link&lt;/a&gt;, and a list.&lt;/p&gt;&lt;ul&gt;&lt;li&gt;one&lt;/li&gt;&lt;li&gt;two&lt;/li&gt;&lt;/ul&gt;</content>\n");
            builder.Append("      <post-name type=\"text\">post-").Append(ordinal).Append("</post-name>\n");
            builder.Append("      <authors>\n        <author ref=\"author-").Append(((i % 3) + 1).ToString(CultureInfo.InvariantCulture)).Append("\"/>\n      </authors>\n");
            builder.Append("      <categories>\n        <category ref=\"cat-1\"/>\n        <category ref=\"cat-").Append(((i % 2) + 2).ToString(CultureInfo.InvariantCulture)).Append("\"/>\n      </categories>\n");
            builder.Append("      <comments>\n");
            builder.Append("        <comment id=\"comment-").Append(ordinal).Append("-1\" date-created=\"2026-08-07T11:00:00Z\" date-modified=\"2026-08-07T11:00:00Z\" approved=\"true\" user-name=\"Reader One\" user-email=\"reader1@example.com\">\n");
            builder.Append("          <content type=\"text\">A comment on post ").Append(ordinal).Append(".</content>\n        </comment>\n");
            builder.Append("        <comment id=\"comment-").Append(ordinal).Append("-2\" date-created=\"2026-08-07T11:30:00Z\" date-modified=\"2026-08-07T11:30:00Z\" approved=\"true\" user-name=\"Reader Two\">\n");
            builder.Append("          <content type=\"text\">A second comment on post ").Append(ordinal).Append(".</content>\n        </comment>\n");

            if (i % 4 == 0)
            {
                builder.Append("        <comment id=\"comment-").Append(ordinal).Append("-3\" date-created=\"2026-08-07T12:00:00Z\" date-modified=\"2026-08-07T12:00:00Z\" approved=\"false\" user-name=\"Unmoderated\">\n");
                builder.Append("          <content type=\"text\">An unapproved comment awaiting moderation.</content>\n        </comment>\n");
            }

            builder.Append("      </comments>\n");

            if (i % 8 == 0)
            {
                builder.Append("      <trackbacks>\n");
                builder.Append("        <trackback id=\"trackback-").Append(ordinal).Append("\" date-created=\"2026-08-07T11:15:00Z\" date-modified=\"2026-08-07T11:15:00Z\" approved=\"true\" url=\"https://other.example.com/trackback/post").Append(ordinal).Append("\">\n");
                builder.Append("          <title type=\"text\">Referenced elsewhere</title>\n        </trackback>\n");
                builder.Append("      </trackbacks>\n");
            }

            if (i % 16 == 0)
            {
                builder.Append("      <attachments>\n");
                builder.Append("        <attachment id=\"attach-").Append(ordinal).Append("\" date-created=\"2026-08-07T10:00:00Z\" date-modified=\"2026-08-07T10:00:00Z\" approved=\"true\" url=\"https://example.com/images/post")
                       .Append(ordinal).Append(".png\" mime-type=\"image/png\" size=\"1040\" embedded=\"true\" path=\"images/post").Append(ordinal).Append(".png\">")
                       .Append(embeddedPayload).Append("</attachment>\n");
                builder.Append("      </attachments>\n");
            }

            builder.Append("    </post>\n");
        }

        builder.Append("  </posts>\n</blog>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an APML 0.6 attention profile with the requested number of implicit concepts.
    /// </summary>
    /// <param name="conceptCount">The number of implicit <c>Concept</c> elements in the first profile.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     Two profiles with a <c>defaultprofile</c> selection (the attribute only means something
    ///     when there is a choice), implicit and explicit data sub-trees, and sources at a quarter
    ///     of the concept count — the committed sample's proportions, scaled.
    /// </remarks>
    public static byte[] GenerateApmlUtf8(int conceptCount)
    {
        int sourceCount = Math.Max(1, conceptCount / 4);
        int explicitCount = Math.Max(1, conceptCount / 4);

        StringBuilder builder = new(capacity: 1024 + (conceptCount * 192));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<APML xmlns=\"http://www.apml.org/apml-0.6\" version=\"0.6\">\n");
        builder.Append("  <Head>\n");
        builder.Append("    <Title>Synthetic attention profile</Title>\n");
        builder.Append("    <Generator>Argotic Benchmarks</Generator>\n");
        builder.Append("    <UserEmail>user@example.com</UserEmail>\n");
        builder.Append("    <DateCreated>2026-01-05T08:00:00Z</DateCreated>\n");
        builder.Append("  </Head>\n");
        builder.Append("  <Body defaultprofile=\"primary\">\n");
        builder.Append("    <Profile name=\"primary\">\n");
        builder.Append("      <ImplicitData>\n");
        builder.Append("        <Concepts>\n");

        for (int i = 0; i < conceptCount; i++)
        {
            string value = (0.5 + ((i % 50) / 100.0)).ToString("F2", CultureInfo.InvariantCulture);
            builder.Append("          <Concept key=\"topic-").Append(i.ToString(CultureInfo.InvariantCulture))
                   .Append("\" value=\"").Append(value).Append("\" from=\"https://example.com/observed\" updated=\"2026-08-07T12:00:00Z\"/>\n");
        }

        builder.Append("        </Concepts>\n");
        builder.Append("        <Sources>\n");

        for (int i = 0; i < sourceCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("          <Source key=\"https://example.com/feeds/").Append(ordinal)
                   .Append("\" value=\"0.90\" name=\"Feed ").Append(ordinal)
                   .Append("\" type=\"application/rss+xml\" from=\"https://example.com\" updated=\"2026-08-07T12:00:00Z\">\n");
            builder.Append("            <Author key=\"author-").Append(ordinal)
                   .Append("\" value=\"0.85\" from=\"https://example.com\" updated=\"2026-08-07T12:00:00Z\"/>\n");
            builder.Append("          </Source>\n");
        }

        builder.Append("        </Sources>\n");
        builder.Append("      </ImplicitData>\n");
        builder.Append("      <ExplicitData>\n");
        builder.Append("        <Concepts>\n");

        for (int i = 0; i < explicitCount; i++)
        {
            builder.Append("          <Concept key=\"stated-topic-").Append(i.ToString(CultureInfo.InvariantCulture)).Append("\" value=\"1.0\"/>\n");
        }

        builder.Append("        </Concepts>\n");
        builder.Append("      </ExplicitData>\n");
        builder.Append("    </Profile>\n");
        builder.Append("    <Profile name=\"secondary\">\n");
        builder.Append("      <ImplicitData>\n");
        builder.Append("        <Concepts>\n");
        builder.Append("          <Concept key=\"weekend-topic\" value=\"0.60\" from=\"https://example.com/observed\" updated=\"2026-08-07T12:00:00Z\"/>\n");
        builder.Append("        </Concepts>\n");
        builder.Append("      </ImplicitData>\n");
        builder.Append("    </Profile>\n");
        builder.Append("    <Applications>\n");
        builder.Append("      <Application name=\"example-reader\">\n");
        builder.Append("        <ReaderSettings theme=\"dark\" refresh=\"hourly\"/>\n");
        builder.Append("      </Application>\n");
        builder.Append("    </Applications>\n");
        builder.Append("  </Body>\n");
        builder.Append("</APML>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}