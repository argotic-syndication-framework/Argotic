using Argotic.Common;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class TrackbackDiscoveryMetadataExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<TrackbackDiscoveryMetadata>
{
    protected override TrackbackDiscoveryMetadata CreateInstance()
        => new()
        {
            About = new Uri("http://example.com/post/2"),
            Identifier = new Uri("http://example.com/id/2"),
            PingUrl = new Uri("http://example.com/trackback/2")
        };

    protected override TrackbackDiscoveryMetadata CreateLesserInstance()
        => new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/id/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

    protected override TrackbackDiscoveryMetadata CreateGreaterInstance()
        => new()
        {
            About = new Uri("http://example.com/post/3"),
            Identifier = new Uri("http://example.com/id/3"),
            PingUrl = new Uri("http://example.com/trackback/3")
        };
}