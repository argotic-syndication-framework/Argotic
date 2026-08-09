using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a permanent, universally unique identifier for an <see cref="AtomEntry"/> or <see cref="AtomFeed"/>.
/// </summary>
/// <seealso cref="AtomEntry.Id"/>
/// <seealso cref="AtomFeed.Id"/>
/// <remarks>
///     <para>
///         <b>The identifier must never change.</b> RFC 4287 §4.2.6.1 holds it constant when a document is relocated, migrated, syndicated, republished,
///         exported or imported: one <see cref="AtomId"/> pertains to every instantiation of a particular <see cref="AtomEntry"/> or <see cref="AtomFeed"/>,
///         and a revision keeps the identifier it had. Store it alongside the resource. Deriving it instead from something that can move — the permalink,
///         a reissued database key, the file path — is how a feed silently republishes its entire back catalogue as new.
///     </para>
///     <para>
///         The value is an IRI (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>) and §4.2.6 excludes relative references. It need not be
///         dereferenceable: <c>tag:</c> and <c>urn:uuid:</c> identifiers are conformant and common in the wild.
///     </para>
///     <para>
///         The content must be created in a way that assures uniqueness. Because IRIs that would be equivalent only after mapping to URIs and dereferencing
///         invite confusion, §4.2.6.1 recommends normalising an identifier once, when it is minted:
///         <list type="bullet">
///             <item>
///                 <description>
///                      Provide the scheme in lowercase characters.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     Provide the host, if any, in lowercase characters.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                    Only perform percent-encoding where it is essential.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     Use uppercase A through F characters when percent-encoding.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                    Prevent dot-segments from appearing in paths.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     For schemes that define a default authority, use an empty authority if the default is desired.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                    For schemes that define an empty path to be equivalent to a path of "/", use "/".
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     For schemes that define a port, use an empty port if the default is desired.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                    Preserve empty fragment identifiers and queries.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                    Ensure that all components of the IRI are appropriately character normalized, e.g., by using NFC or NFKC.
///                 </description>
///             </item>
///         </list>
///     </para>
///     <para>
///         Comparing two instances answers "is this the entry I already have?". §4.2.6 requires that comparison to be character-by-character and
///         case-sensitive, over the IRI strings alone, never by dereferencing them or the URIs they map to. <see cref="CompareTo(AtomId)"/> compares
///         <see cref="Value"/> ordinally for exactly that reason.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomIdExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomId class." />
/// </example>
public class AtomId : IAtomCommonObjectAttributes, IComparable<AtomId>, IEquatable<AtomId>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomId"/> class.
    /// </summary>
    public AtomId()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomId"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">An absolute IRI that permanently and uniquely identifies the entity. Its <see cref="System.Uri.OriginalString"/> becomes <see cref="Value"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    public AtomId(Uri uri)
    {
        this.Uri = uri;
    }

    /// <summary>
    /// Gets or sets the permanent, universally unique identifier exactly as its characters appear.
    /// </summary>
    /// <value>The identifier exactly as its characters appear. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     <para>
    ///     This is the identity the specification means. RFC 4287 §4.2.6 requires processors to
    ///     compare atom:id values "on a character-by-character basis (in a case-sensitive fashion)",
    ///     §2 forbids mapping an IRI serving as an atom:id to a URI, and §4.2.6.1 says the value
    ///     "MUST NOT change". <see cref="System.Uri"/> honours none of that — it lowercases the
    ///     scheme and host and rewrites percent-encoding — so identity, loading and writing all run
    ///     through this property, and <see cref="Uri"/> is a parsed convenience derived from it.
    ///     </para>
    ///     <para>
    ///     Setting <see cref="Uri"/> stores <see cref="System.Uri.OriginalString"/> here: the
    ///     characters the caller actually supplied, before the class had opinions about them.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public string Value
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = string.Empty;

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
    /// Gets or sets the identifier parsed as a <see cref="System.Uri"/>.
    /// </summary>
    /// <value>The identifier as a <see cref="Uri"/>, or <see langword="null"/> when the loaded value was not an absolute IRI this runtime could parse.</value>
    /// <remarks>
    ///     <para>
    ///         A convenience derived from <see cref="Value"/>, which is the identity. §4.2.6 excludes relative references, so this holds an
    ///         <i>absolute</i> IRI (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>) or nothing at all — a document whose <c>atom:id</c>
    ///         is unparseable still loads, with <see cref="Value"/> carrying the characters verbatim and this property left <see langword="null"/>.
    ///     </para>
    ///     <para>
    ///         Setting it writes <see cref="System.Uri.OriginalString"/> to <see cref="Value"/>, so the characters the caller supplied survive rather than
    ///         the normalised form <see cref="System.Uri"/> would otherwise hand back.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Uri
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
            this.Value = value.OriginalString;
        }
    }

    /// <summary>
    /// Loads this <see cref="AtomId"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomId"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomId"/>.
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
            // The characters are the identity, so they are kept verbatim; the parsed Uri is a
            // convenience whose normalisations must not leak back into Value. Value is assigned
            // AFTER the Uri property, whose setter would otherwise overwrite it with OriginalString -
            // the same characters here, but the order states the rule. An id System.Uri cannot parse
            // (a relative reference, say) is invalid Atom but still loads: dropping it silently lost
            // the one value the spec says must never change.
            if (Uri.TryCreate(source.Value, UriKind.Absolute, out Uri? uri))
            {
                this.Uri = uri;
            }

            this.Value = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomId"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomId"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomId"/>.
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
    /// Saves the current <see cref="AtomId"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("id", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteString(this.Value);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomId"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomId"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomId"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomId? other)
    {
        if (other is null)
        {
            return 1;
        }

        // Ordinal over the character string, per §4.2.6 - Uri.Compare unified ids the spec's own
        // §4.2.6.2 examples list as distinct (case-differing hosts and schemes, percent-encodings).
        int result = string.CompareOrdinal(this.Value, other.Value);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomId"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomId"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomId"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomId? other)
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
    public override bool Equals(object? obj) => obj is AtomId other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Value));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomId? first, AtomId? second)
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
    public static bool operator !=(AtomId? first, AtomId? second) => !(first == second);

}