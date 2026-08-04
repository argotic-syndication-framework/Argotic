using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the pixel location of the edges of the outline window for a <see cref="OpmlDocument"/>.
/// </summary>
[Serializable]
public class OpmlWindow : IComparable<OpmlWindow>, IEquatable<OpmlWindow>, IComparisonOperators, IXmlWritable
{
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
    /// <value>The pixel location of the bottom edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
    public int Bottom { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the pixel location of the left edge of this window.
    /// </summary>
    /// <value>The pixel location of the left edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
    public int Left { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the pixel location of the right edge of this window.
    /// </summary>
    /// <value>The pixel location of the right edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
    public int Right { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the pixel location of the top edge of this window.
    /// </summary>
    /// <value>The pixel location of the top edge of this window. The default value is <see cref="Int32.MinValue"/>, which indicates no pixel location was specified.</value>
    public int Top { get; set; } = int.MinValue;

    /// <summary>
    /// Loads this <see cref="OpmlWindow"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="OpmlWindow"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator? windowTopNavigator = source.SelectSingleNode("windowTop");
        XPathNavigator? windowLeftNavigator = source.SelectSingleNode("windowLeft");
        XPathNavigator? windowBottomNavigator = source.SelectSingleNode("windowBottom");
        XPathNavigator? windowRightNavigator = source.SelectSingleNode("windowRight");

        if (windowTopNavigator != null)
        {
            if (int.TryParse(windowTopNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int top))
            {
                this.Top = top;
                wasLoaded = true;
            }
        }

        if (windowLeftNavigator != null)
        {
            if (int.TryParse(windowLeftNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int left))
            {
                this.Left = left;
                wasLoaded = true;
            }
        }

        if (windowBottomNavigator != null)
        {
            if (int.TryParse(windowBottomNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int bottom))
            {
                this.Bottom = bottom;
                wasLoaded = true;
            }
        }

        if (windowRightNavigator != null)
        {
            if (int.TryParse(windowRightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int right))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (this.Top != int.MinValue)
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
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(OpmlWindow? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Bottom.CompareTo(other.Bottom);
        if (result == 0) result = this.Left.CompareTo(other.Left);
        if (result == 0) result = this.Right.CompareTo(other.Right);
        if (result == 0) result = this.Top.CompareTo(other.Top);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="OpmlWindow"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="OpmlWindow"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="OpmlWindow"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(OpmlWindow? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is OpmlWindow other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Bottom), HashCodeUtility.Component(this.Left), HashCodeUtility.Component(this.Right), HashCodeUtility.Component(this.Top));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(OpmlWindow? first, OpmlWindow? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(OpmlWindow? first, OpmlWindow? second)
    {
        return !(first == second);
    }
}