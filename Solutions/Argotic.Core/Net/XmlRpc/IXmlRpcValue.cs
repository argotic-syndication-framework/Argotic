namespace Argotic.Net
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Defines generalized properties, methods, indexers and events that a value type or class
    /// implements to create a type-specific XML-RPC values.
    /// </summary>
    /// <seealso cref="XmlRpcArrayValue"/>
    /// <seealso cref="XmlRpcScalarValue"/>
    /// <seealso cref="XmlRpcStructureValue"/>
    public interface IXmlRpcValue
    {
        /// <summary>
        /// Loads this <see cref="IXmlRpcValue"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="IXmlRpcValue"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="IXmlRpcValue"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        bool Load(XPathNavigator source);

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="IXmlRpcValue"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="IXmlRpcValue"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        string ToString();

        /// <summary>
        /// Saves the current <see cref="IXmlRpcValue"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        void WriteTo(XmlWriter writer);
    }
}