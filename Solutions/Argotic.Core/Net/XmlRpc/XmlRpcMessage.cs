/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcMessage Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents a remote procedure call that can be sent using the <see cref="XmlRpcClient"/> class.
    /// </summary>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcMessage class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\XmlRpcClientExample.cs" 
    ///             region="XmlRpcClient" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    public class XmlRpcMessage : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the name of the method to be called.
        /// </summary>
        private string messageMethodName    = String.Empty;
        /// <summary>
        /// Private member to hold the method parameters.
        /// </summary>
        private Collection<IXmlRpcValue> messageParameters;
        /// <summary>
        /// Private member to hold the character encoding of the message.
        /// </summary>
        private Encoding messageEncoding    = Encoding.UTF8;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region XmlRpcMessage()
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class.
        /// </summary>
        public XmlRpcMessage()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region XmlRpcMessage(string methodName)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class using the specified method name.
        /// </summary>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is an empty string.</exception>
        public XmlRpcMessage(string methodName)
        {
            //------------------------------------------------------------
            //	Initialize class using guarded properties
            //------------------------------------------------------------
            this.MethodName = methodName;
        }
        #endregion

        #region XmlRpcMessage(string methodName, Collection<IXmlRpcValue> parameters)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class using the specified method name and parameters.
        /// </summary>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <param name="parameters">A <see cref="Collection{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the method parameters.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="parameters"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcMessage(string methodName, Collection<IXmlRpcValue> parameters) : this(methodName)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(parameters, "parameters");

            //------------------------------------------------------------
            //	Load the supplied parameters
            //------------------------------------------------------------
            foreach(IXmlRpcValue parameter in parameters)
            {
                this.Parameters.Add(parameter);
            }
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

        #region MethodName
        /// <summary>
        /// Gets or sets the name of the method to be called.
        /// </summary>
        /// <value>The name of the method to be called.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string MethodName
        {
            get
            {
                return messageMethodName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                messageMethodName   = value.Trim();
            }
        }
        #endregion

        #region Parameters
        /// <summary>
        /// Gets the method parameters.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the method parameters. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<IXmlRpcValue> Parameters
        {
            get
            {
                if (messageParameters == null)
                {
                    messageParameters = new Collection<IXmlRpcValue>();
                }
                return messageParameters;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<IXmlRpcValue> source, Collection<IXmlRpcValue> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{IXmlRpcValue}"/> collections.
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
        public static int CompareSequence(Collection<IXmlRpcValue> source, Collection<IXmlRpcValue> target)
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
                for (int i = 0; i < source.Count; i++)
                {
                    IXmlRpcValue value  = source[i];
                    if(!target.Contains(value))
                    {
                        result  = -1;
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
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="XmlRpcMessage"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcMessage"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcMessage"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
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
            //	Attempt to extract scalar parameter information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator methodNameNavigator  = source.SelectSingleNode("methodName");
                XPathNavigator parametersNavigator  = source.SelectSingleNode("params");

                if (methodNameNavigator != null && !String.IsNullOrEmpty(methodNameNavigator.Value))
                {
                    this.MethodName = methodNameNavigator.Value;
                    wasLoaded       = true;
                }

                if (parametersNavigator != null)
                {
                    XPathNodeIterator valueIterator = parametersNavigator.Select("param/value");
                    if (valueIterator != null && valueIterator.Count > 0)
                    {
                        while (valueIterator.MoveNext())
                        {
                            IXmlRpcValue value;
                            if (XmlRpcClient.TryParseValue(valueIterator.Current, out value))
                            {
                                this.Parameters.Add(value);
                                wasLoaded   = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="XmlRpcMessage"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("methodCall");

            writer.WriteElementString("methodName", this.MethodName);

            if(this.Parameters.Count > 0)
            {
                writer.WriteStartElement("params");
                foreach(IXmlRpcValue value in this.Parameters)
                {
                    writer.WriteStartElement("param");
                    value.WriteTo(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
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
            XmlRpcMessage value  = obj as XmlRpcMessage;

            if (value != null)
            {
                int result  = String.Compare(this.Encoding.WebName, value.Encoding.WebName, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.MethodName, value.MethodName, StringComparison.OrdinalIgnoreCase);
                result      = result | XmlRpcMessage.CompareSequence(this.Parameters, value.Parameters);

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
            if (!(obj is XmlRpcMessage))
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
        public static bool operator ==(XmlRpcMessage first, XmlRpcMessage second)
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
        public static bool operator !=(XmlRpcMessage first, XmlRpcMessage second)
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
        public static bool operator <(XmlRpcMessage first, XmlRpcMessage second)
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
        public static bool operator >(XmlRpcMessage first, XmlRpcMessage second)
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
