﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents the permissible audience for a media object.
    /// </summary>
    [Serializable]
    public class YahooMediaRating : IComparable
    {

        /// <summary>
        /// Private member to hold the permissible audience value.
        /// </summary>
        private string ratingContent = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaRating"/> class.
        /// </summary>
        public YahooMediaRating()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaRating"/> class using the supplied audience.
        /// </summary>
        /// <param name="audience">A textual value that represents the permissible audience(s) for this media object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="audience"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="audience"/> is an empty string.</exception>
        public YahooMediaRating(string audience)
        {
            this.Content = audience;
        }

        /// <summary>
        /// Gets the media simple rating scheme adult content rating.
        /// </summary>
        /// <value>The media simple rating scheme <b>adult</b> content rating value.</value>
        public static string SimpleAdultRating => "adult";

        /// <summary>
        /// Gets the media simple rating scheme non-adult content rating.
        /// </summary>
        /// <value>The media simple rating scheme <b>nonadult</b> content rating value.</value>
        public static string SimpleNonAdultRating => "nonadult";

        /// <summary>
        /// Gets the media simple rating scheme.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the media simple rating scheme. The simple scheme has a value of <b>urn:simple</b>.</value>
        public static Uri SimpleScheme => new Uri("urn:simple");

        /// <summary>
        /// Gets or sets the permissible audience for this media object.
        /// </summary>
        /// <value>A textual value that represents the permissible audience(s) for this media object.</value>
        /// <seealso cref="SimpleAdultRating"/>
        /// <seealso cref="SimpleNonAdultRating"/>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Content
        {
            get
            {
                return this.ratingContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.ratingContent = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a URI that identifies this rating scheme.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents this rating scheme. The default value is <b>null</b>.</value>
        /// <remarks>
        ///     If no rating scheme is provided, the default scheme is <b>urn:simple</b>.
        /// </remarks>
        /// <seealso cref="SimpleScheme"/>
        public Uri Scheme { get; set; }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaRating first, YahooMediaRating second)
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
        public static bool operator !=(YahooMediaRating first, YahooMediaRating second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaRating first, YahooMediaRating second)
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
        public static bool operator >(YahooMediaRating first, YahooMediaRating second)
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
        /// Loads this <see cref="YahooMediaRating"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaRating"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaRating"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string schemeAttribute = source.GetAttribute("scheme", string.Empty);
                if (!string.IsNullOrEmpty(schemeAttribute))
                {
                    Uri scheme;
                    if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out scheme))
                    {
                        this.Scheme = scheme;
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
        /// Saves the current <see cref="YahooMediaRating"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("rating", extension.XmlNamespace);

            if (this.Scheme != null)
            {
                writer.WriteAttributeString("scheme", this.Scheme.ToString());
            }

            if (!string.IsNullOrEmpty(this.Content))
            {
                writer.WriteString(this.Content);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaRating"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaRating"/>.</returns>
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

            YahooMediaRating value = obj as YahooMediaRating;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Scheme, value.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

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
            if (!(obj is YahooMediaRating))
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