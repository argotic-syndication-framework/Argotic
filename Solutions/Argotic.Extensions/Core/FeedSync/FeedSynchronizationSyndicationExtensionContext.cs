﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="FeedSynchronizationSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class FeedSynchronizationSyndicationExtensionContext
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationSyndicationExtensionContext"/> class.
        /// </summary>
        public FeedSynchronizationSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets information from a specific feed publisher to the specific feed consumer that requested the feed.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedSynchronizationSharingInformation"/> object that represents information from a specific feed publisher to the specific feed consumer that requested the feed.
        ///     The default value is <b>null</b>.
        /// </value>
        public FeedSynchronizationSharingInformation Sharing { get; set; }

        /// <summary>
        /// Gets or sets the information required for synchronization.
        /// </summary>
        /// <value>A <see cref="Synchronization"/> object that represents the information required for synchronization.</value>
        /// <remarks>
        ///     <para>
        ///         This is <b>required</b> of all items in all feeds wishing to participate in FeedSync-based synchronization.
        ///         Since <see cref="FeedSynchronizationSharingInformation"/> is not required, feed consumers <b>must</b> consider the presence of <see cref="FeedSynchronizationItem"/> in items or entries
        ///         as an indication that the feed contains sync data.
        ///     </para>
        ///     <para>
        ///         It acceptable for a feed to have some items or entries with <see cref="FeedSynchronizationItem"/> elements, and some without a <see cref="FeedSynchronizationItem"/>.
        ///         Only the items and entries that include the <see cref="FeedSynchronizationItem"/> element participate in FeedSync synchronization.
        ///     </para>
        /// </remarks>
        public FeedSynchronizationItem Synchronization { get; set; }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="FeedSynchronizationSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="FeedSynchronizationSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator sharingNavigator = source.SelectSingleNode("sx:sharing", manager);
                XPathNavigator syncNavigator = source.SelectSingleNode("sx:sync", manager);

                if (sharingNavigator != null)
                {
                    FeedSynchronizationSharingInformation sharing = new FeedSynchronizationSharingInformation();
                    if (sharing.Load(sharingNavigator))
                    {
                        this.Sharing = sharing;
                        wasLoaded = true;
                    }
                }

                if (syncNavigator != null)
                {
                    FeedSynchronizationItem synchronization = new FeedSynchronizationItem();
                    if (synchronization.Load(syncNavigator))
                    {
                        this.Synchronization = synchronization;
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
            if (this.Sharing != null)
            {
                this.Sharing.WriteTo(writer);
            }

            if (this.Synchronization != null)
            {
                this.Synchronization.WriteTo(writer);
            }
        }
    }
}