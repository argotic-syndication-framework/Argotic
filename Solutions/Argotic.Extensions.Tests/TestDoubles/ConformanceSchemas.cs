using System.Xml.Schema;
namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// The XML schemas the conformance tests validate against, and the validation itself.
/// </summary>
/// <remarks>
///     <para>
///     <b>Why this exists at all.</b> A round-trip test cannot see a defect the reader is lenient about.
///     <c>SitemapVideo.WriteTo</c> emitted <c>tag</c>, <c>restriction</c>, <c>platform</c> and
///     <c>uploader</c> in an order <c>sitemap-video-1.1.xsd</c> rejects, so every video sitemap this
///     library wrote carrying one of them was invalid against Google's own schema — and save-then-load
///     stayed symmetric over it, so every existing test agreed with the defect. A validating parser is
///     the only instrument in the suite that can disagree.
///     </para>
///     <para>
///     <b>Schemas are embedded resources, not files and not string literals.</b> Embedding keeps them
///     byte-for-byte identical to what the publisher served, so a checksum against the canonical URL
///     still matches — which a re-indented raw string literal cannot claim. Nothing here reads the file
///     system.
///     </para>
///     <para>
///     <b>Google's three sitemap extension schemas are not here.</b> They carry an explicit
///     "All Rights Reserved" notice, so they are never committed; the integration tier fetches them at
///     test time instead. <see cref="SitemapWith"/> is how those are folded in. See
///     <c>Schemas/NOTICE.md</c>.
///     </para>
/// </remarks>
internal static class ConformanceSchemas
{
    /// <summary>The sitemaps.org core schema, covering <c>urlset</c> and <c>url</c>.</summary>
    public const string SitemapCore = "sitemap-0.9.xsd";

    /// <summary>The sitemaps.org index schema, covering <c>sitemapindex</c>.</summary>
    public const string SitemapIndex = "siteindex-0.9.xsd";

    /// <summary>Our single-element declaration of <c>xhtml:link</c>, for hreflang annotations.</summary>
    public const string XhtmlLink = "xhtml-link.xsd";

    /// <summary>The APML 0.6 schema.</summary>
    public const string Apml = "apml-0.6.xsd";

    private static readonly Lazy<XmlSchemaSet> SitemapSchemas = new(() => Compile(Read(SitemapCore), Read(SitemapIndex), Read(XhtmlLink)));

    private static readonly Lazy<XmlSchemaSet> ApmlSchemas = new(() => Compile(Read(Apml)));

    /// <summary>
    /// Gets the schema set for sitemaps, sitemap indexes and hreflang annotations.
    /// </summary>
    public static XmlSchemaSet Sitemap => SitemapSchemas.Value;

    /// <summary>
    /// Gets the schema set for APML documents.
    /// </summary>
    public static XmlSchemaSet ApmlDocument => ApmlSchemas.Value;

    /// <summary>
    /// Reads an embedded schema as text.
    /// </summary>
    /// <param name="fileName">The schema's file name, as one of the constants on this class.</param>
    /// <returns>The schema source.</returns>
    public static string Read(string fileName)
    {
        string resource = $"Argotic.Extensions.Tests.Schemas.{fileName}";

        using Stream? stream = typeof(ConformanceSchemas).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException(
                $"The schema '{resource}' is not embedded in the test assembly. Check the EmbeddedResource glob in Argotic.Extensions.Tests.csproj.");

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Builds a sitemap schema set extended with further schemas, for the Google extensions the
    /// integration tier fetches rather than embeds.
    /// </summary>
    /// <param name="additional">The extra schema sources to compile alongside the sitemap set.</param>
    /// <returns>A compiled schema set.</returns>
    /// <remarks>
    ///     A new set rather than an addition to <see cref="Sitemap"/>: the cached set is shared by
    ///     method-level parallel tests, and mutating it from one of them would change what the others
    ///     validate against.
    /// </remarks>
    public static XmlSchemaSet SitemapWith(params string[] additional) =>
        Compile([Read(SitemapCore), Read(SitemapIndex), Read(XhtmlLink), .. additional]);

    /// <summary>
    /// Validates a document and returns every problem the parser reported.
    /// </summary>
    /// <param name="document">The XML to validate.</param>
    /// <param name="schemas">The schema set to validate against.</param>
    /// <returns>The problems found, empty when the document conforms.</returns>
    public static IReadOnlyList<string> Validate(string document, XmlSchemaSet schemas)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(schemas);

        List<string> problems = [];

        XmlReaderSettings settings = new()
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
        };

        // Without this, a wildcard failure - "the element is not declared", which is what an unknown
        // extension namespace produces under processContents="strict" - arrives as a warning and is
        // dropped on the floor. The check would then pass on documents it never actually inspected.
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, e) => problems.Add($"{e.Severity}: {e.Message}");

        try
        {
            using StringReader text = new(StripByteOrderMark(document));
            using XmlReader reader = XmlReader.Create(text, settings);
            while (reader.Read())
            {
            }
        }
        catch (XmlException ex)
        {
            problems.Add($"XmlException: {ex.Message}");
        }

        return problems;
    }

    /// <summary>
    /// Validates a document and returns the problems, as a single message ready to hand to Shouldly.
    /// </summary>
    /// <param name="document">The XML to validate.</param>
    /// <param name="schemas">The schema set to validate against.</param>
    /// <returns>An empty string when the document conforms; otherwise every problem, one per line.</returns>
    public static string Describe(string document, XmlSchemaSet schemas) =>
        string.Join(Environment.NewLine, Validate(document, schemas));

    /// <summary>
    /// Removes a leading byte order mark.
    /// </summary>
    /// <param name="document">The document text.</param>
    /// <returns>The text without a leading U+FEFF.</returns>
    /// <remarks>
    ///     <c>Sitemap.Save</c> writes a UTF-8 BOM, and <see cref="StringReader"/> hands the resulting
    ///     U+FEFF to the parser as content. The failure is "Data at the root level is invalid. Line 1,
    ///     position 1" — which reads as a malformed document when nothing is wrong with it, and cost an
    ///     afternoon the first time. Strip it here so no caller has to know.
    /// </remarks>
    private static string StripByteOrderMark(string document) => document.TrimStart('﻿');

    private static XmlSchemaSet Compile(params string[] sources)
    {
        XmlSchemaSet schemas = new();

        foreach (string source in sources)
        {
            using StringReader text = new(source);
            using XmlReader reader = XmlReader.Create(text);

            // Null rather than a named namespace: each schema declares its own targetNamespace, and
            // naming it here would only be a second place for the two to disagree.
            schemas.Add(null, reader);
        }

        schemas.Compile();
        return schemas;
    }
}