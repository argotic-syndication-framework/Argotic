/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/19/2008	brian.kuhn	Created TrackbackMessage Class
****************************************************************************/
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents a Trackback ping request that can be sent using the <see cref="TrackbackClient"/> class.
    /// </summary>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackMessage class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\TrackbackClientExample.cs" 
    ///             region="TrackbackClient" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public class TrackbackMessage : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the character encoding of the message.
        /// </summary>
        private Encoding messageEncoding    = Encoding.UTF8;
        /// <summary>
        /// Private member to hold the title of the entry.
        /// </summary>
        private string messageTitle         = String.Empty;
        /// <summary>
        /// Private member to hold an excerpt of the entry.
        /// </summary>
        private string messageExcerpt       = String.Empty;
        /// <summary>
        /// Private member to hold the name of the weblog to which the entry was posted.
        /// </summary>
        private string messageWeblogName    = String.Empty;
        /// <summary>
        /// Private member to hold the permalink for the entry.
        /// </summary>
        private Uri messageUrl;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region TrackbackMessage()
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessage"/> class.
        /// </summary>
        public TrackbackMessage()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region TrackbackMessage(Uri permalink)
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessage"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="permalink">A <see cref="Uri"/> that represents the permalink for the entry.</param>
        /// <remarks>
        ///     The <paramref name="permalink"/> should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "permalink")]
        public TrackbackMessage(Uri permalink)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Permalink  = permalink;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Encoding
        /// <summary>
        /// Gets or sets the <see cref="Encoding">character encoding</see> of this message.
        /// </summary>
        /// <value>A <see cref="Encoding"/> that specifies the character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Encoding Encoding
        {
            get
            {
                return messageEncoding;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                messageEncoding = value;
            }
        }
        #endregion

        #region Excerpt
        /// <summary>
        /// Gets or sets an excerpt for the entry.
        /// </summary>
        /// <value>An excerpt of the entry.</value>
        /// <remarks>
        ///     The excerpt <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string Excerpt
        {
            get
            {
                return messageExcerpt;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    messageExcerpt = String.Empty;
                }
                else
                {
                    messageExcerpt = value.Trim();
                }
            }
        }
        #endregion

        #region Permalink
        /// <summary>
        /// Gets or sets the permalink for the entry.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the permalink for the entry.</value>
        /// <remarks>
        ///     The permalink should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Permalink")]
        public Uri Permalink
        {
            get
            {
                return messageUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                messageUrl = value;
            }
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets the title of the entry.
        /// </summary>
        /// <value>The title of the entry.</value>
        /// <remarks>
        ///     The title <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string Title
        {
            get
            {
                return messageTitle;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    messageTitle = String.Empty;
                }
                else
                {
                    messageTitle = value.Trim();
                }
            }
        }
        #endregion

        #region WeblogName
        /// <summary>
        /// Gets or sets the name of the weblog to which the entry was posted.
        /// </summary>
        /// <value>The name of the weblog to which the entry was posted.</value>
        /// <remarks>
        ///     The blog name <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string WeblogName
        {
            get
            {
                return messageWeblogName;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    messageWeblogName = String.Empty;
                }
                else
                {
                    messageWeblogName = value.Trim();
                }
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(NameValueCollection source)
        /// <summary>
        /// Loads this <see cref="TrackbackMessage"/> using the supplied <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="source">The <see cref="NameValueCollection"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="TrackbackMessage"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be the <see cref="HttpRequest.Params">HTTP Request Parameters</see> or a similar subset.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(NameValueCollection source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract message information
            //------------------------------------------------------------
            if (source.Count > 0)
            {
                foreach (String parameterName in source.AllKeys)
                {
                    if (String.Compare(parameterName, "url", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Uri url;
                        if (Uri.TryCreate(source[parameterName], UriKind.RelativeOrAbsolute, out url))
                        {
                            this.Permalink  = url;
                            wasLoaded       = true;
                        }
                    }
                    else if (String.Compare(parameterName, "title", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!String.IsNullOrEmpty(source[parameterName]))
                        {
                            this.Title  = source[parameterName];
                            wasLoaded   = true;
                        }
                    }
                    else if (String.Compare(parameterName, "excerpt", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!String.IsNullOrEmpty(source[parameterName]))
                        {
                            this.Excerpt    = source[parameterName];
                            wasLoaded       = true;
                        }
                    }
                    else if (String.Compare(parameterName, "blog_name", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!String.IsNullOrEmpty(source[parameterName]))
                        {
                            this.WeblogName = source[parameterName];
                            wasLoaded       = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(StreamWriter writer)
        /// <summary>
        /// Saves the current <see cref="TrackbackMessage"/> to the specified <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(StreamWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Write representation of the current instance
            //------------------------------------------------------------
            writer.Write(String.Format(null, "url={0}", this.Permalink != null ? this.Permalink.ToString() : String.Empty));

            if(!String.IsNullOrEmpty(this.Title))
            {
                writer.Write(String.Format(null, "&title={0}", HttpUtility.UrlEncode(this.Title)));
            }

            if (!String.IsNullOrEmpty(this.WeblogName))
            {
                writer.Write(String.Format(null, "&blog_name={0}", HttpUtility.UrlEncode(this.WeblogName)));
            }

            if (!String.IsNullOrEmpty(this.Excerpt))
            {
                writer.Write(String.Format(null, "&excerpt={0}", HttpUtility.UrlEncode(this.Excerpt)));
            }
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="TrackbackMessage"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
        /// <remarks>
        ///     This method returns the URL-encoded representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, this.Encoding))
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
            TrackbackMessage value  = obj as TrackbackMessage;

            if (value != null)
            {
                int result  = String.Compare(this.Encoding.WebName, value.Encoding.WebName, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Excerpt, value.Excerpt, StringComparison.OrdinalIgnoreCase);
                result      = result | Uri.Compare(this.Permalink, value.Permalink, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.WeblogName, value.WeblogName, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is TrackbackMessage))
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
        public static bool operator ==(TrackbackMessage first, TrackbackMessage second)
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
        public static bool operator !=(TrackbackMessage first, TrackbackMessage second)
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
        public static bool operator <(TrackbackMessage first, TrackbackMessage second)
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
        public static bool operator >(TrackbackMessage first, TrackbackMessage second)
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
