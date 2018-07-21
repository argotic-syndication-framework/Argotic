/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created LiveJournalUserPicture Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the picture associated with a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSyndicationExtensionContext.UserPicture"/>
    [Serializable()]
    public class LiveJournalUserPicture : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the URL of the GIF, JPEG, or PNG image.
        /// </summary>
        private Uri userPictureUrl;
        /// <summary>
        /// Private member to hold the keyword (phrase) associated with the picture.
        /// </summary>
        private string userPictureKeywords  = String.Empty;
        /// <summary>
        /// Private member to hold the image width.
        /// </summary>
        private int userPictureWidth        = Int32.MinValue;
        /// <summary>
        /// Private member to hold the image height.
        /// </summary>
        private int userPictureHeight       = Int32.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region LiveJournalUserPicture()
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class.
        /// </summary>
        public LiveJournalUserPicture()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region LiveJournalUserPicture(Uri url, string keyword, int width, int height)
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class using the supplied parameters.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="keyword"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> is greater than <b>100</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="height"/> is greater than <b>100</b>.</exception>
        public LiveJournalUserPicture(Uri url, string keyword, int width, int height)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Url        = url;
            this.Keyword    = keyword;
            this.Width      = width;
            this.Height     = height;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Height
        /// <summary>
        /// Gets or sets the height of this picture.
        /// </summary>
        /// <value>The height of this picture, in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates no height was specified.</value>
        /// <remarks>
        ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
        public int Height
        {
            get
            {
                return userPictureHeight;
            }

            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", 100);
                userPictureHeight = value;
            }
        }
        #endregion

        #region Keyword
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
                return userPictureKeywords;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                userPictureKeywords = value.Trim();
            }
        }
        #endregion

        #region Url
        /// <summary>
        /// Gets or sets the the URL this picture.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of a GIF, JPEG, or PNG for this picture.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return userPictureUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                userPictureUrl = value;
            }
        }
        #endregion

        #region Width
        /// <summary>
        /// Gets or sets the width of this picture.
        /// </summary>
        /// <value>The width of this picture, in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates no width was specified.</value>
        /// <remarks>
        ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
        public int Width
        {
            get
            {
                return userPictureWidth;
            }
            
            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", 100);
                userPictureWidth = value;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Create namespace manager to resolve prefixed elements
            //------------------------------------------------------------
            LiveJournalSyndicationExtension extension   = new LiveJournalSyndicationExtension();
            XmlNamespaceManager manager                 = extension.CreateNamespaceManager(source);

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNavigator urlNavigator     = source.SelectSingleNode("url", manager);
                XPathNavigator keywordNavigator = source.SelectSingleNode("keyword", manager);
                XPathNavigator widthNavigator   = source.SelectSingleNode("width", manager);
                XPathNavigator heightNavigator  = source.SelectSingleNode("height", manager);

                if (urlNavigator != null)
                {
                    Uri url;
                    if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url    = url;
                        wasLoaded   = true;
                    }
                }

                if (keywordNavigator != null && !String.IsNullOrEmpty(keywordNavigator.Value))
                {
                    this.Keyword    = keywordNavigator.Value;
                    wasLoaded       = true;
                }

                if (widthNavigator != null)
                {
                    int width;
                    if (Int32.TryParse(widthNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out width))
                    {
                        if (width > 100)
                        {
                            width = 100;
                        }
                        this.Width  = width;
                        wasLoaded   = true;
                    }
                }

                if (heightNavigator != null)
                {
                    int height;
                    if (Int32.TryParse(heightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out height))
                    {
                        if (height > 100)
                        {
                            height = 100;
                        }
                        this.Height = height;
                        wasLoaded   = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="LiveJournalUserPicture"/> to the specified <see cref="XmlWriter"/>.
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
            LiveJournalSyndicationExtension extension   = new LiveJournalSyndicationExtension();

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("userpic", extension.XmlNamespace);

            writer.WriteElementString("url", extension.XmlNamespace, this.Url != null ? this.Url.ToString() : String.Empty);
            writer.WriteElementString("keyword", extension.XmlNamespace, this.Keyword);
            writer.WriteElementString("width", extension.XmlNamespace, this.Width != Int32.MinValue ? this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");
            writer.WriteElementString("height", extension.XmlNamespace, this.Height != Int32.MinValue ? this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="LiveJournalUserPicture"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="LiveJournalUserPicture"/>.</returns>
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
            LiveJournalUserPicture value  = obj as LiveJournalUserPicture;

            if (value != null)
            {
                int result  = this.Height.CompareTo(value.Height);
                result      = result | String.Compare(this.Keyword, value.Keyword, StringComparison.OrdinalIgnoreCase);
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
            if (!(obj is LiveJournalUserPicture))
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
        public static bool operator ==(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        public static bool operator !=(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        public static bool operator <(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
        public static bool operator >(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
