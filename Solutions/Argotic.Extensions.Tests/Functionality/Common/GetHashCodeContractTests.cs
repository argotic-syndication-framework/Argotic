using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins the <see cref="object.Equals(object)"/> / <see cref="object.GetHashCode"/> contract for the types
/// whose hash codes were previously inconsistent with their comparison semantics.
/// </summary>
/// <remarks>
///     <para>
///         Two defects are covered. The first is a collection member folded into the hash by reference:
///         <c>HashCodeUtility.Component&lt;T&gt;(T)</c> returns its argument unchanged, so passing an
///         <see cref="IList{T}"/> to it made <see cref="HashCode.Combine"/> hash the list instance rather
///         than its elements. Every <c>CompareTo</c> involved compares those collections element by element,
///         so two instances built from identical data compared equal and hashed differently.
///     </para>
///     <para>
///         The second is a <see cref="Uri"/> hashed with <see cref="Uri.GetHashCode"/>, which is case-sensitive
///         over the path, while the matching <c>CompareTo</c> uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
///     </para>
///     <para>
///         Each test asserts the direction that actually matters: equal implies equal hash. The converse is not
///         required of a hash code and is not asserted.
///     </para>
/// </remarks>
[TestClass]
public class GetHashCodeContractTests
{
    /// <summary>
    /// <c>http://example.com/Photo.JPG</c> and <c>http://example.com/photo.jpg</c> are equal
    /// <c>SitemapImage</c>s, and hash equally — the case in the path must not reach the hash code.
    /// </summary>
    [TestMethod]
    public void SitemapImage_LocationsDifferingOnlyByCase_AreEqualAndHashEqually()
    {
        SitemapImage first = new(new Uri("http://example.com/Photo.JPG"));
        SitemapImage second = new(new Uri("http://example.com/photo.jpg"));

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// The same for <c>SitemapVideoSegment</c>: <c>Clip.MP4</c> and <c>clip.mp4</c> are equal and hash
    /// equally.
    /// </summary>
    [TestMethod]
    public void SitemapVideoSegment_LocationsDifferingOnlyByCase_AreEqualAndHashEqually()
    {
        SitemapVideoSegment first = new(new Uri("http://example.com/Clip.MP4"));
        SitemapVideoSegment second = new(new Uri("http://example.com/clip.mp4"));

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two pingback extensions built from the same data — including a two-element <c>Abouts</c> collection —
    /// hash equally, so the collection is folded in by its elements rather than by its instance.
    /// </summary>
    [TestMethod]
    public void PingbackSyndicationExtension_EqualAbouts_HashEqually()
    {
        static PingbackSyndicationExtension Build()
        {
            PingbackSyndicationExtension extension = new();
            extension.Context.Server = new Uri("http://example.com/pingback");
            extension.Context.Target = new Uri("http://example.com/post/1");
            extension.Context.Abouts.Add(new Uri("http://example.com/about/1"));
            extension.Context.Abouts.Add(new Uri("http://example.com/about/2"));
            return extension;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// The same for the trackback extension's <c>Abouts</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackSyndicationExtension_EqualAbouts_HashEqually()
    {
        static TrackbackSyndicationExtension Build()
        {
            TrackbackSyndicationExtension extension = new();
            extension.Context.Ping = new Uri("http://example.com/trackback");
            extension.Context.Abouts.Add(new Uri("http://example.com/about/1"));
            return extension;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two slash extensions carrying the same <c>HitParade</c> — <c>3</c>, <c>1</c>, <c>4</c>, in that order
    /// — hash equally, so a collection of value types is folded in by its elements too.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashSyndicationExtension_EqualHitParade_HashEqually()
    {
        static SiteSummarySlashSyndicationExtension Build()
        {
            SiteSummarySlashSyndicationExtension extension = new();
            extension.Context.Comments = 7;
            extension.Context.Department = "technology";
            extension.Context.Section = "articles";
            extension.Context.HitParade.Add(3);
            extension.Context.HitParade.Add(1);
            extension.Context.HitParade.Add(4);
            return extension;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two feed-history extensions carrying the same <c>Relations</c> entry hash equally, so a collection
    /// whose elements are themselves comparable types is folded in by those elements.
    /// </summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_EqualRelations_HashEqually()
    {
        static FeedHistorySyndicationExtension Build()
        {
            FeedHistorySyndicationExtension extension = new();
            extension.Context.IsArchive = true;
            extension.Context.Relations.Add(
                new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/page/1")));
            return extension;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two simple-list extensions agreeing on <i>both</i> their <c>Grouping</c> and <c>Sorting</c>
    /// collections hash equally.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_EqualGroupingAndSorting_HashEqually()
    {
        static SimpleListSyndicationExtension Build()
        {
            SimpleListSyndicationExtension extension = new();
            extension.Context.TreatAsList = true;
            extension.Context.Grouping.Add(new SimpleListGroup { Element = "category", Label = "Category" });
            extension.Context.Sorting.Add(new SimpleListSort { Element = "price", Label = "Price", IsDefault = true });
            return extension;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two media groups carrying the same single <c>Contents</c> entry hash equally.
    /// </summary>
    [TestMethod]
    public void YahooMediaGroup_EqualContents_HashEqually()
    {
        static YahooMediaGroup Build()
        {
            YahooMediaGroup group = new();
            group.Contents.Add(new YahooMediaContent(new Uri("http://example.com/media.mp4")));
            return group;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two media restrictions carrying the same <c>Entities</c> — <c>us</c> then <c>gb</c> — hash equally,
    /// with the entity type and relationship folded in alongside them.
    /// </summary>
    [TestMethod]
    public void YahooMediaRestriction_EqualEntities_HashEqually()
    {
        static YahooMediaRestriction Build()
        {
            YahooMediaRestriction restriction = new()
            {
                EntityType = YahooMediaRestrictionType.Country,
                Relationship = YahooMediaRestrictionRelationship.Allow,
            };
            restriction.Entities.Add("us");
            restriction.Entities.Add("gb");
            return restriction;
        }

        AssertEqualAndSameHash(Build(), Build());
    }

    /// <summary>
    /// Two heads differing only by <c>Title</c> are unequal — the third defect class, and the one the
    /// §4.3 sweep was looking for and did not find: a member hashed more finely than it is compared.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>OpmlHead.CompareTo</c> was <c>int result = 0;</c> followed by a commented-out comparison of
    ///         a <c>Domain</c> member the type does not have — copy-pasted from another type, so it was never
    ///         the real comparison. <c>Equals</c> is <c>CompareTo(other) == 0</c>, so <i>every</i> pair of
    ///         heads was equal, while <c>GetHashCode</c> folded six members. Two heads that differ in all six
    ///         were equal and hashed differently: a genuine dictionary lookup miss, not a probe-time cost.
    ///     </para>
    ///     <para>
    ///         <c>docs/build-warnings.md</c> §4.3 closes with "A sweep for the inverse defect, a member
    ///         hashed <i>more finely</i> than it is compared, found none." This is that defect. The sweep
    ///         missed it because every type it examined came from <c>Argotic.Extensions.Core</c>; it never
    ///         covered <c>Argotic.Core</c>. A sweep is only as good as the population it names, and that one
    ///         did not name its population.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void OpmlHead_HeadsDifferingOnlyByTitle_AreUnequal()
    {
        OpmlHead first = new() { Title = "Head A" };
        OpmlHead second = new() { Title = "Head B" };

        first.Equals(second).ShouldBeFalse();
        first.CompareTo(second).ShouldBeLessThan(0);
        second.CompareTo(first).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Two heads built from identical data — across all six compared members — are equal and hash equally.
    /// </summary>
    /// <remarks>
    ///     <c>ExpansionState</c> is deliberately absent from both the comparison and the hash. Folding a
    ///     collection in by reference is exactly the defect §4.3 fixed in seven types, and adding it to the
    ///     comparison without folding its elements one at a time would reintroduce it verbatim.
    /// </remarks>
    [TestMethod]
    public void OpmlHead_EqualHeads_HashEqually()
    {
        static OpmlHead Build() => new()
        {
            Title = "Subscriptions",
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedOn = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            VerticalScrollState = 12,
            Owner = new OpmlOwner { Name = "Ada", EmailAddress = "ada@example.com" },
            Window = new OpmlWindow { Top = 1, Left = 2, Bottom = 3, Right = 4 },
        };

        AssertEqualAndSameHash(Build(), Build());
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