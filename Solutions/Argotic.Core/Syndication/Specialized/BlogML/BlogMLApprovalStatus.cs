﻿namespace Argotic.Syndication.Specialized
{
    using System;

    using Argotic.Common;

    /// <summary>
    /// Represents the approval status of a web log entity.
    /// </summary>
    [Serializable]
    public enum BlogMLApprovalStatus
    {
        /// <summary>
        /// No approval status specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None = 0,

        /// <summary>
        /// Indicates that the web log entity is approved.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Approved", AlternateValue = "true")]
        Approved = 1,

        /// <summary>
        /// Indicates that the web log entity is not approved.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Not Approved", AlternateValue = "false")]
        NotApproved = 2,
    }
}