/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcArrayValue Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents a remote procedure parameter value that represents a collection of data elements.
    /// </summary>
    /// <seealso cref="XmlRpcMessage.Parameters"/>
    /// <seealso cref="IXmlRpcValue"/>
    [Serializable()]
    public class XmlRpcArrayValue : IXmlRpcValue, IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold data elements for the array.
        /// </summary>
        private Collection<IXmlRpcValue> arrayValues;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region XmlRpcArrayValue()
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class.
        /// </summary>
        public XmlRpcArrayValue()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region XmlRpcArrayValue(XPathNodeIterator iterator)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class using the supplied <see cref="XPathNodeIterator"/>.
        /// </summary>
        /// <param name="iterator">A <see cref="XPathNodeIterator"/> that represents the <i>value</i> nodes for the array.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcArrayValue(XPathNodeIterator iterator)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(iterator, "iterator");

            //------------------------------------------------------------
            //	Parse iterator nodes to load collection
            //------------------------------------------------------------
            if (iterator.Count > 0)
            {
                while (iterator.MoveNext())
                {
                    IXmlRpcValue value;
                    if (XmlRpcClient.TryParseValue(iterator.Current, out value))
                    {
                        this.Values.Add(value);
                    }
                }
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Values
        /// <summary>
        /// Gets data elements for this array.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the data elements for this array. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<IXmlRpcValue> Values
        {
            get
            {
                if (arrayValues == null)
                {
                    arrayValues = new Collection<IXmlRpcValue>();
                }
                return arrayValues;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="XmlRpcArrayValue"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcArrayValue"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcArrayValue"/>.</para>
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
            //	Attempt to extract array information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNavigator dataNavigator    = source.SelectSingleNode("array/data");
                if (dataNavigator != null && dataNavigator.HasChildren)
                {
                    XPathNodeIterator valueIterator = dataNavigator.Select("value");
                    if (valueIterator != null && valueIterator.Count > 0)
                    {
                        while (valueIterator.MoveNext())
                        {
                            IXmlRpcValue value;
                            if (XmlRpcClient.TryParseValue(valueIterator.Current, out value))
                            {
                                this.Values.Add(value);
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
        /// Saves the current <see cref="XmlRpcArrayValue"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("value");

            writer.WriteStartElement("array");

            writer.WriteStartElement("data");
            foreach(IXmlRpcValue value in this.Values)
            {
                value.WriteTo(writer);
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcArrayValue"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcArrayValue"/>.</returns>
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
            XmlRpcArrayValue value  = obj as XmlRpcArrayValue;

            if (value != null)
            {
                int result  = XmlRpcMessage.CompareSequence(this.Values, value.Values);

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
            if (!(obj is XmlRpcArrayValue))
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
        public static bool operator ==(XmlRpcArrayValue first, XmlRpcArrayValue second)
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
        public static bool operator !=(XmlRpcArrayValue first, XmlRpcArrayValue second)
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
        public static bool operator <(XmlRpcArrayValue first, XmlRpcArrayValue second)
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
        public static bool operator >(XmlRpcArrayValue first, XmlRpcArrayValue second)
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
