using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="FeedRankSyndicationExtension"/>.
/// </summary>
[Serializable]
public class FeedRankSyndicationExtensionContext
{

    /// <summary>
    /// Private member to hold the permanent, universally unique identifier for the ranking scheme.
    /// </summary>
    private Uri extensionScheme;
    /// <summary>
    /// Private member to hold the permanent, universally unique identifier for the ranking domain.
    /// </summary>
    private Uri extensionDomain;
    /// <summary>
    /// Private member to hold the language sensitive, human-readable label for the rank.
    /// </summary>
    private string extensionLabel = string.Empty;
    /// <summary>
    /// Private member to hold the decimal value of the rank.
    /// </summary>
    private decimal extensionValue = decimal.MinValue;
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedRankSyndicationExtensionContext"/> class.
    /// </summary>
    public FeedRankSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedRankSyndicationExtensionContext"/> class using the supplied schem and value.
    /// </summary>
    /// <param name="scheme">The <see cref="Uri"/> that describes the permanent, universally unique identifier for the ranking scheme.</param>
    /// <param name="value">The <see cref="Decimal"/> value of the rank.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="scheme"/> is a null reference.</exception>
    public FeedRankSyndicationExtensionContext(Uri scheme, decimal value)
    {
        this.Scheme = scheme;
        this.Value = value;
    }
    /// <summary>
    /// Gets or sets the <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking domain.
    /// </summary>
    /// <value>The <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking domain.</value>
    public Uri Domain
    {
        get
        {
            return extensionDomain;
        }

        set
        {
            extensionDomain = value;
        }
    }

    /// <summary>
    /// Gets or sets the language sensitive, human-readable label for this rank.
    /// </summary>
    /// <value>The language sensitive, human-readable label for this rank.</value>
    public string Label
    {
        get
        {
            return extensionLabel;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                extensionLabel = string.Empty;
            }
            else
            {
                extensionLabel = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking scheme.
    /// </summary>
    /// <value>The <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking scheme.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Scheme
    {
        get
        {
            return extensionScheme;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            extensionScheme = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of this rank.
    /// </summary>
    /// <value>The <see cref="Decimal"/> value of this rank. The default value is <see cref="Decimal.MinValue"/>, which indicates that no ranking value was specified.</value>
    public decimal Value
    {
        get
        {
            return extensionValue;
        }

        set
        {
            extensionValue = value;
        }
    }
    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="FeedRankSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="FeedRankSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator rankNavigator = source.SelectSingleNode("re:rank", manager);
            if (rankNavigator != null)
            {
                if (rankNavigator.HasAttributes)
                {
                    string schemeAttribute = rankNavigator.GetAttribute("scheme", string.Empty);
                    string domainAttribute = rankNavigator.GetAttribute("domain", string.Empty);
                    string labelAttribute = rankNavigator.GetAttribute("label", string.Empty);

                    if (!string.IsNullOrEmpty(schemeAttribute))
                    {
                        if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri scheme))
                        {
                            this.Scheme = scheme;
                            wasLoaded = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(domainAttribute))
                    {
                        if (Uri.TryCreate(domainAttribute, UriKind.RelativeOrAbsolute, out Uri domain))
                        {
                            this.Domain = domain;
                            wasLoaded = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(labelAttribute))
                    {
                        this.Label = labelAttribute;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(rankNavigator.Value))
                {
                    if (decimal.TryParse(rankNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal value))
                    {
                        this.Value = value;
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        writer.WriteStartElement("rank", xmlNamespace);

        writer.WriteAttributeString("scheme", xmlNamespace, this.Scheme != null ? this.Scheme.ToString() : string.Empty);

        if (this.Domain != null)
        {
            writer.WriteAttributeString("domain", xmlNamespace, this.Domain.ToString());
        }

        if (!string.IsNullOrEmpty(this.Label))
        {
            writer.WriteAttributeString("label", this.Label);
        }

        if (this.Value != decimal.MinValue)
        {
            writer.WriteString(this.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        writer.WriteEndElement();
    }
}