using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

[TestClass]
public class GenericSyndicationCategoryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<GenericSyndicationCategory>
{
    protected override GenericSyndicationCategory CreateInstance()
        => new("category-b", "scheme-b");

    protected override GenericSyndicationCategory CreateLesserInstance()
        => new("category-a", "scheme-a");

    protected override GenericSyndicationCategory CreateGreaterInstance()
        => new("category-c", "scheme-c");
}
