/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcScalarValue Class
****************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents a scalar remote procedure parameter value. 
    /// </summary>
    /// <seealso cref="XmlRpcMessage.Parameters"/>
    /// <seealso cref="IXmlRpcValue"/>
    [Serializable()]
    public class XmlRpcScalarValue : IXmlRpcValue, IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the value of the parameter.
        /// </summary>
        private object scalarParameterValue;
        /// <summary>
        /// Private member to hold the type of scalar value the parameter represents.
        /// </summary>
        private XmlRpcScalarValueType scalarParameterType   = XmlRpcScalarValueType.None;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region XmlRpcScalarValue()
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class.
        /// </summary>
        public XmlRpcScalarValue()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region XmlRpcScalarValue(byte[] value)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(value, "value");

            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.Base64;
            this.Value      = value;
        }
        #endregion

        #region XmlRpcScalarValue(bool value)
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
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.Boolean;
            this.Value      = value;
        }
        #endregion

        #region XmlRpcScalarValue(DateTime value)
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
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.DateTime;
            this.Value      = value;
        }
        #endregion

        #region XmlRpcScalarValue(double value)
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
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.Double;
            this.Value      = value;
        }
        #endregion

        #region XmlRpcScalarValue(int value)
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
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.Integer;
            this.Value      = value;
        }
        #endregion

        #region XmlRpcScalarValue(string value)
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
            //------------------------------------------------------------
            //	Initialize class state
            //------------------------------------------------------------
            this.ValueType  = XmlRpcScalarValueType.String;
            this.Value      = !String.IsNullOrEmpty(value) ? value : String.Empty;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Value
        /// <summary>
        /// Gets or sets the value of this parameter.
        /// </summary>
        /// <value>A <see cref="Object"/> that represents the value of this parameter.</value>
        /// <remarks>
        ///     <para>The <paramref name="value"/> should represent a <see cref="Type"/> that is appropriate for this parameter's <see cref="ValueType"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public object Value
        {
            get
            {
                return scalarParameterValue;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                scalarParameterValue = value;
            }
        }
        #endregion

        #region ValueType
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
        public XmlRpcScalarValueType ValueType
        {
            get
            {
                return scalarParameterType;
            }

            set
            {
                scalarParameterType = value;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract scalar parameter information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                if (source.MoveToFirstChild())
                {
                    XmlRpcScalarValueType type  = XmlRpcScalarValueType.None;
                    if (String.Compare(source.Name, "i4", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Framework prefers the <int> designator for integers, so this handles when the <i4> designator is utilized.
                        type    = XmlRpcScalarValueType.Integer;
                    }
                    else
                    {
                        type    = XmlRpcClient.ScalarTypeByName(source.Name);
                    }

                    if (type != XmlRpcScalarValueType.None)
                    {
                        this.ValueType      = type;
                        if(!String.IsNullOrEmpty(source.Value))
                        {
                            this.Value      = XmlRpcScalarValue.StringAsValue(type, source.Value);
                        }
                        wasLoaded           = true;
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(source.Value))
                {
                    this.Value  = source.Value;
                    wasLoaded   = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="XmlRpcScalarValue"/> to the specified <see cref="XmlWriter"/>.
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

            if (this.ValueType != XmlRpcScalarValueType.None)
            {
                writer.WriteStartElement(XmlRpcClient.ScalarTypeAsString(this.ValueType));
                writer.WriteString(XmlRpcScalarValue.ValueAsString(this.ValueType, this.Value));
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteString(this.Value != null ? this.Value.ToString() : String.Empty);
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcScalarValue"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcScalarValue"/>.</returns>
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
            XmlRpcScalarValue value  = obj as XmlRpcScalarValue;

            if (value != null)
            {
                int result  = String.Compare(this.ToString(), value.ToString(), StringComparison.Ordinal);

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
            if (!(obj is XmlRpcScalarValue))
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
        public static bool operator ==(XmlRpcScalarValue first, XmlRpcScalarValue second)
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
        public static bool operator !=(XmlRpcScalarValue first, XmlRpcScalarValue second)
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
        public static bool operator <(XmlRpcScalarValue first, XmlRpcScalarValue second)
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
        public static bool operator >(XmlRpcScalarValue first, XmlRpcScalarValue second)
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

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region StringAsValue(XmlRpcScalarValueType type, string scalar)
        /// <summary>
        /// Returns an <see cref="Object"/> that represents the converted value for the specified <see cref="XmlRpcScalarValueType"/>.
        /// </summary>
        /// <param name="type">The <see cref="XmlRpcScalarValueType"/> that indicates the expected data type for the scalar value.</param>
        /// <param name="scalar">The string representation of the scalar value.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value for the specified <paramref name="type"/> and <paramref name="scalar"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="scalar"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="scalar"/> is an empty string.</exception>
        private static object StringAsValue(XmlRpcScalarValueType type, string scalar)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            object result   = String.Empty;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(scalar, "scalar");

            //------------------------------------------------------------
            //	Convert string representation to expected value type
            //------------------------------------------------------------
            switch (type)
            {
                case XmlRpcScalarValueType.Base64:
                    result      = Convert.FromBase64String(scalar);
                    break;

                case XmlRpcScalarValueType.Boolean:
                    bool boolean;
                    if (XmlRpcClient.TryParseBoolean(scalar, out boolean))
                    {
                        result  = boolean;
                    }
                    break;

                case XmlRpcScalarValueType.DateTime:
                    result      = SyndicationDateTimeUtility.ParseRfc3339DateTime(scalar);
                    break;

                case XmlRpcScalarValueType.Double:
                    result      = Double.Parse(scalar, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.Integer:
                    result      = Int32.Parse(scalar, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.String:
                    result      = scalar.Trim();
                    break;
            }

            return result;
        }
        #endregion

        #region ValueAsString(XmlRpcScalarValueType type, object scalar)
        /// <summary>
        /// Returns the string representation of the supplied scalar value using the specified <see cref="XmlRpcScalarValueType"/>.
        /// </summary>
        /// <param name="type">The data type used to determine string representation.</param>
        /// <param name="scalar">The scalar value to convert.</param>
        /// <returns>The string representation of the current instance's <see cref="Value"/>, based on its <see cref="ValueType"/>.</returns>
        private static string ValueAsString(XmlRpcScalarValueType type, object scalar)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string value    = String.Empty;

            //------------------------------------------------------------
            //	Return string representation based on data type
            //------------------------------------------------------------
            if (scalar == null)
            {
                return String.Empty;
            }

            switch (type)
            {
                case XmlRpcScalarValueType.Base64:
                    byte[] data = scalar as byte[];
                    if (data != null)
                    {
                        value   = Convert.ToBase64String(data, Base64FormattingOptions.None);
                    }
                    else
                    {
                        value   = Convert.ToString(scalar, CultureInfo.InvariantCulture);
                    }
                    break;

                case XmlRpcScalarValueType.Boolean:
                    value   = Convert.ToBoolean(scalar, CultureInfo.InvariantCulture) ? "1" : "0";
                    break;

                case XmlRpcScalarValueType.DateTime:
                    value   = SyndicationDateTimeUtility.ToRfc3339DateTime(Convert.ToDateTime(scalar, DateTimeFormatInfo.InvariantInfo));
                    break;

                case XmlRpcScalarValueType.Double:
                    value   = Convert.ToDouble(scalar, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.Integer:
                    value   = Convert.ToInt32(scalar, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo);
                    break;

                case XmlRpcScalarValueType.String:
                    value   = Convert.ToString(scalar, CultureInfo.InvariantCulture).Trim();
                    break;
            }

            return value;
        }
        #endregion
    }
}
