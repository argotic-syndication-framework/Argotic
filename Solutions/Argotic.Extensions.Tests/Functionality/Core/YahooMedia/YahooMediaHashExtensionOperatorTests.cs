using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaHashExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaHash>
{
    protected override YahooMediaHash CreateInstance()
        => new() { Value = "hashB123" };

    protected override YahooMediaHash CreateLesserInstance()
        => new() { Value = "hashA123" };

    protected override YahooMediaHash CreateGreaterInstance()
        => new() { Value = "hashC123" };
}