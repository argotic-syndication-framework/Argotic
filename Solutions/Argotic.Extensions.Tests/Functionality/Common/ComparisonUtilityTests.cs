namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers every <c>ComparisonUtility.CompareSequence</c> overload — days of the week, integers, longs,
/// strings, types, URIs, navigators and dictionaries — pinning the length-before-contents rule, the
/// per-element comparison each overload uses, and the null guards.
/// </summary>
[TestClass]
public class ComparisonUtilityTests
{
    #region CompareSequence<DayOfWeek> Tests

    /// <summary>
    /// Two day-of-week lists holding the same days in the same order compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A day-of-week list with more elements than the target returns <c>1</c>, whatever the elements hold.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A day-of-week list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// The first differing position decides the order and its sign is antisymmetric: <c>Friday</c>
    /// (<c>5</c>) is the greater enumeration value, so the list holding it is the greater sequence, and
    /// the same pair compared the other way round is the lesser.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceHoldsTheLaterDay_IsPositiveAndAntisymmetric()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Friday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target);
        int reverse = ComparisonUtility.CompareSequence(target, source);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    /// <summary>
    /// Two empty day-of-week lists compare equal rather than throwing.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenBothEmpty_ReturnsZero()
    {
        // Arrange
        IList<DayOfWeek> source = [];
        IList<DayOfWeek> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> source day-of-week list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<DayOfWeek> target = [DayOfWeek.Monday];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<DayOfWeek>)null!, target));
    }

    /// <summary>
    /// A <see langword="null"/> target day-of-week list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<DayOfWeek>)null!));
    }

    #endregion

    #region CompareSequence<int> Tests

    /// <summary>
    /// Two integer lists holding the same five values in the same order compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<int> source = [1, 2, 3, 4, 5];
        IList<int> target = [1, 2, 3, 4, 5];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// An integer list with more elements than the target returns <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<int> source = [1, 2, 3];
        IList<int> target = [1, 2];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// An integer list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<int> source = [1];
        IList<int> target = [1, 2, 3];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// The second element decides and the third never gets a say: <c>5</c> is greater than <c>2</c>, so
    /// the list holding it is the greater sequence, and the reverse comparison is the lesser.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenSourceHoldsTheLargerValue_IsPositiveAndAntisymmetric()
    {
        // Arrange
        IList<int> source = [1, 5, 3];
        IList<int> target = [1, 2, 3];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target);
        int reverse = ComparisonUtility.CompareSequence(target, source);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    /// <summary>
    /// Two empty integer lists compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenBothEmpty_ReturnsZero()
    {
        // Arrange
        IList<int> source = [];
        IList<int> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> source integer list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<int> target = [1, 2, 3];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<int>)null!, target));
    }

    /// <summary>
    /// A <see langword="null"/> target integer list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Int_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<int> source = [1, 2, 3];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<int>)null!));
    }

    #endregion

    #region CompareSequence<long> Tests

    /// <summary>
    /// Two long lists holding the same values in the same order compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Long_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<long> source = [100L, 200L, 300L];
        IList<long> target = [100L, 200L, 300L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A long list of three elements returns <c>1</c> against a target of one.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Long_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<long> source = [100L, 200L, 300L];
        IList<long> target = [100L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A long list of one element returns <c>-1</c> against a target of two.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Long_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<long> source = [100L];
        IList<long> target = [100L, 200L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// <c>999</c> is greater than <c>200</c>, so the list holding it is the greater sequence and the
    /// reverse comparison is the lesser.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Long_WhenSourceHoldsTheLargerValue_IsPositiveAndAntisymmetric()
    {
        // Arrange
        IList<long> source = [100L, 999L];
        IList<long> target = [100L, 200L];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target);
        int reverse = ComparisonUtility.CompareSequence(target, source);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    /// <summary>
    /// A <see langword="null"/> source long list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Long_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<long> target = [100L];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<long>)null!, target));
    }

    #endregion

    #region CompareSequence<string> Tests

    /// <summary>
    /// Two string lists holding the same words in the same order compare equal under
    /// <c>StringComparison.Ordinal</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<string> source = ["apple", "banana", "cherry"];
        IList<string> target = ["apple", "banana", "cherry"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A string list with more elements than the target returns <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<string> source = ["apple", "banana", "cherry"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A string list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<string> source = ["apple"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// Under <c>StringComparison.Ordinal</c> <c>zebra</c> sorts after <c>banana</c>, so the list holding
    /// it is the greater sequence and the reverse comparison is the lesser.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenSourceHoldsTheLaterWord_IsPositiveAndAntisymmetric()
    {
        // Arrange
        IList<string> source = ["apple", "zebra"];
        IList<string> target = ["apple", "banana"];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);
        int reverse = ComparisonUtility.CompareSequence(target, source, StringComparison.Ordinal);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    /// <summary>
    /// Strings differing only in case compare equal when <c>StringComparison.OrdinalIgnoreCase</c> is
    /// passed.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        IList<string> source = ["APPLE", "BANANA"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> source string list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<string> target = ["test"];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<string>)null!, target, StringComparison.Ordinal));
    }

    /// <summary>
    /// A <see langword="null"/> target string list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_String_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<string> source = ["test"];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<string>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region CompareSequence<Type> Tests

    /// <summary>
    /// Two type lists naming the same types in the same order compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(int), typeof(DateTime)];
        IList<Type> target = [typeof(string), typeof(int), typeof(DateTime)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A type list with more elements than the target returns <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(int), typeof(DateTime)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A type list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<Type> source = [typeof(string)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// The comparison runs over <see cref="Type.FullName"/> and not over the C# keyword, which is what
    /// fixes its direction: <c>System.Double</c> sorts before <c>System.Int32</c> ordinally, so the list
    /// holding <c>double</c> is the lesser sequence and the reverse comparison is the greater.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenSourceHoldsTheEarlierFullName_IsNegativeAndAntisymmetric()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(double)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target);
        int reverse = ComparisonUtility.CompareSequence(target, source);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A <see langword="null"/> source type list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Type> target = [typeof(string)];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<Type>)null!, target));
    }

    /// <summary>
    /// A <see langword="null"/> target type list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Type_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Type> source = [typeof(string)];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<Type>)null!));
    }

    #endregion

    #region CompareSequence<Uri> Tests

    /// <summary>
    /// Two URI lists holding the same absolute URIs in the same order compare equal.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://test.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A URI list with more elements than the target returns <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://test.com")];
        IList<Uri> target = [new Uri("http://example.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A URI list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// The second hosts decide: <c>http://different.com/</c> sorts before <c>http://test.com/</c>
    /// ordinally, so the list holding it is the lesser sequence and the reverse comparison is the
    /// greater.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenSourceHoldsTheEarlierHost_IsNegativeAndAntisymmetric()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://different.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);
        int reverse = ComparisonUtility.CompareSequence(target, source, StringComparison.Ordinal);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A URI written in upper case compares equal to its lower-case counterpart under
    /// <c>StringComparison.OrdinalIgnoreCase</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        IList<Uri> source = [new Uri("HTTP://EXAMPLE.COM")];
        IList<Uri> target = [new Uri("http://example.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> source URI list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Uri> target = [new Uri("http://example.com")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<Uri>)null!, target, StringComparison.Ordinal));
    }

    /// <summary>
    /// A <see langword="null"/> target URI list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Uri_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<Uri>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region CompareSequence<XPathNavigator> Tests

    /// <summary>
    /// Two navigator lists over the same markup compare equal, even though the navigators are
    /// distinct instances — the comparison reads their serialized XML, not their identity.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml2)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A navigator list with more elements than the target returns <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A navigator list with fewer elements than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml2)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// The second documents decide, on their serialized markup: <c>&lt;root&gt;different&lt;/root&gt;</c>
    /// sorts before <c>&lt;root&gt;value2&lt;/root&gt;</c> ordinally, so the list holding it is the lesser
    /// sequence and the reverse comparison is the greater.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceHoldsTheEarlierMarkup_IsNegativeAndAntisymmetric()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>different</root>";
        string xml3 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml3)];

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target);
        int reverse = ComparisonUtility.CompareSequence(target, source);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A <see langword="null"/> source navigator list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<XPathNavigator> target = [CreateNavigator("<root/>")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<XPathNavigator>)null!, target));
    }

    /// <summary>
    /// A <see langword="null"/> target navigator list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<XPathNavigator> source = [CreateNavigator("<root/>")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<XPathNavigator>)null!));
    }

    #endregion

    #region CompareSequence<Dictionary<string,string>> Tests

    /// <summary>
    /// Two dictionaries holding the same keys and the same values compare equal under
    /// <c>StringComparison.Ordinal</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenBothEqual_ReturnsZero()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A dictionary with more entries than the target returns <c>1</c> without comparing any value.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
            ["key3"] = "value3"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A dictionary with fewer entries than the target returns <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// Two single-entry dictionaries sharing a key are ordered by that key's value:
    /// <c>differentValue</c> sorts before <c>value1</c> ordinally, so the dictionary holding it is the
    /// lesser and the reverse comparison is the greater.
    /// </summary>
    /// <remarks>
    ///     The key is present in both, which is what makes this pair antisymmetric at all. A key the
    ///     other dictionary does not hold yields <c>-1</c> in whichever direction it is missing, and
    ///     <c>CompareSequence_Dictionary_WhenKeyNotFoundInTarget_ReturnsNegativeOne</c> pins that
    ///     deliberately asymmetric case separately.
    /// </remarks>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceHoldsTheEarlierValue_IsNegativeAndAntisymmetric()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "differentValue"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };

        // Act
        int forward = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);
        int reverse = ComparisonUtility.CompareSequence(target, source, StringComparison.Ordinal);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A key the target does not hold at all yields <c>-1</c>, so two same-sized dictionaries with
    /// disjoint keys make the source the lesser.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenKeyNotFoundInTarget_ReturnsNegativeOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };
        var target = new Dictionary<string, string>
        {
            ["differentKey"] = "value1"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// Values differing only in case compare equal when <c>StringComparison.OrdinalIgnoreCase</c> is
    /// passed.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "VALUE"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> source dictionary throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        var target = new Dictionary<string, string> { ["key"] = "value" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((Dictionary<string, string>)null!, target, StringComparison.Ordinal));
    }

    /// <summary>
    /// A <see langword="null"/> target dictionary throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CompareSequence_Dictionary_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new Dictionary<string, string> { ["key"] = "value" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (Dictionary<string, string>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region Helper Methods

    private static XPathNavigator CreateNavigator(string xml)
    {
        using StringReader reader = new(xml);
        XPathDocument doc = new(reader);
        return doc.CreateNavigator();
    }

    #endregion
}