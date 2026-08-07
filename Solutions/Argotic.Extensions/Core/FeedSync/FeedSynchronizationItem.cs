using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the information required for synchronization of syndication feeds.
/// </summary>
/// <remarks>
///     <para>The <see cref="FeedSynchronizationItem"/> class represents a <c>sx:sync</c> element in the <i>FeedSync</i> specification.</para>
///     <para>
///         This is <b>required</b> of all items in all feeds wishing to participate in FeedSync-based synchronization.
///         Since <see cref="FeedSynchronizationSharingInformation"/> is not required, feed consumers <b>must</b> consider the presence of <see cref="FeedSynchronizationItem"/> in items or entries
///         as an indication that the feed contains sync data.
///     </para>
///     <para>
///         It is acceptable for a feed to have some items or entries with <see cref="FeedSynchronizationItem"/> elements, and some without a <see cref="FeedSynchronizationItem"/>.
///         Only the items and entries that include the <see cref="FeedSynchronizationItem"/> element participate in FeedSync synchronization.
///     </para>
/// </remarks>
/// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
public class FeedSynchronizationItem : IComparable<FeedSynchronizationItem>, IEquatable<FeedSynchronizationItem>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class.
    /// </summary>
    public FeedSynchronizationItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class using the supplied identifier and number of updates.
    /// </summary>
    /// <param name="id">The globally unique identifier for the item.</param>
    /// <param name="updates">The number of updates applied to this item.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="id"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="updates"/> is less than <c>1</c>.</exception>
    public FeedSynchronizationItem(string id, int updates)
    {
        this.Id = id;
        this.Updates = updates;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class using the supplied identifier, number of updates, and initial <see cref="FeedSynchronizationHistory"/>.
    /// </summary>
    /// <param name="id">The globally unique identifier for the item.</param>
    /// <param name="updates">The number of updates applied to this item.</param>
    /// <param name="history">A <see cref="FeedSynchronizationHistory"/> object that represents the initial information about updates to this item.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="id"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="updates"/> is less than <c>1</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="history"/> is <see langword="null"/>.</exception>
    public FeedSynchronizationItem(string id, int updates, FeedSynchronizationHistory history) : this(id, updates)
    {
        ArgumentNullException.ThrowIfNull(history);
        this.Histories.Add(history);
    }

    /// <summary>
    /// Gets the conflicting updates for this item.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="XPathNavigator"/> objects that represent conflicting updates for this item.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<XPathNavigator> Conflicts { get; } = [];

    /// <summary>
    /// Gets the information about updates to this item.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="FeedSynchronizationHistory"/> objects that represent information about updates to this item.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<FeedSynchronizationHistory> Histories { get; } = [];

    /// <summary>
    /// Gets or sets the globally unique identifier for this item.
    /// </summary>
    /// <value>The identifier. The default value is an <i>empty</i> string; the setter rejects <see langword="null"/> and empty.</value>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Id">identifier</see> <b>must</b> be globally unique within the feed and it <b>must</b> be identical across feeds if an item is being shared or synchronized as part of multiple distinct independent feeds.
    ///     </para>
    ///     <para>
    ///         Atom has a similar requirement for each entry to have a unique id. While the Atom entry <i>id</i> could be used for the sync id at the publisher's discretion,
    ///         implementers <b>must not</b> assume that the Atom <i>id</i> for the entry matches the sync id. Likewise, if the RSS item includes a <i>guid</i>,
    ///         implementers <b>must not</b> assume that the <i>guid</i> is the same as the sync id.
    ///     </para>
    ///     <para>
    ///         In Atom feeds, it is acceptable to have multiple entries in the same feed with the same atom id element; in this case, the entries are considered different versions of the same entry.
    ///         It is allowed to use FeedSync in such a feed, but the <c>sx:sync/@id</c> attributes are still required to be different in each entry.
    ///         FeedSync considers those entries to be different sync items.
    ///     </para>
    ///     <para>
    ///         The <see cref="Id">identifier</see> is assigned by the creator of the item, and <b>must not</b> be changed by subsequent publishers.
    ///         Applications will collate and compare these identifiers; therefore they <b>must</b> conform to the syntax for
    ///         Namespace Specific Strings (the NSS portion of a URN) in <a href="https://www.rfc-editor.org/rfc/rfc2141.html">RFC 2141</a>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Id
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether conflict preservation is performed for this item.
    /// </summary>
    /// <value>
    ///     A <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that indicates whether conflict preservation is performed for this item.
    ///     The default value is <see cref="FeedSynchronizationConflictPreservationDirective.None"/>, which indicates conflict preservation <b>must</b> be performed for the item.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This value <b>must</b> only be set once, and <i>shall</i> only be set when the updates property value is <c>1</c>.
    ///         All updates to the item after the first update must propagate whatever state was set on the first update.
    ///     </para>
    ///     <para>
    ///         Within this framework, <see cref="FeedSynchronizationConflictPreservationDirective.Ignore"/> is equivalent to <see langword="true"/> for the <i>noconflicts</i> attribute in the FeedSync specification,
    ///         while <see cref="FeedSynchronizationConflictPreservationDirective.Perform"/> is equivalent to <see langword="false"/> for the <i>noconflicts</i> attribute in the FeedSync specification.
    ///         Specifying a value of <see cref="FeedSynchronizationConflictPreservationDirective.None"/> is equivalent to the <i>noconflicts</i> attribute not being present.
    ///     </para>
    /// </remarks>
    public FeedSynchronizationConflictPreservationDirective ConflictPreservation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the item has been deleted.
    /// </summary>
    /// <value>
    ///     A <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that indicates whether this item has been deleted.
    ///     The default value is <see cref="FeedSynchronizationTombstoneStatus.None"/>, which indicates the item has not been deleted.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         Within this framework, <see cref="FeedSynchronizationTombstoneStatus.Deleted"/> is equivalent to <see langword="true"/> for the <i>deleted</i> attribute in the FeedSync specification,
    ///         while <see cref="FeedSynchronizationTombstoneStatus.Present"/> is equivalent to <see langword="false"/> for the <i>deleted</i> attribute in the FeedSync specification.
    ///         Specifying a value of <see cref="FeedSynchronizationTombstoneStatus.None"/> is equivalent to the <i>deleted</i> attribute not being present.
    ///     </para>
    /// </remarks>
    public FeedSynchronizationTombstoneStatus TombstoneStatus { get; set; }

    /// <summary>
    /// Gets or sets the number of updates applied to this item.
    /// </summary>
    /// <value>The number of updates applied to this item. The default value is <c>1</c>.</value>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>1</c>.</exception>
    public int Updates
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = 1;

    /// <summary>
    /// Compares two specified <see cref="IList{FeedSynchronizationHistory}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<FeedSynchronizationHistory> source, IList<FeedSynchronizationHistory> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Returns the conflict preservation identifier for the supplied <see cref="FeedSynchronizationConflictPreservationDirective"/>.
    /// </summary>
    /// <param name="directive">The <see cref="FeedSynchronizationConflictPreservationDirective"/> to get the conflict preservation identifier for.</param>
    /// <returns>The conflict preservation identifier for the supplied <paramref name="directive"/>; otherwise, an empty string.</returns>
    public static string ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective directive) =>
        EnumerationMetadataAttribute.GetAlternateValue(directive);

    /// <summary>
    /// Returns the <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that corresponds to the specified conflict preservation name.
    /// </summary>
    /// <param name="name">The name of the conflict preservation.</param>
    /// <returns>A <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that corresponds to the specified string; otherwise, <see cref="FeedSynchronizationConflictPreservationDirective.None"/>.</returns>
    /// <remarks>This method disregards case of specified conflict preservation name.</remarks>
    public static FeedSynchronizationConflictPreservationDirective ConflictPreservationByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, FeedSynchronizationConflictPreservationDirective.None);

    /// <summary>
    /// Returns the tombstone status identifier for the supplied <see cref="FeedSynchronizationTombstoneStatus"/>.
    /// </summary>
    /// <param name="status">The <see cref="FeedSynchronizationTombstoneStatus"/> to get the tombstone status identifier for.</param>
    /// <returns>The tombstone status identifier for the supplied <paramref name="status"/>; otherwise, an empty string.</returns>
    public static string TombstoneStatusAsString(FeedSynchronizationTombstoneStatus status) =>
        EnumerationMetadataAttribute.GetAlternateValue(status);

    /// <summary>
    /// Returns the <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that corresponds to the specified tombstone status name.
    /// </summary>
    /// <param name="name">The name of the tombstone status.</param>
    /// <returns>A <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that corresponds to the specified string; otherwise, <see cref="FeedSynchronizationTombstoneStatus.None"/>.</returns>
    /// <remarks>This method disregards case of specified tombstone status name.</remarks>
    public static FeedSynchronizationTombstoneStatus TombstoneStatusByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, FeedSynchronizationTombstoneStatus.None);

    /// <summary>
    /// Loads this <see cref="FeedSynchronizationItem"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedSynchronizationItem"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        FeedSynchronizationSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasAttributes)
        {
            string idAttribute = source.GetAttribute("id", string.Empty);
            string updatesAttribute = source.GetAttribute("updates", string.Empty);
            string deletedAttribute = source.GetAttribute("deleted", string.Empty);
            string noConflictsAttribute = source.GetAttribute("noconflicts", string.Empty);

            if (!string.IsNullOrEmpty(idAttribute))
            {
                this.Id = idAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(updatesAttribute))
            {
                if (int.TryParse(updatesAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int updates))
                {
                    this.Updates = updates;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(deletedAttribute))
            {
                FeedSynchronizationTombstoneStatus status = FeedSynchronizationItem.TombstoneStatusByName(deletedAttribute);
                if (status != FeedSynchronizationTombstoneStatus.None)
                {
                    this.TombstoneStatus = status;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(noConflictsAttribute))
            {
                FeedSynchronizationConflictPreservationDirective directive = FeedSynchronizationItem.ConflictPreservationByName(noConflictsAttribute);
                if (directive != FeedSynchronizationConflictPreservationDirective.None)
                {
                    this.ConflictPreservation = directive;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNodeIterator historyIterator = source.SelectChildElements("sx", "history", manager);
            XPathNavigator? conflictsNavigator = source.SelectChildElement("sx", "conflicts", manager);

            if (historyIterator is { Count: > 0 })
            {
                while (historyIterator.MoveNext())
                {
                    XPathNavigator? historyNode = historyIterator.Current;
                    if (historyNode is null)
                    {
                        continue;
                    }

                    FeedSynchronizationHistory history = new();
                    if (history.Load(historyNode))
                    {
                        this.Histories.Add(history);
                        wasLoaded = true;
                    }
                }
            }

            if (conflictsNavigator is { HasChildren: true })
            {
                XPathNodeIterator childrenIterator = conflictsNavigator.SelectChildren(XPathNodeType.Element);
                while (childrenIterator.MoveNext())
                {
                    XPathNavigator? conflictNode = childrenIterator.Current;
                    if (conflictNode is not null)
                    {
                        this.Conflicts.Add(conflictNode);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="FeedSynchronizationItem"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        FeedSynchronizationSyndicationExtension extension = new();
        writer.WriteStartElement("sync", extension.XmlNamespace);

        writer.WriteAttributeString("id", this.Id);
        writer.WriteAttributeString("updates", this.Updates.ToString(NumberFormatInfo.InvariantInfo));

        if (this.TombstoneStatus != FeedSynchronizationTombstoneStatus.None)
        {
            writer.WriteAttributeString("deleted", FeedSynchronizationItem.TombstoneStatusAsString(this.TombstoneStatus));
        }

        if (this.ConflictPreservation != FeedSynchronizationConflictPreservationDirective.None)
        {
            writer.WriteAttributeString("noconflicts", FeedSynchronizationItem.ConflictPreservationAsString(this.ConflictPreservation));
        }

        foreach (FeedSynchronizationHistory history in this.Histories)
        {
            history.WriteTo(writer);
        }

        if (this.Conflicts.Count > 0)
        {
            writer.WriteStartElement("conflicts", extension.XmlNamespace);
            foreach (XPathNavigator conflict in this.Conflicts)
            {
                conflict.WriteSubtree(writer);
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedSynchronizationItem"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedSynchronizationItem"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

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
    public int CompareTo(FeedSynchronizationItem? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Id, other.Id, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.ConflictPreservation.CompareTo(other.ConflictPreservation);
        if (result == 0) result = this.TombstoneStatus.CompareTo(other.TombstoneStatus);
        if (result == 0) result = this.Updates.CompareTo(other.Updates);
        if (result == 0) result = FeedSynchronizationItem.CompareSequence(this.Histories, other.Histories);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Conflicts, other.Conflicts);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedSynchronizationItem"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedSynchronizationItem"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="FeedSynchronizationItem"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(FeedSynchronizationItem? other)
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
    public override bool Equals(object? obj) => obj is FeedSynchronizationItem other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.ConflictPreservation), HashCodeUtility.Component(this.TombstoneStatus), HashCodeUtility.Component(this.Updates));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(FeedSynchronizationItem? first, FeedSynchronizationItem? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(FeedSynchronizationItem? first, FeedSynchronizationItem? second) => !(first == second);

}