namespace Argotic.Extensions;

/// <summary>
/// Extension methods for <see cref="IExtensibleSyndicationObject"/>.
/// </summary>
public static class ExtensibleSyndicationObjectExtensions
{
    // CA1034 predates C# 14 extension members and fires on the type the compiler generates for the
    // block below. The empty type name in its message gives it away - there is no nested type here
    // to make non-visible.
#pragma warning disable CA1034
    extension(IExtensibleSyndicationObject obj)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Searches for a syndication extension that matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">
        ///     The predicate that defines the conditions. Each framework extension exposes a static
        ///     <c>MatchByType</c> suitable for this parameter, which is how an extension is normally
        ///     retrieved: <c>item.FindExtension(ITunesSyndicationExtension.MatchByType)</c>.
        /// </param>
        /// <returns>The first matching extension in <see cref="IExtensibleSyndicationObject.Extensions"/>, or <see langword="null"/> if none matches.</returns>
        /// <remarks>
        ///     A linear scan of <see cref="IExtensibleSyndicationObject.Extensions"/>; there is no index
        ///     by type. Reading several properties off one extension should call this once and hold the
        ///     result, not call it per property.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is <see langword="null"/>.</exception>
        public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
        {
            ArgumentNullException.ThrowIfNull(match);
            foreach (ISyndicationExtension extension in obj.Extensions)
            {
                if (match(extension))
                {
                    return extension;
                }
            }

            return null;
        }
    }
}