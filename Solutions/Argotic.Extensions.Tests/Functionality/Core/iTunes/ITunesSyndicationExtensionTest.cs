﻿namespace Argotic.Extensions.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Argotic.Extensions.Core;
    using Argotic.Syndication;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This is a test class for ITunesSyndicationExtensionTest and is intended
    /// to contain all ITunesSyndicationExtensionTest Unit Tests.
    /// </summary>
    [TestClass]
    public class ITunesSyndicationExtensionTest
    {
        /// <summary>
        /// Namespace.
        /// </summary>
        public const string Namespc = @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""";

        private const string NycText =
            "<subtitle xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">That song you like.</subtitle>\r\n"
            + "<author xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">BigStar</author>\r\n"
            + "<summary xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">Duh... That song you like</summary>\r\n"
            + "<owner xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">\r\n  <email>owner@bigstar.com</email>\r\n  <name>BigStar's Guy</name>\r\n</owner>\r\n"
            + "<image href=\"http://www.eexample.com/image.jpg\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />\r\n"
            + "<duration xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">00:03:21</duration>\r\n"
            + "<keywords xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">loud,good for parties</keywords>\r\n"
            + "<explicit xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\">clean</explicit>\r\n"
            + "<category text=\"Rock\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />\r\n"
            + "<category text=\"Folk\" xmlns=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" />";

        private const string StrExtXml =
            "<itunes:subtitle>That song you like.</itunes:subtitle><itunes:author>BigStar</itunes:author>"
            + "<itunes:summary>Duh... That song you like</itunes:summary><itunes:owner><itunes:email>owner@bigstar.com</itunes:email>"
            + "<itunes:name>BigStar's Guy</itunes:name></itunes:owner><itunes:image href=\"http://www.eexample.com/image.jpg\" />"
            + "<itunes:duration>00:03:21</itunes:duration><itunes:keywords>loud,good for parties</itunes:keywords><itunes:explicit>clean</itunes:explicit>"
            + "<itunes:category text=\"Rock\" /><itunes:category text=\"Folk\" />";

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Create context.
        /// </summary>
        /// <returns>Returns <see cref="ITunesSyndicationExtensionContext"/>.</returns>
        public static ITunesSyndicationExtensionContext CreateContext1()
        {
            var nyc = new ITunesSyndicationExtensionContext();
            return nyc;
        }

        /// <summary>
        /// A test for CompareTo.
        /// </summary>
        [TestMethod]
        public void ITunes_CompareToTest()
        {
            ITunesSyndicationExtension target = this.CreateExtension1();
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
        public void ITunes_ContextTest()
        {
            ITunesSyndicationExtension target = this.CreateExtension1();
            ITunesSyndicationExtensionContext expected = CreateContext1();
            ITunesSyndicationExtensionContext actual;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        /// Create xml test.
        /// </summary>
        [TestMethod]
        public void ITunes_CreateXmlTest()
        {
            var itunes = this.CreateExtension1();

            var actual = ExtensionTestUtil.AddExtensionToXml(itunes);
            string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals.
        /// </summary>
        [TestMethod]
        public void ITunes_EqualsTest()
        {
            ITunesSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ConvertDecimalToDegreesMinutesSeconds.
        /// </summary>
        [TestMethod]
        public void ITunes_ExplicitMaterialAsStringTest()
        {
            var value = ITunesExplicitMaterial.Clean;
            string expected = "clean";
            string actual = ITunesSyndicationExtension.ExplicitMaterialAsString(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ConvertDegreesMinutesSecondsToDecimal.
        /// </summary>
        [TestMethod]
        public void ITunes_ExplicitMaterialByNameTest()
        {
            var expected = ITunesExplicitMaterial.Clean;
            var actual = ITunesSyndicationExtension.ExplicitMaterialByName("clean");
            Assert.AreEqual((double)expected, (double)actual, 3e-6);
        }

        /// <summary>
        /// Full test.
        /// </summary>
        [TestMethod]
        public void ITunes_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);

                Assert.AreEqual(1, feed.Channel.Items.Count());
                var item = feed.Channel.Items.Single();
                Assert.IsTrue(item.HasExtensions);
                var itemExtension = item.FindExtension<ITunesSyndicationExtension>();
                Assert.IsNotNull(itemExtension);
                Assert.IsInstanceOfType(
                    item.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension,
                    typeof(ITunesSyndicationExtension));
            }
        }

        /// <summary>
        /// A test for GetHashCode.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ITunes_GetHashCodeTest()
        {
            ITunesSyndicationExtension target = this.CreateExtension1();
            int expected = -765758449;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Load.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ITunes_LoadTest()
        {
            ITunesSyndicationExtension
                target = new ITunesSyndicationExtension(); // TODO: Initialize to an appropriate value
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
        public void ITunes_MatchByTypeTest()
        {
            ISyndicationExtension extension = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = ITunesSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Equality.
        /// </summary>
        [TestMethod]
        public void ITunes_op_EqualityTest_Failure()
        {
            ITunesSyndicationExtension first = this.CreateExtension1();
            ITunesSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        public void ITunes_op_EqualityTest_Success()
        {
            ITunesSyndicationExtension first = this.CreateExtension1();
            ITunesSyndicationExtension second = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_GreaterThan.
        /// </summary>
        [TestMethod]
        public void ITunes_op_GreaterThanTest()
        {
            ITunesSyndicationExtension first = this.CreateExtension1();
            ITunesSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual = false;
            actual = first > second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Inequality.
        /// </summary>
        [TestMethod]
        public void ITunes_op_InequalityTest()
        {
            ITunesSyndicationExtension first = this.CreateExtension1();
            ITunesSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual = first != second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_LessThan.
        /// </summary>
        [TestMethod]
        public void ITunes_op_LessThanTest()
        {
            ITunesSyndicationExtension first = this.CreateExtension1();
            ITunesSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual;
            actual = first < second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString.
        /// </summary>
        [TestMethod]
        public void ITunes_ToStringTest()
        {
            ITunesSyndicationExtension target = this.CreateExtension1();
            string expected = NycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for WriteTo.
        /// </summary>
        [TestMethod]
        public void ITunes_WriteToTest()
        {
            using (var sw = new StringWriter())
            using (XmlWriter writer = new XmlTextWriter(sw))
            {
                var target = this.CreateExtension1();
                target.WriteTo(writer);
                var output = sw.ToString();
                Assert.AreEqual(
                    NycText.Replace(Environment.NewLine + "  ", string.Empty)
                        .Replace(Environment.NewLine, string.Empty),
                    output.Replace(Environment.NewLine, string.Empty));
            }
        }

        /// <summary>
        /// A test for ITunesSyndicationExtension Constructor.
        /// </summary>
        [TestMethod]
        public void ITunesSyndicationExtensionConstructorTest()
        {
            ITunesSyndicationExtension target = new ITunesSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ITunesSyndicationExtension));
        }

        /// <summary>
        /// Create extension.
        /// </summary>
        /// <returns>Returns <see cref="ITunesSyndicationExtension"/>.</returns>
        private ITunesSyndicationExtension CreateExtension1()
        {
            var nyc = new ITunesSyndicationExtension();

            nyc.Context.Author = "BigStar";
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

        /// <summary>
        /// Create extension.
        /// </summary>
        /// <returns>Returns <see cref="ITunesSyndicationExtension"/>.</returns>
        private ITunesSyndicationExtension CreateExtension2()
        {
            var nyc = new ITunesSyndicationExtension();
            nyc.Context.Author = "NewStar";
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
    }
}