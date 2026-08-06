using Argotic.Extensions.Core;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers the two GeoRSS value types, <see cref="GeoRssPosition"/> and <see cref="GeoRssBox"/>.
/// </summary>
/// <remarks>
///     <para>
///     These get their own file because they cannot be registered in
///     <c>ComparisonOperatorContractTests</c>, which every other comparable type in this library uses.
///     That table's <c>Row&lt;T&gt;</c> helper is declared <c>where T : class</c>, and so is the
///     <c>ComparisonOperatorExtensions</c> block it verifies — the relational operators arrive as C# 14
///     extension operators on reference types only.
///     </para>
///     <para>
///     So these two types declare <c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c> and <c>&gt;=</c> themselves and
///     are checked here instead. Being <c>record struct</c>s, their equality and hash code are generated
///     and <b>cannot drift behind their members</b> — which is the defect §2.48 records twice, designed
///     out rather than tested for.
///     </para>
/// </remarks>
[TestClass]
public sealed class GeoRssValueTypeTests
{
    private static readonly GeoRssPosition SantaCruz = new(36.981334686279m, -121.45983123779m);
    private static readonly GeoRssPosition London = new(51.5074m, -0.1278m);

    /// <summary>
    /// A position keeps latitude and longitude apart.
    /// </summary>
    /// <remarks>
    ///     The constructor takes them in wire order, latitude first. If that order were ever reversed,
    ///     every other test in this family would still pass — they would all be consistently wrong — so
    ///     this asserts the two members by name against values that are not interchangeable.
    /// </remarks>
    [TestMethod]
    public void APosition_KeepsLatitudeAndLongitudeApart()
    {
        SantaCruz.Latitude.ShouldBe(36.981334686279m);
        SantaCruz.Longitude.ShouldBe(-121.45983123779m);
    }

    /// <summary>
    /// A position preserves the scale it was given.
    /// </summary>
    /// <remarks>
    ///     <b>The test that catches a <see cref="double"/> where a <see cref="decimal"/> was meant.</b>
    ///     <c>-121.45983123779</c> is a real USGS value and survives a <see cref="decimal"/> exactly;
    ///     through a <see cref="float"/> it loses digits, and through a <see cref="double"/> the trailing
    ///     zero of a value like <c>-71.920</c> disappears.
    /// </remarks>
    [TestMethod]
    public void APosition_PreservesTheScaleItWasGiven()
    {
        SantaCruz.ToString().ShouldBe("36.981334686279 -121.45983123779");
        new GeoRssPosition(45.256m, -71.920m).ToString().ShouldBe("45.256 -71.920", "trailing zeros are part of the value");
    }

    /// <summary>
    /// Positions carrying the same coordinates are equal and hash alike.
    /// </summary>
    [TestMethod]
    public void PositionsCarryingTheSameCoordinates_AreEqual()
    {
        GeoRssPosition same = new(36.981334686279m, -121.45983123779m);

        (SantaCruz == same).ShouldBeTrue();
        SantaCruz.Equals(same).ShouldBeTrue();
        SantaCruz.GetHashCode().ShouldBe(same.GetHashCode());
        SantaCruz.CompareTo(same).ShouldBe(0);
    }

    /// <summary>
    /// Swapping latitude and longitude produces a different position.
    /// </summary>
    /// <remarks>
    ///     London is the case a range check cannot catch: swapped, it becomes latitude −0.1278 and
    ///     longitude 51.5074, both of which are perfectly legal coordinates. Only comparing the values
    ///     themselves distinguishes them.
    /// </remarks>
    [TestMethod]
    public void SwappingLatitudeAndLongitude_ProducesADifferentPosition()
    {
        GeoRssPosition swapped = new(London.Longitude, London.Latitude);

        (London == swapped).ShouldBeFalse("both orderings are in range, so only the values tell them apart");
        (London != swapped).ShouldBeTrue();
    }

    /// <summary>
    /// Positions order by latitude and then longitude.
    /// </summary>
    [TestMethod]
    public void Positions_OrderByLatitudeThenLongitude()
    {
        GeoRssPosition souther = new(10m, 50m);
        GeoRssPosition norther = new(20m, 0m);

        (souther < norther).ShouldBeTrue("latitude is compared first");
        (norther > souther).ShouldBeTrue();

        // Separate instances rather than the same variable twice: comparing a value to itself is a
        // tautology the compiler warns about, and it would pass even if the operators ignored equality.
        GeoRssPosition southerAgain = new(10m, 50m);
        (souther <= southerAgain).ShouldBeTrue();
        (souther >= southerAgain).ShouldBeTrue();

        GeoRssPosition wester = new(10m, 40m);
        (wester < souther).ShouldBeTrue("longitude breaks a latitude tie");
    }

    /// <summary>
    /// A box keeps its two corners apart.
    /// </summary>
    /// <remarks>
    ///     The four numbers are the real Blogger value from the corpus. They are pairwise distinct, so
    ///     any of the orderings other than the correct one fails this.
    /// </remarks>
    [TestMethod]
    public void ABox_KeepsItsTwoCornersApart()
    {
        GeoRssBox box = new(new GeoRssPosition(23.822399163821153m, -29.864984m), new GeoRssPosition(80.442866836178837m, 40.447516m));

        box.LowerLeft.Latitude.ShouldBe(23.822399163821153m);
        box.LowerLeft.Longitude.ShouldBe(-29.864984m);
        box.UpperRight.Latitude.ShouldBe(80.442866836178837m);
        box.UpperRight.Longitude.ShouldBe(40.447516m);

        box.LowerLeft.Latitude.ShouldBeLessThan(box.UpperRight.Latitude, "lower-left is south of upper-right");
        box.LowerLeft.Longitude.ShouldBeLessThan(box.UpperRight.Longitude, "and west of it");
    }

    /// <summary>
    /// A box reports whether its corners are the right way round, rather than correcting them.
    /// </summary>
    /// <remarks>
    ///     Swapping the corners silently would report a geometry the feed does not contain; refusing to
    ///     read it would lose the only location the entry has. Reporting is the third option, and it is
    ///     the one that keeps both the data and the truth about it.
    /// </remarks>
    [TestMethod]
    public void ABox_ReportsWhetherItsCornersAreTheRightWayRound()
    {
        new GeoRssBox(new GeoRssPosition(23m, -29m), new GeoRssPosition(80m, 40m)).IsWellOriented.ShouldBeTrue();
        new GeoRssBox(new GeoRssPosition(80m, 40m), new GeoRssPosition(23m, -29m)).IsWellOriented.ShouldBeFalse();
    }

    /// <summary>
    /// A box renders as the four numbers it is written from.
    /// </summary>
    [TestMethod]
    public void ABox_RendersAsTheFourNumbersItIsWrittenFrom()
    {
        GeoRssBox box = new(new GeoRssPosition(42.943m, -71.032m), new GeoRssPosition(43.039m, -69.856m));

        box.ToString().ShouldBe("42.943 -71.032 43.039 -69.856");
    }

    /// <summary>
    /// Boxes carrying the same corners are equal and hash alike.
    /// </summary>
    [TestMethod]
    public void BoxesCarryingTheSameCorners_AreEqual()
    {
        GeoRssBox first = new(new GeoRssPosition(1m, 2m), new GeoRssPosition(3m, 4m));
        GeoRssBox second = new(new GeoRssPosition(1m, 2m), new GeoRssPosition(3m, 4m));
        GeoRssBox other = new(new GeoRssPosition(1m, 2m), new GeoRssPosition(3m, 5m));

        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        (first == other).ShouldBeFalse();
        (first < other).ShouldBeTrue();
        (other > first).ShouldBeTrue();
        (first <= second).ShouldBeTrue();
        (first >= second).ShouldBeTrue();
    }
}