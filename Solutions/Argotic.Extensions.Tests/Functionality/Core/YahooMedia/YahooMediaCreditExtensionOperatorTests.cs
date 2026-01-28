using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaCreditExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaCredit>
{
    protected override YahooMediaCredit CreateInstance()
        => new() { Entity = "Credit B" };

    protected override YahooMediaCredit CreateLesserInstance()
        => new() { Entity = "Credit A" };

    protected override YahooMediaCredit CreateGreaterInstance()
        => new() { Entity = "Credit C" };
}
