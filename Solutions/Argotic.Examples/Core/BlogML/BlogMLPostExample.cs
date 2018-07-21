/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created BlogMLPostExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="BlogMLPost"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="BlogMLPost"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class BlogMLPostExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the BlogMLPost class.
        /// </summary>
        public static void ClassExample()
        {
            #region BlogMLPost
            BlogMLDocument document = new BlogMLDocument();

            document.RootUrl        = new Uri("/blogs/default.aspx");
            document.GeneratedOn    = new DateTime(2006, 9, 5, 18, 22, 10);
            document.Title          = new BlogMLTextConstruct("BlogML 2.0 Example");
            document.Subtitle       = new BlogMLTextConstruct("This is some sample blog content for BlogML 2.0");

            BlogMLAuthor administrator      = new BlogMLAuthor();
            administrator.Id                = "2100";
            administrator.CreatedOn         = new DateTime(2006, 8, 10, 8, 44, 35);
            administrator.LastModifiedOn    = new DateTime(2006, 9, 4, 13, 46, 38);
            administrator.ApprovalStatus    = BlogMLApprovalStatus.Approved;
            administrator.EmailAddress      = "someone@domain.com";
            administrator.Title             = new BlogMLTextConstruct("admin");
            document.Authors.Add(administrator);

            document.ExtendedProperties.Add("CommentModeration", "Anonymous");
            document.ExtendedProperties.Add("SendTrackback", "yes");

            BlogMLCategory category1    = new BlogMLCategory();
            category1.Id                = "1018";
            category1.CreatedOn         = new DateTime(2006, 9, 5, 17, 54, 58);
            category1.LastModifiedOn    = new DateTime(2006, 9, 5, 17, 54, 58);
            category1.ApprovalStatus    = BlogMLApprovalStatus.Approved;
            category1.Description       = "Sample Category 1";
            category1.ParentId          = "0";
            category1.Title             = new BlogMLTextConstruct("Category 1");
            document.Categories.Add(category1);

            BlogMLCategory category2    = new BlogMLCategory();
            category2.Id                = "1019";
            category2.CreatedOn         = new DateTime(2006, 9, 5, 17, 54, 59);
            category2.LastModifiedOn    = new DateTime(2006, 9, 5, 17, 54, 59);
            category2.ApprovalStatus    = BlogMLApprovalStatus.Approved;
            category2.Description       = "Sample Category 2";
            category2.ParentId          = "0";
            category2.Title             = new BlogMLTextConstruct("Category 2");
            document.Categories.Add(category2);

            BlogMLCategory category3    = new BlogMLCategory();
            category3.Id                = "1020";
            category3.CreatedOn         = new DateTime(2006, 9, 5, 17, 55, 0);
            category3.LastModifiedOn    = new DateTime(2006, 9, 5, 17, 55, 0);
            category3.ApprovalStatus    = BlogMLApprovalStatus.NotApproved;
            category3.Description       = "Sample Category 3";
            category3.ParentId          = "0";
            category3.Title             = new BlogMLTextConstruct("Category 3");
            document.Categories.Add(category3);

            //  Create a blog entry
            BlogMLPost post         = new BlogMLPost();
            post.Id                 = "34";
            post.CreatedOn          = new DateTime(2006, 9, 5, 3, 19, 0);
            post.LastModifiedOn     = new DateTime(2006, 9, 5, 3, 19, 0);
            post.ApprovalStatus     = BlogMLApprovalStatus.Approved;
            post.Url                = new Uri("/blogs/archive/2006/09/05/Sample-Blog-Post.aspx");
            post.PostType           = BlogMLPostType.Normal;
            post.Views              = "0";
            post.Title              = new BlogMLTextConstruct("Sample Blog Post");
            post.Content            = new BlogMLTextConstruct("<p>This is <b>HTML encoded</b> content.&nbsp;</p>", BlogMLContentType.Html);
            post.Name               = new BlogMLTextConstruct("Sample Blog Post");

            post.Categories.Add("1018");
            post.Categories.Add("1020");

            post.Authors.Add("2100");

            BlogMLComment comment   = new BlogMLComment();
            comment.Id              = "35";
            comment.CreatedOn       = new DateTime(2006, 9, 5, 11, 36, 50);
            comment.LastModifiedOn  = new DateTime(2006, 9, 5, 11, 36, 50);
            comment.Title           = new BlogMLTextConstruct("re: Sample Blog Post");
            comment.Content         = new BlogMLTextConstruct("This is a test comment.");
            post.Comments.Add(comment);
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the BlogMLPost.PostTypeAsString(BlogMLPostType) method
        /// </summary>
        public static void PostTypeAsStringExample()
        {
            #region PostTypeAsString(BlogMLPostType type)
            string postType = BlogMLPost.PostTypeAsString(BlogMLPostType.Normal);   // normal

            if (String.Compare(postType, "normal", StringComparison.OrdinalIgnoreCase) == 0)
            {
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the BlogMLPost.PostTypeByName(string) method
        /// </summary>
        public static void PostTypeByNameExample()
        {
            #region PostTypeByName(string name)
            BlogMLPostType postType = BlogMLPost.PostTypeByName("normal");

            if (postType == BlogMLPostType.Normal)
            {
            }
            #endregion
        }
    }
}
