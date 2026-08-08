using Argotic.Net;
namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Pins the ordering contract of the three XML-RPC sequence comparisons.
/// </summary>
/// <remarks>
///     <para>
///         <c>XmlRpcMessage.CompareSequence</c> and <c>XmlRpcStructureValue.CompareSequence</c> walked
///         the source collection asking <c>!target.Contains(element)</c>. That loop has only two
///         answers — <c>-1</c> when an element is absent, <c>0</c> otherwise — so two equal-length
///         collections with disjoint contents each reported <i>themselves</i> the lesser. Both now
///         delegate to the positional <c>ComparisonUtility.CompareSequence</c>.
///     </para>
///     <para>
///         The consequence is not a crash. On .NET 10 <c>GenericArraySortHelper.PickPivotAndPartition</c>
///         carries <c>left &lt; hi</c> and <c>right &gt; lo</c> guards, so the old
///         <see cref="IndexOutOfRangeException"/> path is gone and
///         <see cref="List{T}.Sort()"/> returns quietly. What it returns is a <i>silently arbitrary
///         order</i>, which is worse-shaped than a crash: the same multiset sorts into different
///         sequences depending on the order it arrived in. That is what
///         <see cref="SortingIsIndependentOfTheOrderTheElementsArrivedIn"/> asserts, and no test here
///         asserts that sorting throws.
///     </para>
///     <para>
///         Set membership also ignored order, while <c>XmlRpcArrayValue.GetHashCode</c> folded the
///         values in order — so <c>[alpha, zulu]</c> and <c>[zulu, alpha]</c> were equal with different
///         hash codes, a lookup miss. XML-RPC arrays are ordered, so the positional comparison is also
///         the protocol's own notion of sameness.
///     </para>
/// </remarks>
[TestClass]
public class XmlRpcSequenceOrderingTests
{
    private static XmlRpcArrayValue Array(params string[] values)
    {
        XmlRpcArrayValue array = new();
        foreach (string value in values)
        {
            array.Values.Add(new XmlRpcScalarValue(value));
        }

        return array;
    }

    private static XmlRpcMessage Message(params string[] values)
    {
        XmlRpcMessage message = new("m");
        foreach (string value in values)
        {
            message.Parameters.Add(new XmlRpcScalarValue(value));
        }

        return message;
    }

    private static XmlRpcStructureValue Structure(params string[] values)
    {
        XmlRpcStructureValue structure = new();
        foreach (string value in values)
        {
            structure.Members.Add(new XmlRpcStructureMember("k", new XmlRpcScalarValue(value)));
        }

        return structure;
    }

    [TestMethod]
    public void XmlRpcArrayValue_OrdersTwoDisjointOperandsOppositely()
    {
        XmlRpcArrayValue left = Array("alpha");
        XmlRpcArrayValue right = Array("zulu");

        // The direction is asserted, not just the sign relationship: antisymmetry alone is satisfied by
        // a comparison with its sign inverted, and "alpha" sorts before "zulu".
        left.CompareTo(right).ShouldBeLessThan(0);
        right.CompareTo(left).ShouldBeGreaterThan(0);
        Math.Sign(left.CompareTo(right)).ShouldBe(-Math.Sign(right.CompareTo(left)));
    }

    [TestMethod]
    public void XmlRpcMessage_OrdersTwoDisjointOperandsOppositely()
    {
        XmlRpcMessage left = Message("alpha");
        XmlRpcMessage right = Message("zulu");

        // The direction is asserted, not just the sign relationship: antisymmetry alone is satisfied by
        // a comparison with its sign inverted, and "alpha" sorts before "zulu".
        left.CompareTo(right).ShouldBeLessThan(0);
        right.CompareTo(left).ShouldBeGreaterThan(0);
        Math.Sign(left.CompareTo(right)).ShouldBe(-Math.Sign(right.CompareTo(left)));
    }

    [TestMethod]
    public void XmlRpcStructureValue_OrdersTwoDisjointOperandsOppositely()
    {
        XmlRpcStructureValue left = Structure("alpha");
        XmlRpcStructureValue right = Structure("zulu");

        // The direction is asserted, not just the sign relationship: antisymmetry alone is satisfied by
        // a comparison with its sign inverted, and "alpha" sorts before "zulu".
        left.CompareTo(right).ShouldBeLessThan(0);
        right.CompareTo(left).ShouldBeGreaterThan(0);
        Math.Sign(left.CompareTo(right)).ShouldBe(-Math.Sign(right.CompareTo(left)));
    }

    /// <summary>
    /// An XML-RPC array is ordered, so two arrays holding the same values in a different order are
    /// different arrays — and must not be equal while hashing differently.
    /// </summary>
    [TestMethod]
    public void XmlRpcArrayValue_TreatsAReorderingAsADifferentArray()
    {
        XmlRpcArrayValue forward = Array("alpha", "zulu");
        XmlRpcArrayValue reversed = Array("zulu", "alpha");
        XmlRpcArrayValue twin = Array("alpha", "zulu");

        forward.Equals(reversed).ShouldBeFalse();
        Math.Sign(forward.CompareTo(reversed)).ShouldBe(-Math.Sign(reversed.CompareTo(forward)));

        // The control: an identical array is still equal, and still hashes alike.
        forward.Equals(twin).ShouldBeTrue();
        forward.GetHashCode().ShouldBe(twin.GetHashCode());
    }

    /// <summary>
    /// The real consequence of a comparison that calls both operands the lesser: sorting the same
    /// multiset from two different starting orders produced two different results.
    /// </summary>
    [TestMethod]
    public void SortingIsIndependentOfTheOrderTheElementsArrivedIn()
    {
        List<XmlRpcArrayValue> ascending = [];
        for (int i = 0; i < 24; i++)
        {
            ascending.Add(Array($"value-{i:D2}"));
        }

        List<XmlRpcArrayValue> descending = [.. Enumerable.Reverse(ascending)];

        // Built ascending, so `ascending` arrives already sorted. The expected sequence is stated
        // independently rather than taken from either sort: the assertion used to be
        // `first.ShouldBe(second)`, which says only that the two sorts agree with each other. A
        // comparison with its sign inverted sorts both lists into descending order and satisfies that,
        // as does one keyed on something unrelated. Agreement is necessary, not sufficient.
        List<string> expected = [.. Enumerable.Range(0, 24).Select(i => Array($"value-{i:D2}").Values[0].ToString()!)];

        ascending.Sort();
        descending.Sort();

        string first = string.Join("|", ascending.Select(a => a.Values[0].ToString()));
        string second = string.Join("|", descending.Select(a => a.Values[0].ToString()));

        first.ShouldBe(second);
        first.ShouldBe(string.Join("|", expected));
    }
}