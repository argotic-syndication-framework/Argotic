namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers <see cref="SitemapUtility"/>: the change-frequency vocabulary in both directions, priority
/// parsing and its range check, the namespace every sitemap XPath query resolves against, and the
/// numbering of <see cref="SitemapChangeFrequency"/>.
/// </summary>
[TestClass]
public class SitemapUtilityTests
{
    #region ChangeFrequencyAsString Tests

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Always"/> writes as the token <c>always</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Always_ReturnsAlways()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Always);

        // Assert
        result.ShouldBe("always");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Hourly"/> writes as the token <c>hourly</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Hourly_ReturnsHourly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Hourly);

        // Assert
        result.ShouldBe("hourly");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Daily"/> writes as the token <c>daily</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Daily_ReturnsDaily()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Daily);

        // Assert
        result.ShouldBe("daily");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Weekly"/> writes as the token <c>weekly</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Weekly_ReturnsWeekly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Weekly);

        // Assert
        result.ShouldBe("weekly");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Monthly"/> writes as the token <c>monthly</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Monthly_ReturnsMonthly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Monthly);

        // Assert
        result.ShouldBe("monthly");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Yearly"/> writes as the token <c>yearly</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Yearly_ReturnsYearly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Yearly);

        // Assert
        result.ShouldBe("yearly");
    }

    /// <summary>
    /// <see cref="SitemapChangeFrequency.Never"/> writes as the token <c>never</c>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyAsString_Never_ReturnsNever()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Never);

        // Assert
        result.ShouldBe("never");
    }

    /// <summary>
    /// A value outside the defined set — only an out-of-range cast can produce one — writes as an
    /// <i>empty</i> string.
    /// </summary>
    /// <remarks>
    ///     The alternative would be to write the number, which would put a token in the document that the
    ///     protocol does not define and that no consumer could read back.
    /// </remarks>
    [TestMethod]
    public void ChangeFrequencyAsString_InvalidValue_ReturnsEmptyString()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString((SitemapChangeFrequency)99);

        // Assert
        result.ShouldBe(string.Empty);
    }

    #endregion

    #region ChangeFrequencyByName Tests

    /// <summary>
    /// The token <c>always</c> reads back as <see cref="SitemapChangeFrequency.Always"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Always_ReturnsAlways()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("always");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Always);
    }

    /// <summary>
    /// The token <c>hourly</c> reads back as <see cref="SitemapChangeFrequency.Hourly"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Hourly_ReturnsHourly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("hourly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Hourly);
    }

    /// <summary>
    /// The token <c>daily</c> reads back as <see cref="SitemapChangeFrequency.Daily"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Daily_ReturnsDaily()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("daily");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// The token <c>weekly</c> reads back as <see cref="SitemapChangeFrequency.Weekly"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Weekly_ReturnsWeekly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("weekly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Weekly);
    }

    /// <summary>
    /// The token <c>monthly</c> reads back as <see cref="SitemapChangeFrequency.Monthly"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Monthly_ReturnsMonthly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("monthly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Monthly);
    }

    /// <summary>
    /// The token <c>yearly</c> reads back as <see cref="SitemapChangeFrequency.Yearly"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Yearly_ReturnsYearly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("yearly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Yearly);
    }

    /// <summary>
    /// The token <c>never</c> reads back as <see cref="SitemapChangeFrequency.Never"/>.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_Never_ReturnsNever()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("never");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Never);
    }

    /// <summary>
    /// Casing is irrelevant: <c>ALWAYS</c>, <c>Daily</c> and <c>WEEKLY</c> all resolve.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_CaseInsensitive_ReturnsCorrectValue()
    {
        // Act & Assert
        SitemapUtility.ChangeFrequencyByName("ALWAYS").ShouldBe(SitemapChangeFrequency.Always);
        SitemapUtility.ChangeFrequencyByName("Daily").ShouldBe(SitemapChangeFrequency.Daily);
        SitemapUtility.ChangeFrequencyByName("WEEKLY").ShouldBe(SitemapChangeFrequency.Weekly);
    }

    /// <summary>
    /// Surrounding whitespace is trimmed before the token is matched.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_WithWhitespace_TrimsAndReturnsCorrectValue()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("  daily  ");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// An unrecognised token falls back to <see cref="SitemapChangeFrequency.Daily"/>.
    /// </summary>
    /// <remarks>
    ///     The fallback is indistinguishable from a genuine <c>daily</c>, which is why
    ///     <c>TryParseChangeFrequency</c> exists: this overload cannot tell a caller that the input was
    ///     not a protocol token.
    /// </remarks>
    [TestMethod]
    public void ChangeFrequencyByName_InvalidValue_ReturnsDaily()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("invalid");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// A <see langword="null"/> name is rejected rather than falling back; the guard is
    /// <c>ArgumentException.ThrowIfNullOrEmpty</c>, so what surfaces is its
    /// <see cref="ArgumentNullException"/> branch.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_NullValue_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => SitemapUtility.ChangeFrequencyByName(null!));

    /// <summary>
    /// An <i>empty</i> name throws <see cref="ArgumentException"/> rather than falling back.
    /// </summary>
    [TestMethod]
    public void ChangeFrequencyByName_EmptyValue_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => SitemapUtility.ChangeFrequencyByName(string.Empty));

    #endregion

    #region TryParseChangeFrequency Tests

    /// <summary>
    /// <c>daily</c> parses, reporting <see langword="true"/> and yielding
    /// <see cref="SitemapChangeFrequency.Daily"/>.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_ValidValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("daily", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeTrue();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// All seven tokens the protocol defines parse to their matching member.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_AllValidValues_ReturnsTrueForEach()
    {
        // Arrange
        var testCases = new[]
        {
            ("always", SitemapChangeFrequency.Always),
            ("hourly", SitemapChangeFrequency.Hourly),
            ("daily", SitemapChangeFrequency.Daily),
            ("weekly", SitemapChangeFrequency.Weekly),
            ("monthly", SitemapChangeFrequency.Monthly),
            ("yearly", SitemapChangeFrequency.Yearly),
            ("never", SitemapChangeFrequency.Never)
        };

        foreach (var (input, expected) in testCases)
        {
            // Act
            bool result = SitemapUtility.TryParseChangeFrequency(input, out SitemapChangeFrequency frequency);

            // Assert
            result.ShouldBeTrue($"Failed for input: {input}");
            frequency.ShouldBe(expected, $"Failed for input: {input}");
        }
    }

    /// <summary>
    /// A <see langword="null"/> token reports <see langword="false"/> and leaves the out parameter at
    /// <see cref="SitemapChangeFrequency.Daily"/>, the documented failure value.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_NullValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(null!, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// An <i>empty</i> token reports <see langword="false"/> and leaves the same <c>daily</c> failure value.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_EmptyValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(string.Empty, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// <c>biweekly</c> is not a protocol token: it reports <see langword="false"/> and leaves the
    /// <c>daily</c> failure value.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_InvalidValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("biweekly", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    /// <summary>
    /// <c>MONTHLY</c> in upper case parses to <see cref="SitemapChangeFrequency.Monthly"/>.
    /// </summary>
    [TestMethod]
    public void TryParseChangeFrequency_CaseInsensitive_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("MONTHLY", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeTrue();
        frequency.ShouldBe(SitemapChangeFrequency.Monthly);
    }

    #endregion

    #region TryParsePriority Tests

    /// <summary>
    /// <c>0.8</c> parses to <c>0.8</c>.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_ValidValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0.8", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0.8m);
    }

    /// <summary>
    /// <c>0.0</c> parses; the low end of the permitted range is inclusive.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_ZeroValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0.0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0.0m);
    }

    /// <summary>
    /// <c>1.0</c> parses; the high end of the permitted range is inclusive.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_OneValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1.0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(1.0m);
    }

    /// <summary>
    /// <c>0</c> written without a decimal point parses to zero.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_MinimumBoundary_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0m);
    }

    /// <summary>
    /// <c>1</c> written without a decimal point parses to one.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_MaximumBoundary_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(1m);
    }

    /// <summary>
    /// <c>1.1</c> is rejected, and the out parameter is left at <c>0.5</c> rather than at the number
    /// that was read.
    /// </summary>
    /// <remarks>
    ///     <c>0.5</c> is the priority the protocol assigns a page that declares none, so a caller that
    ///     ignores the boolean still writes a legal document — which is exactly why the boolean has to be
    ///     read.
    /// </remarks>
    [TestMethod]
    public void TryParsePriority_ValueAboveOne_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1.1", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    /// <summary>
    /// <c>-0.1</c> is rejected and leaves the <c>0.5</c> failure value.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_NegativeValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("-0.1", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    /// <summary>
    /// A <see langword="null"/> string is rejected and leaves the <c>0.5</c> failure value rather than
    /// throwing.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_NullValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority(null!, out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    /// <summary>
    /// An <i>empty</i> string is rejected and leaves the <c>0.5</c> failure value.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_EmptyValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority(string.Empty, out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    /// <summary>
    /// <c>high</c> is not a number: it is rejected and leaves the <c>0.5</c> failure value.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_NonNumericValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("high", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    /// <summary>
    /// Every tenth from <c>0.1</c> to <c>0.9</c> parses to itself.
    /// </summary>
    [TestMethod]
    public void TryParsePriority_ValidDecimalValues_ParseCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("0.1", 0.1m),
            ("0.2", 0.2m),
            ("0.3", 0.3m),
            ("0.4", 0.4m),
            ("0.5", 0.5m),
            ("0.6", 0.6m),
            ("0.7", 0.7m),
            ("0.8", 0.8m),
            ("0.9", 0.9m)
        };

        foreach (var (input, expected) in testCases)
        {
            // Act
            bool result = SitemapUtility.TryParsePriority(input, out decimal priority);

            // Assert
            result.ShouldBeTrue($"Failed for input: {input}");
            priority.ShouldBe(expected, $"Failed for input: {input}");
        }
    }

    #endregion

    #region CreateNamespaceManager Tests

    /// <summary>
    /// The manager binds the prefix <c>sm</c> to the sitemap namespace, which is what every <c>sm:</c>
    /// XPath query in this suite resolves against.
    /// </summary>
    [TestMethod]
    public void CreateNamespaceManager_ValidNameTable_ReturnsManagerWithSitemapNamespace()
    {
        // Arrange
        NameTable nameTable = new();

        // Act
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(nameTable);

        // Assert
        manager.ShouldNotBeNull();
        manager.LookupNamespace("sm").ShouldBe(SitemapUtility.SitemapNamespace);
    }

    /// <summary>
    /// A <see langword="null"/> name table throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CreateNamespaceManager_NullNameTable_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SitemapUtility.CreateNamespaceManager(null!));

    /// <summary>
    /// The namespace is exactly <c>http://www.sitemaps.org/schemas/sitemap/0.9</c>.
    /// </summary>
    /// <remarks>
    ///     0.9 is the only version the protocol has ever had; the number in the URI is part of the fixed
    ///     namespace name rather than a version to negotiate. Changing a character of it would stop every
    ///     sitemap parsing, silently.
    /// </remarks>
    [TestMethod]
    public void SitemapNamespace_ReturnsCorrectValue() =>
        // Assert
        SitemapUtility.SitemapNamespace.ShouldBe("http://www.sitemaps.org/schemas/sitemap/0.9");

    #endregion

    #region SitemapChangeFrequency Enum Tests

    /// <summary>
    /// The seven members are numbered <c>0</c> through <c>6</c>, from <c>Always</c> to <c>Never</c>.
    /// </summary>
    [TestMethod]
    public void SitemapChangeFrequency_HasExpectedValues()
    {
        // Assert
        ((int)SitemapChangeFrequency.Always).ShouldBe(0);
        ((int)SitemapChangeFrequency.Hourly).ShouldBe(1);
        ((int)SitemapChangeFrequency.Daily).ShouldBe(2);
        ((int)SitemapChangeFrequency.Weekly).ShouldBe(3);
        ((int)SitemapChangeFrequency.Monthly).ShouldBe(4);
        ((int)SitemapChangeFrequency.Yearly).ShouldBe(5);
        ((int)SitemapChangeFrequency.Never).ShouldBe(6);
    }

    /// <summary>
    /// Every member writes to a token that parses back to the same member, so no value is unwritable or
    /// unreadable.
    /// </summary>
    [TestMethod]
    public void SitemapChangeFrequency_AllValuesCanRoundTrip()
    {
        // Arrange
        var values = Enum.GetValues<SitemapChangeFrequency>();

        foreach (var frequency in values)
        {
            // Act
            string asString = SitemapUtility.ChangeFrequencyAsString(frequency);
            bool parsed = SitemapUtility.TryParseChangeFrequency(asString, out SitemapChangeFrequency result);

            // Assert
            parsed.ShouldBeTrue($"Failed for value: {frequency}");
            result.ShouldBe(frequency, $"Round-trip failed for value: {frequency}");
        }
    }

    #endregion

    #region Case folding

    /// <summary>
    /// The change frequency token is folded with invariant culture rules, not ordinal ones.
    /// </summary>
    /// <remarks>
    ///     Pinned because it is the difference between the implementation this method has and the
    ///     faster one it does not. Comparing spans under
    ///     <see cref="StringComparison.OrdinalIgnoreCase"/> would remove the last allocation from this
    ///     method — and would stop accepting this input, because ordinal comparison does not fold the
    ///     KELVIN SIGN to <c>k</c> while <see cref="string.ToLowerInvariant"/> does.
    ///     <para>
    ///     No real sitemap spells it this way. The point is that the decision to keep invariant folding
    ///     was deliberate and measured, so a later change that narrows what parses has to fail a test
    ///     rather than pass unnoticed.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TryParseChangeFrequency_FoldsWithInvariantCultureRulesNotOrdinal()
    {
        // Arrange - "weekly" with a KELVIN SIGN (U+212A) in place of the letter k.
        const string KelvinSpelling = "weeKly";

        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(KelvinSpelling, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeTrue("ToLowerInvariant folds U+212A to 'k'; an ordinal comparison would not");
        frequency.ShouldBe(SitemapChangeFrequency.Weekly);
    }

    /// <summary>
    /// Surrounding whitespace is trimmed, and casing is irrelevant.
    /// </summary>
    /// <param name="value">One of the protocol's seven change-frequency tokens, in any casing and with any surrounding whitespace.</param>
    /// <param name="expected">The <see cref="SitemapChangeFrequency"/> member <paramref name="value"/> names.</param>
    [TestMethod]
    [DataRow("  daily  ", SitemapChangeFrequency.Daily)]
    [DataRow("WEEKLY", SitemapChangeFrequency.Weekly)]
    [DataRow("mOnThLy", SitemapChangeFrequency.Monthly)]
    [DataRow("never\t", SitemapChangeFrequency.Never)]
    public void TryParseChangeFrequency_IgnoresSurroundingWhitespaceAndCasing(string value, SitemapChangeFrequency expected)
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(value, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeTrue();
        frequency.ShouldBe(expected);
    }

    /// <summary>
    /// An unrecognised token reports failure while still yielding the documented default.
    /// </summary>
    /// <remarks>
    ///     The pairing matters: <c>daily</c> is both a valid token and the fallback, so asserting the
    ///     out parameter alone cannot distinguish "parsed as daily" from "gave up and said daily".
    ///     The boolean is the only thing that can, which is what the rewritten switch had to preserve.
    /// </remarks>
    [TestMethod]
    public void TryParseChangeFrequency_UnrecognisedToken_ReportsFailureAndDefaultsToDaily()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("fortnightly", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    #endregion
}