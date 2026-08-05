using System.Text;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// The result of driving one document through one parse pipeline.
/// </summary>
/// <param name="OuterXml">The parsed document's markup, or <see langword="null"/> if the parse threw.</param>
/// <param name="ExceptionType">The full name of the exception thrown, or <see langword="null"/> if none was.</param>
/// <param name="ExceptionMessage">The exception's message, or <see langword="null"/> if none was thrown.</param>
/// <remarks>
///     <para>
///     The message is carried rather than the parameter name and the line position, because it already
///     contains both: <see cref="ArgumentException"/> appends <c>(Parameter 'xml')</c> and
///     <see cref="System.Xml.XmlException"/> appends <c>Line N, position M</c>. Comparing only the
///     exception <i>type</i> would make any two parse failures equal, which is worse than useless here
///     — most of what this harness protects is the difference between one failure and another.
///     </para>
/// </remarks>
internal readonly record struct ParseOutcome(string? OuterXml, string? ExceptionType, string? ExceptionMessage);

/// <summary>
/// Drives one document through both the frozen pipeline and the live one, so the two can be compared.
/// </summary>
/// <remarks>
///     <para>
///     This is the only safety argument Phases 1 to 3 have. The suite gained 107 tests around the parse
///     path during the test expansion and not one of them is a differential: they assert what the
///     current implementation does, so they move with it. A rewrite that changes behaviour
///     consistently passes all of them.
///     </para>
///     <para>
///     <b>Green on both sides proves nothing by itself.</b> A frozen copy that accidentally binds the
///     live implementation is also green, and stays green through every regression this exists to
///     catch. <see cref="LegacyParsePipeline.Mutated"/> is the control that makes green mean something,
///     and it is committed rather than applied by hand so that it keeps meaning something.
///     </para>
/// </remarks>
internal static class ParsePipelineDifferential
{
    /// <summary>
    /// Compares the two pipelines over the <c>string</c> entry point.
    /// </summary>
    /// <param name="xml">The document.</param>
    /// <param name="legacy">The frozen pipeline to use, defaulting to the faithful copy.</param>
    /// <returns>Both outcomes.</returns>
    public static (ParseOutcome Legacy, ParseOutcome Live) OverString(string xml, LegacyParsePipeline? legacy = null)
        => (Capture(() => (legacy ?? LegacyParsePipeline.Faithful).CreateSafeNavigator(xml)),
            Capture(() => SyndicationEncodingUtility.CreateSafeNavigator(xml)));

    /// <summary>
    /// Compares the two pipelines over the <see cref="TextReader"/> entry point.
    /// </summary>
    /// <param name="xml">The document.</param>
    /// <param name="legacy">The frozen pipeline to use, defaulting to the faithful copy.</param>
    /// <returns>Both outcomes.</returns>
    /// <remarks>
    ///     Each side gets its own reader: a <see cref="TextReader"/> is consumed by the first pipeline
    ///     that touches it, so sharing one would hand the second an empty document and report a
    ///     difference that is entirely the harness's fault.
    /// </remarks>
    public static (ParseOutcome Legacy, ParseOutcome Live) OverTextReader(string xml, LegacyParsePipeline? legacy = null)
        => (Capture(() =>
            {
                using StringReader reader = new(xml);
                return (legacy ?? LegacyParsePipeline.Faithful).CreateSafeNavigator(reader);
            }),
            Capture(() =>
            {
                using StringReader reader = new(xml);
                return SyndicationEncodingUtility.CreateSafeNavigator(reader);
            }));

    /// <summary>
    /// Compares the two pipelines over the <see cref="Stream"/> entry point.
    /// </summary>
    /// <param name="document">The document's bytes, exactly as they would arrive.</param>
    /// <param name="legacy">The frozen pipeline to use, defaulting to the faithful copy.</param>
    /// <returns>Both outcomes.</returns>
    public static (ParseOutcome Legacy, ParseOutcome Live) OverStream(byte[] document, LegacyParsePipeline? legacy = null)
        => (Capture(() =>
            {
                using MemoryStream stream = new(document, writable: false);
                return (legacy ?? LegacyParsePipeline.Faithful).CreateSafeNavigator(stream);
            }),
            Capture(() =>
            {
                using MemoryStream stream = new(document, writable: false);
                return SyndicationEncodingUtility.CreateSafeNavigator(stream);
            }));

    /// <summary>
    /// Compares the two pipelines over the <see cref="Stream"/> and <see cref="Encoding"/> entry point.
    /// </summary>
    /// <param name="document">The document's bytes, exactly as they would arrive.</param>
    /// <param name="encoding">The encoding to decode with, overriding whatever the document declares.</param>
    /// <param name="legacy">The frozen pipeline to use, defaulting to the faithful copy.</param>
    /// <returns>Both outcomes.</returns>
    public static (ParseOutcome Legacy, ParseOutcome Live) OverStreamWithEncoding(byte[] document, Encoding encoding, LegacyParsePipeline? legacy = null)
        => (Capture(() =>
            {
                using MemoryStream stream = new(document, writable: false);
                return (legacy ?? LegacyParsePipeline.Faithful).CreateSafeNavigator(stream, encoding);
            }),
            Capture(() =>
            {
                using MemoryStream stream = new(document, writable: false);
                return SyndicationEncodingUtility.CreateSafeNavigator(stream, encoding);
            }));

    private static ParseOutcome Capture(Func<XPathNavigator> parse)
    {
        try
        {
            XPathNavigator navigator = parse();

            // XPathNavigator.OuterXml is position-dependent, and the two pipelines need not leave
            // their navigators on the same node. Without this, two identical documents can compare
            // unequal for a reason that has nothing to do with either implementation.
            navigator.MoveToRoot();
            return new ParseOutcome(navigator.OuterXml, null, null);
        }
#pragma warning disable CA1031 // The point of this harness is that both pipelines fail identically, whatever they throw.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            return new ParseOutcome(null, exception.GetType().FullName, exception.Message);
        }
    }
}