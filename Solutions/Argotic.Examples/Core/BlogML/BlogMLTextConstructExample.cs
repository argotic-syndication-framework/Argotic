using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.BlogML;

/// <summary>
/// Contains the code examples for the <see cref="BlogMLTextConstruct"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="BlogMLTextConstruct"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class BlogMLTextConstructExample
{
    /// <summary>
    /// Provides example code for the BlogMLTextConstruct class.
    /// </summary>
    public static void ClassExample()
    {
        BlogMLTextConstruct textConstruct = new("<p>This is <b>HTML encoded</b> content.</p>", BlogMLContentType.Html);

        ExampleOutput.ShowBlogMLTextConstruct(textConstruct);
    }

    /// <summary>
    /// Provides example code for the BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType) method
    /// </summary>
    public static void ConstructTypeAsStringExample()
    {
        string contentType = BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType.Html);    // html

        if (string.Equals(contentType, "html", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    /// <summary>
    /// Provides example code for the BlogMLTextConstruct.ConstructTypeByName(string) method
    /// </summary>
    public static void ConstructTypeByNameExample()
    {
        BlogMLContentType contentType = BlogMLTextConstruct.ConstructTypeByName("html");

        if (contentType == BlogMLContentType.Html)
        {
        }
    }
}