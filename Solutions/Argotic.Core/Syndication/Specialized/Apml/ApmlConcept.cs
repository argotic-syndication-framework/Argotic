using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents an area of interest that can be associated to an <see cref="ApmlProfile"/>.
/// </summary>
/// <seealso cref="ApmlProfile.ExplicitConcepts"/>
/// <seealso cref="ApmlProfile.ImplicitConcepts"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Apml\ApmlConceptExample.cs" language="cs" title="The following code example demonstrates the usage of the ApmlConcept class." />
/// </example>
public class ApmlConcept : IComparable<ApmlConcept>, IEquatable<ApmlConcept>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlConcept"/> class.
    /// </summary>
    public ApmlConcept()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlConcept"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this concept.</param>
    /// <param name="value">The decimal score of this concept.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <i>explicit</i> concept. Explicit data is for items that are explicitly added by a user to represent something.
    ///     For example, a user could edit their own APML file and add items they know they're interested in.
    ///     For this reason the <see cref="From"/> and <see cref="UpdatedOn"/> properties are not necessary for explicit data items, because it's a manual process.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    public ApmlConcept(string key, decimal value)
    {
        this.Key = key;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlConcept"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this concept.</param>
    /// <param name="value">The decimal score of this concept.</param>
    /// <param name="from">The name of the entity that contributed this concept.</param>
    /// <param name="utcUpdatedOn">A <see cref="DateTime"/> object that indicates the last time this concept was updated.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <i>implicit</i> concept. Implicit data is added by machines/computers that try to make
    ///     some informed guesses about the things that you are interested in. This stuff will change over time and are added with a certain degree of confidence
    ///     that may have a decay in certain applications. For this reason it is important to keep a track of when things were added/modified.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="from"/> is an empty string.</exception>
    public ApmlConcept(string key, decimal value, string from, DateTime utcUpdatedOn) : this(key, value)
    {
        ArgumentException.ThrowIfNullOrEmpty(from);
        this.From = from;
        this.UpdatedOn = utcUpdatedOn;
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets the name of the entity that contributed this concept.
    /// </summary>
    /// <value>The <c>from</c> attribute — which service or algorithm inferred this — or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     Meaningful only for implicit concepts, where a consumer needs to know whose guess it is reading.
    ///     Explicit concepts came from the user and leave it empty.
    /// </remarks>
    public string From
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the unique key for this concept.
    /// </summary>
    /// <value>The <c>key</c> attribute — the term itself, such as <c>syndication</c>. It is the identity of the concept; APML defines no vocabulary for it.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Key
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating the last time this concept was updated.
    /// </summary>
    /// <value>The <c>updated</c> attribute, written as RFC 3339. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update date was specified, and suppresses the attribute on save.</value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time. It is what lets a consumer decay an inferred score:
    ///     an implicit concept last touched two years ago says less about present interest than one touched
    ///     yesterday.
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the decimal score of this concept.
    /// </summary>
    /// <value>
    ///     The <c>value</c> attribute: a score in the closed range <c>-1</c> to <c>1</c>, where <c>1</c> is
    ///     complete interest and <c>-1</c> complete aversion. The default is <see langword="null"/>, meaning
    ///     no score is known, which suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         <see langword="null"/> and <c>0</c> are different answers and are serialised differently. Zero
    ///         is a real score — indifference, asserted — while <see langword="null"/> is the absence of one.
    ///         An unscored concept therefore omits the attribute rather than writing a number, because in an
    ///         attention-profiling format the score <i>is</i> the payload and any number invented here would
    ///         be read as a claim the profile never made.
    ///     </para>
    ///     <para>
    ///         <b>Omitting the attribute is a known, deliberate deviation from the schema.</b> APML 0.6
    ///         declares <c>value</c> as <c>use="required"</c> on <c>ExplicitNodeType</c>, typed
    ///         <c>NodeValueType</c> — an <c>xs:decimal</c> restricted to <c>[-1, 1]</c>. That type has no
    ///         spelling for "unknown", so a node whose score was never established cannot be serialised
    ///         conformantly at all, and the only question is which non-conformance to prefer.
    ///     </para>
    ///     <para>
    ///         Omission is preferred over the two alternatives. The previous behaviour — defaulting to
    ///         <see cref="decimal.MinValue"/>, a value this setter would itself reject, and writing it
    ///         unconditionally as <c>-79228162514264337593543950335.00</c> — is non-conformant <i>and</i>
    ///         asserts a score twenty-nine orders of magnitude outside the declared range. Writing
    ///         <c>0.00</c> would be schema-valid but would fabricate a neutral-interest claim the profile
    ///         never made, and in an attention-profiling format the score is the entire payload. A missing
    ///         attribute is the only one of the three a consumer can recognise as missing.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is greater than 1.</exception>
    public decimal? Value
    {
        get;
        set
        {
            if (value.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.Value, decimal.MinusOne);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Value, decimal.One);
            }

            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="ApmlConcept"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="ApmlConcept"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlConcept"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string keyAttribute = source.GetAttribute("key", string.Empty);
            string valueAttribute = source.GetAttribute("value", string.Empty);
            string fromAttribute = source.GetAttribute("from", string.Empty);
            string updatedAttribute = source.GetAttribute("updated", string.Empty);

            if (!string.IsNullOrEmpty(keyAttribute))
            {
                this.Key = keyAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(valueAttribute))
            {
                if (decimal.TryParse(valueAttribute, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out decimal value))
                {
                    if (value is >= decimal.MinusOne and <= decimal.One)
                    {
                        this.Value = value;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(fromAttribute))
            {
                this.From = fromAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(updatedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedAttribute, out DateTime updatedOn))
                {
                    this.UpdatedOn = updatedOn;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlConcept"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="ApmlConcept"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlConcept"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ApmlConcept"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Concept", ApmlUtility.ApmlNamespace);

        // key is use="required" and typed xs:string, for which the empty string is a legal value, so an
        // empty key must still be written: omitting it is invalid, emitting it empty is not.
        writer.WriteAttributeString("key", this.Key);

        if (this.Value.HasValue)
        {
            writer.WriteAttributeString("value", this.Value.Value.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        if (!string.IsNullOrEmpty(this.From))
        {
            writer.WriteAttributeString("from", this.From);
        }

        if (this.UpdatedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("updated", ApmlUtility.ToApmlDateTime(this.UpdatedOn));
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ApmlConcept"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlConcept"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ApmlConcept? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.From, other.From, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Key, other.Key, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.UpdatedOn.CompareTo(other.UpdatedOn);
        if (result == 0) result = Nullable.Compare(this.Value, other.Value);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApmlConcept"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ApmlConcept"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="ApmlConcept"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ApmlConcept? other)
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
    public override bool Equals(object? obj) => obj is ApmlConcept other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.From ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Key ?? string.Empty),
            HashCodeUtility.Component(this.UpdatedOn),
            HashCodeUtility.Component(this.Value));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(ApmlConcept? first, ApmlConcept? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(ApmlConcept? first, ApmlConcept? second) => !(first == second);
}