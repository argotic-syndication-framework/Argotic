/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaPlayer Class
****************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents a means of allowing the media object to be accessed through a web browser media player console.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is required only if a direct media <see cref="YahooMediaContent.Url"/> property is not specified in the <see cref="YahooMediaContent"/> class.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class YahooMediaPlayer : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the URL of the player console.
        /// </summary>
        private Uri playerUrl;
        /// <summary>
        /// Private member to hold the height of the browser window that the player console should be opened in.
        /// </summary>
        private int playerHeight    = Int32.MinValue;
        /// <summary>
        /// Private member to hold the with of the browser window that the player console should be opened in.
        /// </summary>
        private int playerWidth     = Int32.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaPlayer()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class.
        /// </summary>
        public YahooMediaPlayer()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region YahooMediaPlayer(Uri url)
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of this player console.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaPlayer(Uri url)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Url    = url;
        }
        #endregion

        #region YahooMediaPlayer(Uri url, int height, int width)
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class using the supplied <see cref="Uri"/>, height and width.
        /// </summary>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of this player console.</param>
        /// <param name="height">The height of the browser window that this player console should be opened in.</param>
        /// <param name="width">The width of the browser window that this player console should be opened in.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaPlayer(Uri url, int height, int width) : this(url)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Height = height;
            this.Width  = width;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Height
        /// <summary>
        /// Gets or sets the height of the browser window that this player console should be opened in.
        /// </summary>
        /// <value>The height of the browser window that this player console should be opened in. The default value is <see cref="Int32.MinValue"/>, which indicates that no height was specified.</value>
        public int Height
        {
            get
            {
                return playerHeight;
            }

            set
            {
                playerHeight = value;
            }
        }
        #endregion

        #region Url
        /// <summary>
        /// Gets or sets the location of this player console.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of this player console that plays the media.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return playerUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                playerUrl = value;
            }
        }
        #endregion

        #region Width
        /// <summary>
        /// Gets or sets the width of the browser window that this player console should be opened in.
        /// </summary>
        /// <value>The width of the browser window that this player console should be opened in. The default value is <see cref="Int32.MinValue"/>, which indicates that no width was specified.</value>
        public int Width
        {
            get
            {
                return playerWidth;
            }

            set
            {
                playerWidth = value;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="YahooMediaPlayer"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaPlayer"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaPlayer"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasAttributes)
            {
                string urlAttribute     = source.GetAttribute("url", String.Empty);
                string heightAttribute  = source.GetAttribute("height", String.Empty);
                string widthAttribute   = source.GetAttribute("width", String.Empty);

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
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="YahooMediaPlayer"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Create extension instance to retrieve XML namespace
            //------------------------------------------------------------
            YahooMediaSyndicationExtension extension    = new YahooMediaSyndicationExtension();

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("player", extension.XmlNamespace);

            writer.WriteAttributeString("url", this.Url != null ? this.Url.ToString() : String.Empty);

            if (this.Height != Int32.MinValue)
            {
                writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (this.Width != Int32.MinValue)
            {
                writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaPlayer"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaPlayer"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
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
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            YahooMediaPlayer value  = obj as YahooMediaPlayer;

            if (value != null)
            {
                int result  = this.Height.CompareTo(value.Height);
                result      = result | Uri.Compare(this.Url, value.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | this.Width.CompareTo(value.Width);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is YahooMediaPlayer))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaPlayer first, YahooMediaPlayer second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
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
        #endregion

        #region != operator
        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(YahooMediaPlayer first, YahooMediaPlayer second)
        {
            return !(first == second);
        }
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaPlayer first, YahooMediaPlayer second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
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
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(YahooMediaPlayer first, YahooMediaPlayer second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
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
        #endregion
    }
}
