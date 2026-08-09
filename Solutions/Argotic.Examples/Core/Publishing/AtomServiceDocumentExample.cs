using System.Xml;

using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Publishing;

/// <summary>
/// Demonstrates <see cref="AtomServiceDocument"/>: the entry point of an Atom Publishing Protocol server.
/// </summary>
/// <remarks>
///     <para>
///     RFC 5023 section 8. A client that knows one URL — this one — can discover every collection a
///     server exposes, where to POST to each, and what each will accept. Nothing else in the protocol
///     has to be configured out of band, which is the whole point of the document existing.
///     </para>
///     <para>
///     endjin exposes no Publishing endpoint, so the <c>Uri</c> example serves its document from
///     loopback. That is not a workaround for the sake of the example: it also means this runs inside
///     <c>--skip-network</c>, where a build can see it.
///     </para>
/// </remarks>
internal static class AtomServiceDocumentExample
{
    /// <summary>
    /// Builds a service document describing two workspaces and prints it.
    /// </summary>
    public static void ClassExample()
    {
        AtomServiceDocument document = new();

        AtomWorkspace blog = new(new AtomTextConstruct("endjin blog"));

        AtomMemberResources posts = new(
            new Uri("https://endjin.com/app/blog/posts"),
            new AtomTextConstruct("Blog posts"));
        posts.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        AtomCategoryDocument tags = new(isFixed: true, new Uri("https://endjin.com/tags"));
        tags.Categories.Add(new AtomCategory("dotnet"));
        tags.Categories.Add(new AtomCategory("data"));
        tags.Categories.Add(new AtomCategory("ai"));
        posts.Categories.Add(tags);

        AtomMemberResources media = new(
            new Uri("https://endjin.com/app/blog/media"),
            new AtomTextConstruct("Post images"));
        media.Accepts.Add(new AtomAcceptedMediaRange("image/png"));
        media.Accepts.Add(new AtomAcceptedMediaRange("image/*"));

        blog.Collections.Add(posts);
        blog.Collections.Add(media);

        AtomWorkspace talks = new(new AtomTextConstruct("endjin talks"));
        AtomMemberResources talkEntries = new(
            new Uri("https://endjin.com/app/talks"),
            new AtomTextConstruct("Talks"));
        talkEntries.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));
        talks.Collections.Add(talkEntries);

        document.Workspaces.Add(blog);
        document.Workspaces.Add(talks);

        ExampleOutput.ShowAtomServiceDocument(document);
    }

    /// <summary>
    /// Loads a service document from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomServiceDocument document = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomServiceDocument);
        document.Load(stream);

        ExampleOutput.ShowAtomServiceDocument(document);
    }

    /// <summary>
    /// Loads a service document from an <see cref="XmlReader"/>.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        AtomServiceDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomServiceDocument);
        using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        document.Load(reader);

        ExampleOutput.ShowAtomServiceDocument(document);
    }

    /// <summary>
    /// Fetches a service document from a <see cref="Uri"/>, served from loopback.
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        string served = await File.ReadAllTextAsync(SampleDataPath.AtomServiceDocument.FullPath).ConfigureAwait(false);
        using LoopbackHost host = LoopbackHost.ServingXml(served, "application/atomsvc+xml");

        AtomServiceDocument document = await AtomServiceDocument.CreateAsync(host.Uri).ConfigureAwait(false);
        ExampleOutput.ShowAtomServiceDocument(document);
    }

    /// <summary>
    /// Saves a service document to a <see cref="Stream"/> and reads it back.
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomServiceDocument document = new();
        AtomWorkspace workspace = new(new AtomTextConstruct("endjin blog"));

        AtomMemberResources posts = new(
            new Uri("https://endjin.com/app/blog/posts"),
            new AtomTextConstruct("Blog posts"));
        posts.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        workspace.Collections.Add(posts);
        document.Workspaces.Add(workspace);

        using MemoryStream stream = new();
        document.Save(stream);
        ExampleOutput.ShowSaved("AtomServiceDocument");

        stream.Seek(0, SeekOrigin.Begin);
        AtomServiceDocument reloaded = new();
        reloaded.Load(stream);
        ExampleOutput.ShowAtomServiceDocument(reloaded);
    }
}