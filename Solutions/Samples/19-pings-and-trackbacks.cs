#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 19 -- Telling another server that you linked to it
//
//     dotnet run --file Solutions/Samples/19-pings-and-trackbacks.cs
//
// Syndication is pull: a reader asks your server for your feed. The two protocols in this sample
// are the push half of the same world -- one server telling another that something happened.
// Trackback says "I have written about your post"; the XML-RPC ping says "my site has changed,
// come and look".
//
// Both are older than most of what has been covered so far, and both are still deployed, which
// makes them a good place to see two things this library does that nothing else in it does:
// construct a schemaless value tree, and treat an error that arrives with a 200 OK as an error.
//
// That second one is the part worth staying for. It is a failure mode you will meet again in
// other protocols, and the first time it costs a day.
// ---------------------------------------------------------------------------------------------

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Net;

// ---------------------------------------------------------------------------------------------
// 1. Trackback: form in, XML out
//
// Trackback is deliberately tiny. You POST four form-encoded fields to a URL the other site
// advertises, and it replies with a small XML document saying whether it accepted you. The
// asymmetry -- a form-encoded request and an XML response -- is the protocol's, not this
// library's, and it is why TrackbackMessage has WriteTo(StreamWriter) while TrackbackResponse has
// Load(XPathNavigator).
//
// The url field is the only required one, and it is your permalink, not the page you linked to.
// The receiver fetches it to check you really did link to them, which is the whole anti-spam
// design, and it is why the field is encoded rather than written raw: a permalink with an
// ordinary query string used to split into extra form fields and arrive truncated.
// ---------------------------------------------------------------------------------------------

TrackbackMessage message = new(new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"))
{
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
    WeblogName = "endjin blog",
    Excerpt = "Generative AI is a new instrument in an orchestra that already existed.",
};

using ProtocolHost accepting = ProtocolHost.Serving("""
    <?xml version="1.0" encoding="utf-8"?>
    <response><error>0</error></response>
    """);

// TrackbackClient and XmlRpcClient are not IDisposable. They are meant to be resolved from DI as
// typed clients -- see sample 21 -- so the HttpClient underneath is owned by the factory and its
// handler is rotated for you. Constructing one directly, as here, is the shape a sample needs and
// not the shape an application wants.
TrackbackClient trackback = new(accepting.Uri);
TrackbackResponse accepted = await trackback.SendAsync(message);

Heading("What actually went over the wire");
Console.WriteLine($"  {accepting.LastBody}");
Console.WriteLine();
Console.WriteLine("  Four fields, form-encoded, and the permalink encoded along with the rest.");

Heading("Accepted");
Console.WriteLine($"  HasError      {accepted.HasError}");
Console.WriteLine($"  ErrorMessage  {Absent(accepted.ErrorMessage)}");

// A rejection is the same shape with error = 1 and a message. Note the status code below: the
// receiver said 200 OK and still refused the trackback. HasError, not the HTTP status, is the
// answer to "did this work".
using ProtocolHost refusing = ProtocolHost.Serving("""
    <?xml version="1.0" encoding="utf-8"?>
    <response><error>1</error><message>The URL you provided does not link to this entry.</message></response>
    """);

TrackbackClient refusedClient = new(refusing.Uri);
TrackbackResponse refused = await refusedClient.SendAsync(message);

Heading("Refused, with a 200 OK");
Console.WriteLine($"  HTTP status   200 OK");
Console.WriteLine($"  HasError      {refused.HasError}");
Console.WriteLine($"  ErrorMessage  {refused.ErrorMessage}");

// ---------------------------------------------------------------------------------------------
// 2. XML-RPC: the value tree is the schema
//
// XML-RPC has no schema language and no types beyond six primitives, an array and a struct. There
// is nothing to generate a client from, so this library models the wire format directly:
// IXmlRpcValue with three implementations, which you compose into whatever shape the method you
// are calling expects.
//
// That is more typing than a generated client and it is the honest amount of typing for a
// protocol where the only specification of a method's parameters is prose on somebody's website.
// weblogUpdates.ping, below, takes two strings; extendedPing takes four. Nothing enforces either.
//
// MaxValueNestingDepth is 64 and exists because the parser is recursive and a response is
// attacker-controlled. Without a bound, a few kilobytes of nested <array> elements is a stack
// overflow, which cannot be caught and takes the process with it.
// ---------------------------------------------------------------------------------------------

XmlRpcMessage ping = new(
    "weblogUpdates.ping",
    [
        new XmlRpcScalarValue("endjin blog"),
        new XmlRpcScalarValue("https://endjin.com/blog/"),
    ]);

Heading("A ping");
Console.WriteLine($"  method          {ping.MethodName}");
Console.WriteLine($"  parameters      {ping.Parameters.Count}");
Console.WriteLine($"  nesting bound   {XmlRpcClient.MaxValueNestingDepth} levels");

using ProtocolHost rpcOk = ProtocolHost.Serving("""
    <?xml version="1.0" encoding="utf-8"?>
    <methodResponse>
      <params><param><value><struct>
        <member><name>flerror</name><value><boolean>0</boolean></value></member>
        <member><name>message</name><value><string>Thanks for the ping.</string></value></member>
      </struct></value></param></params>
    </methodResponse>
    """);

XmlRpcClient rpcClient = new(rpcOk.Uri);
XmlRpcResponse pinged = await rpcClient.SendAsync(ping);

Heading("The response, unpacked");
Describe(pinged);

// ---------------------------------------------------------------------------------------------
// 3. A fault is not an exception
//
// This is the section to remember. XML-RPC reports method-level failure in the response body, as
// a <fault> element, and the HTTP status is 200 OK -- the transport worked perfectly, and it did:
// your request arrived, was understood, and was refused.
//
// So XmlRpcResponse.Fault is populated and nothing throws. A caller that checks only the HTTP
// status, or only that SendAsync returned without an exception, records every ping as a success.
// You find out months later that a service has been rejecting you the whole time, because there
// was nothing to find out from.
//
//     Check Fault. Always. It is the return value, not the exception, that carries the verdict.
//
// Both responses below are 200 OK. Only one of them worked.
// ---------------------------------------------------------------------------------------------

using ProtocolHost rpcFault = ProtocolHost.Serving("""
    <?xml version="1.0" encoding="utf-8"?>
    <methodResponse>
      <fault><value><struct>
        <member><name>faultCode</name><value><int>4</int></value></member>
        <member><name>faultString</name><value><string>Too many pings from this address.</string></value></member>
      </struct></value></fault>
    </methodResponse>
    """);

XmlRpcClient faultClient = new(rpcFault.Uri);
XmlRpcResponse faulted = await faultClient.SendAsync(ping);

Heading("Two 200 OK responses");
Console.WriteLine($"  {"",-12} {"threw?",-8} {"Fault",-10} what a naive caller records");
Console.WriteLine($"  {"the ping",-12} {"no",-8} {(pinged.Fault is null ? "null" : "present"),-10} success   (correct)");
Console.WriteLine($"  {"the refusal",-12} {"no",-8} {(faulted.Fault is null ? "null" : "present"),-10} success   (wrong)");

if (faulted.Fault is { } fault)
{
    Console.WriteLine();
    Console.WriteLine("  What was actually said:");
    foreach (XmlRpcStructureMember member in fault.Members)
    {
        Console.WriteLine($"    {member.Name,-12} {Scalar(member.Value)}");
    }
}

// ---------------------------------------------------------------------------------------------
// 4. Finding out where to send it
//
// Neither protocol has a registry. A site that accepts trackbacks advertises the address in an
// RDF comment embedded in the page, which is as strange as it sounds and is what
// ExtractTrackbackNotificationServers reads. Pingback -- trackback's better-designed successor --
// advertises itself in an X-Pingback response header or a <link rel="pingback"> element.
//
// So the sequence for a real client is: fetch the page you linked to, look for an endpoint, and
// send only if you find one. SyndicationDiscoveryUtility has a method for each step, and the two
// Is...Enabled helpers exist so you can answer the question without parsing anything yourself.
// ---------------------------------------------------------------------------------------------

const string PageHtml = """
    <!doctype html>
    <html><head><title>A post that accepts trackbacks</title>
    <link rel="pingback" href="https://example.invalid/xmlrpc.php" />
    </head><body>
    <p>A post.</p>
    <!--
    <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
             xmlns:dc="http://purl.org/dc/elements/1.1/"
             xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/">
    <rdf:Description
        rdf:about="https://example.invalid/posts/1"
        dc:identifier="https://example.invalid/posts/1"
        dc:title="A post that accepts trackbacks"
        trackback:ping="https://example.invalid/trackback/1" />
    </rdf:RDF>
    -->
    </body></html>
    """;

IList<TrackbackDiscoveryMetadata> servers = SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(PageHtml);

Heading("Where to send it");
foreach (TrackbackDiscoveryMetadata server in servers)
{
    Console.WriteLine($"  title      {server.Title}");
    Console.WriteLine($"  about      {server.About}");
    Console.WriteLine($"  ping to    {server.PingUrl}");
}

HtmlAnchor? pingback = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(PageHtml);
string pingbackServer = pingback is { HRef.Length: > 0 } ? pingback.HRef : "(none advertised)";
Console.WriteLine($"  pingback   {pingbackServer}");

Console.WriteLine();
Console.WriteLine("  Discovery here is entirely local -- the page is a string in this file, and nothing");
Console.WriteLine("  above resolved example.invalid. The async overloads fetch; these two parse.");

Console.WriteLine();
Console.WriteLine("Next: 20-the-long-tail.cs -- three formats you will meet once, and one API you already know.");

static void Describe(XmlRpcResponse response)
{
    if (response.Fault is not null)
    {
        Console.WriteLine("  a fault -- see section 3");
        return;
    }

    if (response.Parameter is XmlRpcStructureValue structure)
    {
        foreach (XmlRpcStructureMember member in structure.Members)
        {
            Console.WriteLine($"  {member.Name,-12} {Scalar(member.Value)}");
        }

        return;
    }

    Console.WriteLine($"  {Scalar(response.Parameter)}");
}

static string Scalar(IXmlRpcValue? value) => value switch
{
    XmlRpcScalarValue scalar => $"{scalar.Value} ({scalar.ValueType})",
    XmlRpcArrayValue array => $"an array of {array.Values.Count}",
    XmlRpcStructureValue structure => $"a struct of {structure.Members.Count}",
    _ => "(nothing)",
};

static string Absent(string? value) => string.IsNullOrEmpty(value) ? "(none)" : value;

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// Answers every POST with one document, and remembers the request body -- which is how section 1
/// can show what was actually sent rather than assert it.
/// </summary>
internal sealed class ProtocolHost : IDisposable
{
    private readonly HttpListener listener;
    private readonly byte[] body;

    private ProtocolHost(HttpListener listener, Uri uri, byte[] body)
    {
        this.listener = listener;
        this.Uri = uri;
        this.body = body;
        _ = this.ServeAsync();
    }

    public Uri Uri { get; }

    public string? LastBody { get; private set; }

    public static ProtocolHost Serving(string response)
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        // CA2000 cannot see that ownership of the listener passes to the host, and of the host to
        // the caller, which disposes it with a `using`.
#pragma warning disable CA2000
        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        return new ProtocolHost(listener, new Uri($"http://127.0.0.1:{port}/"), Encoding.UTF8.GetBytes(response));
#pragma warning restore CA2000
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync()
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

            using (StreamReader reader = new(context.Request.InputStream, Encoding.UTF8))
            {
                this.LastBody = await reader.ReadToEndAsync();
            }

            using HttpListenerResponse response = context.Response;

            // 200 OK on every path, including the refusals. That is the point of sections 1 and 3.
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/xml";
            response.ContentLength64 = this.body.Length;
            await response.OutputStream.WriteAsync(this.body);
        }
    }
}