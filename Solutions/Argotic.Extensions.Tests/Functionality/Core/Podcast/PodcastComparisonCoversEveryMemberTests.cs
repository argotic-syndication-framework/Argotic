namespace Argotic.Extensions.Tests.Functionality.Core.Podcast;

/// <summary>
/// Covers every member of <see cref="PodcastSyndicationExtensionContext"/> against the comparison
/// contract that <see cref="PodcastSyndicationExtension"/> declares.
/// </summary>
/// <remarks>
///     <para>
///     The twin of <c>ITunesComparisonCoversEveryMemberTests</c>, and it exists because that one found
///     a real defect: five members added to the iTunes context never reached its <c>CompareTo</c>, so
///     two extensions describing different episodes reported themselves equal (§2.48).
///     </para>
///     <para>
///     A namespace that gains tags over time — as this one explicitly does — will gain context members
///     over time, and each is a chance to repeat that. Writing the guard at the same moment as the
///     family is cheaper than writing it after the second occurrence.
///     </para>
/// </remarks>
[TestClass]
public sealed class PodcastComparisonCoversEveryMemberTests
{
    /// <summary>
    /// Builds an extension whose context carries a value in every member the comparison must see.
    /// </summary>
    private static PodcastSyndicationExtension Populated()
    {
        PodcastSyndicationExtension extension = new();
        extension.Context.IsLocked = true;
        extension.Context.LockOwner = "owner@example.com";
        extension.Context.Identifier = "917393e3-1b1e-5cef-ace4-edaa54e1f810";
        extension.Context.Medium = PodcastMedium.Podcast;
        extension.Context.MediumIsList = false;
        extension.Context.UsesPodping = true;
        extension.Context.Season = 3;
        extension.Context.SeasonName = "A Season";
        extension.Context.Episode = 42m;
        extension.Context.EpisodeDisplay = "Ep. 42";
        extension.Context.Chapters = new(new Uri("https://example.com/chapters.json"), "application/json+chapters");
        extension.Context.License = new("cc-by-4.0");
        extension.Context.Transcripts.Add(new(new Uri("https://example.com/t.vtt"), "text/vtt"));
        extension.Context.FundingLinks.Add(new(new Uri("https://example.com/donate"), "Support the show!"));
        extension.Context.People.Add(new("Alice Example"));
        extension.Context.TextEntries.Add(new(PodcastText.ApplePodcastsVerifyPurpose, "e6cfaae0-9496-11f0-a272-f9e230f88be0"));
        return extension;
    }

    /// <summary>
    /// Two extensions carrying the same values are equal, and agree on their hash code.
    /// </summary>
    [TestMethod]
    public void TwoExtensionsCarryingTheSameValues_AreEqual()
    {
        PodcastSyndicationExtension first = Populated();
        PodcastSyndicationExtension second = Populated();

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        first.CompareTo(second).ShouldBe(0);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Changing any single member makes the two extensions unequal.
    /// </summary>
    /// <param name="member">The name of the member the row mutates, quoted back as the failure
    /// message.</param>
    /// <param name="mutate">An action that changes exactly one member of the context it is
    /// handed.</param>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_MakesTheExtensionsUnequal(string member, Action<PodcastSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        PodcastSyndicationExtension first = Populated();
        PodcastSyndicationExtension second = Populated();

        mutate(second.Context);

        first.Equals(second).ShouldBeFalse($"{member} is not compared");
        (first != second).ShouldBeTrue($"{member} is not compared");
        first.CompareTo(second).ShouldNotBe(0, $"{member} is not compared");
    }

    /// <summary>
    /// Changing any single member changes the hash code.
    /// </summary>
    /// <param name="member">The name of the member the row mutates, quoted back as the failure
    /// message.</param>
    /// <param name="mutate">An action that changes exactly one member of the context it is
    /// handed.</param>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_ChangesTheHashCode(string member, Action<PodcastSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        PodcastSyndicationExtension first = Populated();
        PodcastSyndicationExtension second = Populated();

        mutate(second.Context);

        first.GetHashCode().ShouldNotBe(second.GetHashCode(), $"{member} does not contribute to the hash code");
    }

    /// <summary>
    /// Gets one row per member of the context, each mutating that member alone.
    /// </summary>
    /// <remarks>
    ///     Block bodies throughout: an expression-bodied lambda whose body is an assignment has the
    ///     natural type <c>Func&lt;T, TResult&gt;</c>, not <see cref="Action{T}"/>, and a collection
    ///     expression targeting <c>object[]</c> supplies no target type to correct that.
    /// </remarks>
    public static IEnumerable<object[]> SingleMemberMutations =>
    [
        ["Chapters", (PodcastSyndicationExtensionContext c) => { c.Chapters = new(new Uri("https://example.com/other.json"), "application/json+chapters"); }],
        ["Episode", (PodcastSyndicationExtensionContext c) => { c.Episode = 43m; }],
        ["EpisodeDisplay", (PodcastSyndicationExtensionContext c) => { c.EpisodeDisplay = "Ep. 43"; }],
        ["FundingLinks", (PodcastSyndicationExtensionContext c) => { c.FundingLinks.Add(new(new Uri("https://example.com/more"), "Another way")); }],
        ["Identifier", (PodcastSyndicationExtensionContext c) => { c.Identifier = "9b024349-ccf0-5f69-a609-6b82873eab3c"; }],
        ["IsLocked", (PodcastSyndicationExtensionContext c) => { c.IsLocked = false; }],
        ["License", (PodcastSyndicationExtensionContext c) => { c.License = new("cc-by-sa-4.0"); }],
        ["LockOwner", (PodcastSyndicationExtensionContext c) => { c.LockOwner = "someone.else@example.com"; }],
        ["Medium", (PodcastSyndicationExtensionContext c) => { c.Medium = PodcastMedium.Music; }],
        ["MediumIsList", (PodcastSyndicationExtensionContext c) => { c.MediumIsList = true; }],
        ["People", (PodcastSyndicationExtensionContext c) => { c.People.Add(new("Bob Example")); }],
        ["Season", (PodcastSyndicationExtensionContext c) => { c.Season = 4; }],
        ["SeasonName", (PodcastSyndicationExtensionContext c) => { c.SeasonName = "Another Season"; }],
        ["TextEntries", (PodcastSyndicationExtensionContext c) => { c.TextEntries.Add(new("example.com", "another value")); }],
        ["Transcripts", (PodcastSyndicationExtensionContext c) => { c.Transcripts.Add(new(new Uri("https://example.com/t.srt"), "application/x-subrip")); }],
        ["UsesPodping", (PodcastSyndicationExtensionContext c) => { c.UsesPodping = false; }],
    ];
}