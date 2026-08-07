using System.Xml.XPath;

using Argotic.Syndication.Specialized;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

/// <summary>
/// Covers how the three scored APML entities — <see cref="ApmlConcept"/>, <see cref="ApmlSource"/> and
/// <see cref="ApmlAuthor"/> — serialise a score they were never given, and what they do with a score that
/// arrives outside the range APML declares for it.
/// </summary>
/// <remarks>
///     <para>
///     All three used to default <c>Value</c> to <see cref="decimal.MinValue"/> — a value their own setters
///     would have rejected — and write it unconditionally, so an unscored entity emitted
///     <c>value="-79228162514264337593543950335.00"</c>.
///     </para>
///     <para>
///     The worse case was a load. <c>Load</c> assigns <c>Value</c> only when the attribute parses <i>and</i>
///     falls in <c>[-1, 1]</c>, while <c>wasLoaded</c> is already true from <c>key</c> alone. So
///     <c>&lt;Concept key="tech" value="1.5"/&gt;</c> — a mildly non-conformant real document — loaded
///     successfully sitting at the sentinel, and saving wrote the sentinel back. A small conformance error
///     in became a grotesque one out.
///     </para>
///     <para>
///     <c>Value</c> is now <see cref="decimal"/>?, defaulting to <see langword="null"/>, and the attribute
///     is omitted when it is null. Omission is the only serialisation that says "unknown" honestly: in an
///     attention-profiling format the score <i>is</i> the payload, and writing <c>0.00</c> would invent a
///     neutral-interest assertion the profile never made. Whether <c>value</c> is normatively REQUIRED is
///     unverified — the APML 0.6 specification is archive-only — so this takes the conservative reading and
///     states the deviation rather than guessing a number.
///     </para>
/// </remarks>
[TestClass]
public class ApmlScoreSerializationTests
{
    /// <summary>
    /// A concept that was never given a score omits the <c>value</c> attribute rather than emitting a
    /// sentinel, and omits <c>key</c> rather than emitting an empty one.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_WithNoScoreAssigned_OmitsTheValueAndKeyAttributes()
    {
        ApmlConcept concept = new();

        concept.Value.ShouldBeNull();

        string xml = concept.ToString();

        xml.ShouldNotContain("value=", Case.Sensitive);
        xml.ShouldNotContain("key=", Case.Sensitive);
        xml.ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// The same for a source, whose <c>name</c> and <c>type</c> attributes are unaffected.
    /// </summary>
    [TestMethod]
    public void ApmlSource_WithNoScoreAssigned_OmitsTheValueAttribute()
    {
        ApmlSource source = new();

        source.Value.ShouldBeNull();
        source.ToString().ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// The same for an author.
    /// </summary>
    [TestMethod]
    public void ApmlAuthor_WithNoScoreAssigned_OmitsTheValueAttribute()
    {
        ApmlAuthor author = new();

        author.Value.ShouldBeNull();
        author.ToString().ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// A concept carrying an out-of-range score still loads — the key is enough — but leaves the score
    /// unknown rather than assigning a sentinel, and saves without inventing one.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_WithAnOutOfRangeScore_LeavesTheScoreUnknownAndOmitsItOnSave()
    {
        ApmlConcept concept = new();

        concept.Load(Navigator("""<Concept key="tech" value="1.5" />""")).ShouldBeTrue();

        concept.Key.ShouldBe("tech");
        concept.Value.ShouldBeNull();

        string xml = concept.ToString();
        xml.ShouldContain("key=\"tech\"", Case.Sensitive);
        xml.ShouldNotContain("value=", Case.Sensitive);
        xml.ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// The same for a source.
    /// </summary>
    [TestMethod]
    public void ApmlSource_WithAnOutOfRangeScore_LeavesTheScoreUnknownAndOmitsItOnSave()
    {
        ApmlSource source = new();

        source.Load(Navigator("""<Source key="feed" name="Feed" type="application/rss+xml" value="1.5" />""")).ShouldBeTrue();

        source.Key.ShouldBe("feed");
        source.Value.ShouldBeNull();

        string xml = source.ToString();
        xml.ShouldNotContain("value=", Case.Sensitive);
        xml.ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// The same for an author.
    /// </summary>
    [TestMethod]
    public void ApmlAuthor_WithAnOutOfRangeScore_LeavesTheScoreUnknownAndOmitsItOnSave()
    {
        ApmlAuthor author = new();

        author.Load(Navigator("""<Author key="ada" value="1.5" />""")).ShouldBeTrue();

        author.Key.ShouldBe("ada");
        author.Value.ShouldBeNull();

        string xml = author.ToString();
        xml.ShouldNotContain("value=", Case.Sensitive);
        xml.ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// A score inside the range is read, kept and written back at two decimal places, so the fix costs the
    /// conformant case nothing.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_WithAnInRangeScore_KeepsItAndWritesItBack()
    {
        ApmlConcept concept = new();

        concept.Load(Navigator("""<Concept key="tech" value="0.8" />""")).ShouldBeTrue();

        concept.Value.ShouldBe(0.8m);
        concept.ToString().ShouldContain("value=\"0.80\"", Case.Sensitive);
    }

    /// <summary>
    /// Zero is a real score, not an absent one, and survives the round trip as <c>0.00</c>.
    /// </summary>
    /// <remarks>
    ///     This is the assertion that makes the <see cref="decimal"/>? shape load-bearing. With a plain
    ///     <see cref="decimal"/> and a "write it if it is not zero" guard, a genuine neutral score would be
    ///     indistinguishable from an unassigned one and would vanish on save.
    /// </remarks>
    [TestMethod]
    public void ApmlConcept_WithAZeroScore_TreatsItAsARealScoreRatherThanAnAbsentOne()
    {
        ApmlConcept concept = new();

        concept.Load(Navigator("""<Concept key="tech" value="0" />""")).ShouldBeTrue();

        concept.Value.ShouldBe(0m);
        concept.ToString().ShouldContain("value=\"0.00\"", Case.Sensitive);
    }

    /// <summary>
    /// The setter still rejects an out-of-range score assigned programmatically: a loader skips an unusable
    /// value, a guard throws for a caller error.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_AssignedAnOutOfRangeScore_Throws()
    {
        ApmlConcept concept = new();

        Should.Throw<ArgumentOutOfRangeException>(() => concept.Value = 1.5m);
        Should.Throw<ArgumentOutOfRangeException>(() => concept.Value = -1.5m);
    }

    /// <summary>
    /// Assigning <see langword="null"/> is how a caller says "unknown" after the fact, and is not a guard
    /// violation.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_AssignedANullScore_ForgetsTheScore()
    {
        ApmlConcept concept = new("tech", 0.5m);

        concept.Value = null;

        concept.Value.ShouldBeNull();
        concept.ToString().ShouldNotContain("value=", Case.Sensitive);
    }

    /// <summary>
    /// Builds a navigator positioned on the root element of the supplied XML.
    /// </summary>
    /// <param name="xml">The element to parse.</param>
    /// <returns>A navigator standing on that element.</returns>
    private static XPathNavigator Navigator(string xml)
    {
        using StringReader reader = new(xml);
        XPathNavigator navigator = new XPathDocument(reader).CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();
        return navigator;
    }
}