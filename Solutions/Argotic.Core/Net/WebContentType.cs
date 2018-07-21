/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
07/15/2008	brian.kuhn	Created WebContentType class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents the media type for the content of a <see cref="WebRequest"/> or <see cref="WebResponse"/>.
    /// </summary>
    /// <remarks>
    ///     <para>See <a href="http://www.iana.org/assignments/media-types">http://www.iana.org/assignments/media-types</a> for a listing of the registered IANA MIME media types and sub-types.</para>
    /// </remarks>
    [Serializable()]
    public class WebContentType : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the well known name for the character encoding parameter.
        /// </summary>
        private const string CHARSET_PARAMETER_NAME = "charset";
        /// <summary>
        /// Private member to hold the well known name for the type discriminator parameter.
        /// </summary>
        private const string TYPE_PARAMETER_NAME    = "type";
        /// <summary>
        /// Private member to hold the type of the media content.
        /// </summary>
        private string webContentMediaType          = String.Empty;
        /// <summary>
        /// Private member to hold the sub-type of the media content.
        /// </summary>
        private string webContentMediaSubType       = String.Empty;
        /// <summary>
        /// Private member to hold additional parameters applied to the media content.
        /// </summary>
        private Dictionary<string, string> webContentMediaParameters;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region WebContentType()
        /// <summary>
        /// Initializes a new instance of the <see cref="WebContentType"/> class.
        /// </summary>
        public WebContentType()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region WebContentType(string mediaType, string mediaSubtype)
        /// <summary>
        /// Initializes a new instance of the <see cref="WebContentType"/> class using the specified media type and sub-type.
        /// </summary>
        /// <param name="mediaType">The top-level media type used to declare the general type of data the media content represents.</param>
        /// <param name="mediaSubtype">The specific format for the general type of data the media content represents</param>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is an empty string.</exception>
        public WebContentType(string mediaType, string mediaSubtype)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.MediaType      = mediaType;
            this.MediaSubtype   = mediaSubtype;
        }
        #endregion

        #region WebContentType(string mediaType, string mediaSubtype, string discriminator)
        /// <summary>
        /// Initializes a new instance of the <see cref="WebContentType"/> class using the specified media type, sub-type and type discriminator.
        /// </summary>
        /// <param name="mediaType">The top-level media type used to declare the general type of data the media content represents.</param>
        /// <param name="mediaSubtype">The specific format for the general type of data the media content represents</param>
        /// <param name="discriminator">A string value that provides a means of discriminating the media content.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is an empty string.</exception>
        public WebContentType(string mediaType, string mediaSubtype, string discriminator) : this(mediaType, mediaSubtype)
        {
            //------------------------------------------------------------
            //	Initialize class state using specified properties
            //------------------------------------------------------------
            this.Discriminator  = discriminator;
        }
        #endregion

        #region WebContentType(string mediaType, string mediaSubtype, string discriminator, Encoding characterSet)
        /// <summary>
        /// Initializes a new instance of the <see cref="WebContentType"/> class using the specified media type, sub-type, type discriminator and character encoding.
        /// </summary>
        /// <param name="mediaType">The top-level media type used to declare the general type of data the media content represents.</param>
        /// <param name="mediaSubtype">The specific format for the general type of data the media content represents</param>
        /// <param name="discriminator">A string value that provides a means of discriminating the media content.</param>
        /// <param name="characterSet">A <see cref="Encoding"/> object that represents the character encoding of the media content.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaType"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="mediaSubtype"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="characterSet"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        public WebContentType(string mediaType, string mediaSubtype, string discriminator, Encoding characterSet) : this(mediaType, mediaSubtype, discriminator)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(characterSet, "characterSet");

            //------------------------------------------------------------
            //	Initialize class state using specified properties
            //------------------------------------------------------------
            this.CharacterSet   = characterSet.WebName;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region CharacterSet
        /// <summary>
        /// Gets or sets the character encoding of this media content.
        /// </summary>
        /// <value>
        ///     The name registered with the <a href="http://www.iana.org">Internet Assigned Numbers Authority</a> (IANA) for the character encoding of this media content. 
        ///     If no character encoding has been specified, returns <see cref="String.Empty"/>.
        /// </value>
        /// <remarks>
        ///     The <see cref="CharacterSet"/> property gets or sets the <i>charset</i> parameter within the <see cref="Parameters"/> collection. 
        ///     Specifiying <see cref="String.Empty"/> for the character encoding will remove the <i>charset</i> parameter from the <see cref="Parameters"/> collection.
        /// </remarks>
        public string CharacterSet
        {
            get
            {
                return this.Parameters.ContainsKey(CHARSET_PARAMETER_NAME) ? this.Parameters[CHARSET_PARAMETER_NAME] : String.Empty;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this.Parameters[CHARSET_PARAMETER_NAME] = value.Trim();
                }
                else
                {
                    this.Parameters.Remove(CHARSET_PARAMETER_NAME);
                }
            }
        }
        #endregion

        #region Discriminator
        /// <summary>
        /// Gets or sets the type discriminator of this media content.
        /// </summary>
        /// <value>
        ///     A value that provides a means of discriminating this media content. 
        ///     If no type discriminator has been specified, returns <see cref="String.Empty"/>.
        /// </value>
        /// <remarks>
        ///     The <see cref="Discriminator"/> property gets or sets the <i>type</i> parameter within the <see cref="Parameters"/> collection. 
        ///     The <i>type</i> parameter can be used to discrimiate between resource representations that share the same <see cref="MediaType"/> and <see cref="MediaSubtype"/>. 
        ///     Specifiying <see cref="String.Empty"/> for the type discriminator will remove the <i>type</i> parameter from the <see cref="Parameters"/> collection.
        /// </remarks>
        public string Discriminator
        {
            get
            {
                return this.Parameters.ContainsKey(TYPE_PARAMETER_NAME) ? this.Parameters[TYPE_PARAMETER_NAME] : String.Empty;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this.Parameters[TYPE_PARAMETER_NAME] = value.Trim();
                }
                else
                {
                    this.Parameters.Remove(TYPE_PARAMETER_NAME);
                }
            }
        }
        #endregion

        #region Encoding
        /// <summary>
        /// Gets the <see cref="Encoding"/> for the current <see cref="CharacterSet"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="Encoding"/> object that represents the character encoding of this media content. 
        ///     If the <see cref="CharacterSet"/> is not specified, a <b>null</b> reference (Nothing in Visual Basic) is returned.
        /// </returns>
        /// <exception cref="ArgumentException">The <see cref="CharacterSet"/> is not a valid code page name.</exception>
        /// <exception cref="ArgumentException">The code page indicated by <see cref="CharacterSet"/> is not supported by the underlying platform.</exception>
        public Encoding Encoding
        {
            get
            {
                Encoding encoding   = null;
                string characterSet = this.CharacterSet;

                if (!String.IsNullOrEmpty(characterSet))
                {
                    encoding    = Encoding.GetEncoding(characterSet);
                }

                return encoding;
            }
        }
        #endregion

        #region MediaType
        /// <summary>
        /// Gets or sets the top-level media type used to declare the general type of data this media content represents.
        /// </summary>
        /// <value>The top-level media type used to declare the general type of data this media content represents.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string MediaType
        {
            get
            {
                return webContentMediaType;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                webContentMediaType = value.Trim();
            }
        }
        #endregion

        #region MediaSubtype
        /// <summary>
        /// Gets or sets the specific format for the general type of data this media content represents.
        /// </summary>
        /// <value>The specific format for the general type of data this media content represents.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string MediaSubtype
        {
            get
            {
                return webContentMediaSubType;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                webContentMediaSubType = value.Trim();
            }
        }
        #endregion

        #region Parameters
        /// <summary>
        /// Gets the name/value pairs of additional parameters applied to this media content.
        /// </summary>
        /// <value>A <see cref="Dictionary{T, T}"/> collection of strings that represents the name/value pairs of additional parameters applied to this media content.</value>
        /// <remarks>
        ///     The <see cref="WebContentType"/> class provides accessors for the commonly used <i>type</i> and <i>charset</i> parameters through the <see cref="CharacterSet"/> and <see cref="Discriminator"/> properties.
        /// </remarks>
        /// <seealso cref="CharacterSet"/>
        /// <seealso cref="Discriminator"/>
        public Dictionary<string, string> Parameters
        {
            get
            {
                if (webContentMediaParameters == null)
                {
                    webContentMediaParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                return webContentMediaParameters;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Dictionary<string, string> source, Dictionary<string, string> target)
        /// <summary>
        /// Compares two specified <see cref="Dictionary{T, T}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Dictionary<string, string> source, Dictionary<string, string> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                foreach(string key in source.Keys)
                {
                    if(target.ContainsKey(key))
                    {
                        result  = result | String.Compare(source[key], target[key], StringComparison.Ordinal);
                    }
                    else
                    {
                        result  = result | - 1;
                        break;
                    }
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="WebContentType"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="WebContentType"/>.</returns>
        /// <remarks>
        ///     This method returns the MIME content type representation for the current instance.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            StringBuilder builder   = new StringBuilder();

            builder.Append(String.Format(null, "{0}/{1}", this.MediaType.ToLowerInvariant(), this.MediaSubtype));

            if(!String.IsNullOrEmpty(this.Discriminator))
            {
                builder.Append(String.Format(null, ";{0}={1}", TYPE_PARAMETER_NAME, this.Discriminator));
            }
            if (!String.IsNullOrEmpty(this.CharacterSet))
            {
                builder.Append(String.Format(null, ";{0}={1}", CHARSET_PARAMETER_NAME, this.CharacterSet));
            }

            foreach(string parameterName in this.Parameters.Keys)
            {
                string parameterValue   = !String.IsNullOrEmpty(this.Parameters[parameterName]) ? this.Parameters[parameterName].Trim() : String.Empty;
                if (String.Compare(parameterName, TYPE_PARAMETER_NAME, StringComparison.OrdinalIgnoreCase) != 0 && String.Compare(parameterName, CHARSET_PARAMETER_NAME, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    builder.Append(String.Format(null, ";{0}={1}", parameterName, parameterValue));
                }
            }

            return builder.ToString();
        }
        #endregion

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
            WebContentType value  = obj as WebContentType;

            if (value != null)
            {
                int result  = String.Compare(this.MediaType, value.MediaType, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.MediaSubtype, value.MediaSubtype, StringComparison.Ordinal);
                result      = result | WebContentType.CompareSequence(this.Parameters, value.Parameters);

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
            if (!(obj is WebContentType))
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
        public static bool operator ==(WebContentType first, WebContentType second)
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
        public static bool operator !=(WebContentType first, WebContentType second)
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
        public static bool operator <(WebContentType first, WebContentType second)
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
        public static bool operator >(WebContentType first, WebContentType second)
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
