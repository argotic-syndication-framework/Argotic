namespace Argotic.Extensions;

/// <summary>
/// Extension methods for <see cref="IExtensibleSyndicationObject"/>.
/// </summary>
public static class ExtensibleSyndicationObjectExtensions
{
    extension(IExtensibleSyndicationObject obj)
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