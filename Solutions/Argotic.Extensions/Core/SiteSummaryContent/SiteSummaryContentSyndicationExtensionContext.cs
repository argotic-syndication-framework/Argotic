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
    /// Gets or sets the full content of the item, usually its complete HTML.
    /// </summary>
    /// <value>
    ///     The content of <c>content:encoded</c>, trimmed. The default value is an <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     This is the element that matters. A publisher who puts a summary in RSS's <c>description</c>
    ///     puts the whole article here, so an aggregator that ignores it shows excerpts of feeds that
    ///     shipped the full text.
    ///     </para>
    ///     <para>
    ///     What arrives may be entity-encoded HTML; what is written out is <i>always</i> CDATA-escaped.
    ///     A round-trip therefore normalises the escaping — the markup is preserved, the way it was
    ///     escaped is not. Setting <see langword="null"/> or an empty string clears the value rather than
    ///     throwing.
    ///     </para>
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
    ///     A collection of <see cref="SiteSummaryContentItem"/> objects, each one encoding of the same
    ///     content. The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     The module's original RDF syntax, superseded by <see cref="Encoded"/> and effectively extinct in
    ///     live feeds. Read and written for the feeds that still carry it; prefer <see cref="Encoded"/> for
    ///     anything you generate.
    /// </remarks>
    public IList<SiteSummaryContentItem> Items { get; } = [];

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SiteSummaryContentSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryContentSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
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
                XPathNodeIterator itemIterator = itemsNavigator.SelectChildElements("content", "item", manager);
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
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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