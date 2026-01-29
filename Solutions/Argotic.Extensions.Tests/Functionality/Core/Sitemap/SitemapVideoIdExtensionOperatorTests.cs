using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapVideoIdExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapVideoId>
{
    protected override SitemapVideoId CreateInstance()
        => new() { Value = "video-id-b" };

    protected override SitemapVideoId CreateLesserInstance()
        => new() { Value = "video-id-a" };

    protected override SitemapVideoId CreateGreaterInstance()
        => new() { Value = "video-id-c" };
}