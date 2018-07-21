/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/08/2008	brian.kuhn	Created YahooMediaText Class
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
    /// Represents a means of allowing the inclusion of a text transcript, closed captioning, or lyrics of the media content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Many of these <see cref="YahooMediaText"/> objects are permitted to provide a time series of text. 
    ///         In such cases, it is encouraged, but not required, that the <see cref="YahooMediaText"/> objects be grouped by language and appear in time sequence order based on the start time. 
    ///         <see cref="YahooMediaText"/> objects can have overlapping start and end times.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class YahooMediaText : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the type of the embedded text.
        /// </summary>
        private YahooMediaTextConstructType textType    = YahooMediaTextConstructType.None;
        /// <summary>
        /// Private member to hold the primary language encapsulated in the media object.
        /// </summary>
        private CultureInfo textLanguage;
        /// <summary>
        /// Private member to hold the start time offset that the text starts being relevant to the media object.
        /// </summary>
        private TimeSpan textStart                      = TimeSpan.MinValue;
        /// <summary>
        /// Private member to hold the end time offset that the text stops being relevant to the media object.
        /// </summary>
        private TimeSpan textEnd                        = TimeSpan.MinValue;
        /// <summary>
        /// Private member to hold the text transcript, closed captioning, or lyrics for the media content.
        /// </summary>
        private string textContent                      = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaText()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaText"/> class.
        /// </summary>
        public YahooMediaText()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region YahooMediaText(string text)
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaText"/> class using the supplied textual content.
        /// </summary>
        /// <param name="text">The text transcript, closed captioning, or lyrics for this media content.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public YahooMediaText(string text)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Content    = text;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Content
        /// <summary>
        /// Gets or sets the content of this embedded text.
        /// </summary>
        /// <value>The text transcript, closed captioning, or lyrics for this media content.</value>
        /// <remarks>
        ///     All HTML <b>must</b> be entity-encoded.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Content
        {
            get
            {
                return textContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                textContent = value.Trim();
            }
        }
        #endregion

        #region End
        /// <summary>
        /// Gets or sets the end time offset that this text stops being relevant to the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that represents the end time offset that this text stops being relevant to the media object. 
        ///     The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no end time was specified.
        /// </value>
        /// <remarks>
        ///     If this property is not provided, and a <see cref="Start">start time</see> is used, 
        ///     it is expected that the <see cref="End">end time</see> is either the end of the clip or the start of the next <see cref="YahooMediaText"/> object.
        /// </remarks>
        /// <seealso cref="Start"/>
        public TimeSpan End
        {
            get
            {
                return textEnd;
            }

            set
            {
                textEnd = value;
            }
        }
        #endregion

        #region Language
        /// <summary>
        /// Gets or sets the primary language encapsulated in this media object.
        /// </summary>
        /// <value>
        ///     A <see cref="CultureInfo"/> that represents the natural or formal language in which the <see cref="Content"/> is written. 
        ///     The default value is a <b>null</b> reference, which indicates no language was specified.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
        ///     </para>
        /// </remarks>
        public CultureInfo Language
        {
            get
            {
                return textLanguage;
            }

            set
            {
                textLanguage = value;
            }
        }
        #endregion

        #region Start
        /// <summary>
        /// Gets or sets the start time offset that this text starts being relevant to the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that represents the start time offset that this text starts being relevant to the media object. 
        ///     The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no start time was specified.
        /// </value>
        /// <seealso cref="End"/>
        public TimeSpan Start
        {
            get
            {
                return textStart;
            }

            set
            {
                textStart = value;
            }
        }
        #endregion

        #region TextType
        /// <summary>
        /// Gets or sets the entity encoding utilized by this embedded text.
        /// </summary>
        /// <value>
        ///     An <see cref="YahooMediaTextConstruct"/> enumeration value that represents the entity encoding utilized by this embedded text. 
        ///     The default value is <see cref="YahooMediaTextConstructType.None"/>.
        /// </value>
        /// <remarks>
        ///     If no entity encoding is specified, a default value of <see cref="YahooMediaTextConstructType.Plain"/> can be assumed.
        /// </remarks>
        public YahooMediaTextConstructType TextType
        {
            get
            {
                return textType;
            }

            set
            {
                textType = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region TextTypeAsString(YahooMediaTextConstructType type)
        /// <summary>
        /// Returns the entity encoding type identifier for the supplied <see cref="YahooMediaTextConstructType"/>.
        /// </summary>
        /// <param name="type">The <see cref="YahooMediaTextConstructType"/> to get the entity encoding type identifier for.</param>
        /// <returns>The entity encoding type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        public static string TextTypeAsString(YahooMediaTextConstructType type)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType constructType   = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (constructType == type)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region TextTypeByName(string name)
        /// <summary>
        /// Returns the <see cref="YahooMediaTextConstructType"/> enumeration value that corresponds to the specified entity encoding type name.
        /// </summary>
        /// <param name="name">The name of the entity encoding type.</param>
        /// <returns>A <see cref="YahooMediaTextConstructType"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaTextConstructType.None</b>.</returns>
        /// <remarks>This method disregards case of specified entity encoding type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaTextConstructType TextTypeByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            YahooMediaTextConstructType constructType   = YahooMediaTextConstructType.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType type    = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes           = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            constructType = type;
                            break;
                        }
                    }
                }
            }

            return constructType;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="YahooMediaText"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaText"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaText"/>.
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
                string typeAttribute        = source.GetAttribute("type", String.Empty);
                string languageAttribute    = source.GetAttribute("lang", String.Empty);
                string startAttribute       = source.GetAttribute("start", String.Empty);
                string endAttribute         = source.GetAttribute("end", String.Empty);

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    YahooMediaTextConstructType type = YahooMediaTextConstruct.TextTypeByName(typeAttribute);
                    if (type != YahooMediaTextConstructType.None)
                    {
                        this.TextType   = type;
                        wasLoaded       = true;
                    }
                }

                if (!String.IsNullOrEmpty(languageAttribute))
                {
                    try
                    {
                        CultureInfo language    = new CultureInfo(languageAttribute);
                        this.Language           = language;
                        wasLoaded               = true;
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("YahooMediaText was unable to determine CultureInfo with a name of {0}.", languageAttribute);
                    }
                }

                if (!String.IsNullOrEmpty(startAttribute))
                {
                    TimeSpan start;
                    if (TimeSpan.TryParse(startAttribute, out start))
                    {
                        this.Start  = start;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(endAttribute))
                {
                    TimeSpan end;
                    if (TimeSpan.TryParse(endAttribute, out end))
                    {
                        this.End    = end;
                        wasLoaded   = true;
                    }
                }
            }

            if (!String.IsNullOrEmpty(source.Value))
            {
                this.Content    = source.Value;
                wasLoaded       = true;
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="YahooMediaText"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("text", extension.XmlNamespace);

            if (this.TextType != YahooMediaTextConstructType.None)
            {
                writer.WriteAttributeString("type", YahooMediaText.TextTypeAsString(this.TextType));
            }

            if (this.Language != null)
            {
                writer.WriteAttributeString("lang", this.Language.Name);
            }

            if (this.Start != TimeSpan.MinValue)
            {
                writer.WriteAttributeString("start", this.Start.ToString());
            }

            if (this.End != TimeSpan.MinValue)
            {
                writer.WriteAttributeString("end", this.End.ToString());
            }

            if (!String.IsNullOrEmpty(this.Content))
            {
                writer.WriteString(this.Content);
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaText"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaText"/>.</returns>
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
            YahooMediaText value  = obj as YahooMediaText;

            if (value != null)
            {
                int result  = String.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result      = result | this.End.CompareTo(value.End);

                string sourceLanguageName   = this.Language != null ? this.Language.Name : String.Empty;
                string targetLanguageName   = value.Language != null ? value.Language.Name : String.Empty;
                result      = result | String.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);

                result      = result | this.Start.CompareTo(value.Start);
                result      = result | this.TextType.CompareTo(value.TextType);

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
            if (!(obj is YahooMediaText))
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
        public static bool operator ==(YahooMediaText first, YahooMediaText second)
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
        public static bool operator !=(YahooMediaText first, YahooMediaText second)
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
        public static bool operator <(YahooMediaText first, YahooMediaText second)
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
        public static bool operator >(YahooMediaText first, YahooMediaText second)
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
