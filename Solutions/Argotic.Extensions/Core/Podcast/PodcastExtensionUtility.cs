using System.Xml;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Helpers shared by the Podcasting 2.0 element types.
/// </summary>
/// <remarks>
///     <para>
///     The namespace defines many small elements that differ only in which attributes they carry, so the
///     same three pieces of code would otherwise be written out once per type: rendering an element to a
///     string, reading a "yes"/"no" node value, and writing an optional attribute. Sharing them keeps
///     the difference between two element types visible as the difference between two element types.
///     </para>
/// </remarks>
internal static class PodcastExtensionUtility
{
    /// <summary>
    /// Renders an element to its XML representation.
    /// </summary>
    /// <param name="writeTo">The element's own write callback.</param>
    /// <returns>The XML representation of the element.</returns>
    public static string ToXmlString(Action<XmlWriter> writeTo)
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            writeTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Interprets a node value that the specification defines as "yes" or "no".
    /// </summary>
    /// <param name="value">The node value to interpret.</param>
    /// <returns><b>true</b> for yes, <b>false</b> for no, and <see langword="null"/> for anything else.</returns>
    /// <remarks>
    ///     <para>
    ///     The specification says these elements carry <c>yes</c> or <c>no</c>. Live feeds disagree: of
    ///     321 <c>podcast:locked</c> elements surveyed, 126 say <c>yes</c> and 191 say <c>no</c> — but 3
    ///     say <c>true</c> and 1 says <c>false</c>, and at least one capitalises it. Reading only the two
    ///     documented spellings would silently mis-read those feeds as unlocked, which is the same
    ///     defect §2.46 records for <c>itunes:explicit</c>.
    ///     </para>
    ///     <para>
    ///     An unrecognised value returns <see langword="null"/> rather than <b>false</b>. The two are not
    ///     the same: "the publisher said no" and "the publisher said something we did not understand"
    ///     lead to different handling, and collapsing them would invent a statement nobody made.
    ///     </para>
    /// </remarks>
    public static bool? ParseYesNo(string? value)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        if (string.Equals(trimmed, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(trimmed, "no", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }

    /// <summary>
    /// Returns the spelling the specification uses when writing a "yes" or "no" node value.
    /// </summary>
    /// <param name="value">The value to spell.</param>
    /// <returns><c>yes</c> or <c>no</c>.</returns>
    /// <remarks>
    ///     Reading accepts <c>true</c> and <c>false</c> because real feeds use them; writing emits only
    ///     the documented spelling, so a feed round-tripped through this library comes out conformant
    ///     whichever spelling it went in with.
    /// </remarks>
    public static string YesNo(bool value) => value ? "yes" : "no";

    /// <summary>
    /// Writes an attribute when it has a value, and omits it otherwise.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    public static void WriteOptionalAttribute(XmlWriter writer, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            writer.WriteAttributeString(name, value);
        }
    }

    /// <summary>
    /// Writes an attribute holding a URI when it has a value, and omits it otherwise.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    public static void WriteOptionalAttribute(XmlWriter writer, string name, Uri? value)
    {
        if (value is not null)
        {
            writer.WriteAttributeString(name, value.ToString());
        }
    }

    /// <summary>
    /// Reads an attribute as a <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">The navigator positioned on the element.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The parsed <see cref="Uri"/>, or <see langword="null"/> if the attribute is absent or unparseable.</returns>
    public static Uri? ReadUriAttribute(System.Xml.XPath.XPathNavigator source, string name)
    {
        string value = source.GetAttribute(name, string.Empty);

        return !string.IsNullOrEmpty(value) && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? uri)
            ? uri
            : null;
    }
}