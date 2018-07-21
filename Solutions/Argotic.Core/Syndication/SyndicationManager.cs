/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/22/2008	brian.kuhn	Created SyndicationManager Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;

using Argotic.Common;
using Argotic.Configuration;
using Argotic.Configuration.Provider;

namespace Argotic.Syndication
{
    /// <summary>
    /// Manages application syndication resources. This class cannot be inherited.
    /// </summary>
    public static class SyndicationManager
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the default provider that is used to manage syndication resources.
        /// </summary>
        private static SyndicationResourceProvider syndicationManagerDefaultProvider;
        /// <summary>
        /// Private member to hold a collection of providers that have been configured to manage syndication resources.
        /// </summary>
        private static SyndicationResourceProviderCollection syndicationManagerProviders;
        /// <summary>
        /// Private member to hold a value indicating if the syndication manager has already been initialized.
        /// </summary>
        private static bool syndicationManagerInitialized;
        /// <summary>
        /// Private member to hold an exception that occurred while attempting to initialize the syndication manager.
        /// </summary>
        private static Exception syndicationManagerInitializationException;
        /// <summary>
        /// Private member to hold object used to synchronize locks when initializing the syndication manager.
        /// </summary>
        private static object syndicationManagerLock    = new object();
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region ApplicationName
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        /// <remarks>
        ///     The <see cref="ApplicationName"/> is used to identify users specific to an application. That is, the same syndication resource can exist in the data store 
        ///     for multiple applications that specify a different <see cref="ApplicationName"/>. This enables multiple applications to use the same data store to store resource 
        ///     information without running into duplicate syndication resource conflicts. Alternatively, multiple applications can use the same syndication resource data store 
        ///     by specifying the same <see cref="ApplicationName"/>. The <see cref="ApplicationName"/> can be set programmatically or declaratively in the configuration for the application.
        /// </remarks>
        public static string ApplicationName
        {
            get
            {
                return SyndicationManager.Provider.ApplicationName;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    SyndicationManager.Provider.ApplicationName = String.Empty;
                }
                else
                {
                    SyndicationManager.Provider.ApplicationName = value;
                }
            }
        }
        #endregion

        #region Provider
        /// <summary>
        /// Gets a reference to the default syndication resource provider for the application.
        /// </summary>
        /// <value>The default syndication resource provider for the application exposed using the <see cref="SyndicationResourceProvider"/> abstract base class.</value>
        /// <remarks>
        ///     The <see cref="Provider"/> property enables you to reference the default syndication resource provider for an application directly. 
        ///     This is commonly used to access custom members of the syndication resource provider that are not part of the <see cref="SyndicationResourceProvider"/> abstract base class.
        /// </remarks>
        public static SyndicationResourceProvider Provider
        {
            get
            {
                SyndicationManager.Initialize();
                return syndicationManagerDefaultProvider;
            }
        }
        #endregion

        #region Providers
        /// <summary>
        /// Gets a collection of the syndication resource providers for the application.
        /// </summary>
        /// <value>A <see cref="SyndicationResourceProviderCollection"/> of the syndication resource providers configured for the application.</value>
        /// <remarks>
        ///     The <see cref="Providers"/> property references all of the syndication resource providers enabled for an application, 
        ///     including providers added in the <i>Web.config</i> file for ASP.NET applications, the <i>App.config</i> for client applications, 
        ///     and the <i>Machine.config</i> file for all applications.
        /// </remarks>
        public static SyndicationResourceProviderCollection Providers
        {
            get
            {
                SyndicationManager.Initialize();
                return syndicationManagerProviders;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region CreateResource(ISyndicationResource resource, Object providerResourceKey)
        /// <summary>
        /// Adds a new syndication resource to the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource within the syndication data source.</param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to be created within the data source.</param>
        /// <returns>A <see cref="SyndicationResourceCreateStatus"/> enumeration value indicating whether the syndication resource was created successfully.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public static SyndicationResourceCreateStatus CreateResource(Object providerResourceKey, ISyndicationResource resource)
        {
            return SyndicationManager.Provider.CreateResource(providerResourceKey, resource);
        }
        #endregion

        #region DeleteResource(Object providerResourceKey)
        /// <summary>
        /// Removes a resource from the syndication data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier from the syndication data source for the resource to be removed.</param>
        /// <returns><b>true</b> if the syndication resource was successfully deleted; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool DeleteResource(Object providerResourceKey)
        {
            return SyndicationManager.Provider.DeleteResource(providerResourceKey);
        }
        #endregion

        #region GetResource(Object providerResourceKey)
        /// <summary>
        /// Gets resource information from the data source based on the unique identifier for the syndication resource.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier for the syndication resource to get information for.</param>
        /// <returns>An object that implements the <see cref="ISyndicationResource"/> interface populated with the specified resources's information from the data source.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        public static ISyndicationResource GetResource(Object providerResourceKey)
        {
            return SyndicationManager.Provider.GetResource(providerResourceKey);
        }
        #endregion

        #region GetResources(SyndicationContentFormat format)
        /// <summary>
        /// Gets a collection of all the resources in the data source that conform to the specified <see cref="SyndicationContentFormat"/>.
        /// </summary>
        /// <param name="format">A <see cref="SyndicationContentFormat"/> enumeration values that indicates the format of the resources to be returned.</param>
        /// <returns>A <see cref="Collection{T}"/> of all of the syndication resources contained in the data source that conform to the specified <see cref="SyndicationContentFormat"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         <see cref="GetResources(SyndicationContentFormat)"/> returns a list of all of the resources from the data source for the configured <see cref="ApplicationName"/> property. 
        ///         Syndication resources are returned in order of last time they were updated in the data source.
        ///     </para>
        /// </remarks>
        public static Collection<ISyndicationResource> GetResources(SyndicationContentFormat format)
        {
            return SyndicationManager.Provider.GetResources(format);
        }
        #endregion

        #region GetResources(int pageIndex, int pageSize, out int totalRecords)
        /// <summary>
        /// Gets a collection of all the resources in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched resources.</param>
        /// <returns>
        ///     A <see cref="Collection{T}"/> that contains a page of <see cref="ISyndicationResource"/> objects 
        ///     with a size of <paramref name="pageSize"/>, beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <see cref="GetResources(int, int, out int)"/> returns a list of all of the resources from the data source for the configured <see cref="ApplicationName"/> property. 
        ///         Syndication resources are returned in order of last time they were updated in the data source.
        ///     </para>
        ///     <para>
        ///         The results returned by <see cref="GetResources(int, int, out int)"/> are constrained by the <paramref name="pageIndex"/> and <paramref name="pageSize"/> parameters. 
        ///         The <paramref name="pageSize"/> parameter identifies the number of <see cref="ISyndicationResource"/> objects to return in the collection. 
        ///         The <paramref name="pageIndex"/> parameter identifies which page of results to return, where 0 identifies the first page. 
        ///         The <paramref name="totalRecords"/> parameter is an out parameter that is set to the total number of syndication resources in the data source.
        ///     </para>
        ///     <para>
        ///         For example, if there are 13 resources in the data source, and the <paramref name="pageIndex"/> value was 1 with a <paramref name="pageSize"/> of 5, 
        ///         then the <see cref="Collection{T}"/> would contain the sixth through the tenth resources returned. The <paramref name="totalRecords"/> parameter would be set to 13.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="pageIndex"/> is <i>less than</i> zero.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public static Collection<ISyndicationResource> GetResources(int pageIndex, int pageSize, out int totalRecords)
        {
            return SyndicationManager.Provider.GetResources(pageIndex, pageSize, out totalRecords);
        }
        #endregion

        #region UpdateResource(Object providerResourceKey, ISyndicationResource resource)
        /// <summary>
        /// Updates information about a syndication resource in the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier from the syndication data source for the resource to be updated.</param>
        /// <param name="resource">
        ///     An object that implements the <see cref="ISyndicationResource"/> interface that represents the updated information for the resource.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void UpdateResource(Object providerResourceKey, ISyndicationResource resource)
        {
            SyndicationManager.Provider.UpdateResource(providerResourceKey, resource);
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region Initialize()
        /// <summary>
        /// Initializes the <see cref="SyndicationManager"/> using the declaratively configured application settings.
        /// </summary>
        /// <seealso cref="SyndicationResourceSection"/>
        private static void Initialize()
        {
            //------------------------------------------------------------
            //	Verify manager has not already been initialized previously
            //------------------------------------------------------------
            if (syndicationManagerInitialized)
            {
                //------------------------------------------------------------
                //	Throw stored exception if initialization raised an exception
                //------------------------------------------------------------
                if (syndicationManagerInitializationException != null)
                {
                    throw syndicationManagerInitializationException;
                }
            }
            else
            {
                //------------------------------------------------------------
                //	Throw stored exception if initialization raised an exception
                //------------------------------------------------------------
                if (syndicationManagerInitializationException != null)
                {
                    throw syndicationManagerInitializationException;
                }

                lock (syndicationManagerLock)
                {
                    try
                    {
                        //------------------------------------------------------------
                        //	Extract configuration section from application configuration
                        //------------------------------------------------------------
                        SyndicationResourceSection section  = PrivilegedConfigurationManager.GetSyndicationResourceSection();

                        if (section != null)
                        {
                            //------------------------------------------------------------
                            //	Raise exception if no providers were specified
                            //------------------------------------------------------------
                            if (((section.DefaultProvider == null) || (section.Providers == null)) || (section.Providers.Count < 1))
                            {
                                throw new ProviderException(String.Format(null, "The syndication resource providers were not specified. Source configuration file was {0}", section.ElementInformation.Source));
                            }

                            //------------------------------------------------------------
                            //	Fill providers collection
                            //------------------------------------------------------------
                            syndicationManagerProviders = new SyndicationResourceProviderCollection();

                            if (HostingEnvironment.IsHosted)
                            {
                                ProvidersHelper.InstantiateProviders(section.Providers, syndicationManagerProviders, typeof(SyndicationResourceProvider));
                            }
                            else
                            {
                                foreach (ProviderSettings settings in section.Providers)
                                {
                                    Type settingsType   = Type.GetType(settings.Type, true, true);
                                    if (!typeof(SyndicationResourceProvider).IsAssignableFrom(settingsType))
                                    {
                                        throw new ArgumentException(String.Format(null, "The provider must implement type of {0}. Source configuration file was {1}", typeof(SyndicationResourceProvider).FullName, section.ElementInformation.Source));
                                    }

                                    SyndicationResourceProvider provider    = (SyndicationResourceProvider)Activator.CreateInstance(settingsType);
                                    NameValueCollection parameters          = settings.Parameters;
                                    NameValueCollection configuration       = new NameValueCollection(parameters.Count, StringComparer.Ordinal);

                                    foreach (string parameter in parameters)
                                    {
                                        configuration[parameter]    = parameters[parameter];
                                    }

                                    provider.Initialize(settings.Name, configuration);
                                    syndicationManagerProviders.Add(provider);
                                }
                            }

                            //------------------------------------------------------------
                            //	Set and validate default provider
                            //------------------------------------------------------------
                            syndicationManagerDefaultProvider   = syndicationManagerProviders[section.DefaultProvider];

                            if (syndicationManagerDefaultProvider == null)
                            {
                                throw new ConfigurationErrorsException(String.Format(null, "The default syndication resource provider was not found. Source configuration file was {0}", section.ElementInformation.Source), section.ElementInformation.Properties["defaultProvider"].Source, section.ElementInformation.Properties["defaultProvider"].LineNumber);
                            }

                            //------------------------------------------------------------
                            //	Prevent collection from being modified further
                            //------------------------------------------------------------
                            syndicationManagerProviders.SetReadOnly();
                        }
                    }
                    catch(Exception exception)
                    {
                        //------------------------------------------------------------
                        //	Store exception locally and then throw
                        //------------------------------------------------------------
                        syndicationManagerInitializationException   = exception;
                        throw;
                    }

                    //------------------------------------------------------------
                    //	Set initialization indicator flag
                    //------------------------------------------------------------
                    syndicationManagerInitialized   = true;
                }
            }
        }
        #endregion
    }
}
