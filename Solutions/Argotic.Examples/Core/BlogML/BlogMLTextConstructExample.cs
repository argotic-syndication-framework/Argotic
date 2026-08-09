using Argotic.Syndication;

namespace Argotic.Examples.Core.BlogML;

/// <summary>
/// Wraps post content in a <see cref="BlogMLTextConstruct"/>, and converts between <c>BlogMLContentType</c> and the string that appears in the XML.
/// </summary>
internal static class BlogMLTextConstructExample
{
    /// <summary>
    /// Builds a <see cref="BlogMLTextConstruct"/> holding HTML-encoded content and prints it.
    /// </summary>
    public static void ClassExample()
    {
        BlogMLTextConstruct textConstruct = new("<p>This is <b>HTML encoded</b> content.</p>", BlogMLContentType.Html);

        ExampleOutput.ShowBlogMLTextConstruct(textConstruct);
    }

    /// <summary>
    /// Converts a <c>BlogMLContentType</c> to the string that appears in the XML.
    /// </summary>
    public static void ConstructTypeAsStringExample()
    {
        string contentType = BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType.Html);    // html

        if (string.Equals(contentType, "html", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    /// <summary>
    /// Converts the string that appears in the XML back to a <c>BlogMLContentType</c>.
    /// </summary>
    public static void ConstructTypeByNameExample()
    {
        BlogMLContentType contentType = BlogMLTextConstruct.ConstructTypeByName("html");

        if (contentType == BlogMLContentType.Html)
        {
        }
    }
}