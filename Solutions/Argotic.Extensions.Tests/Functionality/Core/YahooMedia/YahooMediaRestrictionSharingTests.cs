namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

/// <summary>
/// Covers the third <c>type</c> the maintained Media RSS specification permits on
/// <c>media:restriction</c>: <c>sharing</c>.
/// </summary>
/// <remarks>
///     <para>
///         The specification lists <c>country</c>, <c>uri</c> and <c>sharing</c>, the last of which means
///         "content cannot be shared — for example via embed tags". <see cref="YahooMediaRestrictionType"/>
///         modelled only the first two, so <c>RestrictionTypeByName("sharing")</c> returned
///         <see cref="YahooMediaRestrictionType.None"/>, the load guard skipped the assignment, and the
///         writer omitted the <c>type</c> attribute altogether.
///     </para>
///     <para>
///         That last step is what made it a defect rather than a gap: per the specification an absent
///         <c>type</c> means the entity list is one of the reserved <c>all</c>/<c>none</c> values, so a
///         sharing ban round-tripped as a blanket ban.
///     </para>
/// </remarks>
[TestClass]
public class YahooMediaRestrictionSharingTests
{
    /// <summary>
    /// <c>sharing</c> is recognised by name, alongside <c>country</c> and <c>uri</c>.
    /// </summary>
    [TestMethod]
    public void RestrictionTypeByName_Sharing_IsRecognised() =>
        YahooMediaRestriction.RestrictionTypeByName("sharing").ShouldBe(YahooMediaRestrictionType.Sharing);

    /// <summary>
    /// The name is matched without regard to case, as the other two are.
    /// </summary>
    [TestMethod]
    public void RestrictionTypeByName_Sharing_IsMatchedWithoutRegardToCase() =>
        YahooMediaRestriction.RestrictionTypeByName("SHARING").ShouldBe(YahooMediaRestrictionType.Sharing);

    /// <summary>
    /// The round trip back to the attribute value is <c>sharing</c>, lower case.
    /// </summary>
    [TestMethod]
    public void RestrictionTypeAsString_Sharing_IsTheAttributeSpelling() =>
        YahooMediaRestriction.RestrictionTypeAsString(YahooMediaRestrictionType.Sharing).ShouldBe("sharing");

    /// <summary>
    /// A sharing restriction read from a feed keeps its type, and writing it back restates
    /// <c>type="sharing"</c> rather than dropping the attribute and turning a sharing ban into a blanket one.
    /// </summary>
    [TestMethod]
    public void ASharingRestriction_KeepsItsTypeThroughARoundTrip()
    {
        YahooMediaRestriction restriction = LoadRestriction("""<restriction xmlns="http://search.yahoo.com/mrss/" relationship="deny" type="sharing">all</restriction>""");

        restriction.EntityType.ShouldBe(YahooMediaRestrictionType.Sharing);
        restriction.Relationship.ShouldBe(YahooMediaRestrictionRelationship.Deny);
        restriction.Entities.ShouldBe(["all"]);

        restriction.ToString().ShouldContain("type=\"sharing\"", Case.Sensitive);
    }

    /// <summary>
    /// Loads a <see cref="YahooMediaRestriction"/> from an XML fragment.
    /// </summary>
    /// <param name="xml">The fragment, rooted on the <c>restriction</c> element.</param>
    /// <returns>The loaded restriction.</returns>
    private static YahooMediaRestriction LoadRestriction(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        using XmlReader reader = XmlReader.Create(stream);
        XPathDocument document = new(reader);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToFirstChild();

        YahooMediaRestriction restriction = new();
        restriction.Load(navigator).ShouldBeTrue();
        return restriction;
    }
}