using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssGuidExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssGuid>
{
    protected override RssGuid CreateInstance()
        => new() { Value = "guid-b" };

    protected override RssGuid CreateLesserInstance()
        => new() { Value = "guid-a" };

    protected override RssGuid CreateGreaterInstance()
        => new() { Value = "guid-c" };
}