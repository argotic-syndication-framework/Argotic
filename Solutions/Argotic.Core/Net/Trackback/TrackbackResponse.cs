/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/19/2008	brian.kuhn	Created TrackbackResponse Class
****************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents the response to a Trackback ping request.
    /// </summary>
    /// <seealso cref="TrackbackClient.Send(TrackbackMessage)"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackResponse class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\TrackbackClientExample.cs" 
    ///             region="TrackbackClient" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public class TrackbackResponse : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        /// <summary>
        /// Private member to hold a value indicating if the the Trackback ping request failed.
        /// </summary>
        private bool responseHasError;
        /// <summary>
        /// Private member to hold information about the cause of the Trackback ping request failure.
        /// </summary>
        private string responseErrorMessage;

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackResponse"/> class.
        /// </summary>
        /// <remarks>
        ///     The default instance of the <see cref="TrackbackResponse"/> class represents the response to a succesful ping request.
        /// </remarks>
        public TrackbackResponse()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackResponse"/> class using the supplied error message.
        /// </summary>
        /// <param name="errorMessage">Information about cause of the Trackback ping request failure.</param>
        /// <remarks>
        ///     The <paramref name="errorMessage"/> <b>must</b> be provided in a <b>UTF-8</b> character encoding.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is an empty string.</exception>
        public TrackbackResponse(string errorMessage)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(errorMessage, "errorMessage");

            responseErrorMessage    = errorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackResponse"/> class using the supplied <see cref="WebResponse"/>.
        /// </summary>
        /// <param name="response">A <see cref="WebResponse"/> object that represents the Trackback server's response to the remote procedure call.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content type.</exception>
        /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content length.</exception>
        /// <exception cref="XmlException">The <paramref name="response"/> body does not represent a valid XML document, or an error was encountered in the XML data.</exception>
        public TrackbackResponse(WebResponse response)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(response, "response");

            //------------------------------------------------------------
            //	Validate response format
            //------------------------------------------------------------
            if (String.Compare(response.ContentType, "text/xml", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(String.Format(null, "The WebResponse content type is invalid. Content type of the response was {0}", response.ContentType), "response");
            }
            else if (response.ContentLength <= 0)
            {
                throw new ArgumentException(String.Format(null, "The WebResponse content length is invalid. Content length was {0}. ", response.ContentLength), "response");
            }

            //------------------------------------------------------------
            //	Extract Trackback response information
            //------------------------------------------------------------
            using (Stream stream = response.GetResponseStream())
            {
                XmlReaderSettings settings              = new XmlReaderSettings();
                settings.ConformanceLevel               = ConformanceLevel.Document;
                settings.IgnoreComments                 = true;
                settings.IgnoreProcessingInstructions   = true;
                settings.IgnoreWhitespace               = true;
                settings.DtdProcessing = DtdProcessing.Ignore;

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    XPathDocument document  = new XPathDocument(reader);
                    XPathNavigator source   = document.CreateNavigator();

                    XPathNavigator responseNavigator    = source.SelectSingleNode("response");
                    if (responseNavigator != null)
                    {
                        this.Load(responseNavigator);
                    }
                }
            }
        }

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        /// <summary>
        /// Gets information about cause of the Trackback ping request failure.
        /// </summary>
        /// <value>Information about the cause of the Trackback ping request failure. The default value is an <b>empty</b> string.</value>
        public string ErrorMessage
        {
            get
            {
                return responseErrorMessage;
            }
        }

        /// <summary>
        /// Gets a value indicating if the the Trackback ping request failed.
        /// </summary>
        /// <value><b>true</b> if the Trackback ping response contains an error indicator; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        public bool HasError
        {
            get
            {
                return responseHasError;
            }
        }

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        /// <summary>
        /// Loads this <see cref="TrackbackResponse"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="TrackbackResponse"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="TrackbackResponse"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract response information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator errorNavigator   = source.SelectSingleNode("error");
                XPathNavigator messageNavigator = source.SelectSingleNode("message");

                if (errorNavigator != null)
                {
                    if(String.Compare(errorNavigator.Value, "0", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        responseHasError    = false;
                        wasLoaded           = true;
                    }
                    else if(String.Compare(errorNavigator.Value, "1", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        responseHasError    = true;
                        wasLoaded           = true;
                    }
                }

                if (messageNavigator != null)
                {
                    responseErrorMessage    = !String.IsNullOrEmpty(messageNavigator.Value) ? messageNavigator.Value : String.Empty;
                    wasLoaded               = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="TrackbackResponse"/> to the specified <see cref="XmlWriter"/>.
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
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("response");

            if (!String.IsNullOrEmpty(this.ErrorMessage))
            {
                writer.WriteElementString("error", "1");
                writer.WriteElementString("message", this.ErrorMessage);
            }
            else
            {
                writer.WriteElementString("error", "0");
            }

            writer.WriteEndElement();
        }

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="TrackbackMessage"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
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

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
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
            TrackbackResponse value  = obj as TrackbackResponse;

            if (value != null)
            {
                int result  = String.Compare(this.ErrorMessage, value.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                result      = result | this.HasError.CompareTo(value.HasError);

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
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is TrackbackResponse))
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
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(TrackbackResponse first, TrackbackResponse second)
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

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(TrackbackResponse first, TrackbackResponse second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(TrackbackResponse first, TrackbackResponse second)
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

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(TrackbackResponse first, TrackbackResponse second)
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
    }
}
