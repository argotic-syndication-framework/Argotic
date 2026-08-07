using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="WellFormedWebCommentsSyndicationExtension"/>.
/// </summary>
public class WellFormedWebCommentsSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> class.
    /// </summary>
    public WellFormedWebCommentsSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the endpoint a new comment is posted to.
    /// </summary>
    /// <value>
    ///     The <c>wfw:comment</c> URL, or <see langword="null"/> if none was specified. Write, not read —
    ///     see <see cref="CommentsFeed"/> for fetching the existing comments.
    /// </value>
    public Uri? Comments { get; set; }

    /// <summary>
    /// Gets or sets the location of the feed carrying this item's comments.
    /// </summary>
    /// <value>
    ///     The <c>wfw:commentRss</c> URL, or <see langword="null"/> if none was specified.
    /// </value>
    /// <remarks>
    ///     Populated from either spelling of the element, <c>commentRss</c> or the specification's
    ///     original <c>commentRSS</c>, and always written back as <c>commentRss</c>. A feed using the old
    ///     spelling therefore does not round-trip byte-for-byte.
    /// </remarks>
    public Uri? CommentsFeed { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="WellFormedWebCommentsSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? commentNavigator = source.SelectChildElement("wfw", "comment", manager);
            XPathNavigator? commentRssNavigator = source.SelectChildElement("wfw", "commentRss", manager);

            if (commentNavigator is not null)
            {
                if (Uri.TryCreate(commentNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? comments))
                {
                    this.Comments = comments;
                    wasLoaded = true;
                }
            }

            // Early in specification, there was a typo that incorrectly named the comment feed element, this handles the scenario where publisher used incorrect element name
            commentRssNavigator ??= source.SelectChildElement("wfw", "commentRSS", manager);

            if (commentRssNavigator is not null)
            {
                if (Uri.TryCreate(commentRssNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? commentsFeed))
                {
                    this.CommentsFeed = commentsFeed;
                    wasLoaded = true;
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
        if (this.Comments is not null)
        {
            writer.WriteElementString("comment", xmlNamespace, this.Comments.ToString());
        }

        if (this.CommentsFeed is not null)
        {
            writer.WriteElementString("commentRss", xmlNamespace, this.CommentsFeed.ToString());
        }
    }
}