using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

/// <summary>
/// Pins the <see cref="object.Equals(object)"/> / <see cref="object.GetHashCode"/> contract for the Media
/// RSS types, in both of the directions that matter.
/// </summary>
/// <remarks>
///     <para>
///         The contract is one-directional: equal objects <i>must</i> return equal hash codes. Hashing
///         <i>less</i> finely than comparing is legal and costs only probe time; hashing <i>more</i> finely
///         is a lookup miss. Both directions are asserted here, on different types, because both are
///         properties of the shipped code that nothing else checks.
///     </para>
///     <para>
///         <c>YahooMediaSyndicationExtension</c> broke the contract: its hash was
///         <c>HashCode.Combine(HashCodeUtility.Component(this.Context))</c>, and
///         <c>HashCodeUtility.Component&lt;T&gt;(T)</c> returns its argument unchanged, so
///         <see cref="HashCode.Combine{T1}(T1)"/> received a <c>YahooMediaSyndicationExtensionContext</c>
///         that overrode nothing and hashed by reference identity. Its <c>CompareTo</c> walks the context
///         member by member, so two extensions built from identical data were equal and hashed differently.
///         That is the tenth instance of the family recorded in <c>.endjin/build-warnings.md</c> §4.3.
///     </para>
///     <para>
///         <c>YahooMediaGroup</c> is the other direction and is deliberately left alone: it folds only
///         <c>Contents</c> while comparing the twelve shared metadata members as well.
///     </para>
/// </remarks>
[TestClass]
public class YahooMediaHashCodeContractTests
{
    /// <summary>
    /// Two extensions built from identical data — contents, groups and shared metadata alike — are equal
    /// and hash equally, so the type is usable as a dictionary key.
    /// </summary>
    [TestMethod]
    public void TwoExtensionsBuiltFromIdenticalData_AreEqualAndHashAlike()
    {
        AssertEqualAndSameHash(BuildPopulatedExtension(), BuildPopulatedExtension());
    }

    /// <summary>
    /// The same for an extension carrying nothing: two default instances are equal and hash equally.
    /// </summary>
    /// <remarks>
    ///     This is the cheapest possible witness of the defect — no data at all is needed, only two
    ///     instances, because the broken hash was the identity hash of the context object.
    /// </remarks>
    [TestMethod]
    public void TwoDefaultExtensions_AreEqualAndHashAlike()
    {
        AssertEqualAndSameHash(new YahooMediaSyndicationExtension(), new YahooMediaSyndicationExtension());
    }

    /// <summary>
    /// Two groups differing only in <c>Title</c> are <i>not</i> equal yet hash alike, because
    /// <c>YahooMediaGroup.GetHashCode</c> folds only <c>Contents</c> while its <c>CompareTo</c> also walks
    /// the twelve shared metadata members.
    /// </summary>
    /// <remarks>
    ///     This is legal and is deliberately not fixed: a hash coarser than the comparison collides, which
    ///     costs probe time and never yields a wrong answer. The sibling <c>YahooMediaContent</c> omits the
    ///     same twelve, so "coarser than its siblings" was not true either. The test exists to turn a silent
    ///     property into a checked one — if someone widens the hash, this fails and they must decide
    ///     deliberately rather than by accident.
    /// </remarks>
    [TestMethod]
    public void TwoGroupsDifferingOnlyInTitle_AreUnequalButHashAlike()
    {
        YahooMediaGroup first = new() { Title = new YahooMediaTextConstruct("First") };
        YahooMediaGroup second = new() { Title = new YahooMediaTextConstruct("Second") };

        first.Equals(second).ShouldBeFalse("the twelve shared members are compared");
        first.GetHashCode().ShouldBe(second.GetHashCode(), "but only Contents is folded into the hash");
    }

    /// <summary>
    /// Builds an extension carrying a content, a group and several of the shared metadata members.
    /// </summary>
    /// <returns>A populated extension.</returns>
    private static YahooMediaSyndicationExtension BuildPopulatedExtension()
    {
        YahooMediaSyndicationExtension extension = new();

        extension.Context.Title = new YahooMediaTextConstruct("A media item");
        extension.Context.Description = new YahooMediaTextConstruct("What the item is");
        extension.Context.Copyright = new YahooMediaCopyright("Copyright 2026");
        extension.Context.Player = new YahooMediaPlayer(new Uri("http://example.com/player"), 480, 640);
        extension.Context.Keywords.Add("first");
        extension.Context.Keywords.Add("second");
        extension.Context.Contents.Add(new YahooMediaContent(new Uri("http://example.com/video.mp4")) { ContentType = "video/mp4" });
        extension.Context.Categories.Add(new YahooMediaCategory("entertainment"));
        extension.Context.Credits.Add(new YahooMediaCredit("Jane Doe") { Role = "director" });
        extension.Context.Hashes.Add(new YahooMediaHash("dfdec888b72151965a34b4b59031290a"));
        extension.Context.Ratings.Add(new YahooMediaRating("nonadult"));
        extension.Context.Thumbnails.Add(new YahooMediaThumbnail(new Uri("http://example.com/thumb.jpg"), 100, 100));

        YahooMediaGroup group = new();
        group.Contents.Add(new YahooMediaContent(new Uri("http://example.com/hd.mp4")) { Height = 1080, Width = 1920 });
        extension.Context.Groups.Add(group);

        YahooMediaRestriction restriction = new() { EntityType = YahooMediaRestrictionType.Country, Relationship = YahooMediaRestrictionRelationship.Deny };
        restriction.Entities.Add("gb");
        extension.Context.Restrictions.Add(restriction);

        return extension;
    }

    /// <summary>
    /// Asserts that two distinct instances are equal, hash equally, and hash stably.
    /// </summary>
    /// <remarks>
    ///     The reference check comes first because it is what stops the rest being vacuous: two aliases of
    ///     one object satisfy every assertion below whatever the hash code does.
    /// </remarks>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="first">An instance.</param>
    /// <param name="second">A separately constructed instance built from identical data.</param>
    private static void AssertEqualAndSameHash<T>(T first, T second)
        where T : IEquatable<T>
    {
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }
}