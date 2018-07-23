namespace Argotic.Net
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;

    /// <summary>
    /// Represents a structured list member.
    /// </summary>
    [Serializable]
    public class XmlRpcStructureMember : IComparable
    {
        /// <summary>
        /// Private member to hold the name of the structure member.
        /// </summary>
        private string memberName = string.Empty;

        /// <summary>
        /// Private member to hold the value of the structure member.
        /// </summary>
        private IXmlRpcValue memberValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcStructureMember"/> class.
        /// </summary>
        public XmlRpcStructureMember()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcStructureMember"/> class using the specified name and value.
        /// </summary>
        /// <param name="name">The name of this structure member.</param>
        /// <param name="value">An object that implements the <see cref="IXmlRpcValue"/> interface that represents the value of this structure member.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcStructureMember(string name, IXmlRpcValue value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the name of this structure member.
        /// </summary>
        /// <value>The name of this structure member.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Name
        {
            get
            {
                return this.memberName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.memberName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the value of this structure member.
        /// </summary>
        /// <value>An object that implements the <see cref="IXmlRpcValue"/> interface that represents the value of this structure member.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IXmlRpcValue Value
        {
            get
            {
                return this.memberValue;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.memberValue = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(XmlRpcStructureMember first, XmlRpcStructureMember second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator >(XmlRpcStructureMember first, XmlRpcStructureMember second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator !=(XmlRpcStructureMember first, XmlRpcStructureMember second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(XmlRpcStructureMember first, XmlRpcStructureMember second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
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

            XmlRpcStructureMember value = obj as XmlRpcStructureMember;

            if (value != null)
            {
                int result = string.Compare(this.ToString(), value.ToString(), StringComparison.Ordinal);

                return result;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "obj is not of type {0}, type was found to be '{1}'.",
                        this.GetType().FullName,
                        obj.GetType().FullName),
                    "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is XmlRpcStructureMember))
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
        /// Loads this <see cref="XmlRpcStructureMember"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcStructureMember"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcStructureMember"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasChildren)
            {
                XPathNavigator nameNavigator = source.SelectSingleNode("name");
                XPathNavigator valueNavigator = source.SelectSingleNode("value");

                if (nameNavigator != null && !string.IsNullOrEmpty(nameNavigator.Value))
                {
                    this.Name = nameNavigator.Value;
                    wasLoaded = true;
                }

                if (valueNavigator != null)
                {
                    IXmlRpcValue value;
                    if (XmlRpcClient.TryParseValue(valueNavigator, out value))
                    {
                        this.Value = value;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcStructureMember"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcStructureMember"/>.</returns>
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
        /// Saves the current <see cref="XmlRpcStructureMember"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("member");

            writer.WriteElementString("name", this.Name);

            if (this.Value != null)
            {
                this.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteElementString("value", string.Empty);
            }

            writer.WriteEndElement();
        }
    }
}