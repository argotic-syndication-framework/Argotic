using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaThumbnailExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaThumbnail>
{
    protected override YahooMediaThumbnail CreateInstance()
        => new() { Url = new Uri("http://example.com/thumb-b.jpg") };

    protected override YahooMediaThumbnail CreateLesserInstance()
        => new() { Url = new Uri("http://example.com/thumb-a.jpg") };

    protected override YahooMediaThumbnail CreateGreaterInstance()
        => new() { Url = new Uri("http://example.com/thumb-c.jpg") };
}
