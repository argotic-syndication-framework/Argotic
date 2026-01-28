using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents information from a specific feed publisher to the specific feed consumer that requested the feed.
/// </summary>
/// <remarks>
///     Endpoints that consume the feed <b>must not</b> republish the <see cref="FeedSynchronizationSharingInformation"/> or any of its sub-elements to other feed consumers.
/// </remarks>
/// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
[Serializable]
public class FeedSynchronizationSharingInformation : IComparable<FeedSynchronizationSharingInformation>, IEquatable<FeedSynchronizationSharingInformation>
{

    /// <summary>
    /// Private member to hold a lower bound of items contained within the feed.
    /// </summary>
    private string sharingInformationSince = string.Empty;
    /// <summary>
    /// Private member to hold an upper bound of items contained within the feed.
    /// </summary>
    private string sharingInformationUntil = string.Empty;
    /// <summary>
    /// Private member to hold the publisher suggested date-time before which subscribers should read the feed in order to avoid missing item updates.
    /// </summary>
    private DateTime sharingInformationExpires = DateTime.MinValue;
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationSharingInformation"/> class.
    /// </summary>
    public FeedSynchronizationSharingInformation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationSharingInformation"/> class using the supplied parameters.
    /// </summary>
    /// <param name="since">A lower bound of items contained within the feed.</param>
    /// <param name="until">An upper bound of items contained within the feed.</param>
    public FeedSynchronizationSharingInformation(string since, string until)
    {
        this.Since = since;
        this.Until = until;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationSharingInformation"/> class using the supplied parameters.
    /// </summary>
    /// <param name="since">A lower bound of items contained within the feed.</param>
    /// <param name="until">An upper bound of items contained within the feed.</param>
    /// <param name="utcExpires">A <see cref="DateTime"/> that represents the publisher suggested date-time before which subscribers <i>should</i> read the feed in order to avoid missing item updates.</param>
    public FeedSynchronizationSharingInformation(string since, string until, DateTime utcExpires) : this(since, until)
    {
        this.ExpiresOn = utcExpires;
    }

    /// <summary>
    /// Gets or sets the publisher suggested date-time subscribers should read the feed in order to avoid missing item updates.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that represents the publisher suggested date-time before which subscribers <i>should</i> read the feed in order to avoid missing item updates. 
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no expiration date was specified.
    /// </value>
    /// <remarks>
    ///     <para>The value for this attribute <i>should</i> be interpreted as a best effort, uncalibrated value.</para>
    ///     <para>The <see cref="DateTime"/> value should be provided in Coordinated Universal Time (UTC).</para>
    /// </remarks>
    public DateTime ExpiresOn
    {
        get
        {
            return sharingInformationExpires;
        }

        set
        {
            sharingInformationExpires = value;
        }
    }

    /// <summary>
    /// Gets the related feeds or locations.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="FeedSynchronizationRelatedInformation"/> objects that represent the related feeds or locations.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<FeedSynchronizationRelatedInformation> Relations { get; } = [];

    /// <summary>
    /// Gets or sets a lower bound of items contained within the feed.
    /// </summary>
    /// <value>A lower bound of items contained within the feed.</value>
    /// <remarks>
    ///     <para>If this property is defined, the <see cref="Until"/> property <b>must</b> also be specified.</para>
    ///     <para>
    ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window. 
    ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
    ///     </para>
    ///     <para>
    ///         In the feed sharing context new subscribers, or existing subscribers failing to subscribe within the published feed window, will need to initially 
    ///         copy a complete set of items from a publisher before being in a position to process incremental updates. As such, the specification provides for the 
    ///         ability for the latter feed to reference the complete feed.
    ///     </para>
    ///     <para>
    ///         Subscribers <i>may</i> optionally use the <b>since</b> and <b>until</b> properties of <see cref="FeedSynchronizationSharingInformation"/> to ensure 
    ///         that all item updates are synchronized, even if the publisher periodically purges items from its feed.
    ///     </para>
    /// </remarks>
    public string Since
    {
        get
        {
            return sharingInformationSince;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                sharingInformationSince = string.Empty;
            }
            else
            {
                sharingInformationSince = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets an upper bound of items contained within the feed.
    /// </summary>
    /// <value>An upper bound of items contained within the feed.</value>
    /// <remarks>
    ///     <para>If this property is defined, the <see cref="Since"/> property <b>must</b> also be specified.</para>
    ///     <para>
    ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window. 
    ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
    ///     </para>
    ///     <para>
    ///         In the feed sharing context new subscribers, or existing subscribers failing to subscribe within the published feed window, will need to initially 
    ///         copy a complete set of items from a publisher before being in a position to process incremental updates. As such, the specification provides for the 
    ///         ability for the latter feed to reference the complete feed.
    ///     </para>
    ///     <para>
    ///         Subscribers <i>may</i> optionally use the <b>since</b> and <b>until</b> properties of <see cref="FeedSynchronizationSharingInformation"/> to ensure 
    ///         that all item updates are synchronized, even if the publisher periodically purges items from its feed.
    ///     </para>
    /// </remarks>
    public string Until
    {
        get
        {
            return sharingInformationUntil;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                sharingInformationUntil = string.Empty;
            }
            else
            {
                sharingInformationUntil = value.Trim();
            }
        }
    }

    /// <summary>
    /// Compares two specified <see cref="Collection{FeedSynchronizationRelatedInformation}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<FeedSynchronizationRelatedInformation> source, IList<FeedSynchronizationRelatedInformation> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }
    /// <summary>
    /// Loads this <see cref="FeedSynchronizationSharingInformation"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="FeedSynchronizationSharingInformation"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationSharingInformation"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        FeedSynchronizationSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasAttributes)
        {
            string sinceAttribute = source.GetAttribute("since", string.Empty);
            string untilAttribute = source.GetAttribute("until", string.Empty);
            string expiresAttribute = source.GetAttribute("expires", string.Empty);

            if (!string.IsNullOrEmpty(sinceAttribute))
            {
                this.Since = sinceAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(untilAttribute))
            {
                this.Until = untilAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(expiresAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(expiresAttribute, out DateTime expiresOn))
                {
                    this.ExpiresOn = expiresOn;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNodeIterator relatedIterator = source.Select("sx:related", manager);

            if (relatedIterator is { Count: > 0 })
            {
                while (relatedIterator.MoveNext())
                {
                    FeedSynchronizationRelatedInformation relation = new();
                    if (relation.Load(relatedIterator.Current))
                    {
                        this.Relations.Add(relation);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="FeedSynchronizationSharingInformation"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        FeedSynchronizationSyndicationExtension extension = new();
        writer.WriteStartElement("sharing", extension.XmlNamespace);

        if (!string.IsNullOrEmpty(this.Since))
        {
            writer.WriteAttributeString("since", this.Since);
        }

        if (!string.IsNullOrEmpty(this.Until))
        {
            writer.WriteAttributeString("until", this.Until);
        }

        if (this.ExpiresOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("expires", SyndicationDateTimeUtility.ToRfc3339DateTime(this.ExpiresOn));
        }

        foreach (FeedSynchronizationRelatedInformation relation in this.Relations)
        {
            relation.WriteTo(writer);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="FeedSynchronizationSharingInformation"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="FeedSynchronizationSharingInformation"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(FeedSynchronizationSharingInformation? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.ExpiresOn.CompareTo(other.ExpiresOn);
        result |= FeedSynchronizationSharingInformation.CompareSequence(this.Relations, other.Relations);
        result |= string.Compare(this.Since, other.Since, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Until, other.Until, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedSynchronizationSharingInformation"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedSynchronizationSharingInformation"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="FeedSynchronizationSharingInformation"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(FeedSynchronizationSharingInformation? other)
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
        return obj is FeedSynchronizationSharingInformation other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.ExpiresOn, this.Since, this.Until);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
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
    public static bool operator !=(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
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
    public static bool operator >(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
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
    public static bool operator <=(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
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
    public static bool operator >=(FeedSynchronizationSharingInformation first, FeedSynchronizationSharingInformation second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}