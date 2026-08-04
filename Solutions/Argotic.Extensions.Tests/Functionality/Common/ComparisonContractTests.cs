using Argotic.Common;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Tests that the framework's ordering and equality implementations honour the contracts that
/// <see cref="List{T}.Sort()"/>, <see cref="HashSet{T}"/> and <see cref="Dictionary{TKey, TValue}"/> rely on.
/// </summary>
[TestClass]
public class ComparisonContractTests
{
    [TestMethod]
    public void CompareTo_WhenEarlierMemberIsGreaterAndLaterMemberIsLess_IsAntisymmetric()
    {
        // Arrange
        // The first member that differs decides the ordering; a later member comparing the other way
        // must not flip the result, or the comparer reports both operands as the lesser one.
        RssItem greater = new() { Author = "zoe@example.com", Description = "aaa", Title = "t" };
        RssItem lesser = new() { Author = "adam@example.com", Description = "zzz", Title = "t" };

        // Act
        int forward = greater.CompareTo(lesser);
        int reverse = lesser.CompareTo(greater);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    [TestMethod]
    public void Sort_WithMembersComparingInOppositeDirections_OrdersByFirstDifferingMember()
    {
        // Arrange
        RssItem greater = new() { Author = "zoe@example.com", Description = "aaa", Title = "t" };
        RssItem lesser = new() { Author = "adam@example.com", Description = "zzz", Title = "t" };
        List<RssItem> items = [greater, lesser];

        // Act
        items.Sort();

        // Assert
        items[0].Author.ShouldBe("adam@example.com");
        items[1].Author.ShouldBe("zoe@example.com");
    }

    [TestMethod]
    public void CompareSequence_WhenFirstElementIsGreaterAndSecondIsLess_IsAntisymmetric()
    {
        // Arrange
        List<string> first = ["zoe", "aaa"];
        List<string> second = ["adam", "zzz"];

        // Act
        int forward = ComparisonUtility.CompareSequence(first, second, StringComparison.OrdinalIgnoreCase);
        int reverse = ComparisonUtility.CompareSequence(second, first, StringComparison.OrdinalIgnoreCase);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    [TestMethod]
    public void GetHashCode_ForValuesThatCompareEqualIgnoringCase_IsEqual()
    {
        // Arrange
        // Equality is defined in terms of a case-insensitive comparison, so the hash code has to
        // disregard case as well or equal instances land in different hash buckets.
        AtomTextConstruct lower = new("Hello World");
        AtomTextConstruct upper = new("HELLO WORLD");

        // Act & Assert
        lower.Equals(upper).ShouldBeTrue();
        lower.GetHashCode().ShouldBe(upper.GetHashCode());
    }

    [TestMethod]
    public void HashSet_FindsInstanceThatDiffersOnlyByCase()
    {
        // Arrange
        AtomTextConstruct lower = new("Hello World");
        AtomTextConstruct upper = new("HELLO WORLD");
        HashSet<AtomTextConstruct> set = [lower];

        // Act & Assert
        set.Contains(upper).ShouldBeTrue();
    }

    [TestMethod]
    public void HashCodeUtility_Component_DisregardsCaseForStrings()
    {
        // Act & Assert
        HashCodeUtility.Component("abc").ShouldBe(HashCodeUtility.Component("ABC"));
    }

    [TestMethod]
    public void HashCodeUtility_Component_DisregardsCaseForUris()
    {
        // Act & Assert
        HashCodeUtility.Component(new Uri("http://example.com/Path"))
            .ShouldBe(HashCodeUtility.Component(new Uri("http://example.com/path")));
    }

    [TestMethod]
    public void HashCodeUtility_Component_ReturnsNonTextualValuesUnchanged()
    {
        // Act & Assert
        HashCodeUtility.Component(42).ShouldBe(42);
        HashCodeUtility.Component(true).ShouldBeTrue();
    }

    [TestMethod]
    public void HashCodeUtility_Component_TreatsNullAsZero()
    {
        // Act & Assert
        HashCodeUtility.Component((string)null).ShouldBe(0);
        HashCodeUtility.Component((Uri)null).ShouldBe(0);
    }
}