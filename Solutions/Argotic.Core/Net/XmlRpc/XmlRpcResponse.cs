/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcResponse Class
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
    /// Represents the response to an XML remote procedure call.
    /// </summary>
    /// <seealso cref="XmlRpcClient.Send(XmlRpcMessage)"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcResponse class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\XmlRpcClientExample.cs" 
    ///             region="XmlRpcClient" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    public class XmlRpcResponse : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        /// <summary>
        /// Private member to hold the response value that was returned for the remote procedure call.
        /// </summary>
        private IXmlRpcValue responseParameter;
        /// <summary>
        /// Private member to hold the response fault information.
        /// </summary>
        private XmlRpcStructureValue responseFault;

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class.
        /// </summary>
        public XmlRpcResponse()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="IXmlRpcValue"/>.
        /// </summary>
        /// <param name="parameter">A <see cref="IXmlRpcValue"/> that represents the response value that was returned for the remote procedure call.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="parameter"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcResponse(IXmlRpcValue parameter)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(parameter, "parameter");

            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            responseParameter   = parameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="XmlRpcStructureValue"/>.
        /// </summary>
        /// <param name="fault">A <see cref="XmlRpcStructureValue"/> that represents the response to the remote procedure call.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="fault"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcResponse(XmlRpcStructureValue fault)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(fault, "fault");

            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            responseFault   = fault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied fault code and message.
        /// </summary>
        /// <param name="faultCode">Machine-readable code that identifies the reason the remote procedure call failed.</param>
        /// <param name="faultMessage">Human-readable information about the reason the remote procedure call failed.</param>
        public XmlRpcResponse(int faultCode, string faultMessage)
        {
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            XmlRpcStructureValue faultStructure = new XmlRpcStructureValue();
            XmlRpcStructureMember codeMember    = new XmlRpcStructureMember("faultCode", new XmlRpcScalarValue(faultCode));
            XmlRpcStructureMember stringMember  = new XmlRpcStructureMember("faultString", new XmlRpcScalarValue(faultMessage));
            faultStructure.Members.Add(codeMember);
            faultStructure.Members.Add(stringMember);

            responseFault   = faultStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="WebResponse"/>.
        /// </summary>
        /// <param name="response">A <see cref="WebResponse"/> object that represents the XML-RPC server's response to the remote procedure call.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content type.</exception>
        /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content length.</exception>
        /// <exception cref="XmlException">The <paramref name="response"/> body does not represent a valid XML document, or an error was encountered in the XML data.</exception>
        public XmlRpcResponse(WebResponse response)
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
            //	Extract XML-RPC response information
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

                    XPathNavigator methodResponseNavigator  = source.SelectSingleNode("methodResponse");
                    if (methodResponseNavigator != null)
                    {
                        this.Load(methodResponseNavigator);
                    }
                }
            }
        }

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        /// <summary>
        /// Gets the fault information that was returned for the remote procedure call.
        /// </summary>
        /// <value>
        ///     A <see cref="XmlRpcStructureValue"/> that represents the fault information that was returned for the remote procedure call. 
        ///     If the remote procedure call executed without errors, will return <b>null</b>.
        /// </value>
        /// <seealso cref="XmlRpcResponse(XmlRpcStructureValue)"/>
        /// <seealso cref="XmlRpcResponse(int, string)"/>
        public XmlRpcStructureValue Fault
        {
            get
            {
                return responseFault;
            }
        }

        /// <summary>
        /// Gets the response information that was returned for the remote procedure call.
        /// </summary>
        /// <value>
        ///     A <see cref="IXmlRpcValue"/> that represents the response value that was returned for the remote procedure call. 
        ///     If the remote procedure call raised an execption, will return <b>null</b> and the <see cref="Fault"/> <i>should</i> be populated.
        /// </value>
        /// <seealso cref="XmlRpcResponse(IXmlRpcValue)"/>
        public IXmlRpcValue Parameter
        {
            get
            {
                return responseParameter;
            }
        }

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        /// <summary>
        /// Loads this <see cref="XmlRpcResponse"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcResponse"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcResponse"/>.</para>
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
                XPathNavigator parametersNavigator  = source.SelectSingleNode("params");
                XPathNavigator faultNavigator       = source.SelectSingleNode("fault");

                if (parametersNavigator != null)
                {
                    XPathNavigator valueNavigator   = parametersNavigator.SelectSingleNode("param/value");
                    if (valueNavigator != null)
                    {
                        IXmlRpcValue value;
                        if (XmlRpcClient.TryParseValue(valueNavigator, out value))
                        {
                            responseParameter   = value;
                            wasLoaded           = true;
                        }
                    }
                }

                if (faultNavigator != null)
                {
                    XPathNavigator structNavigator  = faultNavigator.SelectSingleNode("value");
                    if (structNavigator != null)
                    {
                        XmlRpcStructureValue structure  = new XmlRpcStructureValue();
                        if (structure.Load(structNavigator))
                        {
                            responseFault   = structure;
                            wasLoaded       = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="XmlRpcResponse"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("methodResponse");

            if(this.Parameter != null)
            {
                writer.WriteStartElement("params");
                writer.WriteStartElement("param");
                this.Parameter.WriteTo(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            if (this.Fault != null)
            {
                writer.WriteStartElement("fault");
                this.Fault.WriteTo(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcMessage"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcMessage"/>.</returns>
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
            XmlRpcResponse value  = obj as XmlRpcResponse;

            if (value != null)
            {
                int result  = 0;

                if(this.Fault != null)
                {
                    if (value.Fault != null)
                    {
                        result  = result | this.Fault.CompareTo(value.Fault);
                    }
                    else
                    {
                        result  = result | 1;
                    }
                }
                else if(value.Fault != null)
                {
                    result  = result | -1;
                }

                if (this.Parameter != null)
                {
                    if (value.Parameter != null)
                    {
                        result  = result | String.Compare(this.Parameter.ToString(), value.Parameter.ToString(), StringComparison.Ordinal);
                    }
                    else
                    {
                        result  = result | 1;
                    }
                }
                else if (value.Parameter != null)
                {
                    result  = result | -1;
                }

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
            if (!(obj is XmlRpcResponse))
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
        public static bool operator ==(XmlRpcResponse first, XmlRpcResponse second)
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
        public static bool operator !=(XmlRpcResponse first, XmlRpcResponse second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(XmlRpcResponse first, XmlRpcResponse second)
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
        public static bool operator >(XmlRpcResponse first, XmlRpcResponse second)
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
