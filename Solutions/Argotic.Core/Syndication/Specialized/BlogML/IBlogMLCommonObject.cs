namespace Argotic.Syndication.Specialized;

/// <summary>
/// Allows an object to implement common Web Log Markup Language (BlogML) entity information by representing a set of properties, methods, indexers and events common to BlogML syndication resources.
/// </summary>
/// <remarks>
///     The four attributes and one element that BlogML puts on almost everything it stores — <c>id</c>,
///     <c>date-created</c>, <c>date-modified</c>, <c>approved</c> and <c>title</c>. Implemented by
///     <see cref="BlogMLPost"/>, <see cref="BlogMLComment"/>, <see cref="BlogMLTrackback"/>,
///     <see cref="BlogMLAuthor"/> and <see cref="BlogMLCategory"/>, which is what lets one pair of read and
///     write helpers serve all five.
/// </remarks>
/// <seealso cref="Argotic.Syndication.Specialized.BlogMLAuthor"/>
interface IBlogMLCommonObject
{
    /// <summary>
    /// Gets or sets the approval status of the web log entity.
    /// </summary>
    /// <value>
    ///     The <c>approved</c> attribute, read and written as <c>true</c> or <c>false</c>.
    ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified, and suppresses the attribute on save.
    /// </value>
    BlogMLApprovalStatus ApprovalStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a date-time indicating when the web log entity was created.
    /// </summary>
    /// <value>
    ///     The <c>date-created</c> attribute.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; BlogML dates are written as RFC 3339.
    /// </remarks>
    DateTime CreatedOn
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the web log entity.
    /// </summary>
    /// <value>The <c>id</c> attribute, or an <i>empty</i> string if none was specified. It is unique only within the document, and is the string other entities reference the entity by.</value>
    string Id
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a date-time indicating when the web log entity was last modified.
    /// </summary>
    /// <value>
    ///     The <c>date-modified</c> attribute — the last change the publisher considered significant.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; BlogML dates are written as RFC 3339.
    /// </remarks>
    DateTime LastModifiedOn
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the title of the web log entity.
    /// </summary>
    /// <value>The entity's <c>title</c> element. Implementations start it empty rather than null and reject null on assignment.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    BlogMLTextConstruct Title
    {
        get;
        set;
    }
}