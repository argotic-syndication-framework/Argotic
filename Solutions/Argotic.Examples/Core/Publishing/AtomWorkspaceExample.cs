using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Publishing;

/// <summary>
/// Demonstrates <see cref="AtomWorkspace"/>: a named group of collections inside a service document.
/// </summary>
/// <remarks>
///     RFC 5023 section 8.3.2. A workspace has no URI of its own and no meaning to the protocol beyond
///     grouping — it exists so a server can present "the blog" and "the talks" as separate areas of one
///     API. A client is expected to show the grouping and is not expected to derive anything from it.
/// </remarks>
internal static class AtomWorkspaceExample
{
    /// <summary>
    /// Builds a workspace holding two collections and prints it.
    /// </summary>
    public static void ClassExample()
    {
        AtomWorkspace workspace = new(new AtomTextConstruct("endjin blog"));

        AtomMemberResources posts = new(
            new Uri("https://endjin.com/app/blog/posts"),
            new AtomTextConstruct("Blog posts"));
        posts.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        AtomMemberResources media = new(
            new Uri("https://endjin.com/app/blog/media"),
            new AtomTextConstruct("Post images"));
        media.Accepts.Add(new AtomAcceptedMediaRange("image/png"));

        workspace.Collections.Add(posts);
        workspace.Collections.Add(media);

        ExampleOutput.ShowAtomWorkspace(workspace);
    }

    /// <summary>
    /// Builds a workspace from a collection sequence in one call.
    /// </summary>
    public static void ConstructorExample()
    {
        AtomMemberResources talks = new(
            new Uri("https://endjin.com/app/talks"),
            new AtomTextConstruct("Talks"));
        talks.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        AtomMemberResources audio = new(
            new Uri("https://endjin.com/app/talks/audio"),
            new AtomTextConstruct("Audio versions"));
        audio.Accepts.Add(new AtomAcceptedMediaRange("audio/mpeg"));

        AtomWorkspace workspace = new(new AtomTextConstruct("endjin talks"), [talks, audio]);

        ExampleOutput.ShowAtomWorkspace(workspace);
    }

    /// <summary>
    /// Reads the workspaces out of a loaded service document.
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomServiceDocument document = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomServiceDocument);
        document.Load(stream);

        foreach (AtomWorkspace workspace in document.Workspaces)
        {
            ExampleOutput.ShowAtomWorkspace(workspace);
        }
    }
}