using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

[TestClass]
public class BlogMLTextConstructExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<BlogMLTextConstruct>
{
    protected override BlogMLTextConstruct CreateInstance()
        => new() { Content = "Text B" };

    protected override BlogMLTextConstruct CreateLesserInstance()
        => new() { Content = "Text A" };

    protected override BlogMLTextConstruct CreateGreaterInstance()
        => new() { Content = "Text C" };
}