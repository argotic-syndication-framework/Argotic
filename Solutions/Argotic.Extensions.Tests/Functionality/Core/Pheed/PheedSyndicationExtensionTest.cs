﻿using System;
using System.IO;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;

namespace Argotic.Extensions.Tests
{
    /// <summary>
    ///This is a test class for PheedSyndicationExtensionTest and is intended
    ///to contain all PheedSyndicationExtensionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PheedSyndicationExtensionTest
    {
        const string namespc = @"xmlns:photo=""http://www.pheed.com/pheed/""";

        private string nycText = "<thumbnail xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/thumbnail.jpg</thumbnail>" + Environment.NewLine
                                       + "<imgsrc xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/</imgsrc>";


        private const string strExtXml = "<photo:thumbnail>http://www.example.com/thumbnail.jpg</photo:thumbnail><photo:imgsrc>http://www.example.com/</photo:imgsrc>";

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
        ///A test for PheedSyndicationExtension Constructor
        ///</summary>
        [TestMethod()]
        public void PheedSyndicationExtensionConstructorTest()
        {
            PheedSyndicationExtension target = new PheedSyndicationExtension();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(PheedSyndicationExtension));
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void Pheed_CompareToTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
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
        public void Pheed_EqualsTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
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
        public void Pheed_GetHashCodeTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
            int expected = -1671096665;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod, Ignore]
        public void Pheed_LoadTest()
        {
            PheedSyndicationExtension target = new PheedSyndicationExtension(); // TODO: Initialize to an appropriate value
            var nt = new NameTable();
            var ns = new XmlNamespaceManager(nt);
            var xpc = new XmlParserContext(nt, ns, "US-en", XmlSpace.Default);
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);
            }
        }

        [TestMethod]
        public void Pheed_CreateXmlTest()
        {
            var pheed = new PheedSyndicationExtension();

            pheed.Context.Source = new Uri("http://www.example.com");
            pheed.Context.Thumbnail = new Uri("http://www.example.com/thumbnail.jpg");

            var actual = ExtensionTestUtil.AddExtensionToXml(pheed);
            string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Pheed_FullTest()
        {
            var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

            using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
            {
                RssFeed feed = new RssFeed();
                feed.Load(reader);
                Assert.AreEqual(1, feed.Channel.Items.Count());
                var item = feed.Channel.Items.Single();
                Assert.IsTrue(item.HasExtensions);
                var itemExtension = item.FindExtension<PheedSyndicationExtension>();
                Assert.IsNotNull(itemExtension);
                Assert.IsInstanceOfType(
                    item.FindExtension(PheedSyndicationExtension.MatchByType) as PheedSyndicationExtension,
                    typeof(PheedSyndicationExtension));

            }
        }

        /// <summary>
        ///A test for MatchByType
        ///</summary>
        [TestMethod()]
        public void Pheed_MatchByTypeTest()
        {
            ISyndicationExtension extension = CreateExtension1();
            bool expected = true;
            bool actual;
            actual = PheedSyndicationExtension.MatchByType(extension);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void Pheed_ToStringTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
            string expected = nycText;
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for WriteTo
        ///</summary>
        [TestMethod()]
        public void Pheed_WriteToTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
            using (var sw = new StringWriter())
            using (XmlWriter writer = new XmlTextWriter(sw))
            {

                target.WriteTo(writer);
                var output = sw.ToString();
                Assert.AreEqual(nycText.Replace(Environment.NewLine, ""), output.Replace(Environment.NewLine, ""));
            }
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void Pheed_op_EqualityTest_Failure()
        {
            PheedSyndicationExtension first = CreateExtension1();
            PheedSyndicationExtension second = CreateExtension2();
            bool expected = false;
            bool actual;
            actual = (first == second);
            Assert.AreEqual(expected, actual);
        }

        public void Pheed_op_EqualityTest_Success()
        {
            PheedSyndicationExtension first = CreateExtension1();
            PheedSyndicationExtension second = CreateExtension1();
            bool expected = true;
            bool actual;
            actual = (first == second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void Pheed_op_GreaterThanTest()
        {
            PheedSyndicationExtension first = CreateExtension1();
            PheedSyndicationExtension second = CreateExtension2();
            bool expected = false;
            bool actual = false;
            actual = (first > second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void Pheed_op_InequalityTest()
        {
            PheedSyndicationExtension first = CreateExtension1();
            PheedSyndicationExtension second = CreateExtension2();
            bool expected = true;
            bool actual = (first != second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void Pheed_op_LessThanTest()
        {
            PheedSyndicationExtension first = CreateExtension1();
            PheedSyndicationExtension second = CreateExtension2();
            bool expected = true;
            bool actual;
            actual = (first < second);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Context
        ///</summary>
        [TestMethod(), Ignore]
        public void Pheed_ContextTest()
        {
            PheedSyndicationExtension target = CreateExtension1();
            PheedSyndicationExtensionContext expected = CreateContext1();
            PheedSyndicationExtensionContext actual;
            //			target.Context = expected;
            actual = target.Context;
            var b = actual.Equals(expected);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        private PheedSyndicationExtension CreateExtension1()
        {
            var nyc = new PheedSyndicationExtension();
            nyc.Context.Source = new Uri("http://www.example.com");
            nyc.Context.Thumbnail = new Uri("http://www.example.com/thumbnail.jpg");

            return nyc;
        }

        private PheedSyndicationExtension CreateExtension2()
        {
            var nyc = new PheedSyndicationExtension();
            nyc.Context.Source = new Uri("http://www.example.net");
            nyc.Context.Thumbnail = new Uri("http://www.example.net/thumbnail.png");

            return nyc;
        }

        public static PheedSyndicationExtensionContext CreateContext1()
        {
            var nyc = new PheedSyndicationExtensionContext();
            nyc.Source = new Uri("http://www.example.com");
            nyc.Thumbnail = new Uri("http://www.example.com/thumbnail.jpg");

            return nyc;
        }
    }
}