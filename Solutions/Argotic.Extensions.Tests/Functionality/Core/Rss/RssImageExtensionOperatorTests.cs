using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssImageExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssImage>
{
    protected override RssImage CreateInstance()
        => new(new Uri("http://example.com/b.png"), "Image B", new Uri("http://example.com/b"));

    protected override RssImage CreateLesserInstance()
        => new(new Uri("http://example.com/a.png"), "Image A", new Uri("http://example.com/a"));

    protected override RssImage CreateGreaterInstance()
        => new(new Uri("http://example.com/c.png"), "Image C", new Uri("http://example.com/c"));
}
