# How to generate Web Log Markup Language (BlogML) formatted content

[Web Log Markup Language](http://blogml.org/) (BlogML) provides an open format derived from XML to store and restore the content of a blog. As the number of web log software platforms grows, there is a need to be able to import and export blog content in a portable, open format; and so the BlogML format was created to ease the difficulties faced when attempting to move web log content from one software platform to another.

The classes that together compose the implementation of BlogML are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with BlogML formated syndication resources is the _BlogMLDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated web log content. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated portable web log content and attempt to handle malformed XML data.

One of the core principals of the framework is "**Read flexibly, Write strictly**". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

To create a new BlogML portable description of web log content, you simply instantiate a new instance of the _BlogMLDocument_ class and utilize its properties and methods to describe the web log content you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

	using System.IO;
	using Argotic.Syndication.Specialized;

	BlogMLDocument document = new BlogMLDocument();

	document.RootUrl = new Uri("/blogs/default.aspx");
	document.GeneratedOn = new DateTime(2006, 9, 5, 18, 22, 10);
	document.Title = new BlogMLTextConstruct("BlogML 2.0 Example");
	document.Subtitle = new BlogMLTextConstruct("This is some sample blog content for BlogML 2.0");

	BlogMLAuthor administrator = new BlogMLAuthor();
	administrator.Id = "2100";
	administrator.CreatedOn = new DateTime(2006, 8, 10, 8, 44, 35);
	administrator.LastModifiedOn = new DateTime(2006, 9, 4, 13, 46, 38);
	administrator.ApprovalStatus = BlogMLApprovalStatus.Approved;
	administrator.EmailAddress = "someone@domain.com";
	administrator.Title = new BlogMLTextConstruct("admin");
	document.Authors.Add(administrator);

	document.ExtendedProperties.Add("CommentModeration", "Anonymous");
	document.ExtendedProperties.Add("SendTrackback", "yes");

	BlogMLCategory category1 = new BlogMLCategory();
	category1.Id = "1018";
	category1.CreatedOn = new DateTime(2006, 9, 5, 17, 54, 58);
	category1.LastModifiedOn = new DateTime(2006, 9, 5, 17, 54, 58);
	category1.ApprovalStatus = BlogMLApprovalStatus.Approved;
	category1.Description = "Sample Category 1";
	category1.ParentId = "0";
	category1.Title = new BlogMLTextConstruct("Category 1");
	document.Categories.Add(category1);

	BlogMLCategory category2 = new BlogMLCategory();
	category2.Id = "1019";
	category2.CreatedOn = new DateTime(2006, 9, 5, 17, 54, 59);
	category2.LastModifiedOn = new DateTime(2006, 9, 5, 17, 54, 59);
	category2.ApprovalStatus = BlogMLApprovalStatus.Approved;
	category2.Description = "Sample Category 2";
	category2.ParentId = "0";
	category2.Title = new BlogMLTextConstruct("Category 2");
	document.Categories.Add(category2);

	BlogMLCategory category3 = new BlogMLCategory();
	category3.Id = "1020";
	category3.CreatedOn = new DateTime(2006, 9, 5, 17, 55, 0);
	category3.LastModifiedOn = new DateTime(2006, 9, 5, 17, 55, 0);
	category3.ApprovalStatus = BlogMLApprovalStatus.NotApproved;
	category3.Description = "Sample Category 3";
	category3.ParentId = "0";
	category3.Title = new BlogMLTextConstruct("Category 3");
	document.Categories.Add(category3);

	BlogMLPost post = new BlogMLPost();
	post.Id = "34";
	post.CreatedOn = new DateTime(2006, 9, 5, 3, 19, 0);
	post.LastModifiedOn = new DateTime(2006, 9, 5, 3, 19, 0);
	post.ApprovalStatus = BlogMLApprovalStatus.Approved;
	post.Url = new Uri("/blogs/archive/2006/09/05/Sample-Blog-Post.aspx");
	post.PostType = BlogMLPostType.Normal;
	post.Views = "0";
	post.Title = new BlogMLTextConstruct("Sample Blog Post");
	post.Content = new BlogMLTextConstruct("<p>This is <b>HTML encoded</b> content.&nbsp;</p>", BlogMLContentType.Html);
	post.Name = new BlogMLTextConstruct("Sample Blog Post");

	post.Categories.Add("1018");
	post.Categories.Add("1020");

	post.Authors.Add("2100");

	BlogMLComment comment = new BlogMLComment();
	comment.Id = "35";
	comment.CreatedOn = new DateTime(2006, 9, 5, 11, 36, 50);
	comment.LastModifiedOn  = new DateTime(2006, 9, 5, 11, 36, 50);
	comment.Title = new BlogMLTextConstruct("re: Sample Blog Post");
	comment.Content = new BlogMLTextConstruct("This is a test comment.");
	post.Comments.Add(comment);

The BlogML description generated in the above example represents an example of a categorized, multiple author web log and includes post comments.