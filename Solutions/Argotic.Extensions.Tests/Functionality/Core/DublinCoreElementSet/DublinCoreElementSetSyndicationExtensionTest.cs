using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.DublinCoreElementSet;

/// <summary>
/// Covers the Dublin Core Element Set extension — the fifteen legacy <c>dc:</c> elements under
/// <c>http://purl.org/dc/elements/1.1/</c> — from the context that holds them, through the XML
/// <c>WriteTo</c> and <c>ToString</c> produce, to the comparison and equality contracts.
/// </summary>
[TestClass]
public class DublinCoreElementSetSyndicationExtensionTest
{
    const string namespc = @"xmlns:dc=""http://purl.org/dc/elements/1.1/""";

    private readonly string nycText = "<contributor xmlns=\"http://purl.org/dc/elements/1.1/\">Helper</contributor>" + Environment.NewLine +
                                      "<coverage xmlns=\"http://purl.org/dc/elements/1.1/\">US</coverage>" + Environment.NewLine +
                                      "<creator xmlns=\"http://purl.org/dc/elements/1.1/\">The Big Guy</creator>" + Environment.NewLine +
                                      "<date xmlns=\"http://purl.org/dc/elements/1.1/\">2010-08-01T00:00:00.00Z</date>" + Environment.NewLine +
                                      "<description xmlns=\"http://purl.org/dc/elements/1.1/\">That kind of thing</description>" + Environment.NewLine +
                                      "<format xmlns=\"http://purl.org/dc/elements/1.1/\">CDROM</format>" + Environment.NewLine +
                                      "<identifier xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-1</identifier>" + Environment.NewLine +
                                      "<language xmlns=\"http://purl.org/dc/elements/1.1/\">en-US</language>" + Environment.NewLine +
                                      "<publisher xmlns=\"http://purl.org/dc/elements/1.1/\">MeMeMe</publisher>" + Environment.NewLine +
                                      "<relation xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-2</relation>" + Environment.NewLine +
                                      "<rights xmlns=\"http://purl.org/dc/elements/1.1/\">Copyright 2010</rights>" + Environment.NewLine +
                                      "<source xmlns=\"http://purl.org/dc/elements/1.1/\">Out of Me Head</source>" + Environment.NewLine +
                                      "<subject xmlns=\"http://purl.org/dc/elements/1.1/\">Test data (Stupid variety)</subject>" + Environment.NewLine +
                                      "<title xmlns=\"http://purl.org/dc/elements/1.1/\">Stupid test data</title>" + Environment.NewLine +
                                      "<type xmlns=\"http://purl.org/dc/elements/1.1/\">PhysicalObject</type>";

    private const string strExtXml = "<dc:contributor>Helper</dc:contributor>"
                                     + "<dc:coverage>US</dc:coverage>"
                                     + "<dc:creator>The Big Guy</dc:creator>"
                                     + "<dc:date>2010-08-01T00:00:00.00Z</dc:date>"
                                     + "<dc:description>That kind of thing</dc:description>"
                                     + "<dc:format>CDROM</dc:format>"
                                     + "<dc:identifier>MYTESTCDROM-1</dc:identifier>"
                                     + "<dc:language>en-US</dc:language>"
                                     + "<dc:publisher>MeMeMe</dc:publisher>"
                                     + "<dc:relation>MYTESTCDROM-2</dc:relation>"
                                     + "<dc:rights>Copyright 2010</dc:rights>"
                                     + "<dc:source>Out of Me Head</dc:source>"
                                     + "<dc:subject>Test data (Stupid variety)</dc:subject>"
                                     + "<dc:title>Stupid test data</dc:title>"
                                     + "<dc:type>PhysicalObject</dc:type>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor yields an instance of the Dublin Core element set extension type.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetSyndicationExtensionConstructorTest()
    {
        DublinCoreElementSetSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<DublinCoreElementSetSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetCompareToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        DublinCoreElementSetSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// The <c>MovingImage</c> vocabulary term renders as the string <c>MovingImage</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreTypeVocabularyAsString()
    {
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.MovingImage;
        string expected = "MovingImage";
        string actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(value);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The name <c>MovingImage</c> resolves back to the <c>MovingImage</c> vocabulary term.
    /// </summary>
    [TestMethod]
    public void DublinCoreTypeVocabularyByName()
    {
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.MovingImage;
        DublinCoreTypeVocabularies actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName("MovingImage");
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetEqualsTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// A hash code is stable across calls, and equal extensions hash equally.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        DublinCoreElementSetSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// An RSS 2.0 feed carrying all fifteen <c>dc:</c> elements yields an extension holding all fifteen
    /// values the document declared.
    /// </summary>
    /// <remarks>
    ///     The body used to end at <c>feed.Load(reader)</c> and assert nothing whatever, so it could only
    ///     ever have caught an exception thrown out of the parse.
    /// </remarks>
    [TestMethod]
    public void DublinCoreElementSetLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        DublinCoreElementSetSyndicationExtension extension = item.FindExtension<DublinCoreElementSetSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Contributor.ShouldBe("Helper");
        extension.Context.Coverage.ShouldBe("US");
        extension.Context.Creator.ShouldBe("The Big Guy");
        extension.Context.Date.ShouldBe(new DateTime(2010, 8, 1, 0, 0, 0, DateTimeKind.Utc));
        extension.Context.Description.ShouldBe("That kind of thing");
        extension.Context.Format.ShouldBe("CDROM");
        extension.Context.Identifier.ShouldBe("MYTESTCDROM-1");
        extension.Context.Language!.Name.ShouldBe("en-US");
        extension.Context.Publisher.ShouldBe("MeMeMe");
        extension.Context.Relation.ShouldBe("MYTESTCDROM-2");
        extension.Context.Rights.ShouldBe("Copyright 2010");
        extension.Context.Source.ShouldBe("Out of Me Head");
        extension.Context.Subject.ShouldBe("Test data (Stupid variety)");
        extension.Context.Title.ShouldBe("Stupid test data");
        extension.Context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.PhysicalObject);
    }

    /// <summary>
    /// Attaching the extension to an RSS item emits every populated element under the <c>dc</c> prefix, in the order the fixture spells them.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetCreateXmlTest()
    {
        DublinCoreElementSetSyndicationExtension dub = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(dub);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the Dublin Core values the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as DublinCoreElementSetSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
    [TestMethod]
    public void DublinCoreElementSetFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension byType = item.FindExtension<DublinCoreElementSetSyndicationExtension>().ShouldNotBeNull();
        DublinCoreElementSetSyndicationExtension byPredicate = item
            .FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType)
            .ShouldBeOfType<DublinCoreElementSetSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Creator.ShouldBe("The Big Guy");
        byPredicate.Context.Title.ShouldBe("Stupid test data");
        byPredicate.Context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.PhysicalObject);
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an instance of its own extension type.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = DublinCoreElementSetSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders each populated element on its own line, every one redeclaring the Dublin Core namespace as its default.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetToStringTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to an <see cref="XmlWriter"/> emits the same elements as <c>ToString</c>, once line breaks are discounted.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetWriteToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Extensions built from different Dublin Core values are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetOpEqualityTestFailure()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding identical context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetOpEqualityTestSuccess()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// An extension whose first differing context value sorts earlier is not greater than the other.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetOpGreaterThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetOpInequalityTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// An extension whose first differing context value sorts earlier — contributor <c>Helper</c> ahead of <c>Helper-er</c> — is less than the other.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetOpLessThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The context reports all fifteen Dublin Core values it was given, down to the <c>PhysicalObject</c> type vocabulary.
    /// </summary>
    [TestMethod]
    public void DublinCoreElementSetContextTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        DublinCoreElementSetSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Contributor.ShouldBe("Helper");
        context.Coverage.ShouldBe("US");
        context.Creator.ShouldBe("The Big Guy");
        context.Date.ShouldBe(new DateTime(2010, 8, 1));
        context.Description.ShouldBe("That kind of thing");
        context.Format.ShouldBe("CDROM");
        context.Identifier.ShouldBe("MYTESTCDROM-1");
        context.Language!.Name.ShouldBe("en-US");
        context.Publisher.ShouldBe("MeMeMe");
        context.Relation.ShouldBe("MYTESTCDROM-2");
        context.Rights.ShouldBe("Copyright 2010");
        context.Source.ShouldBe("Out of Me Head");
        context.Subject.ShouldBe("Test data (Stupid variety)");
        context.Title.ShouldBe("Stupid test data");
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.PhysicalObject);
    }

    private static DublinCoreElementSetSyndicationExtension CreateExtension1()
    {
        DublinCoreElementSetSyndicationExtension dub = new()
        {
            Context =
            {
                Contributor = "Helper",
                Coverage = "US",
                Creator = "The Big Guy",
                Date = new DateTime(2010, 8, 1),
                Description = "That kind of thing",
                Format = "CDROM",
                Identifier = "MYTESTCDROM-1",
                Language = new CultureInfo("en-US"),
                Publisher = "MeMeMe",
                Relation = "MYTESTCDROM-2",
                Rights = "Copyright 2010",
                Source = "Out of Me Head",
                Subject = "Test data (Stupid variety)",
                Title = "Stupid test data",
                TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
            }
        };

        return dub;
    }

    private static DublinCoreElementSetSyndicationExtension CreateExtension2()
    {
        DublinCoreElementSetSyndicationExtension dub = new()
        {
            Context =
            {
                Contributor = "Helper-er",
                Coverage = "US",
                Creator = "The Not-So-Big Guy",
                Date = new DateTime(2010, 8, 1),
                Description = "This kind of thing",
                Format = "CDROM",
                Identifier = "MYTESTCDROM-2",
                Language = new CultureInfo("en-US"),
                Publisher = "MeMyselfI",
                Relation = "MYTESTCDROM-1",
                Rights = "Copyright 2010",
                Source = "Nowheres, man",
                Subject = "Test data (Son of)",
                Title = "More Stupid test data",
                TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
            }
        };

        return dub;
    }

    public static DublinCoreElementSetSyndicationExtensionContext CreateContext1()
    {
        DublinCoreElementSetSyndicationExtensionContext dub = new()
        {
            Contributor = "",
            Coverage = "",
            Creator = "",
            Date = new DateTime(2010, 8, 1),
            Description = "",
            Format = "",
            Identifier = "",
            Language = new CultureInfo("US-en"),
            Publisher = "",
            Relation = "",
            Rights = "",
            Source = "",
            Subject = "",
            Title = "",
            TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
        };

        return dub;
    }
}