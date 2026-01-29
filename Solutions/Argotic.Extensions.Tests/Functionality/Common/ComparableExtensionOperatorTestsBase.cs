using Argotic.Common;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Base test class for verifying C# 14 extension comparison operators work correctly
/// for types implementing <see cref="IComparable{T}"/>.
/// </summary>
/// <typeparam name="T">The comparable type to test.</typeparam>
public abstract class ComparableExtensionOperatorTestsBase<T> where T : class, IComparable<T>, IComparisonOperators
{
    /// <summary>Creates a baseline instance for comparison.</summary>
    protected abstract T CreateInstance();

    /// <summary>Creates an instance that compares less than the baseline.</summary>
    protected abstract T CreateLesserInstance();

    /// <summary>Creates an instance that compares greater than the baseline.</summary>
    protected abstract T CreateGreaterInstance();

    #region Less Than Operator (<)

    [TestMethod]
    public virtual void OperatorLessThan_LesserThanGreater_ReturnsTrue()
    {
        // Arrange
        T lesser = CreateLesserInstance();
        T greater = CreateGreaterInstance();

        // Act
        bool result = lesser < greater;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_GreaterThanLesser_ReturnsFalse()
    {
        // Arrange
        T greater = CreateGreaterInstance();
        T lesser = CreateLesserInstance();

        // Act
        bool result = greater < lesser;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThan_EqualInstances_ReturnsFalse()
    {
        // Arrange
        T first = CreateInstance();
        T second = CreateInstance();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThan_NullLessThanInstance_ReturnsTrue()
    {
        // Arrange
        T? first = null;
        T second = CreateInstance();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_InstanceLessThanNull_ReturnsFalse()
    {
        // Arrange
        T first = CreateInstance();
        T? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Greater Than Operator (>)

    [TestMethod]
    public virtual void OperatorGreaterThan_GreaterThanLesser_ReturnsTrue()
    {
        // Arrange
        T greater = CreateGreaterInstance();
        T lesser = CreateLesserInstance();

        // Act
        bool result = greater > lesser;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_LesserThanGreater_ReturnsFalse()
    {
        // Arrange
        T lesser = CreateLesserInstance();
        T greater = CreateGreaterInstance();

        // Act
        bool result = lesser > greater;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_EqualInstances_ReturnsFalse()
    {
        // Arrange
        T first = CreateInstance();
        T second = CreateInstance();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_NullGreaterThanInstance_ReturnsFalse()
    {
        // Arrange
        T? first = null;
        T second = CreateInstance();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_InstanceGreaterThanNull_ReturnsTrue()
    {
        // Arrange
        T first = CreateInstance();
        T? second = null;

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region Less Than Or Equal Operator (<=)

    [TestMethod]
    public void OperatorLessThanOrEqual_LesserThanGreater_ReturnsTrue()
    {
        // Arrange
        T lesser = CreateLesserInstance();
        T greater = CreateGreaterInstance();

        // Act
        bool result = lesser <= greater;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_EqualInstances_ReturnsTrue()
    {
        // Arrange
        T first = CreateInstance();
        T second = CreateInstance();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public virtual void OperatorLessThanOrEqual_GreaterThanLesser_ReturnsFalse()
    {
        // Arrange
        T greater = CreateGreaterInstance();
        T lesser = CreateLesserInstance();

        // Act
        bool result = greater <= lesser;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_NullLessThanOrEqualInstance_ReturnsTrue()
    {
        // Arrange
        T? first = null;
        T second = CreateInstance();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_NullLessThanOrEqualNull_ReturnsTrue()
    {
        // Arrange
        T? first = null;
        T? second = null;

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region Greater Than Or Equal Operator (>=)

    [TestMethod]
    public void OperatorGreaterThanOrEqual_GreaterThanLesser_ReturnsTrue()
    {
        // Arrange
        T greater = CreateGreaterInstance();
        T lesser = CreateLesserInstance();

        // Act
        bool result = greater >= lesser;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_EqualInstances_ReturnsTrue()
    {
        // Arrange
        T first = CreateInstance();
        T second = CreateInstance();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public virtual void OperatorGreaterThanOrEqual_LesserThanGreater_ReturnsFalse()
    {
        // Arrange
        T lesser = CreateLesserInstance();
        T greater = CreateGreaterInstance();

        // Act
        bool result = lesser >= greater;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_InstanceGreaterThanOrEqualNull_ReturnsTrue()
    {
        // Arrange
        T first = CreateInstance();
        T? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullGreaterThanOrEqualNull_ReturnsTrue()
    {
        // Arrange
        T? first = null;
        T? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion
}
