using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Publishing;

/// <summary>
/// Demonstrates <see cref="AtomCategoryDocument"/>: the vocabulary a collection will accept.
/// </summary>
/// <remarks>
///     RFC 5023 section 7. A category document appears two ways — inline inside a collection in the
///     service document, or served separately and referenced by <c>href</c>. The out-of-line form lets
///     several collections share one vocabulary and lets it change without reissuing the service
///     document, which is why both forms exist and why a client has to handle both.
/// </remarks>
internal static class AtomCategoryDocumentExample
{
    /// <summary>
    /// Builds a fixed category document and prints it.
    /// </summary>
    /// <remarks>
    ///     <c>fixed="yes"</c> closes the vocabulary: a client must not post a term outside it. The
    ///     attribute defaults to <c>no</c>, so a client that ignores it will invent terms a server is
    ///     entitled to reject.
    /// </remarks>
    public static void ClassExample()
    {
        AtomCategoryDocument document = new(isFixed: true, new Uri("https://endjin.com/tags"));

        document.Categories.Add(new AtomCategory("dotnet") { Label = ".NET" });
        document.Categories.Add(new AtomCategory("rx-dotnet") { Label = "Rx.NET" });
        document.Categories.Add(new AtomCategory("power-bi") { Label = "Power BI" });
        document.Categories.Add(new AtomCategory("duckdb") { Label = "DuckDB" });

        ExampleOutput.ShowAtomCategoryDocument(document);
    }

    /// <summary>
    /// Loads a stand-alone category document from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomCategoryDocument document = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomCategoryDocument);
        document.Load(stream);

        ExampleOutput.ShowAtomCategoryDocument(document);
    }

    /// <summary>
    /// Builds the out-of-line form, which carries a reference rather than terms.
    /// </summary>
    /// <remarks>
    ///     An out-of-line document has an <c>href</c> and no categories of its own. A reader that
    ///     assumed terms are always present would report this collection as accepting nothing, when in
    ///     fact it accepts whatever the referenced document lists.
    /// </remarks>
    public static void OutOfLineExample()
    {
        AtomCategoryDocument reference = new(new Uri("https://endjin.com/app/categories/media"));

        Console.WriteLine($"Out-of-line reference: {reference.Uri}");
        Console.WriteLine($"Terms carried inline:  {reference.Categories.Count}");

        ExampleOutput.ShowAtomCategoryDocument(reference);
    }

    /// <summary>
    /// Saves a category document and reads it back, showing that the fixed flag survives.
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomCategoryDocument document = new(isFixed: true, new Uri("https://endjin.com/tags"));
        document.Categories.Add(new AtomCategory("dotnet") { Label = ".NET" });
        document.Categories.Add(new AtomCategory("ai") { Label = "AI" });

        using MemoryStream stream = new();
        document.Save(stream);
        ExampleOutput.ShowSaved("AtomCategoryDocument");

        stream.Seek(0, SeekOrigin.Begin);
        AtomCategoryDocument reloaded = new();
        reloaded.Load(stream);

        ExampleOutput.ShowAtomCategoryDocument(reloaded);
    }
}