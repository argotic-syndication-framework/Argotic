namespace Argotic.Benchmarks;

/// <summary>
/// Locates the real-world document corpus collected under <c>.endjin/perf-spike/corpus/</c>.
/// </summary>
/// <remarks>
/// <para>
/// 136 documents, 76.2 MiB, fetched byte-exact from live publishers and <c>web.archive.org</c>:
/// RSS 0.90 through 2.0, RDF, Atom 0.3 and 1.0, OPML 1.0/1.1/2.0, sitemaps, RSD, APML, BlogML and
/// AtomPub, plus a feed for each of the twenty extension families.
/// </para>
/// <para>
/// <b>It lives outside the repository on purpose.</b> 76 MiB of third-party documents is not
/// something to commit, and <c>.endjin/</c> is gitignored, so these benchmarks are opt-in: they
/// measure what the synthetic generators structurally cannot — real encodings, byte-order marks,
/// unusual date spellings, partial extension usage, and publishers who declare a namespace they never
/// use.
/// </para>
/// <para>
/// When the corpus is absent, <see cref="Directory"/> throws rather than returning an empty set. A
/// benchmark that silently measures zero documents reports a very fast time and a very small
/// allocation, and both are lies; the whole point of this class is that the failure is loud.
/// </para>
/// </remarks>
internal static class RealWorldCorpus
{
    private const string RelativeRoot = ".endjin/perf-spike/corpus";

    /// <summary>
    /// Gets the corpus root directory.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">The corpus has not been collected on this machine.</exception>
    public static string Directory
    {
        get
        {
            DirectoryInfo? candidate = new(AppContext.BaseDirectory);

            while (candidate is not null)
            {
                string corpus = Path.Combine(candidate.FullName, RelativeRoot);
                if (System.IO.Directory.Exists(corpus))
                {
                    return corpus;
                }

                candidate = candidate.Parent;
            }

            throw new DirectoryNotFoundException(
                $"The real-world corpus was not found in any ancestor of '{AppContext.BaseDirectory}' at '{RelativeRoot}'. " +
                "It is gitignored and machine-local; re-collect it before running the realworld benchmarks.");
        }
    }

    /// <summary>
    /// Reads every document under a corpus subdirectory, exactly as it sits on disk.
    /// </summary>
    /// <param name="relativePath">The subdirectory, e.g. <c>rss20</c> or <c>extensions/itunes</c>.</param>
    /// <returns>The documents' bytes, ordered by file name so a run is reproducible.</returns>
    /// <exception cref="DirectoryNotFoundException">The subdirectory does not exist.</exception>
    /// <exception cref="InvalidOperationException">The subdirectory contains no documents.</exception>
    public static byte[][] ReadAll(string relativePath)
    {
        string directory = Path.Combine(Directory, relativePath);

        if (!System.IO.Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The corpus has no '{relativePath}' subdirectory.");
        }

        // Ordered, because an unordered enumeration makes one run's numbers incomparable with the
        // next when documents differ in size by three orders of magnitude, as these do.
        string[] files = [.. System.IO.Directory
            .EnumerateFiles(directory)
            .Where(static path => Path.GetExtension(path) is not (".md" or ".tsv" or ".cs"))
            .OrderBy(static path => path, StringComparer.Ordinal)];

        return files.Length == 0
            ? throw new InvalidOperationException($"The corpus subdirectory '{relativePath}' is empty; a benchmark over it would measure nothing.")
            : [.. files.Select(File.ReadAllBytes)];
    }

    /// <summary>
    /// Reads a single named corpus document.
    /// </summary>
    /// <param name="relativePath">The path within the corpus, e.g. <c>sitemap/wikihow-urlset-50000.xml</c>.</param>
    /// <returns>The document's bytes, byte-order mark and all.</returns>
    /// <exception cref="FileNotFoundException">The document is not present.</exception>
    public static byte[] Read(string relativePath)
    {
        string path = Path.Combine(Directory, relativePath);

        return File.Exists(path)
            ? File.ReadAllBytes(path)
            : throw new FileNotFoundException($"The corpus document '{relativePath}' is not present.", path);
    }
}