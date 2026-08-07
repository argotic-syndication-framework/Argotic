using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedRank;

/// <summary>
/// Covers <c>FeedRankSyndicationExtension</c>, the Atom ranking extension whose single <c>re:rank</c>
/// element carries a value alongside a scheme, a domain and a label.
/// </summary>
[TestClass]
public class FeedRankSyndicationExtensionTest
{
    private const string namespc = @"xmlns:re=""http://purl.org/atompub/rank/1.0""";
    private const string nycText = """<rank scheme="http://example.com/scheme.txt" domain="http://example.com/" label="Title" xmlns="http://purl.org/atompub/rank/1.0">1.0</rank>""";
    private const string writeToText = """<rank scheme="http://example.com/scheme.txt" domain="http://example.com/" label="Title" xmlns="http://purl.org/atompub/rank/1.0">1.0</rank>""";
    private const string strExtXml = """<re:rank scheme="http://example.com/scheme.txt" domain="http://example.com/" label="Title">1.0</re:rank>""";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor produces a non-null <c>FeedRankSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankSyndicationExtensionConstructorTest()
    {
        FeedRankSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedRankSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions built from the same scheme, domain, label and value compare equal, so
    /// <c>CompareTo</c> returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankCompareToTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        FeedRankSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same rank are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankEqualsTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The hash code is stable across repeated calls, and two equal extensions agree on it.
    /// </summary>
    [TestMethod]
    public void FeedRankGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        FeedRankSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        FeedRankSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// Saving a feed with the extension attached writes a single <c>re:rank</c> element carrying
    /// <c>scheme</c>, <c>domain</c> and <c>label</c>, none of them prefixed.
    /// </summary>
    [TestMethod]
    public void FeedRankCreateXmlTest()
    {
        FeedRankSyndicationExtension re = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(re);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the scheme, domain, label and value the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as FeedRankSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes. This file is the reason the pattern matters: the extension's write path
    ///     qualified <c>scheme</c> and <c>domain</c> into the extension namespace for years, and a test
    ///     that only asked whether an extension came back could not see it.
    /// </remarks>
    [TestMethod]
    public void FeedRankFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        FeedRankSyndicationExtension byType = item.FindExtension<FeedRankSyndicationExtension>().ShouldNotBeNull();
        FeedRankSyndicationExtension byPredicate = item
            .FindExtension(FeedRankSyndicationExtension.MatchByType)
            .ShouldBeOfType<FeedRankSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Scheme.ShouldBe(new Uri("http://example.com/scheme.txt"));
        byPredicate.Context.Domain.ShouldBe(new Uri("http://example.com/"));
        byPredicate.Context.Label.ShouldBe("Title");
        byPredicate.Context.Value.ShouldBe(1.0m);
    }

    /// <summary>
    /// A <c>re:rank</c> whose <c>scheme</c> and <c>domain</c> are unprefixed — the conformant spelling,
    /// and the one <c>SampleData/RssFeedWithExtensions.xml</c> uses — is read in full.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation: an unprefixed attribute lives in the no-namespace partition
    ///     whatever the element's own namespace is, so this passes both before and after the write path
    ///     is corrected. It is here to record that the read side was never the broken half.
    /// </remarks>
    [TestMethod]
    public void FeedRankUnprefixedAttributesAreRead()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(
            namespc,
            """<re:rank scheme="http://example.com/scheme.txt" domain="http://example.com/" label="Title">1.0</re:rank>""");

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        FeedRankSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<FeedRankSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Scheme.ShouldBe(new Uri("http://example.com/scheme.txt"));
        itemExtension.Context.Domain.ShouldBe(new Uri("http://example.com/"));
        itemExtension.Context.Label.ShouldBe("Title");
        itemExtension.Context.Value.ShouldBe(1.0m);
    }

    /// <summary>
    /// All three attributes are written unqualified, which is the only partition an unprefixed attribute
    /// can occupy and the one the loader reads from.
    /// </summary>
    /// <remarks>
    ///     The asymmetry this replaces was the whole proof. <c>scheme</c> and <c>domain</c> were written
    ///     into the extension namespace and came back with a generated <c>p1</c> prefix; <c>label</c>,
    ///     written unqualified three lines later, did not — and <c>label</c> was the only one of the
    ///     three that round-tripped.
    /// </remarks>
    [TestMethod]
    public void FeedRankWritesEveryAttributeUnqualified()
    {
        FeedRankSyndicationExtension target = CreateExtension1();

        string actual = target.ToString();

        actual.ShouldContain("scheme=", Case.Sensitive);
        actual.ShouldContain("domain=", Case.Sensitive);
        actual.ShouldContain("label=", Case.Sensitive);
        actual.ShouldNotContain("p1:", Case.Sensitive);
    }

    /// <summary>
    /// A rank that names no scheme and no domain writes neither attribute, rather than asserting an
    /// empty one.
    /// </summary>
    /// <remarks>
    ///     <c>scheme</c> used to be written unconditionally, so an unset one shipped as
    ///     <c>scheme=""</c> — well-formed, and a same-document reference when read as a URI, which is
    ///     an assertion the publisher never made. <c>Load</c> rejects an empty value, so it could not
    ///     survive a round trip either. <c>domain</c> was already guarded and is pinned here beside it.
    /// </remarks>
    [TestMethod]
    public void FeedRankOmitsTheSchemeAndDomainAttributesWhenNeitherWasSet()
    {
        FeedRankSyndicationExtension target = CreateExtensionWithoutSchemeOrDomain();

        string actual = target.ToString();

        actual.ShouldNotContain("scheme=", Case.Sensitive);
        actual.ShouldNotContain("domain=", Case.Sensitive);
        actual.ShouldContain(@"label=""Title""", Case.Sensitive);
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>FeedRankSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = FeedRankSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders the rank as a single element carrying the value <c>1.0</c>, its three
    /// unprefixed attributes, and the ranking namespace as its default.
    /// </summary>
    [TestMethod]
    public void FeedRankToStringTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same element as <c>ToString</c>,
    /// down to the generated prefix and the pair of namespace declarations.
    /// </summary>
    [TestMethod]
    public void FeedRankWriteToTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(writeToText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions carrying different ranks are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankOpEqualityTestFailure()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions carrying the same rank are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankOpEqualityTestSuccess()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose domain is <c>example.com</c> does not sort above the one whose domain is
    /// <c>example.net</c> — the domain is the first member that differs, so neither the label nor the
    /// value is reached.
    /// </summary>
    [TestMethod]
    public void FeedRankOpGreaterThanTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions carrying different ranks are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankOpInequalityTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose domain is <c>example.com</c> sorts below the one whose domain is
    /// <c>example.net</c>.
    /// </summary>
    [TestMethod]
    public void FeedRankOpLessThanTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the domain, label, scheme and value the extension was
    /// built from.
    /// </summary>
    [TestMethod]
    public void FeedRankContextTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        FeedRankSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Domain.ShouldBe(new Uri("http://example.com"));
        context.Label.ShouldBe("Title");
        context.Scheme.ShouldBe(new Uri("http://example.com/scheme.txt"));
        context.Value.ShouldBe(1.0m);
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser: domain <c>example.com</c>, label
    /// <c>Title</c>, value <c>1.0</c>.
    /// </summary>
    /// <returns>An extension carrying a complete rank.</returns>
    private static FeedRankSyndicationExtension CreateExtension1()
    {
        FeedRankSyndicationExtension re = new()
        {
            Context =
            {
                Domain = new Uri("http://example.com"),
                Label = "Title",
                Scheme = new Uri("http://example.com/scheme.txt"),
                Value = 1.0m
            }
        };
        return re;
    }

    /// <summary>
    /// Builds a rank that names neither a scheme nor a domain — the shape a consumer gets by setting
    /// only what it has, both properties being nullable and neither being written by the constructor
    /// this uses.
    /// </summary>
    /// <returns>An extension carrying a label and a value and nothing else.</returns>
    private static FeedRankSyndicationExtension CreateExtensionWithoutSchemeOrDomain()
    {
        FeedRankSyndicationExtension re = new()
        {
            Context =
            {
                Label = "Title",
                Value = 1.0m
            }
        };
        return re;
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater: domain <c>example.net</c>, label
    /// <c>label</c>, value <c>2.0</c>.
    /// </summary>
    /// <returns>An extension carrying a complete rank.</returns>
    private static FeedRankSyndicationExtension CreateExtension2()
    {
        FeedRankSyndicationExtension re = new()
        {
            Context =
            {
                Domain = new Uri("http://example.net"),
                Label = "label",
                Scheme = new Uri("http://example.net/scheme.html"),
                Value = 2.0m
            }
        };
        return re;
    }

    /// <summary>
    /// Builds a context carrying value <c>1.0</c> and nothing else meaningful, without an extension
    /// around it.
    /// </summary>
    /// <returns>A context whose scheme and domain are empty relative URIs.</returns>
    public static FeedRankSyndicationExtensionContext CreateContext1()
    {
        FeedRankSyndicationExtensionContext re = new()
        {
            Domain = new Uri(""),
            Label = "",
            Scheme = new Uri(""),
            Value = 1.0m
        };
        return re;
    }
}