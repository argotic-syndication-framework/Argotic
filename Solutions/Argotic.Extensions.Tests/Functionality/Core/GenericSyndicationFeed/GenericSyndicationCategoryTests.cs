namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

/// <summary>
/// Covers the term-and-scheme abstraction a category presents once an Atom <c>category</c> or an RSS
/// <c>category</c> has been folded into it, together with its guards, comparison and equality contracts.
/// </summary>
[TestClass]
public class GenericSyndicationCategoryTests
{
    #region Constructor Tests - String Term

    /// <summary>
    /// A category built from a term alone keeps that term verbatim.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTerm_SetsTermProperty()
    {
        // Arrange
        string term = "technology";

        // Act
        var category = new GenericSyndicationCategory(term);

        // Assert
        category.Term.ShouldBe(term);
    }

    /// <summary>
    /// A category built from a term alone has an empty scheme, not a <see langword="null"/> one.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTerm_SchemeShouldBeEmpty()
    {
        // Arrange
        string term = "technology";

        // Act
        var category = new GenericSyndicationCategory(term);

        // Assert
        category.Scheme.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A <see langword="null"/> term is refused with <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullTerm_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationCategory((string)null!));

    /// <summary>
    /// An empty term is refused with <see cref="ArgumentException"/>, which is the other half of the
    /// pair <c>ArgumentException.ThrowIfNullOrEmpty</c> throws.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyTerm_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GenericSyndicationCategory(string.Empty));

    #endregion

    #region Constructor Tests - Term and Scheme

    /// <summary>
    /// A term and a scheme supplied together are both kept as given.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTermAndScheme_SetsBothProperties()
    {
        // Arrange
        string term = "technology";
        string scheme = "http://example.com/categories";

        // Act
        var category = new GenericSyndicationCategory(term, scheme);

        // Assert
        category.Term.ShouldBe(term);
        category.Scheme.ShouldBe(scheme);
    }

    /// <summary>
    /// A scheme passed explicitly as <see langword="null"/> is stored as <see langword="null"/> rather
    /// than being normalised to the empty string the one-argument constructor leaves behind.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTermAndNullScheme_SetsSchemeToNull()
    {
        // Arrange
        string term = "technology";

        // Act
        var category = new GenericSyndicationCategory(term, null!);

        // Assert
        category.Term.ShouldBe(term);
        category.Scheme.ShouldBeNull();
    }

    #endregion

    #region Constructor Tests - AtomCategory

    /// <summary>
    /// An Atom category's <c>term</c> attribute becomes the generic term.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_SetsTermFromAtomTerm()
    {
        // Arrange
        var atomCategory = new AtomCategory("atomTerm");

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert
        category.Term.ShouldBe("atomTerm");
    }

    /// <summary>
    /// An Atom category's <c>scheme</c> IRI becomes the generic scheme, flattened to its string form.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_SetsSchemeFromAtomScheme()
    {
        // Arrange
        var atomCategory = new AtomCategory("atomTerm")
        {
            Scheme = new Uri("http://example.com/scheme")
        };

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert
        category.Scheme.ShouldBe("http://example.com/scheme");
    }

    /// <summary>
    /// Whitespace around an Atom term is trimmed off on the way in.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_TrimsWhitespace()
    {
        // Arrange
        var atomCategory = new AtomCategory("  atomTerm  ");

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert
        category.Term.ShouldBe("atomTerm");
    }

    /// <summary>
    /// An Atom category carrying only a <c>label</c> yields that label as its term.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_FallsBackToLabelWhenTermNotSet()
    {
        // Arrange - AtomCategory created without a term, only a label
        var atomCategory = new AtomCategory
        {
            Label = "labelValue"
        };

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert - When Term is empty, it falls back to Label
        category.Term.ShouldBe("labelValue");
    }

    /// <summary>
    /// When an Atom category carries both, the <c>term</c> wins and the <c>label</c> is ignored.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_PrefersTermOverLabel()
    {
        // Arrange
        var atomCategory = new AtomCategory("termValue")
        {
            Label = "labelValue"
        };

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert
        category.Term.ShouldBe("termValue");
    }

    /// <summary>
    /// A <see langword="null"/> Atom category is refused rather than producing an empty abstraction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAtomCategory_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationCategory((AtomCategory)null!));

    /// <summary>
    /// An Atom category with no <c>scheme</c> yields an empty scheme, not a <see langword="null"/> one.
    /// </summary>
    [TestMethod]
    public void Constructor_WithAtomCategory_NoScheme_SetsSchemeToEmpty()
    {
        // Arrange
        var atomCategory = new AtomCategory("term");

        // Act
        var category = new GenericSyndicationCategory(atomCategory);

        // Assert
        category.Scheme.ShouldBe(string.Empty);
    }

    #endregion

    #region Constructor Tests - RssCategory

    /// <summary>
    /// An RSS category's element content becomes the generic term.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRssCategory_SetsTermFromValue()
    {
        // Arrange
        var rssCategory = new RssCategory("rssValue");

        // Act
        var category = new GenericSyndicationCategory(rssCategory);

        // Assert
        category.Term.ShouldBe("rssValue");
    }

    /// <summary>
    /// An RSS category's <c>domain</c> attribute becomes the generic scheme, which is what makes an
    /// RSS category and an Atom category comparable through one abstraction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRssCategory_SetsSchemeFromDomain()
    {
        // Arrange
        var rssCategory = new RssCategory("rssValue", "http://example.com/domain");

        // Act
        var category = new GenericSyndicationCategory(rssCategory);

        // Assert
        category.Scheme.ShouldBe("http://example.com/domain");
    }

    /// <summary>
    /// Whitespace around an RSS category's value and domain is trimmed off both.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRssCategory_TrimsWhitespace()
    {
        // Arrange
        var rssCategory = new RssCategory("  rssValue  ", "  domain  ");

        // Act
        var category = new GenericSyndicationCategory(rssCategory);

        // Assert
        category.Term.ShouldBe("rssValue");
        category.Scheme.ShouldBe("domain");
    }

    /// <summary>
    /// A <see langword="null"/> RSS category is refused rather than producing an empty abstraction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullRssCategory_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationCategory((RssCategory)null!));

    /// <summary>
    /// An RSS category with no <c>domain</c> yields an empty scheme, matching the Atom side.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRssCategory_NoDomain_SetsSchemeToEmpty()
    {
        // Arrange
        var rssCategory = new RssCategory("value");

        // Act
        var category = new GenericSyndicationCategory(rssCategory);

        // Assert
        category.Scheme.ShouldBe(string.Empty);
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// The string form names the type and spells out both the term and the scheme, so a category is
    /// legible in a debugger and in an assertion failure.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var category = new GenericSyndicationCategory("tech", "http://example.com");

        // Act
        string result = category.ToString();

        // Assert
        result.ShouldContain("GenericSyndicationCategory");
        result.ShouldContain("Term = tech");
        result.ShouldContain("Scheme = http://example.com");
    }

    #endregion

    #region CompareTo Tests

    /// <summary>
    /// Comparing against <see langword="null"/> yields <c>1</c>, so a null sorts before every category.
    /// </summary>
    [TestMethod]
    public void CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var category = new GenericSyndicationCategory("term");

        // Act
        int result = category.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// Two categories with the same term and scheme compare equal, so ordering agrees with equality.
    /// </summary>
    [TestMethod]
    public void CompareTo_WithEqualCategory_ReturnsZero()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term", "scheme");
        var category2 = new GenericSyndicationCategory("term", "scheme");

        // Act
        int result = category1.CompareTo(category2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// The term decides the ordering once the schemes agree, and it decides it in one direction only:
    /// <c>apple</c> sorts before <c>banana</c>, and the reverse comparison reports the opposite sign.
    /// </summary>
    /// <remarks>
    ///     The schemes are identical, so <c>CompareTo</c> falls through to comparing the terms with
    ///     <see cref="StringComparison.OrdinalIgnoreCase"/>, under which <c>apple</c> is the lesser.
    /// </remarks>
    [TestMethod]
    public void CompareTo_WithDifferentTerm_OrdersByTermAndIsAntisymmetric()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("apple", "scheme");
        var category2 = new GenericSyndicationCategory("banana", "scheme");

        // Act
        int forward = category1.CompareTo(category2);
        int reverse = category2.CompareTo(category1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// The scheme is consulted before the term, so the same term under <c>schemeA</c> sorts before the
    /// same term under <c>schemeB</c>, and the reverse comparison reports the opposite sign.
    /// </summary>
    /// <remarks>
    ///     The schemes are compared with <see cref="StringComparison.Ordinal"/>, under which <c>A</c>
    ///     precedes <c>B</c>.
    /// </remarks>
    [TestMethod]
    public void CompareTo_WithDifferentScheme_OrdersBySchemeAndIsAntisymmetric()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term", "schemeA");
        var category2 = new GenericSyndicationCategory("term", "schemeB");

        // Act
        int forward = category1.CompareTo(category2);
        int reverse = category2.CompareTo(category1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Equals Tests

    /// <summary>
    /// Equality is by value: two separately constructed categories with the same term and scheme are equal.
    /// </summary>
    [TestMethod]
    public void Equals_WithEqualCategory_ReturnsTrue()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term", "scheme");
        var category2 = new GenericSyndicationCategory("term", "scheme");

        // Act & Assert
        category1.Equals(category2).ShouldBeTrue();
    }

    /// <summary>
    /// Categories differing only in their term are not equal.
    /// </summary>
    [TestMethod]
    public void Equals_WithDifferentCategory_ReturnsFalse()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term1", "scheme");
        var category2 = new GenericSyndicationCategory("term2", "scheme");

        // Act & Assert
        category1.Equals(category2).ShouldBeFalse();
    }

    /// <summary>
    /// Nothing equals <see langword="null"/>, and asking does not throw.
    /// </summary>
    [TestMethod]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var category = new GenericSyndicationCategory("term");

        // Act & Assert
        category.Equals(null).ShouldBeFalse();
    }

    /// <summary>
    /// An object of an unrelated type is not equal to a category, and asking does not throw.
    /// </summary>
    [TestMethod]
    public void Equals_WithWrongType_ReturnsFalse()
    {
        // Arrange
        var category = new GenericSyndicationCategory("term");

        // Act & Assert
        category.Equals("string").ShouldBeFalse();
    }

    #endregion

    #region GetHashCode Tests

    /// <summary>
    /// Two distinct categories that compare equal hash equally, and one category hashes the same every
    /// time it is asked — the contract <see cref="HashSet{T}"/> and <see cref="Dictionary{TKey, TValue}"/>
    /// rely on to find a category they have already stored.
    /// </summary>
    [TestMethod]
    public void GetHashCode_ForEqualCategories_MatchesAndIsStable()
    {
        // Arrange
        var first = new GenericSyndicationCategory("term", "scheme");
        var second = new GenericSyndicationCategory("term", "scheme");

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    #endregion

    #region Equality Operator Tests

    /// <summary>
    /// The <c>==</c> operator agrees with <c>Equals</c> for two equal categories.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithEqualCategories_ReturnsTrue()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term", "scheme");
        var category2 = new GenericSyndicationCategory("term", "scheme");

        // Act & Assert
        (category1 == category2).ShouldBeTrue();
    }

    /// <summary>
    /// The <c>==</c> operator separates two categories with different terms.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithDifferentCategories_ReturnsFalse()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term1");
        var category2 = new GenericSyndicationCategory("term2");

        // Act & Assert
        (category1 == category2).ShouldBeFalse();
    }

    /// <summary>
    /// Two <see langword="null"/> references compare equal instead of dereferencing either one.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        GenericSyndicationCategory? category1 = null;
        GenericSyndicationCategory? category2 = null;

        // Act & Assert
        (category1 == category2).ShouldBeTrue();
    }

    /// <summary>
    /// A <see langword="null"/> left operand is unequal to a real category rather than throwing.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithFirstNull_ReturnsFalse()
    {
        // Arrange
        GenericSyndicationCategory? category1 = null;
        var category2 = new GenericSyndicationCategory("term");

        // Act & Assert
        (category1 == category2).ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> right operand is unequal to a real category, symmetrically.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithSecondNull_ReturnsFalse()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term");
        GenericSyndicationCategory? category2 = null;

        // Act & Assert
        (category1 == category2).ShouldBeFalse();
    }

    #endregion

    #region Inequality Operator Tests

    /// <summary>
    /// The <c>!=</c> operator is the negation of <c>==</c> for two equal categories.
    /// </summary>
    [TestMethod]
    public void InequalityOperator_WithEqualCategories_ReturnsFalse()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term", "scheme");
        var category2 = new GenericSyndicationCategory("term", "scheme");

        // Act & Assert
        (category1 != category2).ShouldBeFalse();
    }

    /// <summary>
    /// The <c>!=</c> operator is the negation of <c>==</c> for two differing categories.
    /// </summary>
    [TestMethod]
    public void InequalityOperator_WithDifferentCategories_ReturnsTrue()
    {
        // Arrange
        var category1 = new GenericSyndicationCategory("term1");
        var category2 = new GenericSyndicationCategory("term2");

        // Act & Assert
        (category1 != category2).ShouldBeTrue();
    }

    #endregion
}