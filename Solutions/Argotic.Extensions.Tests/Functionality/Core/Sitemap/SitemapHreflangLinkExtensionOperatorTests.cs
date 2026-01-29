using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapHreflangLinkExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapHreflangLink>
{
    protected override SitemapHreflangLink CreateInstance()
        => new() { Hreflang = "en-US", Href = new Uri("http://example.com/en-us") };

    protected override SitemapHreflangLink CreateLesserInstance()
        => new() { Hreflang = "de-DE", Href = new Uri("http://example.com/de-de") };

    protected override SitemapHreflangLink CreateGreaterInstance()
        => new() { Hreflang = "fr-FR", Href = new Uri("http://example.com/fr-fr") };
}