#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 20 -- Three formats you will meet once
//
//     dotnet run --file Solutions/Samples/20-the-long-tail.cs
//
// RSS, Atom, OPML and sitemaps are the four you will use. This library reads three more, and they
// are worth twenty minutes not because you will need them often but because of what they
// demonstrate: the API shape from sample 01 was not an RSS convention. It is the shape.
//
// BlogML exports a whole blog for migration. APML describes what a person pays attention to. RSD
// tells a client which publishing API a site speaks. They have nothing in common -- different
// authors, different decades, different problems -- and all three expose exactly the same
// CreateAsync, Load, Save surface as RssFeed.
//
// So this sample is deliberately shaped as a demonstration of sameness. One generic function
// round-trips all three, and the sections after it cover the one thing about each that is not
// obvious from the type names.
// ---------------------------------------------------------------------------------------------

using System.Globalization;

using Argotic.Common;
using Argotic.Syndication;

const string BlogMLXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="https://endjin.com/blog/"
          date-created="2026-08-07T12:00:00Z">
      <title type="text">endjin blog</title>
      <sub-title type="text">Technical writing from endjin.</sub-title>
      <authors>
        <author id="barry" date-created="2020-01-01T00:00:00Z" approved="true" email="hello@endjin.com">
          <title type="text">Barry Smart</title>
        </author>
        <author id="spam" date-created="2026-01-01T00:00:00Z" approved="false" email="nobody@example.invalid">
          <title type="text">Not A Real Author</title>
        </author>
      </authors>
      <categories>
        <category id="ai" date-created="2020-01-01T00:00:00Z" approved="true">
          <title type="text">AI</title>
        </category>
      </categories>
      <posts>
        <post id="1" date-created="2026-05-14T08:54:29Z" approved="true" post-url="https://endjin.com/blog/1">
          <title type="text">The GenAI Reality Check</title>
          <content type="html">&lt;p&gt;A new instrument.&lt;/p&gt;</content>
          <authors><author ref="barry" /></authors>
          <categories><category ref="ai" /></categories>
        </post>
        <post id="2" date-created="2026-06-01T00:00:00Z" approved="false" post-url="https://endjin.com/blog/2">
          <title type="text">A draft nobody published</title>
          <content type="html">&lt;p&gt;Unfinished.&lt;/p&gt;</content>
        </post>
      </posts>
    </blog>
    """;

const string ApmlXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <APML xmlns="http://www.apml.org/apml-0.6" version="0.6" defaultprofile="work">
      <Head>
        <Title>endjin attention profile</Title>
        <Generator>Argotic</Generator>
        <DateCreated>2026-08-07T12:00:00Z</DateCreated>
      </Head>
      <Body defaultprofile="work">
        <Profile name="work">
          <ImplicitData>
            <Concepts>
              <Concept key="dotnet" value="0.95" from="endjin" updated="2026-08-07T12:00:00Z" />
              <Concept key="data-engineering" value="0.82" from="endjin" updated="2026-08-07T12:00:00Z" />
            </Concepts>
          </ImplicitData>
          <ExplicitData>
            <Concepts>
              <Concept key="reactive-programming" value="0.9" />
            </Concepts>
          </ExplicitData>
        </Profile>
      </Body>
    </APML>
    """;

const string RsdXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
      <service>
        <engineName>endjin publishing</engineName>
        <engineLink>https://endjin.com/</engineLink>
        <homePageLink>https://endjin.com/blog/</homePageLink>
        <apis>
          <api name="Atom" preferred="true" apiLink="https://endjin.com/app/service" blogID="endjin" />
          <api name="MetaWeblog" preferred="false" apiLink="https://endjin.com/xmlrpc" blogID="endjin" />
        </apis>
      </service>
    </rsd>
    """;

// ---------------------------------------------------------------------------------------------
// 1. Eleven document types, one shape
//
// The function at the foot of this file is constrained to ISyndicationResource with a public
// parameterless constructor, and that constraint is all it needs to load, save and reload any of
// them. It never mentions BlogML, APML or RSD. It would work just as well on RssFeed, AtomFeed,
// Sitemap or AtomServiceDocument -- and on a format added next year, without being recompiled
// against it.
//
// That is the practical value of the uniform surface, and it is why the absences noted in earlier
// samples matter: because there is no Save(path) on any of them, a helper like this does not have
// to special-case one type that has it. Uniformity is only useful when it has no exceptions.
// ---------------------------------------------------------------------------------------------

Heading("The same code over three unrelated formats");
Console.WriteLine($"  {"format",-10} {"version",-8} {"in",-8} {"out",-8} reload equals the original");
RoundTrip<BlogMLDocument>("BlogML", BlogMLXml);
RoundTrip<ApmlDocument>("APML", ApmlXml);
RoundTrip<RsdDocument>("RSD", RsdXml);

// ---------------------------------------------------------------------------------------------
// 2. BlogML: the format that has to carry what a feed drops
//
// A feed is a window over the recent past. BlogML is an export, and an export has to survive
// being imported -- so it carries everything a blog holds that a feed deliberately omits:
// unapproved authors, unapproved posts, category hierarchy, attachments, comment threads, and
// engine-specific extended properties.
//
// The approval flags are the ones to get right, and they are the reason BlogMLApprovalStatus has
// three values rather than being a bool. None means the export said nothing. An importer that
// reads None as approved publishes a draft the author never published, under their name, on a
// site they have just migrated to -- which is about the worst outcome a migration tool has
// available to it.
//
// Note also that this is where SyndicationContentLengthLimits.Archive comes in: 64 MiB rather
// than a feed's 8, because the document is sized by the history of the site rather than by a
// window over it.
// ---------------------------------------------------------------------------------------------

BlogMLDocument blog = new();
blog.Load(SyndicationEncodingUtility.CreateSafeNavigator(BlogMLXml));

Heading("BlogML: approval is three-valued");
Console.WriteLine($"  {blog.Title.Content}, {blog.Posts.Count} posts, {blog.Authors.Count} authors");
Console.WriteLine();
Console.WriteLine($"  {"post",-34} {"approval",-10} an importer that assumes approved would");
foreach (BlogMLPost post in blog.Posts)
{
    string consequence = post.ApprovalStatus switch
    {
        BlogMLApprovalStatus.Approved => "publish it -- correct",
        BlogMLApprovalStatus.NotApproved => "publish a draft -- wrong",
        _ => "publish something nobody vouched for",
    };

    Console.WriteLine($"  {post.Title.Content,-34} {post.ApprovalStatus,-10} {consequence}");
}

Console.WriteLine();
Console.WriteLine($"  archive byte budget  {SyndicationContentLengthLimits.Archive / 1024 / 1024} MiB, against a feed's {SyndicationContentLengthLimits.Feed / 1024 / 1024}");

// ---------------------------------------------------------------------------------------------
// 3. APML: what somebody pays attention to
//
// APML is the odd one out and the only format here that describes a person rather than a
// publication. Each concept carries a key and a value between -1 and 1, and the split between
// implicit and explicit is the whole idea: implicit concepts were inferred from behaviour,
// explicit ones were stated by the person.
//
// Conflating them is the mistake the format exists to prevent. "You read a lot about Kubernetes"
// and "you told us you care about Kubernetes" are different claims with different reliability,
// and a recommender that treats them alike will confidently tell you about something you were
// only ever reading out of obligation.
//
// It never caught on. It is here because Argotic has always read it, and because the distinction
// it draws is one every recommendation system re-invents badly.
// ---------------------------------------------------------------------------------------------

ApmlDocument attention = new();
attention.Load(SyndicationEncodingUtility.CreateSafeNavigator(ApmlXml));

Heading("APML: inferred against stated");
Console.WriteLine($"  {attention.Head.Title}, default profile {Quote(attention.DefaultProfileName)}");
foreach (ApmlProfile profile in attention.Profiles)
{
    Console.WriteLine($"  profile {profile.Name}");
    foreach (ApmlConcept concept in profile.ImplicitConcepts)
    {
        Console.WriteLine($"    implicit  {concept.Key,-22} {concept.Value?.ToString("0.00", CultureInfo.InvariantCulture) ?? "(unstated)"}  (inferred from behaviour)");
    }

    foreach (ApmlConcept concept in profile.ExplicitConcepts)
    {
        Console.WriteLine($"    explicit  {concept.Key,-22} {concept.Value?.ToString("0.00", CultureInfo.InvariantCulture) ?? "(unstated)"}  (the person said so)");
    }
}

// ---------------------------------------------------------------------------------------------
// 4. RSD: the format whose job is to point at another protocol
//
// RSD carries no content at all. It is one step of indirection: an editor knows a blog's URL,
// fetches the RSD document linked from its head, and learns which publishing APIs the site
// speaks and where they live. That is how sample 18's client finds the Atom service document
// without being told its address.
//
// The preferred flag is the only piece of judgement in it, and it is the site's rather than
// yours: it says which API the site would rather you used. Honour it, and fall back only when
// your client cannot speak the preferred one.
// ---------------------------------------------------------------------------------------------

RsdDocument discovery = new();
discovery.Load(SyndicationEncodingUtility.CreateSafeNavigator(RsdXml));

Heading("RSD: one step of indirection");
Console.WriteLine($"  engine     {discovery.EngineName} ({discovery.EngineLink})");
Console.WriteLine($"  homepage   {discovery.Homepage}");
foreach (RsdApplicationInterface api in discovery.Interfaces)
{
    Console.WriteLine($"  api        {api.Name,-14} {(api.IsPreferred ? "preferred" : "fallback ")}  {api.Link}");
}

RsdApplicationInterface? preferred = discovery.Interfaces.FirstOrDefault(api => api.IsPreferred);
Console.WriteLine();
Console.WriteLine($"  So a client starts at {preferred?.Link}, which is where sample 18 began.");

Console.WriteLine();
Console.WriteLine("Next: 21-a-polling-service.cs -- everything so far, wired together the way an application would.");

// The point of section 1: nothing below names a format.
static void RoundTrip<TResource>(string label, string xml)
    where TResource : ISyndicationResource, new()
{
    TResource original = new();
    original.Load(SyndicationEncodingUtility.CreateSafeNavigator(xml));

    byte[] saved = Save(original);

    TResource reloaded = new();
    using (MemoryStream stream = new(saved))
    {
        reloaded.Load(stream);
    }

    // Compare the two saved forms rather than the first document to its own re-save: the second
    // save onwards is a fixed point, which is the honest thing to assert. Sample 08 is why.
    bool stable = Save(reloaded).SequenceEqual(saved);

    Console.WriteLine($"  {label,-10} {original.Version,-8} {xml.Length,-8:N0} {saved.Length,-8:N0} {stable}");
}

static byte[] Save(ISyndicationResource resource)
{
    using MemoryStream stream = new();
    resource.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

    return stream.ToArray();
}

static string Quote(string value) => value.Length == 0 ? "(none)" : $"\"{value}\"";

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}