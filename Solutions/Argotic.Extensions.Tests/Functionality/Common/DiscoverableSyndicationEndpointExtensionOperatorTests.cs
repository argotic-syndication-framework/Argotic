using Argotic.Common;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class DiscoverableSyndicationEndpointExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<DiscoverableSyndicationEndpoint>
{
    protected override DiscoverableSyndicationEndpoint CreateInstance()
        => new(new Uri("http://example.com/feed.xml"), "application/rss+xml");

    protected override DiscoverableSyndicationEndpoint CreateLesserInstance()
        => new(new Uri("http://example.com/atom.xml"), "application/atom+xml");

    protected override DiscoverableSyndicationEndpoint CreateGreaterInstance()
        => new(new Uri("http://example.com/rss.xml"), "application/rss+xml");
}
