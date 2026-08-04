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
        /// <param name="match">The predicate delegate that defines the conditions.</param>
        /// <returns>The first syndication extension that matches, or null if none found.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
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