using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a form to submit a text query to a <see cref="RssFeed">feed's</see> publisher over the Common Gateway Interface (CGI).
/// </summary>
/// <seealso cref="RssChannel.TextInput"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssTextInput class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssTextInputExample.cs" 
///             region="RssTextInput" 
///         />
///     </code>
/// </example>
[Serializable]
public class RssTextInput : IComparable<RssTextInput>, IEquatable<RssTextInput>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Private member to hold character data that provides a human-readable label explaining the form's purpose.
    /// </summary>
    private string textInputDescription = string.Empty;

    /// <summary>
    /// Private member to hold the URL of the CGI script that handles the query.
    /// </summary>
    private Uri textInputLink;

    /// <summary>
    /// Private member to hold the name of the form component that contains the query.
    /// </summary>
    private string textInputName = string.Empty;

    /// <summary>
    /// Private member to hold a value that labels the button used to submit the query.
    /// </summary>
    private string textInputTitle = string.Empty;

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
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is an empty string.</exception>
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
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets character data that provides a human-readable label explaining this form's purpose.
    /// </summary>
    /// <value>Character data that provides a human-readable label explaining this form's purpose.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Description
    {
        get => textInputDescription;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            textInputDescription = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the URL of the CGI script that handles the query.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the CGI script that handles the query.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Link
    {
        get => textInputLink;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            textInputLink = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the form component that contains the query.
    /// </summary>
    /// <value>The name of the form component that contains the query.</value>
    /// <remarks>
    ///     The value of this property <b>must</b> begin with a letter and contain only these characters: 
    ///     the letters A to Z in either case, numeric digits, colons (":"), hyphens ("-"), periods (".") and underscores ("_").
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get => textInputName;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            textInputName = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets a value that labels the button used to submit the query.
    /// </summary>
    /// <value>A string value that labels the button used to submit the query.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Title
    {
        get => textInputTitle;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            textInputTitle = value.Trim();
        }
    }

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
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
    /// <returns><b>true</b> if the <see cref="RssTextInput"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssTextInput"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator? descriptionNavigator = source.SelectSingleNode("description", manager);
        XPathNavigator? linkNavigator = source.SelectSingleNode("link", manager);
        XPathNavigator? nameNavigator = source.SelectSingleNode("name", manager);
        XPathNavigator? titleNavigator = source.SelectSingleNode("title", manager);

        if (descriptionNavigator != null)
        {
            if (!string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }
        }
        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }
        if (nameNavigator != null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                this.Name = nameNavigator.Value;
                wasLoaded = true;
            }
        }
        if (titleNavigator != null)
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
    /// <returns><b>true</b> if the <see cref="RssTextInput"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssTextInput"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// <returns><b>true</b> if the specified <see cref="RssTextInput"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is RssTextInput other && this.Equals(other);
    }

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
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(RssTextInput? first, RssTextInput? second)
    {
        return !(first == second);
    }
}