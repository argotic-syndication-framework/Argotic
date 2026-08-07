using System.Reflection;

using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the contract every <see cref="EnumerationMetadataAttribute"/>-annotated enumeration has to
/// keep for its values to survive a load and a save.
/// </summary>
/// <remarks>
///     <para>
///     These sweep the assemblies rather than naming enumerations, because the defect they exist to
///     catch is <b>invisible per enumeration</b>. An <c>AlternateValue</c> of <c>""</c> compiles,
///     analyses clean, and reads perfectly plausibly at the declaration site. What it does is make
///     <see cref="EnumerationMetadataAttribute.GetEnumByAlternateValue{TEnum}"/> unable to match the
///     member, so the document's attribute is read and thrown away, and
///     <see cref="EnumerationMetadataAttribute.GetAlternateValue{TEnum}"/> writes an empty string back.
///     Both directions fail, and neither throws.
///     </para>
///     <para>
///     Found on <c>YahooMediaExpression</c>, where all four members declared <c>AlternateValue = ""</c>,
///     so a conformant Media RSS <c>expression="full"</c> — the attribute YouTube's feeds carry — never
///     parsed. 117 YahooMedia tests passed throughout, because every one of them asserted round-trip
///     through the object model rather than through the token.
///     </para>
///     <para>
///     A test naming one enumeration would have prevented one recurrence. This sweeps all of them,
///     which is the only version that prevents the next one.
///     </para>
/// </remarks>
[TestClass]
public sealed class EnumerationMetadataContractTests
{
    /// <summary>
    /// Gets the product assemblies whose exported enumerations carry the metadata attribute.
    /// </summary>
    private static IEnumerable<Assembly> ProductAssemblies =>
    [
        typeof(SyndicationDateTimeUtility).Assembly,
        typeof(Argotic.Syndication.RssFeed).Assembly,
        typeof(Argotic.Extensions.Core.YahooMediaSyndicationExtension).Assembly,
    ];

    /// <summary>
    /// No annotated member other than an explicit absence marker declares an empty alternate value.
    /// </summary>
    [TestMethod]
    public void NoAnnotatedMember_DeclaresAnEmptyAlternateValue()
    {
        List<string> offenders = [];

        foreach ((Type type, string name, string alternate) in AnnotatedMembers())
        {
            // "None"/"Unknown" mean "the document did not say". They have no token to match and must
            // not be written back, so an empty alternate value is correct for those and only those.
            bool isAbsenceMarker = name is "None" or "Unknown";

            if (alternate.Length == 0 && !isAbsenceMarker)
            {
                offenders.Add($"{type.Name}.{name}");
            }
        }

        offenders.ShouldBeEmpty(
            "an empty AlternateValue makes the member unmatchable on load and blank on save, silently");
    }

    /// <summary>
    /// Two members of one enumeration never claim the same alternate value.
    /// </summary>
    /// <remarks>
    ///     The lookup is built case-insensitively, so a duplicate does not merely shadow — it decides
    ///     arbitrarily which member a document's token resolves to.
    /// </remarks>
    [TestMethod]
    public void NoEnumeration_DeclaresTheSameAlternateValueTwice()
    {
        List<string> offenders = [];

        foreach (IGrouping<Type, (Type Type, string Name, string Alternate)> byType in AnnotatedMembers().GroupBy(m => m.Type))
        {
            IEnumerable<IGrouping<string, (Type Type, string Name, string Alternate)>> duplicates = byType
                .Where(m => m.Alternate.Length > 0)
                .GroupBy(m => m.Alternate, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (IGrouping<string, (Type Type, string Name, string Alternate)> duplicate in duplicates)
            {
                offenders.Add($"{byType.Key.Name}: '{duplicate.Key}' claimed by {string.Join(" and ", duplicate.Select(m => m.Name))}");
            }
        }

        offenders.ShouldBeEmpty("a duplicated AlternateValue makes the token resolve to an arbitrary member");
    }

    /// <summary>
    /// Every annotated member survives the round trip its parser and writer perform.
    /// </summary>
    /// <remarks>
    ///     The end-to-end statement of the two assertions above, exercised through the same public pair
    ///     the extensions call: <c>XxxAsString</c> is <see cref="EnumerationMetadataAttribute.GetAlternateValue{TEnum}"/>
    ///     and <c>XxxByName</c> is <see cref="EnumerationMetadataAttribute.GetEnumByAlternateValue{TEnum}"/>.
    /// </remarks>
    [TestMethod]
    public void EveryAnnotatedMember_RoundTripsThroughItsAlternateValue()
    {
        MethodInfo getAlternate = typeof(EnumerationMetadataAttribute).GetMethod(nameof(EnumerationMetadataAttribute.GetAlternateValue))!;
        MethodInfo getByAlternate = typeof(EnumerationMetadataAttribute).GetMethod(nameof(EnumerationMetadataAttribute.GetEnumByAlternateValue))!;
        List<string> offenders = [];

        foreach ((Type type, string name, string alternate) in AnnotatedMembers())
        {
            if (name is "None" or "Unknown")
            {
                continue;
            }

            object member = Enum.Parse(type, name);
            object? written = getAlternate.MakeGenericMethod(type).Invoke(null, [member]);
            object? readBack = getByAlternate.MakeGenericMethod(type).Invoke(null, [written, member]);

            if (written is not string token || token.Length == 0 || !Equals(readBack, member))
            {
                offenders.Add($"{type.Name}.{name} wrote '{written}' and read back '{readBack}'");
            }
        }

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// The Media RSS content expression, which is where this was found.
    /// </summary>
    /// <remarks>
    ///     Kept as a named case alongside the sweep. The sweep proves the class of defect is absent;
    ///     this one names the tokens the Media RSS specification defines, so a change that renames them
    ///     to something self-consistent but wrong still fails.
    /// </remarks>
    /// <param name="token">The <c>expression</c> attribute value as Media RSS spells it.</param>
    /// <param name="expected">The member <paramref name="token"/> must resolve to, and write back out as.</param>
    [TestMethod]
    [DataRow("full", Argotic.Extensions.Core.YahooMediaExpression.Full)]
    [DataRow("sample", Argotic.Extensions.Core.YahooMediaExpression.Sample)]
    [DataRow("nonstop", Argotic.Extensions.Core.YahooMediaExpression.Nonstop)]
    public void TheMediaRssContentExpression_UsesTheTokensTheSpecificationDefines(string token, Argotic.Extensions.Core.YahooMediaExpression expected)
    {
        Argotic.Extensions.Core.YahooMediaSyndicationExtension.ExpressionByName(token).ShouldBe(expected);
        Argotic.Extensions.Core.YahooMediaSyndicationExtension.ExpressionAsString(expected).ShouldBe(token);
    }

    private static List<(Type Type, string Name, string Alternate)> AnnotatedMembers()
    {
        List<(Type, string, string)> members = [];

        foreach (Assembly assembly in ProductAssemblies.Distinct())
        {
            foreach (Type type in assembly.GetExportedTypes().Where(t => t.IsEnum))
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.GetCustomAttribute<EnumerationMetadataAttribute>() is { } metadata)
                    {
                        members.Add((type, field.Name, metadata.AlternateValue ?? string.Empty));
                    }
                }
            }
        }

        members.ShouldNotBeEmpty("the sweep found no annotated members at all, so it is asserting nothing");
        return members;
    }
}