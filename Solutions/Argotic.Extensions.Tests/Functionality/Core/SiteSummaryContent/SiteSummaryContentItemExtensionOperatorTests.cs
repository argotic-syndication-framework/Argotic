using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummaryContent;

[TestClass]
public class SiteSummaryContentItemExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SiteSummaryContentItem>
{
    protected override SiteSummaryContentItem CreateInstance()
        => new() { Content = "Content B" };

    protected override SiteSummaryContentItem CreateLesserInstance()
        => new() { Content = "Content A" };

    protected override SiteSummaryContentItem CreateGreaterInstance()
        => new() { Content = "Content C" };
}
