using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssItemExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssItem>
{
    protected override RssItem CreateInstance()
        => new() { Title = "Item B" };

    protected override RssItem CreateLesserInstance()
        => new() { Title = "Item A" };

    protected override RssItem CreateGreaterInstance()
        => new() { Title = "Item C" };
}
