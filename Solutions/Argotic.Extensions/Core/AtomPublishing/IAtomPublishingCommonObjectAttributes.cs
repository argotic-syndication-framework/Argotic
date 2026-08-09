using System.Globalization;

namespace Argotic.Extensions.Core;

/// <summary>
/// Allows an object to implement common Atom entity attributes by representing a set of properties, methods, indexers and events common to Atom syndication resources.
/// </summary>
interface IAtomPublishingCommonObjectAttributes
{
    /// <summary>
    /// Gets or sets the base URI other than the base URI of the document or external entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <see langword="null"/> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is interpreted as a URI Reference as defined in <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986: Uniform Resource Identifier (URI): Generic Syntax</a>, 
    ///         after processing according to <a href="https://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
    /// </remarks>
    Uri? BaseUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <see langword="null"/> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066: Tags for the Identification of Languages</a> (BCP 47; now RFC 5646).
    ///     </para>
    /// </remarks>
    CultureInfo? Language
    {
        get;
        set;
    }
}