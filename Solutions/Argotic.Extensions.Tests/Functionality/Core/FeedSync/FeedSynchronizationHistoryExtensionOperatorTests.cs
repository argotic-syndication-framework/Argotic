using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationHistoryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<FeedSynchronizationHistory>
{
    protected override FeedSynchronizationHistory CreateInstance()
        => new() { Sequence = 2, By = "user-b" };

    protected override FeedSynchronizationHistory CreateLesserInstance()
        => new() { Sequence = 1, By = "user-a" };

    protected override FeedSynchronizationHistory CreateGreaterInstance()
        => new() { Sequence = 3, By = "user-c" };
}
