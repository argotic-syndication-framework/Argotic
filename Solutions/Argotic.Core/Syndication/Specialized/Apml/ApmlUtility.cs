namespace Argotic.Syndication.Specialized
{
    using System;
    using System.Xml;

    using Argotic.Common;

    /// <summary>
    /// Provides methods that comprise common utility features shared across the Attention Profiling Markup Language (APML) syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Attention Profiling Markup Language (APML) syndication entities within the framework.</remarks>
    internal static class ApmlUtility
    {
        /// <summary>
        /// Private member to hold the Attention Profiling Markup Language (APML) 0.6 namespace identifier.
        /// </summary>
        private const string InternalApmlNamespace = "http://www.apml.org/apml-0.6";

        /// <summary>
        /// Gets the XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.</value>
        public static string ApmlNamespace
        {
            get
            {
                return InternalApmlNamespace;
            }
        }

        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Attention Profiling Markup Language (APML) syndication entities.
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
                "apml",
                !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : InternalApmlNamespace);

            return manager;
        }
    }
}