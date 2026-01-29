using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssCategoryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssCategory>
{
    protected override RssCategory CreateInstance()
        => new() { Value = "CategoryB" };

    protected override RssCategory CreateLesserInstance()
        => new() { Value = "CategoryA" };

    protected override RssCategory CreateGreaterInstance()
        => new() { Value = "CategoryC" };
}