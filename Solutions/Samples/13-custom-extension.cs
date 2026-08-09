#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 13 -- Your own namespace
//
//     dotnet run --file Solutions/Samples/13-custom-extension.cs
//
// The twenty-seven extensions this library ships cover the namespaces that got popular. Yours did
// not, because you invented it this morning -- an internal feed that has to carry a reading time,
// a review status, a cost centre, something no standard models and none ever will.
//
// RSS was built for exactly this. An element in a namespace other than the default is somebody
// else's business, so publishing your own vocabulary alongside the standard one is not a hack; it
// is the mechanism working as designed. What you need from the library is a way to make your
// elements first-class: written on save, attached on load, reachable through FindExtension like
// any other.
//
// That takes one class, and the class is mostly boilerplate. The interesting part is the last
// section: discovery finds the framework's extensions automatically and cannot possibly find
// yours, for three separate reasons, each of which fails silently.
// ---------------------------------------------------------------------------------------------

using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

// ---------------------------------------------------------------------------------------------
// 1. Writing needs no registration at all
//
// Attach the extension, save, and the elements and the namespace declaration are both there. This
// is the AutoDetectExtensions machinery from sample 10, and it works for your type for the same
// reason it works for iTunes: on save it walks the object graph and asks what is actually
// attached. It never consults a registry, so there is no registry for your type to be missing
// from.
// ---------------------------------------------------------------------------------------------

RssFeed feed = new();
feed.Channel.Title = "endjin blog";
feed.Channel.Link = new Uri("https://endjin.com/blog/");
feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

RssItem post = new()
{
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
    Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
    Description = "A new instrument in an orchestra that already existed.",
    PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
};

EditorialSyndicationExtension editorial = new();
editorial.Context.ReadingMinutes = 9;
editorial.Context.ReviewedBy = "Howard van Rooijen";
post.Extensions.Add(editorial);

feed.Channel.Items.Add(post);

string document = Save(feed);

Heading("Saving, with nothing registered");
Console.WriteLine($"  root element   {RootElement(document)}");
Console.WriteLine($"  elements       {Extract(document, "<endjin:")}");

// ---------------------------------------------------------------------------------------------
// 2. Reading is where it stops working, and here is why
//
// SyndicationExtensionAdapter finds the framework's extensions by reflecting over
// Assembly.GetExecutingAssembly().GetExportedTypes() and keeping everything assignable to
// SyndicationExtension. Read that sentence with your own type in mind and three separate
// exclusions fall out, any one of which is enough:
//
//   1. GetExecutingAssembly is Argotic.Extensions. Your type is in your assembly, and no amount
//      of correctness on your part will put it in theirs. This is the one that applies here.
//   2. GetExportedTypes returns public types only. Marking an extension internal removes it from
//      discovery even inside Argotic.Extensions itself.
//   3. The filter is "assignable to SyndicationExtension", the abstract base class -- not to
//      ISyndicationExtension, the interface. Implementing the interface directly is not enough.
//
// None of the three produce a diagnostic. You get a feed that loads cleanly with your elements
// sitting in the document and nothing attached to the item, which is indistinguishable from a
// feed that never carried them. The first run below is that outcome.
//
// The fix is one line, and it is the only line: put your type in
// SyndicationResourceLoadSettings.SupportedExtensions. Those are merged on top of the
// namespace-filtered framework set rather than replacing it, so registering yours does not cost
// you Dublin Core.
// ---------------------------------------------------------------------------------------------

Heading("Loading, with and without registration");

RssFeed unregistered = new();
unregistered.Load(SyndicationEncodingUtility.CreateSafeNavigator(document));
Report("no SupportedExtensions", unregistered);

SyndicationResourceLoadSettings settings = new();
settings.SupportedExtensions.Add(typeof(EditorialSyndicationExtension));

RssFeed registered = new();
registered.Load(SyndicationEncodingUtility.CreateSafeNavigator(document), settings);
Report("registered", registered);

Console.WriteLine();
Console.WriteLine("  The document is byte-identical in both runs. The only difference is one line of");
Console.WriteLine("  configuration, and without it the failure is silent.");

// The framework extensions still attach alongside yours -- SupportedExtensions adds, it does not
// replace. Worth proving, because "will registering mine break the built-in ones?" is the first
// question anybody asks.
const string Mixed = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0"
         xmlns:dc="http://purl.org/dc/elements/1.1/"
         xmlns:endjin="https://endjin.com/ns/editorial/1.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin.</description>
        <item>
          <title>The GenAI Reality Check</title>
          <link>https://endjin.com/blog/1</link>
          <description>An article.</description>
          <dc:creator>Barry Smart</dc:creator>
          <endjin:readingMinutes>9</endjin:readingMinutes>
        </item>
      </channel>
    </rss>
    """;

RssFeed mixed = new();
mixed.Load(SyndicationEncodingUtility.CreateSafeNavigator(Mixed), settings);

Heading("Yours alongside theirs");
Console.WriteLine($"  attached to the item: {string.Join(", ", mixed.Channel.Items[0].Extensions.Select(extension => extension.XmlPrefix))}");

Console.WriteLine();
Console.WriteLine("Next: 14-fetching.cs -- the first sample that opens a socket, and it opens it to itself.");

static string Save(RssFeed feed)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });
    stream.Position = 0;

    using StreamReader reader = new(stream);

    return reader.ReadToEnd();
}

static void Report(string label, RssFeed feed)
{
    RssItem item = feed.Channel.Items[0];
    string detail = item.FindExtension(EditorialSyndicationExtension.MatchByType) is EditorialSyndicationExtension found
        ? $"{found.Context.ReadingMinutes} minutes, reviewed by {found.Context.ReviewedBy}"
        : "(nothing -- the elements are in the document and were not turned into objects)";

    Console.WriteLine($"  {label,-24} {item.Extensions.Count} attached   {detail}");
}

static string RootElement(string document)
{
    int start = document.IndexOf("<rss", StringComparison.Ordinal);

    return document[start..(document.IndexOf('>', start) + 1)];
}

static string Extract(string document, string prefix)
{
    List<string> names = [];
    for (int at = document.IndexOf(prefix, StringComparison.Ordinal); at >= 0; at = document.IndexOf(prefix, at + 1, StringComparison.Ordinal))
    {
        int end = document.IndexOfAny([' ', '>', '/'], at);
        names.Add(document[at..end].TrimStart('<'));
    }

    return string.Join(", ", names);
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

// ---------------------------------------------------------------------------------------------
// 3. The class itself
//
// Three members are abstract on SyndicationExtension and you must supply all three: the two Load
// overloads and WriteTo. Everything else -- the prefix, the namespace, the version, the namespace
// manager, the ExistsInSource gate that decides whether Load is even called -- comes from the
// base, and you configure it by what you pass to the base constructor.
//
// Two details are worth copying rather than reinventing.
//
// CreateNamespaceManager, from the base, is what you build your XPath against. Do not construct
// an XmlNamespaceManager yourself and bind your prefix to your namespace: the base deliberately
// follows the *document's* binding for your prefix when there is one, which is what makes a feed
// declaring your namespace under a slightly different URI still readable. Sample 11's truth table
// is that behaviour seen from the outside.
//
// MatchByType is a convention rather than a requirement. Every framework extension exposes one so
// that callers write FindExtension(Thing.MatchByType) instead of a lambda, and yours should too,
// for no reason other than that it makes your extension look like the others.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Editorial metadata endjin wants on its own feed and no standard models: how long a post takes
/// to read, and who reviewed it.
/// </summary>
public sealed class EditorialSyndicationExtension : SyndicationExtension
{
    public EditorialSyndicationExtension()
        : base(
            "endjin",
            "https://endjin.com/ns/editorial/1.0",
            new Version(1, 0),
            new Uri("https://endjin.com/ns/editorial/1.0"),
            "endjin Editorial Extension",
            "Reading time and review metadata for endjin-published content.")
    {
    }

    /// <summary>
    /// Gets the properties this extension carries. The Context convention is not enforced by the
    /// base class -- it is a habit worth keeping, because every framework extension has one and a
    /// reader who knows one knows them all.
    /// </summary>
    public EditorialContext Context { get; } = new();

    /// <summary>
    /// Supports <c>FindExtension(EditorialSyndicationExtension.MatchByType)</c>.
    /// </summary>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        return extension is EditorialSyndicationExtension;
    }

    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The source could not produce a navigator.", nameof(source));

        // From the base, and it matters: this follows the document's binding for "endjin" rather
        // than asserting this extension's own namespace URI.
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        bool loaded = false;

        if (navigator.SelectSingleNode("endjin:readingMinutes", manager) is { } minutes
            && int.TryParse(minutes.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int parsed))
        {
            this.Context.ReadingMinutes = parsed;
            loaded = true;
        }

        if (navigator.SelectSingleNode("endjin:reviewedBy", manager) is { } reviewer)
        {
            this.Context.ReviewedBy = reviewer.Value;
            loaded = true;
        }

        // The event exists so that a caller can watch extensions attach during a large load. Raise
        // it whether or not anything was found; "nothing was found" is information too.
        this.OnExtensionLoaded(new SyndicationExtensionLoadedEventArgs(source, this));

        return loaded;
    }

    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        // The XmlReader overload exists for callers who have a reader rather than a navigator.
        // Materialising it is honest here: this extension's elements are a handful of leaves, and
        // a hand-written forward-only parser would be more code and more ways to be wrong.
        return this.Load(new XPathDocument(reader));
    }

    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // Write the elements and nothing else. Do not write the xmlns declaration here -- the base
        // class has WriteXmlNamespaceDeclaration for that, and the save pass calls it once on the
        // document element. Declaring it again here would bind the prefix a second time, on every
        // item that carries the extension.
        if (this.Context.ReadingMinutes is { } minutes)
        {
            writer.WriteElementString(
                this.XmlPrefix,
                "readingMinutes",
                this.XmlNamespace,
                minutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (this.Context.ReviewedBy.Length > 0)
        {
            writer.WriteElementString(this.XmlPrefix, "reviewedBy", this.XmlNamespace, this.Context.ReviewedBy);
        }
    }
}

/// <summary>
/// The properties <see cref="EditorialSyndicationExtension"/> carries.
/// </summary>
public sealed class EditorialContext
{
    /// <summary>
    /// Gets or sets how many minutes the post takes to read, or <see langword="null"/> if unstated.
    /// </summary>
    public int? ReadingMinutes { get; set; }

    /// <summary>
    /// Gets or sets who reviewed the post. The default is an empty string, never null.
    /// </summary>
    public string ReviewedBy
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;
}