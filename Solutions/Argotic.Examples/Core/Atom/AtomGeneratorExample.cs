/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created AtomGeneratorExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="AtomGenerator"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="AtomGenerator"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class AtomGeneratorExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the AtomGenerator class.
        /// </summary>
        public static void ClassExample()
        {
            #region AtomGenerator
            AtomFeed feed   = new AtomFeed();

            feed.Id         = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
            feed.Title      = new AtomTextConstruct("Example Feed");
            feed.UpdatedOn  = new DateTime(2003, 12, 13, 18, 30, 2);

            feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
            feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

            feed.Authors.Add(new AtomPersonConstruct("John Doe"));

            //  Identify the software used to generate the feed
            AtomGenerator generator = new AtomGenerator("Example Toolkit");
            generator.Uri           = new Uri("/myblog.php");
            generator.Version       = "1.0";
            feed.Generator          = generator;

            AtomEntry entry = new AtomEntry();

            entry.Id        = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a"));
            entry.Title     = new AtomTextConstruct("Atom-Powered Robots Run Amok");
            entry.UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2);

            entry.Summary   = new AtomTextConstruct("Some text.");

            feed.AddEntry(entry);
            #endregion
        }
    }
}
