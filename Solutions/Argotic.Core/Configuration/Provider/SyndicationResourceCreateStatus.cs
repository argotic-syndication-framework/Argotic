/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/22/2008	brian.kuhn	Created SyndicationResourceCreateStatus Enumeration
****************************************************************************/
using System;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Configuration.Provider
{
    /// <summary>
    /// Describes the result of a <see cref="SyndicationResourceProvider.CreateResource(Object, ISyndicationResource)">CreateResource</see> operation.
    /// </summary>
    public enum SyndicationResourceCreateStatus
    {
        /// <summary>
        /// No syndication resource creation status was specified.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "")]
        None                            = 0,

        /// <summary>
        /// The provider resource key already exists in the data store for the application.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Duplicate Provider Resource Key")]
        DuplicateProviderResourceKey    = 1,

        /// <summary>
        /// The provider does not support the <see cref="SyndicationContentFormat"/> that the syndication resource conforms to.
        /// </summary>
        /// <seealso cref="ISyndicationResource.Format"/>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Invalid Format")]
        InvalidFormat                   = 2,

        /// <summary>
        /// The provider does not support the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that the syndication resource conforms to or the <see cref="Version"/> was a <b>null</b> reference.
        /// </summary>
        /// <seealso cref="ISyndicationResource.Version"/>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Invalid Format Version")]
        InvalidFormatVersion            = 3,

        /// <summary>
        /// The provider resource key is of an invalid type or format.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Invalid Provider Resource Key")]
        InvalidProviderResourceKey      = 4,

        /// <summary>
        /// The XML data source that describes the syndication resource was a <b>null</b> reference or does not conform to the provided <see cref="SyndicationContentFormat">format</see>.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Invalid Source")]
        InvalidSource                   = 5,

        /// <summary>
        /// The provider returned an error that is not described by other <see cref="SyndicationResourceCreateStatus"/> enumeration values.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Provider Error")]
        ProviderError                   = 6,

        /// <summary>
        /// The syndication resource was not created, for a reason defined by the provider.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Resource Rejected")]
        ResourceRejected                = 7,

        /// <summary>
        /// The syndication resource was successfully created.
        /// </summary>
        [EnumerationMetadata(AlternateValue = "", DisplayName = "Success")]
        Success                         = 8
    }
}
