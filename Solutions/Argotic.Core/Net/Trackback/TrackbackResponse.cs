﻿namespace Argotic.Net
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;

    /// <summary>
    /// Represents the response to a Trackback ping request.
    /// </summary>
    /// <seealso cref="TrackbackClient.Send(TrackbackMessage)"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackResponse class.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs"
    ///             region="TrackbackClient"
    ///         />
    ///     </code>
    /// </example>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Trackback")]
    public class TrackbackResponse : IComparable
    {
        /// <summary>
        /// Private member to hold information about the cause of the Trackback ping request failure.
        /// </summary>
        private string responseErrorMessage;

        /// <summary>
        /// Private member to hold a value indicating if the the Trackback ping request failed.
        /// </summary>
        private bool responseHasError;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackResponse"/> class.
        /// </summary>
        /// <remarks>
        ///     The default instance of the <see cref="TrackbackResponse"/> class represents the response to a succesful ping request.
        /// </remarks>
        public TrackbackResponse()
        {
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
            Guard.ArgumentNotNullOrEmptyString(errorMessage, "errorMessage");

            this.responseErrorMessage = errorMessage;
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
            Guard.ArgumentNotNull(response, "response");

            if (string.Compare(response.ContentType, "text/xml", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "The WebResponse content type is invalid. Content type of the response was {0}",
                        response.ContentType),
                    "response");
            }

            if (response.ContentLength <= 0)
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "The WebResponse content length is invalid. Content length was {0}. ",
                        response.ContentLength),
                    "response");
            }

            using (Stream stream = response.GetResponseStream())
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Document;
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
                settings.DtdProcessing = DtdProcessing.Ignore;

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    XPathDocument document = new XPathDocument(reader);
                    XPathNavigator source = document.CreateNavigator();

                    XPathNavigator responseNavigator = source.SelectSingleNode("response");
                    if (responseNavigator != null)
                    {
                        this.Load(responseNavigator);
                    }
                }
            }
        }

        /// <summary>
        /// Gets information about cause of the Trackback ping request failure.
        /// </summary>
        /// <value>Information about the cause of the Trackback ping request failure. The default value is an <b>empty</b> string.</value>
        public string ErrorMessage
        {
            get
            {
                return this.responseErrorMessage;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the the Trackback ping request failed.
        /// </summary>
        /// <value><b>true</b> if the Trackback ping response contains an error indicator; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        public bool HasError
        {
            get
            {
                return this.responseHasError;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(TrackbackResponse first, TrackbackResponse second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(TrackbackResponse first, TrackbackResponse second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
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
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
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

            TrackbackResponse value = obj as TrackbackResponse;

            if (value != null)
            {
                int result = string.Compare(this.ErrorMessage, value.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                result = result | this.HasError.CompareTo(value.HasError);

                return result;
            }

            throw new ArgumentException(
                string.Format(
                    null,
                    "obj is not of type {0}, type was found to be '{1}'.",
                    this.GetType().FullName,
                    obj.GetType().FullName),
                "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TrackbackResponse))
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
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasChildren)
            {
                XPathNavigator errorNavigator = source.SelectSingleNode("error");
                XPathNavigator messageNavigator = source.SelectSingleNode("message");

                if (errorNavigator != null)
                {
                    if (string.Compare(errorNavigator.Value, "0", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.responseHasError = false;
                        wasLoaded = true;
                    }
                    else if (string.Compare(errorNavigator.Value, "1", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.responseHasError = true;
                        wasLoaded = true;
                    }
                }

                if (messageNavigator != null)
                {
                    this.responseErrorMessage = !string.IsNullOrEmpty(messageNavigator.Value)
                                                    ? messageNavigator.Value
                                                    : string.Empty;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
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
        /// Saves the current <see cref="TrackbackResponse"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("response");

            if (!string.IsNullOrEmpty(this.ErrorMessage))
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
    }
}