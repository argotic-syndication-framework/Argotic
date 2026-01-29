using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssChannelExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssChannel>
{
    protected override RssChannel CreateInstance()
        => new(new Uri("http://example.com/b"), "Channel B", "Description B");

    protected override RssChannel CreateLesserInstance()
        => new(new Uri("http://example.com/a"), "Channel A", "Description A");

    protected override RssChannel CreateGreaterInstance()
        => new(new Uri("http://example.com/c"), "Channel C", "Description C");
}