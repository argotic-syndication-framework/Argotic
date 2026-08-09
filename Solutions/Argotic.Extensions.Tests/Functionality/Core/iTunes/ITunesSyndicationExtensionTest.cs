using static Argotic.Common.ComparisonOperatorExtensions;
namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

/// <summary>
/// Covers <c>ITunesSyndicationExtension</c> across a context populated with every element Apple's
/// original podcasting specification defines.
/// </summary>
/// <remarks>
///     The elements Apple added later, and the spellings real feeds use for them, are covered separately
///     by <c>ITunesExplicitSpellingTests</c>, <c>ITunesCompleteTests</c> and
///     <c>ITunesVerificationTokenTests</c>.
/// </remarks>
[TestClass]
public class ITunesSyndicationExtensionTest
{
    const string namespc = """
                           xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd"
                           """;

    private readonly string nycText = """
        <subtitle xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">That song you like.</subtitle>
        <author xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">BigStar</author>
        <summary xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">Duh... That song you like</summary>
        <owner xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">
          <email>owner@bigstar.com</email>
          <name>BigStar's Guy</name>
        </owner>
        <image href="http://www.eexample.com/image.jpg" xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd" />
        <duration xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">00:03:21</duration>
        <keywords xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">loud,good for parties</keywords>
        <explicit xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd">clean</explicit>
        <category text="Rock" xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd" />
        <category text="Folk" xmlns="http://www.itunes.com/dtds/podcast-1.0.dtd" />
        """.ReplaceLineEndings();

    private const string strExtXml = "<itunes:subtitle>That song you like.</itunes:subtitle><itunes:author>BigStar</itunes:author>"
                                     + "<itunes:summary>Duh... That song you like</itunes:summary><itunes:owner><itunes:email>owner@bigstar.com</itunes:email>"
                                     + """<itunes:name>BigStar's Guy</itunes:name></itunes:owner><itunes:image href="http://www.eexample.com/image.jpg" />"""
                                     + "<itunes:duration>00:03:21</itunes:duration><itunes:keywords>loud,good for parties</itunes:keywords><itunes:explicit>clean</itunes:explicit>"
                                     + """<itunes:category text="Rock" /><itunes:category text="Folk" />""";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions built from the same podcast metadata compare equal, so <c>CompareTo</c> returns
    /// <c>0</c> — the category and keyword lists are compared by their contents.
    /// </summary>
    [TestMethod]
    public void ITunesCompareToTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        ITunesSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// <c>ITunesExplicitMaterial.Clean</c> is written back as the lower-case spelling <c>clean</c>.
    /// </summary>
    [TestMethod]
    public void ITunesExplicitMaterialAsStringTest()
    {
        ITunesExplicitMaterial value = ITunesExplicitMaterial.Clean;
        string expected = "clean";
        string actual = ITunesSyndicationExtension.ExplicitMaterialAsString(value);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The spelling <c>clean</c> is read back as <c>ITunesExplicitMaterial.Clean</c>.
    /// </summary>
    [TestMethod]
    public void ITunesExplicitMaterialByNameTest()
    {
        ITunesExplicitMaterial expected = ITunesExplicitMaterial.Clean;
        ITunesExplicitMaterial actual = ITunesSyndicationExtension.ExplicitMaterialByName("clean");
        ((double)actual).ShouldBe((double)expected, 3e-6);
    }

    /// <summary>
    /// Two separately built extensions carrying the same podcast metadata are equal through the
    /// <c>object</c> overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void ITunesEqualsTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The hash code is stable across repeated calls, and two equal extensions agree on it.
    /// </summary>
    [TestMethod]
    public void ITunesGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        ITunesSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        ITunesSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// Saving a feed with the extension attached writes the iTunes elements in the order the extension
    /// declares, with <c>owner</c> nested and <c>image</c> and <c>category</c> carrying their values as
    /// attributes rather than as text.
    /// </summary>
    [TestMethod]
    public void ITunesCreateXmlTest()
    {
        ITunesSyndicationExtension itunes = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(itunes);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying every iTunes value the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as ITunesSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes. A non-null extension proves that <i>one</i> of the ten elements parsed,
    ///     which is why each is asserted here rather than counted.
    /// </remarks>
    [TestMethod]
    public void ITunesFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        ITunesSyndicationExtension byType = item.FindExtension<ITunesSyndicationExtension>().ShouldNotBeNull();
        ITunesSyndicationExtension byPredicate = item
            .FindExtension(ITunesSyndicationExtension.MatchByType)
            .ShouldBeOfType<ITunesSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Subtitle.ShouldBe("That song you like.");
        byPredicate.Context.Author.ShouldBe("BigStar");
        byPredicate.Context.Summary.ShouldBe("Duh... That song you like");
        byPredicate.Context.Owner!.EmailAddress.ShouldBe("owner@bigstar.com");
        byPredicate.Context.Owner.Name.ShouldBe("BigStar's Guy");
        byPredicate.Context.Image.ShouldBe(new Uri("http://www.eexample.com/image.jpg"));
        byPredicate.Context.Duration.ShouldBe(new TimeSpan(0, 3, 21));
        byPredicate.Context.Keywords.ShouldBe(["loud", "good for parties"]);
        byPredicate.Context.ExplicitMaterial.ShouldBe(ITunesExplicitMaterial.Clean);
        byPredicate.Context.Categories.Select(category => category.Text).ShouldBe(["Rock", "Folk"]);
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is an
    /// <c>ITunesSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void ITunesMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = ITunesSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders every populated element, with the duration as <c>00:03:21</c>, the
    /// keywords joined by commas into one <c>keywords</c> element, and a <c>category</c> element per
    /// category.
    /// </summary>
    [TestMethod]
    public void ITunesToStringTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same elements as <c>ToString</c>,
    /// without the line breaks or the indentation of the nested <c>owner</c> children.
    /// </summary>
    [TestMethod]
    public void ITunesWriteToTest()
    {
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        ITunesSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine + "  ", "", StringComparison.Ordinal).Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions describing different podcasts are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void ITunesOpEqualityTestFailure()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions describing the same podcast are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void ITunesOpEqualityTestSuccess()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension authored by <c>BigStar</c> does not sort above the one authored by <c>NewStar</c> —
    /// the author is the first member the comparison reaches.
    /// </summary>
    [TestMethod]
    public void ITunesOpGreaterThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions describing different podcasts are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void ITunesOpInequalityTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension authored by <c>BigStar</c> sorts below the one authored by <c>NewStar</c>.
    /// </summary>
    [TestMethod]
    public void ITunesOpLessThanTest()
    {
        ITunesSyndicationExtension first = CreateExtension1();
        ITunesSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back every value the extension was built from, including both
    /// categories, both keywords and the owner's name and address.
    /// </summary>
    [TestMethod]
    public void ITunesContextTest()
    {
        ITunesSyndicationExtension target = CreateExtension1();
        ITunesSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Author.ShouldBe("BigStar");
        context.Categories.Count.ShouldBe(2);
        context.Duration.ShouldBe(new TimeSpan(0, 3, 21));
        context.ExplicitMaterial.ShouldBe(ITunesExplicitMaterial.Clean);
        context.Image.ShouldBe(new Uri("http://www.eexample.com/image.jpg"));
        context.IsBlocked.ShouldBeFalse();
        context.Keywords.Count.ShouldBe(2);
        context.Owner!.EmailAddress.ShouldBe("owner@bigstar.com");
        context.Owner.Name.ShouldBe("BigStar's Guy");
        context.Subtitle.ShouldBe("That song you like.");
        context.Summary.ShouldBe("Duh... That song you like");
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, authored by <c>BigStar</c> and
    /// carrying a value in every element of Apple's original specification.
    /// </summary>
    /// <returns>A fully populated extension.</returns>
    private static ITunesSyndicationExtension CreateExtension1()
    {
        ITunesSyndicationExtension nyc = new()
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

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, authored by <c>NewStar</c> and
    /// differing from the first in every member.
    /// </summary>
    /// <returns>A fully populated extension.</returns>
    private static ITunesSyndicationExtension CreateExtension2()
    {
        ITunesSyndicationExtension nyc = new()
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

    /// <summary>
    /// Builds an empty context, without an extension around it.
    /// </summary>
    /// <returns>A context carrying no podcast metadata.</returns>
    public static ITunesSyndicationExtensionContext CreateContext1() => new();
}