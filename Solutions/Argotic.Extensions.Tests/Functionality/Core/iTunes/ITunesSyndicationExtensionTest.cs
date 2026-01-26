namespace Argotic.Extensions.Tests;

using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class ITunesSyndicationExtensionTest
{
    const string namespc = """
                           xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd"
                           """;

    private readonly string nycText = "<subtitle xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">That song you like.</subtitle>" + Environment.NewLine
                                                                                                                                      + "<author xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">BigStar</author>" + Environment.NewLine
                                                                                                                                      + "<summary xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">Duh... That song you like</summary>" + Environment.NewLine
                                                                                                                                      + "<owner xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">" + Environment.NewLine
                                                                                                                                      + "  <email>owner@bigstar.com</email>" + Environment.NewLine
                                                                                                                                      + "  <name>BigStar's Guy</name>" + Environment.NewLine
                                                                                                                                      + "</owner>" + Environment.NewLine
                                                                                                                                      + "<image href=\"http://www.eexample.com/image.jpg\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />" + Environment.NewLine
                                                                                                                                      + "<duration xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">00:03:21</duration>" + Environment.NewLine
                                                                                                                                      + "<keywords xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">loud,good for parties</keywords>" + Environment.NewLine
                                                                                                                                      + "<explicit xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">clean</explicit>" + Environment.NewLine
                                                                                                                                      + "<category text=\"Rock\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />" + Environment.NewLine
                                                                                                                                      + "<category text=\"Folk\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />";

    private const string strExtXml = "<itunes:subtitle>That song you like.</itunes:subtitle><itunes:author>BigStar</itunes:author>"
                                     + "<itunes:summary>Duh... That song you like</itunes:summary><itunes:owner><itunes:email>owner@bigstar.com</itunes:email>"
                                     + "<itunes:name>BigStar's Guy</itunes:name></itunes:owner><itunes:image href=\"http://www.eexample.com/image.jpg\" />"
                                     + "<itunes:duration>00:03:21</itunes:duration><itunes:keywords>loud,good for parties</itunes:keywords><itunes:explicit>clean</itunes:explicit>"
                                     + "<itunes:category text=\"Rock\" /><itunes:category text=\"Folk\" />";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void ITunesSyndicationExtensionConstructorTest()
    {
        ITunesSyndicationExtension target = new ITunesSyndicationExtension();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<ITunesSyndicationExtension>();
    }

    [TestMethod]
    public void ITunesCompareToTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void ITunesExplicitMaterialAsStringTest()
    {
        ITunesExplicitMaterial value = ITunesExplicitMaterial.Clean;
        string expected = "clean";
        string actual = ITunesSyndicationExtension.ExplicitMaterialAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void ITunesExplicitMaterialByNameTest()
    {
        ITunesExplicitMaterial expected = ITunesExplicitMaterial.Clean;
        ITunesExplicitMaterial actual = ITunesSyndicationExtension.ExplicitMaterialByName("clean");
        ((double)actual).ShouldBe((double)expected, 3e-6);
    }

    [TestMethod]
    public void ITunesEqualsTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("GetHashCode implementation is not deterministic across runs")]
    public void ITunesGetHashCodeTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        int expected = -765758449;
        int actual = target.GetHashCode();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    [Ignore("Test requires manual verification of Load behavior")]
    public void ITunesLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void ITunesCreateXmlTest()
    {
        ITunesSyndicationExtension itunes = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(itunes);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void ITunesFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        ITunesSyndicationExtension itemExtension = item.FindExtension<ITunesSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension)
            .ShouldBeOfType<ITunesSyndicationExtension>();
    }

    [TestMethod]
    public void ITunesMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = ITunesSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void ITunesToStringTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    [TestMethod]
    public void ITunesWriteToTest()
    {
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        ITunesSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine + "  ", "").Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void ITunesOpEqualityTestFailure()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void ITunesOpEqualityTestSuccess()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void ITunesOpGreaterThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void ITunesOpInequalityTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void ITunesOpLessThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("Context equality comparison not implemented")]
    public void ITunesContextTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        ITunesSyndicationExtensionContext expected = CreateContext1();
        ITunesSyndicationExtensionContext actual = target.Context;
        actual.ShouldBe(expected);
    }

    private ITunesSyndicationExtension CreateExtension1()
    {
        ITunesSyndicationExtension nyc = new ITunesSyndicationExtension
        {
            Context =
            {
                Author = "BigStar"
            }
        };

        nyc.Context.Categories.Add(new ITunesCategory("Rock"));
        nyc.Context.Categories.Add(new ITunesCategory("Folk"));
        nyc.Context.Duration = new TimeSpan(0, 3, 21);
        nyc.Context.ExplicitMaterial = ITunesExplicitMaterial.Clean;
        nyc.Context.Image = new Uri("http://www.eexample.com/image.jpg");
        nyc.Context.IsBlocked = false;
        nyc.Context.Keywords.Add("loud");
        nyc.Context.Keywords.Add("good for parties");
        nyc.Context.Owner = new ITunesOwner("owner@bigstar.com", "BigStar's Guy");
        nyc.Context.Subtitle = "That song you like.";
        nyc.Context.Summary = "Duh... That song you like";

        return nyc;
    }

    private ITunesSyndicationExtension CreateExtension2()
    {
        ITunesSyndicationExtension nyc = new ITunesSyndicationExtension
        {
            Context =
            {
                Author = "NewStar"
            }
        };
        nyc.Context.Categories.Add(new ITunesCategory("Dance"));
        nyc.Context.Categories.Add(new ITunesCategory("Funk"));
        nyc.Context.Duration = new TimeSpan(0, 4, 32);
        nyc.Context.ExplicitMaterial = ITunesExplicitMaterial.Yes;
        nyc.Context.Image = new Uri("http://www.example.com/newimage.png");
        nyc.Context.IsBlocked = true;
        nyc.Context.Keywords.Add("loud");
        nyc.Context.Keywords.Add("offend your parents");
        nyc.Context.Owner = new ITunesOwner("owner@newstar.com", "NewStar's Friend's Uncle");
        nyc.Context.Subtitle = "That song you will like.";
        nyc.Context.Summary = "Better than that other song.";
        return nyc;
    }

    public static ITunesSyndicationExtensionContext CreateContext1()
    {
        ITunesSyndicationExtensionContext nyc = new ITunesSyndicationExtensionContext();
        return nyc;
    }
}
