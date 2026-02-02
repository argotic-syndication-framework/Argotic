using System.Collections.Frozen;
using System.Reflection;

namespace Argotic.Common;

/// <summary>
/// Associates enumeration field description information with a target element. This class cannot be inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[Serializable]
public sealed class EnumerationMetadataAttribute : Attribute, IComparable<EnumerationMetadataAttribute>, IEquatable<EnumerationMetadataAttribute>, IComparisonOperators
{
    /// <summary>
    ///  Private member to hold the display name for the attributed field.
    /// </summary>
    private string enumMetadataDisplayName = string.Empty;

    /// <summary>
    /// Private member to hold the alternate textual value for the attributed field.
    /// </summary>
    private string enumMetadataAlternateValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumerationMetadataAttribute"/> class.
    /// </summary>
    public EnumerationMetadataAttribute() : base()
    {
    }

    /// <summary>
    /// Gets or sets the alternate textual value for the attributed field.
    /// </summary>
    /// <value>The alternate textual value for the attributed field.</value>
    public string AlternateValue
    {
        get => enumMetadataAlternateValue;
        set => enumMetadataAlternateValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Gets or sets the display name for the attributed field.
    /// </summary>
    /// <value>The display name for the attributed field.</value>
    public string DisplayName
    {
        get => enumMetadataDisplayName;
        set => enumMetadataDisplayName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="EnumerationMetadataAttribute"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="EnumerationMetadataAttribute"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance.
    /// </remarks>
    public override string ToString()
    {
        return $"[EnumerationMetadata(DisplayName = \"{this.DisplayName}\", AlternateValue=\"{this.AlternateValue}\")]";
    }

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
        result |= string.Compare(this.DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="EnumerationMetadataAttribute"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="EnumerationMetadataAttribute"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="EnumerationMetadataAttribute"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is EnumerationMetadataAttribute other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.AlternateValue, this.DisplayName);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Gets the alternate value for the specified enum value using its <see cref="EnumerationMetadataAttribute"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to get the alternate value for.</param>
    /// <returns>The alternate value if found, otherwise an empty string.</returns>
    public static string GetAlternateValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return EnumMetadataCache<TEnum>.EnumToAlternateValue.GetValueOrDefault(value, string.Empty);
    }

    /// <summary>
    /// Gets the enum value that corresponds to the specified alternate value name.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="name">The alternate value name to search for.</param>
    /// <param name="defaultValue">The default value to return if not found.</param>
    /// <returns>The enum value if found, otherwise the default value.</returns>
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
    /// <returns>A FrozenDictionary mapping enum values to alternate value strings.</returns>
    public static FrozenDictionary<TEnum, string> GetAlternateValueMapping<TEnum>() where TEnum : struct, Enum
    {
        return EnumMetadataCache<TEnum>.EnumToAlternateValue;
    }

    /// <summary>
    /// Gets the cached mapping from alternate value strings to enum values (case-insensitive).
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>A FrozenDictionary mapping alternate value strings to enum values.</returns>
    public static FrozenDictionary<string, TEnum> GetEnumByAlternateValueMapping<TEnum>() where TEnum : struct, Enum
    {
        return EnumMetadataCache<TEnum>.AlternateValueToEnum;
    }

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
                    TEnum enumValue = (TEnum)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes is { Length: > 0 } && customAttributes[0] is EnumerationMetadataAttribute enumerationMetadata)
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
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes is { Length: > 0 } && customAttributes[0] is EnumerationMetadataAttribute enumerationMetadata)
                    {
                        if (!string.IsNullOrEmpty(enumerationMetadata.AlternateValue))
                        {
                            TEnum enumValue = (TEnum)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                            mappings[enumerationMetadata.AlternateValue] = enumValue;
                        }
                    }
                }
            }

            return mappings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }
    }
}