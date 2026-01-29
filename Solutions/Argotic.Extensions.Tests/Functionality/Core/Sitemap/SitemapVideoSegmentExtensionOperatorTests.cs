using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapVideoSegmentExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapVideoSegment>
{
    protected override SitemapVideoSegment CreateInstance()
        => new() { Location = new Uri("http://example.com/segment-b.mp4") };

    protected override SitemapVideoSegment CreateLesserInstance()
        => new() { Location = new Uri("http://example.com/segment-a.mp4") };

    protected override SitemapVideoSegment CreateGreaterInstance()
        => new() { Location = new Uri("http://example.com/segment-c.mp4") };
}