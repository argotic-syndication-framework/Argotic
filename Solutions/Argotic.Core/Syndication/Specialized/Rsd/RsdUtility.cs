/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/08/2008	brian.kuhn	Created RsdUtility Class
****************************************************************************/
using System;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Provides methods that comprise common utility features shared across the Really Simple Discoverability (RSD) syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Really Simple Discoverability (RSD) syndication entities within the framework.</remarks>
    internal static class RsdUtility
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the Really Simple Discoverability (RSD) 1.0 namespace identifier.
        /// </summary>
        private const string RSD_NAMESPACE  = "http://archipelago.phrasewise.com/rsd";
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region RsdNamespace
        /// <summary>
        /// Gets the XML namespace URI for the Really Simple Discoverability (RSD) 1.0 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Really Simple Discoverability (RSD) 1.0 specification.</value>
        public static string RsdNamespace
        {
            get
            {
                return RSD_NAMESPACE;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region CreateNamespaceManager(XmlNameTable nameTable)
        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Really Simple Discoverability (RSD) syndication entities.
        /// </summary>
        /// <param name="nameTable">The table of atomized string objects.</param>
        /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            XmlNamespaceManager manager = null;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(nameTable, "nameTable");

            //------------------------------------------------------------
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace("rsd", !String.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : RSD_NAMESPACE);

            return manager;
        }
        #endregion

        #region SelectSafe(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
        /// <summary>
        /// Selects a node set using the specified XPath expression with the <see cref="IXmlNamespaceResolver"/> object specified to resolve namespace prefixes.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to execute the XPath query against.</param>
        /// <param name="xpath">A <see cref="String"/> representing an XPath expression. May be <i>/</i> delimited query. Query shound <b>not</b> contain any prefixing.</param>
        /// <param name="resolver">The <see cref="IXmlNamespaceResolver"/> object used to resolve namespace prefixes in the XPath query.</param>
        /// <returns>
        ///     An <see cref="XPathNodeIterator"/> that points to the selected node set.
        /// </returns>
        /// <remarks>
        ///     This method performs a safe XPath query for Really Simple Discoverability (RSD) syndication entities by first attempting the query as provided. 
        ///     If no result is found, this method then attempts the query without any prefixing by removing instances of <i>rsd:</i> from the supplied <paramref name="xpath"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resolver"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNodeIterator SelectSafe(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            XPathNodeIterator iterator = null;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNullOrEmptyString(xpath, "xpath");
            Guard.ArgumentNotNull(resolver, "resolver");

            iterator    = source.Select(xpath, resolver);

            if (iterator == null || iterator.Count <= 0)
            {
                string safeXpath    = xpath.Replace("rsd:", String.Empty);
                iterator            = source.Select(safeXpath, resolver);
            }

            return iterator;
        }
        #endregion

        #region SelectSafeSingleNode(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
        /// <summary>
        /// Selects a single node in the <see cref="XPathNavigator"/> object using the specified XPath query with the <see cref="IXmlNamespaceResolver"/> object specified to resolve namespace prefixes.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to execute the XPath query against.</param>
        /// <param name="xpath">A <see cref="String"/> representing an XPath expression. May be <i>/</i> delimited query. Query shound <b>not</b> contain any prefixing.</param>
        /// <param name="resolver">The <see cref="IXmlNamespaceResolver"/> object used to resolve namespace prefixes in the XPath query.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> object that contains the first matching node for the XPath query specified; otherwise <b>null</b> if there are no query results.
        /// </returns>
        /// <remarks>
        ///     This method performs a safe XPath query for Really Simple Discoverability (RSD) syndication entities by first attempting the query as provided. 
        ///     If no result is found, this method then attempts the query without any prefixing by removing instances of <i>rsd:</i> from the supplied <paramref name="xpath"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resolver"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator SelectSafeSingleNode(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            XPathNavigator navigator = null;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNullOrEmptyString(xpath, "xpath");
            Guard.ArgumentNotNull(resolver, "resolver");

            navigator   = source.SelectSingleNode(xpath, resolver);

            if (navigator == null)
            {
                string safeXpath    = xpath.Replace("rsd:", String.Empty);
                navigator           = source.SelectSingleNode(safeXpath, resolver);
            }

            return navigator;
        }
        #endregion
    }
}
