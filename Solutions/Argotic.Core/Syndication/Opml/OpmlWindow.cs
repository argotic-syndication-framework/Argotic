/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/04/2007	brian.kuhn	Created OpmlWindow Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication
{
    /// <summary>
    /// Represents the pixel location of the edges of the outline window for a <see cref="OpmlDocument"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    [Serializable()]
    public class OpmlWindow : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold pixel location of the top edge of the window.
        /// </summary>
        private int windowTop       = Int32.MinValue;
        /// <summary>
        /// Private member to hold pixel location of the left edge of the window.
        /// </summary>
        private int windowLeft      = Int32.MinValue;
        /// <summary>
        /// Private member to hold pixel location of the bottom edge of the window.
        /// </summary>
        private int windowBottom    = Int32.MinValue;
        /// <summary>
        /// Private member to hold pixel location of the right edge of the window.
        /// </summary>
        private int windowRight     = Int32.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region OpmlWindow()
        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlWindow"/> class.
        /// </summary>
        public OpmlWindow()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region OpmlWindow(int top, int left, int bottom, int right)
        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlWindow"/> class using the supplied pixel locations.
        /// </summary>
        /// <param name="top">The pixel location of the top edge of this window.</param>
        /// <param name="left">The pixel location of the left edge of this window.</param>
        /// <param name="bottom">The pixel location of the bottom edge of this window.</param>
        /// <param name="right">The pixel location of the right edge of this window.</param>
        public OpmlWindow(int top, int left, int bottom, int right)
        {
            //------------------------------------------------------------
            //	Initialize class state using property setters
            //------------------------------------------------------------
            this.Bottom     = bottom;
            this.Left       = left;
            this.Right      = right;
            this.Top        = top;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Bottom
        /// <summary>
        /// Gets or sets the pixel location of the bottom edge of this window.
        /// </summary>
        /// <value>The pixel location of the bottom edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
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
        #endregion

        #region Left
        /// <summary>
        /// Gets or sets the pixel location of the left edge of this window.
        /// </summary>
        /// <value>The pixel location of the left edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
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
        #endregion

        #region Right
        /// <summary>
        /// Gets or sets the pixel location of the right edge of this window.
        /// </summary>
        /// <value>The pixel location of the right edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
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
        #endregion

        #region Top
        /// <summary>
        /// Gets or sets the pixel location of the top edge of this window.
        /// </summary>
        /// <value>The pixel location of the top edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
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
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            XPathNavigator windowTopNavigator       = source.SelectSingleNode("windowTop");
            XPathNavigator windowLeftNavigator      = source.SelectSingleNode("windowLeft");
            XPathNavigator windowBottomNavigator    = source.SelectSingleNode("windowBottom");
            XPathNavigator windowRightNavigator     = source.SelectSingleNode("windowRight");

            if (windowTopNavigator != null)
            {
                int top;
                if (Int32.TryParse(windowTopNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out top))
                {
                    this.Top    = top;
                    wasLoaded   = true;
                }
            }

            if (windowLeftNavigator != null)
            {
                int left;
                if (Int32.TryParse(windowLeftNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out left))
                {
                    this.Left   = left;
                    wasLoaded   = true;
                }
            }

            if (windowBottomNavigator != null)
            {
                int bottom;
                if (Int32.TryParse(windowBottomNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out bottom))
                {
                    this.Bottom = bottom;
                    wasLoaded   = true;
                }
            }

            if (windowRightNavigator != null)
            {
                int right;
                if (Int32.TryParse(windowRightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out right))
                {
                    this.Right  = right;
                    wasLoaded   = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="OpmlWindow"/> to the specified <see cref="XmlWriter"/>.
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
            if(this.Top != Int32.MinValue)
            {
                writer.WriteElementString("windowTop", this.Top.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Left != Int32.MinValue)
            {
                writer.WriteElementString("windowLeft", this.Left.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Bottom != Int32.MinValue)
            {
                writer.WriteElementString("windowBottom", this.Bottom.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Right != Int32.MinValue)
            {
                writer.WriteElementString("windowRight", this.Right.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="OpmlWindow"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="OpmlWindow"/>.</returns>
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
            OpmlWindow value  = obj as OpmlWindow;

            if (value != null)
            {
                int result  = this.Bottom.CompareTo(value.Bottom);
                result      = result | this.Left.CompareTo(value.Left);
                result      = result | this.Right.CompareTo(value.Right);
                result      = result | this.Top.CompareTo(value.Top);

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
            if (!(obj is OpmlWindow))
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
        public static bool operator ==(OpmlWindow first, OpmlWindow second)
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
        public static bool operator !=(OpmlWindow first, OpmlWindow second)
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
        public static bool operator <(OpmlWindow first, OpmlWindow second)
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
        public static bool operator >(OpmlWindow first, OpmlWindow second)
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
