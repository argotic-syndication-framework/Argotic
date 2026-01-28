using Spectre.IO;

namespace Argotic.Examples;

/// <summary>
/// Provides paths to sample data files for examples.
/// </summary>
public static class SampleDataPath
{
    private static readonly IFileSystem FileSystem = Spectre.IO.FileSystem.Shared;
    private static readonly DirectoryPath BasePath = new DirectoryPath(AppContext.BaseDirectory).Combine("SampleData");

    /// <summary>Gets the file system instance for file operations.</summary>
    public static IFileSystem Files => FileSystem;

    /// <summary>Gets the path to the sample RSS feed file.</summary>
    public static FilePath RssFeed => BasePath.CombineWithFilePath("RssFeed.xml");

    /// <summary>Gets the path to the sample Atom feed file.</summary>
    public static FilePath AtomFeed => BasePath.CombineWithFilePath("AtomFeed.xml");

    /// <summary>Gets the path to the sample Atom entry document file.</summary>
    public static FilePath AtomEntryDocument => BasePath.CombineWithFilePath("AtomEntryDocument.xml");

    /// <summary>Gets the path to the sample OPML document file.</summary>
    public static FilePath OpmlDocument => BasePath.CombineWithFilePath("OpmlDocument.xml");

    /// <summary>Gets the path to the sample APML document file.</summary>
    public static FilePath ApmlDocument => BasePath.CombineWithFilePath("ApmlDocument.xml");

    /// <summary>Gets the path to the sample BlogML document file.</summary>
    public static FilePath BlogMLDocument => BasePath.CombineWithFilePath("BlogMLDocument.xml");

    /// <summary>Gets the path to the sample RSD document file.</summary>
    public static FilePath RsdDocument => BasePath.CombineWithFilePath("RsdDocument.xml");

    /// <summary>Gets the path to the sample generic feed file.</summary>
    public static FilePath GenericFeed => BasePath.CombineWithFilePath("GenericFeed.xml");

    /// <summary>Gets the path to the sample RSS feed with extensions file.</summary>
    public static FilePath RssFeedWithExtensions => BasePath.CombineWithFilePath("RssFeedWithExtensions.xml");

    /// <summary>Gets the path to the sample Atom feed with extensions file.</summary>
    public static FilePath AtomFeedWithExtensions => BasePath.CombineWithFilePath("AtomFeedWithExtensions.xml");

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="path">The path to the file to open.</param>
    /// <returns>A stream for reading the file.</returns>
    public static Stream OpenRead(FilePath path)
    {
        return FileSystem.GetFile(path).OpenRead();
    }
}
