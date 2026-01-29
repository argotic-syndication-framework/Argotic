using System.Xml;

namespace Argotic.Common;

/// <summary>
/// Represents an object that can write its contents to an <see cref="XmlWriter"/>.
/// </summary>
public interface IXmlWritable
{
    /// <summary>
    /// Saves the current object to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    void WriteTo(XmlWriter writer);
}
