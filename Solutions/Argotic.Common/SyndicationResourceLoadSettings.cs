using System.Text;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Specifies a set of features to support on a <see cref="ISyndicationResource"/> object loaded by the <see cref="ISyndicationResource.Load(IXPathNavigable, SyndicationResourceLoadSettings)"/> method.
/// </summary>
public sealed class SyndicationResourceLoadSettings : IComparable<SyndicationResourceLoadSettings>, IEquatable<SyndicationResourceLoadSettings>, IComparisonOperators
{
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
    ///     <see langword="true"/> if the syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on a syndication resource; otherwise, <see langword="false"/>.
    ///     The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    ///     Automatic detection of supported syndication extensions will <i>not</i> remove any syndication extensions already added
    ///     to the <see cref="SupportedExtensions"/> collection prior to the load operation execution.
    /// </remarks>
    public bool AutoDetectExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the character encoding to use when parsing a syndication resource.
    /// </summary>
    /// <value>
    ///     An <see cref="Encoding"/> to decode the resource with, or <see langword="null"/> to determine
    ///     the encoding from the document itself. The default is <see langword="null"/>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     Detection reads the byte-order mark if there is one and the <c>encoding</c> pseudo-attribute
    ///     of the XML declaration otherwise, falling back to <see cref="Encoding.UTF8"/> when a document
    ///     declares nothing. Setting this property overrides the declaration — which is what you want
    ///     for a server known to lie about its own encoding, and what you do not want otherwise.
    ///     </para>
    ///     <para>
    ///     This property used to default to <see cref="Encoding.UTF8"/> and to reject
    ///     <see langword="null"/>. Between them those two facts meant a settings object constructed
    ///     for an unrelated reason — a retrieval limit, say — silently overrode a correctly declared
    ///     <c>iso-8859-1</c> and replaced every accented character with U+FFFD. There was no way to
    ///     spell "detect", because the value that meant it was also the value you got by accident.
    ///     </para>
    ///     <para>
    ///     The asynchronous path spelled it as <c>CharacterEncoding == Encoding.UTF8 ? null : …</c>,
    ///     which made detection depend on <i>reference</i> equality with a singleton: an equivalent
    ///     <c>new UTF8Encoding(false)</c> decoded identically and behaved oppositely. Both are gone.
    ///     </para>
    /// </remarks>
    public Encoding? CharacterEncoding { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of resource entities to retrieve from a syndication resource.
    /// </summary>
    /// <value>The maximum number of entities to retrieve. The default value is <c>0</c>, which imposes no limit.</value>
    /// <remarks>
    ///     Bounds how many entities are built into the object model, so reading the first ten items of a
    ///     thousand-item feed costs ten items rather than a thousand. It bounds that and nothing before
    ///     it: the whole document is still downloaded and still parsed into a navigator, because the
    ///     limit is applied while walking that navigator. A resource that is not a list of retrievable
    ///     entities ignores the setting.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than zero.</exception>
    public int RetrievalLimit
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            field = value;
        }
    }
    /// <summary>
    /// The value of <see cref="MaxResponseContentLength"/> that asks for no limit at all.
    /// </summary>
    /// <remarks>
    ///     Spelled as <see cref="long.MaxValue"/> rather than as <c>0</c> or <see langword="null"/>,
    ///     both of which already mean something else on that property. Asking for no limit means
    ///     accepting whatever an origin sends, so it should read as a decision at the call site.
    /// </remarks>
    public const long Unbounded = long.MaxValue;

    /// <summary>
    /// Gets or sets the maximum number of bytes a load will accept from an HTTP response.
    /// </summary>
    /// <value>
    ///     The maximum number of bytes to accept, or <see langword="null"/> to use the default for the
    ///     type being loaded. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     A <see langword="null"/> means the loading type's format default, not unbounded. Were
    ///     it keyed on whether settings were supplied at all, a caller who constructed a settings object
    ///     for an unrelated reason would silently lose the larger allowance their document type is
    ///     entitled to. See <see cref="SyndicationContentLengthLimits"/>. Use <see cref="Unbounded"/> to
    ///     ask for no limit.
    ///     </para>
    ///     <para>
    ///     The count is of <i>decompressed</i> bytes, which is the number that matters: a few kilobytes
    ///     of gzip can expand to a megabyte. It does not bound a <see cref="Stream"/> the caller opened
    ///     themselves, only what this framework downloads.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
    public long? MaxResponseContentLength
    {
        get;
        set
        {
            if (value is { } bytes)
            {
                // Zero is rejected rather than read as "no limit". RetrievalLimit already spends that
                // meaning on zero in this same type, and a cap of "accept nothing" is indistinguishable
                // from a misconfiguration.
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytes);
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the syndication extensions to attempt to load from a syndication resource.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="Type"/> objects that represent syndication extension instances to attempt to instantiate during the load operation.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     If <see cref="AutoDetectExtensions"/> is <see langword="true"/>, this collection will be automatically filled during the load operation based on the XML namespaces declared on the syndication resource.
    ///     Automatic detection will <i>not</i> remove any syndication extensions already added to this collection prior to the load operation execution.
    /// </remarks>
    public IList<Type> SupportedExtensions => field ??= [];

    /// <summary>
    /// Gets or sets a value that specifies the amount of time after which asynchronous load operations will time-out.
    /// </summary>
    /// <value>
    ///     A <see cref="TimeSpan"/> that specifies the time-out period, or <see langword="null"/> to
    ///     impose no deadline at all. The default value is
    ///     <see cref="SyndicationEncodingUtility.DefaultRequestTimeout"/> — 100 seconds, matching the
    ///     default time-out of the framework's previous <see cref="System.Net.HttpWebRequest"/>-based pipeline.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     Enforced by <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> rather than by
    ///     <see cref="HttpClient.Timeout"/>, so it applies to the whole load — the response body
    ///     included — and not merely to the point where the headers arrive.
    ///     </para>
    ///     <para>
    ///     A <see langword="null"/> means no deadline, not "use the default". The shared
    ///     <see cref="HttpClient"/> is built with
    ///     <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>, so a null here leaves nothing at
    ///     all bounding the load but the caller's own <see cref="CancellationToken"/>. That is the
    ///     right setting for a large archive on a slow link and the wrong one for almost everything
    ///     else. <see cref="TimeSpan.Zero"/> is <i>not</i> a way to spell it — it cancels immediately.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is greater than a year.</exception>
    public TimeSpan? Timeout
    {
        get;
        set
        {
            if (value is { } period && (period.TotalMilliseconds < 0 || period > TimeSpan.FromDays(365)))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            field = value;
        }
    } = SyndicationEncodingUtility.DefaultRequestTimeout;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <returns>The settings, with the unset cases spelled out as <c>detect</c>, <c>none</c> and <c>default</c> rather than as an empty value.</returns>
    public override string ToString() => $"[SyndicationResourceLoadSettings(CharacterEncoding = \"{this.CharacterEncoding?.WebName ?? "detect"}\", RetrievalLimit = \"{this.RetrievalLimit}\", Timeout = \"{this.Timeout?.TotalMilliseconds.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? "none"}\", MaxResponseContentLength = \"{this.MaxResponseContentLength?.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? "default"}\", Autodetect = \"{this.AutoDetectExtensions}\", SupportedExtensions = \"{this.SupportedExtensions.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo)}\")]";

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

        // Both properties are now nullable, and null sorts first in each: "detect the encoding" and
        // "no deadline" are the absence of a value, not a value that happens to be small.
        int result = string.Compare(this.CharacterEncoding?.WebName, other.CharacterEncoding?.WebName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.RetrievalLimit.CompareTo(other.RetrievalLimit);
        if (result == 0) result = Nullable.Compare(this.Timeout, other.Timeout);
        if (result == 0) result = this.AutoDetectExtensions.CompareTo(other.AutoDetectExtensions);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.SupportedExtensions, other.SupportedExtensions);

        // Appended rather than inserted: any earlier position could flip the sign of a comparison
        // that two existing members already decided between them.
        if (result == 0) result = Nullable.Compare(this.MaxResponseContentLength, other.MaxResponseContentLength);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationResourceLoadSettings"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationResourceLoadSettings"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SyndicationResourceLoadSettings"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SyndicationResourceLoadSettings other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.CharacterEncoding?.WebName), HashCodeUtility.Component(this.RetrievalLimit), HashCodeUtility.Component(this.Timeout), HashCodeUtility.Component(this.AutoDetectExtensions), HashCodeUtility.Component(this.MaxResponseContentLength));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SyndicationResourceLoadSettings? first, SyndicationResourceLoadSettings? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(SyndicationResourceLoadSettings? first, SyndicationResourceLoadSettings? second) => !(first == second);

}