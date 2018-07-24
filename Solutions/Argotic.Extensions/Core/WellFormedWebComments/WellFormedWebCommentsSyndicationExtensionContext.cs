namespace Argotic.Extensions.Core
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="WellFormedWebCommentsSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class WellFormedWebCommentsSyndicationExtensionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> class.
        /// </summary>
        public WellFormedWebCommentsSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets the URL that comment entries are to be posted to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URI that comment entries are to be posted to.</value>
        public Uri Comments { get; set; }

        /// <summary>
        /// Gets or sets the URL of the syndication feed for comment entries.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URI of the syndication feed for comment entries.</value>
        public Uri CommentsFeed { get; set; }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="WellFormedWebCommentsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator commentNavigator = source.SelectSingleNode("wfw:comment", manager);
                XPathNavigator commentRssNavigator = source.SelectSingleNode("wfw:commentRss", manager);

                if (commentNavigator != null)
                {
                    Uri comments;
                    if (Uri.TryCreate(commentNavigator.Value, UriKind.RelativeOrAbsolute, out comments))
                    {
                        this.Comments = comments;
                        wasLoaded = true;
                    }
                }

                // Early in specification, there was a typo that incorrectly named the comment feed element, this handles the scenario where publisher used incorrect element name
                if (commentRssNavigator == null)
                {
                    commentRssNavigator = source.SelectSingleNode("wfw:commentRSS", manager);
                }

                if (commentRssNavigator != null)
                {
                    Uri commentsFeed;
                    if (Uri.TryCreate(commentRssNavigator.Value, UriKind.RelativeOrAbsolute, out commentsFeed))
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
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        public void WriteTo(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (this.Comments != null)
            {
                writer.WriteElementString("comment", xmlNamespace, this.Comments.ToString());
            }

            if (this.CommentsFeed != null)
            {
                writer.WriteElementString("commentRss", xmlNamespace, this.CommentsFeed.ToString());
            }
        }
    }
}