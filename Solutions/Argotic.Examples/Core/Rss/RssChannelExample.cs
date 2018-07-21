/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RssChannelExample Class
****************************************************************************/
using System;
using System.Globalization;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssChannel"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssChannel"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssChannelExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssChannel class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssChannel
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.Categories.Add(new RssCategory("Media"));
            feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

            feed.Channel.Cloud              = new RssCloud("server.example.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
            feed.Channel.Copyright          = "Copyright 2007 Dallas Times-Herald";
            feed.Channel.Generator          = "Microsoft Spaces v1.1";

            RssImage image                  = new RssImage(new Uri("http://dallas.example.com"), "Dallas Times-Herald", new Uri("http://dallas.example.com/masthead.gif"));
            image.Description               = "Read the Dallas Times-Herald";
            image.Height                    = 32;
            image.Width                     = 96;
            feed.Channel.Image              = image;

            feed.Channel.Language           = new CultureInfo("en-US");
            feed.Channel.LastBuildDate      = new DateTime(2007, 10, 14, 17, 17, 44);
            feed.Channel.ManagingEditor     = "jlehrer@dallas.example.com (Jim Lehrer)";
            feed.Channel.PublicationDate    = new DateTime(2007, 10, 14, 5, 0, 0);
            feed.Channel.Rating             = "(PICS-1.1 \"http://www.rsac.org/ratingsv01.html\" l by \"webmaster@example.com\" on \"2007.01.29T10:09-0800\" r (n 0 s 0 v 0 l 0))";

            feed.Channel.SkipDays.Add(DayOfWeek.Saturday);
            feed.Channel.SkipDays.Add(DayOfWeek.Sunday);

            feed.Channel.SkipHours.Add(0);
            feed.Channel.SkipHours.Add(1);
            feed.Channel.SkipHours.Add(2);
            feed.Channel.SkipHours.Add(22);
            feed.Channel.SkipHours.Add(23);

            feed.Channel.TextInput          = new RssTextInput("What software are you using?", new Uri("http://www.cadenhead.org/textinput.php"), "query", "TextInput Inquiry");
            feed.Channel.TimeToLive         = 60;
            feed.Channel.Webmaster          = "helpdesk@dallas.example.com";
            #endregion
        }
    }
}
