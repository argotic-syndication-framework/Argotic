namespace Argotic.Net
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;

    /// <summary>
    /// Represents a scalar remote procedure parameter value.
    /// </summary>
    /// <seealso cref="XmlRpcMessage.Parameters"/>
    /// <seealso cref="IXmlRpcValue"/>
    [Serializable]
    public class XmlRpcScalarValue : IXmlRpcValue, IComparable
    {

        /// <summary>
        /// Private member to hold the value of the parameter.
        /// </summary>
        private object scalarParameterValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class.
        /// </summary>
        public XmlRpcScalarValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified array of bytes.
        /// </summary>
        /// <param name="value">An array of 8-bit unsigned integers.</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Base64"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcScalarValue(byte[] value)
        {
            Guard.ArgumentNotNull(value, "value");

            this.ValueType = XmlRpcScalarValueType.Base64;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified boolean value.
        /// </summary>
        /// <param name="value">A boolean value.</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Boolean"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        public XmlRpcScalarValue(bool value)
        {
            this.ValueType = XmlRpcScalarValueType.Boolean;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified instance in time.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/>, provided in Coordinated Universal Time (UTC).</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.DateTime"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        public XmlRpcScalarValue(DateTime value)
        {
            this.ValueType = XmlRpcScalarValueType.DateTime;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified double-precision signed floating-point number.
        /// </summary>
        /// <param name="value">A double-precision signed floating-point number.</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Double"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        public XmlRpcScalarValue(double value)
        {
            this.ValueType = XmlRpcScalarValueType.Double;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified 32-bit signed integer.
        /// </summary>
        /// <param name="value">A 32-bit signed integer.</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Integer"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        public XmlRpcScalarValue(int value)
        {
            this.ValueType = XmlRpcScalarValueType.Integer;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified series of characters.
        /// </summary>
        /// <param name="value">A series of characters.</param>
        /// <remarks>
        ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.String"/>,
        ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
        /// </remarks>
        public XmlRpcScalarValue(string value)
        {
            this.ValueType = XmlRpcScalarValueType.String;
            this.Value = !string.IsNullOrEmpty(value) ? value : string.Empty;
        }

        /// <summary>
        /// Gets or sets the value of this parameter.
        /// </summary>
        /// <value>A <see cref="object"/> that represents the value of this parameter.</value>
        /// <remarks>
        ///     <para>The <paramref name="value"/> should represent a <see cref="Type"/> that is appropriate for this parameter's <see cref="ValueType"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public object Value
        {
            get
            {
                return this.scalarParameterValue;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.scalarParameterValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of scalar value this parameter represents.
        /// </summary>
        /// <value>
        ///     A <see cref="XmlRpcScalarValueType"/> enumeration value that indicates the type of scalar value this parameter represents.
        ///     The default value is <see cref="XmlRpcScalarValueType.None"/>, which indicates that no type was specified.
        /// </value>
        /// <remarks>
        ///     <para>If no type is indicated, the type is assumed to be <see cref="XmlRpcScalarValueType.String"/>.</para>
        /// </remarks>
        /// <seealso cref="XmlRpcClient.ScalarTypeAsString(XmlRpcScalarValueType)"/>
        /// <seealso cref="XmlRpcClient.ScalarTypeByName(string)"/>
        public XmlRpcScalarValueType ValueType { get; set; } = XmlRpcScalarValueType.None;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(XmlRpcScalarValue first, XmlRpcScalarValue second)
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
        public static bool operator >(XmlRpcScalarValue first, XmlRpcScalarValue second)
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
        public static bool operator !=(XmlRpcScalarValue first, XmlRpcScalarValue second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(XmlRpcScalarValue first, XmlRpcScalarValue second)
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

            XmlRpcScalarValue value = obj as XmlRpcScalarValue;

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
            if (!(obj is XmlRpcScalarValue))
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
        /// Loads this <see cref="XmlRpcScalarValue"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcScalarValue"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcScalarValue"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasChildren)
            {
                if (source.MoveToFirstChild())
                {
                    XmlRpcScalarValueType type = XmlRpcScalarValueType.None;
                    if (string.Compare(source.Name, "i4", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Framework prefers the <int> designator for integers, so this handles when the <i4> designator is utilized.
                        type = XmlRpcScalarValueType.Integer;
                    }
                    else
                    {
                        type = XmlRpcClient.ScalarTypeByName(source.Name);
                    }

                    if (type != XmlRpcScalarValueType.None)
                    {
                        this.ValueType = type;
                        if (!string.IsNullOrEmpty(source.Value))
                        {
                            this.Value = StringAsValue(type, source.Value);
                        }

                        wasLoaded = true;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(source.Value))
                {
                    this.Value = source.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcScalarValue"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcScalarValue"/>.</returns>
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
        /// Saves the current <see cref="XmlRpcScalarValue"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("value");

            if (this.ValueType != XmlRpcScalarValueType.None)
            {
                writer.WriteStartElement(XmlRpcClient.ScalarTypeAsString(this.ValueType));
                writer.WriteString(ValueAsString(this.ValueType, this.Value));
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteString(this.Value != null ? this.Value.ToString() : string.Empty);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns an <see cref="object"/> that represents the converted value for the specified <see cref="XmlRpcScalarValueType"/>.
        /// </summary>
        /// <param name="type">The <see cref="XmlRpcScalarValueType"/> that indicates the expected data type for the scalar value.</param>
        /// <param name="scalar">The string representation of the scalar value.</param>
        /// <returns>An <see cref="object"/> that represents the converted value for the specified <paramref name="type"/> and <paramref name="scalar"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="scalar"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="scalar"/> is an empty string.</exception>
        private static object StringAsValue(XmlRpcScalarValueType type, string scalar)
        {
            object result = string.Empty;

            Guard.ArgumentNotNullOrEmptyString(scalar, "scalar");

            switch (type)
            {
                case XmlRpcScalarValueType.Base64:
                    result = Convert.FromBase64String(scalar);
                    break;

                case XmlRpcScalarValueType.Boolean:
                    bool boolean;
                    if (XmlRpcClient.TryParseBoolean(scalar, out boolean))
                    {
                        result = boolean;
                    }

                    break;

                case XmlRpcScalarValueType.DateTime:
                    result = SyndicationDateTimeUtility.ParseRfc3339DateTime(scalar);
                    break;

                case XmlRpcScalarValueType.Double:
                    result = double.Parse(scalar, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.Integer:
                    result = int.Parse(scalar, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.String:
                    result = scalar.Trim();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns the string representation of the supplied scalar value using the specified <see cref="XmlRpcScalarValueType"/>.
        /// </summary>
        /// <param name="type">The data type used to determine string representation.</param>
        /// <param name="scalar">The scalar value to convert.</param>
        /// <returns>The string representation of the current instance's <see cref="Value"/>, based on its <see cref="ValueType"/>.</returns>
        private static string ValueAsString(XmlRpcScalarValueType type, object scalar)
        {
            string value = string.Empty;

            if (scalar == null)
            {
                return string.Empty;
            }

            switch (type)
            {
                case XmlRpcScalarValueType.Base64:
                    byte[] data = scalar as byte[];
                    if (data != null)
                    {
                        value = Convert.ToBase64String(data, Base64FormattingOptions.None);
                    }
                    else
                    {
                        value = Convert.ToString(scalar, CultureInfo.InvariantCulture);
                    }

                    break;

                case XmlRpcScalarValueType.Boolean:
                    value = Convert.ToBoolean(scalar, CultureInfo.InvariantCulture) ? "1" : "0";
                    break;

                case XmlRpcScalarValueType.DateTime:
                    value = SyndicationDateTimeUtility.ToRfc3339DateTime(
                        Convert.ToDateTime(scalar, DateTimeFormatInfo.InvariantInfo));
                    break;

                case XmlRpcScalarValueType.Double:
                    value = Convert.ToDouble(scalar, NumberFormatInfo.InvariantInfo)
                        .ToString(NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.Integer:
                    value = Convert.ToInt32(scalar, NumberFormatInfo.InvariantInfo)
                        .ToString(NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.String:
                    value = Convert.ToString(scalar, CultureInfo.InvariantCulture).Trim();
                    break;
            }

            return value;
        }
    }
}