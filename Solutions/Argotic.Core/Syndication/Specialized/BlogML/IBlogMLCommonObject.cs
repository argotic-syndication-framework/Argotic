﻿namespace Argotic.Syndication.Specialized
{
    using System;

    /// <summary>
    /// Allows an object to implement common Web Log Markup Language (BlogML) entity information by representing a set of properties, methods, indexers and events common to BlogML syndication resources.
    /// </summary>
    /// <seealso cref="Argotic.Syndication.Specialized.BlogMLAuthor"/>
    public interface IBlogMLCommonObject
    {
        /// <summary>
        /// Gets or sets the approval status of the web log entity.
        /// </summary>
        /// <value>
        ///     An <see cref="BlogMLApprovalStatus"/> enumeration value that represents whether the web log entity was approved to be publicly available.
        ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified.
        /// </value>
        BlogMLApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets a date-time indicating when the web log entity was created.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates an instant in time associated with an event early in the life cycle of the web log entity.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the web log entity.
        /// </summary>
        /// <value>An identification string for the web log entity. The default value is an <b>empty</b> string, which indicated that no identifier was specified.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets a date-time indicating when the web log entity was last modified.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when the web log entity was modified in a way the publisher considers significant.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        DateTime LastModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the title of the web log entity.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the title of the web log entity.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        BlogMLTextConstruct Title { get; set; }
    }
}