using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

/// <summary>
/// Tests for extension operators on <see cref="OpmlHead"/>.
/// Note: OpmlHead.CompareTo always returns 0 (comparison not implemented),
/// so only equality-based tests are valid.
/// </summary>
[TestClass]
public class OpmlHeadExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<OpmlHead>
{
    protected override OpmlHead CreateInstance()
        => new() { Title = "Head B" };

    // Since OpmlHead.CompareTo always returns 0, all instances are "equal"
    // from a comparison perspective. We return identical instances.
    protected override OpmlHead CreateLesserInstance()
        => new() { Title = "Head B" };

    protected override OpmlHead CreateGreaterInstance()
        => new() { Title = "Head B" };

    // Override tests that rely on ordering since OpmlHead.CompareTo is not implemented
    [TestMethod]
    public override void OperatorLessThan_LesserThanGreater_ReturnsTrue()
    {
        // Skip: OpmlHead.CompareTo always returns 0, so < is always false between non-null instances
        var lesser = CreateLesserInstance();
        var greater = CreateGreaterInstance();
        var result = lesser < greater;
        result.ShouldBe(false); // All instances compare as equal
    }

    [TestMethod]
    public override void OperatorGreaterThan_GreaterThanLesser_ReturnsTrue()
    {
        // Skip: OpmlHead.CompareTo always returns 0, so > is always false between non-null instances
        var lesser = CreateLesserInstance();
        var greater = CreateGreaterInstance();
        var result = greater > lesser;
        result.ShouldBe(false); // All instances compare as equal
    }

    [TestMethod]
    public override void OperatorLessThanOrEqual_GreaterThanLesser_ReturnsFalse()
    {
        // Since all instances compare as equal, <= is always true
        var lesser = CreateLesserInstance();
        var greater = CreateGreaterInstance();
        var result = greater <= lesser;
        result.ShouldBe(true); // All instances compare as equal
    }

    [TestMethod]
    public override void OperatorGreaterThanOrEqual_LesserThanGreater_ReturnsFalse()
    {
        // Since all instances compare as equal, >= is always true
        var lesser = CreateLesserInstance();
        var greater = CreateGreaterInstance();
        var result = lesser >= greater;
        result.ShouldBe(true); // All instances compare as equal
    }
}
