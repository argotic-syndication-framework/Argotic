using System.Collections;
using System.Reflection;
using System.Text;
using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

// Every probe runs against a generated instance of an arbitrary exported type. A comparison or a
// property read that throws is a finding to record, not an exception to propagate — propagating it
// would end the sweep at the first awkward type and hide every violation after it. CA1031 is disabled
// for the file for that reason.
#pragma warning disable CA1031

/// <summary>
/// Attacks the equality, ordering and hashing contracts across every comparable type in the three
/// product assemblies, generating instances rather than enumerating examples.
/// </summary>
[TestClass]
public class ComparisonContractTests
{
    private static readonly string ReportDirectory =
        Environment.GetEnvironmentVariable("ARGOTIC_CONTRACT_REPORT_DIR") ?? Path.GetTempPath();

    /// <summary>Records the population sizes so the report can state what was actually covered.</summary>
    private static void Report(string name, IEnumerable<string> lines)
    {
        try
        {
            File.WriteAllLines(Path.Combine(ReportDirectory, $"contract-{name}.txt"), lines);
        }
        catch (IOException)
        {
            // The report is a convenience; the assertion below is the verdict.
        }
    }

    private static string Describe(Type type, object instance)
    {
        StringBuilder builder = new();
        builder.Append(type.Name).Append('{');
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.GetIndexParameters().Length == 0)
                     .OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            object? value;
            try
            {
                value = property.GetValue(instance);
            }
            catch (Exception)
            {
                continue;
            }

            string rendered = value switch
            {
                null => "null",
                string s => $"\"{s}\"",
                IEnumerable e and not string => "[" + string.Join("|", e.Cast<object?>().Select(x => x?.ToString() ?? "null")) + "]",
                _ => value.ToString() ?? string.Empty,
            };

            if (rendered is "null" or "\"\"" or "[]" or "0" or "False")
            {
                continue;
            }

            builder.Append(property.Name).Append('=').Append(rendered).Append(' ');
        }

        return builder.Append('}').ToString();
    }

    /// <summary>Contract 1: equal instances must return equal hash codes. A violation is a lookup miss.</summary>
    [TestMethod]
    public void EqualInstancesMustReturnEqualHashCodes()
    {
        List<string> violations = [];
        List<string> covered = [];

        foreach (Type type in ComparisonContractHarness.EqualityTypes())
        {
            IReadOnlyList<object> population = ComparisonContractHarness.Population(type, cap: 40);
            covered.Add($"{type.FullName}\t{population.Count}");
            if (population.Count < 2)
            {
                continue;
            }

            for (int i = 0; i < population.Count; i++)
            {
                for (int j = i + 1; j < population.Count; j++)
                {
                    bool equal;
                    try
                    {
                        equal = population[i].Equals(population[j]);
                    }
                    catch (Exception ex)
                    {
                        violations.Add($"THROW {type.FullName}: Equals threw {ex.GetType().Name}: {ex.Message}");
                        continue;
                    }

                    if (!equal)
                    {
                        continue;
                    }

                    int leftHash = population[i].GetHashCode();
                    int rightHash = population[j].GetHashCode();
                    if (leftHash != rightHash)
                    {
                        violations.Add(
                            $"MISS {type.FullName}: equal but hashes {leftHash} vs {rightHash}\n" +
                            $"      a = {Describe(type, population[i])}\n" +
                            $"      b = {Describe(type, population[j])}");
                    }
                }
            }
        }

        Report("hash-population", covered);
        Report("hash-violations", violations);
        violations.ShouldBeEmpty(
            $"{violations.Count} equal-but-unequal-hash violations over {covered.Count} types:\n" +
            string.Join("\n", violations.Take(40)));
    }

    /// <summary>Contract 2: <c>sign(a.CompareTo(b))</c> must equal <c>-sign(b.CompareTo(a))</c>.</summary>
    [TestMethod]
    public void CompareToMustBeAntisymmetric()
    {
        List<string> violations = [];
        List<string> covered = [];

        foreach (Type type in ComparisonContractHarness.ComparableTypes())
        {
            if (!ComparisonContractHarness.HasSelfCompareTo(type))
            {
                continue;
            }

            IReadOnlyList<object> population = ComparisonContractHarness.Population(type, cap: 40);
            covered.Add($"{type.FullName}\t{population.Count}");

            for (int i = 0; i < population.Count; i++)
            {
                for (int j = i + 1; j < population.Count; j++)
                {
                    int forward;
                    int backward;
                    try
                    {
                        forward = ComparisonContractHarness.Compare(type, population[i], population[j]);
                        backward = ComparisonContractHarness.Compare(type, population[j], population[i]);
                    }
                    catch (Exception ex)
                    {
                        violations.Add($"THROW {type.FullName}: CompareTo threw {ex.GetType().Name}: {ex.Message}");
                        continue;
                    }

                    if (Math.Sign(forward) != -Math.Sign(backward))
                    {
                        violations.Add(
                            $"ASYM {type.FullName}: a.CompareTo(b)={forward}, b.CompareTo(a)={backward}\n" +
                            $"      a = {Describe(type, population[i])}\n" +
                            $"      b = {Describe(type, population[j])}");
                    }
                }
            }
        }

        Report("antisymmetry-population", covered);
        Report("antisymmetry-violations", violations);
        violations.ShouldBeEmpty(
            $"{violations.Count} antisymmetry violations over {covered.Count} types:\n" +
            string.Join("\n", violations.Take(40)));
    }

    /// <summary>Contract 3: the ordering must be transitive.</summary>
    [TestMethod]
    public void CompareToMustBeTransitive()
    {
        List<string> violations = [];
        List<string> covered = [];

        foreach (Type type in ComparisonContractHarness.ComparableTypes())
        {
            if (!ComparisonContractHarness.HasSelfCompareTo(type))
            {
                continue;
            }

            IReadOnlyList<object> population = ComparisonContractHarness.Population(type, cap: 14);
            covered.Add($"{type.FullName}\t{population.Count}");

            for (int i = 0; i < population.Count; i++)
            {
                for (int j = 0; j < population.Count; j++)
                {
                    for (int k = 0; k < population.Count; k++)
                    {
                        int ab;
                        int bc;
                        int ac;
                        try
                        {
                            ab = ComparisonContractHarness.Compare(type, population[i], population[j]);
                            bc = ComparisonContractHarness.Compare(type, population[j], population[k]);
                            ac = ComparisonContractHarness.Compare(type, population[i], population[k]);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (ab <= 0 && bc <= 0 && ac > 0)
                        {
                            violations.Add(
                                $"TRANS {type.FullName}: a<=b ({ab}), b<=c ({bc}), but a>c ({ac})\n" +
                                $"      a = {Describe(type, population[i])}\n" +
                                $"      b = {Describe(type, population[j])}\n" +
                                $"      c = {Describe(type, population[k])}");
                        }
                    }
                }
            }
        }

        Report("transitivity-population", covered);
        Report("transitivity-violations", violations);
        violations.ShouldBeEmpty(
            $"{violations.Count} transitivity violations over {covered.Count} types:\n" +
            string.Join("\n", violations.Take(20)));
    }

    /// <summary>Contract 4: reflexivity, and the null answers every implementation must give.</summary>
    [TestMethod]
    public void CompareToMustBeReflexiveAndHandleNull()
    {
        List<string> violations = [];
        List<string> covered = [];

        foreach (Type type in ComparisonContractHarness.ComparableTypes())
        {
            if (!ComparisonContractHarness.HasSelfCompareTo(type))
            {
                continue;
            }

            IReadOnlyList<object> population = ComparisonContractHarness.Population(type, cap: 20);
            covered.Add($"{type.FullName}\t{population.Count}");

            foreach (object instance in population)
            {
                try
                {
                    int self = ComparisonContractHarness.Compare(type, instance, instance);
                    if (self != 0)
                    {
                        violations.Add($"REFL {type.FullName}: a.CompareTo(a)={self} for {Describe(type, instance)}");
                    }
                }
                catch (Exception ex)
                {
                    violations.Add($"REFL-THROW {type.FullName}: {ex.GetType().Name}: {ex.Message}");
                }

                try
                {
                    int againstNull = ComparisonContractHarness.Compare(type, instance, null);
                    if (againstNull <= 0)
                    {
                        violations.Add($"NULL {type.FullName}: a.CompareTo(null)={againstNull}");
                    }
                }
                catch (Exception ex)
                {
                    violations.Add($"NULL-THROW {type.FullName}: CompareTo(null) threw {ex.GetType().Name}: {ex.Message}");
                }

                if (instance.Equals(null))
                {
                    violations.Add($"EQNULL {type.FullName}: a.Equals(null) returned true");
                }
            }
        }

        Report("reflexivity-population", covered);
        Report("reflexivity-violations", violations);
        violations.ShouldBeEmpty(
            $"{violations.Count} reflexivity/null violations over {covered.Count} types:\n" +
            string.Join("\n", violations.Take(40)));
    }

    /// <summary>Contract 5: sorting a list of varied instances must terminate without throwing.</summary>
    [TestMethod]
    public void SortingAVariedListMustNotThrow()
    {
        List<string> violations = [];
        List<string> covered = [];

        foreach (Type type in ComparisonContractHarness.ComparableTypes())
        {
            if (!ComparisonContractHarness.HasSelfCompareTo(type))
            {
                continue;
            }

            IReadOnlyList<object> population = ComparisonContractHarness.Population(type, cap: 50);
            if (population.Count == 0)
            {
                continue;
            }

            covered.Add($"{type.FullName}\t{population.Count}");

            Type listType = typeof(List<>).MakeGenericType(type);
            IList list = (IList)Activator.CreateInstance(listType)!;
            for (int i = 0; list.Count < 50; i++)
            {
                list.Add(population[i % population.Count]);
            }

            MethodInfo sort = listType.GetMethod("Sort", Type.EmptyTypes)!;
            try
            {
                sort.Invoke(list, null);
            }
            catch (TargetInvocationException ex)
            {
                violations.Add($"SORT {type.FullName}: {ex.InnerException?.GetType().Name}: {ex.InnerException?.Message}");
            }
        }

        Report("sort-population", covered);
        Report("sort-violations", violations);
        violations.ShouldBeEmpty(
            $"{violations.Count} sort failures over {covered.Count} types:\n" +
            string.Join("\n", violations.Take(40)));
    }
}