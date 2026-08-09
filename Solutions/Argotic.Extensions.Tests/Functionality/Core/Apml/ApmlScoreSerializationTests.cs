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
///     is omitted when it is null.
///     </para>
///     <para>
///     <b>The APML 0.6 schema is reachable and says <c>value</c> is required.</b> It is published at
///     <c>github.com/apml/spec-0.6</c> as <c>apml.xsd</c>, target namespace <c>http://www.apml.org/apml-0.6</c>
///     — the same one this library writes — and it declares <c>key</c> and <c>value</c> as
///     <c>use="required"</c> on <c>ExplicitNodeType</c>, plus <c>name</c> and <c>type</c> as
///     <c>use="required"</c> on both source types. Validating candidate documents against it settles each
///     attribute separately, and the answers do not agree with one another:
///     </para>
///     <list type="bullet">
///         <item><description><c>key=""</c> and <c>name=""</c> are <b>valid</b>; omitting either is <b>invalid</b>. Empty is the conformant spelling of "unset", so both are written unconditionally.</description></item>
///         <item><description><c>type=""</c> is <b>invalid</b> (the <c>apml:MimeType</c> pattern <c>[^/]+/[^/]+</c> fails) and omitting <c>type</c> is <b>invalid</b> too. Neither spelling conforms.</description></item>
///         <item><description>The old <c>value</c> sentinel is <b>invalid</b> (it fails <c>NodeValueType</c>'s <c>minInclusive</c>) and omitting <c>value</c> is <b>invalid</b>. <c>0.00</c> is the only valid spelling, and it is a lie.</description></item>
///     </list>
///     <para>
///     So omission is a <i>known</i> deviation, not a conservative reading of an unknown: a score that was
///     never established is not representable in conformant APML, and the choice is only which
///     non-conformance to prefer. Omission wins because it is the one a consumer can recognise as missing —
///     the sentinel asserts a score twenty-nine orders of magnitude out of range, and <c>0.00</c> fabricates
///     a neutral-interest claim the profile never made. In an attention-profiling format the score is the
///     entire payload, so fabricating it is the one option that corrupts data rather than merely omitting it.
///     </para>
/// </remarks>
[TestClass]
public class ApmlScoreSerializationTests
{
    /// <summary>
    /// A concept that was never given a score omits the <c>value</c> attribute rather than emitting a
    /// sentinel, but still writes <c>key</c> — empty — because the schema requires the attribute to be
    /// present and permits it to be empty.
    /// </summary>
    /// <remarks>
    ///     <c>key</c> and <c>value</c> pull in opposite directions and it is the schema, not symmetry, that
    ///     decides each. Both are <c>use="required"</c> on <c>ExplicitNodeType</c>, but <c>key</c> is a plain
    ///     <c>xs:string</c>, for which the empty string is a legal value, whereas <c>value</c> is
    ///     <c>NodeValueType</c> — <c>xs:decimal</c> restricted to <c>[-1, 1]</c> — for which no "absent"
    ///     spelling exists at all. So <c>key=""</c> is valid and omitting <c>key</c> is not, while every
    ///     available spelling of an unknown <c>value</c> is invalid and the choice is only which invalidity
    ///     to prefer.
    /// </remarks>
    [TestMethod]
    public void ApmlConcept_WithNoScoreAssigned_OmitsTheValueButStillWritesTheRequiredKey()
    {
        ApmlConcept concept = new();

        concept.Value.ShouldBeNull();

        string xml = concept.ToString();

        xml.ShouldNotContain("value=", Case.Sensitive);
        xml.ShouldContain("key=\"\"", Case.Sensitive);
        xml.ShouldNotContain("79228162514264337593543950335", Case.Sensitive);
    }

    /// <summary>
    /// A source that was never populated still writes <c>key</c>, <c>name</c> and <c>type</c>, all three of
    /// which the schema declares required.
    /// </summary>
    /// <remarks>
    ///     <c>type</c> is the one attribute in this family that has no conformant spelling when unset:
    ///     <c>apml:MimeType</c> restricts it to the pattern <c>[^/]+/[^/]+</c>, which the empty string fails,
    ///     and omitting a required attribute fails too. A source with no MIME type is simply not
    ///     representable in conformant APML, so the write stays unconditional and consistent with its two
    ///     neighbours rather than inventing a third behaviour for a case that cannot be made valid either
    ///     way.
    /// </remarks>
    [TestMethod]
    public void ApmlSource_WithNothingPopulated_StillWritesTheThreeRequiredAttributes()
    {
        ApmlSource source = new();

        string xml = source.ToString();

        xml.ShouldContain("key=\"\"", Case.Sensitive);
        xml.ShouldContain("name=\"\"", Case.Sensitive);
        xml.ShouldContain("type=\"\"", Case.Sensitive);
    }

    /// <summary>
    /// An author that was never populated still writes its required <c>key</c>.
    /// </summary>
    [TestMethod]
    public void ApmlAuthor_WithNothingPopulated_StillWritesTheRequiredKey()
    {
        ApmlAuthor author = new();

        author.ToString().ShouldContain("key=\"\"", Case.Sensitive);
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
    /// Assigning <see langword="null"/> does not erase the key: the two attributes are independent, and
    /// only <c>value</c> has an "unknown" spelling.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_WithAKeyButNoScore_KeepsTheKeyAndDropsOnlyTheScore()
    {
        ApmlConcept concept = new("tech", 0.5m) { Value = null };

        string xml = concept.ToString();
        xml.ShouldContain("key=\"tech\"", Case.Sensitive);
        xml.ShouldNotContain("value=", Case.Sensitive);
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