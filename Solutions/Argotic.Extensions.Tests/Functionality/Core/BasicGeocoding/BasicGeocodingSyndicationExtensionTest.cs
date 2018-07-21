using System;
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
	///This is a test class for BasicGeocodingSyndicationExtensionTest and is intended
	///to contain all BasicGeocodingSyndicationExtensionTest Unit Tests
	///</summary>
	[TestClass()]
	public class BasicGeocodingSyndicationExtensionTest
	{

		const string namespc = @"xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""";

		private const string nycText = "<lat xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">40.0000000</lat>\r\n" +
									   "<long xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">-74.0000000</long>";

		private const string strExtXml = "<geo:lat>41.0000000</geo:lat><geo:long>-74.1200000</geo:long>";

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

	    // 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//

	    /// <summary>
		///A test for BasicGeocodingSyndicationExtension Constructor
		///</summary>
		[TestMethod()]
		public void BasicGeocodingSyndicationExtensionConstructorTest()
		{
			BasicGeocodingSyndicationExtension target = new BasicGeocodingSyndicationExtension();
			Assert.IsNotNull(target);
			Assert.IsInstanceOfType(target, typeof(BasicGeocodingSyndicationExtension));
		}

		/// <summary>
		///A test for CompareTo
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_CompareToTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
			object obj = CreateExtension1();
			int expected = 0; 
			int actual;
			actual = target.CompareTo(obj);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for ConvertDecimalToDegreesMinutesSeconds
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_ConvertDecimalToDegreesMinutesSecondsTest()
		{
			Decimal value = new Decimal(12.582438888888888888888888888889);
			string expected = "12°34'56.78\"";
			string actual;
			actual = BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(value);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for ConvertDegreesMinutesSecondsToDecimal
		///</summary>
		[TestMethod()]
		public void ConvertDegreesMinutesSecondsToDecimalTest()
		{
			string degreesMinutesSeconds = "12°34'56.78\"";
			Decimal expected = new Decimal(12.582438888888888888888888888889);
			Decimal actual;
			actual = BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal(degreesMinutesSeconds);
			Assert.AreEqual((double)expected, (double)actual, 3e-6);
		}

		/// <summary>
		///A test for Equals
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_EqualsTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
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
		public void BasicGeocoding_GetHashCodeTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
			int expected = -1112179344;
			int actual;
			actual = target.GetHashCode();
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Load
		///</summary>
		[TestMethod, Ignore]
		public void BasicGeocoding_LoadTest()
		{
			BasicGeocodingSyndicationExtension target = new BasicGeocodingSyndicationExtension(); // TODO: Initialize to an appropriate value
			var nt = new NameTable();
			var ns = new XmlNamespaceManager(nt);
			 var xpc = new XmlParserContext(nt, ns, "US-en",XmlSpace.Default);
			 var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

			using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc)	)
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
		public void BasicGeocoding_CreateXmlTest()
	  {
		  var geo = new BasicGeocodingSyndicationExtension();
		  geo.Context.Latitude = 41.0m;
		  geo.Context.Longitude = -74.12m;

		  var actual = ExtensionTestUtil.AddExtensionToXml(geo);
		  string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
		  Assert.AreEqual(expected, actual);
	  }


		[TestMethod]
	public void BasicGeocoding_FullTest()
		{
			var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

			 using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
			 {
				 RssFeed feed = new RssFeed();
				 feed.Load(reader);

				 //				 Assert.IsTrue(feed.Channel.HasExtensions);
				 //				 Assert.IsInstanceOfType(feed.Channel.FindExtension(BasicGeocodingSyndicationExtension.MatchByType) as BasicGeocodingSyndicationExtension,
				 //						 typeof(BasicGeocodingSyndicationExtension));

				 Assert.AreEqual(1, feed.Channel.Items.Count());
				 var item = feed.Channel.Items.Single();
				 Assert.IsTrue(item.HasExtensions);
				 var itemExtension = item.FindExtension<BasicGeocodingSyndicationExtension>();
				 Assert.IsNotNull(itemExtension);
				 Assert.IsInstanceOfType(item.FindExtension(BasicGeocodingSyndicationExtension.MatchByType) as BasicGeocodingSyndicationExtension,
				  typeof(BasicGeocodingSyndicationExtension));
			 }
		}

		/// <summary>
		///A test for MatchByType
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_MatchByTypeTest()
		{
			ISyndicationExtension extension = CreateExtension1();
			bool expected = true;
			bool actual;
			actual = BasicGeocodingSyndicationExtension.MatchByType(extension);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_ToStringTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
			string expected = nycText;
			string actual;
			actual = target.ToString();
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for WriteTo
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_WriteToTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
			using(var sw = new StringWriter())
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
		public void BasicGeocoding_op_EqualityTest_Failure()
		{
			BasicGeocodingSyndicationExtension first = CreateExtension1();
			BasicGeocodingSyndicationExtension second = CreateExtension2();
			bool expected = false; 
			bool actual;
			actual = (first == second);
			Assert.AreEqual(expected, actual);
		}

		public void BasicGeocoding_op_EqualityTest_Success()
		{
			BasicGeocodingSyndicationExtension first = CreateExtension1();
			BasicGeocodingSyndicationExtension second = CreateExtension1();
			bool expected = true;
			bool actual;
			actual = (first == second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_GreaterThan
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_op_GreaterThanTest()
		{
			BasicGeocodingSyndicationExtension first = CreateExtension1();
			BasicGeocodingSyndicationExtension second = CreateExtension2();
			bool expected = false; 
			bool actual = false;
			actual = (first > second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_Inequality
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_op_InequalityTest()
		{
			BasicGeocodingSyndicationExtension first = CreateExtension1();
			BasicGeocodingSyndicationExtension second = CreateExtension2();
			bool expected = true;
			bool actual = (first != second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_LessThan
		///</summary>
		[TestMethod()]
		public void BasicGeocoding_op_LessThanTest()
		{
			BasicGeocodingSyndicationExtension first = CreateExtension1();
			BasicGeocodingSyndicationExtension second = CreateExtension2();
			bool expected = true; 
			bool actual;
			actual = (first < second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Context
		///</summary>
		[TestMethod(), Ignore]
		public void BasicGeocoding_ContextTest()
		{
			BasicGeocodingSyndicationExtension target = CreateExtension1();
			BasicGeocodingSyndicationExtensionContext expected =CreateContext1();
			BasicGeocodingSyndicationExtensionContext actual;
//			target.Context = expected;
			actual = target.Context;
			var b = actual.Equals(expected);
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		private BasicGeocodingSyndicationExtension CreateExtension1()
		{
			var nyc = new BasicGeocodingSyndicationExtension();
			nyc.Context.Latitude = 40;
			nyc.Context.Longitude = -74;
			return nyc;
		}
		private BasicGeocodingSyndicationExtension CreateExtension2()
		{
			var nyc = new BasicGeocodingSyndicationExtension();
			nyc.Context.Latitude = 43;
			nyc.Context.Longitude = -80;
			return nyc;
		}

		public static BasicGeocodingSyndicationExtensionContext CreateContext1()
		{
			var nyc = new BasicGeocodingSyndicationExtensionContext();
			nyc.Latitude = 40;
			nyc.Longitude = -74;
			return nyc;
		}

	}
}
