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
public class SyndicationExtensionLoadedEventArgs : EventArgs, IComparable
{

    /// <summary>
    /// Private member to hold instance of event with no event data.
    /// </summary>
    private static readonly SyndicationExtensionLoadedEventArgs emptyEventArguments = new();
    /// <summary>
    /// Private member to hold read-only XPathNavigator object for navigating the XML data used to load the syndication extension.
    /// </summary>
    [NonSerialized]
    private readonly XPathNavigator eventNavigator;
    /// <summary>
    /// Private member to hold the syndication extension that resulted from the load operation.
    /// </summary>
    private readonly ISyndicationExtension eventExtension;

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
    public XPathNavigator Data
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
    public ISyndicationExtension Extension
    {
        get
        {
            return eventExtension;
        }
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
    /// </remarks>
    public override string ToString()
    {
        string name = this.Extension != null ? this.Extension.Name : string.Empty;
        string prefix = this.Extension != null ? this.Extension.XmlPrefix : string.Empty;
        string xmlNamespace = this.Extension != null ? this.Extension.XmlNamespace : string.Empty;
        string extension = this.Extension != null ? this.Extension.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
        string data = this.Data != null ? this.Data.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;

        return string.Format(null, "[SyndicationExtensionLoadedEventArgs(Name = \"{0}\", Prefix = \"{1}\", Namespace = \"{2}\", Extension = \"{3}\", Data = \"{4}\")]", name, prefix, xmlNamespace, extension, data);
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        SyndicationExtensionLoadedEventArgs? value = obj as SyndicationExtensionLoadedEventArgs;

        if (value != null)
        {
            int result = 0;

            if (this.Data != null)
            {
                if (value.Data != null)
                {
                    result |= string.Compare(this.Data.OuterXml, value.Data.OuterXml, StringComparison.Ordinal);
                }
                else
                {
                    result |= 1;
                }
            }
            else if (value.Data != null)
            {
                result |= -1;
            }

            if (this.Extension != null)
            {
                if (value.Extension != null)
                {
                    result |= string.Compare(this.Extension.ToString(), value.Extension.ToString(), StringComparison.Ordinal);
                }
                else
                {
                    result |= 1;
                }
            }
            else if (value.Extension != null)
            {
                result |= -1;
            }

            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not SyndicationExtensionLoadedEventArgs)
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
    public static bool operator ==(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
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
    public static bool operator !=(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(SyndicationExtensionLoadedEventArgs first, SyndicationExtensionLoadedEventArgs second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}