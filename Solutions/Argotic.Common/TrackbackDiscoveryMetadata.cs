using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Represents metadata about a web log entry that allows clients to auto-discover the TrackBack ping URL for that entry.
/// </summary>
[Serializable]
public class TrackbackDiscoveryMetadata : IComparable<TrackbackDiscoveryMetadata>, IEquatable<TrackbackDiscoveryMetadata>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the XML namespace for Resource Description Framework (RDF) entities.
    /// </summary>
    private const string RDF_NAMESPACE = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    /// <summary>
    /// Private member to hold the XML namespace for Dublin Core entities.
    /// </summary>
    private const string DUBLIN_CORE_NAMESPACE = "http://purl.org/dc/elements/1.1/";

    /// <summary>
    /// Private member to hold the XML namespace for Trackback entities.
    /// </summary>
    private const string TRACKBACK_NAMESPACE = "http://madskills.com/public/xml/rss/module/trackback/";

    /// <summary>
    /// Private member to hold the title of the discoverable web log entry.
    /// </summary>
    private string trackbackDiscoveryTitle = string.Empty;

    /// <summary>
    /// Private member to hold Resource Description Framework entity reference.
    /// </summary>
    private Uri? trackbackDiscoveryAbout;

    /// <summary>
    /// Private member to hold the unique identifier of the discoverable web log entry.
    /// </summary>
    private Uri? trackbackDiscoveryIdentifier;

    /// <summary>
    /// Private member to hold Trackback ping endpoint of the discoverable web log entry.
    /// </summary>
    private Uri? trackbackDiscoveryPingEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackDiscoveryMetadata"/> class.
    /// </summary>
    public TrackbackDiscoveryMetadata()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackDiscoveryMetadata"/> class using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the Trackback auto-discovery meta-data from.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    public TrackbackDiscoveryMetadata(XPathNavigator navigator) : this()
    {
        ArgumentNullException.ThrowIfNull(navigator);

        this.Load(navigator);
    }

    /// <summary>
    /// Gets or sets the Resource Description Framework (RDF) entity reference.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the Resource Description Framework (RDF) entity reference.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri? About
    {
        get => trackbackDiscoveryAbout;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            trackbackDiscoveryAbout = value;
        }
    }

    /// <summary>
    /// Gets or sets the unique identifier for the discoverable web log entry.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the unique identifier for the discoverable web log entry.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri? Identifier
    {
        get => trackbackDiscoveryIdentifier;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            trackbackDiscoveryIdentifier = value;
        }
    }

    /// <summary>
    /// Gets or sets the Trackback ping notification endpoint for the discoverable web log entry.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the Trackback ping URL for the discoverable web log entry.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri? PingUrl
    {
        get => trackbackDiscoveryPingEndpoint;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            trackbackDiscoveryPingEndpoint = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the discoverable web log entry.
    /// </summary>
    /// <value>The title of the discoverable web log entry.</value>
    public string Title
    {
        get => trackbackDiscoveryTitle;
        set => trackbackDiscoveryTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Loads this <see cref="TrackbackDiscoveryMetadata"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the Trackback auto-discovery meta-data from.</param>
    /// <returns><b>true</b> if Trackback auto-discovery meta-data was extracted from the <paramref name="navigator"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     Will return <b>false</b> if the <i>trackback:ping</i> attribute is not found on the <b>rdf:Description</b> element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    public bool Load(XPathNavigator navigator)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(navigator);

        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("rdf", RDF_NAMESPACE);
        manager.AddNamespace("dc", DUBLIN_CORE_NAMESPACE);
        manager.AddNamespace("trackback", TRACKBACK_NAMESPACE);

        XPathNavigator? descriptionNavigator = navigator.SelectSingleNode("rdf:RDF/rdf:Description", manager);

        if (descriptionNavigator is { HasAttributes: true })
        {
            string aboutAttribute = descriptionNavigator.GetAttribute("about", RDF_NAMESPACE);
            string identifierAttribute = descriptionNavigator.GetAttribute("identifier", DUBLIN_CORE_NAMESPACE);
            string titleAttribute = descriptionNavigator.GetAttribute("title", DUBLIN_CORE_NAMESPACE);
            string pingAttribute = descriptionNavigator.GetAttribute("ping", TRACKBACK_NAMESPACE);

            if (string.IsNullOrEmpty(pingAttribute))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(aboutAttribute))
            {
                if (Uri.TryCreate(aboutAttribute, UriKind.RelativeOrAbsolute, out Uri? about))
                {
                    this.About = about;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(identifierAttribute))
            {
                if (Uri.TryCreate(identifierAttribute, UriKind.RelativeOrAbsolute, out Uri? identifier))
                {
                    this.Identifier = identifier;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(titleAttribute))
            {
                this.Title = titleAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(pingAttribute))
            {
                if (Uri.TryCreate(pingAttribute, UriKind.RelativeOrAbsolute, out Uri? ping))
                {
                    this.PingUrl = ping;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="TrackbackDiscoveryMetadata"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("rdf", "RDF", RDF_NAMESPACE);
        writer.WriteAttributeString("xmlns", "dc", null, DUBLIN_CORE_NAMESPACE);
        writer.WriteAttributeString("xmlns", "trackback", null, TRACKBACK_NAMESPACE);

        writer.WriteStartElement("rdf", "Description", RDF_NAMESPACE);
        writer.WriteAttributeString("rdf", "about", RDF_NAMESPACE, this.About?.ToString() ?? string.Empty);
        writer.WriteAttributeString("dc", "identifier", DUBLIN_CORE_NAMESPACE, this.Identifier?.ToString() ?? string.Empty);
        writer.WriteAttributeString("dc", "title", DUBLIN_CORE_NAMESPACE, !string.IsNullOrEmpty(this.Title) ? this.Title : string.Empty);
        writer.WriteAttributeString("trackback", "ping", TRACKBACK_NAMESPACE, this.PingUrl?.ToString() ?? string.Empty);
        writer.WriteEndElement();

        writer.WriteFullEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackDiscoveryMetadata"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackDiscoveryMetadata"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(TrackbackDiscoveryMetadata? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.About, other.About, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Identifier, other.Identifier, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.PingUrl, other.PingUrl, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrackbackDiscoveryMetadata"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TrackbackDiscoveryMetadata"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="TrackbackDiscoveryMetadata"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(TrackbackDiscoveryMetadata? other)
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
        return obj is TrackbackDiscoveryMetadata other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.About), HashCodeUtility.Component(this.Identifier), HashCodeUtility.Component(this.PingUrl), HashCodeUtility.Component(this.Title));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(TrackbackDiscoveryMetadata? first, TrackbackDiscoveryMetadata? second)
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
    public static bool operator !=(TrackbackDiscoveryMetadata? first, TrackbackDiscoveryMetadata? second)
    {
        return !(first == second);
    }
}