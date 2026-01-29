using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssSourceExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssSource>
{
    protected override RssSource CreateInstance()
        => new() { Title = "Source B" };

    protected override RssSource CreateLesserInstance()
        => new() { Title = "Source A" };

    protected override RssSource CreateGreaterInstance()
        => new() { Title = "Source C" };
}