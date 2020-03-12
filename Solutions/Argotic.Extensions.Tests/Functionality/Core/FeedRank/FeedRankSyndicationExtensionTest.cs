namespace Argotic.Extensions.Tests
{
    using System;
    using System.IO;
    using System.Xml;
    using Argotic.Extensions.Core;
    using Argotic.Syndication;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;

    /// <summary>
    ///This is a test class for FeedRankSyndicationExtensionTest and is intended
    ///to contain all FeedRankSyndicationExtensionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FeedRankSyndicationExtensionTest
    {
        private const string namespc = @"xmlns:re=""http://purl.org/atompub/rank/1.0""";
        private const string nycText = "<rank p1:scheme=\"http://example.com/scheme.txt\" p1:domain=\"http://example.com/\" label=\"Title\" xmlns:p1=\"http://purl.org/atompub/rank/1.0\" xmlns=\"http://purl.org/atompub/rank/1.0\">1.0</rank>";
        private const string writeToText = "<rank d1p1:scheme=\"http://example.com/scheme.txt\" d1p1:domain=\"http://example.com/\" label=\"Title\" xmlns:d1p1=\"http://purl.org/atompub/rank/1.0\" xmlns=\"http://purl.org/atompub/rank/1.0\">1.0</rank>";
        private const string strExtXml = "<re:rank re:scheme=\"http://example.com/scheme.txt\" re:domain=\"http://example.com/\" label=\"Title\">1.0</re:rank>";

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        ///A test for FeedRankSyndicationExtension Constructor
        ///</summary>
        [TestMethod()]
        public void FeedRankSyndicationExtensionConstructorTest()
        {
            FeedRankSyndicationExtension target = new FeedRankSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(FeedRankSyndicationExtension));
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void FeedRank_CompareToTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            object obj = CreateExtension1();
            int expected = 0;
            int actual;
            actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void FeedRank_EqualsTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            object obj = CreateExtension1();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod, Ignore]
        public void FeedRank_GetHashCodeTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            int expected = 1719638022;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod, Ignore]
        public void FeedRank_LoadTest()
        {
            FeedRankSyndicationExtension target = new FeedRankSyndicationExtension(); // TODO: Initialize to an appropriate value
            var nt = new NameTable();
            var ns = new XmlNamespaceManager(nt);
            var xpc = new XmlParserContext(nt, ns, "US-en", XmlSpace.Default);
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc))
            {
#if false
//var document  = new XPathDocument(reader);
//var nav = document.CreateNavigator();
//nav.Select("//item");
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

        [TestMethod]
        public void FeedRank_CreateXmlTest()
        {
            var re = CreateExtension1();

            var actual = ExtensionTestUtil.AddExtensionToXml(re);
            string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void FeedRank_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);

                //				 Assert.IsTrue(feed.Channel.HasExtensions);
                //				 Assert.IsInstanceOfType(feed.Channel.FindExtension(FeedRankSyndicationExtension.MatchByType) as FeedRankSyndicationExtension,
                //						 typeof(FeedRankSyndicationExtension));

                Assert.AreEqual(1, feed.Channel.Items.Count());
                var item = feed.Channel.Items.Single();
                Assert.IsTrue(item.HasExtensions);
                var itemExtension = item.FindExtension<FeedRankSyndicationExtension>();
                Assert.IsNotNull(itemExtension);
                Assert.IsInstanceOfType(
                    item.FindExtension(FeedRankSyndicationExtension.MatchByType) as FeedRankSyndicationExtension,
                    typeof(FeedRankSyndicationExtension));
            }
        }

        /// <summary>
        ///A test for MatchByType
        ///</summary>
        [TestMethod()]
        public void FeedRank_MatchByTypeTest()
        {
            ISyndicationExtension extension = CreateExtension1();
            bool expected = true;
            bool actual;
            actual = FeedRankSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void FeedRank_ToStringTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            string expected = nycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for WriteTo
        ///</summary>
        [TestMethod()]
        public void FeedRank_WriteToTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            using (var sw = new StringWriter())
            using (XmlWriter writer = new XmlTextWriter(sw))
            {
                target.WriteTo(writer);
                var output = sw.ToString();
                Assert.AreEqual(writeToText.Replace(Environment.NewLine, ""), output.Replace(Environment.NewLine, ""));
            }
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void FeedRank_op_EqualityTest_Failure()
        {
            FeedRankSyndicationExtension first = CreateExtension1();
            FeedRankSyndicationExtension second = CreateExtension2();
            bool expected = false;
            bool actual;
            actual = (first == second);
            Assert.AreEqual(expected, actual);
        }

        public void FeedRank_op_EqualityTest_Success()
        {
            FeedRankSyndicationExtension first = CreateExtension1();
            FeedRankSyndicationExtension second = CreateExtension1();
            bool expected = true;
            bool actual;
            actual = (first == second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void FeedRank_op_GreaterThanTest()
        {
            FeedRankSyndicationExtension first = CreateExtension1();
            FeedRankSyndicationExtension second = CreateExtension2();
            bool expected = false;
            bool actual = false;
            actual = (first > second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void FeedRank_op_InequalityTest()
        {
            FeedRankSyndicationExtension first = CreateExtension1();
            FeedRankSyndicationExtension second = CreateExtension2();
            bool expected = true;
            bool actual = (first != second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void FeedRank_op_LessThanTest()
        {
            FeedRankSyndicationExtension first = CreateExtension1();
            FeedRankSyndicationExtension second = CreateExtension2();
            bool expected = true;
            bool actual;
            actual = (first < second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Context
        ///</summary>
        [TestMethod(), Ignore]
        public void FeedRank_ContextTest()
        {
            FeedRankSyndicationExtension target = CreateExtension1();
            FeedRankSyndicationExtensionContext expected = CreateContext1();
            FeedRankSyndicationExtensionContext actual;
            //			target.Context = expected;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        private FeedRankSyndicationExtension CreateExtension1()
        {
            var re = new FeedRankSyndicationExtension();
            re.Context.Domain = new Uri("http://example.com");
            re.Context.Label = "Title";
            re.Context.Scheme = new Uri("http://example.com/scheme.txt");
            re.Context.Value = 1.0m;
            return re;
        }

        private FeedRankSyndicationExtension CreateExtension2()
        {
            var re = new FeedRankSyndicationExtension();
            re.Context.Domain = new Uri("http://example.net");
            re.Context.Label = "label";
            re.Context.Scheme = new Uri("http://example.net/scheme.html");
            re.Context.Value = 2.0m;
            return re;
        }

        public static FeedRankSyndicationExtensionContext CreateContext1()
        {
            var re = new FeedRankSyndicationExtensionContext();
            re.Domain = new Uri("");
            re.Label = "";
            re.Scheme = new Uri("");
            re.Value = 1.0m;
            return re;
        }
    }
}