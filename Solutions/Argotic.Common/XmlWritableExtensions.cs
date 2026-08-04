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
    /// <returns>An XML string representation of the object.</returns>
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
    /// <param name="elementName">The element name to use when writing the object.</param>
    /// <returns>An XML string representation of the object.</returns>
    /// <remarks>
    /// This overload is useful for objects that have a WriteTo method with an additional element name parameter.
    /// </remarks>
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