using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the entity encoding utilized by human-readable text constructs.
/// </summary>
/// <seealso cref="AtomTextConstruct"/>
/// <remarks>
///     The three values RFC 4287 §3.1.1 permits for a Text construct's <c>type</c> attribute, plus <see cref="None"/> for its absence. See
///     <see cref="AtomTextConstruct"/> for how each one changes what the content means.
/// </remarks>
public enum AtomTextConstructType
{
    /// <summary>
    /// No <c>type</c> attribute was present. RFC 4287 §3.1.1 makes this equivalent to <see cref="Text"/>, and no attribute is written on save.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The content is HTML, XML-escaped in the element's character data, and suitable to appear inside an HTML <c>div</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "HTML", AlternateValue = "html")]
    Html = 1,

    /// <summary>
    /// The content is plain text: any markup in it represents the characters themselves, not markup, and must not be rendered as such.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Text", AlternateValue = "text")]
    Text = 2,

    /// <summary>
    /// The content is a single XHTML <c>div</c> whose <i>inner</i> markup is the content — the div itself is a wrapper and is not part of it.
    /// </summary>
    [EnumerationMetadata(DisplayName = "XHTML", AlternateValue = "xhtml")]
    Xhtml = 3
}