﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a single version of the content for its parent item.
    /// </summary>
    /// <seealso cref="SiteSummaryContentSyndicationExtensionContext.Items"/>
    [Serializable]
    public class SiteSummaryContentItem : IComparable
    {
        /// <summary>
        /// Private member to hold the textual content of the item.
        /// </summary>
        private string itemContent = string.Empty;

        /// <summary>
        /// Private member to hold a URI representing the format of the item.
        /// </summary>
        private Uri itemFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSummaryContentItem"/> class.
        /// </summary>
        public SiteSummaryContentItem()
        {
        }

        /// <summary>
        /// Gets the URI used when syndicated content is encoded as well-formed XML.
        /// </summary>
        /// <value>A <see cref="Uri"/> with a value of <b>http://www.w3.org/TR/REC-xml#dt-wellformed</b> that indicates that the encoding is well-formed XML.</value>
        /// <seealso cref="SiteSummaryContentItem.Encoding"/>
        public static Uri WellFormedXmlEncoding => new Uri("http://www.w3.org/TR/REC-xml#dt-wellformed");

        /// <summary>
        /// Gets or sets the textual content of this item.
        /// </summary>
        /// <value>The textual or entity encoded content of this item.</value>
        /// <remarks>
        ///     The value of this property <i>may</i> be entity-encoded, but will <b>always</b> be CDATA-escaped.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Content
        {
            get
            {
                return this.itemContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.itemContent = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the encoding of this item.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the encoding of this item.</value>
        /// <remarks>
        ///     An encoding is a reversable method of including content within syndicated content.
        /// </remarks>
        /// <seealso cref="SiteSummaryContentItem.WellFormedXmlEncoding"/>
        public Uri Encoding { get; set; }

        /// <summary>
        /// Gets or sets the format of this item.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the format of this item.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Format
        {
            get
            {
                return this.itemFormat;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.itemFormat = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(SiteSummaryContentItem first, SiteSummaryContentItem second)
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
        public static bool operator !=(SiteSummaryContentItem first, SiteSummaryContentItem second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(SiteSummaryContentItem first, SiteSummaryContentItem second)
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
        public static bool operator >(SiteSummaryContentItem first, SiteSummaryContentItem second)
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
        /// Loads this <see cref="SiteSummaryContentItem"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummaryContentItem"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SiteSummaryContentItem"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            SiteSummaryContentSyndicationExtension extension = new SiteSummaryContentSyndicationExtension();
            XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
            if (source.HasChildren)
            {
                XPathNavigator formatNavigator = source.SelectSingleNode("content:format", manager);
                XPathNavigator encodingNavigator = source.SelectSingleNode("content:encoding", manager);

                if (formatNavigator != null)
                {
                    Uri format;
                    if (Uri.TryCreate(formatNavigator.Value, UriKind.RelativeOrAbsolute, out format))
                    {
                        this.Format = format;
                        wasLoaded = true;
                    }
                }

                if (encodingNavigator != null)
                {
                    Uri encoding;
                    if (Uri.TryCreate(encodingNavigator.Value, UriKind.RelativeOrAbsolute, out encoding))
                    {
                        this.Encoding = encoding;
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
        /// Saves the current <see cref="SiteSummaryContentItem"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            SiteSummaryContentSyndicationExtension extension = new SiteSummaryContentSyndicationExtension();
            writer.WriteStartElement("item", extension.XmlNamespace);

            writer.WriteElementString("format", extension.XmlNamespace, this.Format != null ? this.Format.ToString() : string.Empty);

            if (this.Encoding != null)
            {
                writer.WriteElementString("encoding", extension.XmlNamespace, this.Encoding.ToString());
            }

            writer.WriteCData(this.Content);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummaryContentItem"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SiteSummaryContentItem"/>.</returns>
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

            SiteSummaryContentItem value = obj as SiteSummaryContentItem;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Encoding, value.Encoding, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);
                result = result | Uri.Compare(this.Format, value.Format, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

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
            if (!(obj is SiteSummaryContentItem))
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