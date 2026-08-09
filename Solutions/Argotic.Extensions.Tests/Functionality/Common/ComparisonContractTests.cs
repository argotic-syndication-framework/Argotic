namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Tests that the framework's ordering and equality implementations honour the contracts that
/// <see cref="List{T}.Sort()"/>, <see cref="HashSet{T}"/> and <see cref="Dictionary{TKey, TValue}"/> rely on.
/// </summary>
[TestClass]
public class ComparisonContractTests
{
    /// <summary>
    /// When two members disagree about the direction, the first differing one decides and the comparison
    /// stays antisymmetric: <c>x.CompareTo(y)</c> and <c>y.CompareTo(x)</c> have opposite signs.
    /// </summary>
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

    /// <summary>
    /// <see cref="List{T}.Sort()"/> orders the same pair by <c>Author</c>, the first differing member, rather
    /// than rejecting the comparer for returning inconsistent results.
    /// </summary>
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

    /// <summary>
    /// <c>CompareSequence</c> is antisymmetric too: the first differing element decides, and a later element
    /// comparing the other way does not make both sequences the lesser one.
    /// </summary>
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

    /// <summary>
    /// <c>Hello World</c> and <c>HELLO WORLD</c> are equal <c>AtomTextConstruct</c>s, and produce the same
    /// hash code.
    /// </summary>
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

    /// <summary>
    /// The consequence, through the type that depends on it: a <see cref="HashSet{T}"/> holding
    /// <c>Hello World</c> finds <c>HELLO WORLD</c>, so the two land in the same bucket.
    /// </summary>
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

    /// <summary>
    /// <c>HashCodeUtility.Component</c> folds a string case-insensitively, so <c>abc</c> and <c>ABC</c>
    /// contribute the same component.
    /// </summary>
    [TestMethod]
    public void HashCodeUtility_Component_DisregardsCaseForStrings() =>
        // Act & Assert
        HashCodeUtility.Component("abc").ShouldBe(HashCodeUtility.Component("ABC"));

    /// <summary>
    /// It does the same for a <see cref="Uri"/>, whose path is where <see cref="Uri.GetHashCode"/> is
    /// case-sensitive: <c>/Path</c> and <c>/path</c> contribute the same component.
    /// </summary>
    [TestMethod]
    public void HashCodeUtility_Component_DisregardsCaseForUris()
    {
        // Act & Assert
        HashCodeUtility.Component(new Uri("http://example.com/Path"))
            .ShouldBe(HashCodeUtility.Component(new Uri("http://example.com/path")));
    }

    /// <summary>
    /// A value that is neither a string nor a <see cref="Uri"/> is returned unchanged, so the helper is a
    /// pass-through everywhere case cannot arise.
    /// </summary>
    [TestMethod]
    public void HashCodeUtility_Component_ReturnsNonTextualValuesUnchanged()
    {
        // Act & Assert
        HashCodeUtility.Component(42).ShouldBe(42);
        HashCodeUtility.Component(true).ShouldBeTrue();
    }

    /// <summary>
    /// A <see langword="null"/> string or <see cref="Uri"/> contributes <c>0</c> rather than throwing, so an
    /// unset member does not make <c>GetHashCode</c> fail.
    /// </summary>
    [TestMethod]
    public void HashCodeUtility_Component_TreatsNullAsZero()
    {
        // Act & Assert
        HashCodeUtility.Component((string)null!).ShouldBe(0);
        HashCodeUtility.Component((Uri)null!).ShouldBe(0);
    }
}