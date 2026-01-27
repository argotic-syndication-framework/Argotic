using System.Xml;
using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:sx=""http://feedsync.org/2007/feedsync""";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtensionConstructorTest()
    {
        FeedSynchronizationSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedSynchronizationSyndicationExtension>();
    }

    [TestMethod]
    public void FeedSynchronizationCompareToTest()
    {
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationEqualsTest()
    {
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSyncGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = FeedSynchronizationSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationWriteToTest()
    {
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.ShouldNotBeNull();
    }

    [TestMethod]
    public void FeedSynchronizationOpEqualityTestFailure()
    {
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpEqualityTestSuccess()
    {
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpGreaterThanTest()
    {
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpInequalityTest()
    {
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThanTest()
    {
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationContextTest()
    {
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Sharing.ShouldNotBeNull();
    }

    private static FeedSynchronizationSyndicationExtension CreateExtension1()
    {
        FeedSynchronizationSyndicationExtension ext = new();
        ext.Context.Sharing = new FeedSynchronizationSharingInformation
        {
            Since = "2010-01-01",
            Until = "2010-12-31",
            ExpiresOn = new DateTime(2011, 1, 1)
        };

        return ext;
    }

    private static FeedSynchronizationSyndicationExtension CreateExtension2()
    {
        FeedSynchronizationSyndicationExtension ext = new();
        ext.Context.Sharing = new FeedSynchronizationSharingInformation
        {
            Since = "2020-01-01",
            Until = "2020-12-31",
            ExpiresOn = new DateTime(2021, 1, 1)
        };

        return ext;
    }
}
