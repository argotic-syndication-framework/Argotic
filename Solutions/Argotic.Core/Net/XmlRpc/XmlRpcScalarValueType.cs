/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcScalarValueType Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents the permissible data types for a <see cref="XmlRpcScalarValue"/>.
    /// </summary>
    /// <seealso cref="XmlRpcScalarValue.ValueType"/>
    /// <seealso cref="XmlRpcScalarValue"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    [Serializable()]
    public enum XmlRpcScalarValueType
    {
        /// <summary>
        /// No scalar parameter type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None        = 0,

        /// <summary>
        /// The parameter value represents binary data that has been encoded to its base64 representation.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Base64", AlternateValue = "base64")]
        Base64      = 1,

        /// <summary>
        /// The parameter value represents a logical boolean.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Boolean", AlternateValue = "boolean")]
        Boolean     = 2,

        /// <summary>
        /// The parameter value represents an instant in time, typically expressed as a date and time of day.
        /// </summary>
        [EnumerationMetadata(DisplayName = "DateTime", AlternateValue = "dateTime.iso8601")]
        DateTime    = 3,

        /// <summary>
        /// The parameter value represents a double-precision signed floating-point number.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Double", AlternateValue = "double")]
        Double      = 4,

        /// <summary>
        /// The parameter value represents a 32-bit signed integer.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Integer", AlternateValue = "int")]
        Integer     = 5,

        /// <summary>
        /// The parameter value represents text as a series of characters.
        /// </summary>
        [EnumerationMetadata(DisplayName = "String", AlternateValue = "string")]
        String      = 6
    }
}