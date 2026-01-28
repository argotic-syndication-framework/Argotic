using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaContentExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaContent>
{
    protected override YahooMediaContent CreateInstance()
        => new() { Url = new Uri("http://example.com/media-b.mp4") };

    protected override YahooMediaContent CreateLesserInstance()
        => new() { Url = new Uri("http://example.com/media-a.mp4") };

    protected override YahooMediaContent CreateGreaterInstance()
        => new() { Url = new Uri("http://example.com/media-c.mp4") };
}
