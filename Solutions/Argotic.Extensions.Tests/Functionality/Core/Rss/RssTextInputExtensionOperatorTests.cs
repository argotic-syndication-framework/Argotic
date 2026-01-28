using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssTextInputExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssTextInput>
{
    protected override RssTextInput CreateInstance()
        => new() { Title = "Input B" };

    protected override RssTextInput CreateLesserInstance()
        => new() { Title = "Input A" };

    protected override RssTextInput CreateGreaterInstance()
        => new() { Title = "Input C" };
}
