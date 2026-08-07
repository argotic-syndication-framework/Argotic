using Argotic.Extensions.Core;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers the GeoRSS coordinate-list grammar: what is accepted, what is refused, and what is neither.
/// </summary>
/// <remarks>
///     <para>
///     Every GeoRSS geometry is the same thing on the wire — a whitespace-separated run of decimal
///     numbers read as latitude/longitude pairs — so this one grammar is the entire parse surface of the
///     family, and this file is its specification.
///     </para>
///     <para>
///     The governing rule, in one line: <b>reject the element when it cannot be represented faithfully,
///     accept and report when it can, and never repair.</b> That divides the cases into two kinds, and
///     the division is the point. A value that cannot be read at all is refused outright rather than
///     half-read. A value that reads perfectly well but breaks a rule of the specification is kept, and
///     the breach is reported through a property.
///     </para>
/// </remarks>
[TestClass]
public sealed class GeoRssGeometryParsingTests
{
    /// <summary>
    /// A coordinate list that cannot be read as whole pairs is refused entirely.
    /// </summary>
    /// <remarks>
    ///     <b>All or nothing, and the reason is that a partial geometry is a wrong geometry.</b> Reading
    ///     the good prefix of <c>"45.256 -110.45 46.46"</c> gives a point somewhere real, which is worse
    ///     than reading nothing: nothing is visibly absent, whereas a truncated geometry is silently
    ///     incorrect. The same applies to a token that will not parse.
    /// </remarks>
    /// <param name="written">The node value to write into <c>georss:line</c>; anything that is not a
    /// whitespace-separated run of an even number of invariant-culture decimals.</param>
    /// <param name="why">Why the value cannot be read, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("45.256 -110.45 46.46", "an odd number of coordinates leaves one without a partner")]
    [DataRow("45.256", "a single coordinate is not a position")]
    [DataRow("north south", "words are not coordinates")]
    [DataRow("45.256 abc", "one bad token fails the element, not just itself")]
    [DataRow("45.256,-71.92", "the separator is whitespace; a comma is not a separator here")]
    [DataRow("45,256 -71,92", "and a comma decimal separator is not the invariant culture")]
    [DataRow("", "an empty element says nothing")]
    [DataRow("   ", "and neither does whitespace")]
    [DataRow("NaN NaN", "decimal cannot represent NaN, so the parse refuses it")]
    public void ACoordinateListThatCannotBeReadAsWholePairs_IsRefusedEntirely(string written, string why)
    {
        GeoRssSyndicationExtensionContext? context =
            GeoRssSyndicationExtensionTests.ItemContext($"<georss:line>{written}</georss:line>");

        (context?.Line).ShouldBeNull(why);
    }

    /// <summary>
    /// A point carrying anything other than one pair is refused.
    /// </summary>
    /// <remarks>
    ///     Two pairs is not a point. Taking the first and discarding the second would invent a geometry
    ///     the publisher did not write.
    /// </remarks>
    /// <param name="written">The node value to write into <c>georss:point</c>; any number of
    /// coordinates other than two.</param>
    /// <param name="why">Why the value is not a point, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("45.256 -110.45 46.46 -109.48", "two pairs is a line or a box, not a point")]
    [DataRow("45.256", "half a pair is not a point either")]
    public void APointCarryingAnythingOtherThanOnePair_IsRefused(string written, string why)
    {
        GeoRssSyndicationExtensionContext? context =
            GeoRssSyndicationExtensionTests.ItemContext($"<georss:point>{written}</georss:point>");

        (context?.Point).ShouldBeNull(why);
    }

    /// <summary>
    /// A box carrying anything other than two pairs is refused.
    /// </summary>
    /// <param name="written">The node value to write into <c>georss:box</c>; any number of
    /// coordinates other than four.</param>
    /// <param name="why">Why the value is not a box, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("42.943 -71.032", "one corner does not bound anything")]
    [DataRow("42.943 -71.032 43.039 -69.856 44.0 -68.0", "three corners is not a box")]
    public void ABoxCarryingAnythingOtherThanTwoPairs_IsRefused(string written, string why)
    {
        GeoRssSyndicationExtensionContext? context =
            GeoRssSyndicationExtensionTests.ItemContext($"<georss:box>{written}</georss:box>");

        (context?.Box).ShouldBeNull(why);
    }

    /// <summary>
    /// Any run of whitespace separates coordinates.
    /// </summary>
    /// <remarks>
    ///     <b>The most likely place for a real bug.</b> A polygon written one pair per indented line is
    ///     the ordinary shape a pretty-printing publisher produces, and splitting that on a space
    ///     character yields empty entries and tokens with newlines still attached. Every row here is the
    ///     same three positions written differently, and all three must agree.
    /// </remarks>
    /// <param name="written">The same three positions, written with a different arrangement of
    /// spaces, tabs and newlines each time.</param>
    /// <param name="shape">The arrangement the row stands for, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("45.256 -110.45 46.46 -109.48 43.84 -109.86", "single spaces")]
    [DataRow("  45.256   -110.45\t46.46 -109.48\n43.84 -109.86  ", "mixed runs, tabs, newlines, and surrounding space")]
    [DataRow("45.256 -110.45\n      46.46 -109.48\n      43.84 -109.86", "one pair per indented line")]
    public void AnyRunOfWhitespace_SeparatesCoordinates(string written, string shape)
    {
        GeoRssSyndicationExtensionContext? context =
            GeoRssSyndicationExtensionTests.ItemContext($"<georss:line>{written}</georss:line>");

        context.ShouldNotBeNull(shape);
        context.Line.ShouldNotBeNull(shape);
        context.Line.Positions.Count.ShouldBe(3, shape);
        context.Line.Positions[0].ShouldBe(new GeoRssPosition(45.256m, -110.45m), shape);
        context.Line.Positions[2].ShouldBe(new GeoRssPosition(43.84m, -109.86m), shape);
    }

    /// <summary>
    /// A coordinate list that reads cleanly but breaks a rule is kept.
    /// </summary>
    /// <remarks>
    ///     The other half of the rule. A one-position line and a three-position polygon are both
    ///     non-conformant and both perfectly readable, so they are read. Enforcement would lose data;
    ///     repair would invent it.
    /// </remarks>
    [TestMethod]
    public void ACoordinateListThatReadsCleanlyButBreaksARule_IsKept()
    {
        GeoRssSyndicationExtensionContext? line =
            GeoRssSyndicationExtensionTests.ItemContext("<georss:line>45.256 -110.45</georss:line>");
        line!.Line.ShouldNotBeNull();
        line.Line.Positions.Count.ShouldBe(1, "the specification wants two or more; the feed said one");

        GeoRssSyndicationExtensionContext? polygon =
            GeoRssSyndicationExtensionTests.ItemContext("<georss:polygon>45.256 -110.45 46.46 -109.48 43.84 -109.86</georss:polygon>");
        polygon!.Polygon.ShouldNotBeNull();
        polygon.Polygon.Positions.Count.ShouldBe(3);
        polygon.Polygon.IsClosed.ShouldBeFalse("fewer than four positions cannot be a closed ring");
    }

    /// <summary>
    /// Exponent notation is a number.
    /// </summary>
    /// <remarks>
    ///     Nothing in the corpus writes coordinates this way. It is pinned because the parse admits it as
    ///     a consequence of using <c>NumberStyles.Float</c>, and an untested consequence is an accident
    ///     rather than a decision.
    /// </remarks>
    [TestMethod]
    public void ExponentNotation_IsANumber()
    {
        GeoRssSyndicationExtensionContext? context =
            GeoRssSyndicationExtensionTests.ItemContext("<georss:point>4.5256E1 -1.1045E2</georss:point>");

        context!.Point.ShouldBe(new GeoRssPosition(45.256m, -110.45m));
    }

    /// <summary>
    /// A refused geometry does not fail the rest of the entry.
    /// </summary>
    /// <remarks>
    ///     <b>The containment guarantee.</b> One malformed element among 387 must not cost a consumer the
    ///     whole feed, so a refusal is local and silent. That silence is a real limitation — <c>Load</c>
    ///     returns a <see cref="bool"/> and has no channel to report why — and it is the reason the
    ///     refusal rules above are pinned by tests rather than left to be discovered.
    /// </remarks>
    [TestMethod]
    public void ARefusedGeometry_DoesNotFailTheRestOfTheEntry()
    {
        GeoRssSyndicationExtensionContext? context = GeoRssSyndicationExtensionTests.ItemContext("""
            <georss:point>not a point at all</georss:point>
            <georss:elev>313</georss:elev>
            <georss:featurename>Podunk</georss:featurename>
            """);

        context.ShouldNotBeNull("the entry still loads");
        context.Point.ShouldBeNull("the bad element is dropped");
        context.Elevation.ShouldBe(313m, "its neighbours are not");
        context.FeatureName.ShouldBe("Podunk");
    }
}