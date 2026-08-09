using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Argotic.Common;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Measures the per-call <see cref="Regex"/> construction in <c>SyndicationDiscoveryUtility</c>.
/// </summary>
/// <remarks>
/// <para>
/// The audit that went looking for a <c>FrozenDictionary</c> rebuilt per call found none — every one
/// is <c>static readonly</c>. It found this instead. Six <c>new Regex(...)</c> were constructed as
/// local variables, so each paid the engine's pattern parse on every call, and one of them was
/// worse than per-call: <c>ExtractHtmlAttributes</c> built its pattern once per invocation and
/// <c>ExtractUrls</c> invokes it once per matched <c>&lt;link&gt;</c> and once per matched
/// <c>&lt;a&gt;</c>. A page with 50 links and 200 anchors constructed 250 of them. All six are now
/// <c>[GeneratedRegex]</c>, and this class is what measured the change.
/// </para>
/// <para>
/// The <c>Count</c> axis is the number of anchors on the page, which is what multiplies the
/// construction. It is a real axis rather than decoration: the arms diverge along it, because the
/// hoisted variants pay construction once whatever the page contains.
/// </para>
/// <para>
/// Three variants, and the middle one matters. Interpreted-per-call is what used to ship.
/// Interpreted-hoisted isolates how much of the cost is construction alone rather than
/// matching, which is the number that says whether source generation is doing anything beyond what a
/// <c>static readonly</c> field would — without that arm, a two-arm comparison would have credited
/// source generation with a win a one-line field declaration already delivers. Source-generated is
/// what ships now, and was already the house style for <c>XmlDeclarationEncodingRegex</c>.
/// </para>
/// </remarks>
[BenchmarkCategory("utilities", "discovery", "regex")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public partial class DiscoveryRegexBenchmarks
{
    private const string AttributePattern = """([a-zA-Z]+)=["']([^"']+)["']|([a-zA-Z]+)=([^"'>\r\n\t ]+)""";

    private static readonly Regex HoistedAttributePattern = new(AttributePattern, RegexOptions.IgnoreCase);

    private string page = string.Empty;
    private string[] tags = [];

    /// <summary>
    /// Gets or sets the number of anchor elements on the page.
    /// </summary>
    [Params(10, 100, 500)]
    public int AnchorCount { get; set; }

    /// <summary>
    /// Builds a page of the configured size, shaped like the HTML head-and-body a discovery call meets.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        System.Text.StringBuilder builder = new();
        builder.Append("<html><head>");
        builder.Append("""<link rel="alternate" type="application/rss+xml" title="Feed" href="https://example.com/feed.xml">""");
        builder.Append("""<link rel="alternate" type="application/atom+xml" title="Atom" href="https://example.com/feed.atom">""");
        builder.Append("</head><body>");

        List<string> elements = [];
        for (int i = 0; i < this.AnchorCount; i++)
        {
            string anchor = $"""<a href="https://example.com/page{i}" class="entry-link" rel="bookmark">Item {i}</a>""";
            builder.Append(anchor);
            elements.Add(anchor);
        }

        builder.Append("</body></html>");
        this.page = builder.ToString();
        this.tags = [.. elements];
    }

    /// <summary>
    /// The whole public path, as it ships — source-generated throughout, where it once constructed a
    /// pattern per matched element.
    /// </summary>
    /// <returns>The number of URLs extracted.</returns>
    [Benchmark(Baseline = true, Description = "ExtractUrls (public path, as shipped)")]
    public int ExtractUrlsAsShipped() => SyndicationDiscoveryUtility.ExtractUrls(this.page).Count;

    /// <summary>
    /// The attribute scan alone, constructing the pattern per call as the shipped code does.
    /// </summary>
    /// <returns>The number of attributes matched.</returns>
    [Benchmark(Description = "attribute scan, interpreted, constructed per call")]
    public int InterpretedPerCall()
    {
        int total = 0;
        foreach (string tag in this.tags)
        {
            Regex pattern = new(AttributePattern, RegexOptions.IgnoreCase);
            total += ReadAttributes(pattern, tag);
        }

        return total;
    }

    /// <summary>
    /// The same scan against a pattern constructed once, isolating construction from matching.
    /// </summary>
    /// <returns>The number of attributes matched.</returns>
    [Benchmark(Description = "attribute scan, interpreted, hoisted to a static")]
    public int InterpretedHoisted()
    {
        int total = 0;
        foreach (string tag in this.tags)
        {
            total += ReadAttributes(HoistedAttributePattern, tag);
        }

        return total;
    }

    /// <summary>
    /// The same scan against the source-generated matcher.
    /// </summary>
    /// <returns>The number of attributes matched.</returns>
    [Benchmark(Description = "attribute scan, source-generated")]
    public int SourceGenerated()
    {
        int total = 0;
        foreach (string tag in this.tags)
        {
            total += ReadAttributes(AttributeRegex(), tag);
        }

        return total;
    }

    // Mirrors what ExtractHtmlAttributes does with each match: reads the captured groups. Counting
    // the matches instead would skip the group access the real method pays for.
    private static int ReadAttributes(Regex pattern, string tag)
    {
        int total = 0;
        foreach (Match match in pattern.Matches(tag))
        {
            total += match.Groups[1].Value.Length + match.Groups[2].Value.Length;
        }

        return total;
    }

    [GeneratedRegex(AttributePattern, RegexOptions.IgnoreCase)]
    private static partial Regex AttributeRegex();
}