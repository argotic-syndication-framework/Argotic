using System.Xml;
using System.Xml.XPath;

namespace Argotic.Net;

/// <summary>
/// Represents the content of an XML-RPC <c>&lt;value&gt;</c> element.
/// </summary>
/// <remarks>
///     XML-RPC has exactly three shapes of value, and this interface has exactly three implementations
///     to match: <see cref="XmlRpcScalarValue"/> for the seven leaf types, <see cref="XmlRpcArrayValue"/>
///     for an ordered list, and <see cref="XmlRpcStructureValue"/> for a named-member record. Arrays and
///     structures nest, so a parameter is a tree, not a row.
/// </remarks>
/// <seealso cref="XmlRpcArrayValue"/>
/// <seealso cref="XmlRpcScalarValue"/>
/// <seealso cref="XmlRpcStructureValue"/>
public interface IXmlRpcValue
{
    /// <summary>
    /// Loads this <see cref="IXmlRpcValue"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="IXmlRpcValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="IXmlRpcValue"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    bool Load(XPathNavigator source);

    /// <summary>
    /// Saves the current <see cref="IXmlRpcValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    void WriteTo(XmlWriter writer);

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="IXmlRpcValue"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance, as <see cref="WriteTo(XmlWriter)"/> would write it — a <c>&lt;value&gt;</c> element and its content.</returns>
    string ToString();
}