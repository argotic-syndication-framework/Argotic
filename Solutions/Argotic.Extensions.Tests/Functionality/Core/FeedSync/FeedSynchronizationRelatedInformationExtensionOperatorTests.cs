using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationRelatedInformationExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<FeedSynchronizationRelatedInformation>
{
    protected override FeedSynchronizationRelatedInformation CreateInstance()
        => new() { Link = new Uri("http://example.com/related/b") };

    protected override FeedSynchronizationRelatedInformation CreateLesserInstance()
        => new() { Link = new Uri("http://example.com/related/a") };

    protected override FeedSynchronizationRelatedInformation CreateGreaterInstance()
        => new() { Link = new Uri("http://example.com/related/c") };
}