/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaHash Class
****************************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the hash of a binary media object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This entity can be assocaited multiple times to a media object as long as each <see cref="YahooMediaHash"/> instance has a different <see cref="Algorithm"/>.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class YahooMediaHash : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the algorithm used to create the hash.
        /// </summary>
        private YahooMediaHashAlgorithm hashAlgorithm   = YahooMediaHashAlgorithm.None;
        /// <summary>
        /// Private member to hold the hash value.
        /// </summary>
        private string hashValue                        = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaHash()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaHash"/> class.
        /// </summary>
        public YahooMediaHash()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region YahooMediaHash(string value)
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaHash"/> class using the supplied hash digest value.
        /// </summary>
        /// <param name="value">The value of this hash.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public YahooMediaHash(string value)
        {
            //------------------------------------------------------------
            //	Initialize class state suing guarded properties
            //------------------------------------------------------------
            this.Value  = value;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Algorithm
        /// <summary>
        /// Gets or sets the algorithm used to create this hash.
        /// </summary>
        /// <value>
        ///     A <see cref="YahooMediaHashAlgorithm"/> enumeration value that indicates the algorithm used to create this hash. 
        ///     The default value is <see cref="YahooMediaHashAlgorithm.None"/>, which indicates that no hash algorithm was specified.
        /// </value>
        /// <remarks>
        ///     If no algorithm is specified, it can be assumed that <see cref="YahooMediaHashAlgorithm.MD5"/> was used to create this hash.
        /// </remarks>
        public YahooMediaHashAlgorithm Algorithm
        {
            get
            {
                return hashAlgorithm;
            }

            set
            {
                hashAlgorithm = value;
            }
        }
        #endregion

        #region Value
        /// <summary>
        /// Gets or sets the value of this hash.
        /// </summary>
        /// <value>The value of this hash.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Value
        {
            get
            {
                return hashValue;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                hashValue = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region GenerateHash(Stream stream, YahooMediaHashAlgorithm algorithm)
        /// <summary>
        /// Computes the hash value for the supplied <see cref="Stream"/> using the specified <see cref="YahooMediaHashAlgorithm"/>.
        /// </summary>
        /// <param name="stream">The input to compute the hash code for.</param>
        /// <param name="algorithm">A <see cref="YahooMediaHashAlgorithm"/> enumeration value that indicates the algorithm to use.</param>
        /// <returns>The <b>base64</b> encoded result of the computed hash code.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="algorithm"/> is equal to <see cref="YahooMediaHashAlgorithm.None"/>.</exception>
        public static string GenerateHash(Stream stream, YahooMediaHashAlgorithm algorithm)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string base64EncodedHash    = String.Empty;
            MD5 md5                     = MD5.Create();
            SHA1 sha1                   = SHA1.Create();

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(stream, "stream");
            if (algorithm == YahooMediaHashAlgorithm.None)
            {
                throw new ArgumentException(String.Format(null, "Unable to generate a hash value for the {0} algorithm.", algorithm), "algorithm");
            }

            if(algorithm == YahooMediaHashAlgorithm.MD5)
            {
                byte[] hash         = md5.ComputeHash(stream);
                base64EncodedHash   = Convert.ToBase64String(hash);
            }
            else if (algorithm == YahooMediaHashAlgorithm.Sha1)
            {
                byte[] hash         = sha1.ComputeHash(stream);
                base64EncodedHash   = Convert.ToBase64String(hash);
            }

            return base64EncodedHash;
        }
        #endregion

        #region HashAlgorithmAsString(YahooMediaHashAlgorithm algorithm)
        /// <summary>
        /// Returns the hash algorithm identifier for the supplied <see cref="YahooMediaHashAlgorithm"/>.
        /// </summary>
        /// <param name="algorithm">The <see cref="YahooMediaHashAlgorithm"/> to get the hash algorithm identifier for.</param>
        /// <returns>The hash algorithm identifier for the supplied <paramref name="algorithm"/>, otherwise returns an empty string.</returns>
        public static string HashAlgorithmAsString(YahooMediaHashAlgorithm algorithm)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaHashAlgorithm).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaHashAlgorithm))
                {
                    YahooMediaHashAlgorithm hashAlgorithm   = (YahooMediaHashAlgorithm)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (hashAlgorithm == algorithm)
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

        #region HashAlgorithmByName(string name)
        /// <summary>
        /// Returns the <see cref="YahooMediaHashAlgorithm"/> enumeration value that corresponds to the specified hash algorithm name.
        /// </summary>
        /// <param name="name">The name of the hash algorithm.</param>
        /// <returns>A <see cref="YahooMediaHashAlgorithm"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaHashAlgorithm.None</b>.</returns>
        /// <remarks>This method disregards case of specified hash algorithm name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaHashAlgorithm HashAlgorithmByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            YahooMediaHashAlgorithm hashAlgorithm   = YahooMediaHashAlgorithm.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaHashAlgorithm).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaHashAlgorithm))
                {
                    YahooMediaHashAlgorithm algorithm   = (YahooMediaHashAlgorithm)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes           = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            hashAlgorithm   = algorithm;
                            break;
                        }
                    }
                }
            }

            return hashAlgorithm;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="YahooMediaHash"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaHash"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaHash"/>.
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
                string algorithmAttribute   = source.GetAttribute("algo", String.Empty);
                if (!String.IsNullOrEmpty(algorithmAttribute))
                {
                    YahooMediaHashAlgorithm algorithm   = YahooMediaHash.HashAlgorithmByName(algorithmAttribute);
                    if (algorithm != YahooMediaHashAlgorithm.None)
                    {
                        this.Algorithm  = algorithm;
                        wasLoaded       = true;
                    }
                }
            }

            if (!String.IsNullOrEmpty(source.Value))
            {
                this.Value  = source.Value;
                wasLoaded   = true;
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="YahooMediaHash"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("hash", extension.XmlNamespace);

            if(this.Algorithm != YahooMediaHashAlgorithm.None)
            {
                writer.WriteAttributeString("algo", YahooMediaHash.HashAlgorithmAsString(this.Algorithm));
            }

            if(!String.IsNullOrEmpty(this.Value))
            {
                writer.WriteString(this.Value);
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaHash"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaHash"/>.</returns>
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
            YahooMediaHash value  = obj as YahooMediaHash;

            if (value != null)
            {
                int result  = this.Algorithm.CompareTo(value.Algorithm);
                result      = result | String.Compare(this.Value, value.Value, StringComparison.Ordinal);

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
            if (!(obj is YahooMediaHash))
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
        public static bool operator ==(YahooMediaHash first, YahooMediaHash second)
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
        public static bool operator !=(YahooMediaHash first, YahooMediaHash second)
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
        public static bool operator <(YahooMediaHash first, YahooMediaHash second)
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
        public static bool operator >(YahooMediaHash first, YahooMediaHash second)
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
