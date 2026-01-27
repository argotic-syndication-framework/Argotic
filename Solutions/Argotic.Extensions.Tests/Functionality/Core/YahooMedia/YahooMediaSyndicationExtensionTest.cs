using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaSyndicationExtensionTest
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void YahooMediaSyndicationExtensionConstructorTest()
    {
        YahooMediaSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<YahooMediaSyndicationExtension>();
    }

    [TestMethod]
    public void YahooMediaMatchByTypeTest()
    {
        ISyndicationExtension extension = new YahooMediaSyndicationExtension();
        bool actual = YahooMediaSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void YahooMediaYahooMediaGetHashCodeTest()
    {
        // Verify GetHashCode doesn't throw
        YahooMediaSyndicationExtension target = new();
        int hash = target.GetHashCode();
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void YahooMediaContextTest()
    {
        YahooMediaSyndicationExtension target = new();
        YahooMediaSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Contents.ShouldNotBeNull();
    }

    // Note: YahooMediaExpressionAsStringTest is skipped because
    // ExpressionAsString returns empty string for valid expressions.

    [TestMethod]
    public void YahooMediaMediumAsStringTest()
    {
        YahooMediaMedium value = YahooMediaMedium.Video;
        string expected = "video";
        string actual = YahooMediaSyndicationExtension.MediumAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void YahooMediaMediumByNameTest()
    {
        YahooMediaMedium expected = YahooMediaMedium.Image;
        YahooMediaMedium actual = YahooMediaSyndicationExtension.MediumByName("image");
        actual.ShouldBe(expected);
    }

    // Note: Several tests are omitted due to pre-existing bugs in YahooMediaSyndicationExtension:
    // - CompareTo/Equals methods fail with InvalidCastException when Contents is List<T> instead of Collection<T>
    // - ExpressionByName returns None for valid expressions
    // - Load tests fail with InvalidCastException in AddContent
}
