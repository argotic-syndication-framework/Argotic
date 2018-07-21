/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RsdApplicationInterfaceExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RsdApplicationInterface"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RsdApplicationInterface"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsd")]
    public static class RsdApplicationInterfaceExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RsdApplicationInterface class.
        /// </summary>
        public static void ClassExample()
        {
            #region RsdApplicationInterface
            RsdDocument document    = new RsdDocument();

            document.EngineName     = "Blog Munging CMS";
            document.EngineLink     = new Uri("http://www.blogmunging.com/");
            document.Homepage       = new Uri("http://www.userdomain.com/");

            //  Identify supported services using well known names
            document.AddInterface(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

            RsdApplicationInterface conversantApi   = new RsdApplicationInterface("Conversant", new Uri("http://example.com/xml/rpc/url"), false, String.Empty);
            conversantApi.Documentation             = new Uri("http://www.conversant.com/docs/api/");
            conversantApi.Notes                     = "Additional explanation here.";
            conversantApi.Settings.Add("service-specific-setting", "a value");
            conversantApi.Settings.Add("another-setting", "another value");
            document.AddInterface(conversantApi);
            #endregion
        }
    }
}
