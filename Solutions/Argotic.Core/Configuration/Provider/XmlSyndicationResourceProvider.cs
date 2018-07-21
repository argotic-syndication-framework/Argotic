/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/22/2008	brian.kuhn	Created XmlSyndicationResourceProvider Class
07/01/2008  brian.kuhn  Implemented fix for work item 10410.
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.IO;
using System.Security.Permissions;
using System.Web;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

namespace Argotic.Configuration.Provider
{
    /// <summary>
    /// Manages storage of syndication resource information for applications in an XML file data store.
    /// </summary>
    //[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //[AspNetHostingPermissionAttribute(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class XmlSyndicationResourceProvider : SyndicationResourceProvider
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the name of the application using the syndication resource provider.
        /// </summary>
        private string providerApplicationName      = String.Empty;
        /// <summary>
        /// Private member to hold the path to the directory XML data files will be stored in.
        /// </summary>
        private string providerDirectoryPath        = String.Empty;
        /// <summary>
        /// Private member to hold the object used to synchronize modifications to the provider data store.
        /// </summary>
        private static object providerSyncObject    = new object();
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region ApplicationName
        /// <summary>
        /// Gets or sets the name of the application using the XML syndication resource provider.
        /// </summary>
        /// <value>The name of the application using the XML syndication resource provider.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="ApplicationName"/> property value is stored in the data source with related 
        ///         syndication resource information to associate a resource with a particular application.
        ///     </para>
        ///     <para>
        ///         Because syndication resource providers store resource information uniquely for each application, 
        ///         multiple applications can use the same data source without running into a conflict if duplicate syndication resources are created. 
        ///         Alternatively, multiple applications can use the same syndication resource data source by specifying the same <see cref="ApplicationName"/>.
        ///     </para>
        /// </remarks>
        public override string ApplicationName
        {
            get
            {
                return providerApplicationName;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    providerApplicationName = String.Empty;
                }
                else
                {
                    providerApplicationName = value.Trim();
                }
            }
        }
        #endregion

        #region Path
        /// <summary>
        /// Gets or sets the path to the data storage directory.
        /// </summary>
        /// <value>The path to the XML data storage directory.</value>
        /// <remarks>
        ///     The path will be mapped to the physical file path that corresponds to the virtual path on the Web server if the <paramref name="value"/> 
        ///     starts with <i>~</i> and <see cref="System.Web.HttpContext.Current"/> is not a <b>null</b> reference, otherwise a physical file path is assumed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Path
        {
            get
            {
                return providerDirectoryPath;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                providerDirectoryPath   = value.Trim();
            }
        }
        #endregion

        //============================================================
        //	PRIVATE PROPERTIES
        //============================================================
        #region ApplicationPath
        /// <summary>
        /// Gets the physical path to the application data storage directory.
        /// </summary>
        /// <value>The physical file path to the application XML data storage directory specifed by the <see cref="Path"/> and <see cref="ApplicationName"/>.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Path"/> is mapped to the physical file path that corresponds to the virtual path on the Web server if the <paramref name="value"/> 
        ///         starts with <i>~</i> and <see cref="System.Web.HttpContext.Current"/> is not a <b>null</b> reference, otherwise a physical file path is assumed.
        ///     </para>
        ///     <para>
        ///         The <see cref="ApplicationName"/>, if specified, is encoded using <see cref="SyndicationEncodingUtility.EncodeSafeDirectoryName(string)"/>. 
        ///         When an application name is specified for the provider, a sub-directory for the <see cref="ApplicationName"/> is created to store syndication 
        ///         resources that are segmented by application.
        ///     </para>
        /// </remarks>
        private string ApplicationPath
        {
            get
            {
                string applicationPath  = this.Path;

                if (HttpContext.Current == null)
                {
                    applicationPath             = applicationPath.Replace("/", "\\");
                    string baseDirectoryName    = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory).TrimEnd("\\".ToCharArray());

                    if (applicationPath.StartsWith("~", StringComparison.Ordinal))
                    {
                        applicationPath = String.Format(null, "{0}\\{1}", baseDirectoryName, applicationPath.Substring(1).TrimStart("\\".ToCharArray()));
                    }
                    else
                    {
                        applicationPath = String.Format(null, "{0}\\{1}", baseDirectoryName, applicationPath.TrimStart("\\".ToCharArray()));
                    }
                }
                else
                {
                    if (applicationPath.StartsWith("~", StringComparison.Ordinal))
                    {
                        applicationPath = HttpContext.Current.Server.MapPath(applicationPath);
                    }
                }

                if (!String.IsNullOrEmpty(this.ApplicationName))
                {
                    applicationPath     = String.Format(null, "{0}\\{1}", applicationPath.TrimEnd("\\".ToCharArray()), SyndicationEncodingUtility.EncodeSafeDirectoryName(this.ApplicationName));
                }

                return applicationPath;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region BuildResource(SyndicationContentFormat format, Stream stream)
        /// <summary>
        /// Instantiates a <see cref="ISyndicationResource"/> that conforms to the specified <see cref="SyndicationContentFormat"/> using the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
        /// <param name="format">A <see cref="SyndicationContentFormat"/> enumeration value that indicates the type syndication resource the <paramref name="stream"/> represents.</param>
        /// <returns>
        ///     An <see cref="ISyndicationResource"/> object that conforms to the specified <paramref name="format"/>, initialized using the supplied <paramref name="stream"/>. 
        ///     If the <paramref name="format"/> is not supported by the provider, returns a <b>null</b> reference.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        private static ISyndicationResource BuildResource(SyndicationContentFormat format, Stream stream)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(stream, "stream");

            //------------------------------------------------------------
            //	Create syndication resource based on content format
            //------------------------------------------------------------
            if (format == SyndicationContentFormat.Apml)
            {
                ApmlDocument document   = new ApmlDocument();
                document.Load(stream);
                return document;
            }
            else if (format == SyndicationContentFormat.Atom)
            {
                XPathDocument document      = new XPathDocument(stream);
                XPathNavigator navigator    = document.CreateNavigator();
                navigator.MoveToRoot();
                navigator.MoveToChild(XPathNodeType.Element);

                if(String.Compare(navigator.LocalName, "entry", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AtomEntry entry     = new AtomEntry();
                    entry.Load(navigator);
                    return entry;
                }
                else if (String.Compare(navigator.LocalName, "feed", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AtomFeed feed       = new AtomFeed();
                    feed.Load(navigator);
                    return feed;
                }
                else
                {
                    return null;
                }
            }
            else if (format == SyndicationContentFormat.BlogML)
            {
                BlogMLDocument document = new BlogMLDocument();
                document.Load(stream);
                return document;
            }
            else if (format == SyndicationContentFormat.Opml)
            {
                OpmlDocument document   = new OpmlDocument();
                document.Load(stream);
                return document;
            }
            else if (format == SyndicationContentFormat.Rsd)
            {
                RsdDocument document    = new RsdDocument();
                document.Load(stream);
                return document;
            }
            else if (format == SyndicationContentFormat.Rss)
            {
                RssFeed feed            = new RssFeed();
                feed.Load(stream);
                return feed;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ContentFormatAsFileExtension(SyndicationContentFormat format)
        /// <summary>
        /// Returns the file extension for the supplied <see cref="SyndicationContentFormat"/>.
        /// </summary>
        /// <param name="format">The <see cref="SyndicationContentFormat"/> to get the file extension for.</param>
        /// <returns>The file extension for the supplied <paramref name="format"/>, otherwise returns the <b>.xml</b> file extension.</returns>
        private static string ContentFormatAsFileExtension(SyndicationContentFormat format)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string fileExtension    = ".xml";

            //------------------------------------------------------------
            //	Return file extension based on content format
            //------------------------------------------------------------
            switch (format)
            {
                case SyndicationContentFormat.Apml:
                    fileExtension   = ".apml";
                    break;

                case SyndicationContentFormat.Atom:
                    fileExtension   = ".atom";
                    break;

                case SyndicationContentFormat.BlogML:
                    fileExtension   = ".blogML";
                    break;

                case SyndicationContentFormat.MicroSummaryGenerator:
                    fileExtension   = ".microSummary";
                    break;

                case SyndicationContentFormat.NewsML:
                    fileExtension   = ".newsML";
                    break;

                case SyndicationContentFormat.OpenSearchDescription:
                    fileExtension   = ".openSearch";
                    break;

                case SyndicationContentFormat.Opml:
                    fileExtension   = ".opml";
                    break;

                case SyndicationContentFormat.Rsd:
                    fileExtension   = ".rsd";
                    break;

                case SyndicationContentFormat.Rss:
                    fileExtension   = ".rss";
                    break;
            }

            return fileExtension;
        }
        #endregion

        #region ContentFormatByFileExtension(string fileExtension)
        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension of the content format.</param>
        /// <returns>A <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified string, otherwise returns <b>SyndicationContentFormat.None</b>.</returns>
        /// <remarks>This method disregards case of specified file extension.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="fileExtension"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="fileExtension"/> is an empty string.</exception>
        private static SyndicationContentFormat ContentFormatByFileExtension(string fileExtension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            SyndicationContentFormat format = SyndicationContentFormat.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(fileExtension, "fileExtension");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            if (String.Compare(fileExtension, ".apml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.Apml;
            }
            else if (String.Compare(fileExtension, ".atom", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.Atom;
            }
            else if (String.Compare(fileExtension, ".blogML", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.BlogML;
            }
            else if (String.Compare(fileExtension, ".microSummary", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.MicroSummaryGenerator;
            }
            else if (String.Compare(fileExtension, ".newsML", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.NewsML;
            }
            else if (String.Compare(fileExtension, ".openSearch", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.OpenSearchDescription;
            }
            else if (String.Compare(fileExtension, ".opml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.Opml;
            }
            else if (String.Compare(fileExtension, ".rsd", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.Rsd;
            }
            else if (String.Compare(fileExtension, ".rss", StringComparison.OrdinalIgnoreCase) == 0)
            {
                format  = SyndicationContentFormat.Rss;
            }

            return format;
        }
        #endregion

        #region TryParseGuid(string value, out Guid result)
        /// <summary>
        /// Converts the specified string representation of a globally unique identifier to its <see cref="Guid"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a globally unique identifier to convert.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="Guid"/> value equivalent to the globally unique identifier contained in <paramref name="value"/>, if the conversion succeeded, 
        ///     or <see cref="Guid.Empty"/> if the conversion failed. The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, 
        ///     or does not contain a valid string representation of a globally unique identifier. This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
        private static bool TryParseGuid(string value, out Guid result)
        {
            if (String.IsNullOrEmpty(value))
            {
                result  = Guid.Empty;
                return false;
            }
            else
            {
                try
                {
                    result  = new Guid(value);
                    return true;
                }
                catch (FormatException formatException)
                {
                    System.Diagnostics.Trace.TraceError(formatException.Message);
                    result  = Guid.Empty;
                    return false;
                }
                catch (OverflowException overflowException)
                {
                    System.Diagnostics.Trace.TraceError(overflowException.Message);
                    result  = Guid.Empty;
                    return false;
                }
            }
        }
        #endregion

        //============================================================
        //	UTILITY METHODS
        //============================================================
        #region BuildResourcePath(Guid resourceKey, SyndicationContentFormat format)
        /// <summary>
        /// Builds the physical file path for a syndication resource XML data file using the supplied <see cref="Guid"/> and <see cref="SyndicationContentFormat"/>.
        /// </summary>
        /// <param name="resourceKey">The globally unique identifier for the syndication resource that is used to as the file name.</param>
        /// <param name="format">The <see cref="SyndicationContentFormat"/> enumeration value that determines the file extension that is used.</param>
        /// <returns>The physical file path for a syndication resource XML data file using the supplied <see cref="Guid"/> and <see cref="SyndicationContentFormat"/>.</returns>
        /// <remarks>
        ///     If the physical file path for a syndication resource XML data file defines a directory path that does not exist, 
        ///     the <see cref="BuildResourcePath(Guid, SyndicationContentFormat)"/> method will attempt to create the directory.
        /// </remarks>
        private string BuildResourcePath(Guid resourceKey, SyndicationContentFormat format)
        {
            string applicationDirectory     = this.ApplicationPath.TrimEnd("\\".ToCharArray());
            string resourceFileExtension    = XmlSyndicationResourceProvider.ContentFormatAsFileExtension(format);

            string path = String.Format(null, "{0}\\{1}{2}", applicationDirectory, resourceKey.ToString(), resourceFileExtension);

            FileInfo pathInfo   = new FileInfo(path);
            if (!pathInfo.Directory.Exists)
            {
                pathInfo.Directory.Create();
            }

            return path;
        }
        #endregion

        #region ResourceKeyExists(Guid key)
        /// <summary>
        /// Returns a value indicating if a syndication resource exists for the supplied provider key.
        /// </summary>
        /// <param name="key">A <see cref="Guid"/> that represents the globally unique identifier for the resource.</param>
        /// <returns><b>true</b> if the syndication resource exists for the supplied provider key; otherwise, <b>false</b>.</returns>
        private bool ResourceKeyExists(Guid key)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool keyExists  = false;

            //------------------------------------------------------------
            //	Determine if resource file exists for specified key
            //------------------------------------------------------------
            DirectoryInfo applicationDirectory  = new DirectoryInfo(this.ApplicationPath);
            if (applicationDirectory.Exists)
            {
                FileInfo[] files = applicationDirectory.GetFiles(String.Format(null, "{0}.*", key.ToString()));
                if (files != null && files.Length == 1)
                {
                    return true;
                }
            }

            return keyExists;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region CreateResource(Object providerResourceKey, ISyndicationResource resource)
        /// <summary>
        /// Adds a new syndication resource to the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource within the syndication data source.</param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to be created within the data source.</param>
        /// <returns>A <see cref="SyndicationResourceCreateStatus"/> enumeration value indicating whether the syndication resource was created successfully.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public override SyndicationResourceCreateStatus CreateResource(Object providerResourceKey, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Guid resourceKey    = Guid.Empty;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(providerResourceKey, "providerResourceKey");
            Guard.ArgumentNotNull(resource, "resource");

            if (!XmlSyndicationResourceProvider.TryParseGuid(providerResourceKey.ToString(), out resourceKey))
            {
                System.Diagnostics.Trace.TraceError(String.Format(null, "The provider resource key of {0} does not represent a valid System.Guid structure.", providerResourceKey));
                return SyndicationResourceCreateStatus.InvalidProviderResourceKey;
            }

            //------------------------------------------------------------
            //	Add syndication resource to data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                return this.ResourceAdd(resourceKey, resource);
            }
        }
        #endregion

        #region DeleteResource(Object providerResourceKey)
        /// <summary>
        /// Removes a resource from the syndication data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource to be removed.</param>
        /// <returns><b>true</b> if the syndication resource was successfully deleted; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="providerResourceKey"/> does not represent a valid <see cref="System.Guid"/> structure.</exception>
        public override bool DeleteResource(Object providerResourceKey)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Guid resourceKey    = Guid.Empty;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(providerResourceKey, "providerResourceKey");

            if (!XmlSyndicationResourceProvider.TryParseGuid(providerResourceKey.ToString(), out resourceKey))
            {
                throw new ArgumentException(String.Format(null, "The provider resource key of {0} does not represent a valid System.Guid structure.", providerResourceKey), "providerResourceKey");
            }

            //------------------------------------------------------------
            //	Remove syndication resource from data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                return this.ResourceRemove(resourceKey);
            }
        }
        #endregion

        #region GetResource(Object providerResourceKey)
        /// <summary>
        /// Gets resource information from the data source based on the unique identifier for the syndication resource.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the syndication resource to get information for.</param>
        /// <returns>An object that implements the <see cref="ISyndicationResource"/> interface populated with the specified resources's information from the data source.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="providerResourceKey"/> does not represent a valid <see cref="System.Guid"/> structure.</exception>
        public override ISyndicationResource GetResource(Object providerResourceKey)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Guid resourceKey    = Guid.Empty;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(providerResourceKey, "providerResourceKey");

            if (!XmlSyndicationResourceProvider.TryParseGuid(providerResourceKey.ToString(), out resourceKey))
            {
                throw new ArgumentException(String.Format(null, "The provider resource key of {0} does not represent a valid System.Guid structure.", providerResourceKey), "providerResourceKey");
            }

            //------------------------------------------------------------
            //	Get syndication resource from data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                return this.ResourceGet(resourceKey);
            }
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
        /// <exception cref="ArgumentException">The <paramref name="format"/> is invalid.</exception>
        public override Collection<ISyndicationResource> GetResources(SyndicationContentFormat format)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            if (format == SyndicationContentFormat.None)
            {
                throw new ArgumentException(String.Format(null, "The the specified content format of {0} is invalid.", format), "format");
            }

            //------------------------------------------------------------
            //	Get syndication resources from data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                return this.ResourcesGet(format);
            }
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
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="pageSize"/> is <i>less than or equal to</i> zero.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public override Collection<ISyndicationResource> GetResources(int pageIndex, int pageSize, out int totalRecords)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotLessThan(pageIndex, "pageIndex", 0);
            Guard.ArgumentNotLessThan(pageSize, "pageSize", 1);
            
            //------------------------------------------------------------
            //	Get syndication resources from data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                return this.ResourcesGet(pageIndex, pageSize, out totalRecords);
            }
        }
        #endregion

        #region UpdateResource(Object providerResourceKey, ISyndicationResource resource)
        /// <summary>
        /// Updates information about a syndication resource in the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource to be updated.</param>
        /// <param name="resource">
        ///     An object that implements the <see cref="ISyndicationResource"/> interface that represents the updated information for the resource.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="providerResourceKey"/> does not represent a valid <see cref="System.Guid"/> structure.</exception>
        public override void UpdateResource(Object providerResourceKey, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Guid resourceKey    = Guid.Empty;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(providerResourceKey, "providerResourceKey");
            Guard.ArgumentNotNull(resource, "resource");

            if (!XmlSyndicationResourceProvider.TryParseGuid(providerResourceKey.ToString(), out resourceKey))
            {
                throw new ArgumentException(String.Format(null, "The provider resource key of {0} does not represent a valid System.Guid structure.", providerResourceKey), "providerResourceKey");
            }

            //------------------------------------------------------------
            //	Update syndication resource within data store
            //------------------------------------------------------------
            lock (providerSyncObject)
            {
                this.ResourceUpdate(resourceKey, resource);
            }
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region ResourceAdd(Guid resourceKey, ISyndicationResource resource)
        /// <summary>
        /// Adds the supplied <see cref="ISyndicationResource"/> to the data store using the specifed <see cref="Guid"/>.
        /// </summary>
        /// <param name="resourceKey">A <see cref="Guid"/> that represents the globally unique identifier for the <paramref name="resource"/>.</param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to be added to the data store.</param>
        /// <returns>A <see cref="SyndicationResourceCreateStatus"/> enumeration value that indicates the result of the adding the syndication resource to the data store.</returns>
        private SyndicationResourceCreateStatus ResourceAdd(Guid resourceKey, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Validate resource does not already exist
            //------------------------------------------------------------
            if (this.ResourceKeyExists(resourceKey))
            {
                return SyndicationResourceCreateStatus.DuplicateProviderResourceKey;
            }

            //------------------------------------------------------------
            //	Build path to XML data file for syndication resource
            //------------------------------------------------------------
            string resourceFilePath = this.BuildResourcePath(resourceKey, resource.Format);

            //------------------------------------------------------------
            //	Persist syndication resource
            //------------------------------------------------------------
            using (FileStream stream = new FileStream(resourceFilePath, FileMode.Create, FileAccess.Write))
            {
                SyndicationResourceSaveSettings settings    = new SyndicationResourceSaveSettings();
                settings.AutoDetectExtensions               = true;
                settings.MinimizeOutputSize                 = false;

                resource.Save(stream, settings);
            }

            return SyndicationResourceCreateStatus.Success;
        }
        #endregion

        #region ResourceGet(Guid resourceKey)
        /// <summary>
        /// Retrieves a <see cref="ISyndicationResource"/> from the data store that has the specified key.
        /// </summary>
        /// <param name="resourceKey">A <see cref="Guid"/> that represents the globally unique identifier for the resource to be retrieved.</param>
        /// <returns>
        ///     The <see cref="ISyndicationResource"/> that has the specified <paramref name="resourceKey"/>. 
        ///     If no resource exists for the specified <paramref name="resourceKey"/>, returns a <b>null</b> reference.
        /// </returns>
        private ISyndicationResource ResourceGet(Guid resourceKey)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            ISyndicationResource resource   = null;

            //------------------------------------------------------------
            //	Validate resource exists
            //------------------------------------------------------------
            if (!this.ResourceKeyExists(resourceKey))
            {
                return null;
            }

            //------------------------------------------------------------
            //	Load syndication resource from XML data source
            //------------------------------------------------------------
            DirectoryInfo applicationDirectory  = new DirectoryInfo(this.ApplicationPath);
            if (applicationDirectory.Exists)
            {
                FileInfo[] files = applicationDirectory.GetFiles(String.Format(null, "{0}.*", resourceKey.ToString()));
                if (files != null && files.Length == 1 && files[0].Exists)
                {
                    using (FileStream stream = new FileStream(files[0].FullName, FileMode.Open, FileAccess.Read))
                    {
                        SyndicationContentFormat format = XmlSyndicationResourceProvider.ContentFormatByFileExtension(files[0].Extension);
                        if (format != SyndicationContentFormat.None)
                        {
                            resource    = XmlSyndicationResourceProvider.BuildResource(format, stream);
                        }
                    }
                }
            }

            return resource;
        }
        #endregion

        #region ResourceRemove(Guid resourceKey)
        /// <summary>
        /// Removes a syndication resource from the data store that has the supplied key.
        /// </summary>
        /// <param name="resourceKey">A <see cref="Guid"/> that represents the globally unique identifier for the resource to be removed.</param>
        /// <returns>
        ///     <b>true</b> if the syndication resource was successfully removed; otherwise, <b>false</b>. 
        ///     If resource does not exist for specified <paramref name="resourceKey"/>, returns <b>false</b>.
        /// </returns>
        private bool ResourceRemove(Guid resourceKey)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool resourceRemoved    = false;

            //------------------------------------------------------------
            //	Validate resource exists
            //------------------------------------------------------------
            if (!this.ResourceKeyExists(resourceKey))
            {
                return false;
            }

            //------------------------------------------------------------
            //	Remove syndication resource XML data source
            //------------------------------------------------------------
            DirectoryInfo applicationDirectory  = new DirectoryInfo(this.ApplicationPath);
            if (applicationDirectory.Exists)
            {
                FileInfo[] files    = applicationDirectory.GetFiles(String.Format(null, "{0}.*", resourceKey.ToString()));
                if (files != null && files.Length == 1 && files[0].Exists)
                {
                    files[0].Delete();
                    resourceRemoved = true;
                }
            }

            return resourceRemoved;
        }
        #endregion

        #region ResourcesGet(SyndicationContentFormat format)
        /// <summary>
        /// Gets a collection of all the resources in the data source that conform to the specified <see cref="SyndicationContentFormat"/>.
        /// </summary>
        /// <param name="format">A <see cref="SyndicationContentFormat"/> enumeration value that indicates the content format to filter resources by.</param>
        /// <returns>
        ///     A <see cref="Collection{T}"/> of <see cref="ISyndicationResource"/> objects that represent the syndication resources that conform to the specified <paramref name="format"/>.
        /// </returns>
        private Collection<ISyndicationResource> ResourcesGet(SyndicationContentFormat format)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Collection<ISyndicationResource> resources  = new Collection<ISyndicationResource>();

            //------------------------------------------------------------
            //	Load filtered syndication resources from XML data store
            //------------------------------------------------------------
            DirectoryInfo applicationDirectory = new DirectoryInfo(this.ApplicationPath);
            if (applicationDirectory.Exists)
            {
                FileInfo[] files    = applicationDirectory.GetFiles();
                foreach(FileInfo file in files)
                {
                    if(file.Exists)
                    {
                        SyndicationContentFormat contentFormat  = XmlSyndicationResourceProvider.ContentFormatByFileExtension(file.Extension);
                        if (contentFormat == format)
                        {
                            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                            {
                                ISyndicationResource resource   = XmlSyndicationResourceProvider.BuildResource(contentFormat, stream);
                                if (resource != null)
                                {
                                    resources.Add(resource);
                                }
                            }
                        }
                    }
                }
            }

            return resources;
        }
        #endregion

        #region ResourcesGet(int pageIndex, int pageSize, out int totalRecords)
        /// <summary>
        /// Gets a collection of all the resources in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of syndication resources in the data store.</param>
        /// <returns>
        ///     A <see cref="Collection{T}"/> that contains a page of <see cref="ISyndicationResource"/> objects 
        ///     with a size of <paramref name="pageSize"/>, beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        private Collection<ISyndicationResource> ResourcesGet(int pageIndex, int pageSize, out int totalRecords)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Collection<ISyndicationResource> resources      = new Collection<ISyndicationResource>();
            Collection<ISyndicationResource> pagedResources = new Collection<ISyndicationResource>();

            //------------------------------------------------------------
            //	Load syndication resources from XML data store
            //------------------------------------------------------------
            DirectoryInfo applicationDirectory = new DirectoryInfo(this.ApplicationPath);
            if (applicationDirectory.Exists)
            {
                FileInfo[] files    = applicationDirectory.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Exists)
                    {
                        SyndicationContentFormat format = XmlSyndicationResourceProvider.ContentFormatByFileExtension(file.Extension);
                        if (format != SyndicationContentFormat.None)
                        {
                            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                            {
                                ISyndicationResource resource = XmlSyndicationResourceProvider.BuildResource(format, stream);
                                if (resource != null)
                                {
                                    resources.Add(resource);
                                }
                            }
                        }
                    }
                }
            }

            //------------------------------------------------------------
            //	Build paged syndication resources
            //------------------------------------------------------------
            totalRecords    = resources.Count;

            if (pageSize > resources.Count)
            {
                pagedResources  = new Collection<ISyndicationResource>(resources);
            }
            else
            {
                ISyndicationResource[] array = new ISyndicationResource[resources.Count];
                resources.CopyTo(array, 0);

                int startIndex  = (pageIndex == 0 ? 0 : (pageSize + pageIndex - 1));
                for (int i = startIndex; i < pageSize; i++)
                {
                    if(i > array.Length - 1)
                    {
                        break;
                    }
                    pagedResources.Add(array[i]);
                }
            }

            return pagedResources;
        }
        #endregion

        #region ResourceUpdate(Guid resourceKey, ISyndicationResource resource)
        /// <summary>
        /// Updates the supplied <see cref="ISyndicationResource"/> within the data store that has the specifed <see cref="Guid"/>.
        /// </summary>
        /// <param name="resourceKey">A <see cref="Guid"/> that represents the globally unique identifier of the resource to be updated.</param>
        /// <param name="resource">A <see cref="ISyndicationResource"/> object that represents the updated information for the resource to be updated.</param>
        /// <remarks>
        ///     If there is no resource for the specified <paramref name="resourceKey"/>, no modification to the data store occurs.
        /// </remarks>
        private void ResourceUpdate(Guid resourceKey, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Validate resource exists
            //------------------------------------------------------------
            if (!this.ResourceKeyExists(resourceKey))
            {
                return;
            }

            //------------------------------------------------------------
            //	Build path to XML data file for syndication resource
            //------------------------------------------------------------
            string resourceFilePath = this.BuildResourcePath(resourceKey, resource.Format);

            //------------------------------------------------------------
            //	Persist updated syndication resource information
            //------------------------------------------------------------
            if (File.Exists(resourceFilePath))
            {
                using (FileStream stream = new FileStream(resourceFilePath, FileMode.Create, FileAccess.Write))
                {
                    SyndicationResourceSaveSettings settings    = new SyndicationResourceSaveSettings();
                    settings.AutoDetectExtensions               = true;
                    settings.MinimizeOutputSize                 = false;

                    resource.Save(stream, settings);
                }
            }
        }
        #endregion

        //============================================================
        //	OVERRIDDEN METHODS
        //============================================================
        #region Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <remarks>
        ///     The base class implementation internally tracks the number of times the provider's <see cref="Initialize(string, NameValueCollection)">Initialize</see> method 
        ///     has been called. If a provider is initialized more than once, an <see cref="InvalidOperationException"/> is thrown stating that the provider is already initialized. 
        ///     Because most feature providers call <see cref="Initialize(string, NameValueCollection)">Initialize</see> prior to performing provider-specific initialization, 
        ///     this method is a central location for preventing double initialization.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> of the provider is <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="name"/> of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">
        ///     An attempt is made to call <see cref="Initialize(string, NameValueCollection)">Initialize</see> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            //------------------------------------------------------------
            //	Initialize state of provider base atributes
            //------------------------------------------------------------
            base.Initialize(name, config);

            //------------------------------------------------------------
            //	Initialize custom provider properties
            //------------------------------------------------------------
            string applicationName  = config["applicationName"];
            string path             = config["path"];

            if(String.IsNullOrEmpty(applicationName))
            {
                throw new ProviderException(String.Format(null, "The {0} expects the applicationName attribute to be configured. Please specify this attribute.", this.Name));
            }
            if (String.IsNullOrEmpty(path))
            {
                throw new ProviderException(String.Format(null, "The {0} expects the path attribute to be configured. Please specify this attribute.", this.Name));
            }

            this.ApplicationName    = applicationName;
            config.Remove("applicationName");

            this.Path               = path;
            config.Remove("path");

            //------------------------------------------------------------
            //	Validate that there are no unexpected attributes
            //------------------------------------------------------------
            if (config.Count > 0)
            {
                string extraAttribute   = config.GetKey(0);
                if (!String.IsNullOrEmpty(extraAttribute))
                {
                    throw new ProviderException(String.Format(null, "The following unrecognized attribute was found in {0}'s configuration: {1}", this.Name, extraAttribute));
                }
                else
                {
                    throw new ProviderException(String.Format(null, "An unrecognized attribute was found in the {0}'s configuration.", this.Name));
                }
            }
        }
        #endregion
    }
}
