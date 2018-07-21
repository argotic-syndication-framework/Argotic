/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created AtomSourceExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="AtomSource"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="AtomSource"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class AtomSourceExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the AtomSource class.
        /// </summary>
        public static void ClassExample()
        {
            #region AtomSource
            AtomFeed feed   = new AtomFeed();

            feed.Id         = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
            feed.Title      = new AtomTextConstruct("Example Feed");
            feed.UpdatedOn  = new DateTime(2003, 12, 13, 18, 30, 2);

            feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
            feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

            feed.Authors.Add(new AtomPersonConstruct("John Doe"));

            AtomEntry entry = new AtomEntry();

            entry.Id        = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a"));
            entry.Title     = new AtomTextConstruct("Atom-Powered Robots Run Amok");
            entry.UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2);

            entry.Summary   = new AtomTextConstruct("Some text.");

            //  Entry was copied from another feed, so preserve source meta-data
            AtomSource source   = new AtomSource();
            source.Id           = new AtomId(new Uri("http://example2.org/"));
            source.Title        = new AtomTextConstruct("Fourty-Two");
            source.UpdatedOn    = new DateTime(2003, 11, 13, 18, 30, 2);
            source.Rights       = new AtomTextConstruct("© 2003 Example, Inc.");
            entry.Source        = source;

            feed.AddEntry(entry);
            #endregion
        }
    }
}
