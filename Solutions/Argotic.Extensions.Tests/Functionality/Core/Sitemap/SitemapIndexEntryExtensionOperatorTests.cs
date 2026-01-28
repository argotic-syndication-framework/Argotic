using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapIndexEntryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapIndexEntry>
{
    protected override SitemapIndexEntry CreateInstance()
        => new() { Location = new Uri("http://example.com/sitemap-b.xml") };

    protected override SitemapIndexEntry CreateLesserInstance()
        => new() { Location = new Uri("http://example.com/sitemap-a.xml") };

    protected override SitemapIndexEntry CreateGreaterInstance()
        => new() { Location = new Uri("http://example.com/sitemap-c.xml") };
}
