using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaCategoryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaCategory>
{
    protected override YahooMediaCategory CreateInstance()
        => new() { Content = "category-b" };

    protected override YahooMediaCategory CreateLesserInstance()
        => new() { Content = "category-a" };

    protected override YahooMediaCategory CreateGreaterInstance()
        => new() { Content = "category-c" };
}