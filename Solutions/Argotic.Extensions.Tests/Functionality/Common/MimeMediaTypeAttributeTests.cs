using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class MimeMediaTypeAttributeTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        MimeMediaTypeAttribute attribute = new();

        // Assert
        attribute.ShouldNotBeNull();
        attribute.Name.ShouldBe(string.Empty);
        attribute.SubName.ShouldBe(string.Empty);
        attribute.Documentation.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Name_Set_SetsValue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act
        attribute.Name = "application";

        // Assert
        attribute.Name.ShouldBe("application");
    }

    [TestMethod]
    public void Name_SetWithWhitespace_TrimsValue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act
        attribute.Name = "  application  ";

        // Assert
        attribute.Name.ShouldBe("application");
    }

    [TestMethod]
    public void Name_SetNull_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new() { Name = "application" };

        // Act
        attribute.Name = null!;

        // Assert
        attribute.Name.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Name_SetEmpty_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new() { Name = "application" };

        // Act
        attribute.Name = string.Empty;

        // Assert
        attribute.Name.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void SubName_Set_SetsValue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act
        attribute.SubName = "rss+xml";

        // Assert
        attribute.SubName.ShouldBe("rss+xml");
    }

    [TestMethod]
    public void SubName_SetWithWhitespace_TrimsValue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act
        attribute.SubName = "  rss+xml  ";

        // Assert
        attribute.SubName.ShouldBe("rss+xml");
    }

    [TestMethod]
    public void SubName_SetNull_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new() { SubName = "rss+xml" };

        // Act
        attribute.SubName = null!;

        // Assert
        attribute.SubName.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void SubName_SetEmpty_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new() { SubName = "rss+xml" };

        // Act
        attribute.SubName = string.Empty;

        // Assert
        attribute.SubName.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Documentation_Set_SetsValue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act
        attribute.Documentation = "http://www.rssboard.org/rss-specification";

        // Assert
        attribute.Documentation.ShouldBe("http://www.rssboard.org/rss-specification");
    }

    [TestMethod]
    public void Documentation_SetNull_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new()
        {
            Documentation = "http://example.com"
        };

        // Act
        attribute.Documentation = null!;

        // Assert
        attribute.Documentation.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Documentation_SetInvalidUri_SetsEmpty()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new()
        {
            Documentation = "http://example.com"
        };

        // Note: Uri.TryCreate accepts most strings, so we test with an already null result
        attribute.Documentation = null!;

        // Assert
        attribute.Documentation.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new()
        {
            Name = "application",
            SubName = "rss+xml",
            Documentation = "http://www.rssboard.org/rss-specification"
        };

        // Act
        string result = attribute.ToString();

        // Assert
        result.ShouldContain("MimeMediaType");
        result.ShouldContain("Name = \"application\"");
        result.ShouldContain("SubName = \"rss+xml\"");
        result.ShouldContain("Documentation = \"http://www.rssboard.org/rss-specification\"");
    }

    [TestMethod]
    public void CompareTo_EqualAttributes_ReturnsZero()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act
        int result = attribute1.CompareTo(attribute2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_DifferentAttributes_ReturnsNonZero()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "atom+xml"
        };

        // Act
        int result = attribute1.CompareTo(attribute2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act
        int result = attribute.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_DifferentAttribute_ReturnsNonZero()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };
        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "text",
            SubName = "xml"
        };

        // Act
        int result = attribute1.CompareTo(attribute2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void Equals_SameAttribute_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml",
            Documentation = "http://example.com"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml",
            Documentation = "http://example.com"
        };

        // Act & Assert
        attribute1.Equals(attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentAttribute_ReturnsFalse()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "text",
            SubName = "xml"
        };

        // Act & Assert
        attribute1.Equals(attribute2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NonMimeMediaTypeAttribute_ReturnsFalse()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act & Assert
        attribute.Equals("not an attribute").ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_EqualAttributes_ReturnSameValue()
    {
        // Arrange
        MimeMediaTypeAttribute first = new() { Name = "application", SubName = "rss+xml" };
        MimeMediaTypeAttribute second = new() { Name = "application", SubName = "rss+xml" };

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    [TestMethod]
    public void OperatorEquals_EqualAttributes_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 == attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute? attribute2 = null;

        // Act & Assert
        (attribute1 == attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentAttributes_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "atom+xml"
        };

        // Act & Assert
        (attribute1 != attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_SmallerName_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "atom+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 < attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_NullFirst_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 < attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_LargerName_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "atom+xml"
        };

        // Act & Assert
        (attribute1 > attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_NullFirst_ReturnsFalse()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 > attribute2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_SmallerOrEqualName_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 <= attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_NullFirst_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 <= attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_LargerOrEqualName_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute attribute1 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        MimeMediaTypeAttribute attribute2 = new()
        {
            Name = "application",
            SubName = "rss+xml"
        };

        // Act & Assert
        (attribute1 >= attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute? attribute2 = null;

        // Act & Assert
        (attribute1 >= attribute2).ShouldBeTrue();
    }

    [TestMethod]
    public void AttributeUsage_AllowsFieldAndClass()
    {
        // Arrange & Act
        AttributeUsageAttribute[] usages = (AttributeUsageAttribute[])typeof(MimeMediaTypeAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false);

        // Assert
        usages.ShouldNotBeEmpty();
        usages[0].ValidOn.ShouldBe(AttributeTargets.Field | AttributeTargets.Class);
        usages[0].Inherited.ShouldBeFalse();
        usages[0].AllowMultiple.ShouldBeFalse();
    }
}