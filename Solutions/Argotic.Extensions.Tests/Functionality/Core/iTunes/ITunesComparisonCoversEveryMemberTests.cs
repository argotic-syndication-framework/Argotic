using Argotic.Extensions.Core;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

/// <summary>
/// Covers every member of <see cref="ITunesSyndicationExtensionContext"/> against the comparison
/// contract that <see cref="ITunesSyndicationExtension"/> declares.
/// </summary>
/// <remarks>
///     <para>
///     <see cref="ITunesSyndicationExtension.Equals(ITunesSyndicationExtension)"/> is defined as
///     <c>CompareTo(other) == 0</c>, and <c>CompareTo</c> enumerates the context's members one at a
///     time. That makes the comparison a <b>hand-maintained list</b>, and a hand-maintained list is
///     exactly the kind of thing that a new property is added without.
///     </para>
///     <para>
///     It happened. Five members — <c>Title</c>, <c>Episode</c>, <c>Season</c>, <c>EpisodeType</c> and
///     <c>PodcastType</c> — were added when Apple's 2017 revision was implemented and never reached
///     <c>CompareTo</c> or <c>GetHashCode</c>. Two extensions describing different episodes of the same
///     podcast therefore reported themselves <b>equal</b>, which is wrong everywhere equality is load
///     bearing: a <see cref="HashSet{T}"/> would keep one of them, a <c>Distinct</c> would drop the
///     rest, and a sort would call them ties.
///     </para>
///     <para>
///     The test is written per-member rather than as one combined case so that a future addition that
///     misses the list fails on its own row and names itself.
///     </para>
/// </remarks>
[TestClass]
public sealed class ITunesComparisonCoversEveryMemberTests
{
    /// <summary>
    /// Builds an extension whose context carries a value in every member the comparison must see.
    /// </summary>
    private static ITunesSyndicationExtension Populated()
    {
        ITunesSyndicationExtension extension = new();
        extension.Context.Author = "An Author";
        extension.Context.Subtitle = "A subtitle";
        extension.Context.Summary = "A summary";
        extension.Context.Duration = new TimeSpan(1, 2, 3);
        extension.Context.ExplicitMaterial = ITunesExplicitMaterial.Clean;
        extension.Context.Image = new Uri("https://example.com/art.png");
        extension.Context.NewFeedUrl = new Uri("https://example.com/moved.xml");
        extension.Context.IsBlocked = false;
        extension.Context.IsComplete = false;
        extension.Context.Title = "A clean episode name";
        extension.Context.Episode = 42;
        extension.Context.Season = 3;
        extension.Context.EpisodeType = ITunesEpisodeType.Full;
        extension.Context.PodcastType = ITunesPodcastType.Episodic;
        extension.Context.VerificationToken = "657bc6db-cc95-4ae2-b257-e32553e76cdd";
        return extension;
    }

    /// <summary>
    /// Two extensions carrying the same values are equal, and agree on their hash code.
    /// </summary>
    /// <remarks>
    ///     The control. Without it, a <c>CompareTo</c> that returned a non-zero constant would pass every
    ///     other test in this class while making nothing equal to anything.
    /// </remarks>
    [TestMethod]
    public void TwoExtensionsCarryingTheSameValues_AreEqual()
    {
        ITunesSyndicationExtension first = Populated();
        ITunesSyndicationExtension second = Populated();

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        first.CompareTo(second).ShouldBe(0);
        first.GetHashCode().ShouldBe(second.GetHashCode(), "equal objects must agree on their hash code");
    }

    /// <summary>
    /// Changing any single member makes the two extensions unequal.
    /// </summary>
    /// <remarks>
    ///     Each row mutates exactly one member of an otherwise identical pair. A member missing from
    ///     <c>CompareTo</c> fails its own row and no other, so the failure names the member.
    /// </remarks>
    /// <param name="member">The name of the member the row mutates, quoted back as the failure
    /// message.</param>
    /// <param name="mutate">An action that changes exactly one member of the context it is
    /// handed.</param>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_MakesTheExtensionsUnequal(string member, Action<ITunesSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        ITunesSyndicationExtension first = Populated();
        ITunesSyndicationExtension second = Populated();

        mutate(second.Context);

        first.Equals(second).ShouldBeFalse($"{member} is not compared");
        (first != second).ShouldBeTrue($"{member} is not compared");
        first.CompareTo(second).ShouldNotBe(0, $"{member} is not compared");
    }

    /// <summary>
    /// Changing any single member changes the hash code.
    /// </summary>
    /// <remarks>
    ///     Separate from the equality rows because it is a weaker contract: equality <em>must</em>
    ///     distinguish these, whereas a hash code is only obliged not to collide gratuitously. It is
    ///     asserted because a member absent from <c>GetHashCode</c> but present in <c>CompareTo</c>
    ///     leaves a type that is correct in a <see cref="List{T}"/> and quietly lossy in a
    ///     <see cref="HashSet{T}"/>.
    /// </remarks>
    /// <param name="member">The name of the member the row mutates, quoted back as the failure
    /// message.</param>
    /// <param name="mutate">An action that changes exactly one member of the context it is
    /// handed.</param>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_ChangesTheHashCode(string member, Action<ITunesSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        ITunesSyndicationExtension first = Populated();
        ITunesSyndicationExtension second = Populated();

        mutate(second.Context);

        first.GetHashCode().ShouldNotBe(second.GetHashCode(), $"{member} does not contribute to the hash code");
    }

    /// <summary>
    /// Gets one row per member of the context, each mutating that member alone.
    /// </summary>
    /// <remarks>
    ///     Every mutation is written with a block body. An expression-bodied lambda whose body is an
    ///     assignment has the natural type <c>Func&lt;T, TResult&gt;</c> rather than
    ///     <see cref="Action{T}"/>, because an assignment is an expression that yields the assigned
    ///     value — and inside a collection expression targeting <c>object[]</c> there is no target type
    ///     to correct the inference.
    /// </remarks>
    public static IEnumerable<object[]> SingleMemberMutations =>
    [
        ["Author", (ITunesSyndicationExtensionContext c) => { c.Author = "Another Author"; }],
        ["Subtitle", (ITunesSyndicationExtensionContext c) => { c.Subtitle = "Another subtitle"; }],
        ["Summary", (ITunesSyndicationExtensionContext c) => { c.Summary = "Another summary"; }],
        ["Duration", (ITunesSyndicationExtensionContext c) => { c.Duration = new TimeSpan(4, 5, 6); }],
        ["ExplicitMaterial", (ITunesSyndicationExtensionContext c) => { c.ExplicitMaterial = ITunesExplicitMaterial.Yes; }],
        ["Image", (ITunesSyndicationExtensionContext c) => { c.Image = new Uri("https://example.com/other.png"); }],
        ["NewFeedUrl", (ITunesSyndicationExtensionContext c) => { c.NewFeedUrl = new Uri("https://example.com/elsewhere.xml"); }],
        ["IsBlocked", (ITunesSyndicationExtensionContext c) => { c.IsBlocked = true; }],
        ["Categories", (ITunesSyndicationExtensionContext c) => { c.Categories.Add(new ITunesCategory("Technology")); }],
        ["Keywords", (ITunesSyndicationExtensionContext c) => { c.Keywords.Add("a keyword"); }],
        ["Owner", (ITunesSyndicationExtensionContext c) => { c.Owner = new ITunesOwner { Name = "An Owner", EmailAddress = "owner@example.com" }; }],
        ["IsComplete", (ITunesSyndicationExtensionContext c) => { c.IsComplete = true; }],
        ["Title", (ITunesSyndicationExtensionContext c) => { c.Title = "A different episode name"; }],
        ["Episode", (ITunesSyndicationExtensionContext c) => { c.Episode = 43; }],
        ["Season", (ITunesSyndicationExtensionContext c) => { c.Season = 4; }],
        ["EpisodeType", (ITunesSyndicationExtensionContext c) => { c.EpisodeType = ITunesEpisodeType.Bonus; }],
        ["PodcastType", (ITunesSyndicationExtensionContext c) => { c.PodcastType = ITunesPodcastType.Serial; }],
        ["VerificationToken", (ITunesSyndicationExtensionContext c) => { c.VerificationToken = "668b25d0-3e5a-11f1-a5f6-473c059f121e"; }],
    ];
}