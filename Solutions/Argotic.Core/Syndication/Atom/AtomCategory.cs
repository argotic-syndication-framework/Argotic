using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents information about a category associated with a <see cref="AtomEntry"/> or <see cref="AtomFeed"/>.
/// </summary>
/// <seealso cref="AtomEntry.Categories"/>
/// <seealso cref="AtomFeed.Categories"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the AtomCategory class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Atom\AtomCategoryExample.cs"
///             region="AtomCategory"
///         />
///     </code>
/// </example>
[Serializable]
public class AtomCategory : IAtomCommonObjectAttributes, IComparable<AtomCategory>, IEquatable<AtomCategory>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomCategory"/> class.
    /// </summary>
    public AtomCategory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomCategory"/> class using the supplied term.
    /// </summary>
    /// <param name="term">A string that identifies this category.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="term"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="term"/> is an empty string.</exception>
    public AtomCategory(string term)
    {
        this.Term = term;
    }

    /// <summary>
    /// Gets or sets the base URI other than the base URI of the document or external entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
    ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
    /// </remarks>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
    ///     </para>
    /// </remarks>
    public CultureInfo Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets the human-readable label of this category for display in end-user applications.
    /// </summary>
    /// <value>The human-readable label of this category for display in end-user applications.</value>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Label"/> property is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
    ///         Entities represent their corresponding characters, not markup.
    ///     </para>
    /// </remarks>
    public string Label
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets an IRI that identifies the categorization scheme used by this category.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents an Internationalized Resource Identifier (IRI) that identifies the categorization scheme used by this category.</value>
    /// <remarks>
    ///     <para>See <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
    ///     <para>See <a href="http://msdn2.microsoft.com/en-us/library/system.uri.aspx">System.Uri</a> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    public Uri Scheme { get; set; }

    /// <summary>
    /// Gets or sets a string that identifies this category.
    /// </summary>
    /// <value>A string that identifies this category.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Term
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="AtomCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="AtomCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
            wasLoaded = true;
        }

        if (source.HasAttributes)
        {
            string termAttribute = source.GetAttribute("term", string.Empty);
            string schemeAttribute = source.GetAttribute("scheme", string.Empty);
            string labelAttribute = source.GetAttribute("label", string.Empty);

            if (!string.IsNullOrEmpty(termAttribute))
            {
                this.Term = termAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(schemeAttribute))
            {
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri scheme))
                {
                    this.Scheme = scheme;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(labelAttribute))
            {
                this.Label = labelAttribute;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomCategory"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="AtomCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);

        bool wasLoaded = this.Load(source);

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="AtomCategory"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("category", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteAttributeString("term", this.Term);

        if (this.Scheme != null)
        {
            writer.WriteAttributeString("scheme", this.Scheme.ToString());
        }

        if (!string.IsNullOrEmpty(this.Label))
        {
            writer.WriteAttributeString("label", this.Label);
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomCategory"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomCategory"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomCategory"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.Term, other.Term, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomCategory"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="AtomCategory"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(AtomCategory? other)
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
        return obj is AtomCategory other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Scheme), HashCodeUtility.Component(this.Term));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(AtomCategory first, AtomCategory second)
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
    public static bool operator !=(AtomCategory first, AtomCategory second)
    {
        return !(first == second);
    }

}