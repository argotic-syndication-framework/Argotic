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
    /// This is a test class for AtomPublishingControlSyndicationExtensionTest and is intended
    /// to contain all AtomPublishingControlSyndicationExtensionTest Unit Tests.
    /// </summary>
    [TestClass]
    public class AtomPublishingControlSyndicationExtensionTest
    {
        private const string Namespc = @"xmlns:app=""http://www.w3.org/2007/app""";

        private const string NycText =
            "<control xml:base=\"http://www.example.com/control.html\" xml:lang=\"en-US\" xmlns=\"http://www.w3.org/2007/app\">\r\n"
            + "  <draft>yes</draft>\r\n" + "</control>";

        private const string StrExtXml =
            "<app:control xml:base=\"http://www.example.com/control.html\" xml:lang=\"en-US\"><app:draft>yes</app:draft></app:control>";

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Create context.
        /// </summary>
        /// <returns>Returns <see cref="AtomPublishingControlSyndicationExtensionContext"/>.</returns>
        public static AtomPublishingControlSyndicationExtensionContext CreateContext1()
        {
            var nyc = new AtomPublishingControlSyndicationExtensionContext();
            return nyc;
        }

        /// <summary>
        /// A test for CompareTo.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_CompareToTest()
        {
            AtomPublishingControlSyndicationExtension target = this.CreateExtension1();
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
        public void AtomPublishingControl_ContextTest()
        {
            AtomPublishingControlSyndicationExtension target = this.CreateExtension1();
            AtomPublishingControlSyndicationExtensionContext expected = CreateContext1();
            AtomPublishingControlSyndicationExtensionContext actual;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        /// Create xml test.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_CreateXmlTest()
        {
            var itunes = this.CreateExtension1();

            var actual = ExtensionTestUtil.AddExtensionToXml(itunes).Trim();
            string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml).Trim();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Equals.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_EqualsTest()
        {
            AtomPublishingControlSyndicationExtension target = this.CreateExtension1();
            object obj = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Atom feed publishing full test.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AtomPublishingControl_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);

                Assert.AreEqual(1, feed.Channel.Items.Count());
                var item = feed.Channel.Items.Single();
                var ext = item.HasExtensions;
                Assert.IsTrue(item.HasExtensions);
                var itemExtension = item.FindExtension<AtomPublishingControlSyndicationExtension>();
                Assert.IsNotNull(itemExtension);
                Assert.IsInstanceOfType(
                    item.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as
                        AtomPublishingControlSyndicationExtension,
                    typeof(AtomPublishingControlSyndicationExtension));
            }
        }

        /// <summary>
        /// A test for GetHashCode.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AtomPublishingControl_GetHashCodeTest()
        {
            AtomPublishingControlSyndicationExtension target = this.CreateExtension1();
            int expected = -1862124151;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Load.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AtomPublishingControl_LoadTest()
        {
            AtomPublishingControlSyndicationExtension
                target = new AtomPublishingControlSyndicationExtension(); // TODO: Initialize to an appropriate value
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
        public void AtomPublishingControl_MatchByTypeTest()
        {
            ISyndicationExtension extension = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = AtomPublishingControlSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Equality.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_op_EqualityTest_Failure()
        {
            AtomPublishingControlSyndicationExtension first = this.CreateExtension1();
            AtomPublishingControlSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test equality.
        /// </summary>
        public void AtomPublishingControl_op_EqualityTest_Success()
        {
            AtomPublishingControlSyndicationExtension first = this.CreateExtension1();
            AtomPublishingControlSyndicationExtension second = this.CreateExtension1();
            bool expected = true;
            bool actual;
            actual = first == second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_GreaterThan.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_op_GreaterThanTest()
        {
            AtomPublishingControlSyndicationExtension first = this.CreateExtension1();
            AtomPublishingControlSyndicationExtension second = this.CreateExtension2();
            bool expected = false;
            bool actual = false;
            actual = first > second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_Inequality.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_op_InequalityTest()
        {
            AtomPublishingControlSyndicationExtension first = this.CreateExtension1();
            AtomPublishingControlSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual = first != second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for op_LessThan.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_op_LessThanTest()
        {
            AtomPublishingControlSyndicationExtension first = this.CreateExtension1();
            AtomPublishingControlSyndicationExtension second = this.CreateExtension2();
            bool expected = true;
            bool actual;
            actual = first < second;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToString.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_ToStringTest()
        {
            AtomPublishingControlSyndicationExtension target = this.CreateExtension1();
            string expected = NycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for WriteTo.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControl_WriteToTest()
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
        /// A test for AtomPublishingControlSyndicationExtension Constructor.
        /// </summary>
        [TestMethod]
        public void AtomPublishingControlSyndicationExtensionConstructorTest()
        {
            AtomPublishingControlSyndicationExtension target = new AtomPublishingControlSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(AtomPublishingControlSyndicationExtension));
        }

        /// <summary>
        /// Create extension.
        /// </summary>
        /// <returns>Returns <see cref="AtomPublishingControlSyndicationExtension"/>.</returns>
        private AtomPublishingControlSyndicationExtension CreateExtension1()
        {
            var nyc = new AtomPublishingControlSyndicationExtension
                          {
                              Context =
                                  {
                                      BaseUri = new Uri(
                                          "http://www.example.com/control.html"),
                                      IsDraft = true,
                                      Language = new CultureInfo(
                                          "en-US"),
                                  },
                          };

            return nyc;
        }

        /// <summary>
        /// Create extension 2.
        /// </summary>
        /// <returns>Returns <see cref="AtomPublishingControlSyndicationExtension"/>.</returns>
        private AtomPublishingControlSyndicationExtension CreateExtension2()
        {
            var nyc = new AtomPublishingControlSyndicationExtension();
            nyc.Context.BaseUri = new Uri("http://www.example.net/control.html");
            nyc.Context.IsDraft = false;
            nyc.Context.Language = new CultureInfo("fr-CA");
            return nyc;
        }
    }
}