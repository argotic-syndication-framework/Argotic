namespace Argotic.Configuration.Provider
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration.Provider;
    using System.Security.Permissions;
    using System.Web;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Provides a base implementation for the syndication resource extensible provider model.
    /// </summary>
    /// <seealso cref="Argotic.Common.ISyndicationResource"/>
    // [AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    // [AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public abstract class SyndicationResourceProvider : ProviderBase
    {
        /// <summary>
        /// Gets or sets the name of the application using the custom syndication resource provider.
        /// </summary>
        /// <value>The name of the application using the custom syndication resource provider.</value>
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
        ///     <para>
        ///         In your syndication resource provider implementation, you will need to ensure that your data schema includes the <see cref="ApplicationName"/>
        ///         and that data source queries and updates also include the <see cref="ApplicationName"/>.
        ///     </para>
        /// </remarks>
        public abstract string ApplicationName
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a new syndication resource to the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource within the syndication data source.</param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to be created within the data source.</param>
        /// <returns>A <see cref="SyndicationResourceCreateStatus"/> enumeration value indicating whether the syndication resource was created successfully.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public abstract SyndicationResourceCreateStatus CreateResource(object providerResourceKey, ISyndicationResource resource);

        /// <summary>
        /// Removes a resource from the syndication data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource to be removed.</param>
        /// <returns><b>true</b> if the syndication resource was successfully deleted; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        public abstract bool DeleteResource(object providerResourceKey);

        /// <summary>
        /// Gets resource information from the data source based on the unique identifier for the syndication resource.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the syndication resource to get information for.</param>
        /// <returns>An object that implements the <see cref="ISyndicationResource"/> interface populated with the specified resources's information from the data source.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        public abstract ISyndicationResource GetResource(object providerResourceKey);

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
        public abstract Collection<ISyndicationResource> GetResources(SyndicationContentFormat format);

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
        public abstract Collection<ISyndicationResource> GetResources(int pageIndex, int pageSize, out int totalRecords);

        /// <summary>
        /// Updates information about a syndication resource in the data source.
        /// </summary>
        /// <param name="providerResourceKey">The unique identifier that identifies the resource to be updated.</param>
        /// <param name="resource">
        ///     An object that implements the <see cref="ISyndicationResource"/> interface that represents the updated information for the resource.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="providerResourceKey"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public abstract void UpdateResource(object providerResourceKey, ISyndicationResource resource);
    }
}
