#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 18 -- Discovering where to POST, and what a draft is
//
//     dotnet run --file Solutions/Samples/18-atom-publishing.cs
//
// Every sample so far has read feeds. The Atom Publishing Protocol, RFC 5023, is the other
// direction: an HTTP API for creating, editing and deleting the entries in a collection, defined
// on top of Atom rather than beside it. It is what the WordPress and Blogger APIs were replaced
// by, and what a headless CMS exposes when it wants to be scriptable.
//
// Two documents make it work, and both are read with the same Load/Save shape as everything else.
// The service document says what collections exist and what each will accept. The category
// document says which terms a collection allows. Between them a client that knows one URL can
// discover everything else, which is the entire point -- nothing has to be configured out of band.
//
// The third type here, AtomEntryResource, is where the interesting language problem lives, and it
// is a good illustration of why "just inherit from it" is sometimes not enough.
// ---------------------------------------------------------------------------------------------

using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Publishing;
using Argotic.Syndication;

// A service document: one workspace, two collections, each declaring what it accepts.
const string ServiceXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
      <workspace>
        <atom:title type="text">endjin</atom:title>
        <collection href="https://endjin.com/app/blog">
          <atom:title type="text">Blog posts</atom:title>
          <accept>application/atom+xml;type=entry</accept>
          <categories fixed="yes" scheme="https://endjin.com/what-we-do/">
            <atom:category term="dotnet" label=".NET" />
            <atom:category term="data" label="Data and Analytics" />
            <atom:category term="ai" label="AI" />
          </categories>
        </collection>
        <collection href="https://endjin.com/app/media">
          <atom:title type="text">Images and audio</atom:title>
          <accept>image/png</accept>
          <accept>image/jpeg</accept>
          <accept>audio/mpeg</accept>
        </collection>
      </workspace>
    </service>
    """;

using LoopbackHost host = LoopbackHost.Serving(ServiceXml, "application/atomsvc+xml");

// ---------------------------------------------------------------------------------------------
// 1. One URL in, the whole API out
//
// A service document is a list of workspaces, each a list of collections, each with an address, a
// title, the media types it accepts, and optionally the categories it permits. That is enough for
// a client to build its own UI without being told anything else: which collections to show, which
// will take an upload and which will take an entry, and what to put in a category picker.
//
// The accept elements matter more than they look. A collection that accepts
// application/atom+xml;type=entry takes Atom entries -- posts. One that accepts image/png takes
// binary uploads and creates media entries. A client that ignores the distinction will POST a
// JPEG to the blog collection and get a 415 it cannot explain.
// ---------------------------------------------------------------------------------------------

AtomServiceDocument service = await AtomServiceDocument.CreateAsync(host.Uri);

Heading("The service document");
foreach (AtomWorkspace workspace in service.Workspaces)
{
    Console.WriteLine($"  workspace: {workspace.Title?.Content}");

    foreach (AtomMemberResources collection in workspace.Collections)
    {
        Console.WriteLine($"    {collection.Title?.Content}");
        Console.WriteLine($"      POST to  {collection.Uri}");
        Console.WriteLine($"      accepts  {Describe(collection.Accepts.Select(range => range.MediaRange))}");
        Console.WriteLine($"      takes    {(collection.Accepts.Any(IsEntryRange) ? "Atom entries -- this is where posts go" : "binary uploads -- media entries")}");

        foreach (AtomCategoryDocument categories in collection.Categories)
        {
            string terms = Describe(categories.Categories.Select(category => category.Term));
            Console.WriteLine($"      categories {(categories.IsFixed ? "fixed" : "open")}, scheme {categories.Scheme}");
            Console.WriteLine($"        {terms}");
        }
    }
}

// A fixed category list is a contract: RFC 5023 says a client must not invent terms outside it,
// and a server is entitled to reject one that does. An open list -- fixed="no", or absent -- means
// the listed terms are suggestions.
AtomCategoryDocument? blogCategories = service.Workspaces[0].Collections[0].Categories.FirstOrDefault();

Heading("Fixed or open");
Console.WriteLine($"  IsFixed  {blogCategories?.IsFixed}");
Console.WriteLine(blogCategories?.IsFixed == true
    ? "  Only the listed terms may be used. Anything else is the client's bug, not the server's."
    : "  The listed terms are suggestions and a client may add its own.");

// ---------------------------------------------------------------------------------------------
// 2. AtomEntryResource, and why a `new` shadow was unavoidable
//
// A member of a collection is an Atom entry plus two things only the protocol knows: when the
// server last edited it (app:edited) and whether it is a draft (app:control/app:draft). So
// AtomEntryResource derives from AtomEntry and adds EditedOn and IsDraft.
//
// That inheritance creates a problem C# cannot solve cleanly, and the shape of the fix is worth
// recognising because it recurs. CreateAsync is static, and static methods cannot be overridden.
// So AtomEntryResource.CreateAsync(uri, settings) used to bind AtomEntry's inherited version,
// which returns an AtomEntry -- and quietly dropped EditedOn and IsDraft, the two members you
// chose the derived type in order to get. It compiled, it ran, it returned an object with the
// right title and the wrong type.
//
// The fix is `new` shadows, and they bring a second wrinkle. CreateAsync(Uri, CancellationToken)
// deliberately has *no* default on its token, because giving it one would make CreateAsync(uri)
// ambiguous against CreateAsync(Uri, settings = null, token = default) -- CS0121, at every call
// site. So the asymmetry you may notice in the signatures is load-bearing rather than an
// oversight.
// ---------------------------------------------------------------------------------------------

const string DraftXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
      <id>urn:uuid:8f2d4c1e-9b37-4a5e-8c12-1d3e5a7b9c02</id>
      <title type="text">Writing Effective Copilot Instructions</title>
      <updated>2026-08-07T00:00:00Z</updated>
      <app:edited>2026-08-07T11:32:00Z</app:edited>
      <app:control><app:draft>yes</app:draft></app:control>
      <link rel="edit" href="https://endjin.com/app/blog/8f2d4c1e" />
      <summary type="text">Modular skill files rather than one monolith.</summary>
    </entry>
    """;

using LoopbackHost entryHost = LoopbackHost.Serving(DraftXml, "application/atom+xml;type=entry");

AtomEntryResource member = await AtomEntryResource.CreateAsync(entryHost.Uri);

Heading("A member resource");
Console.WriteLine($"  static type   {member.GetType().Name}");
Console.WriteLine($"  title         {member.Title?.Content}");
Console.WriteLine($"  updated       {member.UpdatedOn:u}   (the author changed it)");
Console.WriteLine($"  edited        {member.EditedOn:u}   (the server touched it)");
Console.WriteLine($"  IsDraft       {member.IsDraft}");
Console.WriteLine($"  edit link     {member.Links.FirstOrDefault(link => link.Relation == "edit")?.Uri}");

// ---------------------------------------------------------------------------------------------
// 3. The distinction IsDraft cannot make
//
// IsDraft is a bool, so it has two states, and the protocol has three: draft, published, and no
// app:control element at all. RFC 5023 says the absence means published -- so folding it into
// false is correct behaviour and loses information anyway.
//
// Most of the time that does not matter. It matters when you are writing the server, or a
// round-tripping client, because publishing an entry back with an app:control element the author
// never had is a change you did not intend. When you need to tell them apart, read the extension
// directly: AtomPublishingControlSyndicationExtension is attached only when the element was
// present. Its absence is the third state.
// ---------------------------------------------------------------------------------------------

const string PublishedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <entry xmlns="http://www.w3.org/2005/Atom">
      <id>urn:uuid:60a76c80-d399-11d9-b93c-0003939e0af6</id>
      <title type="text">The GenAI Reality Check</title>
      <updated>2026-05-14T08:54:29Z</updated>
      <summary type="text">A new instrument in an orchestra that already existed.</summary>
    </entry>
    """;

using LoopbackHost plainHost = LoopbackHost.Serving(PublishedXml, "application/atom+xml;type=entry");
AtomEntryResource plain = await AtomEntryResource.CreateAsync(plainHost.Uri);

Heading("Three states, one bool");
Console.WriteLine($"  {"entry",-28} {"IsDraft",-9} app:control attached?");
Report("draft (app:draft = yes)", member);
Report("no app:control at all", plain);

Console.WriteLine();
Console.WriteLine("  The second reads false -- and so would an entry that said <app:draft>no</app:draft>.");
Console.WriteLine("  Only the extension's presence tells you which of the two you are holding.");

// ---------------------------------------------------------------------------------------------
// 4. Building one to POST
//
// A client creates an entry by POSTing an Atom entry document to a collection's href. The four-
// argument constructor takes exactly what a new member needs, and IsDraft is how you say "save
// this but do not publish it yet" -- which is the whole reason app:control exists.
//
// What you send is what Save writes. There is no separate serialisation for the protocol.
// ---------------------------------------------------------------------------------------------

AtomEntryResource fresh = new(
    new AtomId(new Uri("urn:uuid:3c9b1f47-6d2a-4e8b-91a5-7f0c2d5e8a13")),
    new AtomTextConstruct("Rx.NET v7.0 Released") { TextType = AtomTextConstructType.Text },
    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
    isDraft: true);

fresh.Summary = new AtomTextConstruct("What changed in Rx.NET v7.0, from the team that maintains it.")
{
    TextType = AtomTextConstructType.Text,
};

fresh.Categories.Add(new AtomCategory("dotnet")
{
    Scheme = new Uri("https://endjin.com/what-we-do/"),
    Label = ".NET",
});

using MemoryStream body = new();
fresh.Save(body, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

Heading($"What a client would POST to {service.Workspaces[0].Collections[0].Uri}");
Console.WriteLine($"  Content-Type: {AtomAcceptedMediaRange.AtomEntryMediaRange}");
Console.WriteLine($"  {body.Length:N0} bytes, draft = {fresh.IsDraft}");
Console.WriteLine($"  carries app:control  {Encoding.UTF8.GetString(body.ToArray()).Contains("draft", StringComparison.Ordinal)}");
Console.WriteLine();
Console.WriteLine("  The category term came from the server's fixed list, which is why reading the");
Console.WriteLine("  service document first is not optional.");

Console.WriteLine();
Console.WriteLine("Next: 19-pings-and-trackbacks.cs -- telling another server that you linked to it.");

static void Report(string label, AtomEntryResource entry)
{
    bool hasControl = entry.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) is not null;
    Console.WriteLine($"  {label,-28} {entry.IsDraft,-9} {(hasControl ? "yes" : "no -- the third state")}");
}

static bool IsEntryRange(AtomAcceptedMediaRange range) =>
    range.MediaRange.Contains("type=entry", StringComparison.OrdinalIgnoreCase);

static string Describe(IEnumerable<string> values)
{
    string joined = string.Join(", ", values);

    return joined.Length == 0 ? "(none stated)" : joined;
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>Serves one document on 127.0.0.1. See sample 14 for why each sample carries its own.</summary>
internal sealed class LoopbackHost : IDisposable
{
    private readonly HttpListener listener;

    private LoopbackHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    public Uri Uri { get; }

    public static LoopbackHost Serving(string body, string mediaType)
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        // CA2000 cannot see that ownership passes to the caller, which disposes with `using`.
#pragma warning disable CA2000
        LoopbackHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"));
#pragma warning restore CA2000
        _ = host.ServeAsync(Encoding.UTF8.GetBytes(body), mediaType);

        return host;
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync(byte[] bytes, string mediaType)
    {
        while (this.listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await this.listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using HttpListenerResponse response = context.Response;
            response.ContentType = mediaType;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
        }
    }
}