namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers the two enumerations a video's platform restriction is built from:
/// <see cref="SitemapVideoPlatform"/>, whose members are combinable flags, and
/// <see cref="SitemapVideoRelationship"/>, which supplies the allow-or-deny sense.
/// </summary>
/// <remarks>
///     <para>
///     The remaining enum tests pin the <i>declared values</i> — <c>None = 0</c>, <c>Web = 1</c>,
///     <c>Mobile = 2</c>, <c>Tv = 4</c>, and the <c>[Flags]</c> attribute. Those are load-bearing
///     because <c>SitemapVideo.LoadPlatform</c> composes with <c>|=</c> and <c>WritePlatformElement</c>
///     tests with <c>&amp;</c>: both are wrong the moment the members stop being distinct powers of two.
///     </para>
///     <para>
///     Twelve further tests were removed from this class rather than kept. They asserted things like
///     <c>(Web | Mobile).HasFlag(Web)</c>, <c>(SitemapVideoPlatform)(int)combined == combined</c>, and
///     <c>SitemapVideoRelationship.Allow.ShouldBe(SitemapVideoRelationship.Allow)</c> — bit arithmetic
///     and language guarantees, authored in the test and true of every enum in .NET. Two of them ran a
///     <c>switch</c> written inside the test body and asserted its arms.
///     </para>
///     <para>
///     What they stood in for, and what the <c>TheParsePath_*</c> tests below now cover, is where the
///     two enumerations acquire meaning: the space-delimited composition in
///     <c>SitemapVideo.LoadPlatform</c> and the fail-closed relationship rule beside it, under which
///     <i>only</i> the literal <c>allow</c> yields <see cref="SitemapVideoRelationship.Allow"/> and
///     anything else — including a typo — yields <see cref="SitemapVideoRelationship.Deny"/>. Neither
///     was asserted anywhere in the suite: the only test that parsed a <c>video:platform</c> element
///     asserted <c>video.Platform.ShouldNotBeNull()</c> and never looked at the flags or the
///     relationship.
///     </para>
/// </remarks>
[TestClass]
public class SitemapVideoPlatformTests
{
    #region SitemapVideoPlatform Enum Tests

    /// <summary>
    /// <c>None</c> is <c>0</c>, so an unset platform restriction combines with anything without changing it.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_None_HasValueZero() =>
        // Assert
        ((int)SitemapVideoPlatform.None).ShouldBe(0);

    /// <summary>
    /// <c>Web</c> is <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Web_HasValueOne() =>
        // Assert
        ((int)SitemapVideoPlatform.Web).ShouldBe(1);

    /// <summary>
    /// <c>Mobile</c> is <c>2</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Mobile_HasValueTwo() =>
        // Assert
        ((int)SitemapVideoPlatform.Mobile).ShouldBe(2);

    /// <summary>
    /// <c>Tv</c> is <c>4</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Tv_HasValueFour() =>
        // Assert
        ((int)SitemapVideoPlatform.Tv).ShouldBe(4);

    /// <summary>
    /// The enumeration carries exactly one <see cref="FlagsAttribute"/>, which is what makes a combined
    /// value legal and formattable as a list of names.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_IsFlagsEnum()
    {
        // Arrange
        var type = typeof(SitemapVideoPlatform);

        // Assert
        type.GetCustomAttributes(typeof(FlagsAttribute), false).Length.ShouldBe(1);
    }
    #endregion

    #region SitemapVideoRelationship Enum Tests
    /// <summary>
    /// The relationship enumeration has exactly two members, so a restriction is allow or deny and
    /// nothing else.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_HasTwoValues()
    {
        // Arrange
        var values = Enum.GetValues<SitemapVideoRelationship>();

        // Assert
        values.Length.ShouldBe(2);
    }

    /// <summary>
    /// <c>Allow</c> and <c>Deny</c> are distinct values.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_AllowAndDeny_AreDifferent() =>
        // Assert
        SitemapVideoRelationship.Allow.ShouldNotBe(SitemapVideoRelationship.Deny);
    #endregion

    #region Combined Usage Tests
    #endregion


    #region Parse and write behaviour

    /// <summary>
    /// Builds a sitemap carrying one video whose <c>video:platform</c> element is as supplied, and
    /// returns the parsed video.
    /// </summary>
    /// <param name="platformElement">The <c>video:platform</c> element to embed.</param>
    /// <returns>The single parsed video.</returns>
    private static SitemapVideo ParseVideoWith(string platformElement)
    {
        string xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                    xmlns:video="http://www.google.com/schemas/sitemap-video/1.1">
              <url>
                <loc>https://example.com/watch</loc>
                <video:video>
                  <video:thumbnail_loc>https://example.com/thumb.jpg</video:thumbnail_loc>
                  <video:title>Example Video</video:title>
                  <video:description>A sample video description</video:description>
                  <video:content_loc>https://example.com/video.mp4</video:content_loc>
                  {platformElement}
                </video:video>
              </url>
            </urlset>
            """;

        Argotic.Syndication.Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        sitemap.Load(stream);

        return sitemap.Urls.Single().Extensions.OfType<SitemapVideoExtension>().Single().Videos.Single();
    }

    /// <summary>
    /// The platform text is split on spaces and each recognised token is folded in as a flag, so
    /// <c>web mobile</c> yields <c>Web | Mobile</c> and leaves <c>Tv</c> clear.
    /// </summary>
    /// <param name="text">The text content of the <c>video:platform</c> element.</param>
    /// <param name="expected">The flag combination it must produce.</param>
    /// <remarks>
    ///     The <c>tv web</c> row is deliberately out of declaration order, and the mixed-case row proves
    ///     the token match is case-insensitive. The unrecognised-token rows pin the silent drop: a
    ///     misspelt platform contributes nothing rather than failing the parse.
    /// </remarks>
    [TestMethod]
    [DataRow("web", SitemapVideoPlatform.Web)]
    [DataRow("mobile", SitemapVideoPlatform.Mobile)]
    [DataRow("tv", SitemapVideoPlatform.Tv)]
    [DataRow("web mobile", SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile)]
    [DataRow("tv web", SitemapVideoPlatform.Web | SitemapVideoPlatform.Tv)]
    [DataRow("web mobile tv", SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile | SitemapVideoPlatform.Tv)]
    [DataRow("WEB Mobile", SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile)]
    [DataRow("web  mobile", SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile)]
    [DataRow("desktop", SitemapVideoPlatform.None)]
    [DataRow("web desktop", SitemapVideoPlatform.Web)]
    public void TheParsePath_ComposesThePlatformFlagsFromTheSpaceDelimitedText(string text, SitemapVideoPlatform expected) =>
        ParseVideoWith($"<video:platform>{text}</video:platform>").Platform.ShouldBe(expected);

    /// <summary>
    /// Only the literal <c>allow</c> yields <see cref="SitemapVideoRelationship.Allow"/>; every other
    /// spelling, including a typo, falls closed to <see cref="SitemapVideoRelationship.Deny"/>.
    /// </summary>
    /// <param name="relationship">The <c>relationship</c> attribute value.</param>
    /// <param name="expected">The relationship it must produce.</param>
    /// <remarks>
    ///     The <c>allwo</c> row is the point of the test. A restriction list that silently widens on a
    ///     typo is a security-shaped defect, and this is the rule that stops it — but nothing in the
    ///     suite asserted the rule until now.
    /// </remarks>
    [TestMethod]
    [DataRow("allow", SitemapVideoRelationship.Allow)]
    [DataRow("ALLOW", SitemapVideoRelationship.Allow)]
    [DataRow("deny", SitemapVideoRelationship.Deny)]
    [DataRow("allwo", SitemapVideoRelationship.Deny)]
    [DataRow("permit", SitemapVideoRelationship.Deny)]
    public void TheParsePath_TreatsAnythingButAllowAsDeny(string relationship, SitemapVideoRelationship expected) =>
        ParseVideoWith($"""<video:platform relationship="{relationship}">web</video:platform>""")
            .PlatformRelationship.ShouldBe(expected);

    /// <summary>
    /// A platform element with no <c>relationship</c> attribute leaves the relationship unset rather
    /// than defaulting it, so the flags survive without inventing an allow-or-deny sense.
    /// </summary>
    [TestMethod]
    public void TheParsePath_LeavesTheRelationshipUnsetWhenTheAttributeIsAbsent()
    {
        SitemapVideo video = ParseVideoWith("<video:platform>web tv</video:platform>");

        video.Platform.ShouldBe(SitemapVideoPlatform.Web | SitemapVideoPlatform.Tv);
        video.PlatformRelationship.ShouldBeNull();
    }

    /// <summary>
    /// The flags are written back out as the same space-delimited tokens, in declaration order, beside
    /// the relationship — so a document survives being read and written.
    /// </summary>
    [TestMethod]
    public void TheWritePath_EmitsTheFlagsAsSpaceDelimitedTokensBesideTheRelationship()
    {
        SitemapVideo video = ParseVideoWith($"""<video:platform relationship="deny">tv web</video:platform>""");

        SitemapVideoExtension extension = new();
        extension.Videos.Add(video);

        using StringWriter sw = new();
        using (XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment }))
        {
            extension.WriteTo(writer);
        }

        // Declaration order, not the order the document used: the writer tests Web, then Mobile, then Tv.
        sw.ToString().ShouldContain("""<platform relationship="deny">web tv</platform>""");
    }

    #endregion
}