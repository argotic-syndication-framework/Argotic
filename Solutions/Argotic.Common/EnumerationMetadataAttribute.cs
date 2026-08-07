using System.Collections.Frozen;
using System.Reflection;

namespace Argotic.Common;

/// <summary>
/// Associates enumeration field description information with a target element. This class cannot be inherited.
/// </summary>
/// <remarks>
///     The <see cref="AlternateValue"/> is the wire form: for <see cref="SyndicationContentFormat"/> it is
///     the document's root element name, which is how a format is recognised from a parsed document. The
///     lookups are built once per enumeration type and cached in a <see cref="System.Collections.Frozen.FrozenDictionary{TKey, TValue}"/>,
///     so reflection runs on first use and never again.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EnumerationMetadataAttribute : Attribute, IComparable<EnumerationMetadataAttribute>, IEquatable<EnumerationMetadataAttribute>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumerationMetadataAttribute"/> class.
    /// </summary>
    public EnumerationMetadataAttribute() : base()
    {
    }

    /// <summary>
    /// Gets or sets the alternate textual value for the attributed field.
    /// </summary>
    /// <value>The alternate textual value, trimmed, or an <i>empty</i> string if none was specified. The default value is an <i>empty</i> string.</value>
    public string AlternateValue
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the attributed field.
    /// </summary>
    /// <value>The display name, trimmed, or an <i>empty</i> string if none was specified. The default value is an <i>empty</i> string.</value>
    public string DisplayName
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="EnumerationMetadataAttribute"/>.
    /// </summary>
    /// <returns>The attribute written out as it would appear in source.</returns>
    public override string ToString() => $"""[EnumerationMetadata(DisplayName = "{this.DisplayName}", AlternateValue="{this.AlternateValue}")]""";

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(EnumerationMetadataAttribute? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.AlternateValue, other.AlternateValue, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="EnumerationMetadataAttribute"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="EnumerationMetadataAttribute"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="EnumerationMetadataAttribute"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(EnumerationMetadataAttribute? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is EnumerationMetadataAttribute other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.AlternateValue), HashCodeUtility.Component(this.DisplayName));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(EnumerationMetadataAttribute? first, EnumerationMetadataAttribute? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(EnumerationMetadataAttribute? first, EnumerationMetadataAttribute? second) => !(first == second);

    /// <summary>
    /// Gets the alternate value for the specified enum value using its <see cref="EnumerationMetadataAttribute"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to get the alternate value for.</param>
    /// <returns>The alternate value; otherwise, an <i>empty</i> string, both for a member carrying no <see cref="EnumerationMetadataAttribute"/> and for one whose attribute names no alternate value.</returns>
    public static string GetAlternateValue<TEnum>(TEnum value) where TEnum : struct, Enum => EnumMetadataCache<TEnum>.EnumToAlternateValue.GetValueOrDefault(value, string.Empty);

    /// <summary>
    /// Gets the enum value that corresponds to the specified alternate value name.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="name">The alternate value name to search for. Matched without regard to case.</param>
    /// <param name="defaultValue">The value to return when <paramref name="name"/> matches nothing, or is <see langword="null"/> or empty.</param>
    /// <returns>The matching enum value; otherwise, <paramref name="defaultValue"/>.</returns>
    public static TEnum GetEnumByAlternateValue<TEnum>(string name, TEnum defaultValue) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(name))
        {
            return defaultValue;
        }

        return EnumMetadataCache<TEnum>.AlternateValueToEnum.GetValueOrDefault(name, defaultValue);
    }

    /// <summary>
    /// Gets the cached mapping from enum values to their alternate value strings.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>The cached mapping. Members carrying no <see cref="EnumerationMetadataAttribute"/> are absent from it.</returns>
    public static FrozenDictionary<TEnum, string> GetAlternateValueMapping<TEnum>() where TEnum : struct, Enum => EnumMetadataCache<TEnum>.EnumToAlternateValue;

    /// <summary>
    /// Gets the cached mapping from alternate value strings to enum values, keyed without regard to case.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>The cached mapping. Members whose alternate value is empty are absent from it, so it is not the exact inverse of <see cref="GetAlternateValueMapping{TEnum}"/>.</returns>
    public static FrozenDictionary<string, TEnum> GetEnumByAlternateValueMapping<TEnum>() where TEnum : struct, Enum => EnumMetadataCache<TEnum>.AlternateValueToEnum;

    /// <summary>
    /// Provides cached FrozenDictionary mappings for enum metadata lookups.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    private static class EnumMetadataCache<TEnum> where TEnum : struct, Enum
    {
        /// <summary>
        /// Maps enum values to their alternate value strings.
        /// </summary>
        public static readonly FrozenDictionary<TEnum, string> EnumToAlternateValue = BuildEnumToAlternateValueMapping();

        /// <summary>
        /// Maps alternate value strings to their corresponding enum values (case-insensitive).
        /// </summary>
        public static readonly FrozenDictionary<string, TEnum> AlternateValueToEnum = BuildAlternateValueToEnumMapping();

        private static FrozenDictionary<TEnum, string> BuildEnumToAlternateValueMapping()
        {
            var mappings = new Dictionary<TEnum, string>();

            foreach (FieldInfo fieldInfo in typeof(TEnum).GetFields())
            {
                if (fieldInfo.FieldType == typeof(TEnum))
                {
                    TEnum enumValue = Enum.Parse<TEnum>(fieldInfo.Name);

                    if (fieldInfo.GetCustomAttribute<EnumerationMetadataAttribute>(inherit: false) is { } enumerationMetadata)
                    {
                        mappings[enumValue] = enumerationMetadata.AlternateValue;
                    }
                }
            }

            return mappings.ToFrozenDictionary();
        }

        private static FrozenDictionary<string, TEnum> BuildAlternateValueToEnumMapping()
        {
            var mappings = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

            foreach (FieldInfo fieldInfo in typeof(TEnum).GetFields())
            {
                if (fieldInfo.FieldType == typeof(TEnum))
                {
                    if (fieldInfo.GetCustomAttribute<EnumerationMetadataAttribute>(inherit: false) is { } enumerationMetadata)
                    {
                        if (!string.IsNullOrEmpty(enumerationMetadata.AlternateValue))
                        {
                            mappings[enumerationMetadata.AlternateValue] = Enum.Parse<TEnum>(fieldInfo.Name);
                        }
                    }
                }
            }

            return mappings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }
    }
}