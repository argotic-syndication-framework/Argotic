using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaTextExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaText>
{
    protected override YahooMediaText CreateInstance()
        => new() { Content = "Text B" };

    protected override YahooMediaText CreateLesserInstance()
        => new() { Content = "Text A" };

    protected override YahooMediaText CreateGreaterInstance()
        => new() { Content = "Text C" };
}