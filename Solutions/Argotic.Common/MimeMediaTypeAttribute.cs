namespace Argotic.Common;

/// <summary>
/// Associates IANA MIME media type information with a target element. This class cannot be inherited.
/// </summary>
/// <remarks>
///     See <a href="http://www.iana.org/assignments/media-types">http://www.iana.org/assignments/media-types</a> for a listing of the registered IANA MIME media types and subtypes.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[Serializable]
public sealed class MimeMediaTypeAttribute : Attribute, IComparable<MimeMediaTypeAttribute>, IEquatable<MimeMediaTypeAttribute>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the MIME media type name.
    /// </summary>
    private string mimeMediaTypeName = string.Empty;
    /// <summary>
    /// Private member to hold the MIME media subtype name.
    /// </summary>
    private string mimeMediaSubTypeName = string.Empty;
    /// <summary>
    /// Private member to hold a URI that points to the documentation the describes the MIME media type.
    /// </summary>
    private Uri mimeMediaDocumentation;

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeMediaTypeAttribute"/> class.
    /// </summary>
    public MimeMediaTypeAttribute() : base()
    {
    }

    /// <summary>
    /// Gets or sets a URI that points to the documentation the describes the MIME media type for the attributed field.
    /// </summary>
    /// <value>A <see cref="Uri"/> that points to the documentation the describes the MIME media type for the attributed field.</value>
    public string Documentation
    {
        get => mimeMediaDocumentation?.ToString() ?? string.Empty;
        set => mimeMediaDocumentation = value != null && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri url) ? url : null;
    }

    /// <summary>
    /// Gets or sets the MIME media type name for the attributed field.
    /// </summary>
    /// <value>The MIME media type name for the attributed field.</value>
    public string Name
    {
        get => mimeMediaTypeName;
        set => mimeMediaTypeName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Gets or sets the MIME media subtype name for the attributed field.
    /// </summary>
    /// <value>The MIME media subtype name for the attributed field.</value>
    public string SubName
    {
        get => mimeMediaSubTypeName;
        set => mimeMediaSubTypeName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="MimeMediaTypeAttribute"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="MimeMediaTypeAttribute"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance.
    /// </remarks>
    public override string ToString()
    {
        return $"[MimeMediaType(Name = \"{this.Name}\", SubName = \"{this.SubName}\", Documentation = \"{this.Documentation ?? string.Empty}\")]";
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(MimeMediaTypeAttribute? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Documentation, other.Documentation, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.SubName, other.SubName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="MimeMediaTypeAttribute"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MimeMediaTypeAttribute"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="MimeMediaTypeAttribute"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(MimeMediaTypeAttribute? other)
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
        return obj is MimeMediaTypeAttribute other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Documentation, this.Name, this.SubName);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
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
    public static bool operator !=(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
    {
        return !(first == second);
    }

}