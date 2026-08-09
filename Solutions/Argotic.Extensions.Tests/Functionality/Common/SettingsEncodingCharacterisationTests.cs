namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins what supplying a <see cref="SyndicationResourceLoadSettings"/> does to character encoding.
/// </summary>
/// <remarks>
///     <para>
///     Written one commit earlier asserting the <b>broken</b> behaviour, so that the fix had to invert
///     named assertions rather than merely stay green. Four of the six inverted; the two that did not
///     are the ones marked as controls, which is what makes the four meaningful.
///     </para>
///     <para>
///     They were the only tests in the suite that read
///     <see cref="SyndicationResourceLoadSettings.CharacterEncoding"/> at all. Without them the
///     property could have been changed in any direction, including into a no-op, with 2,697 tests
///     staying green.
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
    /// Supplying a settings object no longer changes the encoding.
    /// </summary>
    /// <remarks>
    ///     The inversion. <c>Load(Stream, settings)</c> used to branch on <c>settings is not null</c> and
    ///     pass <c>settings.CharacterEncoding</c>, whose default was <see cref="Encoding.UTF8"/> — so the
    ///     default value of a property the caller never touched overrode what the document said about
    ///     itself. The branch now asks whether an encoding was <i>named</i>, and the default is
    ///     <see langword="null"/>.
    /// </remarks>
    [TestMethod]
    public void WithDefaultSettings_TheDeclaredEncodingIsStillHonoured()
    {
        RssFeed feed = new();
        using MemoryStream stream = new(Latin1Feed);

        feed.Load(stream, new SyndicationResourceLoadSettings());

        feed.Channel.Title.ShouldBe("Café");
    }

    /// <summary>
    /// A settings object set for an unrelated reason says nothing about encoding.
    /// </summary>
    /// <remarks>
    ///     The shape that made this worth a test rather than a note, and the one most likely to regress:
    ///     a caller who wants the newest ten items has no reason to think they have said anything about
    ///     character encoding, and it used to be the property they did not set that broke their feed.
    /// </remarks>
    [TestMethod]
    public void ASettingsObjectSetForAnUnrelatedReason_LeavesTheEncodingAlone()
    {
        RssFeed feed = new();
        using MemoryStream stream = new(Latin1Feed);

        feed.Load(stream, new SyndicationResourceLoadSettings { RetrievalLimit = 10 });

        feed.Channel.Title.ShouldBe("Café");
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
    /// The sync and async loads now agree about what a default settings object means.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The async path never passed the encoding through. It translated: <c>CharacterEncoding ==
    ///     Encoding.UTF8 ? null : CharacterEncoding</c>, where null meant sniff. Default settings held
    ///     the <see cref="Encoding.UTF8"/> singleton, so the comparison was true and async sniffed
    ///     while sync forced. One object, two overloads, two encodings, and neither signature hinted
    ///     at it.
    ///     </para>
    ///     <para>
    ///     The translation is gone because the property can now hold the answer directly.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task WithDefaultSettings_TheSyncAndAsyncLoadsAgree()
    {
        using MockHttpMessageHandler handler = ServingLatin1();
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed asynchronous = new();
        await asynchronous.LoadAsync(
            Source, client, new SyndicationResourceLoadSettings(), null, this.TestContext.CancellationTokenSource.Token);

        RssFeed synchronous = new();
        using MemoryStream stream = new(Latin1Feed);
        synchronous.Load(stream, new SyndicationResourceLoadSettings());

        asynchronous.Channel.Title.ShouldBe("Café", "INVARIANT: the async path always sniffed");
        synchronous.Channel.Title.ShouldBe("Café", "INVERTED: the sync path used to force UTF-8 here");
    }

    /// <summary>
    /// Naming an encoding means naming it, whichever UTF-8 instance you name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The sentinel was reference equality against a singleton, so <c>new UTF8Encoding(false)</c> —
    ///     which decodes every byte sequence exactly as <see cref="Encoding.UTF8"/> does — failed it and
    ///     forced the encoding, while the singleton passed it and sniffed. Two ways of saying the same
    ///     thing, two different documents.
    ///     </para>
    ///     <para>
    ///     Both now force. The third arm is the one that could not previously be written at all: there
    ///     was no value of <c>CharacterEncoding</c> that meant "detect" without also meaning "UTF-8",
    ///     which is why a caller who genuinely wanted UTF-8 imposed on a lying feed had no way to ask.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnEquivalentUtf8Instance_NowBehavesLikeTheSingleton()
    {
        using MockHttpMessageHandler handler = ServingLatin1();
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed singleton = await FetchAsync(client, Encoding.UTF8);
        RssFeed equivalent = await FetchAsync(client, new UTF8Encoding(false));
        RssFeed unset = await FetchAsync(client, null);

        singleton.Channel.Title.ShouldBe("Caf\uFFFD", "INVERTED: naming UTF-8 now forces UTF-8");
        equivalent.Channel.Title.ShouldBe("Caf\uFFFD", "INVERTED: and an equivalent instance does the same");
        unset.Channel.Title.ShouldBe("Café", "and null is how you ask for detection, which is now expressible");
    }

    private async Task<RssFeed> FetchAsync(HttpClient client, Encoding? encoding)
    {
        RssFeed feed = new();
        await feed.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { CharacterEncoding = encoding },
            null,
            this.TestContext.CancellationTokenSource.Token);

        return feed;
    }
}