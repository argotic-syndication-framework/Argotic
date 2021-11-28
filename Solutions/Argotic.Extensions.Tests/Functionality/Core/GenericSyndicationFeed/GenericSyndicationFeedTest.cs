namespace Argotic.Extensions.Tests
{
	using Argotic.Extensions.Core;
	using Argotic.Syndication;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using System.Globalization;

	/// <summary>
	///This is a test class for GenericSyndicationFeedTest and is intended
	///to contain all GenericSyndicationFeedTest Unit Tests
	///</summary>
	[TestClass()]
	public class GenericSyndicationFeedTest
	{
		private string namespc = @"xmlns:app=""http://www.wdr.de/rss/1.0/modules/app/1.0/""";

        private const string strExtXml = "<app:sportverlagID>ma9242941</app:sportverlagID>"
                                        + "<app:tags>Fußball, Bundesliga, Netcast, 11. Spieltag, Saison 2021/2022, Zusammenfassung, VFL, TSG</app:tags>"
                                        + "<app:teasertyp>teaserleiste</app:teasertyp>"
                                        + "<app:componentid>8934211509154180752</app:componentid>"
                                        + "<app:componenttitle>Videos 1. Bundesliga</app:componenttitle>"
                                        + "<app:highlight>nein</app:highlight>";

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

        [TestMethod, TestCategory("fix-39")]
        public void TestCustomXmlNamespace()
        {
            //   xmlns:content=""http://purl.org/rss/1.0/modules/content/""
            var xml = @"<rss xmlns:app=""http:/example.com"" version=""2.0""></rss>";

            var feed = new GenericSyndicationFeed();

            feed.Load(xml);
            Assert.AreNotSame(new GenericSyndicationFeed(), feed);
        }
    }
}