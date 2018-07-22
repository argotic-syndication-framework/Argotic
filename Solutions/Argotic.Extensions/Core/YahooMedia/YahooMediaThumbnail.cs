using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents an image that can be used as a representative image for a media object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If multiple thumbnails are associated to a media object, and time coding is not at play, it is assumed that the images are in order of importance.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class YahooMediaThumbnail : IComparable
    {

        /// <summary>
        /// Private member to hold the url of the thumbnail.
        /// </summary>
        private Uri thumbnailUrl;
        /// <summary>
        /// Private member to hold the height of the thumbnail.
        /// </summary>
        private int thumbnailHeight     = Int32.MinValue;
        /// <summary>
        /// Private member to hold the width of the thumbnail.
        /// </summary>
        private int thumbnailWidth      = Int32.MinValue;
        /// <summary>
        /// Private member to hold the time offset in relation to the media object.
        /// </summary>
        private TimeSpan thumbnailTime  = TimeSpan.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class.
        /// </summary>
        public YahooMediaThumbnail()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of this thumbnail image.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaThumbnail(Uri url)
        {
            this.Url    = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class using the supplied <see cref="Uri"/>, height and width.
        /// </summary>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of this thumbnail image.</param>
        /// <param name="height">The height of this thumbnail, typically in pixels.</param>
        /// <param name="width">The width of this thumbnail, typically in pixels.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaThumbnail(Uri url, int height, int width) : this(url)
        {
            this.Height = height;
            this.Width  = width;
        }

        /// <summary>
        /// Gets or sets the height of this thumbnail.
        /// </summary>
        /// <value>The height of this thumbnail, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no height was specified.</value>
        public int Height
        {
            get
            {
                return thumbnailHeight;
            }

            set
            {
                thumbnailHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the time offset in relation to the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that represents the time offset in relation to the media object. 
        ///     The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no time offset was specified.
        /// </value>
        /// <remarks>
        ///     Typically this property is used when creating multiple keyframes within a single video.
        /// </remarks>
        public TimeSpan Time
        {
            get
            {
                return thumbnailTime;
            }

            set
            {
                thumbnailTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the location of this thumbnail image.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of this thumbnail image.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return thumbnailUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                thumbnailUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of this thumbnail.
        /// </summary>
        /// <value>The width of this thumbnail, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no width was specified.</value>
        public int Width
        {
            get
            {
                return thumbnailWidth;
            }

            set
            {
                thumbnailWidth = value;
            }
        }

        /// <summary>
        /// Loads this <see cref="YahooMediaThumbnail"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaThumbnail"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaThumbnail"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded              = false;
            Guard.ArgumentNotNull(source, "source");
            if(source.HasAttributes)
            {
                string urlAttribute     = source.GetAttribute("url", String.Empty);
                string heightAttribute  = source.GetAttribute("height", String.Empty);
                string widthAttribute   = source.GetAttribute("width", String.Empty);
                string timeAttribute    = source.GetAttribute("time", String.Empty);

                if (!String.IsNullOrEmpty(urlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url    = url;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(heightAttribute))
                {
                    int height;
                    if (Int32.TryParse(heightAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out height))
                    {
                        this.Height = height;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(widthAttribute))
                {
                    int width;
                    if (Int32.TryParse(widthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out width))
                    {
                        this.Width  = width;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(timeAttribute))
                {
                    TimeSpan time;
                    if (TimeSpan.TryParse(timeAttribute, out time))
                    {
                        this.Time   = time;
                        wasLoaded   = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaThumbnail"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension    = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("thumbnail", extension.XmlNamespace);

            writer.WriteAttributeString("url", this.Url != null ? this.Url.ToString() : String.Empty);

            if(this.Height != Int32.MinValue)
            {
                writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Width != Int32.MinValue)
            {
                writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Time != TimeSpan.MinValue)
            {
                writer.WriteAttributeString("time", this.Time.ToString());
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaThumbnail"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaThumbnail"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Fragment;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
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
            YahooMediaThumbnail value  = obj as YahooMediaThumbnail;

            if (value != null)
            {
                int result  = this.Height.CompareTo(value.Height);
                result      = result | this.Time.CompareTo(value.Time);
                result      = result | Uri.Compare(this.Url, value.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | this.Width.CompareTo(value.Width);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            if (!(obj is YahooMediaThumbnail))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaThumbnail first, YahooMediaThumbnail second)
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
        public static bool operator !=(YahooMediaThumbnail first, YahooMediaThumbnail second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaThumbnail first, YahooMediaThumbnail second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(YahooMediaThumbnail first, YahooMediaThumbnail second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
    }
}