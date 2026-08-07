using System.Xml;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Writing an extended item out and reading it straight back in.
/// </summary>
/// <remarks>
///     <para>
///     The property under test is the one a consumer assumes without checking: whatever the library
///     wrote, the library can read. Every <c>WriteTo</c> test in <c>Functionality/</c> compares the
///     output against a literal, and every <c>Load</c> test parses a literal — so a writer and a reader
///     that disagree with each other, while each agreeing with its own literal, pass both.
///     </para>
///     <para>
///     Two families were caught that way. <c>FeedRank</c> wrote <c>scheme</c> and <c>domain</c> into the
///     extension namespace and read them from the no-namespace partition, so a rank this library saved
///     came back with neither. <c>LiveJournal</c> read a <c>lj:userpic</c> into a full object and then
///     never wrote it, so an item's picture disappeared on the first save. Both are fixed; these are the
///     regression guards.
///     </para>
/// </remarks>
[TestClass]
public class SaveAndReloadAnExtendedItem
{
    private const string LiveJournalNamespace = @"xmlns:lj=""http://livejournal.org/rss/lj/2.0/""";

    private const string LiveJournalItemWithAUserPicture =
        "<lj:music>Around the World</lj:music>"
        + "<lj:userpic><url>http://example.com/pic.jpg</url><keyword>coding</keyword><width>100</width><height>100</height></lj:userpic>";

    /// <summary>
    /// A rank this library wrote comes back whole, scheme and domain included.
    /// </summary>
    [TestMethod]
    public void AFeedRankItemThisLibraryWrote_ComesBackWhole()
    {
        // Arrange
        FeedRankSyndicationExtension original = new()
        {
            Context =
            {
                Scheme = new Uri("http://example.com/scheme.txt"),
                Domain = new Uri("http://example.com/"),
                Label = "Title",
                Value = 1.0m,
            },
        };

        // Act
        string saved = ExtensionTestUtil.AddExtensionToXml(original);
        FeedRankSyndicationExtension reloaded = ReloadExtension<FeedRankSyndicationExtension>(saved);

        // Assert
        reloaded.Context.Label.ShouldBe("Title");
        reloaded.Context.Value.ShouldBe(1.0m);
        reloaded.Context.Scheme.ShouldBe(new Uri("http://example.com/scheme.txt"));
        reloaded.Context.Domain.ShouldBe(new Uri("http://example.com/"));
        reloaded.ShouldBe(original);
    }

    /// <summary>
    /// An item's <c>lj:userpic</c> survives being saved and read back, so a load-edit-save leaves an
    /// extension equal to the one that was read.
    /// </summary>
    [TestMethod]
    public void ALiveJournalUserPicture_SurvivesBeingSavedAndReloaded()
    {
        // Arrange
        string source = ExtensionTestUtil.GetWrappedXml(LiveJournalNamespace, LiveJournalItemWithAUserPicture);

        // Act
        LiveJournalSyndicationExtension loaded = LoadExtension<LiveJournalSyndicationExtension>(source);
        loaded.Context.UserPicture.ShouldNotBeNull();

        string saved = ExtensionTestUtil.AddExtensionToXml(loaded);
        LiveJournalSyndicationExtension reloaded = ReloadExtension<LiveJournalSyndicationExtension>(saved);

        // Assert
        saved.ShouldContain("userpic", Case.Sensitive);
        reloaded.Context.UserPicture.ShouldNotBeNull();
        reloaded.Context.UserPicture.Url.ShouldBe(new Uri("http://example.com/pic.jpg"));
        reloaded.Context.UserPicture.Keyword.ShouldBe("coding");
        reloaded.Context.UserPicture.Width.ShouldBe(100);
        reloaded.Context.UserPicture.Height.ShouldBe(100);
        reloaded.ShouldBe(loaded);
    }

    /// <summary>
    /// Parses a saved RSS document and returns the extension of the requested type attached to its only
    /// item.
    /// </summary>
    /// <typeparam name="T">The extension type to find.</typeparam>
    /// <param name="document">An RSS 2.0 document carrying a single item.</param>
    /// <returns>The extension attached to that item.</returns>
    private static T ReloadExtension<T>(string document)
        where T : SyndicationExtension
    {
        using XmlReader reader = XmlReader.Create(new StringReader(document));
        RssFeed feed = new();
        feed.Load(reader);

        T? extension = feed.Channel.Items.Single().FindExtension<T>();
        extension.ShouldNotBeNull();
        return extension;
    }

    /// <summary>
    /// The same as <see cref="ReloadExtension{T}"/>, named for the first leg of a round trip so the
    /// scenarios above read in the order they happen.
    /// </summary>
    /// <typeparam name="T">The extension type to find.</typeparam>
    /// <param name="document">An RSS 2.0 document carrying a single item.</param>
    /// <returns>The extension attached to that item.</returns>
    private static T LoadExtension<T>(string document)
        where T : SyndicationExtension => ReloadExtension<T>(document);
}