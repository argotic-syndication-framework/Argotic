﻿namespace Argotic.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;
    using Argotic.Extensions.Core;

    /// <summary>
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="IExtensibleSyndicationObject"/>.
    /// </summary>
    public class SyndicationExtensionAdapter
    {
        /// <summary>
        /// Private member to hold the XPathNavigator used to load a syndication extension.
        /// </summary>
        private XPathNavigator adapterNavigator;

        /// <summary>
        /// Private member to hold the XPathNavigator used to configure the load of a syndication extension.
        /// </summary>
        private SyndicationResourceLoadSettings adapterSettings = new SyndicationResourceLoadSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationExtensionAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the extended syndication resource information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="IExtensibleSyndicationObject"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationExtensionAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(settings, "settings");

            this.adapterNavigator = navigator;
            this.adapterSettings = settings;
        }

        /// <summary>
        /// Gets the collection of <see cref="Type"/> objects that represent <see cref="ISyndicationExtension"/> instances natively supported by the framework.
        /// </summary>
        /// <value>
        ///     <see cref="Collection{T}"/> collection of <see cref="Type"/> objects that represent <see cref="ISyndicationExtension"/> instances natively supported by the framework.
        /// </value>
        public static Collection<Type> FrameworkExtensions
        {
            get
            {
                Collection<Type> extensions = new Collection<Type>();
#if true
                foreach (var type in Assembly.GetExecutingAssembly()
                                            .GetExportedTypes()
                                            .Where(t => typeof(SyndicationExtension).IsAssignableFrom(t) && t != typeof(SyndicationExtension)))
                {
                    extensions.Add(type);
                }
#else
				extensions.Add(typeof(BasicGeocodingSyndicationExtension));
				extensions.Add(typeof(BlogChannelSyndicationExtension));
				extensions.Add(typeof(CreativeCommonsSyndicationExtension));
				extensions.Add(typeof(DublinCoreElementSetSyndicationExtension));
				extensions.Add(typeof(DublinCoreMetadataTermsSyndicationExtension));
				extensions.Add(typeof(FeedHistorySyndicationExtension));
				extensions.Add(typeof(FeedRankSyndicationExtension));
				extensions.Add(typeof(FeedSynchronizationSyndicationExtension));
				extensions.Add(typeof(ITunesSyndicationExtension));
				extensions.Add(typeof(LiveJournalSyndicationExtension));
				extensions.Add(typeof(PheedSyndicationExtension));
				extensions.Add(typeof(PingbackSyndicationExtension));
				extensions.Add(typeof(SimpleListSyndicationExtension));
				extensions.Add(typeof(SiteSummaryContentSyndicationExtension));
				extensions.Add(typeof(SiteSummarySlashSyndicationExtension));
				extensions.Add(typeof(SiteSummaryUpdateSyndicationExtension));
				extensions.Add(typeof(TrackbackSyndicationExtension));
				extensions.Add(typeof(WellFormedWebCommentsSyndicationExtension));
				extensions.Add(typeof(YahooMediaSyndicationExtension));
#endif
                return extensions;
            }
        }

        /// <summary>
        /// Gets the <see cref="XPathNavigator"/> used to fill an extensible syndication resource.
        /// </summary>
        /// <value>The <see cref="XPathNavigator"/> used to fill an extensible syndication resource.</value>
        public XPathNavigator Navigator => this.adapterNavigator;

        /// <summary>
        /// Gets the <see cref="SyndicationResourceLoadSettings"/> used to configure the fill of an extensible syndication resource.
        /// </summary>
        /// <value>The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill of an extensible syndication resource.</value>
        public SyndicationResourceLoadSettings Settings => this.adapterSettings;

        /// <summary>
        /// Fills the specified collection of <see cref="Type"/> objects using the supplied <see cref="IExtensibleSyndicationObject"/>.
        /// </summary>
        /// <param name="entity">A <see cref="IExtensibleSyndicationObject"/> to extract syndication extensions from.</param>
        /// <param name="types">The <see cref="Collection{T}"/> collection of <see cref="Type"/> objects to be filled.</param>
        /// <remarks>
        ///    This method provides implementers of the <see cref="ISyndicationResource"/> interface with a simple way
        ///    to fill a <see cref="SyndicationResourceSaveSettings.SupportedExtensions"/> collection when implementing the
        ///    <see cref="ISyndicationResource.Save(XmlWriter, SyndicationResourceSaveSettings)"/> abstract method.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="types"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void FillExtensionTypes(IExtensibleSyndicationObject entity, Collection<Type> types)
        {
            Guard.ArgumentNotNull(entity, "entity");
            Guard.ArgumentNotNull(types, "types");

            if (entity.HasExtensions)
            {
                foreach (ISyndicationExtension extension in entity.Extensions)
                {
                    if (extension != null)
                    {
                        Type type = extension.GetType();
                        if (!types.Contains(type))
                        {
                            types.Add(type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a collection of <see cref="ISyndicationExtension"/> instances for the specified types.
        /// </summary>
        /// <param name="types">A <see cref="Collection{T}"/> collection of <see cref="Type"/> objects to be instantiated.</param>
        /// <returns>A <see cref="Collection{T}"/> collection of <see cref="ISyndicationExtension"/> objects instantiated using the supplied <paramref name="types"/>.</returns>
        /// <remarks>
        ///     <para>Each <see cref="ISyndicationExtension"/> instance in the <see cref="Collection{T}"/> collection will be instantiated using its default constructor. </para>
        ///     <para>Types that are a null reference or do not implement the <see cref="ISyndicationExtension"/> interface are ignored.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="types"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Collection<ISyndicationExtension> GetExtensions(Collection<Type> types)
        {
            Collection<ISyndicationExtension> extensions = new Collection<ISyndicationExtension>();
            Guard.ArgumentNotNull(types, "types");

            foreach (Type type in types)
            {
                if (type != null)
                {
                    ISyndicationExtension extension = Activator.CreateInstance(type) as ISyndicationExtension;
                    if (extension != null)
                    {
                        extensions.Add(extension);
                    }
                }
            }

            return extensions;
        }

        /// <summary>
        /// Creates a collection of <see cref="ISyndicationExtension"/> instances for the specified types.
        /// </summary>
        /// <param name="types">A <see cref="Collection{T}"/> collection of <see cref="Type"/> objects that represent user-defined syndication extensions to be instantiated.</param>
        /// <param name="namespaces">A collection of XML nameapces that are used to filter the available native framework syndication extensions.</param>
        /// <returns>
        ///     A <see cref="Collection{T}"/> collection of <see cref="ISyndicationExtension"/> objects instantiated using the supplied <paramref name="types"/> and <paramref name="namespaces"/>.
        /// </returns>
        /// <remarks>
        ///     This method instantiates all of the available native framework syndication extensions, and then filters them based on the XML namespaces and prefixes contained in the supplied <paramref name="namespaces"/>.
        ///     The user defined syndication extensions are then instantiated, and are added to the return collection if they do not already exist.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="types"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="namespaces"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Collection<ISyndicationExtension> GetExtensions(Collection<Type> types, Dictionary<string, string> namespaces)
        {
            Collection<ISyndicationExtension> supportedExtensions = new Collection<ISyndicationExtension>();
            Guard.ArgumentNotNull(types, "types");
            Guard.ArgumentNotNull(namespaces, "namespaces");

            Collection<ISyndicationExtension> nativeExtensions = SyndicationExtensionAdapter.GetExtensions(SyndicationExtensionAdapter.FrameworkExtensions);

            foreach (ISyndicationExtension extension in nativeExtensions)
            {
                if (namespaces.ContainsValue(extension.XmlNamespace) || namespaces.ContainsKey(extension.XmlPrefix))
                {
                    if (!supportedExtensions.Contains(extension))
                    {
                        supportedExtensions.Add(extension);
                    }
                }
            }

            Collection<ISyndicationExtension> userExtensions = SyndicationExtensionAdapter.GetExtensions(types);
            foreach (ISyndicationExtension extension in userExtensions)
            {
                if (!supportedExtensions.Contains(extension))
                {
                    supportedExtensions.Add(extension);
                }
            }

            return supportedExtensions;
        }

        /// <summary>
        /// Saves the supplied <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="extensions">A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent the syndication extensions to be written.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="extensions"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteExtensionsTo(IEnumerable<ISyndicationExtension> extensions, XmlWriter writer)
        {
            Guard.ArgumentNotNull(extensions, "extensions");
            Guard.ArgumentNotNull(writer, "writer");

            foreach (ISyndicationExtension extension in extensions)
            {
                extension.WriteTo(writer);
            }
        }

        /// <summary>
        /// Writes the prefixed XML namespace declarations for the supplied <see cref="Collection{T}"/> collection of syndication extension <see cref="Type"/> objects to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="types">A <see cref="Collection{T}"/> collection of <see cref="Type"/> objects that represent the syndication extensions to write prefixed XML namespace declarations for.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="types"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteXmlNamespaceDeclarations(Collection<Type> types, XmlWriter writer)
        {
            Guard.ArgumentNotNull(types, "types");
            Guard.ArgumentNotNull(writer, "writer");

            foreach (Type type in types)
            {
                if (type != null)
                {
                    ISyndicationExtension extension = Activator.CreateInstance(type) as ISyndicationExtension;
                    if (extension != null)
                    {
                        extension.WriteXmlNamespaceDeclaration(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Modifies the <see cref="IExtensibleSyndicationObject"/> to match the data source.
        /// </summary>
        /// <remarks>
        ///     A default <see cref="XmlNamespaceManager"/> is created against this adapter's <see cref="Navigator"/> property
        ///     when resolving prefixed syndication elements and attributes.
        /// </remarks>
        /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(IExtensibleSyndicationObject entity)
        {
            Guard.ArgumentNotNull(entity, "entity");
            XmlNamespaceManager manager = new XmlNamespaceManager(this.Navigator.NameTable);

            this.Fill(entity, manager);
        }

        /// <summary>
        /// Modifies the <see cref="IExtensibleSyndicationObject"/> to match the data source.
        /// </summary>
        /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> to be filled.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve prefixed syndication elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(IExtensibleSyndicationObject entity, XmlNamespaceManager manager)
        {
            Collection<ISyndicationExtension> extensions = new Collection<ISyndicationExtension>();
            Guard.ArgumentNotNull(entity, "entity");
            Guard.ArgumentNotNull(manager, "manager");

            if (this.Settings.AutoDetectExtensions)
            {
                extensions = SyndicationExtensionAdapter.GetExtensions(this.Settings.SupportedExtensions, (Dictionary<string, string>)this.Navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml));
            }
            else
            {
                extensions = SyndicationExtensionAdapter.GetExtensions(this.Settings.SupportedExtensions);
            }

            foreach (ISyndicationExtension extension in extensions)
            {
                if (extension.ExistsInSource(this.Navigator) && extension.GetType() != entity.GetType())
                {
                    ISyndicationExtension instance = (ISyndicationExtension)Activator.CreateInstance(extension.GetType());

                    if (instance.Load(this.Navigator))
                    {
                        ((Collection<ISyndicationExtension>)entity.Extensions).Add(instance);
                    }
                }
            }
        }
    }
}