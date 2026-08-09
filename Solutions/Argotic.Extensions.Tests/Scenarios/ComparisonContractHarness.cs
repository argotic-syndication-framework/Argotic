using System.Collections;
using System.Globalization;
using System.Reflection;
namespace Argotic.Extensions.Tests.Scenarios;

// The generator constructs and mutates every exported type reflectively. A constructor that rejects a
// synthesised argument, a setter that validates, a property that throws on read — each is a shape this
// population cannot reach, not a fault to propagate, and the harness must carry on to the next
// candidate. Catching broadly is the design, so CA1031 is disabled for the file.
#pragma warning disable CA1031

/// <summary>
/// Reflection-driven generator of instance populations for the comparison-contract attack.
/// </summary>
internal static class ComparisonContractHarness
{
    /// <summary>Gets the three product assemblies whose exported types form the population.</summary>
    public static Assembly[] ProductAssemblies =>
    [
        typeof(HashCodeUtility).Assembly,
        typeof(Argotic.Syndication.OpmlHead).Assembly,
        typeof(Argotic.Extensions.Core.SitemapVideo).Assembly,
    ];

    /// <summary>Gets every exported, concrete type that implements <c>IComparable&lt;itself&gt;</c>.</summary>
    public static IReadOnlyList<Type> ComparableTypes() =>
        ProductAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IComparable<>) &&
                i.GetGenericArguments()[0].IsAssignableFrom(t)))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

    /// <summary>Gets every exported, concrete type that declares its own <c>GetHashCode</c> or <c>Equals(object)</c>.</summary>
    public static IReadOnlyList<Type> EqualityTypes() =>
        ProductAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .Where(t =>
                t.GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)?.DeclaringType == t ||
                t.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance, [typeof(object)])?.DeclaringType == t)
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

    /// <summary>Invokes <c>CompareTo</c> through the closed <see cref="IComparable{T}"/> interface.</summary>
    public static int Compare(Type type, object? left, object? right)
    {
        MethodInfo method = type.GetMethod("CompareTo", BindingFlags.Public | BindingFlags.Instance, [type])
            ?? throw new InvalidOperationException($"{type.FullName} has no CompareTo({type.Name}).");
        try
        {
            return (int)method.Invoke(left, [right])!;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    /// <summary>Gets a value indicating whether the type exposes <c>CompareTo</c> taking its own type.</summary>
    public static bool HasSelfCompareTo(Type type) =>
        type.GetMethod("CompareTo", BindingFlags.Public | BindingFlags.Instance, [type]) is not null;

    /// <summary>Creates an instance with every member left at its default, or <see langword="null"/> if it cannot be built.</summary>
    public static object? TryCreateDefault(Type type)
    {
        ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        foreach (ConstructorInfo constructor in constructors.OrderBy(c => c.GetParameters().Length))
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length > 3)
            {
                continue;
            }

            object?[] arguments = new object?[parameters.Length];
            bool usable = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                IReadOnlyList<object?> pool = ValuesFor(parameters[i].ParameterType);
                object? candidate = pool.FirstOrDefault(v => v is not null);
                if (candidate is null && parameters[i].ParameterType.IsValueType)
                {
                    candidate = Activator.CreateInstance(parameters[i].ParameterType);
                }

                if (candidate is null && !IsNullableReference(parameters[i].ParameterType))
                {
                    usable = false;
                    break;
                }

                arguments[i] = candidate;
            }

            if (!usable)
            {
                continue;
            }

            try
            {
                return constructor.Invoke(arguments);
            }
            catch (Exception)
            {
                // Constructor validation rejected the synthesised arguments; try the next overload.
            }
        }

        return null;
    }

    /// <summary>
    /// Builds a varied population for the supplied type: one default instance, then one instance per
    /// (writable member, candidate value) pair, then collection-shaped variants for read-only lists.
    /// </summary>
    /// <param name="type">The type to populate.</param>
    /// <param name="cap">The maximum number of instances to return.</param>
    /// <returns>A list of distinct instances, or an empty list if the type cannot be constructed.</returns>
    public static IReadOnlyList<object> Population(Type type, int cap = 60)
    {
        List<object> instances = [];
        object? baseline = TryCreateDefault(type);
        if (baseline is null)
        {
            return instances;
        }

        instances.Add(baseline);

        // A second default instance: distinct reference, identical state. This is what catches a
        // GetHashCode that was never overridden at all.
        object? twin = TryCreateDefault(type);
        if (twin is not null)
        {
            instances.Add(twin);
        }

        PropertyInfo[] properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            if (instances.Count >= cap)
            {
                break;
            }

            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (property.CanWrite && property.SetMethod is { IsPublic: true })
            {
                foreach (object? value in ValuesFor(property.PropertyType))
                {
                    object? instance = TryCreateDefault(type);
                    if (instance is null)
                    {
                        continue;
                    }

                    try
                    {
                        property.SetValue(instance, value);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    instances.Add(instance);
                    if (instances.Count >= cap)
                    {
                        break;
                    }
                }
            }
            else if (property.CanRead && IsMutableList(property.PropertyType, out Type? elementType))
            {
                foreach (object instance in CollectionVariants(type, property, elementType!))
                {
                    instances.Add(instance);
                    if (instances.Count >= cap)
                    {
                        break;
                    }
                }
            }
        }

        return instances;
    }

    /// <summary>Builds instances that differ only in the contents and ordering of one read-only list member.</summary>
    private static IEnumerable<object> CollectionVariants(Type ownerType, PropertyInfo property, Type elementType)
    {
        List<object?> elements = ElementSamples(elementType);
        if (elements.Count < 2)
        {
            yield break;
        }

        // Three probes, each aimed at a different failure:
        //   [a] against [b]         -- equal length, disjoint contents. A set-membership comparison
        //                              that can only answer "absent" calls both the lesser.
        //   [a,b] against [b,a]     -- equal length, same contents, different order. A comparison that
        //                              ignores order against a hash that folds it is a lookup miss.
        //   [a,a] against [b,b]     -- equal length, disjoint, with repeats.
        List<object?[]> shapes =
        [
            [elements[0]],
            [elements[1]],
            [elements[0], elements[1]],
            [elements[1], elements[0]],
            [elements[0], elements[0]],
            [elements[1], elements[1]],
        ];

        foreach (object?[] shape in shapes)
        {
            object? instance = TryCreateDefault(ownerType);
            if (instance is null)
            {
                continue;
            }

            object? list;
            try
            {
                list = property.GetValue(instance);
            }
            catch (Exception)
            {
                continue;
            }

            if (list is not IList target || target.IsReadOnly)
            {
                continue;
            }

            bool filled = true;
            foreach (object? element in shape)
            {
                try
                {
                    target.Add(element);
                }
                catch (Exception)
                {
                    filled = false;
                    break;
                }
            }

            if (filled)
            {
                yield return instance;
            }
        }
    }

    /// <summary>Produces two distinct sample elements for a collection element type.</summary>
    private static List<object?> ElementSamples(Type elementType)
    {
        IReadOnlyList<object?> pool = elementType.IsAbstract || elementType.IsInterface
            ? []
            : ValuesFor(elementType);
        List<object?> distinct = pool.Where(v => v is not null).Take(2).ToList();
        if (distinct.Count == 2)
        {
            return distinct;
        }

        // Fall back to two product objects that are genuinely unequal. An abstract or interface element
        // type — IXmlRpcValue, say — is resolved to a concrete implementation first, without which
        // XmlRpcMessage.Parameters could never be filled and its comparison never probed. Picking two
        // instances that merely differ by reference is not enough: a set-membership comparison answers
        // "present" for two equal elements, so the probe could not fail.
        if (NestingDepth.Value >= 2)
        {
            return [];
        }

        foreach (Type concrete in ConcreteFor(elementType))
        {
            NestingDepth.Value++;
            IReadOnlyList<object> candidates;
            try
            {
                candidates = Population(concrete, cap: 12);
            }
            finally
            {
                NestingDepth.Value--;
            }

            foreach (object candidate in candidates)
            {
                foreach (object other in candidates)
                {
                    if (!ReferenceEquals(candidate, other) && !candidate.Equals(other))
                    {
                        return [candidate, other];
                    }
                }
            }
        }

        return [];
    }

    /// <summary>Gets a value indicating whether the supplied type is a mutable list, yielding its element type.</summary>
    private static bool IsMutableList(Type type, out Type? elementType)
    {
        elementType = null;
        if (!typeof(IList).IsAssignableFrom(type) && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            return false;
        }

        Type? closed = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>)
            ? type
            : type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

        if (closed is null)
        {
            return false;
        }

        elementType = closed.GetGenericArguments()[0];
        return true;
    }

    private static bool IsNullableReference(Type type) => !type.IsValueType;

    /// <summary>
    /// Gets the candidate values used to vary a member. Deliberately includes pairs that a
    /// case-insensitive or unescaping comparison collapses but a naive hash does not.
    /// </summary>
    /// <param name="type">The member type.</param>
    /// <returns>Candidate values, which may include <see langword="null"/>.</returns>
    public static IReadOnlyList<object?> ValuesFor(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;
        List<object?> values = [];

        if (Nullable.GetUnderlyingType(type) is not null || !type.IsValueType)
        {
            values.Add(null);
        }

        if (underlying == typeof(string))
        {
            values.AddRange([string.Empty, "alpha", "ALPHA", "zulu", " ", "ÿ"]);
        }
        else if (underlying == typeof(int))
        {
            values.AddRange([0, 1, -1, int.MinValue, int.MaxValue]);
        }
        else if (underlying == typeof(long))
        {
            values.AddRange([0L, 1L, long.MinValue, long.MaxValue]);
        }
        else if (underlying == typeof(decimal))
        {
            values.AddRange([0m, 1m, decimal.MinValue, decimal.MaxValue]);
        }
        else if (underlying == typeof(double))
        {
            values.AddRange([0d, 1d, double.MinValue, double.MaxValue]);
        }
        else if (underlying == typeof(float))
        {
            values.AddRange([0f, 1f, float.MinValue, float.MaxValue]);
        }
        else if (underlying == typeof(bool))
        {
            values.AddRange([false, true]);
        }
        else if (underlying == typeof(DateTime))
        {
            values.AddRange([DateTime.MinValue, DateTime.MaxValue, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc)]);
        }
        else if (underlying == typeof(TimeSpan))
        {
            values.AddRange([TimeSpan.Zero, TimeSpan.MaxValue, TimeSpan.FromMinutes(7)]);
        }
        else if (underlying == typeof(Uri))
        {
            values.AddRange(UriSamples);
        }
        else if (underlying == typeof(CultureInfo))
        {
            values.AddRange([CultureInfo.InvariantCulture, new CultureInfo("en-GB"), new CultureInfo("fr-FR")]);
        }
        else if (underlying == typeof(Encoding))
        {
            values.AddRange([Encoding.UTF8, Encoding.Unicode]);
        }
        else if (underlying.IsEnum)
        {
            values.AddRange(Enum.GetValues(underlying).Cast<object?>().Take(4));
        }
        else if (IsProductType(underlying))
        {
            values.AddRange(NestedInstances(underlying));
        }

        return values;
    }

    private static readonly ThreadLocal<int> NestingDepth = new(() => 0);

    /// <summary>Gets a value indicating whether the type ships in one of the three product assemblies.</summary>
    public static bool IsProductType(Type type) =>
        !type.IsAbstract && type.IsClass && ProductAssemblies.Contains(type.Assembly);

    /// <summary>
    /// Builds a handful of varied instances of a nested product type, so that a member whose whole
    /// state lives behind a read-only aggregate — every extension's <c>Context</c> — is actually varied.
    /// </summary>
    /// <param name="type">The nested type.</param>
    /// <returns>Up to four varied instances.</returns>
    private static List<object?> NestedInstances(Type type)
    {
        if (NestingDepth.Value >= 2)
        {
            return [];
        }

        NestingDepth.Value++;
        try
        {
            return Population(type, cap: 6).Take(6).Cast<object?>().ToList();
        }
        finally
        {
            NestingDepth.Value--;
        }
    }

    /// <summary>Finds every concrete product type assignable to an abstract or interface element type.</summary>
    public static IReadOnlyList<Type> ConcreteFor(Type elementType)
    {
        if (IsProductType(elementType))
        {
            return [elementType];
        }

        return ProductAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && elementType.IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Gets the URI samples. The escaped/unescaped pairs are the load-bearing ones: they are equal
    /// under <see cref="UriFormat.Unescaped"/> yet differ in <see cref="Uri.ToString"/>, which is what
    /// <see cref="HashCodeUtility.Component(Uri)"/> hashes.
    /// </summary>
    public static IReadOnlyList<object?> UriSamples =>
    [
        new Uri("http://example.com/a"),
        new Uri("HTTP://EXAMPLE.COM/A"),
        new Uri("http://example.com/a%2Fb"),
        new Uri("http://example.com/a/b"),
        new Uri("http://example.com/a%20b"),
        new Uri("http://example.com/a b"),
        new Uri("relative/path", UriKind.Relative),
        new Uri("http://example.com/z"),
    ];
}