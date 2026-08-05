using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers what <see cref="SyndicationResourceLoadSettings.Timeout"/> means once it can be null.
/// </summary>
/// <remarks>
///     Making the property nullable adds a value the library must act on, and a new value with no test
///     is how a capability ships as documentation only. These are the three states: a deadline that
///     fires, a deadline that does not, and no deadline at all.
/// </remarks>
[TestClass]
public sealed class LoadSettingsTimeoutTests
{
    private static readonly Uri Source = new("http://timeout.invalid/feed.xml");

    private const string Feed =
        """<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>t</title><link>http://timeout.invalid/</link><description>d</description></channel></rss>""";

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// The default is the framework's, and it is not null.
    /// </summary>
    /// <remarks>
    ///     Making a property nullable invites making its default null too. Here that would silently
    ///     remove the 100-second deadline from every caller who never set one, which is the opposite of
    ///     what the nullability is for — <see langword="null"/> is a thing you now <i>can</i> ask for,
    ///     not a thing you get by accident.
    /// </remarks>
    [TestMethod]
    public void TheDefaultIsADeadline_NotTheAbsenceOfOne()
        => new SyndicationResourceLoadSettings().Timeout
            .ShouldBe(SyndicationEncodingUtility.DefaultRequestTimeout);

    /// <summary>
    /// A deadline that expires cancels the load.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ADeadlineThatExpires_CancelsTheLoad()
    {
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromSeconds(30), Feed);
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed feed = new();

        await Should.ThrowAsync<OperationCanceledException>(async () => await feed.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromMilliseconds(50) },
            null,
            this.TestContext.CancellationTokenSource.Token));
    }

    /// <summary>
    /// A null deadline does not cancel a load that outlives the default.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The capability itself. A delay longer than any deadline the library would otherwise impose
    ///     is impractical to wait for in a test, so this asserts the mechanism rather than the
    ///     duration: with a deadline of one tick the load is cancelled, and with the same handler and
    ///     <see langword="null"/> it completes. The handler is the control — it is identical in both
    ///     arms, so the difference cannot be anything else.
    ///     </para>
    ///     <para>
    ///     One tick rather than zero: <see cref="TimeSpan.Zero"/> is a legal value the setter accepts
    ///     and means "cancel immediately", which would pass this test without the deadline plumbing
    ///     working at all.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ANullDeadline_ImposesNoDeadlineOfItsOwn()
    {
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromMilliseconds(250), Feed);
        using HttpClient client = new(handler, disposeHandler: false);

        RssFeed bounded = new();
        await Should.ThrowAsync<OperationCanceledException>(async () => await bounded.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromTicks(1) },
            null,
            this.TestContext.CancellationTokenSource.Token));

        RssFeed unbounded = new();
        await unbounded.LoadAsync(
            Source,
            client,
            new SyndicationResourceLoadSettings { Timeout = null },
            null,
            this.TestContext.CancellationTokenSource.Token);

        unbounded.Channel.Title.ShouldBe("t");
    }

    /// <summary>
    /// The caller's own token still cancels a load with no deadline.
    /// </summary>
    /// <remarks>
    ///     What null removes is the library's deadline, not the caller's control. The linked
    ///     <see cref="CancellationTokenSource"/> is still built and still observes the caller's token —
    ///     it is simply never armed. Without this, "no deadline" could reasonably be implemented by
    ///     passing <see cref="CancellationToken.None"/>, and a caller would lose the ability to abort.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ANullDeadline_LeavesTheCallersOwnTokenWorking()
    {
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromSeconds(30), Feed);
        using HttpClient client = new(handler, disposeHandler: false);
        using CancellationTokenSource caller = new(TimeSpan.FromMilliseconds(50));

        RssFeed feed = new();

        await Should.ThrowAsync<OperationCanceledException>(async () => await feed.LoadAsync(
            Source, client, new SyndicationResourceLoadSettings { Timeout = null }, null, caller.Token));
    }

    /// <summary>
    /// The bounds still reject what they used to, and null is not a bound.
    /// </summary>
    /// <remarks>
    ///     The setter's guard had to be rewritten to lift over the nullable, which is the kind of edit
    ///     that quietly loses a condition. Both ends are checked, and null passes — the point of the
    ///     rewrite was to admit exactly that one new value and nothing else.
    /// </remarks>
    [TestMethod]
    public void TheBoundsSurviveTheRewrite()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromSeconds(-1) });
        Should.Throw<ArgumentOutOfRangeException>(
            () => new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromDays(366) });

        new SyndicationResourceLoadSettings { Timeout = null }.Timeout.ShouldBeNull();
        new SyndicationResourceLoadSettings { Timeout = TimeSpan.Zero }.Timeout.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// Both nullable properties read as words rather than as blanks.
    /// </summary>
    /// <remarks>
    ///     <c>ToString</c> dereferenced both properties and had to be guarded. Interpolating a null
    ///     yields an empty string, so <c>Timeout = ""</c> would have been the natural result — true,
    ///     useless, and indistinguishable from a formatting bug.
    /// </remarks>
    [TestMethod]
    public void ToStringNamesTheAbsentValues()
    {
        string described = new SyndicationResourceLoadSettings { Timeout = null }.ToString();

        described.ShouldContain("Timeout = \"none\"");
        described.ShouldContain("CharacterEncoding = \"detect\"");
    }
}