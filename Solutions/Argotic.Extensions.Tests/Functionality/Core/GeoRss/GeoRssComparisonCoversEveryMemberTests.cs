using Argotic.Extensions.Core;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers every member of <see cref="GeoRssSyndicationExtensionContext"/> against the comparison
/// contract that <see cref="GeoRssSyndicationExtension"/> declares.
/// </summary>
/// <remarks>
///     The third of these, after the iTunes one that found the defect (§2.48) and the Podcasting 2.0 one
///     that was written to prevent it recurring. <c>CompareTo</c> is a hand-maintained list of members,
///     and a hand-maintained list falls behind the moment somebody adds a property — which is exactly
///     what adding a property feels like nothing to do with.
/// </remarks>
[TestClass]
public sealed class GeoRssComparisonCoversEveryMemberTests
{
    /// <summary>
    /// Builds an extension whose context carries a value in every member the comparison must see.
    /// </summary>
    private static GeoRssSyndicationExtension Populated()
    {
        GeoRssSyndicationExtension extension = new();
        extension.Context.Point = new GeoRssPosition(36.981334686279m, -121.45983123779m);
        extension.Context.Line = new GeoRssLine([new GeoRssPosition(45.256m, -110.45m), new GeoRssPosition(46.46m, -109.48m)]);
        extension.Context.Polygon = new GeoRssPolygon([new GeoRssPosition(1m, 1m), new GeoRssPosition(2m, 2m), new GeoRssPosition(3m, 3m), new GeoRssPosition(1m, 1m)]);
        extension.Context.Box = new GeoRssBox(new GeoRssPosition(42.943m, -71.032m), new GeoRssPosition(43.039m, -69.856m));
        extension.Context.Elevation = -5280.0002098083m;
        extension.Context.Floor = 2;
        extension.Context.Radius = 500m;
        extension.Context.FeatureTypeTag = "city";
        extension.Context.RelationshipTag = "is-centered-at";
        extension.Context.FeatureName = "Podunk";
        extension.Context.GeometryIsWrappedInWhere = false;
        extension.Context.Encoding = GeoRssEncoding.Simple;
        return extension;
    }

    /// <summary>
    /// Two extensions carrying the same values are equal, and agree on their hash code.
    /// </summary>
    /// <remarks>
    ///     The control, and here it earns its keep twice over: <see cref="GeoRssLine"/> and
    ///     <see cref="GeoRssPolygon"/> hold collections, so this is what fails if either hashed by
    ///     reference while comparing by content — a type that behaves in a <see cref="List{T}"/> and is
    ///     quietly lossy in a <see cref="HashSet{T}"/>.
    /// </remarks>
    [TestMethod]
    public void TwoExtensionsCarryingTheSameValues_AreEqual()
    {
        GeoRssSyndicationExtension first = Populated();
        GeoRssSyndicationExtension second = Populated();

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        first.CompareTo(second).ShouldBe(0);
        first.GetHashCode().ShouldBe(second.GetHashCode(), "equal geometries must hash alike, contents and not references");
    }

    /// <summary>
    /// Changing any single member makes the two extensions unequal.
    /// </summary>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_MakesTheExtensionsUnequal(string member, Action<GeoRssSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        GeoRssSyndicationExtension first = Populated();
        GeoRssSyndicationExtension second = Populated();

        mutate(second.Context);

        first.Equals(second).ShouldBeFalse($"{member} is not compared");
        (first != second).ShouldBeTrue($"{member} is not compared");
        first.CompareTo(second).ShouldNotBe(0, $"{member} is not compared");
    }

    /// <summary>
    /// Changing any single member changes the hash code.
    /// </summary>
    [TestMethod]
    [DynamicData(nameof(SingleMemberMutations))]
    public void ChangingASingleMember_ChangesTheHashCode(string member, Action<GeoRssSyndicationExtensionContext> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        GeoRssSyndicationExtension first = Populated();
        GeoRssSyndicationExtension second = Populated();

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
        ["Point", (GeoRssSyndicationExtensionContext c) => { c.Point = new GeoRssPosition(51.5074m, -0.1278m); }],
        ["Line", (GeoRssSyndicationExtensionContext c) => { c.Line = new GeoRssLine([new GeoRssPosition(1m, 1m), new GeoRssPosition(9m, 9m)]); }],
        ["Polygon", (GeoRssSyndicationExtensionContext c) => { c.Polygon = new GeoRssPolygon([new GeoRssPosition(4m, 4m), new GeoRssPosition(5m, 5m), new GeoRssPosition(6m, 6m), new GeoRssPosition(4m, 4m)]); }],
        ["Box", (GeoRssSyndicationExtensionContext c) => { c.Box = new GeoRssBox(new GeoRssPosition(1m, 1m), new GeoRssPosition(2m, 2m)); }],
        ["Elevation", (GeoRssSyndicationExtensionContext c) => { c.Elevation = 313m; }],
        ["Floor", (GeoRssSyndicationExtensionContext c) => { c.Floor = -1; }],
        ["Radius", (GeoRssSyndicationExtensionContext c) => { c.Radius = 1000m; }],
        ["FeatureTypeTag", (GeoRssSyndicationExtensionContext c) => { c.FeatureTypeTag = "county"; }],
        ["RelationshipTag", (GeoRssSyndicationExtensionContext c) => { c.RelationshipTag = "is-near"; }],
        ["FeatureName", (GeoRssSyndicationExtensionContext c) => { c.FeatureName = "Nederland"; }],
        ["GeometryIsWrappedInWhere", (GeoRssSyndicationExtensionContext c) => { c.GeometryIsWrappedInWhere = true; }],
        ["Encoding", (GeoRssSyndicationExtensionContext c) => { c.Encoding = GeoRssEncoding.Gml; }],
    ];
}