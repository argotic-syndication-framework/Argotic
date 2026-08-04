using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions;

/// <summary>
/// Provides data for the <see cref="ISyndicationExtension.Loaded"/> event.
/// </summary>
/// <remarks>
///     A <see cref="ISyndicationExtension.Loaded"/> event occurs whenever the <see cref="ISyndicationExtension.Load(System.Xml.XmlReader)"/>
///     or <see cref="ISyndicationExtension.Load(System.Xml.XPath.IXPathNavigable)"/> methods are called.
/// </remarks>
/// <seealso cref="ISyndicationExtension"/>
/// <seealso cref="ISyndicationExtension.Load(System.Xml.XPath.IXPathNavigable)"/>
/// <seealso cref="ISyndicationExtension.Load(System.Xml.XmlReader)"/>
[Serializable]
public class SyndicationExtensionLoadedEventArgs : EventArgs, IComparable<SyndicationExtensionLoadedEventArgs>, IEquatable<SyndicationExtensionLoadedEventArgs>, IComparisonOperators
{

    /// <summary>
    /// Private member to hold instance of event with no event data.
    /// </summary>
    private static readonly SyndicationExtensionLoadedEventArgs emptyEventArguments = new();

    /// <summary>
    /// Private member to hold read-only XPathNavigator object for navigating the XML data used to load the syndication extension.
    /// </summary>
    [NonSerialized]
    private readonly XPathNavigator? eventNavigator;

    /// <summary>
    /// Private member to hold the syndication extension that resulted from the load operation.
    /// </summary>
    private readonly ISyndicationExtension? eventExtension;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class.
    /// </summary>
    public SyndicationExtensionLoadedEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    public SyndicationExtensionLoadedEventArgs(IXPathNavigable data) : this()
    {
        ArgumentNullException.ThrowIfNull(data);

        eventNavigator = data.CreateNavigator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/> and <see cref="ISyndicationExtension"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication extension.</param>
    /// <param name="extension">
    ///     A <see cref="ISyndicationExtension"/> that represents the syndication extension after the load operation completed.
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public SyndicationExtensionLoadedEventArgs(IXPathNavigable data, ISyndicationExtension extension) : this(data)
    {
        ArgumentNullException.ThrowIfNull(extension);

        eventExtension = extension;
    }

    /// <summary>
    /// Represents an syndication extension loaded event with no event data.
    /// </summary>
    /// <value>An uninitialized instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class.</value>
    /// <remarks>The value of Empty is a read-only instance of <see cref="SyndicationExtensionLoadedEventArgs"/> equivalent to the result of calling the <see cref="SyndicationExtensionLoadedEventArgs()"/> constructor.</remarks>
    public static new SyndicationExtensionLoadedEventArgs Empty
    {
        get
        {
            return emptyEventArguments;
        }
    }

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication extension.
    /// </summary>
    /// <value>
    ///     A read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication extension.
    /// </value>
    public XPathNavigator? Data
    {
        get
        {
            return eventNavigator;
        }
    }

    /// <summary>
    /// Gets the <see cref="ISyndicationExtension"/> that resulted from the load operation.
    /// </summary>
    /// <value>
    ///     The <see cref="ISyndicationExtension"/> that resulted from the load operation. 
    /// </value>
    public ISyndicationExtension? Extension
    {
        get
        {
            return eventExtension;
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
    /// </remarks>
    public override string ToString()
    {
        string name = this.Extension?.Name ?? string.Empty;
        string prefix = this.Extension?.XmlPrefix ?? string.Empty;
        string xmlNamespace = this.Extension?.XmlNamespace ?? string.Empty;
        string extension = this.Extension?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;
        string data = this.Data?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;

        return $"[SyndicationExtensionLoadedEventArgs(Name = \"{name}\", Prefix = \"{prefix}\", Namespace = \"{xmlNamespace}\", Extension = \"{extension}\", Data = \"{data}\")]";
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SyndicationExtensionLoadedEventArgs? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = 0;

        if (this.Data != null)
        {
            if (other.Data != null)
            {
                result = string.Compare(this.Data.OuterXml, other.Data.OuterXml, StringComparison.Ordinal);
            }
            else
            {
                result = 1;
            }
        }
        else if (other.Data != null)
        {
            result = -1;
        }

        if (this.Extension != null)
        {
            if (other.Extension != null)
            {
                if (result == 0) result = string.Compare(this.Extension.ToString(), other.Extension.ToString(), StringComparison.Ordinal);
            }
            else
            {
                if (result == 0) result = 1;
            }
        }
        else if (other.Extension != null)
        {
            if (result == 0) result = -1;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationExtensionLoadedEventArgs"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationExtensionLoadedEventArgs"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SyndicationExtensionLoadedEventArgs"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SyndicationExtensionLoadedEventArgs? other)
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
        return obj is SyndicationExtensionLoadedEventArgs other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Data?.OuterXml), HashCodeUtility.Component(this.Extension?.ToString()));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SyndicationExtensionLoadedEventArgs? first, SyndicationExtensionLoadedEventArgs? second)
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
    public static bool operator !=(SyndicationExtensionLoadedEventArgs? first, SyndicationExtensionLoadedEventArgs? second)
    {
        return !(first == second);
    }

}