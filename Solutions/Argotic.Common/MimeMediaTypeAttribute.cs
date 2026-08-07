namespace Argotic.Common;

/// <summary>
/// Associates IANA MIME media type information with a target element. This class cannot be inherited.
/// </summary>
/// <remarks>
///     See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of the registered IANA MIME media types and subtypes.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MimeMediaTypeAttribute : Attribute, IComparable<MimeMediaTypeAttribute>, IEquatable<MimeMediaTypeAttribute>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold a URI that points to the documentation describing the MIME media type.
    /// </summary>
    private Uri? mimeMediaDocumentation;

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeMediaTypeAttribute"/> class.
    /// </summary>
    public MimeMediaTypeAttribute() : base()
    {
    }

    /// <summary>
    /// Gets or sets the location of the documentation describing the MIME media type for the attributed field.
    /// </summary>
    /// <value>An absolute or relative URI as a string, or an <i>empty</i> string if none was specified. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     A value that will not parse as a <see cref="Uri"/> is discarded rather than rejected, so
    ///     reading this property back can yield an <i>empty</i> string after a non-empty set. It is a
    ///     documentation pointer that nothing dereferences, and an attribute argument cannot throw at the
    ///     point a reader would notice.
    /// </remarks>
    public string Documentation
    {
        get => mimeMediaDocumentation?.ToString() ?? string.Empty;
        set => mimeMediaDocumentation = value is not null && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? url) ? url : null;
    }

    /// <summary>
    /// Gets or sets the MIME media type name for the attributed field.
    /// </summary>
    /// <value>The top-level type, such as <c>application</c> or <c>text</c>, trimmed. The default value is an <i>empty</i> string.</value>
    public string Name
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME media subtype name for the attributed field.
    /// </summary>
    /// <value>The subtype, such as <c>rss+xml</c>, trimmed. The default value is an <i>empty</i> string.</value>
    public string SubName
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="MimeMediaTypeAttribute"/>.
    /// </summary>
    /// <returns>The attribute written out as it would appear in source.</returns>
    public override string ToString() => $"""[MimeMediaType(Name = "{this.Name}", SubName = "{this.SubName}", Documentation = "{this.Documentation ?? string.Empty}")]""";

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
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.SubName, other.SubName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="MimeMediaTypeAttribute"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MimeMediaTypeAttribute"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MimeMediaTypeAttribute"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is MimeMediaTypeAttribute other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.SubName));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(MimeMediaTypeAttribute? first, MimeMediaTypeAttribute? second)
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
    public static bool operator !=(MimeMediaTypeAttribute? first, MimeMediaTypeAttribute? second) => !(first == second);

}