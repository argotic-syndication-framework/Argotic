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
    ///     A <see cref="IList{T}"/> collection of <see cref="SimpleListGroup"/> objects that represent information that allows the client to group or filter on the values of feed properties.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<SimpleListGroup> Grouping { get; } = [];

    /// <summary>
    /// Gets information that allows the client to sort on the values of feed properties.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="SimpleListSort"/> objects that represent information that allows the client to sort on the values of feed properties.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<SimpleListSort> Sorting { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating if this feed is intended to be consumed as a list.
    /// </summary>
    /// <value><b>true</b> if the syndication feed is intended to be consumed as a list; Otherwise, false.</value>
    /// <remarks>
    ///     This property allows the publisher of a feed document to indicate to the consumers of the feed that the feed is intended to be consumed as a list,
    ///     and as such is the primary means for feed consumers to identify lists.
    /// </remarks>
    public bool TreatAsList { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SimpleListSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="SimpleListSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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
                XPathNodeIterator sortIterator = source.Select("cf:sort", manager);
                XPathNodeIterator groupIterator = source.Select("cf:group", manager);

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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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