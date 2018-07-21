/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaHashAlgorithm Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the hashing algorithm used to create a hash value.
    /// </summary>
    /// <seealso cref="YahooMediaHash"/>
    [Serializable()]
    public enum YahooMediaHashAlgorithm
    {
        /// <summary>
        /// No hashing algorithm was specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Indicates that the <b>Message-Digest 5</b> algorithm was used to generate the hash.
        /// </summary>
        [EnumerationMetadata(DisplayName = "MD5", AlternateValue = "md5")]
        MD5    = 1,

        /// <summary>
        /// Indicates that the <b>Secure Hash Algorithm 1</b> algorithm was used to generate the hash.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha")]
        [EnumerationMetadata(DisplayName = "SHA-1", AlternateValue = "sha-1")]
        Sha1    = 2
    }
}