namespace Argotic.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents a media object such as an audio, video, or executable file that can be associated with an <see cref="RssItem"/>.
    /// </summary>
    /// <seealso cref="RssItem.Enclosures"/>
    /// <remarks>
    ///     <para>
    ///         Support for the enclosure element in RSS software varies significantly because of disagreement over whether the specification permits more than one enclosure per item.
    ///         Although the original author intended to permit no more than one enclosure in each item, this limit is not explicit in the specification.
    ///         For best support in the widest number of aggregators, an item <i>should not</i> contain more than one enclosure.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the RssEnclosure class.">
    ///         <code source="..\..\Argotic.Examples\Core\Rss\RssEnclosureExample.cs" region="RssEnclosure" />
    ///     </code>
    /// </example>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    public class RssEnclosure : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the size of the media object in bytes.
        /// </summary>
        private long enclosureLength = long.MinValue;

        /// <summary>
        /// Private member to hold the media object's MIME media type.
        /// </summary>
        private string enclosureType = string.Empty;

        /// <summary>
        /// Private member to hold the URL of the media object.
        /// </summary>
        private Uri enclosureUrl;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssEnclosure"/> class.
        /// </summary>
        public RssEnclosure()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssEnclosure"/> class using the supplied length, MIME type, and <see cref="Uri"/>.
        /// </summary>
        /// <param name="length">The size, in bytes, of the media object.</param>
        /// <param name="type">The media object's MIME content type.</param>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of the media object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="length"/> is less than <i>zero</i>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public RssEnclosure(long length, string type, Uri url)
        {
            this.ContentType = type;
            this.Length = length;
            this.Url = url;
        }

        /// <summary>
        /// Gets or sets the media object's MIME content type.
        /// </summary>
        /// <value>The media object's MIME content type.</value>
        /// <remarks>
        ///     See <a href="http://www.iana.org/assignments/media-types/">http://www.iana.org/assignments/media-types/</a> for a listing of the registered IANA MIME media types.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string ContentType
        {
            get
            {
                return this.enclosureType;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.enclosureType = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the syndication extensions applied to this syndication entity.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (this.objectSyndicationExtensions == null)
                {
                    this.objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }

                return this.objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.objectSyndicationExtensions = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the size of the media object.
        /// </summary>
        /// <value>The size, in bytes, of the media object. The default value is <see cref="long.MinValue"/>, which indicates that no size was specified.</value>
        /// <remarks>
        ///     <para>
        ///         Though an enclosure <b>must</b> specify its size with the length attribute, the size of some media objects cannot be determined by an RSS publisher.
        ///         When an enclosure's size cannot be determined, a publisher <i>should</i> use a length of 0.
        ///     </para>
        ///     <para>
        ///         The peer-to-peer file-sharing protocol BitTorrent deploys files using a small key file called a torrent that tells a client how to find and download the file.
        ///         When an enclosure is delivered in a multi-step process like the one used by BitTorrent, the length <i>should</i> be the size
        ///         of the first file that must be downloaded to begin the process.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <i>zero</i>.</exception>
        public long Length
        {
            get
            {
                return this.enclosureLength;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", 0);
                this.enclosureLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL of the media object.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the media object.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return this.enclosureUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.enclosureUrl = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RssEnclosure first, RssEnclosure second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(RssEnclosure first, RssEnclosure second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(RssEnclosure first, RssEnclosure second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RssEnclosure first, RssEnclosure second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            bool wasAdded = false;
            Guard.ArgumentNotNull(extension, "extension");
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded = true;

            return wasAdded;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            RssEnclosure value = obj as RssEnclosure;

            if (value != null)
            {
                int result = string.Compare(this.ContentType, value.ContentType, StringComparison.OrdinalIgnoreCase);
                result = result | this.Length.CompareTo(value.Length);
                result = result | Uri.Compare(
                             this.Url,
                             value.Url,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);

                return result;
            }

            throw new ArgumentException(
                string.Format(
                    null,
                    "obj is not of type {0}, type was found to be '{1}'.",
                    this.GetType().FullName,
                    obj.GetType().FullName),
                "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RssEnclosure))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
        /// <returns>
        ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
        /// </returns>
        /// <remarks>
        ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
        ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
        ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            Guard.ArgumentNotNull(match, "match");
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Loads this <see cref="RssEnclosure"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RssEnclosure"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssEnclosure"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string lengthAttribute = source.GetAttribute("length", string.Empty);
                string typeAttribute = source.GetAttribute("type", string.Empty);
                string urlAttribute = source.GetAttribute("url", string.Empty);

                if (!string.IsNullOrEmpty(lengthAttribute))
                {
                    long length;
                    if (long.TryParse(
                        lengthAttribute,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.NumberFormatInfo.InvariantInfo,
                        out length))
                    {
                        if (length >= 0)
                        {
                            this.Length = length;
                        }
                        else
                        {
                            this.Length = 0;
                        }

                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    this.ContentType = typeAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(urlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url = url;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="RssEnclosure"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssEnclosure"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssEnclosure"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");
            wasLoaded = this.Load(source);
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }

        /// <summary>
        /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveExtension(ISyndicationExtension extension)
        {
            bool wasRemoved = false;
            Guard.ArgumentNotNull(extension, "extension");
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved = true;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="RssEnclosure"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="RssEnclosure"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.WriteTo(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Saves the current <see cref="RssEnclosure"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("enclosure");

            writer.WriteAttributeString(
                "length",
                this.Length != long.MinValue ? this.Length.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty);
            writer.WriteAttributeString("type", this.ContentType);
            writer.WriteAttributeString("url", this.Url != null ? this.Url.ToString() : string.Empty);
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}