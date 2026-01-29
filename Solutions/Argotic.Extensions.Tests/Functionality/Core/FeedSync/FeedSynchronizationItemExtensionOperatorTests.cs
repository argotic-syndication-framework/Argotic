using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationItemExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<FeedSynchronizationItem>
{
    protected override FeedSynchronizationItem CreateInstance()
        => new() { Id = "item-b", Updates = 2 };

    protected override FeedSynchronizationItem CreateLesserInstance()
        => new() { Id = "item-a", Updates = 1 };

    protected override FeedSynchronizationItem CreateGreaterInstance()
        => new() { Id = "item-c", Updates = 3 };
}