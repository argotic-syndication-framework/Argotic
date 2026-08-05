using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins what supplying a <see cref="SyndicationResourceLoadSettings"/> does to character encoding.
/// </summary>
/// <remarks>
///     <para>
///     <b>These tests assert behaviour that is wrong.</b> They exist so that the commit which fixes it
///     has to invert named assertions rather than merely stay green — every one of them passes against
///     the code as it stands, and the whole point is that they must stop.
///     </para>
///     <para>
///     Nothing else in the suite reads <see cref="SyndicationResourceLoadSettings.CharacterEncoding"/>.
///     A grep across the test project returns no hits at all, so a change to this property could delete
///     every behaviour below and leave 2,691 tests green.
///     </para>
/// </remarks>
[TestClass]
public sealed class SettingsEncodingCharacterisationTests
{
    private static readonly Uri Source = new("http://encoding.invalid/feed.xml");

    /// <summary>
    /// A feed declaring iso-8859-1, with the accented byte written as 0xE9 rather than as UTF-8.
    /// </summary>
    /// <remarks>
    ///     Latin-1 rather than something exotic because it is what a real feed from a 2004 blog engine
    ///     still serves, and because 0xE9 alone is not valid UTF-8 — decoding it as UTF-8 produces
    ///     U+FFFD, which is loud enough to assert on.
    /// </remarks>
    private static byte[] Latin1Feed =>
    [
        .. Encoding.ASCII.GetBytes(
            """<?xml version="1.0" encoding="iso-8859-1"?><rss version="2.0"><channel><title>Caf"""),
        0xE9,
        .. Encoding.ASCII.GetBytes("</title><link>http://encoding.invalid/</link><description>d</description></channel></rss>"),
    ];

    /// <summary>
    /// A handler serving the Latin-1 feed, byte for byte.
    /// </summary>
    /// <remarks>
    ///     Not <c>MockHttpMessageHandler.WithContent</c>, which takes a <see cref="string"/>: a string
    ///     cannot carry a lone 0xE9, so routing this fixture through one would UTF-8 encode it and
    ///     destroy the only thing under test.
    /// </remarks>
    /// <returns>A handler that answers every request with the fixture.</returns>
    private static MockHttpMessageHandler ServingLatin1() => new((_, _) =>
    {
        HttpResponseMessage response = new(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Latin1Feed),
        };

        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/rss+xml");
        return Task.FromResult(response);
    });
    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// With no settings, the declared encoding is honoured.
    /// </summary>
    /// <remarks>
    ///     The control, and the reason every assertion below is a defect rather than a limitation. The
    ///     library can read this document. It simply stops being able to the moment you ask it for
    ///     anything else.
    /// </remarks>
    [TestMethod]
    public void WithNoSettings_TheDeclaredEncodingIsHonoured()
    {
        RssFeed feed = new();
        using MemoryStream stream = new(Latin1Feed);

        feed.Load(stream, null);

        feed.Channel.Title.ShouldBe("Café");
    }

    /// <summary>
    /// Supplying any settings object at all corrupts the title.
    /// </summary>
    /// <remarks>
    ///     <c>Load(Stream, settings)</c> branches on <c>settings is not null</c>: the null arm sniffs the
    ///     declaration, the non-null arm passes <c>settings.CharacterEncoding</c>, which defaults to
    ///     <see cref="Encoding.UTF8"/>. So the default value of a property the caller never touched
    ///     overrides what the document says about itself.
    /// </remarks>
    [TestMethod]
    public void WithDefaultSettings_TheDeclaredEncodingIsIgnoredAndTheTitleIsCorrupted()
    {
        RssFeed feed = new();
        using MemoryStream stream = new(Latin1Feed);

        feed.Load(stream, new SyndicationResourceLoadSettings());

        feed.Channel.Title.ShouldBe("Caf\uFFFD");
    }

    /// <summary>
    /// The settings object need not mention encoding for the encoding to change.
    /// </summary>
    /// <remarks>
    ///     The shape that makes this worth a test rather than a note. A caller who wants the newest ten
    ///     items has no reason to think they have said anything about character encoding, and the
    ///     property they did not set is the one that breaks their feed.
    /// </remarks>
    [TestMethod]
    public void ASettingsObjectSetForAnUnrelatedReason_StillChangesTheEncoding()
    {
        RssFeed feed = new();
        using MemoryStream stream = new(Latin1Feed);

        feed.Load(stream, new SyndicationResourceLoadSettings { RetrievalLimit = 10 });

        feed.Channel.Title.ShouldBe("Caf\uFFFD");
    }

    /// <summary>
    /// A byte-order mark still wins, which bounds how far the defect reaches.
    /// </summary>
    /// <remarks>
    ///     <c>CreateSafeNavigator(Stream, Encoding)</c> constructs its <see cref="StreamReader"/> with
    ///     <c>detectEncodingFromByteOrderMarks: true</c>, so the supplied encoding is a default rather
    ///     than an override. Worth pinning: it means the fix must not be "stop passing the encoding",
    ///     which would change nothing for BOM-bearing documents and everything for the rest.
    /// </remarks>
    [TestMethod]
    public void AByteOrderMarkOverridesTheSuppliedEncoding()
    {
        byte[] utf16 = Encoding.Unicode.GetPreamble()
            .Concat(Encoding.Unicode.GetBytes(
                """<?xml version="1.0"?><rss version="2.0"><channel><title>Café</title><link>http://encoding.invalid/</link><description>d</description></channel></rss>"""))
            .ToArray();

        RssFeed feed = new();
        using MemoryStream stream = new(utf16);

        feed.Load(stream, new SyndicationResourceLoadSettings());

        feed.Channel.Title.ShouldBe("Café", "a BOM outranks the settings, so the defect is declaration-only");
    }

    /// <summary>
    /// The same settings object means opposite things to the sync and async loads.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The async path does not pass the encoding through. It translates: <c>CharacterEncoding ==
    ///     Encoding.UTF8 ? null : CharacterEncoding</c>, and null means sniff. Default settings hold the
    ///     <see cref="Encoding.UTF8"/> singleton, so the comparison is true and async sniffs.
    ///     </para>
    ///     <para>
    ///     One object, two overloads, two encodings. Neither signature hints at it.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task WithDefaultSettings_TheAsyncLoadSniffsWhereTheSyncLoadDoesNot()
    {
        using MockHttpMessageHandler handler = ServingLatin1();
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed asynchronous = new();
        await asynchronous.LoadAsync(
            Source, client, new SyndicationResourceLoadSettings(), null, this.TestContext.CancellationTokenSource.Token);

        RssFeed synchronous = new();
        using MemoryStream stream = new(Latin1Feed);
        synchronous.Load(stream, new SyndicationResourceLoadSettings());

        asynchronous.Channel.Title.ShouldBe("Café");
        synchronous.Channel.Title.ShouldBe("Caf\uFFFD");
    }

    /// <summary>
    /// Two UTF-8 encodings that decode identically produce opposite async behaviour.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The sentinel is reference equality against a singleton, so <c>new UTF8Encoding(false)</c> —
    ///     which decodes every byte sequence exactly as <see cref="Encoding.UTF8"/> does — fails it, and
    ///     the encoding is forced instead of sniffed.
    ///     </para>
    ///     <para>
    ///     This is what makes the sentinel indefensible rather than merely surprising. "Setting the
    ///     property to its own default value" and "setting it to an equivalent instance" are the same
    ///     intent, and they give different documents.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnEquivalentUtf8Instance_DefeatsTheSentinelAndForcesTheEncoding()
    {
        using MockHttpMessageHandler handler = ServingLatin1();
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed singleton = new();
        await singleton.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { CharacterEncoding = Encoding.UTF8 },
            null,
            this.TestContext.CancellationTokenSource.Token);

        RssFeed equivalent = new();
        await equivalent.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { CharacterEncoding = new UTF8Encoding(false) },
            null,
            this.TestContext.CancellationTokenSource.Token);

        singleton.Channel.Title.ShouldBe("Café", "the singleton trips the sentinel and the declaration is honoured");
        equivalent.Channel.Title.ShouldBe("Caf\uFFFD", "an equal-but-not-identical instance does not");
    }
}