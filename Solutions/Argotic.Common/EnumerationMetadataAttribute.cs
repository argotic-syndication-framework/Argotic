namespace Argotic.Common;

/// <summary>
/// Associates enumeration field description information with a target element. This class cannot be inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[Serializable]
public sealed class EnumerationMetadataAttribute : Attribute, IComparable<EnumerationMetadataAttribute>, IEquatable<EnumerationMetadataAttribute>
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
        get
        {
            return enumMetadataAlternateValue;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                enumMetadataAlternateValue = string.Empty;
            }
            else
            {
                enumMetadataAlternateValue = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets the display name for the attributed field.
    /// </summary>
    /// <value>The display name for the attributed field.</value>
    public string DisplayName
    {
        get
        {
            return enumMetadataDisplayName;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                enumMetadataDisplayName = string.Empty;
            }
            else
            {
                enumMetadataDisplayName = value.Trim();
            }
        }
    }

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="EnumerationMetadataAttribute"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="EnumerationMetadataAttribute"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance.
    /// </remarks>
    public override string ToString()
    {
        return string.Format(null, "[EnumerationMetadata(DisplayName = \"{0}\", AlternateValue=\"{1}\")]", this.DisplayName, this.AlternateValue);
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
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(EnumerationMetadataAttribute first, EnumerationMetadataAttribute second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}