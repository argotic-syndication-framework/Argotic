namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="AtomPublishingEditedSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class AtomPublishingEditedSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold the last time a resource was edited. If the resource has not been edited yet, indicates the time the resource was created.
        /// </summary>
        private DateTime extensionEditedOn = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPublishingEditedSyndicationExtensionContext"/> class.
        /// </summary>
        public AtomPublishingEditedSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets a date-time indicating the most recent instant in time when this resource was edited.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this resource was edited.
        ///     If the resource has not been edited yet, indicates the time the resource was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no edit time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime EditedOn
        {
            get
            {
                return this.extensionEditedOn;
            }

            set
            {
                this.extensionEditedOn = value;
            }
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="AtomPublishingEditedSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="AtomPublishingEditedSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator editedNavigator = source.SelectSingleNode("app:edited", manager);
                if (editedNavigator != null && !string.IsNullOrEmpty(editedNavigator.Value))
                {
                    DateTime editedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(editedNavigator.Value, out editedOn))
                    {
                        this.EditedOn = editedOn;
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
            if (this.EditedOn != DateTime.MinValue)
            {
                writer.WriteElementString("edited", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.EditedOn));
            }
        }
    }
}