using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedHistory;

[TestClass]
public class FeedHistoryLinkRelationExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<FeedHistoryLinkRelation>
{
    protected override FeedHistoryLinkRelation CreateInstance()
        => new() { Uri = new Uri("http://example.com/feed/b") };

    protected override FeedHistoryLinkRelation CreateLesserInstance()
        => new() { Uri = new Uri("http://example.com/feed/a") };

    protected override FeedHistoryLinkRelation CreateGreaterInstance()
        => new() { Uri = new Uri("http://example.com/feed/c") };
}