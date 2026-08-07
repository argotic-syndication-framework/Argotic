using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.Pheed;

/// <summary>
/// Covers <c>PheedSyndicationExtension</c>, the Pheed photo module that pairs a full-size
/// <c>photo:imgsrc</c> with a <c>photo:thumbnail</c>.
/// </summary>
[TestClass]
public class PheedSyndicationExtensionTest
{
    const string namespc = @"xmlns:photo=""http://www.pheed.com/pheed/""";

    private readonly string nycText = "<thumbnail xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/thumbnail.jpg</thumbnail>" + Environment.NewLine
                                                                                                                                          + "<imgsrc xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/</imgsrc>";

    private const string strExtXml = "<photo:thumbnail>http://www.example.com/thumbnail.jpg</photo:thumbnail><photo:imgsrc>http://www.example.com/</photo:imgsrc>";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions built from the same source and thumbnail compare equal, so <c>CompareTo</c>
    /// returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void PheedCompareToTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        PheedSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same two URIs are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void PheedEqualsTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The hash code is stable across repeated calls, and two equal extensions agree on it.
    /// </summary>
    [TestMethod]
    public void PheedGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        PheedSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        PheedSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// Saving a feed with the extension attached writes <c>photo:thumbnail</c> before
    /// <c>photo:imgsrc</c>, and the source appears as <c>http://www.example.com/</c> because
    /// <see cref="Uri"/> supplies the empty path.
    /// </summary>
    [TestMethod]
    public void PheedCreateXmlTest()
    {
        PheedSyndicationExtension pheed = new()
        {
            Context =
            {
                Source = new Uri("http://www.example.com"),
                Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
            }
        };

        string actual = ExtensionTestUtil.AddExtensionToXml(pheed);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying both the thumbnail and the source the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as PheedSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected neither URI. Since either element alone is enough for
    ///     the loader to keep the extension, both are asserted.
    /// </remarks>
    [TestMethod]
    public void PheedFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        PheedSyndicationExtension byType = item.FindExtension<PheedSyndicationExtension>().ShouldNotBeNull();
        PheedSyndicationExtension byPredicate = item
            .FindExtension(PheedSyndicationExtension.MatchByType)
            .ShouldBeOfType<PheedSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Thumbnail.ShouldBe(new Uri("http://www.example.com/thumbnail.jpg"));
        byPredicate.Context.Source.ShouldBe(new Uri("http://www.example.com/"));
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>PheedSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void PheedMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = PheedSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders <c>thumbnail</c> and then <c>imgsrc</c> on separate lines, each declaring
    /// the Pheed namespace as its default rather than carrying the <c>photo</c> prefix.
    /// </summary>
    [TestMethod]
    public void PheedToStringTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same two elements as
    /// <c>ToString</c>, without the line break between them.
    /// </summary>
    [TestMethod]
    public void PheedWriteToTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions pointing at different images are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void PheedOpEqualityTestFailure()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at the same images are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void PheedOpEqualityTestSuccess()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension sourced from <c>example.com</c> does not sort above the one sourced from
    /// <c>example.net</c> — the source is the first member that differs.
    /// </summary>
    [TestMethod]
    public void PheedOpGreaterThanTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at different images are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void PheedOpInequalityTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension sourced from <c>example.com</c> sorts below the one sourced from
    /// <c>example.net</c>.
    /// </summary>
    [TestMethod]
    public void PheedOpLessThanTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the source and thumbnail the extension was built from.
    /// </summary>
    [TestMethod]
    public void PheedContextTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        PheedSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Source.ShouldBe(new Uri("http://www.example.com"));
        context.Thumbnail.ShouldBe(new Uri("http://www.example.com/thumbnail.jpg"));
    }

    /// <summary>
    /// An item carrying only a <c>photo:imgsrc</c> saves with only a <c>photo:imgsrc</c>: the thumbnail
    /// that was never in the source is not invented on the way out.
    /// </summary>
    [TestMethod]
    public void PheedWriteToOmitsAThumbnailThatWasNeverThere()
    {
        // Arrange
        PheedSyndicationExtension target = new()
        {
            Context = { Source = new Uri("http://www.example.com/photo.jpg") }
        };

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("imgsrc", Case.Sensitive);
        actual.ShouldNotContain("thumbnail", Case.Sensitive);
    }

    /// <summary>
    /// The same in the other direction — a thumbnail alone saves without an empty <c>photo:imgsrc</c>.
    /// </summary>
    [TestMethod]
    public void PheedWriteToOmitsASourceThatWasNeverThere()
    {
        // Arrange
        PheedSyndicationExtension target = new()
        {
            Context = { Thumbnail = new Uri("http://www.example.com/thumb.jpg") }
        };

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("thumbnail", Case.Sensitive);
        actual.ShouldNotContain("imgsrc", Case.Sensitive);
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, sourced from <c>example.com</c>.
    /// </summary>
    /// <returns>An extension carrying a source and a thumbnail.</returns>
    private static PheedSyndicationExtension CreateExtension1()
    {
        PheedSyndicationExtension nyc = new()
        {
            Context =
            {
                Source = new Uri("http://www.example.com"),
                Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
            }
        };

        return nyc;
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, sourced from <c>example.net</c>.
    /// </summary>
    /// <returns>An extension carrying a source and a thumbnail.</returns>
    private static PheedSyndicationExtension CreateExtension2()
    {
        PheedSyndicationExtension nyc = new()
        {
            Context =
            {
                Source = new Uri("http://www.example.net"),
                Thumbnail = new Uri("http://www.example.net/thumbnail.png")
            }
        };

        return nyc;
    }

    /// <summary>
    /// Builds a context carrying the <c>example.com</c> source and thumbnail, without an extension
    /// around it.
    /// </summary>
    /// <returns>A context carrying a source and a thumbnail.</returns>
    public static PheedSyndicationExtensionContext CreateContext1()
    {
        PheedSyndicationExtensionContext nyc = new()
        {
            Source = new Uri("http://www.example.com"),
            Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
        };

        return nyc;
    }
}