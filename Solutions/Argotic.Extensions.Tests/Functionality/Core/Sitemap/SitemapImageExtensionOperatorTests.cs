using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapImageExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapImage>
{
    protected override SitemapImage CreateInstance()
        => new(new Uri("http://example.com/image-b.png"));

    protected override SitemapImage CreateLesserInstance()
        => new(new Uri("http://example.com/image-a.png"));

    protected override SitemapImage CreateGreaterInstance()
        => new(new Uri("http://example.com/image-c.png"));
}
