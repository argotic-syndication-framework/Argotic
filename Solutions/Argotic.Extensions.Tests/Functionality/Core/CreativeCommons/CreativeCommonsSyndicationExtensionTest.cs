namespace Argotic.Extensions.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Argotic.Extensions.Core;
    using Argotic.Syndication;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This is a test class for CreativeCommonsSyndicationExtensionTest and is intended
    /// to contain all CreativeCommonsSyndicationExtensionTest Unit Tests
    /// </summary>
    [TestClass]
    public class CreativeCommonsSyndicationExtensionTest
    {

        const string namespc = @"xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule""";

        private const string nycText =  "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license1.html</license>" +
                                        "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license2.html</license>";

        private const string strExtXml = "<creativeCommons:license>http://www.example.com/license1.html</creativeCommons:license>"
            + "<creativeCommons:license>http://www.example.com/license2.html</creativeCommons:license>";

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
        /// A test for CreativeCommonsSyndicationExtension Constructor
        /// </summary>
        [TestMethod]
        public void CreativeCommonsSyndicationExtensionConstructorTest()
        {
            CreativeCommonsSyndicationExtension target = new CreativeCommonsSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(CreativeCommonsSyndicationExtension));
        }

        /// <summary>
        /// A test for CompareTo
        /// </summary>
        [TestMethod]
        public void CreativeCommons_CompareToTest()
        {
            CreativeCommonsSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            int expected = 0; 
            int actual;
            actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        /// A test for Equals
        /// </summary>
        [TestMethod]
        public void CreativeCommons_EqualsTest()
        {
            CreativeCommonsSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for GetHashCode
        /// </summary>
        [TestMethod, Ignore]
        public void CreativeCommons_GetHashCodeTest()
        {
            CreativeCommonsSyndicationExtension target = this.CreateExtension1();
            int expected = -2111858259;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Load
        /// </summary>
        [TestMethod, Ignore]
        public void CreativeCommons_LoadTest()
        {
            CreativeCommonsSyndicationExtension target = new CreativeCommonsSyndicationExtension(); // TODO: Initialize to an appropriate value
            var nt = new NameTable();
            var ns = new XmlNamespaceManager(nt);
            var xpc = new XmlParserContext(nt, ns, "US-en",XmlSpace.Default);
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc)  )
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
        public void CreativeCommons_CreateXmlTest()
        {
            var itunes = this.CreateExtension1();

            var actual = ExtensionTestUtil.AddExtensionToXml(itunes);
            string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void CreativeCommons_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
             {
                 RssFeed feed = new RssFeed();
                 feed.Load(reader);

                 Assert.AreEqual(1, feed.Channel.Items.Count());
                 var item = feed.Channel.Items.Single();
                 Assert.IsTrue(item.HasExtensions);
                 var itemExtension = item.FindExtension<CreativeCommonsSyndicationExtension>();
                 Assert.IsNotNull(itemExtension);
                 Assert.IsInstanceOfType(
                     item.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) as CreativeCommonsSyndicationExtension,
                     typeof(CreativeCommonsSyndicationExtension));

             }
        }

        /// <summary>
        /// A test for MatchByType
        /// </summary>
        [TestMethod]
        public void CreativeCommons_MatchByTypeTest()
        {
            ISyndicationExtension extension = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = CreativeCommonsSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString
        /// </summary>
        [TestMethod]
        public void CreativeCommons_ToStringTest()
        {
            CreativeCommonsSyndicationExtension target = this.CreateExtension1();
            string expected = nycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual.Replace(Environment.NewLine, string.Empty));
        }

        /// <summary>
        /// A test for WriteTo
        /// </summary>
        [TestMethod]
        public void CreativeCommons_WriteToTest()
        {
            using (var sw = new StringWriter())
            using (XmlWriter writer = new XmlTextWriter(sw))
            {

                var target = this.CreateExtension1();
                target.WriteTo(writer);
                var output = sw.ToString();
                Assert.AreEqual(nycText.Replace(Environment.NewLine + "  ", string.Empty).Replace(Environment.NewLine, string.Empty), output.Replace(Environment.NewLine, string.Empty));
            }
        }

        /// <summary>
        /// A test for op_Equality
        /// </summary>
        [TestMethod]
        public void CreativeCommons_op_EqualityTest_Failure()
        {
            CreativeCommonsSyndicationExtension first = this.CreateExtension1();
            CreativeCommonsSyndicationExtension second = this.CreateExtension2();
            bool expected = false; 
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        public void CreativeCommons_op_EqualityTest_Success()
        {
            CreativeCommonsSyndicationExtension first = this.CreateExtension1();
            CreativeCommonsSyndicationExtension second = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_GreaterThan
        /// </summary>
        [TestMethod]
        public void CreativeCommons_op_GreaterThanTest()
        {
            CreativeCommonsSyndicationExtension first = this.CreateExtension1();
            CreativeCommonsSyndicationExtension second = this.CreateExtension2();
            bool expected = false; 
            bool actual = false;
            actual = first > second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Inequality
        /// </summary>
        [TestMethod]
        public void CreativeCommons_op_InequalityTest()
        {
            CreativeCommonsSyndicationExtension first = this.CreateExtension1();
            CreativeCommonsSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual = first != second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_LessThan
        /// </summary>
        [TestMethod]
        public void CreativeCommons_op_LessThanTest()
        {
            CreativeCommonsSyndicationExtension first = this.CreateExtension1();
            CreativeCommonsSyndicationExtension second = this.CreateExtension2();
            bool expected = true; 
            bool actual;
            actual = first < second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Context
        /// </summary>
        [TestMethod, Ignore]
        public void CreativeCommons_ContextTest()
        {
            CreativeCommonsSyndicationExtension target = this.CreateExtension1();
            CreativeCommonsSyndicationExtensionContext expected = CreateContext1();
            CreativeCommonsSyndicationExtensionContext actual;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        private CreativeCommonsSyndicationExtension CreateExtension1()
        {
            var nyc = new CreativeCommonsSyndicationExtension();

            nyc.Context.Licenses.Add(new Uri("http://www.example.com/license1.html"));
            nyc.Context.Licenses.Add(new Uri("http://www.example.com/license2.html"));
            return nyc;
        }

        private CreativeCommonsSyndicationExtension CreateExtension2()
        {
            var nyc = new CreativeCommonsSyndicationExtension();
            nyc.Context.Licenses.Add(new Uri("http://www.example.net/license1.html"));
            nyc.Context.Licenses.Add(new Uri("http://www.example.net/license2.html"));
            return nyc;
        }

        public static CreativeCommonsSyndicationExtensionContext CreateContext1()
        {
            var nyc = new CreativeCommonsSyndicationExtensionContext();
            return nyc;
        }
    }
}