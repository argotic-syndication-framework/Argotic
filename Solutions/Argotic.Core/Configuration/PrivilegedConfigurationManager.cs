using System;
using System.Configuration;
using System.Security.Permissions;
using System.Threading;

using Argotic.Common;
using Argotic.Configuration.Provider;

namespace Argotic.Configuration
{
    /// <summary>
    /// Provides privileged access to configuration files for client applications. This class cannot be inherited.
    /// </summary>
    //[ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
    internal static class PrivilegedConfigurationManager
    {
        /// <summary>
        /// Private member to hold object used to synchronize locks when reading Trackback configuration information.
        /// </summary>
        private static object configurationManagerTrackbackSyncObject;
        /// <summary>
        /// Private member to hold object used to synchronize locks when reading XML-RPC configuration information.
        /// </summary>
        private static object configurationManagerXmlRpcSyncObject;
        /// <summary>
        /// Private member to hold object used to synchronize locks when reading syndication resource configuration information.
        /// </summary>
        private static object configurationManagerSyndicationResourceSyncObject;

        /// <summary>
        /// Gets the <see cref="Object"/> used when locking acess to the syndication resource configuration file section being managed.
        /// </summary>
        /// <value>The <see cref="Object"/> used when locking acess to the syndication resource configuration file section being managed.</value>
        internal static object SyndicationSyncObject
        {
            get
            {
                if (configurationManagerSyndicationResourceSyncObject == null)
                {
                    Interlocked.CompareExchange(ref configurationManagerSyndicationResourceSyncObject, new object(), null);
                }
                return configurationManagerSyndicationResourceSyncObject;
            }
        }

        /// <summary>
        /// Gets the <see cref="Object"/> used when locking acess to the Trackback configuration file section being managed.
        /// </summary>
        /// <value>The <see cref="Object"/> used when locking acess to the Trackback configuration file section being managed.</value>
        internal static object TrackbackSyncObject
        {
            get
            {
                if (configurationManagerTrackbackSyncObject == null)
                {
                    Interlocked.CompareExchange(ref configurationManagerTrackbackSyncObject, new object(), null);
                }
                return configurationManagerTrackbackSyncObject;
            }
        }

        /// <summary>
        /// Gets the <see cref="Object"/> used when locking acess to the XML-RPC configuration file section being managed.
        /// </summary>
        /// <value>The <see cref="Object"/> used when locking acess to the  XML-RPC configuration file section being managed.</value>
        internal static object XmlRpcSyncObject
        {
            get
            {
                if (configurationManagerXmlRpcSyncObject == null)
                {
                    Interlocked.CompareExchange(ref configurationManagerXmlRpcSyncObject, new object(), null);
                }
                return configurationManagerXmlRpcSyncObject;
            }
        }

        /// <summary>
        /// Retrieves a specified configuration section for the current application's default configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified ConfigurationSection object, or a null reference (Nothing in Visual Basic) if the section does not exist.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="sectionName"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="sectionName"/> is an empty string.</exception>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        [SecurityPermission(SecurityAction.Demand)]
        internal static object GetSection(string sectionName)
        {
            Guard.ArgumentNotNullOrEmptyString(sectionName, "sectionName");

            return ConfigurationManager.GetSection(sectionName);
        }

        /// <summary>
        /// Returns the syndication resource configuration section information.
        /// </summary>
        /// <returns>
        ///     A <see cref="XmlRpcClientSection"/> object that represents the syndication resource configuration information.
        ///     If no configuration section is defined for syndication resources, returns a <b>null</b> reference.
        /// </returns>
        [SecurityPermission(SecurityAction.Demand)]
        internal static SyndicationResourceSection GetSyndicationResourceSection()
        {
            string sectionPath = "argotic.syndication";

            lock (PrivilegedConfigurationManager.SyndicationSyncObject)
            {
                SyndicationResourceSection section   = PrivilegedConfigurationManager.GetSection(sectionPath) as SyndicationResourceSection;
                if (section == null)
                {
                    return null;
                }
                return section;
            }
        }

        /// <summary>
        /// Returns the Trackback client configuration section information.
        /// </summary>
        /// <returns>
        ///     A <see cref="XmlRpcClientSection"/> object that represents the Trackback client configuration information.
        ///     If no configuration section is defined for the client application, returns a <b>null</b> reference.
        /// </returns>
        [SecurityPermission(SecurityAction.Demand)]
        internal static TrackbackClientSection GetTracbackClientSection()
        {
            string sectionPath = "argotic.net/clientSettings/trackback";

            lock (PrivilegedConfigurationManager.TrackbackSyncObject)
            {
                TrackbackClientSection section  = PrivilegedConfigurationManager.GetSection(sectionPath) as TrackbackClientSection;
                if (section == null)
                {
                    return null;
                }
                return section;
            }
        }

        /// <summary>
        /// Returns the XML-RPC client configuration section information.
        /// </summary>
        /// <returns>
        ///     A <see cref="XmlRpcClientSection"/> object that represents the XML-RPC client configuration information.
        ///     If no configuration section is defined for the client application, returns a <b>null</b> reference.
        /// </returns>
        [SecurityPermission(SecurityAction.Demand)]
        internal static XmlRpcClientSection GetXmlRpcClientSection()
        {
            string sectionPath  = "argotic.net/clientSettings/xmlRpc";

            lock (PrivilegedConfigurationManager.XmlRpcSyncObject)
            {
                XmlRpcClientSection section = PrivilegedConfigurationManager.GetSection(sectionPath) as XmlRpcClientSection;
                if (section == null)
                {
                    return null;
                }
                return section;
            }
        }
    }
}
