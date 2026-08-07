using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.Rsd;

/// <summary>
/// Advertises a blogging API endpoint with <see cref="RsdApplicationInterface"/>.
/// </summary>
internal static class RsdApplicationInterfaceExample
{
    /// <summary>
    /// Builds the containing <see cref="RsdDocument"/> and prints the <see cref="RsdApplicationInterface"/> it holds.
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