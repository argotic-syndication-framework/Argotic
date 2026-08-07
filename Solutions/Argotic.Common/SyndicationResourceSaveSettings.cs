using System.Text;

namespace Argotic.Common;

/// <summary>
/// Specifies a set of features to support on a <see cref="ISyndicationResource"/> object persisted by the <see cref="ISyndicationResource.Save(Stream, SyndicationResourceSaveSettings)"/> method.
/// </summary>
public sealed class SyndicationResourceSaveSettings : IComparable<SyndicationResourceSaveSettings>, IEquatable<SyndicationResourceSaveSettings>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the syndication extensions supported during the save operation.
    /// </summary>
    /// <remarks>
    ///     Declared explicitly rather than using the C# 14 <c>field</c> keyword because
    ///     <see cref="GetHashCode"/> reads it directly: hashing through the property would
    ///     materialize an empty list for settings whose extensions were never touched.
    /// </remarks>
    private List<Type>? supportedSyndicationExtensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceSaveSettings"/> class.
    /// </summary>
    public SyndicationResourceSaveSettings()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating if auto-detection of supported syndication extensions is enabled.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the syndication extensions supported by the save operation are automatically determined based on the syndication extensions added to the syndication resource and its child entities; otherwise, <see langword="false"/>.
    ///     The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    ///     Automatic detection of supported syndication extensions will <i>not</i> remove any syndication extensions already added
    ///     to the <see cref="SupportedExtensions"/> collection prior to the save operation execution.
    /// </remarks>
    public bool AutoDetectExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the character encoding to use when persisting a syndication resource.
    /// </summary>
    /// <value>The encoding to write with. The default value is <see cref="Encoding.UTF8"/>.</value>
    /// <remarks>
    ///     This is what the document's XML declaration will name, so it must be an encoding the readers
    ///     of the feed can decode. It is unrelated to
    ///     <see cref="SyndicationResourceLoadSettings.CharacterEncoding"/>, which is nullable because
    ///     reading has a "detect it" option that writing does not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Encoding CharacterEncoding
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets a value indicating if syndication resource persist operations should attempt to minimize the physical size of the resulting output.
    /// </summary>
    /// <value><see langword="true"/> if output size should be as small as possible; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.</value>
    /// <remarks>
    ///     What it actually controls is indentation: every <c>Save</c> implementation sets
    ///     <see cref="System.Xml.XmlWriterSettings.Indent"/> to the negation of this. The document is
    ///     equally well-formed and carries the same information either way, so the only observable
    ///     difference is its length.
    /// </remarks>
    public bool MinimizeOutputSize { get; set; }

    /// <summary>
    /// Gets the syndication extensions that extend the syndication resource.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="Type"/> objects that represent syndication extension instances used during the save operation.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     During a save operation, each of these syndication extension types is instantiated and used to write the prefixed XML namespace declarations on the root syndication resource entity.
    /// </remarks>
    public IList<Type> SupportedExtensions => supportedSyndicationExtensions ??= [];

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <returns>The settings, with <see cref="SupportedExtensions"/> reduced to its hash code rather than enumerated.</returns>
    public override string ToString() => $"[SyndicationResourceSaveSettings(CharacterEncoding = \"{this.CharacterEncoding.WebName}\", MinimizeOutputSize = \"{this.MinimizeOutputSize}\", Autodetect = \"{this.AutoDetectExtensions}\", SupportedExtensions = \"{this.SupportedExtensions.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo)}\")]";

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SyndicationResourceSaveSettings? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.CharacterEncoding.WebName, other.CharacterEncoding.WebName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.MinimizeOutputSize.CompareTo(other.MinimizeOutputSize);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.SupportedExtensions, other.SupportedExtensions);
        if (result == 0) result = this.AutoDetectExtensions.CompareTo(other.AutoDetectExtensions);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationResourceSaveSettings"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationResourceSaveSettings"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SyndicationResourceSaveSettings"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SyndicationResourceSaveSettings? other)
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
    public override bool Equals(object? obj) => obj is SyndicationResourceSaveSettings other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(HashCodeUtility.Component(this.CharacterEncoding?.WebName));
        hash.Add(HashCodeUtility.Component(this.MinimizeOutputSize));
        hash.Add(HashCodeUtility.Component(this.AutoDetectExtensions));
        var extensions = this.supportedSyndicationExtensions;
        if (extensions is not null)
        {
            hash.Add(HashCodeUtility.Component(extensions.Count));
            foreach (var extension in extensions)
            {
                hash.Add(HashCodeUtility.Component(extension));
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SyndicationResourceSaveSettings? first, SyndicationResourceSaveSettings? second)
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
    public static bool operator !=(SyndicationResourceSaveSettings? first, SyndicationResourceSaveSettings? second) => !(first == second);
}