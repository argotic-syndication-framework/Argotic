namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents the picture associated with a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSyndicationExtensionContext.UserPicture"/>
    [Serializable]
    public class LiveJournalUserPicture : IComparable
    {
        /// <summary>
        /// Private member to hold the URL of the GIF, JPEG, or PNG image.
        /// </summary>
        private Uri userPictureUrl;

        /// <summary>
        /// Private member to hold the keyword (phrase) associated with the picture.
        /// </summary>
        private string userPictureKeywords = string.Empty;

        /// <summary>
        /// Private member to hold the image width.
        /// </summary>
        private int userPictureWidth = int.MinValue;

        /// <summary>
        /// Private member to hold the image height.
        /// </summary>
        private int userPictureHeight = int.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class.
        /// </summary>
        public LiveJournalUserPicture()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class using the supplied parameters.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> is greater than <b>100</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="height"/> is greater than <b>100</b>.</exception>
        public LiveJournalUserPicture(Uri url, string keyword, int width, int height)
        {
            this.Url = url;
            this.Keyword = keyword;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets or sets the height of this picture.
        /// </summary>
        /// <value>The height of this picture, in pixels. The default value is <see cref="int.MinValue"/>, which indicates no height was specified.</value>
        /// <remarks>
        ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
        public int Height
        {
            get
            {
                return this.userPictureHeight;
            }

            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", 100);
                this.userPictureHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the keyword or phrase associated with the picture.
        /// </summary>
        /// <value>The keyword or phrase associated with this picture.</value>
        /// <remarks>The value of this property is expected to be <i>plain text</i>, and so entity-ecoded text should be ommited.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Keyword
        {
            get
            {
                return this.userPictureKeywords;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.userPictureKeywords = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the the URL this picture.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of a GIF, JPEG, or PNG for this picture.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return this.userPictureUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.userPictureUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of this picture.
        /// </summary>
        /// <value>The width of this picture, in pixels. The default value is <see cref="int.MinValue"/>, which indicates no width was specified.</value>
        /// <remarks>
        ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
        public int Width
        {
            get
            {
                return this.userPictureWidth;
            }

            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", 100);
                this.userPictureWidth = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        public static bool operator !=(LiveJournalUserPicture first, LiveJournalUserPicture second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        public static bool operator >(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        /// Loads this <see cref="LiveJournalUserPicture"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="LiveJournalUserPicture"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalUserPicture"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");

            LiveJournalSyndicationExtension extension = new LiveJournalSyndicationExtension();
            XmlNamespaceManager manager = extension.CreateNamespaceManager(source);

            if (source.HasChildren)
            {
                XPathNavigator urlNavigator = source.SelectSingleNode("url", manager);
                XPathNavigator keywordNavigator = source.SelectSingleNode("keyword", manager);
                XPathNavigator widthNavigator = source.SelectSingleNode("width", manager);
                XPathNavigator heightNavigator = source.SelectSingleNode("height", manager);

                if (urlNavigator != null)
                {
                    Uri url;
                    if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url = url;
                        wasLoaded = true;
                    }
                }

                if (keywordNavigator != null && !string.IsNullOrEmpty(keywordNavigator.Value))
                {
                    this.Keyword = keywordNavigator.Value;
                    wasLoaded = true;
                }

                if (widthNavigator != null)
                {
                    int width;
                    if (int.TryParse(widthNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out width))
                    {
                        if (width > 100)
                        {
                            width = 100;
                        }

                        this.Width = width;
                        wasLoaded = true;
                    }
                }

                if (heightNavigator != null)
                {
                    int height;
                    if (int.TryParse(heightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out height))
                    {
                        if (height > 100)
                        {
                            height = 100;
                        }

                        this.Height = height;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="LiveJournalUserPicture"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            LiveJournalSyndicationExtension extension = new LiveJournalSyndicationExtension();
            writer.WriteStartElement("userpic", extension.XmlNamespace);

            writer.WriteElementString("url", extension.XmlNamespace, this.Url != null ? this.Url.ToString() : string.Empty);
            writer.WriteElementString("keyword", extension.XmlNamespace, this.Keyword);
            writer.WriteElementString("width", extension.XmlNamespace, this.Width != int.MinValue ? this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");
            writer.WriteElementString("height", extension.XmlNamespace, this.Height != int.MinValue ? this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");

            writer.WriteEndElement();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is LiveJournalUserPicture))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalUserPicture"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="LiveJournalUserPicture"/>.</returns>
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

            LiveJournalUserPicture value = obj as LiveJournalUserPicture;

            if (value != null)
            {
                int result = this.Height.CompareTo(value.Height);
                result = result | string.Compare(this.Keyword, value.Keyword, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Url, value.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | this.Width.CompareTo(value.Width);

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
    }
}