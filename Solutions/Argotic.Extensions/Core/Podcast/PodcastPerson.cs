using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a person involved in a podcast or in one of its episodes.
/// </summary>
/// <remarks>
///     <para>
///     Podcasting 2.0's <c>podcast:person</c>, present in <b>2.4%</b> of 1,934 live feeds surveyed.
///     </para>
///     <para>
///     The specification is explicit about one behaviour that is easy to get wrong: people declared on
///     an item <b>replace</b> the channel's people for that episode rather than adding to them. That is
///     a consumer's merge rule, not a parsing rule, so this type records what each level declared and
///     leaves the substitution to the caller — inventing the merged list here would destroy the
///     distinction the feed drew.
///     </para>
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.People"/>
public class PodcastPerson : IComparable<PodcastPerson>, IEquatable<PodcastPerson>, IComparisonOperators
{
    /// <summary>
    /// The role assumed when a person declares none.
    /// </summary>
    public const string DefaultRole = "host";

    /// <summary>
    /// The group assumed when a person declares none.
    /// </summary>
    public const string DefaultGroup = "cast";

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastPerson"/> class.
    /// </summary>
    public PodcastPerson()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastPerson"/> class using the supplied name.
    /// </summary>
    /// <param name="name">The person's full name or alias.</param>
    public PodcastPerson(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets or sets the person's full name or alias.
    /// </summary>
    /// <value>The name. The default value is an <i>empty</i> string.</value>
    /// <remarks>Required by the specification, which says it cannot be blank.</remarks>
    public string Name
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the role this person serves on the show or episode.
    /// </summary>
    /// <value>The role, or an <i>empty</i> string if none was specified — in which case <see cref="DefaultRole"/> is implied.</value>
    /// <remarks>
    ///     The specification points at the Podcast Taxonomy Project's list, which is maintained
    ///     separately and changes without the namespace changing. Modelling it as an enumeration would
    ///     therefore mean silently dropping any role added after this library shipped, so it is a string.
    /// </remarks>
    public string Role
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the group this person's role belongs to.
    /// </summary>
    /// <value>The group, or an <i>empty</i> string if none was specified — in which case <see cref="DefaultGroup"/> is implied.</value>
    public string Group
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the location of a picture or avatar of this person.
    /// </summary>
    /// <value>A <see cref="Uri"/>, or <see langword="null"/> if none was specified.</value>
    public Uri? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the location of a relevant resource about this person, such as a homepage or profile.
    /// </summary>
    /// <value>A <see cref="Uri"/>, or <see langword="null"/> if none was specified.</value>
    public Uri? Url { get; set; }

    /// <summary>
    /// Loads this <see cref="PodcastPerson"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="PodcastPerson"/> was initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);
        bool wasLoaded = false;

        string roleAttribute = source.GetAttribute("role", string.Empty);
        string groupAttribute = source.GetAttribute("group", string.Empty);

        if (!string.IsNullOrEmpty(roleAttribute))
        {
            this.Role = roleAttribute;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(groupAttribute))
        {
            this.Group = groupAttribute;
            wasLoaded = true;
        }

        Uri? imageUrl = PodcastExtensionUtility.ReadUriAttribute(source, "img");
        if (imageUrl is not null)
        {
            this.ImageUrl = imageUrl;
            wasLoaded = true;
        }

        Uri? url = PodcastExtensionUtility.ReadUriAttribute(source, "href");
        if (url is not null)
        {
            this.Url = url;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Name = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastPerson"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("person", PodcastSyndicationExtension.NamespaceUri);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "role", this.Role);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "group", this.Group);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "img", this.ImageUrl);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "href", this.Url);
        writer.WriteString(this.Name);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastPerson"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastPerson? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Role, other.Role, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Group, other.Group, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.ImageUrl, other.ImageUrl, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastPerson"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastPerson"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public bool Equals(PodcastPerson? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is PodcastPerson other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Name),
        HashCodeUtility.Component(this.Role),
        HashCodeUtility.Component(this.Group),
        HashCodeUtility.Component(this.ImageUrl),
        HashCodeUtility.Component(this.Url));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(PodcastPerson? first, PodcastPerson? second)
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
    public static bool operator !=(PodcastPerson? first, PodcastPerson? second) => !(first == second);
}