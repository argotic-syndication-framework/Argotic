using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

[TestClass]
public class GenericSyndicationItemExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<GenericSyndicationItem>
{
    protected override GenericSyndicationItem CreateInstance()
        => new(new RssItem { Title = "Item B" });

    protected override GenericSyndicationItem CreateLesserInstance()
        => new(new RssItem { Title = "Item A" });

    protected override GenericSyndicationItem CreateGreaterInstance()
        => new(new RssItem { Title = "Item C" });
}