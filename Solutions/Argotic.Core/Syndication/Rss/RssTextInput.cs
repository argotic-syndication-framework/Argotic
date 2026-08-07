using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a single-field HTML form that a reader may render alongside a <see cref="RssFeed">feed</see>, submitting the query to a CGI script of the publisher's choosing.
/// </summary>
/// <remarks>
///     <para>
///         The RSS 2.0 specification is unusually candid about this element: "The purpose of the
///         <c>&lt;textInput&gt;</c> element is something of a mystery. You can use it to specify a search
///         engine box. Or to allow a reader to provide feedback. Most aggregators ignore it."
///     </para>
///     <para>
///         Treat it as a round-trip concern rather than a feature. It is supported here so that a document
///         carrying one survives being read and written back unchanged; there is very little reason to add
///         one to a new feed.
///     </para>
/// </remarks>
/// <seealso cref="RssChannel.TextInput"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssTextInputExample.cs" language="cs" title="The following code example demonstrates the usage of the RssTextInput class." />
/// </example>
public class RssTextInput : IComparable<RssTextInput>, IEquatable<RssTextInput>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RssTextInput"/> class.
    /// </summary>
    public RssTextInput()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssTextInput"/> class using the supplied description, link, name, and title.
    /// </summary>
    /// <param name="description">Character data that provides a human-readable label explaining this form's purpose.</param>
    /// <param name="link">A <see cref="Uri"/> that represents the URL of the CGI script that handles the query.</param>
    /// <param name="name">The name of the form component that contains the query.</param>
    /// <param name="title">A string value that labels the button used to submit the query.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="description"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="title"/> is an empty string.</exception>
    public RssTextInput(string description, Uri link, string name, string title)
    {
        this.Description = description;
        this.Link = link;
        this.Name = name;
        this.Title = title;
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
    /// Gets or sets the text explaining what the form is for.
    /// </summary>
    /// <value>The <c>description</c> sub-element. Required by the specification. The default value is an <i>empty</i> string.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Description
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the CGI script that handles the query.
    /// </summary>
    /// <value>The <c>link</c> sub-element, or <see langword="null"/> if none was specified. Required by the specification.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Link
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the form field that carries the query.
    /// </summary>
    /// <value>The <c>name</c> sub-element — the HTML input name, not a label. Required by the specification. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     This becomes an HTML attribute name, so it must begin with a letter and contain only letters
    ///     A to Z in either case, digits, colons, hyphens, periods and underscores. Nothing here validates
    ///     that, and an invalid name produces a form the reader cannot submit rather than a load failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the caption of the submit button.
    /// </summary>
    /// <value>The <c>title</c> sub-element. Required by the specification. The default value is an <i>empty</i> string.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Title
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <see langword="true"/> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is <see langword="null"/>.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (ISyndicationExtension extension in this.Extensions)
        {
            if (match(extension))
            {
                return extension;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads this <see cref="RssTextInput"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="RssTextInput"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssTextInput"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator? descriptionNavigator = source.SelectChildElement("description");
        XPathNavigator? linkNavigator = source.SelectChildElement("link");
        XPathNavigator? nameNavigator = source.SelectChildElement("name");
        XPathNavigator? titleNavigator = source.SelectChildElement("title");

        if (descriptionNavigator is not null)
        {
            if (!string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }
        }
        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }
        if (nameNavigator is not null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                this.Name = nameNavigator.Value;
                wasLoaded = true;
            }
        }
        if (titleNavigator is not null)
        {
            if (!string.IsNullOrEmpty(titleNavigator.Value))
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssTextInput"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="RssTextInput"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssTextInput"/>.
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
    /// Saves the current <see cref="RssTextInput"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("textInput");

        writer.WriteElementString("description", this.Description);
        writer.WriteElementString("link", this.Link?.ToString() ?? string.Empty);
        writer.WriteElementString("name", this.Name);
        writer.WriteElementString("title", this.Title);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssTextInput"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssTextInput"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssTextInput"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssTextInput? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssTextInput"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssTextInput"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RssTextInput"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RssTextInput? other)
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
    public override bool Equals(object? obj) => obj is RssTextInput other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Description ?? string.Empty),
            HashCodeUtility.Component(this.Link),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Title ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(RssTextInput? first, RssTextInput? second)
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
    public static bool operator !=(RssTextInput? first, RssTextInput? second) => !(first == second);
}