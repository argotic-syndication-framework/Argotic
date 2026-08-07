using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.AtomPublishing;

/// <summary>
/// Covers the Atom Publishing Protocol <c>app:control</c> extension: what a new instance holds, the
/// <c>xml:base</c>, <c>xml:lang</c> and draft flag its context carries, how those reach XML through
/// <c>WriteTo</c> and <c>ToString</c>, and its comparison, equality and ordering contracts.
/// </summary>
[TestClass]
public class AtomPublishingControlSyndicationExtensionTest
{
    private readonly string namespc = @"xmlns:app=""http://www.w3.org/2007/app""";
    private readonly string nycText = $@"<control xml:base=""http://www.example.com/control.html"" xml:lang=""en-US"" xmlns=""http://www.w3.org/2007/app"">{Environment.NewLine}  <draft>yes</draft>{Environment.NewLine}</control>";
    private readonly string strExtXml = """<app:control xml:base="http://www.example.com/control.html" xml:lang="en-US"><app:draft>yes</app:draft></app:control>""";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor yields an instance of the control extension type.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlSyndicationExtensionConstructorTest()
    {
        AtomPublishingControlSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<AtomPublishingControlSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlCompareToTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        AtomPublishingControlSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlEqualsTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// A hash code is stable across calls, and equal extensions hash equally.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        AtomPublishingControlSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// An RSS 2.0 feed carrying an <c>app:control</c> element parses, leaving the channel and its single item intact.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlLoadTest()
    {
        // Verify that an RSS feed containing APP extension XML can be loaded without errors
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Basic feed structure should be intact
        feed.Channel.ShouldNotBeNull();
        feed.Channel.Items.Count.ShouldBe(1);
    }

    /// <summary>
    /// Attaching the extension to an RSS item emits <c>app:control</c> with its <c>xml:base</c> and <c>xml:lang</c> attributes and an <c>app:draft</c> child.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlCreateXmlTest()
    {
        AtomPublishingControlSyndicationExtension itunes = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(itunes).Trim();
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml).Trim();
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an instance of its own extension type.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = AtomPublishingControlSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders <c>control</c> in the app namespace with the draft flag spelled <c>yes</c> on its own indented line.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlToStringTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to an <see cref="XmlWriter"/> emits the same control element as <c>ToString</c>, once indentation is discounted.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlWriteToTest()
    {
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine + "  ", "", StringComparison.Ordinal).Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Extensions differing in base URI, draft flag and language are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlOpEqualityTestFailure()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding identical context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlOpEqualityTestSuccess()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// An extension whose base URI sorts earlier is not greater than one whose base URI sorts later.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlOpGreaterThanTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlOpInequalityTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// An extension whose base URI sorts earlier — <c>example.com</c> ahead of <c>example.net</c> — is less than the other.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlOpLessThanTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The context reports the base URI, draft flag and language it was given.
    /// </summary>
    [TestMethod]
    public void AtomPublishingControlContextTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        AtomPublishingControlSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.BaseUri.ShouldBe(new Uri("http://www.example.com/control.html"));
        context.IsDraft.ShouldBeTrue();
        context.Language!.Name.ShouldBe("en-US");
    }

    private static AtomPublishingControlSyndicationExtension CreateExtension1()
    {
        AtomPublishingControlSyndicationExtension nyc = new()
        {
            Context =
            {
                BaseUri = new Uri("http://www.example.com/control.html"),
                IsDraft = true,
                Language = new CultureInfo("en-US")
            }
        };

        return nyc;
    }

    private static AtomPublishingControlSyndicationExtension CreateExtension2()
    {
        AtomPublishingControlSyndicationExtension nyc = new()
        {
            Context =
            {
                BaseUri = new Uri("http://www.example.net/control.html"),
                IsDraft = false,
                Language = new CultureInfo("fr-CA")
            }
        };

        return nyc;
    }

    public static AtomPublishingControlSyndicationExtensionContext CreateContext1() => new();
}