/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
07/09/2008	brian.kuhn	Created AtomWorkspace class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;
using Argotic.Extensions;

namespace Argotic.Publishing
{
    /// <summary>
    /// Represents a server-defined group of <see cref="AtomMemberResources"/> objects that describe member resources.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomWorkspace"/> class implements the <i>app:workspace</i> element of the <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a>.
    ///     </para>
    ///     <para>
    ///         A <see cref="AtomServiceDocument">service document</see> groups <see cref="AtomMemberResources">collections</see> into <see cref="AtomWorkspace">workspaces</see>. 
    ///         Operations on <see cref="AtomWorkspace">workspaces</see>, such as creation or deletion, are not defined by the <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a> 
    ///         specification. The <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a> specification assigns no meaning to <see cref="AtomWorkspace">workspaces</see>; 
    ///         that is, a <see cref="AtomWorkspace">workspace</see> does not imply any specific processing assumptions.
    ///     </para>
    ///     <para>
    ///         There is no requirement that a server support multiple <see cref="AtomWorkspace">workspaces</see>. 
    ///         In addition, a <see cref="AtomMemberResources">collection</see> <i>may</i> appear in more than one <see cref="AtomWorkspace">workspace</see>.
    ///     </para>
    /// </remarks>
    [Serializable()]
    public class AtomWorkspace : IComparable, IExtensibleSyndicationObject, IAtomCommonObjectAttributes
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the base URI other than the base URI of the document or external entity.
        /// </summary>
        private Uri commonObjectBaseUri;
        /// <summary>
        /// Private member to hold the natural or formal language in which the content is written.
        /// </summary>
        private CultureInfo commonObjectLanguage;
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
        /// <summary>
        /// Private member to hold a human-readable title for the workspace.
        /// </summary>
        private AtomTextConstruct workspaceTitle    = new AtomTextConstruct();
        /// <summary>
        /// Private member to hold the collections associated to this workspace.
        /// </summary>
        private IEnumerable<AtomMemberResources> workspaceCollections;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region AtomWorkspace()
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomWorkspace"/> class.
        /// </summary>
        public AtomWorkspace()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region AtomWorkspace(AtomTextConstruct title)
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomWorkspace"/> class using the supplied <see cref="AtomTextConstruct"/>.
        /// </summary>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the workspace.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomWorkspace(AtomTextConstruct title)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded property
            //------------------------------------------------------------
            this.Title  = title;
        }
        #endregion

        #region AtomWorkspace(AtomTextConstruct title, Collection<AtomMemberResources> collections)
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomWorkspace"/> class using the supplied <see cref="AtomTextConstruct"/> and <see cref="Collection{AtomMemberResources}"/>.
        /// </summary>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the workspace.</param>
        /// <param name="collections"></param>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="collections"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomWorkspace(AtomTextConstruct title, Collection<AtomMemberResources> collections)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded property
            //------------------------------------------------------------
            this.Title  = title;

            Guard.ArgumentNotNull(collections, "collections");
            foreach (AtomMemberResources collection in collections)
            {
                this.AddCollection(collection);
            }
        }
        #endregion

        //============================================================
        //	INDEXERS
        //============================================================
        #region this[int index]
        /// <summary>
        /// Gets or sets the <see cref="AtomMemberResources"/> available for editing at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the collection to get or set.</param>
        /// <returns>The <see cref="AtomMemberResources"/> available for editing at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="AtomWorkspace.Collections"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomMemberResources this[int index]
        {
            get
            {
                return ((Collection<AtomMemberResources>)this.Collections)[index];
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                ((Collection<AtomMemberResources>)this.Collections)[index] = value;
            }
        }
        #endregion

        //============================================================
        //	COMMON PROPERTIES
        //============================================================
        #region BaseUri
        /// <summary>
        /// Gets or sets the base URI other than the base URI of the document or external entity.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>, 
        ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
        /// </remarks>
        public Uri BaseUri
        {
            get
            {
                return commonObjectBaseUri;
            }

            set
            {
                commonObjectBaseUri = value;
            }
        }
        #endregion

        #region Language
        /// <summary>
        /// Gets or sets the natural or formal language in which the content is written.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
        ///     </para>
        /// </remarks>
        public CultureInfo Language
        {
            get
            {
                return commonObjectLanguage;
            }

            set
            {
                commonObjectLanguage = value;
            }
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
        //	PUBLIC PROPERTIES
        //============================================================
        #region Collections
        /// <summary>
        /// Gets or sets the collections of resources available for editing that are associated with this workspace.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="AtomMemberResources"/> objects that represent the collections of resources available for editing that are associated with this workspace.</value>
        /// <remarks>
        ///     <para>This <see cref="IEnumerable{T}"/> collection of <see cref="AtomMemberResources"/> objects is internally represented as a <see cref="Collection{T}"/> collection.</para>
        ///     <para>The <see cref="Collections"/> for the <see cref="AtomWorkspace"/> can contain zero or more <see cref="AtomMemberResources"/> objects.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<AtomMemberResources> Collections
        {
            get
            {
                if (workspaceCollections == null)
                {
                    workspaceCollections = new Collection<AtomMemberResources>();
                }
                return workspaceCollections;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                workspaceCollections = value;
            }
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets information that conveys a human-readable title for this workspace.
        /// </summary>
        /// <value>
        ///     A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this workspace. 
        ///     The default value is an empty <see cref="AtomTextConstruct"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomTextConstruct Title
        {
            get
            {
                return workspaceTitle;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                workspaceTitle = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<AtomMemberResources> source, Collection<AtomMemberResources> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{AtomMemberResources}"/> collections.
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
        public static int CompareSequence(Collection<AtomMemberResources> source, Collection<AtomMemberResources> target)
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
        #region AddCollection(AtomMemberResources collection)
        /// <summary>
        /// Adds the supplied <see cref="AtomMemberResources"/> to the <see cref="Collections"/> of the workspace.
        /// </summary>
        /// <param name="collection">The <see cref="AtomMemberResources"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was added to the <see cref="Collections"/> of the workspace, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="collection"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddCollection(AtomMemberResources collection)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded   = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(collection, "collection");

            //------------------------------------------------------------
            //	Add element to collection
            //------------------------------------------------------------
            ((Collection<AtomMemberResources>)this.Collections).Add(collection);
            wasAdded    = true;

            return wasAdded;
        }
        #endregion

        #region FindCollection(Predicate<AtomMemberResources> match)
        /// <summary>
        /// Searches for a collection that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Collections"/> of the workspace.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{AtomMemberResources}"/> delegate that defines the conditions of the <see cref="AtomMemberResources"/> to search for.</param>
        /// <returns>
        ///     The first collection that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="AtomMemberResources"/>.
        /// </returns>
        /// <remarks>
        ///     The <see cref="Predicate{AtomMemberResources}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate. 
        ///     The elements of the current <see cref="Collections"/> are individually passed to the <see cref="Predicate{AtomMemberResources}"/> delegate, moving forward in 
        ///     the <see cref="Collections"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomMemberResources FindCollection(Predicate<AtomMemberResources> match)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(match, "match");

            //------------------------------------------------------------
            //	Perform predicate based search
            //------------------------------------------------------------
            List<AtomMemberResources> list = new List<AtomMemberResources>(this.Collections);
            return list.Find(match);
        }
        #endregion

        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="AtomWorkspace"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="AtomWorkspace"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomWorkspace"/>.
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
            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);

            //------------------------------------------------------------
            //	Attempt to extract common attributes information
            //------------------------------------------------------------
            if (AtomUtility.FillCommonObjectAttributes(this, source))
            {
                wasLoaded = true;
            }

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNavigator titleNavigator           = source.SelectSingleNode("atom:title", manager);
                XPathNodeIterator collectionIterator    = source.Select("app:collection", manager);

                if (titleNavigator != null)
                {
                    this.Title  = new AtomTextConstruct();
                    if (this.Title.Load(titleNavigator))
                    {
                        wasLoaded   = true;
                    }
                }

                if (collectionIterator != null && collectionIterator.Count > 0)
                {
                    while (collectionIterator.MoveNext())
                    {
                        AtomMemberResources collection   = new AtomMemberResources();
                        if (collection.Load(collectionIterator.Current))
                        {
                            this.AddCollection(collection);
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
        /// Loads this <see cref="AtomWorkspace"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomWorkspace"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomWorkspace"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            wasLoaded   = this.Load(source);

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }
        #endregion

        #region RemoveCollection(AtomMemberResources collection)
        /// <summary>
        /// Removes the supplied <see cref="AtomMemberResources"/> from the <see cref="Collections"/> of the workspace.
        /// </summary>
        /// <param name="collection">The <see cref="AtomMemberResources"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was removed from the <see cref="Collections"/> of the workspace, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Collections"/> of the workspace does not contain the specified <see cref="AtomMemberResources"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="collection"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveCollection(AtomMemberResources collection)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(collection, "collection");

            //------------------------------------------------------------
            //	Remove element from collection
            //------------------------------------------------------------
            if (((Collection<AtomMemberResources>)this.Collections).Contains(collection))
            {
                ((Collection<AtomMemberResources>)this.Collections).Remove(collection);
                wasRemoved  = true;
            }

            return wasRemoved;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="AtomWorkspace"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("workspace", AtomUtility.AtomPublishingNamespace);
            AtomUtility.WriteCommonObjectAttributes(this, writer);

            if (this.Title != null)
            {
                this.Title.WriteTo(writer, "title");
            }

            foreach(AtomMemberResources collection in this.Collections)
            {
                collection.WriteTo(writer);
            }

            //------------------------------------------------------------
            //	Write the syndication extensions of the current instance
            //------------------------------------------------------------
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="AtomWorkspace"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="AtomWorkspace"/>.</returns>
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
            AtomWorkspace value  = obj as AtomWorkspace;

            if (value != null)
            {
                int result  = this.Title.CompareTo(value.Title);
                result      = result | AtomWorkspace.CompareSequence(((Collection<AtomMemberResources>)this.Collections), ((Collection<AtomMemberResources>)value.Collections));
                result      = result | AtomUtility.CompareCommonObjectAttributes(this, value);

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
            if (!(obj is AtomWorkspace))
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
        public static bool operator ==(AtomWorkspace first, AtomWorkspace second)
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
        public static bool operator !=(AtomWorkspace first, AtomWorkspace second)
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
        public static bool operator <(AtomWorkspace first, AtomWorkspace second)
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
        public static bool operator >(AtomWorkspace first, AtomWorkspace second)
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
