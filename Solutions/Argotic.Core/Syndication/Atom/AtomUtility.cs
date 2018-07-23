namespace Argotic.Syndication
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;

    /// <summary>
    /// Provides methods that comprise common utility features shared across the Atom syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Atom syndication entities within the framework.</remarks>
    internal static class AtomUtility
    {
        /// <summary>
        /// Private member to hold the Atom 1.0 namespace identifier.
        /// </summary>
        private const string InternalAtomNamespace = "http://www.w3.org/2005/Atom";

        /// <summary>
        /// Private member to hold the Atom Publishing Protocol 1.0 namespace identifier.
        /// </summary>
        private const string InternalAtompubNamespace = "http://www.w3.org/2007/app";

        /// <summary>
        /// Private member to hold the XHTML namespace identifier.
        /// </summary>
        private const string InternalXhtmlNamespace = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// Private member to hold the XML 1.1 namespace identifier.
        /// </summary>
        private const string InternalXmlNamespace = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// Gets the XML namespace URI for the Atom 1.0 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Atom 1.0 specification.</value>
        public static string AtomNamespace
        {
            get
            {
                return InternalAtomNamespace;
            }
        }

        /// <summary>
        /// Gets the XML namespace URI for the Atom Publishing Protocol 1.0 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Atom Publishing Protocol 1.0 specification.</value>
        public static string AtomPublishingNamespace
        {
            get
            {
                return InternalAtompubNamespace;
            }
        }

        /// <summary>
        /// Gets the XML namespace URI for the XHTML specification.
        /// </summary>
        /// <value>The XML namespace URI for the Extensible Hyper-Text Markup Lanaguage (XHTML) specification.</value>
        public static string XhtmlNamespace
        {
            get
            {
                return InternalXhtmlNamespace;
            }
        }

        /// <summary>
        /// Compares objects that implement the <see cref="IAtomCommonObjectAttributes"/> interface.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to be compared.</param>
        /// <param name="target">A object that implements the <see cref="IAtomCommonObjectAttributes"/> to compare with the <paramref name="source"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public static int CompareCommonObjectAttributes(
            IAtomCommonObjectAttributes source,
            IAtomCommonObjectAttributes target)
        {
            int result = 0;
            if (source == null && target == null)
            {
                return 0;
            }

            if (source != null && target == null)
            {
                return 1;
            }

            if (source == null && target != null)
            {
                return -1;
            }

            result = result | Uri.Compare(
                         source.BaseUri,
                         target.BaseUri,
                         UriComponents.AbsoluteUri,
                         UriFormat.SafeUnescaped,
                         StringComparison.OrdinalIgnoreCase);

            string sourceLanguageName = source.Language != null ? source.Language.Name : string.Empty;
            string targetLanguageName = target.Language != null ? target.Language.Name : string.Empty;
            result = result | string.Compare(
                         sourceLanguageName,
                         targetLanguageName,
                         StringComparison.OrdinalIgnoreCase);

            return result;
        }

        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Atom syndication entities.
        /// </summary>
        /// <param name="nameTable">The table of atomized string objects.</param>
        /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
        {
            XmlNamespaceManager manager = null;
            Guard.ArgumentNotNull(nameTable, "nameTable");
            manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace(
                "atom",
                !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : InternalAtomNamespace);
            manager.AddNamespace("app", InternalAtompubNamespace);
            manager.AddNamespace("xhtml", InternalXhtmlNamespace);

            return manager;
        }

        /// <summary>
        /// Modifies the <see cref="IAtomCommonObjectAttributes"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract Atom common attribute information from.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool FillCommonObjectAttributes(IAtomCommonObjectAttributes target, XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = CreateNamespaceManager(source.NameTable);
            string xmlBaseAttribute = source.GetAttribute("base", manager.LookupNamespace("xml"));
            if (!string.IsNullOrEmpty(xmlBaseAttribute))
            {
                Uri baseUri;
                if (Uri.TryCreate(xmlBaseAttribute, UriKind.RelativeOrAbsolute, out baseUri))
                {
                    target.BaseUri = baseUri;
                    wasLoaded = true;
                }
            }

            string xmlLangAttribute = source.GetAttribute("lang", manager.LookupNamespace("xml"));
            if (!string.IsNullOrEmpty(xmlLangAttribute))
            {
                try
                {
                    CultureInfo language = new CultureInfo(source.XmlLang);
                    target.Language = language;
                    wasLoaded = true;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Unable to determine CultureInfo with a name of {0}.",
                        source.XmlLang);
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="IAtomCommonObjectAttributes"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to extract Atom common attribute information from.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteCommonObjectAttributes(IAtomCommonObjectAttributes source, XmlWriter writer)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(writer, "writer");
            if (source.BaseUri != null)
            {
                writer.WriteAttributeString("xml", "base", InternalXmlNamespace, source.BaseUri.ToString());
            }

            if (source.Language != null)
            {
                writer.WriteAttributeString("xml", "lang", InternalXmlNamespace, source.Language.Name);
            }
        }
    }
}