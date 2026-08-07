using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.Pingback;

/// <summary>
/// Covers <c>PingbackSyndicationExtension</c>, the module that names the XML-RPC server an item accepts
/// pingbacks at and the resource being pinged.
/// </summary>
[TestClass]
public class PingbackSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:pingback=""http://madskills.com/public/xml/rss/module/pingback/""";

    private readonly string toStringText = "<server xmlns=\"http://madskills.com/public/xml/rss/module/pingback/\">http://www.example.com/xmlrpc.php</server>" + Environment.NewLine +
                                           "<target xmlns=\"http://madskills.com/public/xml/rss/module/pingback/\">http://www.example.com/post/1</target>";

    private const string StrExtXml = "<pingback:server>http://www.example.com/xmlrpc.php</pingback:server>"
                                     + "<pingback:target>http://www.example.com/post/1</pingback:target>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor produces a non-null <c>PingbackSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void PingbackSyndicationExtensionConstructorTest()
    {
        PingbackSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<PingbackSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions built from the same server and target compare equal, so <c>CompareTo</c> returns
    /// <c>0</c>.
    /// </summary>
    [TestMethod]
    public void PingbackCompareToTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        PingbackSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same server and target are equal through the
    /// <c>object</c> overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void PingbackEqualsTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// A populated extension hashes to something other than <c>0</c>.
    /// </summary>
    /// <remarks>
    ///     Weaker than the contract its siblings assert — that equal objects agree on their hash code —
    ///     and deliberately so: this pins only that hashing a populated extension completes and does not
    ///     land on the default.
    /// </remarks>
    [TestMethod]
    public void PingbackGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        PingbackSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// A feed carrying <c>pingback:server</c> and <c>pingback:target</c> parses without throwing.
    /// </summary>
    /// <remarks>
    ///     Nothing about the parsed values is asserted here; <c>PingbackFullTest</c> is what checks that
    ///     the extension was attached and <c>PingbackContextTest</c> what it holds.
    /// </remarks>
    [TestMethod]
    public void PingbackLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    /// <summary>
    /// Saving a feed with the extension attached writes <c>pingback:server</c> before
    /// <c>pingback:target</c>, with the namespace declared on the <c>rss</c> element.
    /// </summary>
    [TestMethod]
    public void PingbackCreateXmlTest()
    {
        PingbackSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// An item carrying the pingback elements is found again after the feed is parsed, by both the
    /// generic lookup and the <c>MatchByType</c> predicate.
    /// </summary>
    [TestMethod]
    public void PingbackFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        PingbackSyndicationExtension? itemExtension = item.FindExtension<PingbackSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(PingbackSyndicationExtension.MatchByType) as PingbackSyndicationExtension)
            .ShouldBeOfType<PingbackSyndicationExtension>();
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>PingbackSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void PingbackMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = PingbackSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders <c>server</c> and then <c>target</c> on separate lines, each declaring
    /// the pingback namespace as its default rather than carrying the prefix.
    /// </summary>
    [TestMethod]
    public void PingbackToStringTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same two elements as
    /// <c>ToString</c>, without the line break between them.
    /// </summary>
    [TestMethod]
    public void PingbackWriteToTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions naming different servers are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void PingbackOpEqualityTestFailure()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions naming the same server and target are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void PingbackOpEqualityTestSuccess()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose server is <c>xmlrpc.php</c> sorts above the one whose server is
    /// <c>other-xmlrpc.php</c>, and the reverse comparison agrees.
    /// </summary>
    [TestMethod]
    public void PingbackOpGreaterThanTest()
    {
        // Ordering is decided by the first member that differs: extension 1's server path ("xmlrpc.php")
        // sorts after extension 2's ("other-xmlrpc.php"), so extension 1 is the greater of the two.
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        (first > second).ShouldBeTrue();
        (second > first).ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions naming different servers are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void PingbackOpInequalityTest()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> agrees with <c>&gt;</c>: the <c>other-xmlrpc.php</c> extension is the lesser of the
    /// two, in both directions.
    /// </summary>
    [TestMethod]
    public void PingbackOpLessThanTest()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        (first < second).ShouldBeFalse();
        (second < first).ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the server and target the extension was built from.
    /// </summary>
    [TestMethod]
    public void PingbackContextTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        PingbackSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Server.ShouldBe(new Uri("http://www.example.com/xmlrpc.php"));
        context.Target.ShouldBe(new Uri("http://www.example.com/post/1"));
    }

    /// <summary>
    /// An item that never named a server does not save with an empty <c>pingback:server</c> — which
    /// would name an XML-RPC endpoint of the empty string.
    /// </summary>
    [TestMethod]
    public void PingbackWriteToOmitsAServerThatWasNeverThere()
    {
        // Arrange
        PingbackSyndicationExtension target = new()
        {
            Context = { Target = new Uri("http://www.example.com/post/1") }
        };

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("target", Case.Sensitive);
        actual.ShouldNotContain("server", Case.Sensitive);
    }

    /// <summary>
    /// The same in the other direction — a server alone saves without an empty <c>pingback:target</c>.
    /// </summary>
    [TestMethod]
    public void PingbackWriteToOmitsATargetThatWasNeverThere()
    {
        // Arrange
        PingbackSyndicationExtension target = new()
        {
            Context = { Server = new Uri("http://www.example.com/xmlrpc.php") }
        };

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("server", Case.Sensitive);
        actual.ShouldNotContain("target", Case.Sensitive);
    }

    /// <summary>
    /// The <c>pingback:about</c> loop three lines below writes only what exists — the shape the two
    /// elements above are measured against.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation: it passes both before and after. It is here because it is
    ///     the counter-example that makes the unconditional writes above a defect rather than a choice.
    /// </remarks>
    [TestMethod]
    public void PingbackWriteToOmitsAboutsWhenThereAreNone()
    {
        // Arrange
        PingbackSyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotContain("about", Case.Sensitive);
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, whose server is
    /// <c>http://www.example.com/xmlrpc.php</c>.
    /// </summary>
    /// <returns>An extension carrying a server and a target.</returns>
    private static PingbackSyndicationExtension CreateExtension1()
    {
        PingbackSyndicationExtension ext = new()
        {
            Context =
            {
                Server = new Uri("http://www.example.com/xmlrpc.php"),
                Target = new Uri("http://www.example.com/post/1")
            }
        };

        return ext;
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, whose server is
    /// <c>http://www.example.com/other-xmlrpc.php</c>.
    /// </summary>
    /// <returns>An extension carrying a server and a target.</returns>
    private static PingbackSyndicationExtension CreateExtension2()
    {
        PingbackSyndicationExtension ext = new()
        {
            Context =
            {
                Server = new Uri("http://www.example.com/other-xmlrpc.php"),
                Target = new Uri("http://www.example.com/post/2")
            }
        };

        return ext;
    }
}