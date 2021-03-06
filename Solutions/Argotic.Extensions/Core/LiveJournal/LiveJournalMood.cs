﻿using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the current mood of a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSyndicationExtensionContext.Mood"/>
    [Serializable()]
    public class LiveJournalMood : IComparable
    {

        /// <summary>
        /// Private member to hold the textual content of the current mood.
        /// </summary>
        private string moodContent  = String.Empty;
        /// <summary>
        /// Private member to hold a site specific identifier for the current mood.
        /// </summary>
        private int moodIdentifier  = Int32.MinValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalMood"/> class.
        /// </summary>
        public LiveJournalMood()
        {
        }

        /// <summary>
        /// Gets or sets the textual content that describes this mood.
        /// </summary>
        /// <value>The textual or entity encoded content that describes this mood.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Content
        {
            get
            {
                return moodContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                moodContent = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a site specific identifier for this mood.
        /// </summary>
        /// <value>A site specific identifier for this mood. The default value is <see cref="Int32.MinValue"/>, which indicates no identifier was specified.</value>
        public int Id
        {
            get
            {
                return moodIdentifier;
            }

            set
            {
                moodIdentifier = value;
            }
        }

        /// <summary>
        /// Loads this <see cref="LiveJournalMood"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="LiveJournalMood"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalMood"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded              = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string idAttribute  = source.GetAttribute("id", String.Empty);
                if (!String.IsNullOrEmpty(idAttribute))
                {
                    int id;
                    if (Int32.TryParse(idAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out id))
                    {
                        this.Id     = id;
                        wasLoaded   = true;
                    }
                }
            }

            if(!String.IsNullOrEmpty(source.Value))
            {
                this.Content    = source.Value;
                wasLoaded       = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="LiveJournalMood"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            LiveJournalSyndicationExtension extension   = new LiveJournalSyndicationExtension();
            writer.WriteStartElement("mood", extension.XmlNamespace);

            if(this.Id != Int32.MinValue)
            {
                writer.WriteAttributeString("id", this.Id.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            writer.WriteCData(this.Content);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="LiveJournalMood"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="LiveJournalMood"/>.</returns>
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
            LiveJournalMood value  = obj as LiveJournalMood;

            if (value != null)
            {
                int result  = this.Id.CompareTo(value.Id);
                result      = result | String.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is LiveJournalMood))
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
        public static bool operator ==(LiveJournalMood first, LiveJournalMood second)
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
        public static bool operator !=(LiveJournalMood first, LiveJournalMood second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(LiveJournalMood first, LiveJournalMood second)
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
        public static bool operator >(LiveJournalMood first, LiveJournalMood second)
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