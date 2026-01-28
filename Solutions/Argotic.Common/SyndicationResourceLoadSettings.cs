using System.Collections.ObjectModel;
using System.Text;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Specifies a set of features to support on a <see cref="ISyndicationResource"/> object loaded by the <see cref="ISyndicationResource.Load(IXPathNavigable, SyndicationResourceLoadSettings)"/> method.
/// </summary>
[Serializable]
public sealed class SyndicationResourceLoadSettings : IComparable<SyndicationResourceLoadSettings>, IEquatable<SyndicationResourceLoadSettings>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the character encoding to use when reading the syndication resource.
    /// </summary>
    private Encoding characterEncoding = Encoding.UTF8;
    /// <summary>
    /// Private member to hold a value indicating the maximum number of resource entities to retrieve from a syndication resource.
    /// </summary>
    private int maximumEntitiesToRetrieve;
    /// <summary>
    /// Private member to hold a value that specifies the amount of time after which a asynchronous load operation call times out.
    /// </summary>
    private TimeSpan requestTimeout = TimeSpan.FromSeconds(15);
    /// <summary>
    /// Private member to hold a collection of types that represent the syndication extensions supported by the load operation.
    /// </summary>
    private Collection<Type> supportedSyndicationExtensions;
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceLoadSettings"/> class.
    /// </summary>
    public SyndicationResourceLoadSettings()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating if auto-detection of supported syndication extensions is enabled.
    /// </summary>
    /// <value>
    ///     <b>true</b> if the syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on a syndication resource; Otherwise, <b>false</b>.
    ///     The default value is <b>true</b>.
    /// </value>
    /// <remarks>
    ///     Automatic detection of supported syndication extensions will <b>not</b> remove any syndication extensions already added
    ///     to the <see cref="SupportedExtensions"/> collection prior to the load operation execution.
    /// </remarks>
    public bool AutoDetectExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the character encoding to use when parsing a syndication resource.
    /// </summary>
    /// <value>A <see cref="Encoding"/> object that indicates the character encoding to use when parsing a syndication resource. The default value is <see cref="Encoding.UTF8"/>.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Encoding CharacterEncoding
    {
        get
        {
            return characterEncoding;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            characterEncoding = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of resource entities to retrieve from a syndication resource.
    /// </summary>
    /// <value>The maximum number of entities to retrieve from a syndication resource. The default value is 0, which indicates there is <b>no limit</b>.</value>
    /// <remarks>
    ///     This setting is typically used to optimize processing by reducing the number of resource entities that must be parsed.
    ///     Some syndication resources may not utilize this setting if they do not represent a list of retrievable entities.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than zero.</exception>
    public int RetrievalLimit
    {
        get
        {
            return maximumEntitiesToRetrieve;
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            maximumEntitiesToRetrieve = value;
        }
    }

    /// <summary>
    /// Gets the syndication extensions to attempt to load from a syndication resource.
    /// </summary>
    /// <value>
    ///     A <see cref="Collection{T}"/> collection of <see cref="Type"/> objects that represent syndication extension instances to attempt to instantiate during the load operation.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     If <see cref="AutoDetectExtensions"/> is <b>true</b>, this collection will be automatically filled during the load operation based on the XML namespaces declared on the syndication resource.
    ///     Automatic detection will <b>not</b> remove any syndication extensions already added to this collection prior to the load operation execution.
    /// </remarks>
    public Collection<Type> SupportedExtensions
    {
        get
        {
            supportedSyndicationExtensions ??= new Collection<Type>();
            return supportedSyndicationExtensions;
        }
    }

    /// <summary>
    /// Gets or sets a value that specifies the amount of time after which asynchronous load operations will time-out.
    /// </summary>
    /// <value>An <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is greater than a year.</exception>
    public TimeSpan Timeout
    {
        get
        {
            return requestTimeout;
        }

        set
        {
            if (value.TotalMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else if (value > TimeSpan.FromDays(365))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else
            {
                requestTimeout = value;
            }
        }
    }

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="SyndicationResourceLoadSettings"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance.
    /// </remarks>
    public override string ToString()
    {
        return string.Format(null, "[SyndicationResourceLoadSettings(CharacterEncoding = \"{0}\", RetrievalLimit = \"{1}\", Timeout = \"{2}\", Autodetect = \"{3}\", SupportedExtensions = \"{4}\")]", this.CharacterEncoding.WebName, this.RetrievalLimit, this.Timeout.TotalMilliseconds, this.AutoDetectExtensions, this.SupportedExtensions.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SyndicationResourceLoadSettings? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.CharacterEncoding.WebName, other.CharacterEncoding.WebName, StringComparison.OrdinalIgnoreCase);
        result |= this.RetrievalLimit.CompareTo(other.RetrievalLimit);
        result |= this.Timeout.CompareTo(other.Timeout);
        result |= this.AutoDetectExtensions.CompareTo(other.AutoDetectExtensions);
        result |= ComparisonUtility.CompareSequence(this.SupportedExtensions, other.SupportedExtensions);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationResourceLoadSettings"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationResourceLoadSettings"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SyndicationResourceLoadSettings"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SyndicationResourceLoadSettings? other)
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
        return obj is SyndicationResourceLoadSettings other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.CharacterEncoding?.WebName, this.RetrievalLimit, this.Timeout, this.AutoDetectExtensions);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
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
    public static bool operator !=(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
    {
        return !(first == second);
    }

}