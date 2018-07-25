﻿namespace Argotic.Examples
{
    using System;

    using Argotic.Syndication;

    /// <summary>
    /// Contains the code examples for the <see cref="RssTextInput"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssTextInput"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    public static class RssTextInputExample
    {
        /// <summary>
        /// Provides example code for the RssTextInput class.
        /// </summary>
        public static void ClassExample()
        {
            RssFeed feed = new RssFeed();

            feed.Channel.Title = "Dallas Times-Herald";
            feed.Channel.Link = new Uri("http://dallas.example.com");
            feed.Channel.Description = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.TextInput = new RssTextInput(
                "What software are you using?",
                new Uri("http://www.cadenhead.org/textinput.php"),
                "query",
                "TextInput Inquiry");
        }
    }
}