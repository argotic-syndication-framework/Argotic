namespace Argotic.Extensions;

/// <summary>
/// Defines generalized extension properties, methods, indexers and events that a value type or class
/// implements to create a type-specific implementation of extension properties, methods, indexers and events.
/// </summary>
public interface IExtensibleSyndicationObject
{
    /// <summary>
    /// Gets the syndication extensions applied to the syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to the syndication entity.</value>
    /// <seealso cref="SyndicationExtension"/>
    IList<ISyndicationExtension> Extensions { get; }

    /// <summary>
    /// Gets a value indicating if the syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    bool HasExtensions { get; }
}