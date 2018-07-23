namespace Argotic.Syndication
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents the pixel location of the edges of the outline window for a <see cref="OpmlDocument"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    [Serializable()]
    public class OpmlWindow : IComparable
    {

        /// <summary>
        /// Private member to hold pixel location of the top edge of the window.
        /// </summary>
        private int windowTop = int.MinValue;

        /// <summary>
        /// Private member to hold pixel location of the left edge of the window.
        /// </summary>
        private int windowLeft = int.MinValue;

        /// <summary>
        /// Private member to hold pixel location of the bottom edge of the window.
        /// </summary>
        private int windowBottom = int.MinValue;

        /// <summary>
        /// Private member to hold pixel location of the right edge of the window.
        /// </summary>
        private int windowRight = int.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlWindow"/> class.
        /// </summary>
        public OpmlWindow()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlWindow"/> class using the supplied pixel locations.
        /// </summary>
        /// <param name="top">The pixel location of the top edge of this window.</param>
        /// <param name="left">The pixel location of the left edge of this window.</param>
        /// <param name="bottom">The pixel location of the bottom edge of this window.</param>
        /// <param name="right">The pixel location of the right edge of this window.</param>
        public OpmlWindow(int top, int left, int bottom, int right)
        {
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
            this.Top = top;
        }

        /// <summary>
        /// Gets or sets the pixel location of the bottom edge of this window.
        /// </summary>
        /// <value>The pixel location of the bottom edge of this window. The default value is <see cref="int.MinValue"/>, which indicates no pixel location was specified.</value>
        public int Bottom
        {
            get
            {
                return windowBottom;
            }

            set
            {
                windowBottom = value;
            }
        }

        /// <summary>
        /// Gets or sets the pixel location of the left edge of this window.
        /// </summary>
        /// <value>The pixel location of the left edge of this window. The default value is <see cref="int.MinValue"/>, which indicates no pixel location was specified.</value>
        public int Left
        {
            get
            {
                return windowLeft;
            }

            set
            {
                windowLeft = value;
            }
        }

        /// <summary>
        /// Gets or sets the pixel location of the right edge of this window.
        /// </summary>
        /// <value>The pixel location of the right edge of this window. The default value is <see cref="int.MinValue"/>, which indicates no pixel location was specified.</value>
        public int Right
        {
            get
            {
                return windowRight;
            }

            set
            {
                windowRight = value;
            }
        }

        /// <summary>
        /// Gets or sets the pixel location of the top edge of this window.
        /// </summary>
        /// <value>The pixel location of the top edge of this window. The default value is <see cref="int.MinValue"/>, which indicates no pixel location was specified.</value>
        public int Top
        {
            get
            {
                return windowTop;
            }

            set
            {
                windowTop = value;
            }
        }

        /// <summary>
        /// Loads this <see cref="OpmlWindow"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="OpmlWindow"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XPathNavigator windowTopNavigator = source.SelectSingleNode("windowTop");
            XPathNavigator windowLeftNavigator = source.SelectSingleNode("windowLeft");
            XPathNavigator windowBottomNavigator = source.SelectSingleNode("windowBottom");
            XPathNavigator windowRightNavigator = source.SelectSingleNode("windowRight");

            if (windowTopNavigator != null)
            {
                int top;
                if (int.TryParse(windowTopNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out top))
                {
                    this.Top = top;
                    wasLoaded = true;
                }
            }

            if (windowLeftNavigator != null)
            {
                int left;
                if (int.TryParse(windowLeftNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out left))
                {
                    this.Left = left;
                    wasLoaded = true;
                }
            }

            if (windowBottomNavigator != null)
            {
                int bottom;
                if (int.TryParse(windowBottomNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out bottom))
                {
                    this.Bottom = bottom;
                    wasLoaded = true;
                }
            }

            if (windowRightNavigator != null)
            {
                int right;
                if (int.TryParse(windowRightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out right))
                {
                    this.Right = right;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="OpmlWindow"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            if(this.Top != int.MinValue)
            {
                writer.WriteElementString("windowTop", this.Top.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Left != int.MinValue)
            {
                writer.WriteElementString("windowLeft", this.Left.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Bottom != int.MinValue)
            {
                writer.WriteElementString("windowBottom", this.Bottom.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Right != int.MinValue)
            {
                writer.WriteElementString("windowRight", this.Right.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="OpmlWindow"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlWindow"/>.</returns>
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

            OpmlWindow value = obj as OpmlWindow;

            if (value != null)
            {
                int result = this.Bottom.CompareTo(value.Bottom);
                result = result | this.Left.CompareTo(value.Left);
                result = result | this.Right.CompareTo(value.Right);
                result = result | this.Top.CompareTo(value.Top);

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
            if (!(obj is OpmlWindow))
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
        public static bool operator ==(OpmlWindow first, OpmlWindow second)
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
        public static bool operator !=(OpmlWindow first, OpmlWindow second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(OpmlWindow first, OpmlWindow second)
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
        public static bool operator >(OpmlWindow first, OpmlWindow second)
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