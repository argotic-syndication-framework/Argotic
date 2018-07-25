namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a means of grouping of <see cref="YahooMediaContent"/> objects that are effectively the same content, yet different representations.
    /// </summary>
    /// <seealso cref="YahooMediaContent"/>
    /// <seealso cref="IYahooMediaCommonObjectEntities"/>
    [Serializable]
    public class YahooMediaGroup : IComparable, IYahooMediaCommonObjectEntities
    {
        /// <summary>
        /// Private member to hold a collection of media objects that are effectively the same content, yet different representations.
        /// </summary>
        private Collection<YahooMediaContent> groupContents;

        /// <summary>
        /// Private member to hold the permissible audiences for the media group.
        /// </summary>
        private Collection<YahooMediaRating> mediaObjectRatings;

        /// <summary>
        /// Private member to hold the relevant keywords that describe the media group.
        /// </summary>
        private Collection<string> mediaObjectKeywords;

        /// <summary>
        /// Private member to hold the representative images for the media group.
        /// </summary>
        private Collection<YahooMediaThumbnail> mediaObjectThumbnails;

        /// <summary>
        /// Private member to hold a taxonomy that gives an indication of the type of content for the media group.
        /// </summary>
        private Collection<YahooMediaCategory> mediaObjectCategories;

        /// <summary>
        /// Private member to hold the hash digests for the media group.
        /// </summary>
        private Collection<YahooMediaHash> mediaObjectHashes;

        /// <summary>
        /// Private member to hold the entities that contributed to the creation of the media group.
        /// </summary>
        private Collection<YahooMediaCredit> mediaObjectCredits;

        /// <summary>
        /// Private member to hold the text transcript, closed captioning, or lyrics for the media group.
        /// </summary>
        private Collection<YahooMediaText> mediaObjectTextSeries;

        /// <summary>
        /// Private member to hold the restrictions to be placed on aggregators that are rendering the media group.
        /// </summary>
        private Collection<YahooMediaRestriction> mediaObjectRestrictions;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaGroup"/> class.
        /// </summary>
        public YahooMediaGroup()
        {
        }

        /// <summary>
        /// Gets a collection of media objects that are effectively the same content, yet different representations.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaContent"/> objects that represent media objects that are effectively the same content, yet different representations.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<YahooMediaContent> Contents
        {
            get
            {
                if (this.groupContents == null)
                {
                    this.groupContents = new Collection<YahooMediaContent>();
                }

                return this.groupContents;
            }
        }

        /// <summary>
        /// Gets a taxonomy that gives an indication of the type of content for this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCategory"/> objects that represent a taxonomy that gives an indication to the type of content for this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<YahooMediaCategory> Categories
        {
            get
            {
                if (this.mediaObjectCategories == null)
                {
                    this.mediaObjectCategories = new Collection<YahooMediaCategory>();
                }

                return this.mediaObjectCategories;
            }
        }

        /// <summary>
        /// Gets or sets the copyright information for this media group.
        /// </summary>
        /// <value>A <see cref="YahooMediaCopyright"/> that represents the copyright information for this media group.</value>
        /// <remarks>
        ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
        /// </remarks>
        public YahooMediaCopyright Copyright { get; set; }

        /// <summary>
        /// Gets the entities that contributed to the creation of this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCredit"/> objects that represent the entities that contributed to the creation of this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Current entities can include people, companies, locations, etc. Specific entities can have multiple roles,
        ///     and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
        /// </remarks>
        public Collection<YahooMediaCredit> Credits
        {
            get
            {
                if (this.mediaObjectCredits == null)
                {
                    this.mediaObjectCredits = new Collection<YahooMediaCredit>();
                }

                return this.mediaObjectCredits;
            }
        }

        /// <summary>
        /// Gets or sets the description of this media group.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents a short description of this media group.</value>
        /// <remarks>
        ///     Media object descriptions are typically a sentence in length.
        /// </remarks>
        public YahooMediaTextConstruct Description { get; set; }

        /// <summary>
        /// Gets the hash digests for this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaHash"/> objects that represent the hash digests for this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     When assigning multiple hashes, each <see cref="YahooMediaHash"/> <b>must</b> have a different <see cref="YahooMediaHash.Algorithm"/>.
        /// </remarks>
        public Collection<YahooMediaHash> Hashes
        {
            get
            {
                if (this.mediaObjectHashes == null)
                {
                    this.mediaObjectHashes = new Collection<YahooMediaHash>();
                }

                return this.mediaObjectHashes;
            }
        }

        /// <summary>
        /// Gets the relevant keywords that describe this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="string"/> objects that represent the relevant keywords that describe this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Media objects are typically assigned maximum of ten keywords or phrases.
        /// </remarks>
        public Collection<string> Keywords
        {
            get
            {
                if (this.mediaObjectKeywords == null)
                {
                    this.mediaObjectKeywords = new Collection<string>();
                }

                return this.mediaObjectKeywords;
            }
        }

        /// <summary>
        /// Gets or sets a web browser media player console this media group can be accessed through.
        /// </summary>
        /// <value>A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this media group can be accessed through.</value>
        public YahooMediaPlayer Player { get; set; }

        /// <summary>
        /// Gets the permissible audiences for this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRating"/> objects that represent the permissible audiences for this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If there are no ratings specified, it can be assumed that no restrictions are necessary.
        /// </remarks>
        public Collection<YahooMediaRating> Ratings
        {
            get
            {
                if (this.mediaObjectRatings == null)
                {
                    this.mediaObjectRatings = new Collection<YahooMediaRating>();
                }

                return this.mediaObjectRatings;
            }
        }

        /// <summary>
        /// Gets the restrictions to be placed on aggregators that are rendering this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRestriction"/> objects that represent restrictions to be placed on aggregators that are rendering this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<YahooMediaRestriction> Restrictions
        {
            get
            {
                if (this.mediaObjectRestrictions == null)
                {
                    this.mediaObjectRestrictions = new Collection<YahooMediaRestriction>();
                }

                return this.mediaObjectRestrictions;
            }
        }

        /// <summary>
        /// Gets the text transcript, closed captioning, or lyrics for this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaText"/> objects that represent text transcript, closed captioning, or lyrics for this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Many of these <see cref="YahooMediaText"/> objects are permitted to provide a time series of text.
        ///     In such cases, it is encouraged, but not required, that the <see cref="YahooMediaText"/> objects be grouped by language and appear in time sequence order based on the start time.
        ///     <see cref="YahooMediaText"/> objects can have overlapping start and end times.
        /// </remarks>
        public Collection<YahooMediaText> TextSeries
        {
            get
            {
                if (this.mediaObjectTextSeries == null)
                {
                    this.mediaObjectTextSeries = new Collection<YahooMediaText>();
                }

                return this.mediaObjectTextSeries;
            }
        }

        /// <summary>
        /// Gets the representative images for this media group.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaThumbnail"/> objects that represent images that are representative of this media group.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If multiple thumbnails are included, and time coding is not at play, it is assumed that the images are in order of importance.
        /// </remarks>
        public Collection<YahooMediaThumbnail> Thumbnails
        {
            get
            {
                if (this.mediaObjectThumbnails == null)
                {
                    this.mediaObjectThumbnails = new Collection<YahooMediaThumbnail>();
                }

                return this.mediaObjectThumbnails;
            }
        }

        /// <summary>
        /// Gets or sets the title of this media group.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents the title of this media group.</value>
        public YahooMediaTextConstruct Title { get; set; }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaGroup first, YahooMediaGroup second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(YahooMediaGroup first, YahooMediaGroup second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaGroup first, YahooMediaGroup second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(YahooMediaGroup first, YahooMediaGroup second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
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
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is YahooMediaGroup))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Loads this <see cref="YahooMediaGroup"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaGroup"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaGroup"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
            if (source.HasChildren)
            {
                XPathNodeIterator contentIterator = source.Select("media:content", manager);

                if (contentIterator != null && contentIterator.Count > 0)
                {
                    while (contentIterator.MoveNext())
                    {
                        YahooMediaContent content = new YahooMediaContent();
                        if (content.Load(contentIterator.Current))
                        {
                            this.Contents.Add(content);
                            wasLoaded = true;
                        }
                    }
                }
            }

            if (YahooMediaUtility.FillCommonObjectEntities(this, source))
            {
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaGroup"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("group", extension.XmlNamespace);

            foreach (YahooMediaContent content in this.Contents)
            {
                content.WriteTo(writer);
            }

            YahooMediaUtility.WriteCommonObjectEntities(this, writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaGroup"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaGroup"/>.</returns>
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

            YahooMediaGroup value = obj as YahooMediaGroup;

            if (value != null)
            {
                int result = YahooMediaUtility.CompareSequence(this.Contents, value.Contents);

                result = result | YahooMediaUtility.CompareCommonObjectEntities(this, value);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
    }
}