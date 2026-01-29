using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapUrlExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapUrl>
{
    protected override SitemapUrl CreateInstance()
        => new() { Location = new Uri("http://example.com/page-b") };

    protected override SitemapUrl CreateLesserInstance()
        => new() { Location = new Uri("http://example.com/page-a") };

    protected override SitemapUrl CreateGreaterInstance()
        => new() { Location = new Uri("http://example.com/page-c") };
}