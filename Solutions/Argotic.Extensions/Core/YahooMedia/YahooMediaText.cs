﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

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
    [Serializable]
    public class YahooMediaText : IComparable
    {
        /// <summary>
        /// Private member to hold the text transcript, closed captioning, or lyrics for the media content.
        /// </summary>
        private string textContent = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaText"/> class.
        /// </summary>
        public YahooMediaText()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaText"/> class using the supplied textual content.
        /// </summary>
        /// <param name="text">The text transcript, closed captioning, or lyrics for this media content.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public YahooMediaText(string text)
        {
            this.Content = text;
        }

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
                return this.textContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.textContent = value.Trim();
            }
        }

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
        public TimeSpan End { get; set; } = TimeSpan.MinValue;

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
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the start time offset that this text starts being relevant to the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that represents the start time offset that this text starts being relevant to the media object.
        ///     The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no start time was specified.
        /// </value>
        /// <seealso cref="End"/>
        public TimeSpan Start { get; set; } = TimeSpan.MinValue;

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
        public YahooMediaTextConstructType TextType { get; set; } = YahooMediaTextConstructType.None;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaText first, YahooMediaText second)
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
        public static bool operator !=(YahooMediaText first, YahooMediaText second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaText first, YahooMediaText second)
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
        public static bool operator >(YahooMediaText first, YahooMediaText second)
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
        /// Returns the entity encoding type identifier for the supplied <see cref="YahooMediaTextConstructType"/>.
        /// </summary>
        /// <param name="type">The <see cref="YahooMediaTextConstructType"/> to get the entity encoding type identifier for.</param>
        /// <returns>The entity encoding type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        public static string TextTypeAsString(YahooMediaTextConstructType type)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType constructType = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (constructType == type)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

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
            YahooMediaTextConstructType constructType = YahooMediaTextConstructType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType type = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            constructType = type;
                            break;
                        }
                    }
                }
            }

            return constructType;
        }

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
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string typeAttribute = source.GetAttribute("type", string.Empty);
                string languageAttribute = source.GetAttribute("lang", string.Empty);
                string startAttribute = source.GetAttribute("start", string.Empty);
                string endAttribute = source.GetAttribute("end", string.Empty);

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    YahooMediaTextConstructType type = YahooMediaTextConstruct.TextTypeByName(typeAttribute);
                    if (type != YahooMediaTextConstructType.None)
                    {
                        this.TextType = type;
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
                        System.Diagnostics.Trace.TraceWarning("YahooMediaText was unable to determine CultureInfo with a name of {0}.", languageAttribute);
                    }
                }

                if (!string.IsNullOrEmpty(startAttribute))
                {
                    TimeSpan start;
                    if (TimeSpan.TryParse(startAttribute, out start))
                    {
                        this.Start = start;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(endAttribute))
                {
                    TimeSpan end;
                    if (TimeSpan.TryParse(endAttribute, out end))
                    {
                        this.End = end;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.Value))
            {
                this.Content = source.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaText"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
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

            if (!string.IsNullOrEmpty(this.Content))
            {
                writer.WriteString(this.Content);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaText"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaText"/>.</returns>
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

            YahooMediaText value = obj as YahooMediaText;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | this.End.CompareTo(value.End);

                string sourceLanguageName = this.Language != null ? this.Language.Name : string.Empty;
                string targetLanguageName = value.Language != null ? value.Language.Name : string.Empty;
                result = result | string.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);

                result = result | this.Start.CompareTo(value.Start);
                result = result | this.TextType.CompareTo(value.TextType);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is YahooMediaText))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
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