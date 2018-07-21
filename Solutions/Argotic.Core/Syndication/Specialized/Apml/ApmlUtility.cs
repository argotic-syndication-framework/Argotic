/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/08/2008	brian.kuhn	Created ApmlUtility Class
****************************************************************************/
using System;
using System.Xml;

using Argotic.Common;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Provides methods that comprise common utility features shared across the Attention Profiling Markup Language (APML) syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Attention Profiling Markup Language (APML) syndication entities within the framework.</remarks>
    internal static class ApmlUtility
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the Attention Profiling Markup Language (APML) 0.6 namespace identifier.
        /// </summary>
        private const string APML_NAMESPACE  = "http://www.apml.org/apml-0.6";
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region ApmlNamespace
        /// <summary>
        /// Gets the XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.</value>
        public static string ApmlNamespace
        {
            get
            {
                return APML_NAMESPACE;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region CreateNamespaceManager(XmlNameTable nameTable)
        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Attention Profiling Markup Language (APML) syndication entities.
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
            manager.AddNamespace("apml", !String.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : APML_NAMESPACE);

            return manager;
        }
        #endregion
    }
}
