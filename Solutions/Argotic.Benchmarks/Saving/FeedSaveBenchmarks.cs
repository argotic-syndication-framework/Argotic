using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Saving;

/// <summary>
/// Measures serialising a populated feed back out, for RSS and Atom, across both save surfaces.
/// </summary>
/// <remarks>
/// <para>
/// Round-tripping is not a synthetic concern for this library: newsletter aggregation loads feeds
/// from many sources and re-emits a normalised feed, so save cost is paid on the same schedule as
/// load cost.
/// </para>
/// <para>
/// The save path also reaches <c>SyndicationExtensionAdapter</c>, via
/// <c>FillExtensionTypes(entity, settings.SupportedExtensions)</c> per entity. Unlike the load
/// path it reads each entity's own already-populated extensions rather than re-deriving the
/// framework's extension list by reflection, so it should NOT show the load path's per-entity
/// reflection cost. That is a prediction, and this class is what tests it — if save turns out to
/// scale like load did, the prediction was wrong and the defect is broader than currently
/// believed.
/// </para>
/// </remarks>
[BenchmarkCategory("save")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class FeedSaveBenchmarks
{
    private RssFeed rssFeed = new();
    private AtomFeed atomFeed = new();

    /// <summary>
    /// Gets or sets the number of items or entries in the feed under test.
    /// </summary>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Loads the feeds once, so the measurement is serialisation rather than parsing.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        using MemoryStream rssStream = new(FeedCorpus.GenerateRssUtf8(this.ItemCount), writable: false);
        this.rssFeed = new RssFeed();
        this.rssFeed.Load(rssStream);

        using MemoryStream atomStream = new(FeedCorpus.GenerateAtomUtf8(this.ItemCount), writable: false);
        this.atomFeed = new AtomFeed();
        this.atomFeed.Load(atomStream);
    }

    /// <summary>
    /// Serialises an RSS feed to a stream.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "RSS Save(Stream)")]
    public long SaveRssToStream()
    {
        using MemoryStream stream = new();
        this.rssFeed.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Serialises an RSS feed through an XmlWriter.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "RSS Save(XmlWriter)")]
    public long SaveRssToXmlWriter()
    {
        using MemoryStream stream = new();
        using (XmlWriter writer = XmlWriter.Create(stream))
        {
            this.rssFeed.Save(writer);
        }

        return stream.Length;
    }

    /// <summary>
    /// Serialises an Atom feed to a stream.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "Atom Save(Stream)")]
    public long SaveAtomToStream()
    {
        using MemoryStream stream = new();
        this.atomFeed.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Serialises an Atom feed through an XmlWriter.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "Atom Save(XmlWriter)")]
    public long SaveAtomToXmlWriter()
    {
        using MemoryStream stream = new();
        using (XmlWriter writer = XmlWriter.Create(stream))
        {
            this.atomFeed.Save(writer);
        }

        return stream.Length;
    }
}