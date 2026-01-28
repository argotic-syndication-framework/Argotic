using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaTextConstructExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaTextConstruct>
{
    protected override YahooMediaTextConstruct CreateInstance()
        => new() { Content = "Construct B" };

    protected override YahooMediaTextConstruct CreateLesserInstance()
        => new() { Content = "Construct A" };

    protected override YahooMediaTextConstruct CreateGreaterInstance()
        => new() { Content = "Construct C" };
}
