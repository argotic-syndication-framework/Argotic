using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationSharingInformationExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<FeedSynchronizationSharingInformation>
{
    protected override FeedSynchronizationSharingInformation CreateInstance()
        => new() { Since = "2024-02-01" };

    protected override FeedSynchronizationSharingInformation CreateLesserInstance()
        => new() { Since = "2024-01-01" };

    protected override FeedSynchronizationSharingInformation CreateGreaterInstance()
        => new() { Since = "2024-03-01" };
}