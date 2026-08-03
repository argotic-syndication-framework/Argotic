using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents information about related feeds or locations.
/// </summary>
/// <remarks>
///     In the case where a publisher’s feed has incorporated items from other feeds, it can be useful for subscribers to see more detailed information about the other feeds. 
///     In the case of feed sharing as envisioned by the <i>FeedSync</i> specification, this class can also be used to notify subscribing feeds of the feeds of other participants 
///     which they might also wish to subscribe to.
/// </remarks>
/// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
[Serializable]
public class FeedSynchronizationRelatedInformation : IComparable<FeedSynchronizationRelatedInformation>, IEquatable<FeedSynchronizationRelatedInformation>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class.
    /// </summary>
    public FeedSynchronizationRelatedInformation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
    /// </summary>
    /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
    /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
    public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type)
    {
        this.Link = link;
        this.RelationType = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
    /// </summary>
    /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
    /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
    /// <param name="title">The name or description of this related feed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
    public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type, string title) : this(link, type)
    {
        this.Title = title;
    }

    /// <summary>
    /// Gets or sets the URI for this related feed.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URI for this related feed.</value>
    /// <remarks>
    ///     The value <b>must not</b> be a relative reference.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Link
    {
        get => field;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the related feed.
    /// </summary>
    /// <value>
    ///     A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.
    ///     The default value is <see cref="FeedSynchronizationRelatedInformationType.None"/>, which indicates that no relation type has been specified.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window.
    ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
    ///     </para>
    ///     <para>
    ///         In the feed sharing context new subscribers, or existing subscribers failing to subscribe within the published feed window, will need to initially
    ///         copy a complete set of items from a publisher before being in a position to process incremental updates. As such, the specification provides for the
    ///         ability for the latter feed to reference the complete feed.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
    public FeedSynchronizationRelatedInformationType RelationType
    {
        get => field;
        set
        {
            if (value == FeedSynchronizationRelatedInformationType.None)
            {
                throw new ArgumentException($"The specified relation type of {value} is invalid.", nameof(value));
            }
            field = value;
        }
    } = FeedSynchronizationRelatedInformationType.None;

    /// <summary>
    /// Gets or sets the name or description of this related feed.
    /// </summary>
    /// <value>The name or description of this related feed.</value>
    public string Title
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Returns the relation type identifier for the supplied <see cref="FeedSynchronizationRelatedInformationType"/>.
    /// </summary>
    /// <param name="type">The <see cref="FeedSynchronizationRelatedInformationType"/> to get the relation type identifier for.</param>
    /// <returns>The relation type identifier for the supplied <paramref name="vocabulary"/>, Otherwise, returns an empty string.</returns>
    public static string RelationTypeAsString(FeedSynchronizationRelatedInformationType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified relation type name.
    /// </summary>
    /// <param name="name">The name of the relation type.</param>
    /// <returns>A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>FeedSynchronizationRelatedInformationType.None</b>.</returns>
    /// <remarks>This method disregards case of specified relation type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static FeedSynchronizationRelatedInformationType RelationTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, FeedSynchronizationRelatedInformationType.None);

    /// <summary>
    /// Loads this <see cref="FeedSynchronizationRelatedInformation"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="FeedSynchronizationRelatedInformation"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationRelatedInformation"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string linkAttribute = source.GetAttribute("link", string.Empty);
            string titleAttribute = source.GetAttribute("title", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);

            if (!string.IsNullOrEmpty(linkAttribute))
            {
                if (Uri.TryCreate(linkAttribute, UriKind.Absolute, out Uri link))
                {
                    this.Link = link;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(titleAttribute))
            {
                this.Title = titleAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                FeedSynchronizationRelatedInformationType type = FeedSynchronizationRelatedInformation.RelationTypeByName(typeAttribute);
                if (type != FeedSynchronizationRelatedInformationType.None)
                {
                    this.RelationType = type;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="FeedSynchronizationRelatedInformation"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        FeedSynchronizationSyndicationExtension extension = new();
        writer.WriteStartElement("related", extension.XmlNamespace);

        writer.WriteAttributeString("link", extension.XmlNamespace, this.Link?.ToString() ?? string.Empty);
        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteAttributeString("title", extension.XmlNamespace, this.Title);
        }
        writer.WriteAttributeString("type", extension.XmlNamespace, FeedSynchronizationRelatedInformation.RelationTypeAsString(this.RelationType));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
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
    public int CompareTo(FeedSynchronizationRelatedInformation? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.RelationType.CompareTo(other.RelationType);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedSynchronizationRelatedInformation"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedSynchronizationRelatedInformation"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="FeedSynchronizationRelatedInformation"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(FeedSynchronizationRelatedInformation? other)
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
        return obj is FeedSynchronizationRelatedInformation other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Link), HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.RelationType));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
    public static bool operator !=(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
    {
        return !(first == second);
    }

}