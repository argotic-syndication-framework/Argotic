using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

/// <summary>
/// Covers <see cref="OpmlHead"/>'s ordering: which members it compares, in what precedence, and which one
/// it deliberately leaves out.
/// </summary>
/// <remarks>
///     <see cref="OpmlHead.CompareTo(OpmlHead)"/> was <c>int result = 0;</c> — every head equal to every
///     other — while <see cref="OpmlHead.GetHashCode"/> folded six members. These tests fix the six, and
///     their precedence, so that a later edit to either member has to change a named assertion rather than
///     slip through.
/// </remarks>
[TestClass]
public class OpmlHeadComparisonTests
{
    /// <summary>
    /// <c>Title</c> is compared without regard to case, matching how it is hashed.
    /// </summary>
    [TestMethod]
    public void OpmlHead_TitlesDifferingOnlyByCase_CompareEqual()
    {
        OpmlHead first = new() { Title = "Subscriptions" };
        OpmlHead second = new() { Title = "SUBSCRIPTIONS" };

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// <c>CreatedOn</c> orders two heads that agree on their titles.
    /// </summary>
    [TestMethod]
    public void OpmlHead_SameTitleDifferentCreatedOn_OrdersByCreatedOn()
    {
        OpmlHead earlier = new() { Title = "Same", CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        OpmlHead later = new() { Title = "Same", CreatedOn = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) };

        earlier.CompareTo(later).ShouldBeLessThan(0);
        later.CompareTo(earlier).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// <c>ModifiedOn</c> orders two heads that agree on title and creation date.
    /// </summary>
    [TestMethod]
    public void OpmlHead_SameTitleAndCreatedOnDifferentModifiedOn_OrdersByModifiedOn()
    {
        OpmlHead earlier = new() { Title = "Same", ModifiedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        OpmlHead later = new() { Title = "Same", ModifiedOn = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) };

        earlier.CompareTo(later).ShouldBeLessThan(0);
    }

    /// <summary>
    /// <c>VerticalScrollState</c> orders two heads that agree on everything ahead of it.
    /// </summary>
    [TestMethod]
    public void OpmlHead_DifferingOnlyByVerticalScrollState_OrdersByThatState()
    {
        OpmlHead lower = new() { Title = "Same", VerticalScrollState = 1 };
        OpmlHead higher = new() { Title = "Same", VerticalScrollState = 2 };

        lower.CompareTo(higher).ShouldBeLessThan(0);
        lower.Equals(higher).ShouldBeFalse();
    }

    /// <summary>
    /// <c>Owner</c> is folded in by delegating to <see cref="OpmlOwner.CompareTo(OpmlOwner)"/>, and a head
    /// with no owner sorts below one that has one.
    /// </summary>
    [TestMethod]
    public void OpmlHead_DifferingOnlyByOwner_DelegatesToTheOwnerComparison()
    {
        OpmlHead withoutOwner = new() { Title = "Same" };
        OpmlHead withOwner = new() { Title = "Same", Owner = new OpmlOwner { Name = "Ada" } };
        OpmlHead withLaterOwner = new() { Title = "Same", Owner = new OpmlOwner { Name = "Grace" } };

        withoutOwner.CompareTo(withOwner).ShouldBeLessThan(0);
        withOwner.CompareTo(withoutOwner).ShouldBeGreaterThan(0);
        withOwner.CompareTo(withLaterOwner).ShouldBe(
            new OpmlOwner { Name = "Ada" }.CompareTo(new OpmlOwner { Name = "Grace" }));
    }

    /// <summary>
    /// <c>Window</c> is folded in by delegating to <see cref="OpmlWindow.CompareTo(OpmlWindow)"/>.
    /// </summary>
    [TestMethod]
    public void OpmlHead_DifferingOnlyByWindow_DelegatesToTheWindowComparison()
    {
        OpmlHead withoutWindow = new() { Title = "Same" };
        OpmlHead withWindow = new() { Title = "Same", Window = new OpmlWindow { Top = 1, Left = 1, Bottom = 2, Right = 2 } };
        OpmlHead withTallerWindow = new() { Title = "Same", Window = new OpmlWindow { Top = 1, Left = 1, Bottom = 9, Right = 2 } };

        withoutWindow.CompareTo(withWindow).ShouldBeLessThan(0);
        withWindow.CompareTo(withTallerWindow).ShouldBeLessThan(0);
    }

    /// <summary>
    /// <c>ExpansionState</c> is deliberately outside the comparison: two heads differing only in it are
    /// equal, and hash equally.
    /// </summary>
    /// <remarks>
    ///     This is not an oversight and must not be "fixed" by adding the property to
    ///     <see cref="OpmlHead.CompareTo(OpmlHead)"/>. <c>HashCodeUtility.Component&lt;T&gt;(T)</c> returns
    ///     its argument unchanged, so folding an <see cref="IList{T}"/> straight into
    ///     <see cref="HashCode.Combine{T1,T2,T3,T4,T5,T6}"/> hashes the list <i>instance</i> — the
    ///     collection-by-reference defect <c>.endjin/build-warnings.md</c> §4.3 removed from seven types.
    ///     Including it correctly means folding its elements one at a time, on both sides, together.
    /// </remarks>
    [TestMethod]
    public void OpmlHead_DifferingOnlyByExpansionState_AreEqualAndHashEqually()
    {
        OpmlHead first = new() { Title = "Same" };
        OpmlHead second = new() { Title = "Same" };
        second.ExpansionState.Add(3);
        second.ExpansionState.Add(7);

        first.CompareTo(second).ShouldBe(0);
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// A head is greater than <see langword="null"/>, which is what every other comparable type in the
    /// library reports and what the comparison operators rely on.
    /// </summary>
    [TestMethod]
    public void OpmlHead_ComparedToNull_IsGreater()
    {
        OpmlHead head = new() { Title = "Same" };

        head.CompareTo(null).ShouldBe(1);
        head.Equals(null).ShouldBeFalse();
    }
}