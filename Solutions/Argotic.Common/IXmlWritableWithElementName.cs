using System.Xml;

namespace Argotic.Common;

/// <summary>
/// Represents an object that can write its contents to an <see cref="XmlWriter"/> with a specified element name.
/// </summary>
public interface IXmlWritableWithElementName : IXmlWritable
{
    /// <summary>
    /// Saves the current object to the specified <see cref="XmlWriter"/> using the specified element name.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name of the element being written.</param>
    void WriteTo(XmlWriter writer, string elementName);
}