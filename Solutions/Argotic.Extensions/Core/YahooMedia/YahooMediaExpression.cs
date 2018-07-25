﻿namespace Argotic.Extensions.Core
{
    using System;
    using Argotic.Common;

    /// <summary>
    /// Represents the expressed version of a media object.
    /// </summary>
    /// <seealso cref="YahooMediaTextConstruct"/>
    [Serializable]
    public enum YahooMediaExpression
    {
        /// <summary>
        /// No media expression specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None = 0,

        /// <summary>
        /// The media object represents the full version.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        Full = 1,

        /// <summary>
        /// he media object represents a continuous stream.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        Nonstop = 2,

        /// <summary>
        /// The media object represents a sample version.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        Sample = 3,
    }
}