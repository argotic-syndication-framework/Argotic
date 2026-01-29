using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaGroupExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaGroup>
{
    protected override YahooMediaGroup CreateInstance()
    {
        var group = new YahooMediaGroup();
        group.Contents.Add(new YahooMediaContent { Url = new Uri("http://example.com/b.mp4") });
        return group;
    }

    protected override YahooMediaGroup CreateLesserInstance()
    {
        var group = new YahooMediaGroup();
        group.Contents.Add(new YahooMediaContent { Url = new Uri("http://example.com/a.mp4") });
        return group;
    }

    protected override YahooMediaGroup CreateGreaterInstance()
    {
        var group = new YahooMediaGroup();
        group.Contents.Add(new YahooMediaContent { Url = new Uri("http://example.com/c.mp4") });
        return group;
    }
}