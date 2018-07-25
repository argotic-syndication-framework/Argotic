﻿namespace Argotic.Examples
{
    using System;

    using Argotic.Syndication.Specialized;

    /// <summary>
    /// Contains the code examples for the <see cref="BlogMLTextConstruct"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="BlogMLTextConstruct"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class BlogMLTextConstructExample
    {
        /// <summary>
        /// Provides example code for the BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType) method.
        /// </summary>
        public static void ConstructTypeAsStringExample()
        {
            string contentType = BlogMLTextConstruct.ConstructTypeAsString(BlogMLContentType.Html); // html

            if (string.Compare(contentType, "html", StringComparison.OrdinalIgnoreCase) == 0)
            {
            }
        }

        /// <summary>
        /// Provides example code for the BlogMLTextConstruct.ConstructTypeByName(string) method.
        /// </summary>
        public static void ConstructTypeByNameExample()
        {
            BlogMLContentType contentType = BlogMLTextConstruct.ConstructTypeByName("html");

            if (contentType == BlogMLContentType.Html)
            {
            }
        }
    }
}