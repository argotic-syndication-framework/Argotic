using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a discoverable application programming interface (API) that provides services to web log clients.
/// </summary>
/// <seealso cref="RsdDocument.Interfaces"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RsdApplicationInterface class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rsd\RsdApplicationInterfaceExample.cs" 
///             region="RsdApplicationInterface" 
///         />
///     </code>
/// </example>
[Serializable]
public class RsdApplicationInterface : IComparable<RsdApplicationInterface>, IEquatable<RsdApplicationInterface>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsdApplicationInterface"/> class.
    /// </summary>
    public RsdApplicationInterface()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RsdApplicationInterface"/> class using the supplied parameters.
    /// </summary>
    /// <param name="name">The name of this application interface.</param>
    /// <param name="link">A <see cref="Uri"/> that represents the endpoint of this application interface clients should use to communication with the service.</param>
    /// <param name="isPreferred">A value indicating if this application interface is the preferred service.</param>
    /// <param name="weblogId">A custom web log identifier utilized by this application interface. Can be an empty string.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    public RsdApplicationInterface(string name, Uri link, bool isPreferred, string weblogId)
    {
        this.IsPreferred = isPreferred;
        this.Link = link;
        this.Name = name;
        this.WeblogId = weblogId;
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
    /// Gets or sets the location of the documentation for this application interface.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of the documentation for this application interface.</value>
    public Uri? Documentation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this application interface is preferred.
    /// </summary>
    /// <value><b>true</b> if this application interface is the preferred service; Otherwise, <b>false</b>.</value>
    public bool IsPreferred { get; set; }

    /// <summary>
    /// Gets or sets the communication endpoint of this application interface.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the endpoint of this application interface clients should use to communication with the service.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// Gets or sets the name of this application interface.
    /// </summary>
    /// <value>The name of this application interface.</value>
    /// <remarks>
    ///     Well known application interface names include the following:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><b>Antville</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>Blogger</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>Conversant</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>LiveJournal</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>Manila</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>MetaWeblog</b></description>
    ///         </item>
    ///         <item>
    ///             <description><b>MetaWiki</b></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
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
    /// Gets or sets human readable text that explains the features and settings for this application interface.
    /// </summary>
    /// <value>Human readable text that explains the features and settings for this application interface.</value>
    public string Notes
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets a collection of service specific settings for this application interface.
    /// </summary>
    /// <value>A <see cref="Dictionary{T, T}"/> collection of service specific settings for this application interface.</value>
    public Dictionary<string, string> Settings { get; } = [];

    /// <summary>
    /// Gets or sets a custom web log identifier utilized by this application interface.
    /// </summary>
    /// <value>A custom web log identifier utilized by this application interface.</value>
    public string WeblogId
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = RsdUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string nameAttribute = source.GetAttribute("name", string.Empty);
            string preferredAttribute = source.GetAttribute("preferred", string.Empty);
            string apiLinkAttribute = source.GetAttribute("apiLink", string.Empty);
            string blogIdAttribute = source.GetAttribute("blogID", string.Empty);

            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(preferredAttribute))
            {
                if (bool.TryParse(preferredAttribute, out bool isPreferred))
                {
                    this.IsPreferred = isPreferred;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(apiLinkAttribute))
            {
                if (Uri.TryCreate(apiLinkAttribute, UriKind.RelativeOrAbsolute, out Uri? link))
                {
                    this.Link = link;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(blogIdAttribute))
            {
                this.WeblogId = blogIdAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? settingsNavigator = RsdUtility.SelectSafeSingleNode(source, "rsd:api/rsd:settings", manager);

            if (settingsNavigator is not null)
            {
                XPathNavigator? docsNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:docs", manager);
                XPathNavigator? notesNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:notes", manager);
                XPathNodeIterator settingIterator = RsdUtility.SelectSafe(settingsNavigator, "rsd:setting", manager);

                if (docsNavigator is not null)
                {
                    if (Uri.TryCreate(docsNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? documentation))
                    {
                        this.Documentation = documentation;
                        wasLoaded = true;
                    }
                }

                if (notesNavigator is not null)
                {
                    this.Notes = notesNavigator.Value;
                    wasLoaded = true;
                }

                if (settingIterator is { Count: > 0 })
                {
                    while (settingIterator.MoveNext())
                    {
                        XPathNavigator? settingNode = settingIterator.Current;
                        if (settingNode is null)
                        {
                            continue;
                        }

                        string settingName = settingNode.GetAttribute("name", string.Empty);
                        string settingValue = settingNode.Value;

                        if (this.Settings.TryAdd(settingName, settingValue))
                        {
                            wasLoaded = true;
                        }
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
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
    /// Saves the current <see cref="RsdApplicationInterface"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("api", RsdUtility.RsdNamespace);

        writer.WriteAttributeString("name", this.Name);
        writer.WriteAttributeString("preferred", this.IsPreferred ? "true" : "false");
        writer.WriteAttributeString("apiLink", this.Link?.ToString() ?? string.Empty);
        writer.WriteAttributeString("blogID", this.WeblogId);

        if (this.Documentation is not null || !string.IsNullOrEmpty(this.Notes) || this.Settings.Count > 0)
        {
            writer.WriteStartElement("settings", RsdUtility.RsdNamespace);

            if (this.Documentation is not null)
            {
                writer.WriteElementString("docs", RsdUtility.RsdNamespace, this.Documentation.ToString());
            }

            if (!string.IsNullOrEmpty(this.Notes))
            {
                writer.WriteElementString("notes", RsdUtility.RsdNamespace, this.Notes);
            }

            foreach (string settingName in this.Settings.Keys)
            {
                writer.WriteStartElement("setting", RsdUtility.RsdNamespace);
                writer.WriteAttributeString("name", settingName);
                writer.WriteString(this.Settings[settingName]);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RsdApplicationInterface"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RsdApplicationInterface"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RsdApplicationInterface? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.IsPreferred.CompareTo(other.IsPreferred);
        if (result == 0) result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Notes, other.Notes, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Settings, other.Settings, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.WeblogId, other.WeblogId, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RsdApplicationInterface"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RsdApplicationInterface"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="RsdApplicationInterface"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(RsdApplicationInterface? other)
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
    public override bool Equals(object? obj) => obj is RsdApplicationInterface other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.Documentation),
            HashCodeUtility.Component(this.IsPreferred),
            HashCodeUtility.Component(this.Link),
            HashCodeUtility.Component(this.Name),
            HashCodeUtility.Component(this.Notes),
            HashCodeUtility.Component(this.Settings.Count),
            HashCodeUtility.Component(this.WeblogId));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RsdApplicationInterface? first, RsdApplicationInterface? second)
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
    public static bool operator !=(RsdApplicationInterface? first, RsdApplicationInterface? second) => !(first == second);
}