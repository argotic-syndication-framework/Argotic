/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created FeedSynchronizationItem Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the information required for synchronization of syndication feeds.
    /// </summary>
    /// <remarks>
    ///     <para>The <see cref="FeedSynchronizationItem"/> class represents a <b>sx:sync</b> element in the <i>FeedSync</i> specification.</para>
    ///     <para>
    ///         This is <b>required</b> of all items in all feeds wishing to participate in FeedSync-based synchronization. 
    ///         Since <see cref="FeedSynchronizationSharingInformation"/> is not required, feed consumers <b>must</b> consider the presence of <see cref="FeedSynchronizationItem"/> in items or entries 
    ///         as an indication that the feed contains sync data.
    ///     </para>
    ///     <para>
    ///         It acceptable for a feed to have some items or entries with <see cref="FeedSynchronizationItem"/> elements, and some without a <see cref="FeedSynchronizationItem"/>. 
    ///         Only the items and entries that include the <see cref="FeedSynchronizationItem"/> element participate in FeedSync synchronization.
    ///     </para>
    /// </remarks>
    /// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
    [Serializable()]
    public class FeedSynchronizationItem : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the globally unique identifier for the item.
        /// </summary>
        private string synchronizationId    = String.Empty;
        /// <summary>
        /// Private member to hold the number of updates applied to an item.
        /// </summary>
        private int synchronizationUpdates  = 1;
        /// <summary>
        /// Private member to hold a value indicating that the item has been deleted and is a tombstone.
        /// </summary>
        private FeedSynchronizationTombstoneStatus synchronizationTombstoneStatus;
        /// <summary>
        /// Private member to hold a value indicating how conflict preservation is processed.
        /// </summary>
        private FeedSynchronizationConflictPreservationDirective synchronizationConflictPreservation;
        /// <summary>
        /// Private member to hold information about updates to the item.
        /// </summary>
        private Collection<FeedSynchronizationHistory> synchronizationHistories;
        /// <summary>
        /// Private member to hold information about conflicting updates to the item.
        /// </summary>
        private Collection<XPathNavigator> synchronizationConflicts;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region FeedSynchronizationItem()
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class.
        /// </summary>
        public FeedSynchronizationItem()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region FeedSynchronizationItem(string id, int updates)
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class using the supplied indentifier and number of updates.
        /// </summary>
        /// <param name="id">The globally unique identifier for the item.</param>
        /// <param name="updates">The number of updates applied to this item.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="updates"/> is less than <b>1</b>.</exception>
        public FeedSynchronizationItem(string id, int updates)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Id         = id;
            this.Updates    = updates;
        }
        #endregion

        #region FeedSynchronizationItem(string id, int updates, FeedSynchronizationHistory history)
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationItem"/> class using the supplied indentifier, number of updates, and initial <see cref="FeedSynchronizationHistory"/>.
        /// </summary>
        /// <param name="id">The globally unique identifier for the item.</param>
        /// <param name="updates">The number of updates applied to this item.</param>
        /// <param name="history">A <see cref="FeedSynchronizationHistory"/> object that represents the initial information about updates to this item.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="updates"/> is less than <b>1</b>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="history"/> is a null reference (Nothing in Visual Basic).</exception>
        public FeedSynchronizationItem(string id, int updates, FeedSynchronizationHistory history) : this(id, updates)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            Guard.ArgumentNotNull(history, "history");
            this.Histories.Add(history);
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Conflicts
        /// <summary>
        /// Gets the conflicting updates for this item.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="XPathNavigator"/> objects that represent conflicting updates for this item. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<XPathNavigator> Conflicts
        {
            get
            {
                if (synchronizationConflicts == null)
                {
                    synchronizationConflicts = new Collection<XPathNavigator>();
                }
                return synchronizationConflicts;
            }
        }
        #endregion

        #region Histories
        /// <summary>
        /// Gets the information about updates to this item.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="FeedSynchronizationHistory"/> objects that represent information about updates to this item. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<FeedSynchronizationHistory> Histories
        {
            get
            {
                if (synchronizationHistories == null)
                {
                    synchronizationHistories = new Collection<FeedSynchronizationHistory>();
                }
                return synchronizationHistories;
            }
        }
        #endregion

        #region Id
        /// <summary>
        /// Gets or sets the globally unique identifier for this item.
        /// </summary>
        /// <value>The globally unique identifier for this item.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Id">identifier</see> <b>must</b> be globally unique within the feed and it <b>must</b> be identical across feeds if an item is being shared or synchronized as part of multiple distinct independent feeds.
        ///     </para>
        ///     <para>
        ///         Atom has a similar requirement for each entry to have a unique id. While the Atom entry <i>id</i> could be used for the sync id at the publisher’s discretion, 
        ///         implementers <b>must not</b> assume that the Atom <i>id</i> for the entry matches the sync id. Likewise, if the RSS item includes a <i>guid</i>, 
        ///         implementers <b>must not</b> assume that the <i>guid</i> is the same as the sync id.
        ///     </para>
        ///     <para>
        ///         In Atom feeds, it is acceptable to have multiple entries in the same feed with the same atom id element; in this case, the entries are considered different versions of the same entry. 
        ///         It is allowed to use FeedSync in such a feed, but the <b>sx:sync/@id</b> attributes are still required to be different in each entry. 
        ///         FeedSync considers those entries to be different sync items.
        ///     </para>
        ///     <para>
        ///         The <see cref="Id">identifier</see> is assigned by the creator of the item, and <b>must not</b> be changed by subsequent publishers. 
        ///         Applications will collate and compare these identifiers; therefore they <b>must</b> conform to the syntax for 
        ///         Namespace Specific Strings (the NSS portion of a URN) in <a href="http://www.ietf.org/rfc/rfc2141.txt">RFC 2141</a>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Id
        {
            get
            {
                return synchronizationId;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                synchronizationId = value.Trim();
            }
        }
        #endregion

        #region ConflictPreservation
        /// <summary>
        /// Gets or sets a value indicating whether conflict preservation is performed for this item.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that indicates whether conflict preservation is performed for this item. 
        ///     The default value is <see cref="FeedSynchronizationConflictPreservationDirective.None"/>, which indicates conflict preservation <b>must</b> be performed for the item.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         This value <b>must</b> only be set once, and <i>shall</i> only be set when the updates property value is <b>1</b>. 
        ///         All updates to the item after the first update must propagate whatever state was set on the first update.
        ///     </para>
        ///     <para>
        ///         Within this framework, <see cref="FeedSynchronizationConflictPreservationDirective.Ignore"/> is equivalent to <b>true</b> for the <i>noconflicts</i> attribute in the FeedSync specification, 
        ///         while <see cref="FeedSynchronizationConflictPreservationDirective.Perform"/> is equivalent to <b>false</b> for the <i>noconflicts</i> attribute in the FeedSync specification. 
        ///         Specifying a value of <see cref="FeedSynchronizationConflictPreservationDirective.None"/> is equivalent to the <i>noconflicts</i> attribute not being present.
        ///     </para>
        /// </remarks>
        public FeedSynchronizationConflictPreservationDirective ConflictPreservation
        {
            get
            {
                return synchronizationConflictPreservation;
            }

            set
            {
                synchronizationConflictPreservation = value;
            }
        }
        #endregion

        #region TombstoneStatus
        /// <summary>
        /// Gets or sets a value indicating if the item has been deleted.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that indicates whether this item has been deleted. 
        ///     The default value is <see cref="FeedSynchronizationTombstoneStatus.None"/>, which indicates the item has not been deleted.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Within this framework, <see cref="FeedSynchronizationTombstoneStatus.Deleted"/> is equivalent to <b>true</b> for the <i>deleted</i> attribute in the FeedSync specification, 
        ///         while <see cref="FeedSynchronizationTombstoneStatus.Present"/> is equivalent to <b>false</b> for the <i>deleted</i> attribute in the FeedSync specification. 
        ///         Specifying a value of <see cref="FeedSynchronizationTombstoneStatus.None"/> is equivalent to the <i>deleted</i> attribute not being present.
        ///     </para>
        /// </remarks>
        public FeedSynchronizationTombstoneStatus TombstoneStatus
        {
            get
            {
                return synchronizationTombstoneStatus;
            }

            set
            {
                synchronizationTombstoneStatus = value;
            }
        }
        #endregion

        #region Updates
        /// <summary>
        /// Gets or sets the number of updates applied to this item.
        /// </summary>
        /// <value>The number of updates applied to this item. The default value is <b>1</b>.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <b>1</b>.</exception>
        public int Updates
        {
            get
            {
                return synchronizationUpdates;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", 1);
                synchronizationUpdates = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<FeedSynchronizationHistory> source, Collection<FeedSynchronizationHistory> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{FeedSynchronizationHistory}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<FeedSynchronizationHistory> source, Collection<FeedSynchronizationHistory> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective directive)
        /// <summary>
        /// Returns the conflict preservation identifier for the supplied <see cref="FeedSynchronizationConflictPreservationDirective"/>.
        /// </summary>
        /// <param name="directive">The <see cref="FeedSynchronizationConflictPreservationDirective"/> to get the conflict preservation identifier for.</param>
        /// <returns>The conflict preservation identifier for the supplied <paramref name="vocabulary"/>, otherwise returns an empty string.</returns>
        public static string ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective directive)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationConflictPreservationDirective).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationConflictPreservationDirective))
                {
                    FeedSynchronizationConflictPreservationDirective preservationDirective  = (FeedSynchronizationConflictPreservationDirective)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (preservationDirective == directive)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region ConflictPreservationByName(string name)
        /// <summary>
        /// Returns the <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that corresponds to the specified conflict preservation name.
        /// </summary>
        /// <param name="name">The name of the conflict preservation.</param>
        /// <returns>A <see cref="FeedSynchronizationConflictPreservationDirective"/> enumeration value that corresponds to the specified string, otherwise returns <b>FeedSynchronizationConflictPreservationDirective.None</b>.</returns>
        /// <remarks>This method disregards case of specified conflict preservation name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static FeedSynchronizationConflictPreservationDirective ConflictPreservationByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            FeedSynchronizationConflictPreservationDirective preservationDirective  = FeedSynchronizationConflictPreservationDirective.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationConflictPreservationDirective).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationConflictPreservationDirective))
                {
                    FeedSynchronizationConflictPreservationDirective directive  = (FeedSynchronizationConflictPreservationDirective)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes                                   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            preservationDirective = directive;
                            break;
                        }
                    }
                }
            }

            return preservationDirective;
        }
        #endregion

        #region TombstoneStatusAsString(FeedSynchronizationTombstoneStatus status)
        /// <summary>
        /// Returns the tombstone status identifier for the supplied <see cref="FeedSynchronizationTombstoneStatus"/>.
        /// </summary>
        /// <param name="status">The <see cref="FeedSynchronizationTombstoneStatus"/> to get the tombstone status identifier for.</param>
        /// <returns>The tombstone status identifier for the supplied <paramref name="vocabulary"/>, otherwise returns an empty string.</returns>
        public static string TombstoneStatusAsString(FeedSynchronizationTombstoneStatus status)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationTombstoneStatus).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationTombstoneStatus))
                {
                    FeedSynchronizationTombstoneStatus tombstoneStatus  = (FeedSynchronizationTombstoneStatus)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (tombstoneStatus == status)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region TombstoneStatusByName(string name)
        /// <summary>
        /// Returns the <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that corresponds to the specified tombstone status name.
        /// </summary>
        /// <param name="name">The name of the tombstone status.</param>
        /// <returns>A <see cref="FeedSynchronizationTombstoneStatus"/> enumeration value that corresponds to the specified string, otherwise returns <b>FeedSynchronizationTombstoneStatus.None</b>.</returns>
        /// <remarks>This method disregards case of specified tombstone status name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static FeedSynchronizationTombstoneStatus TombstoneStatusByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            FeedSynchronizationTombstoneStatus tombstoneStatus  = FeedSynchronizationTombstoneStatus.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationTombstoneStatus).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationTombstoneStatus))
                {
                    FeedSynchronizationTombstoneStatus status   = (FeedSynchronizationTombstoneStatus)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes                   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            tombstoneStatus = status;
                            break;
                        }
                    }
                }
            }

            return tombstoneStatus;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="FeedSynchronizationItem"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="FeedSynchronizationItem"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationItem"/>.
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
            //	Create namespace manager to resolve prefixed elements
            //------------------------------------------------------------
            FeedSynchronizationSyndicationExtension extension   = new FeedSynchronizationSyndicationExtension();
            XmlNamespaceManager manager                         = extension.CreateNamespaceManager(source);

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if (source.HasAttributes)
            {
                string idAttribute          = source.GetAttribute("id", String.Empty);
                string updatesAttribute     = source.GetAttribute("updates", String.Empty);
                string deletedAttribute     = source.GetAttribute("deleted", String.Empty);
                string noConflictsAttribute = source.GetAttribute("noconflicts", String.Empty);

                if (!String.IsNullOrEmpty(idAttribute))
                {
                    this.Id     = idAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(updatesAttribute))
                {
                    int updates;
                    if (Int32.TryParse(updatesAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out updates))
                    {
                        this.Updates    = updates;
                        wasLoaded       = true;
                    }
                }

                if (!String.IsNullOrEmpty(deletedAttribute))
                {
                    FeedSynchronizationTombstoneStatus status   = FeedSynchronizationItem.TombstoneStatusByName(deletedAttribute);
                    if (status != FeedSynchronizationTombstoneStatus.None)
                    {
                        this.TombstoneStatus    = status;
                        wasLoaded               = true;
                    }
                }

                if (!String.IsNullOrEmpty(noConflictsAttribute))
                {
                    FeedSynchronizationConflictPreservationDirective directive  = FeedSynchronizationItem.ConflictPreservationByName(noConflictsAttribute);
                    if (directive != FeedSynchronizationConflictPreservationDirective.None)
                    {
                        this.ConflictPreservation   = directive;
                        wasLoaded                   = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator historyIterator   = source.Select("sx:history", manager);
                XPathNavigator conflictsNavigator   = source.SelectSingleNode("sx:conflicts", manager);

                if (historyIterator != null && historyIterator.Count > 0)
                {
                    while (historyIterator.MoveNext())
                    {
                        FeedSynchronizationHistory history  = new FeedSynchronizationHistory();
                        if (history.Load(historyIterator.Current))
                        {
                            this.Histories.Add(history);
                            wasLoaded   = true;
                        }
                    }
                }

                if (conflictsNavigator != null && conflictsNavigator.HasChildren)
                {
                    XPathNodeIterator childrenIterator  = conflictsNavigator.SelectChildren(XPathNodeType.Element);
                    if (childrenIterator != null && childrenIterator.Count > 0)
                    {
                        this.Conflicts.Add(childrenIterator.Current);
                        wasLoaded   = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="FeedSynchronizationItem"/> to the specified <see cref="XmlWriter"/>.
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
            //	Create extension instance to retrieve XML namespace
            //------------------------------------------------------------
            FeedSynchronizationSyndicationExtension extension   = new FeedSynchronizationSyndicationExtension();

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
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

            foreach(FeedSynchronizationHistory history in this.Histories)
            {
                history.WriteTo(writer);
            }

            if(this.Conflicts.Count > 0)
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
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="FeedSynchronizationItem"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="FeedSynchronizationItem"/>.</returns>
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
            FeedSynchronizationItem value  = obj as FeedSynchronizationItem;

            if (value != null)
            {
                int result  = String.Compare(this.Id, value.Id, StringComparison.OrdinalIgnoreCase);
                result      = result | this.ConflictPreservation.CompareTo(value.ConflictPreservation);
                result      = result | this.TombstoneStatus.CompareTo(value.TombstoneStatus);
                result      = result | this.Updates.CompareTo(value.Updates);
                result      = result | FeedSynchronizationItem.CompareSequence(this.Histories, value.Histories);
                result      = result | ComparisonUtility.CompareSequence(this.Conflicts, value.Conflicts);

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
            if (!(obj is FeedSynchronizationItem))
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
        public static bool operator ==(FeedSynchronizationItem first, FeedSynchronizationItem second)
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
        public static bool operator !=(FeedSynchronizationItem first, FeedSynchronizationItem second)
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
        public static bool operator <(FeedSynchronizationItem first, FeedSynchronizationItem second)
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
        public static bool operator >(FeedSynchronizationItem first, FeedSynchronizationItem second)
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
