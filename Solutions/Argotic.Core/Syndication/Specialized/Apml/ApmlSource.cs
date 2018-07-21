/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/08/2008	brian.kuhn	Created ApmlSource Class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Represents a specific source of information that an entity is interested in. 
    /// </summary>
    /// <seealso cref="ApmlProfile.ExplicitSources"/>
    /// <seealso cref="ApmlProfile.ImplicitSources"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlSource class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Apml\ApmlSourceExample.cs" 
    ///             region="ApmlSource" 
    ///         />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
    [Serializable()]
    public class ApmlSource : IComparable, IExtensibleSyndicationObject
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
        /// <summary>
        /// Private member to hold the unique key for the source.
        /// </summary>
        private string sourceKey            = String.Empty;
        /// <summary>
        /// Private member to hold the friendly name of the source.
        /// </summary>
        private string sourceName           = String.Empty;
        /// <summary>
        /// Private member to hold the decimal score of the source.
        /// </summary>
        private decimal sourceValue         = Decimal.MinValue;
        /// <summary>
        /// Private member to hold the MIME type of the source.
        /// </summary>
        private string sourceType           = String.Empty;
        /// <summary>
        /// Private member to hold the name of the entity that contributed the source.
        /// </summary>
        private string sourceFrom           = String.Empty;
        /// <summary>
        /// Private member to hold a date indicating the last time the source was updated.
        /// </summary>
        private DateTime sourceUpdatedOn    = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the collection of authors of the source.
        /// </summary>
        private Collection<ApmlAuthor> sourceAuthors;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region ApmlSource()
        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlSource"/> class.
        /// </summary>
        public ApmlSource()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region ApmlSource(string key, string name, string type, decimal value)
        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlSource"/> class using the supplied parameters.
        /// </summary>
        /// <param name="key">The unique key for this source.</param>
        /// <param name="name">The friendly name of this source.</param>
        /// <param name="type">The  MIME content type for this source.</param>
        /// <param name="value">The decimal score of this source.</param>
        /// <remarks>
        ///     This constructor is meant to be used when creating an <b>explicit</b> source. Explicit data is for items that are explicitly added by a user to represent something. 
        ///     For example, a user could edit their own APML file and add items they know they're interested in. 
        ///     For this reason the <see cref="From"/> and <see cref="UpdatedOn"/> properties are not necessary for explicit data items, because it's a manual process.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        public ApmlSource(string key, string name, string type, decimal value)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Key        = key;
            this.Name       = name;
            this.MimeType   = type;
            this.Value      = value;
        }
        #endregion

        #region ApmlSource(string key, string name, string type, decimal value, string from, DateTime utcUpdatedOn)
        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlSource"/> class using the supplied parameters.
        /// </summary>
        /// <param name="key">The unique key for this source.</param>
        /// <param name="name">The friendly name of this source.</param>
        /// <param name="type">The  MIME content type for this source.</param>
        /// <param name="value">The decimal score of this source.</param>
        /// <param name="from">The name of the entity that contributed this concept.</param>
        /// <remarks>
        ///     This constructor is meant to be used when creating an <b>implicit</b> source. Implicit data is added by machines/computers that try to make 
        ///     some informed guesses about the things that you are interested in. This stuff will change over time and are added with a certain degree of confidence 
        ///     that may have a decay in certain applications. For this reason it is important to keep a track of when things were added/modified.
        /// </remarks>
        /// <param name="utcUpdatedOn">A <see cref="DateTime"/> object that indicates the last time this concept was updated.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> is an empty string.</exception>
        public ApmlSource(string key, string name, string type, decimal value, string from, DateTime utcUpdatedOn) : this(key, name, type, value)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(from, "from");

            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.From       = from;
            this.UpdatedOn  = utcUpdatedOn;
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY PROPERTIES
        //============================================================
        #region Extensions
        /// <summary>
        /// Gets or sets the syndication extensions applied to this syndication entity.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (objectSyndicationExtensions == null)
                {
                    objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }
                return objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                objectSyndicationExtensions = value;
            }
        }
        #endregion

        #region HasExtensions
        /// <summary>
        /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Authors
        /// <summary>
        /// Gets or sets the authors of this source.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="ApmlAuthor"/> objects that represent the authors of this source.</value>
        public Collection<ApmlAuthor> Authors
        {
            get
            {
                if (sourceAuthors == null)
                {
                    sourceAuthors = new Collection<ApmlAuthor>();
                }
                return sourceAuthors;
            }
        }
        #endregion

        #region From
        /// <summary>
        /// Gets or sets the name of the entity that contributed this source.
        /// </summary>
        /// <value>The name of the entity that contributed this source. The default value is an empty string, which indicates no contributor was specified.</value>
        public string From
        {
            get
            {
                return sourceFrom;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    sourceFrom = String.Empty;
                }
                else
                {
                    sourceFrom = value.Trim();
                }
            }
        }
        #endregion

        #region Key
        /// <summary>
        /// Gets or sets the unique key for this source.
        /// </summary>
        /// <value>The unique key for this source.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Key
        {
            get
            {
                return sourceKey;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                sourceKey = value.Trim();
            }
        }
        #endregion

        #region MimeType
        /// <summary>
        /// Gets or sets the MIME content type for this source.
        /// </summary>
        /// <value>The  MIME content type for this source.</value>
        /// <remarks>
        ///     See <a href="http://www.iana.org/assignments/media-types/">http://www.iana.org/assignments/media-types/</a> for a listing of registered MIME content types.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string MimeType
        {
            get
            {
                return sourceType;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                sourceType = value.Trim();
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Gets or sets the friendly name of this source.
        /// </summary>
        /// <value>The friendly name of this source.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Name
        {
            get
            {
                return sourceName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                sourceName = value.Trim();
            }
        }
        #endregion

        #region UpdatedOn
        /// <summary>
        /// Gets or sets a date-time indicating the last time this source was updated.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that indicates the last time this source was updated. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update date was specified.</value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime UpdatedOn
        {
            get
            {
                return sourceUpdatedOn;
            }

            set
            {
                sourceUpdatedOn = value;
            }
        }
        #endregion

        #region Value
        /// <summary>
        /// Gets or sets the decimal score of this source.
        /// </summary>
        /// <value>The decimal score of this source.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        public decimal Value
        {
            get
            {
                return sourceValue;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", Decimal.MinusOne);
                Guard.ArgumentNotGreaterThan(value, "value", Decimal.One);
                sourceValue = value;
            }
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY METHODS
        //============================================================
        #region AddExtension(ISyndicationExtension extension)
        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded   = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Add syndication extension to collection
            //------------------------------------------------------------
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded    = true;

            return wasAdded;
        }
        #endregion

        #region FindExtension(Predicate<ISyndicationExtension> match)
        /// <summary>
        /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
        /// <returns>
        ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
        /// </returns>
        /// <remarks>
        ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate. 
        ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in 
        ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(match, "match");

            //------------------------------------------------------------
            //	Perform predicate based search
            //------------------------------------------------------------
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }
        #endregion

        #region RemoveExtension(ISyndicationExtension extension)
        /// <summary>
        /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Remove syndication extension from collection
            //------------------------------------------------------------
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved  = true;
            }

            return wasRemoved;
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<ApmlAuthor> source, Collection<ApmlAuthor> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{ApmlAuthor}"/> collections.
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
        public static int CompareSequence(Collection<ApmlAuthor> source, Collection<ApmlAuthor> target)
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

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="ApmlSource"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ApmlSource"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlSource"/>.
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
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if (source.HasAttributes)
            {
                string keyAttribute     = source.GetAttribute("key", String.Empty);
                string nameAttribute    = source.GetAttribute("name", String.Empty);
                string valueAttribute   = source.GetAttribute("value", String.Empty);
                string typeAttribute    = source.GetAttribute("type", String.Empty);
                string fromAttribute    = source.GetAttribute("from", String.Empty);
                string updatedAttribute = source.GetAttribute("updated", String.Empty);

                if (!String.IsNullOrEmpty(keyAttribute))
                {
                    this.Key    = keyAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(nameAttribute))
                {
                    this.Name   = nameAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(valueAttribute))
                {
                    decimal value;
                    if (Decimal.TryParse(valueAttribute, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out value))
                    {
                        if (value >= Decimal.MinusOne && value <= Decimal.One)
                        {
                            this.Value  = value;
                            wasLoaded   = true;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    this.MimeType   = typeAttribute;
                    wasLoaded       = true;
                }

                if (!String.IsNullOrEmpty(fromAttribute))
                {
                    this.From   = fromAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(updatedAttribute))
                {
                    DateTime updatedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedAttribute, out updatedOn))
                    {
                        this.UpdatedOn  = updatedOn;
                        wasLoaded       = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator authorIterator    = source.Select("apml:Author", manager);

                if (authorIterator != null && authorIterator.Count > 0)
                {
                    while (authorIterator.MoveNext())
                    {
                        ApmlAuthor author = new ApmlAuthor();
                        if (author.Load(authorIterator.Current))
                        {
                            this.Authors.Add(author);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads this <see cref="ApmlSource"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlSource"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlSource"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if (source.HasAttributes)
            {
                string keyAttribute     = source.GetAttribute("key", String.Empty);
                string nameAttribute    = source.GetAttribute("name", String.Empty);
                string valueAttribute   = source.GetAttribute("value", String.Empty);
                string typeAttribute    = source.GetAttribute("type", String.Empty);
                string fromAttribute    = source.GetAttribute("from", String.Empty);
                string updatedAttribute = source.GetAttribute("updated", String.Empty);

                if (!String.IsNullOrEmpty(keyAttribute))
                {
                    this.Key    = keyAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(nameAttribute))
                {
                    this.Name   = nameAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(valueAttribute))
                {
                    decimal value;
                    if (Decimal.TryParse(valueAttribute, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out value))
                    {
                        if (value >= Decimal.MinusOne && value <= Decimal.One)
                        {
                            this.Value  = value;
                            wasLoaded   = true;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    this.MimeType   = typeAttribute;
                    wasLoaded       = true;
                }

                if (!String.IsNullOrEmpty(fromAttribute))
                {
                    this.From   = fromAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(updatedAttribute))
                {
                    DateTime updatedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedAttribute, out updatedOn))
                    {
                        this.UpdatedOn  = updatedOn;
                        wasLoaded       = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator authorIterator    = source.Select("apml:Author", manager);

                if (authorIterator != null && authorIterator.Count > 0)
                {
                    while (authorIterator.MoveNext())
                    {
                        ApmlAuthor author = new ApmlAuthor();
                        if (author.Load(authorIterator.Current, settings))
                        {
                            this.Authors.Add(author);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="ApmlSource"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("Source", ApmlUtility.ApmlNamespace);

            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("value", this.Value.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo));
            writer.WriteAttributeString("type", this.MimeType);

            if (!String.IsNullOrEmpty(this.From))
            {
                writer.WriteAttributeString("from", this.From);
            }

            if (this.UpdatedOn != DateTime.MinValue)
            {
                writer.WriteAttributeString("updated", SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
            }

            foreach(ApmlAuthor author in this.Authors)
            {
                author.WriteTo(writer);
            }

            //------------------------------------------------------------
            //	Write the syndication extensions of the current instance
            //------------------------------------------------------------
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="ApmlSource"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="ApmlSource"/>.</returns>
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
            ApmlSource value  = obj as ApmlSource;

            if (value != null)
            {
                int result  = ApmlSource.CompareSequence(this.Authors, value.Authors);
                result      = result | String.Compare(this.From, value.From, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Key, value.Key, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.MimeType, value.MimeType, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result      = result | this.UpdatedOn.CompareTo(value.UpdatedOn);
                result      = result | this.Value.CompareTo(value.Value);

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
            if (!(obj is ApmlSource))
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
        public static bool operator ==(ApmlSource first, ApmlSource second)
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
        public static bool operator !=(ApmlSource first, ApmlSource second)
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
        public static bool operator <(ApmlSource first, ApmlSource second)
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
        public static bool operator >(ApmlSource first, ApmlSource second)
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
