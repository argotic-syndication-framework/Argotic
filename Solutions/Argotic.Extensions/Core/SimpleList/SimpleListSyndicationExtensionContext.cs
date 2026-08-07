using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="SimpleListSyndicationExtension"/>.
/// </summary>
public class SimpleListSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleListSyndicationExtensionContext"/> class.
    /// </summary>
    public SimpleListSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets information that allows the client to group or filter on the values of feed properties.
    /// </summary>
    /// <value>
    ///     One <see cref="SimpleListGroup"/> per property a client may group or filter on. The default
    ///     value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Written inside a single <c>cf:listinfo</c> element, which is emitted only when this or
    ///     <see cref="Sorting"/> is non-empty. Sorts are written before groups regardless of the order
    ///     they were read in.
    /// </remarks>
    public IList<SimpleListGroup> Grouping { get; } = [];

    /// <summary>
    /// Gets the properties a client may sort the list by.
    /// </summary>
    /// <value>
    ///     One <see cref="SimpleListSort"/> per sortable property. The default value is an <i>empty</i>
    ///     collection.
    /// </value>
    public IList<SimpleListSort> Sorting { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this feed is meant to be read as a list.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the feed is a list; otherwise, <see langword="false"/>. The default
    ///     value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    ///     The flag that distinguishes a list from an ordinary feed, and the thing a consumer should test
    ///     first. It is carried by <c>cf:treatAs</c>, whose only defined value is the string <c>list</c>:
    ///     this reads as <see langword="true"/> when that element holds <c>list</c> and
    ///     <see langword="false"/> in every other case, including an element present with some other
    ///     value.
    /// </remarks>
    public bool TreatAsList { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SimpleListSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="SimpleListSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? treatAsNavigator = source.SelectChildElement("cf", "treatAs", manager);
            XPathNavigator? listInformationNavigator = source.SelectChildElement("cf", "listinfo", manager);

            if (treatAsNavigator is not null && string.Equals(treatAsNavigator.Value, "list", StringComparison.OrdinalIgnoreCase))
            {
                this.TreatAsList = true;
                wasLoaded = true;
            }

            if (listInformationNavigator is { HasChildren: true })
            {
                XPathNodeIterator sortIterator = source.SelectChildElements("cf", "sort", manager);
                XPathNodeIterator groupIterator = source.SelectChildElements("cf", "group", manager);

                if (sortIterator is { Count: > 0 })
                {
                    while (sortIterator.MoveNext())
                    {
                        XPathNavigator? sortNode = sortIterator.Current;
                        if (sortNode is null)
                        {
                            continue;
                        }

                        SimpleListSort sort = new();
                        if (sort.Load(sortNode))
                        {
                            this.Sorting.Add(sort);
                            wasLoaded = true;
                        }
                    }
                }

                if (groupIterator is { Count: > 0 })
                {
                    while (groupIterator.MoveNext())
                    {
                        XPathNavigator? groupNode = groupIterator.Current;
                        if (groupNode is null)
                        {
                            continue;
                        }

                        SimpleListGroup group = new();
                        if (group.Load(groupNode))
                        {
                            this.Grouping.Add(group);
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
        if (this.TreatAsList)
        {
            writer.WriteElementString("treatAs", xmlNamespace, "list");
        }

        if (this.Grouping.Count > 0 || this.Sorting.Count > 0)
        {
            writer.WriteStartElement("listinfo", xmlNamespace);

            foreach (SimpleListSort sort in this.Sorting)
            {
                sort.WriteTo(writer);
            }

            foreach (SimpleListGroup group in this.Grouping)
            {
                group.WriteTo(writer);
            }

            writer.WriteEndElement();
        }
    }
}