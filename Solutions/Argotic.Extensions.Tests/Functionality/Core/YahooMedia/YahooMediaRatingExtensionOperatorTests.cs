using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaRatingExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaRating>
{
    protected override YahooMediaRating CreateInstance()
        => new() { Content = "nonadult" };

    protected override YahooMediaRating CreateLesserInstance()
        => new() { Content = "adult" };

    protected override YahooMediaRating CreateGreaterInstance()
        => new() { Content = "simple" };
}
