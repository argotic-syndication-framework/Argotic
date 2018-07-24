namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a publishable media object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Media objects that are not the same content should not be included in the same <see cref="YahooMediaGroup"/>.
    ///         The sequence of <see cref="YahooMediaContent"/> objects within a <see cref="YahooMediaGroup"/> implies the order of presentation.
    ///     </para>
    /// </remarks>
    /// <seealso cref="IYahooMediaCommonObjectEntities"/>
    [Serializable]
    public class YahooMediaContent : IComparable, IYahooMediaCommonObjectEntities
    {
        /// <summary>
        /// Private member to hold the MIME type of the media object.
        /// </summary>
        private string contentMimeType = string.Empty;

        /// <summary>
        /// Private member to hold the permissible audiences for the media object.
        /// </summary>
        private Collection<YahooMediaRating> mediaObjectRatings;

        /// <summary>
        /// Private member to hold the title of the media object.
        /// </summary>
        private YahooMediaTextConstruct mediaObjectTitle;

        /// <summary>
        /// Private member to hold a short description of the media object.
        /// </summary>
        private YahooMediaTextConstruct mediaObjectDescription;

        /// <summary>
        /// Private member to hold the relevant keywords that describe the media object.
        /// </summary>
        private Collection<string> mediaObjectKeywords;

        /// <summary>
        /// Private member to hold the representative images for the media object.
        /// </summary>
        private Collection<YahooMediaThumbnail> mediaObjectThumbnails;

        /// <summary>
        /// Private member to hold a taxonomy that gives an indication of the type of content for the media object.
        /// </summary>
        private Collection<YahooMediaCategory> mediaObjectCategories;

        /// <summary>
        /// Private member to hold the hash digests for the media object.
        /// </summary>
        private Collection<YahooMediaHash> mediaObjectHashes;

        /// <summary>
        /// Private member to hold a web browser media player console the media object can be accessed through.
        /// </summary>
        private YahooMediaPlayer mediaObjectPlayer;

        /// <summary>
        /// Private member to hold the entities that contributed to the creation of the media object.
        /// </summary>
        private Collection<YahooMediaCredit> mediaObjectCredits;

        /// <summary>
        /// Private member to hold the copyright information for the media object.
        /// </summary>
        private YahooMediaCopyright mediaObjectCopyright;

        /// <summary>
        /// Private member to hold the text transcript, closed captioning, or lyrics for the media object.
        /// </summary>
        private Collection<YahooMediaText> mediaObjectTextSeries;

        /// <summary>
        /// Private member to hold the restrictions to be placed on aggregators that are rendering the media object.
        /// </summary>
        private Collection<YahooMediaRestriction> mediaObjectRestrictions;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaContent"/> class.
        /// </summary>
        public YahooMediaContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaContent"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="url">A <see cref="Uri"/> that represents the direct URL to this media object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaContent(Uri url)
        {
            Guard.ArgumentNotNull(url, "url");

            this.Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaContent"/> class using the supplied <see cref="YahooMediaPlayer"/>.
        /// </summary>
        /// <param name="player">A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this media object can be accessed through.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="player"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaContent(YahooMediaPlayer player)
        {
            Guard.ArgumentNotNull(player, "player");

            this.Player = player;
        }

        /// <summary>
        /// Gets or sets the kilobits per second rate of this media object.
        /// </summary>
        /// <value>The <i>kilobits</i> per second rate of this media object. The default value is <see cref="int.MinValue"/>, which indicates that no bit-rate was specified.</value>
        public int Bitrate { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets the number of audio channels in this media object.
        /// </summary>
        /// <value>The number of audio channels in this media object. The default value is <see cref="int.MinValue"/>, which indicates that no audio channels were specified.</value>
        public int Channels { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets the content type of this media object.
        /// </summary>
        /// <value>The standard MIME type of this media object.</value>
        /// <remarks>
        ///     See <a href="http://www.iana.org/assignments/media-types/">IANA MIME Media Types</a> for a listing of registered MIME types.
        /// </remarks>
        public string ContentType
        {
            get
            {
                return this.contentMimeType;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.contentMimeType = string.Empty;
                }
                else
                {
                    this.contentMimeType = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the total play time for this media object.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that represents the total playing time for this media object. The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no duration was specified.</value>
        public TimeSpan Duration { get; set; } = TimeSpan.MinValue;

        /// <summary>
        /// Gets or sets the expressed version of this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="YahooMediaExpression"/> enumeration value that represents the expressed version of this media object.
        ///     The default value is <see cref="YahooMediaExpression.None"/>, which indicates that no expression version was specified.
        /// </value>
        public YahooMediaExpression Expression { get; set; } = YahooMediaExpression.None;

        /// <summary>
        /// Gets or sets the file size of this media object.
        /// </summary>
        /// <value>The number of bytes this media object represents on disk. The default value is <see cref="long.MinValue"/>, which indicates that no file size was specified.</value>
        public long FileSize { get; set; } = long.MinValue;

        /// <summary>
        /// Gets or sets the number of frames per second for this media object.
        /// </summary>
        /// <value>The number of frames per second for this media object. The default value is <see cref="int.MinValue"/>, which indicates that no frame-rate was specified.</value>
        public int FrameRate { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets the height of this media object.
        /// </summary>
        /// <value>The height of this media object, typically in pixels. The default value is <see cref="int.MinValue"/>, which indicates that no height was specified.</value>
        public int Height { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if this media object is the default object in a group.
        /// </summary>
        /// <value><b>true</b> if this media object is the default object that should be used for a <see cref="YahooMediaGroup"/>; otherwise <b>false</b>.</value>
        /// <remarks>
        ///     There should <b>only</b> be one default media object per <see cref="YahooMediaGroup"/>.
        /// </remarks>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the primary language encapsulated in this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="CultureInfo"/> that represents the primary language encapsulated in this media object.
        ///     The default value is a <b>null</b> reference, which indicates no language was specified.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
        ///     </para>
        /// </remarks>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the content medium of this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="YahooMediaMedium"/> enumeration value that represents the type of this media object.
        ///     The default value is <see cref="YahooMediaMedium.None"/>, which indicates that no content medium was specified.
        /// </value>
        public YahooMediaMedium Medium { get; set; } = YahooMediaMedium.None;

        /// <summary>
        /// Gets or sets the number of samples per second taken to create this media object.
        /// </summary>
        /// <value>The number of samples per second taken to create this media object. The default value is <see cref="decimal.MinValue"/>, which indicates that no sampling-rate was specified.</value>
        /// <remarks>
        ///     This property is expressed in thousands of samples per second (kHz).
        /// </remarks>
        public decimal SamplingRate { get; set; } = decimal.MinValue;

        /// <summary>
        /// Gets or sets the location of this media object.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the direct URL to this media object.</value>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the width of this media object.
        /// </summary>
        /// <value>The width of this media object, typically in pixels. The default value is <see cref="int.MinValue"/>, which indicates that no width was specified.</value>
        public int Width { get; set; } = int.MinValue;

        /// <summary>
        /// Gets a taxonomy that gives an indication of the type of content for this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCategory"/> objects that represent a taxonomy that gives an indication to the type of content for this media object.
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
        /// Gets or sets the copyright information for this media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaCopyright"/> that represents the copyright information for this media object.</value>
        /// <remarks>
        ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
        /// </remarks>
        public YahooMediaCopyright Copyright
        {
            get
            {
                return this.mediaObjectCopyright;
            }

            set
            {
                this.mediaObjectCopyright = value;
            }
        }

        /// <summary>
        /// Gets the entities that contributed to the creation of this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCredit"/> objects that represent the entities that contributed to the creation of this media object.
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
        /// Gets or sets the description of this media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents a short description of this media object.</value>
        /// <remarks>
        ///     Media object descriptions are typically a sentence in length.
        /// </remarks>
        public YahooMediaTextConstruct Description
        {
            get
            {
                return this.mediaObjectDescription;
            }

            set
            {
                this.mediaObjectDescription = value;
            }
        }

        /// <summary>
        /// Gets the hash digests for this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaHash"/> objects that represent the hash digests for this media object.
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
        /// Gets the relevant keywords that describe this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="string"/> objects that represent the relevant keywords that describe this media object.
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
        /// Gets or sets a web browser media player console this media object can be accessed through.
        /// </summary>
        /// <value>A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this media object can be accessed through.</value>
        public YahooMediaPlayer Player
        {
            get
            {
                return this.mediaObjectPlayer;
            }

            set
            {
                this.mediaObjectPlayer = value;
            }
        }

        /// <summary>
        /// Gets the permissible audiences for this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRating"/> objects that represent the permissible audiences for this media object.
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
        /// Gets the restrictions to be placed on aggregators that are rendering this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRestriction"/> objects that represent restrictions to be placed on aggregators that are rendering this media object.
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
        /// Gets the text transcript, closed captioning, or lyrics for this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaText"/> objects that represent text transcript, closed captioning, or lyrics for this media object.
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
        /// Gets the representative images for this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaThumbnail"/> objects that represent images that are representative of this media object.
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
        /// Gets or sets the title of this media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents the title of this media object.</value>
        public YahooMediaTextConstruct Title
        {
            get
            {
                return this.mediaObjectTitle;
            }

            set
            {
                this.mediaObjectTitle = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaContent first, YahooMediaContent second)
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
        public static bool operator !=(YahooMediaContent first, YahooMediaContent second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaContent first, YahooMediaContent second)
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
        public static bool operator >(YahooMediaContent first, YahooMediaContent second)
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

            YahooMediaContent value = obj as YahooMediaContent;

            if (value != null)
            {
                int result = this.Bitrate.CompareTo(value.Bitrate);
                result = result | this.Channels.CompareTo(value.Channels);
                result = result | string.Compare(this.ContentType, value.ContentType, StringComparison.OrdinalIgnoreCase);
                result = result | this.Duration.CompareTo(value.Duration);
                result = result | this.Expression.CompareTo(value.Expression);
                result = result | this.FileSize.CompareTo(value.FileSize);
                result = result | this.FrameRate.CompareTo(value.FrameRate);
                result = result | this.Height.CompareTo(value.Height);
                result = result | this.IsDefault.CompareTo(value.IsDefault);

                string sourceLanguageName = this.Language != null ? this.Language.Name : string.Empty;
                string targetLanguageName = value.Language != null ? value.Language.Name : string.Empty;
                result = result | string.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);

                result = result | this.Medium.CompareTo(value.Medium);
                result = result | this.SamplingRate.CompareTo(value.SamplingRate);
                result = result | Uri.Compare(this.Url, value.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | this.Width.CompareTo(value.Width);

                result = result | YahooMediaUtility.CompareCommonObjectEntities(this, value);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
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
        /// Loads this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            wasLoaded = this.LoadPrimary(source);

            if (this.LoadSecondary(source))
            {
                wasLoaded = true;
            }

            if (YahooMediaUtility.FillCommonObjectEntities(this, source))
            {
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaContent"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("content", extension.XmlNamespace);

            if (this.Url != null)
            {
                writer.WriteAttributeString("url", this.Url.ToString());
            }

            if (this.FileSize != long.MinValue)
            {
                writer.WriteAttributeString("fileSize", this.FileSize.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (!string.IsNullOrEmpty(this.ContentType))
            {
                writer.WriteAttributeString("type", this.ContentType);
            }

            if (this.Medium != YahooMediaMedium.None)
            {
                writer.WriteAttributeString("medium", YahooMediaSyndicationExtension.MediumAsString(this.Medium));
            }

            if (this.IsDefault)
            {
                writer.WriteAttributeString("isDefault", "true");
            }

            if (this.Expression != YahooMediaExpression.None)
            {
                writer.WriteAttributeString("expression", YahooMediaSyndicationExtension.ExpressionAsString(this.Expression));
            }

            if (this.Bitrate != int.MinValue)
            {
                writer.WriteAttributeString("bitrate", this.Bitrate.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.FrameRate != int.MinValue)
            {
                writer.WriteAttributeString("framerate", this.FrameRate.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.SamplingRate != decimal.MinValue)
            {
                writer.WriteAttributeString("samplingrate", this.SamplingRate.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Channels != int.MinValue)
            {
                writer.WriteAttributeString("channels", this.Channels.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Duration != TimeSpan.MinValue)
            {
                writer.WriteAttributeString("duration", this.Duration.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Height != int.MinValue)
            {
                writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Width != int.MinValue)
            {
                writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Language != null)
            {
                writer.WriteAttributeString("lang", this.Language.Name);
            }

            YahooMediaUtility.WriteCommonObjectEntities(this, writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaContent"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaContent"/>.</returns>
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
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is YahooMediaContent))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Loads the primary properties of this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadPrimary(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string urlAttribute = source.GetAttribute("url", string.Empty);
                string fileSizeAttribute = source.GetAttribute("fileSize", string.Empty);
                string typeAttribute = source.GetAttribute("type", string.Empty);
                string mediumAttribute = source.GetAttribute("medium", string.Empty);
                string isDefaultAttribute = source.GetAttribute("isDefault", string.Empty);
                string expressionAttribute = source.GetAttribute("expression", string.Empty);
                string bitrateAttribute = source.GetAttribute("bitrate", string.Empty);

                if (!string.IsNullOrEmpty(urlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url = url;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(fileSizeAttribute))
                {
                    long fileSize;
                    if (long.TryParse(fileSizeAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out fileSize))
                    {
                        this.FileSize = fileSize;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    this.ContentType = typeAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(mediumAttribute))
                {
                    YahooMediaMedium medium = YahooMediaSyndicationExtension.MediumByName(mediumAttribute);
                    if (medium != YahooMediaMedium.None)
                    {
                        this.Medium = medium;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(isDefaultAttribute))
                {
                    if (string.Compare(isDefaultAttribute, "true", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.IsDefault = true;
                        wasLoaded = true;
                    }
                    else if (string.Compare(isDefaultAttribute, "false", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.IsDefault = false;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(expressionAttribute))
                {
                    YahooMediaExpression expression = YahooMediaSyndicationExtension.ExpressionByName(expressionAttribute);
                    if (expression != YahooMediaExpression.None)
                    {
                        this.Expression = expression;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(bitrateAttribute))
                {
                    int bitrate;
                    if (int.TryParse(bitrateAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out bitrate))
                    {
                        this.Bitrate = bitrate;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads the secondary properties of this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadSecondary(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string frameRateAttribute = source.GetAttribute("framerate", string.Empty);
                string samplingRateAttribute = source.GetAttribute("samplingrate", string.Empty);
                string channelsAttribute = source.GetAttribute("channels", string.Empty);
                string durationAttribute = source.GetAttribute("duration", string.Empty);
                string heightAttribute = source.GetAttribute("height", string.Empty);
                string widthAttribute = source.GetAttribute("width", string.Empty);
                string languageAttribute = source.GetAttribute("lang", string.Empty);

                if (!string.IsNullOrEmpty(frameRateAttribute))
                {
                    int frameRate;
                    if (int.TryParse(frameRateAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out frameRate))
                    {
                        this.FrameRate = frameRate;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(samplingRateAttribute))
                {
                    decimal samplingRate;
                    if (decimal.TryParse(samplingRateAttribute, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out samplingRate))
                    {
                        this.SamplingRate = samplingRate;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(channelsAttribute))
                {
                    int channels;
                    if (int.TryParse(channelsAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out channels))
                    {
                        this.Channels = channels;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(durationAttribute))
                {
                    int seconds;
                    TimeSpan duration;
                    if (int.TryParse(durationAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out seconds))
                    {
                        this.Duration = new TimeSpan(0, 0, seconds);
                        wasLoaded = true;
                    }
                    else if (TimeSpan.TryParse(durationAttribute, out duration))
                    {
                        this.Duration = duration;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(heightAttribute))
                {
                    int height;
                    if (int.TryParse(heightAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out height))
                    {
                        this.Height = height;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(widthAttribute))
                {
                    int width;
                    if (int.TryParse(widthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out width))
                    {
                        this.Width = width;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(languageAttribute))
                {
                    try
                    {
                        CultureInfo language = new CultureInfo(languageAttribute);
                        this.Language = language;
                        wasLoaded = true;
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("Unable to determine CultureInfo with a name of {0}.", languageAttribute);
                    }
                }
            }

            return wasLoaded;
        }
    }
}