using Argotic.Syndication;
using Shouldly;
using System.Xml;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for <see cref="SitemapUtility"/>.
/// </summary>
[TestClass]
public class SitemapUtilityTests
{
    #region ChangeFrequencyAsString Tests

    [TestMethod]
    public void ChangeFrequencyAsString_Always_ReturnsAlways()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Always);

        // Assert
        result.ShouldBe("always");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Hourly_ReturnsHourly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Hourly);

        // Assert
        result.ShouldBe("hourly");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Daily_ReturnsDaily()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Daily);

        // Assert
        result.ShouldBe("daily");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Weekly_ReturnsWeekly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Weekly);

        // Assert
        result.ShouldBe("weekly");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Monthly_ReturnsMonthly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Monthly);

        // Assert
        result.ShouldBe("monthly");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Yearly_ReturnsYearly()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Yearly);

        // Assert
        result.ShouldBe("yearly");
    }

    [TestMethod]
    public void ChangeFrequencyAsString_Never_ReturnsNever()
    {
        // Act
        string result = SitemapUtility.ChangeFrequencyAsString(SitemapChangeFrequency.Never);

        // Assert
        result.ShouldBe("never");
    }

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

    [TestMethod]
    public void ChangeFrequencyByName_Always_ReturnsAlways()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("always");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Always);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Hourly_ReturnsHourly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("hourly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Hourly);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Daily_ReturnsDaily()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("daily");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Weekly_ReturnsWeekly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("weekly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Weekly);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Monthly_ReturnsMonthly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("monthly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Monthly);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Yearly_ReturnsYearly()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("yearly");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Yearly);
    }

    [TestMethod]
    public void ChangeFrequencyByName_Never_ReturnsNever()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("never");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Never);
    }

    [TestMethod]
    public void ChangeFrequencyByName_CaseInsensitive_ReturnsCorrectValue()
    {
        // Act & Assert
        SitemapUtility.ChangeFrequencyByName("ALWAYS").ShouldBe(SitemapChangeFrequency.Always);
        SitemapUtility.ChangeFrequencyByName("Daily").ShouldBe(SitemapChangeFrequency.Daily);
        SitemapUtility.ChangeFrequencyByName("WEEKLY").ShouldBe(SitemapChangeFrequency.Weekly);
    }

    [TestMethod]
    public void ChangeFrequencyByName_WithWhitespace_TrimsAndReturnsCorrectValue()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("  daily  ");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    [TestMethod]
    public void ChangeFrequencyByName_InvalidValue_ReturnsDaily()
    {
        // Act
        SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName("invalid");

        // Assert
        result.ShouldBe(SitemapChangeFrequency.Daily);
    }

    [TestMethod]
    public void ChangeFrequencyByName_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => SitemapUtility.ChangeFrequencyByName(null!));
    }

    [TestMethod]
    public void ChangeFrequencyByName_EmptyValue_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => SitemapUtility.ChangeFrequencyByName(string.Empty));
    }

    #endregion

    #region TryParseChangeFrequency Tests

    [TestMethod]
    public void TryParseChangeFrequency_ValidValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("daily", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeTrue();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

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

    [TestMethod]
    public void TryParseChangeFrequency_NullValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(null!, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    [TestMethod]
    public void TryParseChangeFrequency_EmptyValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency(string.Empty, out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

    [TestMethod]
    public void TryParseChangeFrequency_InvalidValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParseChangeFrequency("biweekly", out SitemapChangeFrequency frequency);

        // Assert
        result.ShouldBeFalse();
        frequency.ShouldBe(SitemapChangeFrequency.Daily);
    }

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

    [TestMethod]
    public void TryParsePriority_ValidValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0.8", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0.8m);
    }

    [TestMethod]
    public void TryParsePriority_ZeroValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0.0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0.0m);
    }

    [TestMethod]
    public void TryParsePriority_OneValue_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1.0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(1.0m);
    }

    [TestMethod]
    public void TryParsePriority_MinimumBoundary_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("0", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(0m);
    }

    [TestMethod]
    public void TryParsePriority_MaximumBoundary_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1", out decimal priority);

        // Assert
        result.ShouldBeTrue();
        priority.ShouldBe(1m);
    }

    [TestMethod]
    public void TryParsePriority_ValueAboveOne_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("1.1", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    [TestMethod]
    public void TryParsePriority_NegativeValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("-0.1", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    [TestMethod]
    public void TryParsePriority_NullValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority(null!, out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    [TestMethod]
    public void TryParsePriority_EmptyValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority(string.Empty, out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

    [TestMethod]
    public void TryParsePriority_NonNumericValue_ReturnsFalse()
    {
        // Act
        bool result = SitemapUtility.TryParsePriority("high", out decimal priority);

        // Assert
        result.ShouldBeFalse();
        priority.ShouldBe(0.5m);
    }

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

    [TestMethod]
    public void CreateNamespaceManager_NullNameTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SitemapUtility.CreateNamespaceManager(null!));
    }

    [TestMethod]
    public void SitemapNamespace_ReturnsCorrectValue()
    {
        // Assert
        SitemapUtility.SitemapNamespace.ShouldBe("http://www.sitemaps.org/schemas/sitemap/0.9");
    }

    #endregion

    #region SitemapChangeFrequency Enum Tests

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
}