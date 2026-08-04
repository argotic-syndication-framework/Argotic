using Argotic.Extensions.Core;
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
    [TestMethod]
    public void SitemapImage_LocationsDifferingOnlyByCase_AreEqualAndHashEqually()
    {
        SitemapImage first = new(new Uri("http://example.com/Photo.JPG"));
        SitemapImage second = new(new Uri("http://example.com/photo.jpg"));

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [TestMethod]
    public void SitemapVideoSegment_LocationsDifferingOnlyByCase_AreEqualAndHashEqually()
    {
        SitemapVideoSegment first = new(new Uri("http://example.com/Clip.MP4"));
        SitemapVideoSegment second = new(new Uri("http://example.com/clip.mp4"));

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

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

    private static void AssertEqualAndSameHash<T>(T first, T second)
        where T : IEquatable<T>
    {
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }
}