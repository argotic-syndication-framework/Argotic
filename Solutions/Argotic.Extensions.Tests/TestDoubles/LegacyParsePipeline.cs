using System.Text.RegularExpressions;
namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// A frozen copy of the parse pipeline as it stood at <c>c19e5cf</c>, before the HTTP → XML rewrite.
/// </summary>
/// <remarks>
/// <para>
/// <b>This file must not reference <c>Argotic.Common</c>.</b> There is no <c>using</c> for that
/// namespace and there must never be one, nor a <c>using static</c>. A copy that binds the live
/// <c>SyndicationEncodingUtility</c> members moves in lockstep with the rewrite, and a differential
/// harness comparing a thing to itself reports green through any regression whatsoever. The
/// enforcement is mechanical rather than aspirational: with no import in scope, a leftover
/// <c>SyndicationEncodingUtility.</c> prefix is <c>CS0103</c>, and an import added to make one compile
/// is an unused-using warning under <c>dotnet format style</c>, which is a standing gate.
/// </para>
/// <para>
/// Ten members are transcribed, not the five the plan first named. The extra five are not optional:
/// <c>GetXmlEncoding(byte[])</c> falls back to <c>GetXmlEncoding(Stream)</c>, which the rewrite
/// deletes, so a copy binding the live one stops compiling partway through the programme rather than
/// at a point anyone chose.
/// </para>
/// <list type="table">
///   <listheader><term>Member</term><description>Source, at <c>c19e5cf</c></description></listheader>
///   <item><term><c>XmlDeclarationProbeLength</c></term><description><c>SyndicationEncodingUtility.cs:39</c></description></item>
///   <item><term><c>XmlDeclarationEncodingRegex</c></term><description><c>:49-50</c></description></item>
///   <item><term><c>CreateSafeXmlReaderSettings</c></term><description><c>:87-99</c> — called <i>unqualified</i> at <c>:168</c>, which is why it is here</description></item>
///   <item><term><c>CreateSafeNavigator(string)</c></term><description><c>:161-173</c></description></item>
///   <item><term><c>CreateSafeNavigator(Stream)</c></term><description><c>:189-199</c></description></item>
///   <item><term><c>CreateSafeNavigator(Stream, Encoding)</c></term><description><c>:213-220</c></description></item>
///   <item><term><c>CreateSafeNavigator(TextReader)</c></term><description><c>:232-237</c> — the overload Phase 2 rewrites</description></item>
///   <item><term><c>GetXmlEncoding(byte[])</c></term><description><c>:457-494</c></description></item>
///   <item><term><c>GetXmlEncoding(Stream)</c></term><description><c>:505-511</c> — deleted by Phase 3</description></item>
///   <item><term><c>GetXmlEncoding(string)</c></term><description><c>:523-532</c></description></item>
///   <item><term><c>EncodingFromMatch</c></term><description><c>:539-560</c></description></item>
///   <item><term><c>RemoveInvalidXmlHexadecimalCharacters</c></term><description><c>:576-609</c> — replaced by Phase 1</description></item>
///   <item><term><c>IndexOfInvalidXmlCharacter</c></term><description><c>:620-639</c></description></item>
///   <item><term><c>GetStreamBytes</c></term><description><c>:682-708</c> — <c>internal</c> in the product, so a test can never call the live one and the transcription can never be checked against it directly</description></item>
/// </list>
/// <para>
/// Instances are immutable and the two shared ones are safe to use from parallel tests;
/// <c>AssemblyInfo.cs</c> sets method-level parallelism, so static mutable state would be a race.
/// </para>
/// </remarks>
/// <param name="mutateRebuild">
///     <see langword="true"/> to drop the surrogate-pair arm from the sanitiser's rebuild loop. This is
///     not a variant of the legacy behaviour — it is a deliberate defect, used to prove the comparer
///     can tell two pipelines apart. See <see cref="Mutated"/>.
/// </param>
internal sealed partial class LegacyParsePipeline(bool mutateRebuild)
{
    /// <summary>
    /// The faithful copy — what the pipeline did at <c>c19e5cf</c>.
    /// </summary>
    public static LegacyParsePipeline Faithful { get; } = new(mutateRebuild: false);

    /// <summary>
    /// The negative control: identical but for the surrogate-pair arm of the rebuild loop.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     "Both sides green" proves nothing on its own — a copy that accidentally calls the live
    ///     implementation is green too, and stays green through every regression the differential
    ///     exists to catch. This variant is one line different from the real thing, and the harness is
    ///     required to report it as <i>different</i>. It is committed rather than applied by hand
    ///     because a one-time manual perturbation stops protecting anything the moment it is undone.
    ///     </para>
    ///     <para>
    ///     <b>It has to be the rebuild loop, and finding that out is itself worth recording.</b>
    ///     Perturbing the equivalent arm in <see cref="IndexOfInvalidXmlCharacter"/> produces no
    ///     observable difference at all: the scan only decides where the rebuild starts copying, and
    ///     the rebuild is correct from any starting index, so a defect there costs a needless rebuild
    ///     and cannot corrupt a document. That makes the scan's surrogate arm — the one §2.19 reports
    ///     at 3 of 4 branches — unobservable through output, and it means the only honest control is
    ///     the rebuild arm, which §2.19 reports at <i>zero hits</i>.
    ///     </para>
    /// </remarks>
    public static LegacyParsePipeline Mutated { get; } = new(mutateRebuild: true);

    /// <summary>
    /// The number of leading characters of a document examined when looking for an XML declaration.
    /// </summary>
    private const int XmlDeclarationProbeLength = 512;

    /// <summary>
    /// Matches the <c>encoding</c> pseudo-attribute of an XML declaration.
    /// </summary>
    /// <returns>The compiled regular expression.</returns>
    /// <remarks>
    ///     Source-generated, matching the original, and not merely for parity. A hand-rolled
    ///     <c>new Regex(pattern, IgnoreCase | Singleline)</c> uses the current culture's case folding
    ///     where the generated form uses invariant, so the two diverge under a Turkish-locale run on
    ///     the exact input at <c>SyndicationEncodingUtilityTest.cs:283</c>. It would also trip
    ///     SYSLIB1045 under this repository's <c>AnalysisMode=AllEnabledByDefault</c>, which is a
    ///     zero-warning gate failure rather than a style opinion.
    /// </remarks>
    [GeneratedRegex("""^<\?xml.+?encoding\s*=\s*(?:"(?<webName>[^"]*)"|(?<webName>\S+)).*?\?>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex XmlDeclarationEncodingRegex();

    /// <summary>
    /// Creates <see cref="XmlReaderSettings"/> configured for secure XML parsing.
    /// </summary>
    /// <returns>The settings the legacy pipeline parsed with.</returns>
    public static XmlReaderSettings CreateSafeXmlReaderSettings() => new()
    {
        ConformanceLevel = ConformanceLevel.Document,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Parse,
        XmlResolver = null,
        MaxCharactersFromEntities = 10_000_000,
    };

    /// <summary>
    /// Creates an <see cref="XPathNavigator"/> against the supplied XML string.
    /// </summary>
    /// <param name="xml">The XML data to navigate.</param>
    /// <returns>A navigator over the sanitised document.</returns>
    public XPathNavigator CreateSafeNavigator(string xml)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);

        string safeXml = this.RemoveInvalidXmlHexadecimalCharacters(xml);

        using StringReader stringReader = new(safeXml);
        using XmlReader xmlReader = XmlReader.Create(stringReader, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);
        XPathNavigator navigator = document.CreateNavigator();

        return navigator;
    }

    /// <summary>
    /// Creates an <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream containing the XML data.</param>
    /// <returns>A navigator over the sanitised document.</returns>
    public XPathNavigator CreateSafeNavigator(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] buffer = GetStreamBytes(stream);

        Encoding encoding = GetXmlEncoding(buffer);

        using MemoryStream memoryStream = new(buffer);
        return this.CreateSafeNavigator(memoryStream, encoding);
    }

    /// <summary>
    /// Creates an <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/> using the specified <see cref="Encoding"/>.
    /// </summary>
    /// <param name="stream">The stream containing the XML data.</param>
    /// <param name="encoding">The character encoding to read with.</param>
    /// <returns>A navigator over the sanitised document.</returns>
    /// <remarks>
    ///     The two-argument <see cref="StreamReader"/> constructor owns the stream, so this overload
    ///     closes the caller's stream where the single-argument one does not. That asymmetry is part of
    ///     the behaviour being frozen, not an oversight in the transcription.
    /// </remarks>
    public XPathNavigator CreateSafeNavigator(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);

        using StreamReader reader = new(stream, encoding);
        return this.CreateSafeNavigator(reader.ReadToEnd());
    }

    /// <summary>
    /// Creates an <see cref="XPathNavigator"/> against the supplied <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The reader containing the XML data.</param>
    /// <returns>A navigator over the sanitised document.</returns>
    public XPathNavigator CreateSafeNavigator(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return this.CreateSafeNavigator(reader.ReadToEnd());
    }

    /// <summary>
    /// Returns the character encoding declared by the supplied bytes.
    /// </summary>
    /// <param name="data">The XML data source.</param>
    /// <returns>The declared encoding, or <see cref="Encoding.UTF8"/> when none is declared or it is unknown.</returns>
    public static Encoding GetXmlEncoding(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length > XmlDeclarationProbeLength)
        {
            using MemoryStream head = new(data, 0, XmlDeclarationProbeLength, writable: false);
            using StreamReader headReader = new(head, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string prefix = headReader.ReadToEnd();

            Match prefixMatch = XmlDeclarationEncodingRegex().Match(prefix);
            if (prefixMatch.Success)
            {
                return EncodingFromMatch(prefixMatch);
            }

            if (!prefix.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }
        }

        using MemoryStream stream = new(data);
        return GetXmlEncoding(stream);
    }

    /// <summary>
    /// Returns the character encoding declared by the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The XML data source.</param>
    /// <returns>The declared encoding, or <see cref="Encoding.UTF8"/> when none is declared or it is unknown.</returns>
    /// <remarks>
    ///     Reads the entire stream to find forty bytes. Phase 3 deletes this from the product; the copy
    ///     stays because <see cref="GetXmlEncoding(byte[])"/> falls back to it, and an empty array
    ///     reaches the string overload's guard through here — which is where the leaked
    ///     <c>paramName == "content"</c> comes from.
    /// </remarks>
    public static Encoding GetXmlEncoding(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using StreamReader reader = new(stream);
        return GetXmlEncoding(reader.ReadToEnd());
    }

    /// <summary>
    /// Returns the character encoding declared by the supplied content.
    /// </summary>
    /// <param name="content">The XML data.</param>
    /// <returns>The declared encoding, or <see cref="Encoding.UTF8"/> when none is declared or it is unknown.</returns>
    public static Encoding GetXmlEncoding(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        return EncodingFromMatch(XmlDeclarationEncodingRegex().Match(content));
    }

    /// <summary>
    /// Sanitises the supplied string so that it can be safely represented in XML.
    /// </summary>
    /// <param name="content">The XML data to strip invalid characters from.</param>
    /// <returns>The sanitised string, or the original instance when nothing had to go.</returns>
    public string RemoveInvalidXmlHexadecimalCharacters(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        int firstInvalid = IndexOfInvalidXmlCharacter(content);
        if (firstInvalid < 0)
        {
            return content;
        }

        StringBuilder result = new(content.Length);
        result.Append(content.AsSpan(0, firstInvalid));

        for (int i = firstInvalid; i < content.Length; i++)
        {
            if (XmlConvert.IsXmlChar(content[i]))
            {
                result.Append(content[i]);
            }

            // The one line Mutated changes. Without this arm a valid astral character is dropped
            // rather than kept, so the document survives the parse and is simply a different
            // document - which is exactly the class of defect a differential exists to see and an
            // assertion about the current implementation cannot.
            else if (!mutateRebuild && i + 1 < content.Length && XmlConvert.IsXmlSurrogatePair(content[i + 1], content[i]))
            {
                result.Append(content[i]);
                result.Append(content[i + 1]);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Returns the index of the first character the sanitiser would drop.
    /// </summary>
    /// <param name="content">The content to scan.</param>
    /// <returns>The index of the first character that would be removed, or <c>-1</c> when the content is already valid.</returns>
    private static int IndexOfInvalidXmlCharacter(string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            if (XmlConvert.IsXmlChar(content[i]))
            {
                continue;
            }

            if (i + 1 < content.Length && XmlConvert.IsXmlSurrogatePair(content[i + 1], content[i]))
            {
                i++;
                continue;
            }

            return i;
        }

        return -1;
    }

    /// <summary>
    /// Resolves the <see cref="Encoding"/> named by an XML declaration match.
    /// </summary>
    /// <param name="encodingMatch">The result of matching the declaration regex.</param>
    /// <returns>The named encoding, or <see cref="Encoding.UTF8"/> if the match failed or named an unknown encoding.</returns>
    private static Encoding EncodingFromMatch(Match encodingMatch)
    {
        Encoding encoding = Encoding.UTF8;

        if (encodingMatch is { Groups.Count: > 0 })
        {
            Group group = encodingMatch.Groups["webName"];
            if (group is not null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(group.Value);
                }
                catch (ArgumentException)
                {
                    encoding = Encoding.UTF8;
                }
            }
        }

        return encoding;
    }

    /// <summary>
    /// Gets an array of bytes that represent the data of the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream to drain.</param>
    /// <returns>The stream's remaining bytes.</returns>
    /// <remarks>
    ///     Both branches are transcribed verbatim. The <c>CanSeek</c> arm is the one every test and
    ///     benchmark in this repository takes; the <c>CopyTo</c> arm below it has never executed
    ///     anywhere, and it is the arm a live network stream would take.
    /// </remarks>
    private static byte[] GetStreamBytes(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
        {
            long remaining = stream.Length - stream.Position;
            if (remaining == 0)
            {
                return [];
            }

            byte[] exact = new byte[remaining];
            stream.ReadExactly(exact);
            return exact;
        }

        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}