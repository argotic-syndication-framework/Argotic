using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers <c>MimeMediaTypeAttribute</c>: what its three property setters keep, trim or discard, and the
/// ordering, equality and hash contract it derives from all three.
/// </summary>
/// <remarks>
///     <para>
///     Ordering runs <c>Documentation</c>, then <c>Name</c>, then <c>SubName</c>, each compared with
///     <see cref="StringComparison.OrdinalIgnoreCase"/>, and <c>Equals</c> is defined as <c>CompareTo</c>
///     returning zero — so every equality case below is a statement about the comparison as well.
///     </para>
///     <para>
///     The type declares only <c>==</c> and <c>!=</c>. The four relational operators come from the C# 14
///     extension block in <c>ComparisonOperatorExtensions</c>, which is why the null operands are legal
///     rather than a dereference.
///     </para>
/// </remarks>
[TestClass]
public class MimeMediaTypeAttributeTests
{
    /// <summary>
    /// Gets or sets the context MSTest supplies to the test. Nothing in this class reads it.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A newly constructed attribute reports an empty <c>Name</c>, <c>SubName</c> and <c>Documentation</c>
    /// rather than <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// A top-level type name given without surrounding whitespace is stored verbatim.
    /// </summary>
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

    /// <summary>
    /// The setter trims <c>Name</c>, so <c>"  application  "</c> reads back as <c>application</c>.
    /// </summary>
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

    /// <summary>
    /// Assigning <see langword="null"/> to <c>Name</c> clears a stored value to an empty string, and never
    /// makes the property read back <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// Assigning an empty string to <c>Name</c> clears a stored value rather than being ignored.
    /// </summary>
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

    /// <summary>
    /// A subtype given without surrounding whitespace is stored verbatim, <c>+</c> and all.
    /// </summary>
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

    /// <summary>
    /// The setter trims <c>SubName</c>, so <c>"  rss+xml  "</c> reads back as <c>rss+xml</c>.
    /// </summary>
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

    /// <summary>
    /// Assigning <see langword="null"/> to <c>SubName</c> clears a stored value to an empty string.
    /// </summary>
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

    /// <summary>
    /// Assigning an empty string to <c>SubName</c> clears a stored value rather than being ignored.
    /// </summary>
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

    /// <summary>
    /// A documentation location that parses as a <see cref="Uri"/> reads back as the string it was given.
    /// </summary>
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

    /// <summary>
    /// Assigning <see langword="null"/> to <c>Documentation</c> discards the stored location and leaves the
    /// property an empty string.
    /// </summary>
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

    /// <summary>
    /// A location the setter will not accept is discarded rather than rejected, leaving <c>Documentation</c>
    /// an empty string instead of throwing.
    /// </summary>
    /// <remarks>
    ///     The inline note records why the body assigns <see langword="null"/> rather than a malformed
    ///     location: the setter parses with <c>UriKind.RelativeOrAbsolute</c>, which accepts almost every
    ///     string, so <see langword="null"/> is the only input reachable here — and that makes this the same
    ///     case as <c>Documentation_SetNull_SetsEmpty</c> above.
    /// </remarks>
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

    /// <summary>
    /// <c>ToString</c> writes the attribute out in the source form that would declare it, naming all three
    /// properties and quoting each value.
    /// </summary>
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

    /// <summary>
    /// <c>ToString</c> writes an empty <c>Documentation</c> value when none was set.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A guard, green on both sides of the change it protects, and deliberately so.
    ///     <c>ToString</c> interpolated <c>Documentation ?? string.Empty</c>, but <c>Documentation</c>
    ///     is a non-nullable <see cref="string"/> whose getter already ends <c>?? string.Empty</c>, so
    ///     the right-hand operand was unreachable and deleting it is <b>observationally inert</b>. No
    ///     input distinguishes before from after, which is why there is no red-first test here — only
    ///     evidence that the empty case is written the way it always was.
    ///     </para>
    ///     <para>
    ///     CA1508 is on solution-wide and does not flag it: its null analysis does not track values
    ///     returned from property getters. This one had to be found by reading.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ToString_WithNoDocumentation_WritesAnEmptyValue()
    {
        MimeMediaTypeAttribute attribute = new() { Name = "application", SubName = "rss+xml" };

        attribute.Documentation.ShouldBe(string.Empty);
        attribute.ToString().ShouldContain("Documentation = \"\"", Case.Sensitive);
    }

    /// <summary>
    /// Two separately constructed attributes carrying the same name and subtype compare equal.
    /// </summary>
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

    /// <summary>
    /// The subtype alone is enough to separate two attributes: <c>application/rss+xml</c> does not compare
    /// equal to <c>application/atom+xml</c>.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> returns <c>1</c>, so a null sorts ahead of every instance.
    /// </summary>
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

    /// <summary>
    /// A differing top-level type separates two attributes too: <c>application/rss+xml</c> does not compare
    /// equal to <c>text/xml</c>.
    /// </summary>
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

    /// <summary>
    /// Two separately constructed attributes agreeing on all three properties are equal, so equality is by
    /// value and not by reference.
    /// </summary>
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

    /// <summary>
    /// <c>application/rss+xml</c> is not equal to <c>text/xml</c>.
    /// </summary>
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

    /// <summary>
    /// <c>Equals(object)</c> answers <see langword="false"/> for a value of an unrelated type rather than
    /// throwing, which is the <c>is MimeMediaTypeAttribute other</c> pattern in the override doing its job.
    /// </summary>
    [TestMethod]
    public void Equals_NonMimeMediaTypeAttribute_ReturnsFalse()
    {
        // Arrange
        MimeMediaTypeAttribute attribute = new();

        // Act & Assert
        attribute.Equals("not an attribute").ShouldBeFalse();
    }

    /// <summary>
    /// Two instances that compare equal produce the same hash code, and a hash code does not change between
    /// calls on one instance.
    /// </summary>
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

    /// <summary>
    /// <c>==</c> reports two distinct instances of equal value as equal, so the declared operator is reached
    /// rather than predefined reference equality.
    /// </summary>
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

    /// <summary>
    /// Two <see langword="null"/> operands are <c>==</c>, and the operator reaches that answer without
    /// dereferencing either.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute? attribute2 = null;

        // Act & Assert
        (attribute1 == attribute2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>!=</c> reports two attributes differing only in subtype as unequal.
    /// </summary>
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

    /// <summary>
    /// Of two attributes sharing the top-level type <c>application</c>, the one whose subtype sorts earlier
    /// is <c>&lt;</c> the other: <c>atom+xml</c> precedes <c>rss+xml</c>.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> left operand is <c>&lt;</c> any instance, so a null sorts first.
    /// </summary>
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

    /// <summary>
    /// The same pair the other way round: <c>application/rss+xml</c> is <c>&gt;</c>
    /// <c>application/atom+xml</c>, so the ordering is antisymmetric on the subtype.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> left operand is not <c>&gt;</c> an instance.
    /// </summary>
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

    /// <summary>
    /// <c>&lt;=</c> holds between two attributes of equal value — the equal arm of the operator, not the
    /// lesser one.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> left operand is <c>&lt;=</c> any instance.
    /// </summary>
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

    /// <summary>
    /// <c>&gt;=</c> holds between two attributes of equal value — again the equal arm, not the greater one.
    /// </summary>
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

    /// <summary>
    /// Two <see langword="null"/> operands satisfy <c>&gt;=</c>, which is the degenerate corner the extension
    /// block has to get right for a null-tolerant sort to work.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        MimeMediaTypeAttribute? attribute1 = null;
        MimeMediaTypeAttribute? attribute2 = null;

        // Act & Assert
        (attribute1 >= attribute2).ShouldBeTrue();
    }

    /// <summary>
    /// The attribute may be applied to a field or a class and to nothing else, is not inherited, and may not
    /// be applied twice to one target.
    /// </summary>
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