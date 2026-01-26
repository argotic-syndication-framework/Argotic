namespace Argotic.Extensions.Tests;

using Argotic.Extensions.Core;
using Argotic.Syndication;
using System;
using System.IO;
using System.Linq;
using System.Xml;

/// <summary>
///This is a test class for ITunesSyndicationExtensionTest and is intended
///to contain all ITunesSyndicationExtensionTest Unit Tests
///</summary>
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

    public TestContext TestContext { get; set; }

    /// <summary>
    ///A test for ITunesSyndicationExtension Constructor
    ///</summary>
    [TestMethod]
    public void ITunesSyndicationExtensionConstructorTest()
    {
        ITunesSyndicationExtension target = new ITunesSyndicationExtension();
        Assert.IsNotNull(target);
        Assert.IsInstanceOfType<ITunesSyndicationExtension>(target);
    }

    /// <summary>
    ///A test for CompareTo
    ///</summary>
    [TestMethod]
    public void ITunesCompareToTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int expected = 0;
        int actual = target.CompareTo(obj);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ConvertDecimalToDegreesMinutesSeconds
    ///</summary>
    [TestMethod]
    public void ITunesExplicitMaterialAsStringTest()
    {
        ITunesExplicitMaterial value = ITunesExplicitMaterial.Clean;
        string expected = "clean";
        string actual = ITunesSyndicationExtension.ExplicitMaterialAsString(value);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ConvertDegreesMinutesSecondsToDecimal
    ///</summary>
    [TestMethod]
    public void ITunesExplicitMaterialByNameTest()
    {
        ITunesExplicitMaterial expected = ITunesExplicitMaterial.Clean;
        ITunesExplicitMaterial actual = ITunesSyndicationExtension.ExplicitMaterialByName("clean");
        Assert.AreEqual((double)expected, (double)actual, 3e-6);
    }

    /// <summary>
    ///A test for Equals
    ///</summary>
    [TestMethod]
    public void ITunesEqualsTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool expected = true;
        bool actual = target.Equals(obj);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for GetHashCode
    ///</summary>
    [TestMethod, Ignore]
    public void ITunesGetHashCodeTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        int expected = -765758449;
        int actual = target.GetHashCode();
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for Load
    ///</summary>
    [TestMethod, Ignore]
    public void ITunesLoadTest()
    {
        ITunesSyndicationExtension target = new ITunesSyndicationExtension(); // TODO: Initialize to an appropriate value
        NameTable nt = new NameTable();
        XmlNamespaceManager ns = new XmlNamespaceManager(nt);
        XmlParserContext xpc = new XmlParserContext(nt, ns, "US-en", XmlSpace.Default);
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc);
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void ITunesCreateXmlTest()
    {
        ITunesSyndicationExtension itunes = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(itunes);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    public void ITunesFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null);
        RssFeed feed = new RssFeed();
        feed.Load(reader);

        //				 Assert.IsTrue(feed.Channel.HasExtensions);
        //				 Assert.IsInstanceOfType(feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension,
        //						 typeof(ITunesSyndicationExtension));

        Assert.AreEqual(1, feed.Channel.Items.Count());
        RssItem item = feed.Channel.Items.Single();
        Assert.IsTrue(item.HasExtensions);
        ITunesSyndicationExtension itemExtension = item.FindExtension<ITunesSyndicationExtension>();
        Assert.IsNotNull(itemExtension);
        Assert.IsInstanceOfType(
            item.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension,
            typeof(ITunesSyndicationExtension));
    }

    /// <summary>
    ///A test for MatchByType
    ///</summary>
    [TestMethod]
    public void ITunesMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool expected = true;
        bool actual = ITunesSyndicationExtension.MatchByType(extension);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ToString
    ///</summary>
    [TestMethod]
    public void ITunesToStringTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        string expected = nycText;
        string actual = target.ToString();
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for WriteTo
    ///</summary>
    [TestMethod]
    public void ITunesWriteToTest()
    {
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = new XmlTextWriter(sw);
        ITunesSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        string output = sw.ToString();
        Assert.AreEqual(nycText.Replace(Environment.NewLine + "  ", "").Replace(Environment.NewLine, ""), output.Replace(Environment.NewLine, ""));
    }

    /// <summary>
    ///A test for op_Equality
    ///</summary>
    [TestMethod]
    public void ITunesOpEqualityTestFailure()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool expected = false;
        bool actual = (first == second);
        Assert.AreEqual(expected, actual);
    }

    public void ITunesOpEqualityTestSuccess()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension1();
        bool expected = true;
        bool actual = (first == second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_GreaterThan
    ///</summary>
    [TestMethod]
    public void ITunesOpGreaterThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool expected = false;
        bool actual = false;
        actual = (first > second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_Inequality
    ///</summary>
    [TestMethod]
    public void ITunesOpInequalityTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool expected = true;
        bool actual = (first != second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_LessThan
    ///</summary>
    [TestMethod]
    public void ITunesOpLessThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool expected = true;
        bool actual = (first < second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for Context
    ///</summary>
    [TestMethod, Ignore]
    public void ITunesContextTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        ITunesSyndicationExtensionContext expected = CreateContext1();
        ITunesSyndicationExtensionContext actual =
            //			target.Context = expected;
            target.Context;
        bool b = actual.Equals(expected);
        Assert.AreEqual(expected, actual);
        Assert.Inconclusive("Verify the correctness of this test method.");
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
        nyc.Context.NewFeedUrl = null;
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
        nyc.Context.NewFeedUrl = null;
        nyc.Context.Owner = new ITunesOwner("owner@newstar.com", "NewStar's Friend's Uncle");
        nyc.Context.Subtitle = "That song you will like.";
        nyc.Context.Summary = "Better than that other song.";
        return nyc;
    }

    public static ITunesSyndicationExtensionContext CreateContext1()
    {
        ITunesSyndicationExtensionContext nyc = new ITunesSyndicationExtensionContext();
        //nyc.Latitude = 40;
        //nyc.Longitude = -74;
        return nyc;
    }
}