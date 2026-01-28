using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapVideoExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapVideo>
{
    protected override SitemapVideo CreateInstance()
        => new() { Title = "Video B", ThumbnailLocation = new Uri("http://example.com/thumb-b.jpg") };

    protected override SitemapVideo CreateLesserInstance()
        => new() { Title = "Video A", ThumbnailLocation = new Uri("http://example.com/thumb-a.jpg") };

    protected override SitemapVideo CreateGreaterInstance()
        => new() { Title = "Video C", ThumbnailLocation = new Uri("http://example.com/thumb-c.jpg") };
}
