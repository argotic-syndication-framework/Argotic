/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/11/2007	brian.kuhn	Created ITunesExplicitMaterial Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the explicit language or adult content advisory information of an iTunes podcast.
    /// </summary>
    /// <seealso cref="ITunesSyndicationExtensionContext"/>
    [Serializable()]
    public enum ITunesExplicitMaterial
    {
        /// <summary>
        /// No explicit material designation specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// The podcast has no explicit language or adult content included anywhere in its episodes. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "Clean", AlternateValue = "clean")]
        Clean   = 1,

        /// <summary>
        /// The podcast explicit material advisory was not provided by the publisher.
        /// </summary>
        [EnumerationMetadata(DisplayName = "No", AlternateValue = "no")]
        No      = 2,

        /// <summary>
        /// The podcast has explicit language or adult content included in its episodes. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "Yes", AlternateValue = "yes")]
        Yes     = 3
    }
}