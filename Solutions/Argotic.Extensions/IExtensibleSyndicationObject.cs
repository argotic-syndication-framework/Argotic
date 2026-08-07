namespace Argotic.Extensions;

/// <summary>
/// Defines generalized extension properties, methods, indexers and events that a value type or class
/// implements to create a type-specific implementation of extension properties, methods, indexers and events.
/// </summary>
/// <remarks>
///     Implemented by every entity that can carry namespaced foreign markup — a feed, a channel, an item,
///     an outline. <see cref="SyndicationExtensionAdapter.Fill(IExtensibleSyndicationObject)"/> populates
///     <see cref="Extensions"/> during a load, and
///     <see cref="SyndicationExtensionAdapter.WriteExtensionsTo(IEnumerable{ISyndicationExtension}, System.Xml.XmlWriter)"/>
///     writes it back out. To read one of them, use <c>FindExtension</c> with the extension's static
///     <c>MatchByType</c> predicate.
/// </remarks>
public interface IExtensibleSyndicationObject
{
    /// <summary>
    /// Gets the syndication extensions applied to the syndication entity.
    /// </summary>
    /// <value>The extensions, in the order the adapter attached them. The collection is mutable: adding an extension here is what makes it appear in the saved document.</value>
    /// <seealso cref="SyndicationExtension"/>
    IList<ISyndicationExtension> Extensions { get; }

    /// <summary>
    /// Gets a value indicating if the syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Extensions"/> contains at least one <see cref="ISyndicationExtension"/>; otherwise, <see langword="false"/>.</value>
    bool HasExtensions { get; }
}