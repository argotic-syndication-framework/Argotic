/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/08/2008	brian.kuhn	Created ApmlProfile Class
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
    /// Represents an attention profile that can be associated to an <see cref="ApmlDocument"/>.
    /// </summary>
    /// <seealso cref="ApmlDocument.Profiles"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlProfile class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Apml\ApmlProfileExample.cs" 
    ///             region="ApmlProfile" 
    ///         />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
    [Serializable()]
    public class ApmlProfile : IComparable, IExtensibleSyndicationObject
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
        /// Private member to hold the unique name of the profile.
        /// </summary>
        private string profileName  = String.Empty;
        /// <summary>
        /// Private member to hold the implicit concepts of the profile.
        /// </summary>
        private Collection<ApmlConcept> profileImplicitConcepts;
        /// <summary>
        /// Private member to hold the explicit concepts of the profile.
        /// </summary>
        private Collection<ApmlConcept> profileExplicitConcepts;
        /// <summary>
        /// Private member to hold the implicit sources of the profile.
        /// </summary>
        private Collection<ApmlSource> profileImplicitSources;
        /// <summary>
        /// Private member to hold the explicit sources of the profile.
        /// </summary>
        private Collection<ApmlSource> profileExplicitSources;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region ApmlProfile()
        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlProfile"/> class.
        /// </summary>
        public ApmlProfile()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
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
        #region ExplicitConcepts
        /// <summary>
        /// Gets or sets the explicit concepts of this profile.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="ApmlConcept"/> objects that represent the explicit concepts of this profile.</value>
        public Collection<ApmlConcept> ExplicitConcepts
        {
            get
            {
                if (profileExplicitConcepts == null)
                {
                    profileExplicitConcepts = new Collection<ApmlConcept>();
                }
                return profileExplicitConcepts;
            }
        }
        #endregion

        #region ExplicitSources
        /// <summary>
        /// Gets or sets the explicit sources of this profile.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="ApmlSource"/> objects that represent the explicit sources of this profile.</value>
        public Collection<ApmlSource> ExplicitSources
        {
            get
            {
                if (profileExplicitSources == null)
                {
                    profileExplicitSources = new Collection<ApmlSource>();
                }
                return profileExplicitSources;
            }
        }
        #endregion

        #region ImplicitConcepts
        /// <summary>
        /// Gets or sets the implicit concepts of this profile.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="ApmlConcept"/> objects that represent the implicit concepts of this profile.</value>
        public Collection<ApmlConcept> ImplicitConcepts
        {
            get
            {
                if (profileImplicitConcepts == null)
                {
                    profileImplicitConcepts = new Collection<ApmlConcept>();
                }
                return profileImplicitConcepts;
            }
        }
        #endregion

        #region ImplicitSources
        /// <summary>
        /// Gets or sets the implicit sources of this profile.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="ApmlSource"/> objects that represent the implicit sources of this profile.</value>
        public Collection<ApmlSource> ImplicitSources
        {
            get
            {
                if (profileImplicitSources == null)
                {
                    profileImplicitSources = new Collection<ApmlSource>();
                }
                return profileImplicitSources;
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Gets or sets the name of this profile.
        /// </summary>
        /// <value>The unique name of this profile.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Name
        {
            get
            {
                return profileName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                profileName = value.Trim();
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
        #region CompareSequence(Collection<ApmlConcept> source, Collection<ApmlConcept> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{ApmlConcept}"/> collections.
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
        public static int CompareSequence(Collection<ApmlConcept> source, Collection<ApmlConcept> target)
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

        #region CompareSequence(Collection<ApmlSource> source, Collection<ApmlSource> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{ApmlSource}"/> collections.
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
        public static int CompareSequence(Collection<ApmlSource> source, Collection<ApmlSource> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result = result | source[i].CompareTo(target[i]);
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
        /// Loads this <see cref="ApmlProfile"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ApmlProfile"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlProfile"/>.
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
                string nameAttribute    = source.GetAttribute("name", String.Empty);
                if (!String.IsNullOrEmpty(nameAttribute))
                {
                    this.Name   = nameAttribute;
                    wasLoaded   = true;
                }
            }

            if(source.HasChildren)
            {
                XPathNavigator implicitDataNavigator    = source.SelectSingleNode("apml:ImplicitData", manager);
                XPathNavigator explicitDataNavigator    = source.SelectSingleNode("apml:ExplicitData", manager);

                if (implicitDataNavigator != null)
                {
                    XPathNodeIterator conceptsIterator  = implicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                    if (conceptsIterator != null && conceptsIterator.Count > 0)
                    {
                        while (conceptsIterator.MoveNext())
                        {
                            ApmlConcept concept = new ApmlConcept();
                            if (concept.Load(conceptsIterator.Current))
                            {
                                this.ImplicitConcepts.Add(concept);
                                wasLoaded       = true;
                            }
                        }
                    }

                    XPathNodeIterator sourcesIterator   = implicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                    if (sourcesIterator != null && sourcesIterator.Count > 0)
                    {
                        while (sourcesIterator.MoveNext())
                        {
                            ApmlSource attentionSource  = new ApmlSource();
                            if (attentionSource.Load(sourcesIterator.Current))
                            {
                                this.ImplicitSources.Add(attentionSource);
                                wasLoaded               = true;
                            }
                        }
                    }
                }

                if (explicitDataNavigator != null)
                {
                    XPathNodeIterator conceptsIterator  = explicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                    if (conceptsIterator != null && conceptsIterator.Count > 0)
                    {
                        while (conceptsIterator.MoveNext())
                        {
                            ApmlConcept concept = new ApmlConcept();
                            if (concept.Load(conceptsIterator.Current))
                            {
                                this.ExplicitConcepts.Add(concept);
                                wasLoaded       = true;
                            }
                        }
                    }

                    XPathNodeIterator sourcesIterator   = explicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                    if (sourcesIterator != null && sourcesIterator.Count > 0)
                    {
                        while (sourcesIterator.MoveNext())
                        {
                            ApmlSource attentionSource  = new ApmlSource();
                            if (attentionSource.Load(sourcesIterator.Current))
                            {
                                this.ExplicitSources.Add(attentionSource);
                                wasLoaded               = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads this <see cref="ApmlProfile"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlProfile"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlProfile"/>.
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
                string nameAttribute    = source.GetAttribute("name", String.Empty);
                if (!String.IsNullOrEmpty(nameAttribute))
                {
                    this.Name   = nameAttribute;
                    wasLoaded   = true;
                }
            }

            if(source.HasChildren)
            {
                XPathNavigator implicitDataNavigator    = source.SelectSingleNode("apml:ImplicitData", manager);
                XPathNavigator explicitDataNavigator    = source.SelectSingleNode("apml:ExplicitData", manager);

                if (implicitDataNavigator != null)
                {
                    XPathNodeIterator conceptsIterator  = implicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                    if (conceptsIterator != null && conceptsIterator.Count > 0)
                    {
                        while (conceptsIterator.MoveNext())
                        {
                            ApmlConcept concept = new ApmlConcept();
                            if (concept.Load(conceptsIterator.Current, settings))
                            {
                                this.ImplicitConcepts.Add(concept);
                                wasLoaded       = true;
                            }
                        }
                    }

                    XPathNodeIterator sourcesIterator   = implicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                    if (sourcesIterator != null && sourcesIterator.Count > 0)
                    {
                        while (sourcesIterator.MoveNext())
                        {
                            ApmlSource attentionSource  = new ApmlSource();
                            if (attentionSource.Load(sourcesIterator.Current, settings))
                            {
                                this.ImplicitSources.Add(attentionSource);
                                wasLoaded               = true;
                            }
                        }
                    }
                }

                if (explicitDataNavigator != null)
                {
                    XPathNodeIterator conceptsIterator  = explicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                    if (conceptsIterator != null && conceptsIterator.Count > 0)
                    {
                        while (conceptsIterator.MoveNext())
                        {
                            ApmlConcept concept = new ApmlConcept();
                            if (concept.Load(conceptsIterator.Current, settings))
                            {
                                this.ExplicitConcepts.Add(concept);
                                wasLoaded       = true;
                            }
                        }
                    }

                    XPathNodeIterator sourcesIterator   = explicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                    if (sourcesIterator != null && sourcesIterator.Count > 0)
                    {
                        while (sourcesIterator.MoveNext())
                        {
                            ApmlSource attentionSource  = new ApmlSource();
                            if (attentionSource.Load(sourcesIterator.Current, settings))
                            {
                                this.ExplicitSources.Add(attentionSource);
                                wasLoaded               = true;
                            }
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
        /// Saves the current <see cref="ApmlProfile"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("Profile", ApmlUtility.ApmlNamespace);

            writer.WriteAttributeString("name", this.Name);

            if(this.ImplicitConcepts.Count > 0 || this.ImplicitSources.Count > 0)
            {
                writer.WriteStartElement("ImplicitData", ApmlUtility.ApmlNamespace);

                if (this.ImplicitConcepts.Count > 0)
                {
                    writer.WriteStartElement("Concepts", ApmlUtility.ApmlNamespace);
                    foreach (ApmlConcept concept in this.ImplicitConcepts)
                    {
                        concept.WriteTo(writer);
                    }
                    writer.WriteEndElement();
                }

                if (this.ImplicitSources.Count > 0)
                {
                    writer.WriteStartElement("Sources", ApmlUtility.ApmlNamespace);
                    foreach (ApmlSource source in this.ImplicitSources)
                    {
                        source.WriteTo(writer);
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (this.ExplicitConcepts.Count > 0 || this.ExplicitSources.Count > 0)
            {
                writer.WriteStartElement("ExplicitData", ApmlUtility.ApmlNamespace);

                if (this.ExplicitConcepts.Count > 0)
                {
                    writer.WriteStartElement("Concepts", ApmlUtility.ApmlNamespace);
                    foreach (ApmlConcept concept in this.ExplicitConcepts)
                    {
                        concept.WriteTo(writer);
                    }
                    writer.WriteEndElement();
                }

                if (this.ExplicitSources.Count > 0)
                {
                    writer.WriteStartElement("Sources", ApmlUtility.ApmlNamespace);
                    foreach (ApmlSource source in this.ExplicitSources)
                    {
                        source.WriteTo(writer);
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
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
        /// Returns a <see cref="String"/> that represents the current <see cref="ApmlProfile"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="ApmlProfile"/>.</returns>
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
            ApmlProfile value  = obj as ApmlProfile;

            if (value != null)
            {
                int result  = ApmlProfile.CompareSequence(this.ExplicitConcepts, value.ExplicitConcepts);
                result      = result | ApmlProfile.CompareSequence(this.ExplicitSources, value.ExplicitSources);
                result      = result | ApmlProfile.CompareSequence(this.ImplicitConcepts, value.ImplicitConcepts);
                result      = result | ApmlProfile.CompareSequence(this.ImplicitSources, value.ImplicitSources);
                result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is ApmlProfile))
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
        public static bool operator ==(ApmlProfile first, ApmlProfile second)
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
        public static bool operator !=(ApmlProfile first, ApmlProfile second)
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
        public static bool operator <(ApmlProfile first, ApmlProfile second)
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
        public static bool operator >(ApmlProfile first, ApmlProfile second)
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
