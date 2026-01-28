using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaPlayerExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaPlayer>
{
    protected override YahooMediaPlayer CreateInstance()
        => new() { Url = new Uri("http://example.com/player-b") };

    protected override YahooMediaPlayer CreateLesserInstance()
        => new() { Url = new Uri("http://example.com/player-a") };

    protected override YahooMediaPlayer CreateGreaterInstance()
        => new() { Url = new Uri("http://example.com/player-c") };
}
