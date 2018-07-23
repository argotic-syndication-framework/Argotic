namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents an entity that contributed to the creation of a media object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Current entities can include people, companies, locations, etc. Specific entities can have multiple roles,
    ///         and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class YahooMediaCredit : IComparable
    {
        /// <summary>
        /// Private member to hold the role the entity played.
        /// </summary>
        private string creditRole = string.Empty;

        /// <summary>
        /// Private member to hold the URI that identifies the role scheme.
        /// </summary>
        private Uri creditScheme;

        /// <summary>
        /// Private member to hold the name of the entity that contributed to the creation of the media object.
        /// </summary>
        private string creditEntityName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class.
        /// </summary>
        public YahooMediaCredit()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class using the supplied entity name.
        /// </summary>
        /// <param name="entity">The name of the entity that contributed to the creation of the media object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is an empty string.</exception>
        public YahooMediaCredit(string entity)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets the European Broadcasting Union Roles scheme.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the European Broadcasting Union Roles scheme, which has a value of <b>urn:ebu</b>.</value>
        /// <remarks>
        ///     This scheme can be assumed to be the default scheme for <see cref="YahooMediaCredit"/> when no scheme is provided.
        /// </remarks>
        public static Uri EuropeanBroadcastingUnionRoleScheme
        {
            get
            {
                return new Uri("urn:ebu");
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity that contributed to this media object.
        /// </summary>
        /// <value>The name of the entity that contributed to the creation of this media object.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Entity
        {
            get
            {
                return this.creditEntityName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.creditEntityName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the role the entity played in the creation of the media object.
        /// </summary>
        /// <value>The role the entity played in the creation of the media object.</value>
        /// <remarks>
        ///     All roles are converted to their lowercase equivalent. See <a href="http://www.ebu.ch/en/technical/metadata/specifications/role_codes.php">European Broadcasting Union Role Codes</a>
        ///     for a listing of the default entity roles.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public string Role
        {
            get
            {
                return this.creditRole;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.creditRole = string.Empty;
                }
                else
                {
                    this.creditRole = value.ToLowerInvariant().Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a URI that identifies this role scheme.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents this role scheme. The default value is <b>null</b>.</value>
        /// <remarks>
        ///     If no rating scheme is provided, the default scheme is <b>urn:ebu</b>.
        /// </remarks>
        /// <seealso cref="EuropeanBroadcastingUnionRoleScheme"/>
        public Uri Scheme
        {
            get
            {
                return this.creditScheme;
            }

            set
            {
                this.creditScheme = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaCredit first, YahooMediaCredit second)
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
        public static bool operator !=(YahooMediaCredit first, YahooMediaCredit second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaCredit first, YahooMediaCredit second)
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
        public static bool operator >(YahooMediaCredit first, YahooMediaCredit second)
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
        /// Loads this <see cref="YahooMediaCredit"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaCredit"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCredit"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string roleAttribute = source.GetAttribute("role", string.Empty);
                string schemeAttribute = source.GetAttribute("scheme", string.Empty);

                if (!string.IsNullOrEmpty(roleAttribute))
                {
                    this.Role = roleAttribute;
                    wasLoaded = true;
                }

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
                this.Entity = source.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaCredit"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("credit", extension.XmlNamespace);

            if (!string.IsNullOrEmpty(this.Role))
            {
                writer.WriteAttributeString("role", this.Role);
            }

            if (this.Scheme != null)
            {
                writer.WriteAttributeString("scheme", this.Scheme.ToString());
            }

            if (!string.IsNullOrEmpty(this.Entity))
            {
                writer.WriteString(this.Entity);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaCredit"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaCredit"/>.</returns>
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

            YahooMediaCredit value = obj as YahooMediaCredit;

            if (value != null)
            {
                int result = string.Compare(this.Entity, value.Entity, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Role, value.Role, StringComparison.OrdinalIgnoreCase);
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
            if (!(obj is YahooMediaCredit))
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