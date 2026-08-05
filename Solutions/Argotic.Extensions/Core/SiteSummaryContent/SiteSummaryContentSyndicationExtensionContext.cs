using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="SiteSummaryContentSyndicationExtension"/>.
/// </summary>
public class SiteSummaryContentSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummaryContentSyndicationExtensionContext"/> class.
    /// </summary>
    public SiteSummaryContentSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets an alternative version of the content of this item.
    /// </summary>
    /// <value>The alternative version of the content of this item.</value>
    /// <remarks>
    ///     The value of this property <i>may</i> be entity-encoded, but will <b>always</b> be CDATA-escaped.
    /// </remarks>
    public string Encoded
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets the alternative versions of this item's content.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="SiteSummaryContentItem"/> objects that represent multiple versions of this item's content.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     The <see cref="Encoded"/> property represents the <b>updated</b> syntax for the <see cref="SiteSummaryContentSyndicationExtension"/>.
    ///     It is <i>recommended</i> that <see cref="Encoded"/> is utilized when defining an alternative encoding for the content of an item.
    /// </remarks>
    public IList<SiteSummaryContentItem> Items { get; } = [];

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummaryContentSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="SiteSummaryContentSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? encodedNavigator = source.SelectChildElement("content", "encoded", manager);
            XPathNavigator? itemsNavigator = source.SelectChildElement("content", "items", manager);

            if (encodedNavigator is not null && !string.IsNullOrEmpty(encodedNavigator.Value))
            {
                this.Encoded = encodedNavigator.Value;
                wasLoaded = true;
            }

            if (itemsNavigator is { HasChildren: true })
            {
                XPathNodeIterator itemIterator = itemsNavigator.Select("content:item", manager);
                if (itemIterator is { Count: > 0 })
                {
                    while (itemIterator.MoveNext())
                    {
                        XPathNavigator? itemNode = itemIterator.Current;
                        if (itemNode is null)
                        {
                            continue;
                        }

                        SiteSummaryContentItem item = new();
                        if (item.Load(itemNode))
                        {
                            this.Items.Add(item);
                            wasLoaded = true;
                        }
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
        if (!string.IsNullOrEmpty(this.Encoded))
        {
            writer.WriteStartElement("encoded", xmlNamespace);
            writer.WriteCData(this.Encoded);
            writer.WriteEndElement();
        }

        if (this.Items.Count > 0)
        {
            writer.WriteStartElement("items", xmlNamespace);
            foreach (SiteSummaryContentItem item in this.Items)
            {
                item.WriteTo(writer);
            }
            writer.WriteEndElement();
        }
    }
}