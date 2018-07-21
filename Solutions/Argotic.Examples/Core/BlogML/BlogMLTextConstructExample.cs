/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created BlogMLTextConstructExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="BlogMLTextConstruct"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="BlogMLTextConstruct"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class BlogMLTextConstructExample
    {
        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType) method
        /// </summary>
        public static void ConstructTypeAsStringExample()
        {
            #region ConstructTypeAsString(BlogMLContentType type)
            string contentType  = BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType.Html);    // html

            if (String.Compare(contentType, "html", StringComparison.OrdinalIgnoreCase) == 0)
            {
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the BlogMLTextConstruct.ConstructTypeByName(string) method
        /// </summary>
        public static void ConstructTypeByNameExample()
        {
            #region ConstructTypeByName(string name)
            BlogMLContentType contentType   = BlogMLTextConstruct.ConstructTypeByName("html");

            if (contentType == BlogMLContentType.Html)
            {
            }
            #endregion
        }
    }
}
