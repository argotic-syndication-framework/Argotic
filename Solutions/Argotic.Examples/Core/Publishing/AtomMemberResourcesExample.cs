using System.Globalization;

using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Publishing;

/// <summary>
/// Demonstrates <see cref="AtomMemberResources"/>: a collection, what it accepts, and the Slug header.
/// </summary>
/// <remarks>
///     RFC 5023 section 8.3.3. This is the type a client actually posts to, so it carries the two
///     things a client needs before it can: the collection URI, and the media ranges the server will
///     take.
/// </remarks>
internal static class AtomMemberResourcesExample
{
    /// <summary>
    /// Builds a collection accepting Atom entries and prints it.
    /// </summary>
    public static void ClassExample()
    {
        AtomMemberResources collection = new(
            new Uri("https://endjin.com/app/blog/posts"),
            new AtomTextConstruct("Blog posts"));

        collection.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        AtomCategoryDocument tags = new(isFixed: true, new Uri("https://endjin.com/tags"));
        tags.Categories.Add(new AtomCategory("dotnet") { Label = ".NET" });
        tags.Categories.Add(new AtomCategory("data") { Label = "Data and Analytics" });
        collection.Categories.Add(tags);

        ExampleOutput.ShowAtomMemberResources(collection);
    }

    /// <summary>
    /// Shows the difference between a media range and a media type.
    /// </summary>
    /// <remarks>
    ///     <c>app:accept</c> takes a media <i>range</i>, not a media type: <c>image/*</c> is legal and
    ///     means every image type. A client that string-compares the value against its payload's
    ///     content type will decide it cannot post a PNG to a collection that accepts every image.
    /// </remarks>
    public static void AcceptedMediaRangesExample()
    {
        AtomMemberResources collection = new(
            new Uri("https://endjin.com/app/blog/media"),
            new AtomTextConstruct("Post images"));

        collection.Accepts.Add(new AtomAcceptedMediaRange("image/png"));
        collection.Accepts.Add(new AtomAcceptedMediaRange("image/jpeg"));
        collection.Accepts.Add(new AtomAcceptedMediaRange("image/*"));
        collection.Accepts.Add(new AtomAcceptedMediaRange("audio/mpeg"));

        ExampleOutput.ShowAtomMemberResources(collection);

        Console.WriteLine($"  Atom entry range: {AtomAcceptedMediaRange.AtomEntryMediaRange}");
        Console.WriteLine($"  Atom feed range:  {AtomAcceptedMediaRange.AtomFeedMediaRange}");
    }

    /// <summary>
    /// Encodes and decodes a Slug header value.
    /// </summary>
    /// <remarks>
    ///     RFC 5023 section 9.7. Slug is a client's suggestion for the member's name, and it travels in
    ///     an HTTP header — so anything outside US-ASCII has to be percent-encoded on the way out and
    ///     decoded on the way back. Round-tripping a title with an em dash and an accent is the case
    ///     that catches a naive implementation.
    /// </remarks>
    public static void SlugExample()
    {
        const string title = "Rx.NET v7.0 — it could save you 95MB";

        string slug = AtomMemberResources.SlugEncode(title);
        string decoded = AtomMemberResources.SlugDecode(slug);

        Console.WriteLine($"  Title:   {title}");
        Console.WriteLine($"  Slug:    {slug}");
        Console.WriteLine($"  Decoded: {decoded}");
        Console.WriteLine($"  Round trip preserved: {decoded == title}");
    }

    /// <summary>
    /// Builds the edit and edit-media links a server returns with a created member.
    /// </summary>
    /// <remarks>
    ///     A collection member is edited through its <c>edit</c> link and its media through
    ///     <c>edit-media</c>. They are separate URIs and a client must not assume one from the other.
    /// </remarks>
    public static void EditLinksExample()
    {
        AtomLink edit = AtomMemberResources.CreateEditLink(
            new Uri("https://endjin.com/app/blog/posts/rxdotnet-v7-0-released"));

        AtomLink editMedia = AtomMemberResources.CreateEditMediaLink(
            new Uri("https://endjin.com/app/blog/media/rx-dotnet-v7-0-released.jpg"),
            "image/jpeg",
            CultureInfo.GetCultureInfo("en-GB"));

        ExampleOutput.ShowAtomLink(edit);
        ExampleOutput.ShowAtomLink(editMedia);
    }

    /// <summary>
    /// Reads the collections out of a loaded service document.
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomServiceDocument document = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomServiceDocument);
        document.Load(stream);

        foreach (AtomWorkspace workspace in document.Workspaces)
        {
            foreach (AtomMemberResources collection in workspace.Collections)
            {
                ExampleOutput.ShowAtomMemberResources(collection);
            }
        }
    }
}