using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="FeedHistorySyndicationExtension"/>.
/// </summary>
public class FeedHistorySyndicationExtensionContext
{
    /// <summary>
    /// The Atom 1.0 namespace, which is both where the <c>link</c> elements this type reads live and
    /// the test for whether the host document already owns them.
    /// </summary>
    private const string AtomNamespace = "http://www.w3.org/2005/Atom";

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedHistorySyndicationExtensionContext"/> class.
    /// </summary>
    public FeedHistorySyndicationExtensionContext()
    {

    }

    /// <summary>
    /// Gets or sets a value indicating the feed is a set of linked feed documents that together contain the entries of a logical feed, without any guarantees about the stability of the documents' contents.
    /// </summary>
    /// <value><see langword="true"/> if feed is a set of linked feed documents that together contain the entries of a logical feed; otherwise, <see langword="false"/>.</value>
    public bool IsArchive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the feed contains all the entries of a logical feed; any entry not actually in the feed document should not be considered to be part of that feed.
    /// </summary>
    /// <value><see langword="true"/> if feed contains all the entries of a logical feed; otherwise, <see langword="false"/>.</value>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets a collection of <see cref="FeedHistoryLinkRelation"/> objects that represent the relationships between feed documents.
    /// </summary>
    /// <value>The relations. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     <para>
    ///         <b>Populated on load only where the host document has no home of its own for an
    ///         <c>atom:link</c>.</b> RFC 5005 defines no element for these links: §1 assigns meanings to
    ///         relations on Atom's own <c>atom:link</c>, and Appendix B borrows <c>atom:link</c> into
    ///         RSS 2.0 because RSS has nothing that can carry a relation at all. So which layer owns the
    ///         element depends on the host, and the host decides it, not this type.
    ///     </para>
    ///     <para>
    ///         In an <b>Atom</b> document the core model owns it. <see cref="System.Xml.XPath.XPathNavigator"/>-driven
    ///         parsing fills <c>AtomFeed.Links</c> and <c>AtomEntry.Links</c> from every <c>atom:link</c>
    ///         the document carries — with the <c>type</c>, <c>title</c>, <c>hreflang</c> and
    ///         <c>length</c> that <see cref="FeedHistoryLinkRelation"/> does not model — and writes them
    ///         back. <see cref="Load(System.Xml.XPath.XPathNavigator, System.Xml.XmlNamespaceManager)"/>
    ///         therefore leaves them alone and this collection stays empty; read the paging links off
    ///         <c>Links</c> instead. Harvesting them here as well gave the same element two writers, and
    ///         the set doubled on every load-save cycle.
    ///     </para>
    ///     <para>
    ///         Everywhere else — RSS above all — there is no core home, this extension is the only
    ///         reader and the only writer, and the collection is filled as before.
    ///     </para>
    /// </remarks>
    public IList<FeedHistoryLinkRelation> Relations { get; } = [];

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="FeedHistorySyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedHistorySyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (string.IsNullOrEmpty(manager.LookupNamespace("atom")))
        {
            manager.AddNamespace("atom", AtomNamespace);
        }
        if (source.HasChildren)
        {
            XPathNavigator? archiveNavigator = source.SelectChildElement("fh", "archive", manager);
            XPathNavigator? completeNavigator = source.SelectChildElement("fh", "complete", manager);

            if (archiveNavigator is not null)
            {
                this.IsArchive = true;
                wasLoaded = true;
            }

            if (completeNavigator is not null)
            {
                this.IsComplete = true;
                wasLoaded = true;
            }

            // An Atom host already owns its atom:link elements: the core model parses every one of them
            // into AtomFeed.Links or AtomEntry.Links, keeping the attributes FeedHistoryLinkRelation
            // cannot model, and writes them back. Reading them here as well gave one element two
            // writers and doubled the set on every load-save cycle. Outside Atom -- RSS above all, per
            // RFC 5005 Appendix B -- nothing else reads or writes them, so this extension must.
            if (!string.Equals(source.NamespaceURI, AtomNamespace, StringComparison.Ordinal))
            {
                XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);

                while (linkIterator.MoveNext())
                {
                    XPathNavigator? linkNode = linkIterator.Current;
                    if (linkNode is null)
                    {
                        continue;
                    }

                    string relAttribute = linkNode.GetAttribute("rel", string.Empty);

                    if (!string.IsNullOrEmpty(relAttribute) && FeedHistorySyndicationExtension.LinkRelationTypeByName(relAttribute) != FeedHistoryLinkRelationType.None)
                    {
                        FeedHistoryLinkRelation relation = new();
                        if (relation.Load(linkNode))
                        {
                            this.Relations.Add(relation);
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
        if (this.IsArchive)
        {
            writer.WriteElementString("archive", xmlNamespace, string.Empty);
        }

        if (this.IsComplete)
        {
            writer.WriteElementString("complete", xmlNamespace, string.Empty);
        }

        foreach (FeedHistoryLinkRelation relation in this.Relations)
        {
            relation.WriteTo(writer);
        }
    }
}