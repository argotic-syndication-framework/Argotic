namespace Argotic.Net
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a remote procedure parameter value that represents a collection of data elements.
    /// </summary>
    /// <seealso cref="XmlRpcMessage.Parameters"/>
    /// <seealso cref="IXmlRpcValue"/>
    [Serializable()]
    public class XmlRpcArrayValue : IXmlRpcValue, IComparable
    {
        /// <summary>
        /// Private member to hold data elements for the array.
        /// </summary>
        private Collection<IXmlRpcValue> arrayValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class.
        /// </summary>
        public XmlRpcArrayValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class using the supplied <see cref="XPathNodeIterator"/>.
        /// </summary>
        /// <param name="iterator">A <see cref="XPathNodeIterator"/> that represents the <i>value</i> nodes for the array.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcArrayValue(XPathNodeIterator iterator)
        {
            Guard.ArgumentNotNull(iterator, "iterator");

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
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if(source.HasChildren)
            {
                XPathNavigator dataNavigator = source.SelectSingleNode("array/data");
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
                                wasLoaded = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="XmlRpcArrayValue"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

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

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcArrayValue"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcArrayValue"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.Indent = true;
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

            XmlRpcArrayValue value = obj as XmlRpcArrayValue;

            if (value != null)
            {
                int result = XmlRpcMessage.CompareSequence(this.Values, value.Values);

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
            if (!(obj is XmlRpcArrayValue))
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
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(XmlRpcArrayValue first, XmlRpcArrayValue second)
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
        public static bool operator !=(XmlRpcArrayValue first, XmlRpcArrayValue second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(XmlRpcArrayValue first, XmlRpcArrayValue second)
        {
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
        public static bool operator >(XmlRpcArrayValue first, XmlRpcArrayValue second)
        {
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