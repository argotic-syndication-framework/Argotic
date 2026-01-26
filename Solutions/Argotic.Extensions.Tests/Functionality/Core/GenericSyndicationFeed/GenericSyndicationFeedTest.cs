namespace Argotic.Extensions.Tests;

using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class GenericSyndicationFeedTest
{

    public TestContext? TestContext { get; set; }

    [TestMethod, TestCategory("fix-39")]
    public void TestCustomXmlNamespace()
    {
        string xml = @"<rss xmlns:app=""http:/example.com"" version=""2.0""></rss>";

        GenericSyndicationFeed feed = new();

        feed.Load(xml);
        feed.ShouldNotBeSameAs(new GenericSyndicationFeed());
    }
}
