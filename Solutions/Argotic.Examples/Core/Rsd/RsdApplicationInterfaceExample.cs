using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.Rsd;

/// <summary>
/// Contains the code examples for the <see cref="RsdApplicationInterface"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RsdApplicationInterface"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class RsdApplicationInterfaceExample
{
    /// <summary>
    /// Provides example code for the RsdApplicationInterface class.
    /// </summary>
    public static void ClassExample()
    {
        RsdDocument document = new()
        {
            EngineName = "Blog Munging CMS",
            EngineLink = new Uri("http://www.blogmunging.com/"),
            Homepage = new Uri("http://www.userdomain.com/")
        };

        //  Identify supported services using well known names
        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

        RsdApplicationInterface conversantApi = new("Conversant", new Uri("http://example.com/xml/rpc/url"), false, string.Empty)
        {
            Documentation = new Uri("http://www.conversant.com/docs/api/"),
            Notes = "Additional explanation here."
        };
        conversantApi.Settings.Add("service-specific-setting", "a value");
        conversantApi.Settings.Add("another-setting", "another value");
        document.Interfaces.Add(conversantApi);

        ExampleOutput.ShowRsdApplicationInterface(conversantApi);
    }
}