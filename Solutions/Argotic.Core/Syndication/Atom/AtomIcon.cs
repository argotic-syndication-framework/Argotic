using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents an image that provides iconic visual identification for an <see cref="AtomFeed"/>.
/// </summary>
/// <seealso cref="AtomFeed.Icon"/>
/// <remarks>
///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomIconExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomIcon class." />
/// </example>
public class AtomIcon : IAtomCommonObjectAttributes, IComparable<AtomIcon>, IEquatable<AtomIcon>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomIcon"/> class.
    /// </summary>
    public AtomIcon()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomIcon"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">An IRI reference locating the image.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    public AtomIcon(Uri uri)
    {
        this.Uri = uri;
    }

    /// <summary>
    /// Gets or sets the base against which relative references inside this element are resolved.
    /// </summary>
    /// <value>The <c>xml:base</c> in effect for this element, or <see langword="null"/> when none is. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §2 gives <c>xml:base</c> the function described in section 5.1.1 of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986: Uniform Resource Identifier (URI): Generic Syntax</a> — it establishes the base URI,
    ///         or IRI, for every relative reference in the attribute's effective scope. The value itself is a URI reference after processing according to
    ///         <a href="https://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.
    ///     </para>
    ///     <para>
    ///         Loading resolves inheritance: an element without an <c>xml:base</c> of its own reports the nearest ancestor's, so the value here is the
    ///         <i>effective</i> base a consumer can resolve an href against, not the literal attribute.
    ///     </para>
    /// </remarks>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>The language declared by <c>xml:lang</c>, or <see langword="null"/> when none is in scope. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 defines <c>atomLanguageTag</c> as a language identifier per
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066 (BCP 47; now RFC 5646)</a>, or its successor. A tag this runtime cannot turn
    ///         into a <see cref="CultureInfo"/> is traced and dropped rather than failing the load.
    ///     </para>
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Extensions"/> holds at least one <see cref="ISyndicationExtension"/>; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets an IRI that identifies an image that provides iconic visual identification for this feed.
    /// </summary>
    /// <value>The element's content as an IRI reference locating the image. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         An IRI reference (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>), so it may be relative and resolved against
    ///         <see cref="BaseUri"/>. The image is expected to be roughly 1:1 — a favicon-sized mark — but that is a <i>should</i>, not a constraint anything
    ///         here enforces, and nothing fetches or measures the image.
    ///     </para>
    ///     <para>See <see cref="Uri"/> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Uri
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="AtomIcon"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomIcon"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomIcon"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
            wasLoaded = true;
        }
        if (!string.IsNullOrEmpty(source.Value))
        {
            if (Uri.TryCreate(source.Value, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                this.Uri = uri;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomIcon"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomIcon"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomIcon"/>.
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
    /// Saves the current <see cref="AtomIcon"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("icon", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteString(this.Uri?.ToString() ?? string.Empty);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomIcon"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomIcon"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomIcon"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomIcon? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomIcon"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomIcon"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomIcon"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomIcon? other)
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
    public override bool Equals(object? obj) => obj is AtomIcon other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Uri));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomIcon? first, AtomIcon? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(AtomIcon? first, AtomIcon? second) => !(first == second);

}