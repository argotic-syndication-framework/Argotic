/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaCredit Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents an entity that contributed to the creation of a media object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Current entities can include people, companies, locations, etc. Specific entities can have multiple roles, 
    ///         and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class YahooMediaCredit : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the role the entity played.
        /// </summary>
        private string creditRole       = String.Empty;
        /// <summary>
        /// Private member to hold the URI that identifies the role scheme.
        /// </summary>
        private Uri creditScheme;
        /// <summary>
        /// Private member to hold the name of the entity that contributed to the creation of the media object.
        /// </summary>
        private string creditEntityName = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaCredit()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class.
        /// </summary>
        public YahooMediaCredit()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region YahooMediaCredit(string entity)
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class using the supplied entity name.
        /// </summary>
        /// <param name="entity">The name of the entity that contributed to the creation of the media object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is an empty string.</exception>
        public YahooMediaCredit(string entity)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Entity = entity;
        }
        #endregion

        //============================================================
        //	STATIC PROPERTIES
        //============================================================
        #region EuropeanBroadcastingUnionRoleScheme
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
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Entity
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
                return creditEntityName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                creditEntityName = value.Trim();
            }
        }
        #endregion

        #region Role
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
                return creditRole;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    creditRole = String.Empty;
                }
                else
                {
                    creditRole = value.ToLowerInvariant().Trim();
                }
            }
        }
        #endregion

        #region Scheme
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
                return creditScheme;
            }

            set
            {
                creditScheme = value;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
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
                string roleAttribute    = source.GetAttribute("role", String.Empty);
                string schemeAttribute  = source.GetAttribute("scheme", String.Empty);

                if (!String.IsNullOrEmpty(roleAttribute))
                {
                    this.Role   = roleAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(schemeAttribute))
                {
                    Uri scheme;
                    if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out scheme))
                    {
                        this.Scheme = scheme;
                        wasLoaded   = true;
                    }
                }
            }

            if (!String.IsNullOrEmpty(source.Value))
            {
                this.Entity = source.Value;
                wasLoaded   = true;
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="YahooMediaCredit"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("credit", extension.XmlNamespace);

            if (!String.IsNullOrEmpty(this.Role))
            {
                writer.WriteAttributeString("role", this.Role);
            }

            if (this.Scheme != null)
            {
                writer.WriteAttributeString("scheme", this.Scheme.ToString());
            }

            if (!String.IsNullOrEmpty(this.Entity))
            {
                writer.WriteString(this.Entity);
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaCredit"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaCredit"/>.</returns>
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
            YahooMediaCredit value  = obj as YahooMediaCredit;

            if (value != null)
            {
                int result  = String.Compare(this.Entity, value.Entity, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Role, value.Role, StringComparison.OrdinalIgnoreCase);
                result      = result | Uri.Compare(this.Scheme, value.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

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
            if (!(obj is YahooMediaCredit))
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
        public static bool operator ==(YahooMediaCredit first, YahooMediaCredit second)
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
        public static bool operator !=(YahooMediaCredit first, YahooMediaCredit second)
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
        public static bool operator <(YahooMediaCredit first, YahooMediaCredit second)
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
        public static bool operator >(YahooMediaCredit first, YahooMediaCredit second)
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
