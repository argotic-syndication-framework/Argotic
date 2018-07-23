namespace Argotic.Extensions.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Argotic.Extensions.Core;
    using Argotic.Syndication;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This is a test class for DublinCoreElementSetSyndicationExtensionTest and is intended
    /// to contain all DublinCoreElementSetSyndicationExtensionTest Unit Tests.
    /// </summary>
    [TestClass]
    public class DublinCoreElementSetSyndicationExtensionTest
    {
        private const string Namespc = @"xmlns:dc=""http://purl.org/dc/elements/1.1/""";

        private const string NycText =
            "<contributor xmlns=\"http://purl.org/dc/elements/1.1/\">Helper</contributor>\r\n"
            + "<coverage xmlns=\"http://purl.org/dc/elements/1.1/\">US</coverage>\r\n"
            + "<creator xmlns=\"http://purl.org/dc/elements/1.1/\">The Big Guy</creator>\r\n"
            + "<date xmlns=\"http://purl.org/dc/elements/1.1/\">2010-08-01T00:00:00.00Z</date>\r\n"
            + "<description xmlns=\"http://purl.org/dc/elements/1.1/\">That kind of thing</description>\r\n"
            + "<format xmlns=\"http://purl.org/dc/elements/1.1/\">CDROM</format>\r\n"
            + "<identifier xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-1</identifier>\r\n"
            + "<language xmlns=\"http://purl.org/dc/elements/1.1/\">en-US</language>\r\n"
            + "<publisher xmlns=\"http://purl.org/dc/elements/1.1/\">MeMeMe</publisher>\r\n"
            + "<relation xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-2</relation>\r\n"
            + "<rights xmlns=\"http://purl.org/dc/elements/1.1/\">Copyright 2010</rights>\r\n"
            + "<source xmlns=\"http://purl.org/dc/elements/1.1/\">Out of Me Head</source>\r\n"
            + "<subject xmlns=\"http://purl.org/dc/elements/1.1/\">Test data (Stupid variety)</subject>\r\n"
            + "<title xmlns=\"http://purl.org/dc/elements/1.1/\">Stupid test data</title>\r\n"
            + "<type xmlns=\"http://purl.org/dc/elements/1.1/\">PhysicalObject</type>";

        private const string StrExtXml = "<dc:contributor>Helper</dc:contributor>" + "<dc:coverage>US</dc:coverage>"
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

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        /// <summary>
        /// Create context.
        /// </summary>
        /// <returns>Returns <see cref="DublinCoreElementSetSyndicationExtensionContext"/>.</returns>
        public static DublinCoreElementSetSyndicationExtensionContext CreateContext1()
        {
            var dub = new DublinCoreElementSetSyndicationExtensionContext();
            dub.Contributor = string.Empty;
            dub.Coverage = string.Empty;
            dub.Creator = string.Empty;
            dub.Date = new DateTime(2010, 8, 1);
            dub.Description = string.Empty;
            dub.Format = string.Empty;
            dub.Identifier = string.Empty;
            dub.Language = new CultureInfo("US-en");
            dub.Publisher = string.Empty;
            dub.Relation = string.Empty;
            dub.Rights = string.Empty;
            dub.Source = string.Empty;
            dub.Subject = string.Empty;
            dub.Title = string.Empty;
            dub.TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject;
            return dub;
        }

        /// <summary>
        /// Type vocabulary as string.
        /// </summary>
        public void DublinCore_TypeVocabularyAsString()
        {
            var value = DublinCoreTypeVocabularies.MovingImage;
            string expected = "MovingImage";
            string actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ConvertDegreesMinutesSecondsToDecimal.
        /// </summary>
        [TestMethod]
        public void DublinCore_TypeVocabularyByName()
        {
            var expected = DublinCoreTypeVocabularies.MovingImage;
            var actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName("MovingImage");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for CompareTo.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_CompareToTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            int expected = 0;
            int actual;
            actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Context.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void DublinCoreElementSet_ContextTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtensionContext expected = CreateContext1();
            DublinCoreElementSetSyndicationExtensionContext actual;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        /// Create xml test.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_CreateXmlTest()
        {
            var dub = this.CreateExtension1();

            var actual = ExtensionTestUtil.AddExtensionToXml(dub);
            string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_EqualsTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Full test of Dublin core element set.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);

                Assert.AreEqual(1, feed.Channel.Items.Count());
                var item = feed.Channel.Items.Single();
                Assert.IsTrue(item.HasExtensions);
                var itemExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
                Assert.IsNotNull(itemExtension);
                Assert.IsInstanceOfType(
                    item.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as
                        DublinCoreElementSetSyndicationExtension,
                    typeof(DublinCoreElementSetSyndicationExtension));
            }
        }

        /// <summary>
        /// A test for GetHashCode.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void DublinCoreElementSet_GetHashCodeTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            int expected = 1398804031;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Load.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void DublinCoreElementSet_LoadTest()
        {
            DublinCoreElementSetSyndicationExtension
                target = new DublinCoreElementSetSyndicationExtension(); // TODO: Initialize to an appropriate value
            var nt = new NameTable();
            var ns = new XmlNamespaceManager(nt);
            var xpc = new XmlParserContext(nt, ns, "US-en", XmlSpace.Default);
            var strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc))
            {
#if false

// var document  = new XPathDocument(reader);

// var nav = document.CreateNavigator();

// nav.Select("//item");
				do
				{
					if (!reader.Read())
						break;
				} while (reader.NodeType != XmlNodeType.EndElement || reader.Name != "webMaster");

				
				bool expected = true;
				bool actual;
				actual = target.Load(reader);
				Assert.AreEqual(expected, actual);
#else
                RssFeed feed = new RssFeed();
                feed.Load(reader);
#endif
            }
        }

        /// <summary>
        /// A test for MatchByType.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_MatchByTypeTest()
        {
            ISyndicationExtension extension = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = DublinCoreElementSetSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Equality.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_op_EqualityTest_Failure()
        {
            DublinCoreElementSetSyndicationExtension first = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        public void DublinCoreElementSet_op_EqualityTest_Success()
        {
            DublinCoreElementSetSyndicationExtension first = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtension second = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_GreaterThan.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_op_GreaterThanTest()
        {
            DublinCoreElementSetSyndicationExtension first = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual = false;
            actual = first > second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Inequality.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_op_InequalityTest()
        {
            DublinCoreElementSetSyndicationExtension first = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual = first != second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_LessThan.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_op_LessThanTest()
        {
            DublinCoreElementSetSyndicationExtension first = this.CreateExtension1();
            DublinCoreElementSetSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual;
            actual = first < second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_ToStringTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            string expected = NycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for WriteTo.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSet_WriteToTest()
        {
            DublinCoreElementSetSyndicationExtension target = this.CreateExtension1();
            using (var sw = new StringWriter())
            using (XmlWriter writer = new XmlTextWriter(sw))
            {
                target.WriteTo(writer);
                var output = sw.ToString();
                Assert.AreEqual(
                    NycText.Replace(Environment.NewLine, string.Empty),
                    output.Replace(Environment.NewLine, string.Empty));
            }
        }

        /// <summary>
        /// A test for DublinCoreElementSetSyndicationExtension Constructor.
        /// </summary>
        [TestMethod]
        public void DublinCoreElementSetSyndicationExtensionConstructorTest()
        {
            DublinCoreElementSetSyndicationExtension target = new DublinCoreElementSetSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(DublinCoreElementSetSyndicationExtension));
        }

        private DublinCoreElementSetSyndicationExtension CreateExtension1()
        {
            var dub = new DublinCoreElementSetSyndicationExtension();
            dub.Context.Contributor = "Helper";
            dub.Context.Coverage = "US";
            dub.Context.Creator = "The Big Guy";
            dub.Context.Date = new DateTime(2010, 8, 1);
            dub.Context.Description = "That kind of thing";
            dub.Context.Format = "CDROM";
            dub.Context.Identifier = "MYTESTCDROM-1";
            dub.Context.Language = new CultureInfo("en-US");
            dub.Context.Publisher = "MeMeMe";
            dub.Context.Relation = "MYTESTCDROM-2";
            dub.Context.Rights = "Copyright 2010";
            dub.Context.Source = "Out of Me Head";
            dub.Context.Subject = "Test data (Stupid variety)";
            dub.Context.Title = "Stupid test data";
            dub.Context.TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject;
            return dub;
        }

        private DublinCoreElementSetSyndicationExtension CreateExtension2()
        {
            var dub = new DublinCoreElementSetSyndicationExtension();
            dub.Context.Contributor = "Helper-er";
            dub.Context.Coverage = "US";
            dub.Context.Creator = "The Not-So-Big Guy";
            dub.Context.Date = new DateTime(2010, 8, 1);
            dub.Context.Description = "This kind of thing";
            dub.Context.Format = "CDROM";
            dub.Context.Identifier = "MYTESTCDROM-2";
            dub.Context.Language = new CultureInfo("en-US");
            dub.Context.Publisher = "MeMyselfI";
            dub.Context.Relation = "MYTESTCDROM-1";
            dub.Context.Rights = "Copyright 2010";
            dub.Context.Source = "Nowheres, man";
            dub.Context.Subject = "Test data (Son of)";
            dub.Context.Title = "More Stupid test data";
            dub.Context.TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject;
            return dub;
        }
    }
}