using System.Xml;

namespace Argotic.Common;

/// <summary>
/// Provides extension methods for <see cref="IXmlWritable"/> objects.
/// </summary>
public static class XmlWritableExtensions
{
    /// <summary>
    /// Returns an XML string representation of the current <see cref="IXmlWritable"/> object.
    /// </summary>
    /// <param name="writable">The object to convert to an XML string.</param>
    /// <returns>The object as an indented XML fragment, without an XML declaration.</returns>
    /// <remarks>
    ///     Written as a fragment rather than as a document, because these entities are composed into a
    ///     document by their parent. The result is therefore not a standalone XML document and carries
    ///     no encoding declaration.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writable"/> is <see langword="null"/>.</exception>
    public static string ToXmlString(this IXmlWritable writable)
    {
        ArgumentNullException.ThrowIfNull(writable);

        using StringWriter stringWriter = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
        {
            writable.WriteTo(writer);
        }

        return stringWriter.ToString();
    }

    /// <summary>
    /// Returns an XML string representation of the current <see cref="IXmlWritableWithElementName"/> object using a specified element name.
    /// </summary>
    /// <param name="writable">The object to convert to an XML string.</param>
    /// <param name="elementName">The local name to write the object under. An Atom text construct is one shape written as <c>title</c>, <c>rights</c> or <c>subtitle</c> depending on where it sits, and a person construct as <c>author</c> or <c>contributor</c>; the name is what distinguishes them.</param>
    /// <returns>The object as an indented XML fragment, without an XML declaration.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="writable"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="elementName"/> is an empty string.</exception>
    public static string ToXmlString(this IXmlWritableWithElementName writable, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writable);
        ArgumentException.ThrowIfNullOrEmpty(elementName);

        using StringWriter stringWriter = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
        {
            writable.WriteTo(writer, elementName);
        }

        return stringWriter.ToString();
    }
}