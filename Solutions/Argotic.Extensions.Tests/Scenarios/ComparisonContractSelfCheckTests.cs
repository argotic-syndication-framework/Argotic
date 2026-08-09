namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Proves each contract check can fail, by running it against types that deliberately break the
/// contract. A green contract test over a population that cannot express the violation is not
/// evidence of anything.
/// </summary>
[TestClass]
public class ComparisonContractSelfCheckTests
{
    /// <summary>A type whose comparison is finer than its hash in the dangerous direction.</summary>
    private sealed class HashMissesWhatCompareSees : IComparable<HashMissesWhatCompareSees>
    {
        public string Key { get; set; } = string.Empty;

        public int CompareTo(HashMissesWhatCompareSees? other) => other is null ? 1 : 0;

        public override bool Equals(object? obj) => obj is HashMissesWhatCompareSees other && this.CompareTo(other) == 0;

        public override int GetHashCode() => this.Key.GetHashCode(StringComparison.Ordinal);
    }

    /// <summary>A type that calls both operands the lesser.</summary>
    private sealed class BothOperandsAreLesser : IComparable<BothOperandsAreLesser>
    {
        public string Key { get; set; } = string.Empty;

        public int CompareTo(BothOperandsAreLesser? other) =>
            other is null ? 1 : string.Equals(this.Key, other.Key, StringComparison.Ordinal) ? 0 : -1;
    }

    /// <summary>A type whose ordering is antisymmetric but not transitive.</summary>
    private sealed class RockPaperScissors : IComparable<RockPaperScissors>
    {
        public int Hand { get; set; }

        public int CompareTo(RockPaperScissors? other)
        {
            if (other is null)
            {
                return 1;
            }

            int difference = ((this.Hand - other.Hand) % 3 + 3) % 3;
            return difference switch { 0 => 0, 1 => 1, _ => -1 };
        }
    }

    [TestMethod]
    public void TheHashCheckDetectsAHashFinerThanTheComparison()
    {
        HashMissesWhatCompareSees left = new() { Key = "alpha" };
        HashMissesWhatCompareSees right = new() { Key = "zulu" };

        left.Equals(right).ShouldBeTrue("the broken type must compare equal, or the probe proves nothing");
        left.GetHashCode().ShouldNotBe(right.GetHashCode());
    }

    [TestMethod]
    public void TheAntisymmetryCheckDetectsBothOperandsBeingLesser()
    {
        BothOperandsAreLesser left = new() { Key = "alpha" };
        BothOperandsAreLesser right = new() { Key = "zulu" };

        Math.Sign(left.CompareTo(right)).ShouldBe(-1);
        Math.Sign(right.CompareTo(left)).ShouldBe(-1);
        Math.Sign(left.CompareTo(right)).ShouldNotBe(-Math.Sign(right.CompareTo(left)));
    }

    [TestMethod]
    public void TheTransitivityCheckDetectsACycle()
    {
        RockPaperScissors rock = new() { Hand = 0 };
        RockPaperScissors paper = new() { Hand = 1 };
        RockPaperScissors scissors = new() { Hand = 2 };

        // Antisymmetric throughout, so only a transitivity check can catch it.
        Math.Sign(rock.CompareTo(paper)).ShouldBe(-Math.Sign(paper.CompareTo(rock)));

        bool found = false;
        RockPaperScissors[] hands = [rock, paper, scissors];
        foreach (RockPaperScissors a in hands)
        {
            foreach (RockPaperScissors b in hands)
            {
                foreach (RockPaperScissors c in hands)
                {
                    if (a.CompareTo(b) <= 0 && b.CompareTo(c) <= 0 && a.CompareTo(c) > 0)
                    {
                        found = true;
                    }
                }
            }
        }

        found.ShouldBeTrue("the transitivity predicate must fire on a known cycle");
    }

    [TestMethod]
    public void SortDetectsAnInconsistentComparer()
    {
        List<RockPaperScissors> hands = [];
        for (int i = 0; i < 60; i++)
        {
            hands.Add(new RockPaperScissors { Hand = i % 3 });
        }

        // Recorded either way: this establishes whether List.Sort is a detector at all on this runtime,
        // which is what the XML-RPC sort result has to be read against.
        string outcome;
        try
        {
            hands.Sort();
            outcome = "no throw";
        }
        catch (InvalidOperationException ex)
        {
            outcome = ex.GetType().Name;
        }

        List<BothOperandsAreLesser> lessers = [];
        for (int i = 0; i < 60; i++)
        {
            lessers.Add(new BothOperandsAreLesser { Key = $"k{i}" });
        }

        string lesserOutcome;
        try
        {
            lessers.Sort();
            lesserOutcome = "no throw";
        }
        catch (InvalidOperationException ex)
        {
            lesserOutcome = ex.GetType().Name;
        }

        string directory = Environment.GetEnvironmentVariable("ARGOTIC_CONTRACT_REPORT_DIR") ?? Path.GetTempPath();
        File.WriteAllLines(
            Path.Combine(directory, "selfcheck-sort.txt"),
            [
                $"List<RockPaperScissors>(60).Sort() on a known cycle: {outcome}",
                $"List<BothOperandsAreLesser>(60).Sort() on a known both-lesser comparer: {lesserOutcome}",
            ]);

        // The premise ComparisonContractAttackTests reads its own sort results against: on this runtime
        // List<T>.Sort does NOT detect an inconsistent comparer, so "the sort did not throw" is not
        // evidence that the comparer is consistent. Asserting the recorded outcome is what makes that
        // premise able to fail; the previous ShouldNotBeNullOrEmpty was true on both branches of both
        // try/catch blocks above, so it could not.
        outcome.ShouldBe("no throw");
        lesserOutcome.ShouldBe("no throw");
    }
}