/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/11/2008	brian.kuhn	Created BlogMLPost Class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Represents information that describes a web log entry.
    /// </summary>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the BlogMLPost class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\BlogML\BlogMLPostExample.cs" 
    ///             region="BlogMLPost" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    public class BlogMLPost : IBlogMLCommonObject, IComparable, IExtensibleSyndicationObject
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the title of the web log entity.
        /// </summary>
        private BlogMLTextConstruct commonObjectBaseTitle           = new BlogMLTextConstruct();
        /// <summary>
        /// Private member to hold a unique identifier for the  web log entity.
        /// </summary>
        private string commonObjectBaseId                           = String.Empty;
        /// <summary>
        /// Private member to hold a date-time indicating when the  web log entity information was created.
        /// </summary>
        private DateTime commonObjectBaseCreatedOn                  = DateTime.MinValue;
        /// <summary>
        /// Private member to hold a date-time indicating when the  web log entity information was last modified.
        /// </summary>
        private DateTime commonObjectBaseLastModifiedOn             = DateTime.MinValue;
        /// <summary>
        /// Private member to hold a value indicating the web log entity approval status.
        /// </summary>
        private BlogMLApprovalStatus commonObjectBaseApprovalStatus = BlogMLApprovalStatus.None;
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
        /// <summary>
        /// Private member to hold the textual content of the post.
        /// </summary>
        private BlogMLTextConstruct postContent                     = new BlogMLTextConstruct();
        /// <summary>
        /// Private member to hold the name of the post.
        /// </summary>
        private BlogMLTextConstruct postName;
        /// <summary>
        /// Private member to hold the excerpt of the post.
        /// </summary>
        private BlogMLTextConstruct postExcerpt;
        /// <summary>
        /// Private member to hold references to authors of the post.
        /// </summary>
        private Collection<string> postAuthors;
        /// <summary>
        /// Private member to hold references to categories for the post.
        /// </summary>
        private Collection<string> postCategories;
        /// <summary>
        /// Private member to hold comments for the post.
        /// </summary>
        private Collection<BlogMLComment> postComments;
        /// <summary>
        /// Private member to hold trackbacks for the post.
        /// </summary>
        private Collection<BlogMLTrackback> postTrackbacks;
        /// <summary>
        /// Private member to hold attachments for the post.
        /// </summary>
        private Collection<BlogMLAttachment> postAttachments;
        /// <summary>
        /// Private member to hold the URL of the post.
        /// </summary>
        private Uri postUrl;
        /// <summary>
        /// Private member to hold the type of web log entry the post represents.
        /// </summary>
        private BlogMLPostType postType                             = BlogMLPostType.None;
        /// <summary>
        /// Private member to hold views of the post.
        /// </summary>
        private string postViews                                    = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region BlogMLPost()
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogMLPost"/> class.
        /// </summary>
        public BlogMLPost()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	COMMON PROPERTIES
        //============================================================
        #region ApprovalStatus
        /// <summary>
        /// Gets or sets the approval status of this web log entity.
        /// </summary>
        /// <value>
        ///     An <see cref="BlogMLApprovalStatus"/> enumeration value that represents whether this web log entity was approved to be publicly available. 
        ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified.
        /// </value>
        public BlogMLApprovalStatus ApprovalStatus
        {
            get
            {
                return commonObjectBaseApprovalStatus;
            }

            set
            {
                commonObjectBaseApprovalStatus = value;
            }
        }
        #endregion

        #region CreatedOn
        /// <summary>
        /// Gets or sets a date-time indicating when this web log entity was created.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates an instant in time associated with an event early in the life cycle of this web log entity. 
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime CreatedOn
        {
            get
            {
                return commonObjectBaseCreatedOn;
            }

            set
            {
                commonObjectBaseCreatedOn = value;
            }
        }
        #endregion

        #region Id
        /// <summary>
        /// Gets or sets the unique identifier of this web log entity.
        /// </summary>
        /// <value>An identification string for this web log entity. The default value is an <b>empty</b> string, which indicated that no identifier was specified.</value>
        public string Id
        {
            get
            {
                return commonObjectBaseId;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    commonObjectBaseId = String.Empty;
                }
                else
                {
                    commonObjectBaseId = value.Trim();
                }
            }
        }
        #endregion

        #region LastModifiedOn
        /// <summary>
        /// Gets or sets a date-time indicating when this web log entity was last modified.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this web log entity was modified in a way the publisher considers significant. 
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime LastModifiedOn
        {
            get
            {
                return commonObjectBaseLastModifiedOn;
            }

            set
            {
                commonObjectBaseLastModifiedOn = value;
            }
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets the title of this web log entity.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the title of this web log entity.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public BlogMLTextConstruct Title
        {
            get
            {
                return commonObjectBaseTitle;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                commonObjectBaseTitle = value;
            }
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY PROPERTIES
        //============================================================
        #region Extensions
        /// <summary>
        /// Gets or sets the syndication extensions applied to this syndication entity.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (objectSyndicationExtensions == null)
                {
                    objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }
                return objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                objectSyndicationExtensions = value;
            }
        }
        #endregion

        #region HasExtensions
        /// <summary>
        /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Attachments
        /// <summary>
        /// Gets the attachments for this post.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="BlogMLAttachment"/> objects that represent the attachments for this post.</value>
        public Collection<BlogMLAttachment> Attachments
        {
            get
            {
                if (postAttachments == null)
                {
                    postAttachments = new Collection<BlogMLAttachment>();
                }
                return postAttachments;
            }
        }
        #endregion

        #region Authors
        /// <summary>
        /// Gets the authors of this post.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of strings that represent references to the authors of this post.</value>
        /// <remarks>
        ///     The authors referenced by this collection <i>should</i> be located in the post's parent document <see cref="BlogMLDocument.Authors"/> collection.
        /// </remarks>
        public Collection<string> Authors
        {
            get
            {
                if (postAuthors == null)
                {
                    postAuthors = new Collection<string>();
                }
                return postAuthors;
            }
        }
        #endregion

        #region Categories
        /// <summary>
        /// Gets the categories for this post.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of strings that represent references to the categories for this post.</value>
        /// <remarks>
        ///     The categories referenced by this collection <i>should</i> be located in the post's parent document <see cref="BlogMLDocument.Categories"/> collection.
        /// </remarks>
        public Collection<string> Categories
        {
            get
            {
                if (postCategories == null)
                {
                    postCategories = new Collection<string>();
                }
                return postCategories;
            }
        }
        #endregion

        #region Comments
        /// <summary>
        /// Gets the comments for this post.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="BlogMLComment"/> objects that represent the comments for this post.</value>
        public Collection<BlogMLComment> Comments
        {
            get
            {
                if (postComments == null)
                {
                    postComments = new Collection<BlogMLComment>();
                }
                return postComments;
            }
        }
        #endregion

        #region Content
        /// <summary>
        /// Gets or sets the content of this post.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> that represents the content of this post.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public BlogMLTextConstruct Content
        {
            get
            {
                return postContent;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                postContent = value;
            }
        }
        #endregion

        #region Excerpt
        /// <summary>
        /// Gets or sets the excerpt of this post.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> that represents an excerpt of this post.</value>
        public BlogMLTextConstruct Excerpt
        {
            get
            {
                return postExcerpt;
            }

            set
            {
                postExcerpt = value;
            }
        }
        #endregion

        #region HasExcerpt
        /// <summary>
        /// Gets a value indicating if this post has an excerpt.
        /// </summary>
        /// <value><b>true</b> if this post's <see cref="Excerpt"/> is not null; otherwise <b>false</b>.</value>
        public bool HasExcerpt
        {
            get
            {
                return this.Excerpt != null;
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Gets or sets the name of this post.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> that represents the name of this post.</value>
        public BlogMLTextConstruct Name
        {
            get
            {
                return postName;
            }

            set
            {
                postName = value;
            }
        }
        #endregion

        #region PostType
        /// <summary>
        /// Gets or sets the type of web log entry this post represents.
        /// </summary>
        /// <value>
        ///     An <see cref="BlogMLPostType"/> enumeration value that represents the type of web log entry this post represents. 
        ///     The default value is <see cref="BlogMLPostType.None"/>.
        /// </value>
        public BlogMLPostType PostType
        {
            get
            {
                return postType;
            }

            set
            {
                postType = value;
            }
        }
        #endregion

        #region Trackbacks
        /// <summary>
        /// Gets the trackbacks for this post.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="BlogMLTrackback"/> objects that represent the trackbacks for this post.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackbacks")]
        public Collection<BlogMLTrackback> Trackbacks
        {
            get
            {
                if (postTrackbacks == null)
                {
                    postTrackbacks = new Collection<BlogMLTrackback>();
                }
                return postTrackbacks;
            }
        }
        #endregion

        #region Url
        /// <summary>
        /// Gets or sets the URL of this post.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of this post.</value>
        public Uri Url
        {
            get
            {
                return postUrl;
            }

            set
            {
                postUrl = value;
            }
        }
        #endregion

        #region Views
        /// <summary>
        /// Gets or sets the views of this post.
        /// </summary>
        /// <value>The views of this post.</value>
        public string Views
        {
            get
            {
                return postViews;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    postViews = String.Empty;
                }
                else
                {
                    postViews = value.Trim();
                }
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<BlogMLAttachment> source, Collection<BlogMLAttachment> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{BlogMLAttachment}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<BlogMLAttachment> source, Collection<BlogMLAttachment> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }
        #endregion

        #region CompareSequence(Collection<BlogMLAuthor> source, Collection<BlogMLAuthor> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{BlogMLAuthor}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<BlogMLAuthor> source, Collection<BlogMLAuthor> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }
        #endregion

        #region CompareSequence(Collection<BlogMLComment> source, Collection<BlogMLComment> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{BlogMLComment}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<BlogMLComment> source, Collection<BlogMLComment> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }
        #endregion

        #region CompareSequence(Collection<BlogMLTrackback> source, Collection<BlogMLTrackback> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{BlogMLTrackback}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<BlogMLTrackback> source, Collection<BlogMLTrackback> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }
        #endregion

        #region PostTypeAsString(BlogMLPostType type)
        /// <summary>
        /// Returns the post type identifier for the supplied <see cref="BlogMLPostType"/>.
        /// </summary>
        /// <param name="type">The <see cref="BlogMLPostType"/> to get the post type identifier for.</param>
        /// <returns>The post type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the PostTypeAsString method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\BlogML\BlogMLPostExample.cs" 
        ///             region="PostTypeAsString(BlogMLPostType type)" 
        ///         />
        ///     </code>
        /// </example>
        public static string PostTypeAsString(BlogMLPostType type)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(BlogMLPostType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLPostType))
                {
                    BlogMLPostType postType = (BlogMLPostType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (postType == type)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region PostTypeByName(string name)
        /// <summary>
        /// Returns the <see cref="BlogMLPostType"/> enumeration value that corresponds to the specified post type name.
        /// </summary>
        /// <param name="name">The name of the post type.</param>
        /// <returns>A <see cref="BlogMLPostType"/> enumeration value that corresponds to the specified string, otherwise returns <b>BlogMLPostType.None</b>.</returns>
        /// <remarks>This method disregards case of specified post type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the PostTypeByName method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\BlogML\BlogMLPostExample.cs" 
        ///             region="PostTypeByName(string name)" 
        ///         />
        ///     </code>
        /// </example>
        public static BlogMLPostType PostTypeByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            BlogMLPostType postType = BlogMLPostType.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(BlogMLPostType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLPostType))
                {
                    BlogMLPostType type      = (BlogMLPostType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            postType    = type;
                            break;
                        }
                    }
                }
            }

            return postType;
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY METHODS
        //============================================================
        #region AddExtension(ISyndicationExtension extension)
        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded   = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Add syndication extension to collection
            //------------------------------------------------------------
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded    = true;

            return wasAdded;
        }
        #endregion

        #region FindExtension(Predicate<ISyndicationExtension> match)
        /// <summary>
        /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
        /// <returns>
        ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
        /// </returns>
        /// <remarks>
        ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate. 
        ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in 
        ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(match, "match");

            //------------------------------------------------------------
            //	Perform predicate based search
            //------------------------------------------------------------
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }
        #endregion

        #region RemoveExtension(ISyndicationExtension extension)
        /// <summary>
        /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Remove syndication extension from collection
            //------------------------------------------------------------
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved  = true;
            }

            return wasRemoved;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="BlogMLPost"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="BlogMLPost"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);

            //------------------------------------------------------------
            //	Attempt to extract common attributes information
            //------------------------------------------------------------
            if (BlogMLUtility.FillCommonObject(this, source))
            {
                wasLoaded   = true;
            }

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasAttributes)
            {
                string postUrlAttribute = source.GetAttribute("post-url", String.Empty);
                string typeAttribute    = source.GetAttribute("type", String.Empty);
                string viewsAttribute   = source.GetAttribute("views", String.Empty);

                if (!String.IsNullOrEmpty(postUrlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url    = url;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    BlogMLPostType type = BlogMLPost.PostTypeByName(typeAttribute);
                    if (type != BlogMLPostType.None)
                    {
                        this.PostType   = type;
                        wasLoaded       = true;
                    }
                }

                if (!String.IsNullOrEmpty(viewsAttribute))
                {
                    this.Views  = viewsAttribute;
                    wasLoaded   = true;
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator contentNavigator     = source.SelectSingleNode("blog:content", manager);
                XPathNavigator postNameNavigator    = source.SelectSingleNode("blog:post-name", manager);
                XPathNavigator excerptNavigator     = source.SelectSingleNode("blog:excerpt", manager);

                if (contentNavigator != null)
                {
                    BlogMLTextConstruct content = new BlogMLTextConstruct();
                    if (content.Load(contentNavigator))
                    {
                        this.Content    = content;
                        wasLoaded       = true;
                    }
                }

                if (postNameNavigator != null)
                {
                    BlogMLTextConstruct name    = new BlogMLTextConstruct();
                    if (name.Load(postNameNavigator))
                    {
                        this.Name   = name;
                        wasLoaded   = true;
                    }
                }

                if (excerptNavigator != null)
                {
                    BlogMLTextConstruct excerpt = new BlogMLTextConstruct();
                    if (excerpt.Load(excerptNavigator))
                    {
                        this.Excerpt    = excerpt;
                        wasLoaded       = true;
                    }
                }

                if (BlogMLPost.FillPostCollections(this, source, manager))
                {
                    wasLoaded   = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);

            //------------------------------------------------------------
            //	Attempt to extract common attributes information
            //------------------------------------------------------------
            if (BlogMLUtility.FillCommonObject(this, source, settings))
            {
                wasLoaded   = true;
            }

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasAttributes)
            {
                string postUrlAttribute = source.GetAttribute("post-url", String.Empty);
                string typeAttribute    = source.GetAttribute("type", String.Empty);
                string viewsAttribute   = source.GetAttribute("views", String.Empty);

                if (!String.IsNullOrEmpty(postUrlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.Url    = url;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    BlogMLPostType type = BlogMLPost.PostTypeByName(typeAttribute);
                    if (type != BlogMLPostType.None)
                    {
                        this.PostType   = type;
                        wasLoaded       = true;
                    }
                }

                if (!String.IsNullOrEmpty(viewsAttribute))
                {
                    this.Views  = viewsAttribute;
                    wasLoaded   = true;
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator contentNavigator     = source.SelectSingleNode("blog:content", manager);
                XPathNavigator postNameNavigator    = source.SelectSingleNode("blog:post-name", manager);
                XPathNavigator excerptNavigator     = source.SelectSingleNode("blog:excerpt", manager);

                if (contentNavigator != null)
                {
                    BlogMLTextConstruct content = new BlogMLTextConstruct();
                    if (content.Load(contentNavigator, settings))
                    {
                        this.Content    = content;
                        wasLoaded       = true;
                    }
                }

                if (postNameNavigator != null)
                {
                    BlogMLTextConstruct name    = new BlogMLTextConstruct();
                    if (name.Load(postNameNavigator, settings))
                    {
                        this.Name   = name;
                        wasLoaded   = true;
                    }
                }

                if (excerptNavigator != null)
                {
                    BlogMLTextConstruct excerpt = new BlogMLTextConstruct();
                    if (excerpt.Load(excerptNavigator, settings))
                    {
                        this.Excerpt    = excerpt;
                        wasLoaded       = true;
                    }
                }

                if (BlogMLPost.FillPostCollections(this, source, manager, settings))
                {
                    wasLoaded   = true;
                }
            }

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="BlogMLPost"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("post", BlogMLUtility.BlogMLNamespace);
            BlogMLUtility.WriteCommonObjectAttributes(this, writer);

            if(this.Url != null)
            {
                writer.WriteAttributeString("post-url", this.Url.ToString());
            }

            if (this.PostType != BlogMLPostType.None)
            {
                writer.WriteAttributeString("type", BlogMLPost.PostTypeAsString(this.PostType));
            }

            writer.WriteAttributeString("hasexcerpt", this.HasExcerpt ? "true" : "false");

            if (!String.IsNullOrEmpty(this.Views))
            {
                writer.WriteAttributeString("views", this.Views);
            }

            BlogMLUtility.WriteCommonObjectElements(this, writer);

            this.Content.WriteTo(writer, "content");

            if(this.Name != null)
            {
                this.Name.WriteTo(writer, "post-name");
            }

            if (this.Excerpt != null)
            {
                this.Excerpt.WriteTo(writer, "excerpt");
            }

            if(this.Categories.Count > 0)
            {
                writer.WriteStartElement("categories", BlogMLUtility.BlogMLNamespace);
                foreach(string category in this.Categories)
                {
                    writer.WriteStartElement("category", BlogMLUtility.BlogMLNamespace);
                    writer.WriteAttributeString("ref", category);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            if (this.Comments.Count > 0)
            {
                writer.WriteStartElement("comments", BlogMLUtility.BlogMLNamespace);
                foreach (BlogMLComment comment in this.Comments)
                {
                    comment.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            if (this.Trackbacks.Count > 0)
            {
                writer.WriteStartElement("trackbacks", BlogMLUtility.BlogMLNamespace);
                foreach (BlogMLTrackback trackback in this.Trackbacks)
                {
                    trackback.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            if (this.Attachments.Count > 0)
            {
                writer.WriteStartElement("attachments", BlogMLUtility.BlogMLNamespace);
                foreach (BlogMLAttachment attachment in this.Attachments)
                {
                    attachment.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            if (this.Authors.Count > 0)
            {
                writer.WriteStartElement("authors", BlogMLUtility.BlogMLNamespace);
                foreach (string author in this.Authors)
                {
                    writer.WriteStartElement("author", BlogMLUtility.BlogMLNamespace);
                    writer.WriteAttributeString("ref", author);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            //------------------------------------------------------------
            //	Write the syndication extensions of the current instance
            //------------------------------------------------------------
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Modifies the <see cref="BlogMLPost"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
        /// </summary>
        /// <param name="post">The <see cref="BlogMLPost"/> to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private static bool FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(post, "post");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            XPathNodeIterator categoriesIterator    = source.Select("blog:categories/blog:category", manager);
            XPathNodeIterator commentsIterator      = source.Select("blog:comments/blog:comment", manager);
            XPathNodeIterator trackbacksIterator    = source.Select("blog:trackbacks/blog:trackback", manager);
            XPathNodeIterator attachmentsIterator   = source.Select("blog:attachments/blog:attachment", manager);
            XPathNodeIterator authorsIterator       = source.Select("blog:authors/blog:author", manager);

            if (categoriesIterator != null && categoriesIterator.Count > 0)
            {
                while (categoriesIterator.MoveNext())
                {
                    string referenceId  = categoriesIterator.Current.GetAttribute("ref", String.Empty);
                    if (!String.IsNullOrEmpty(referenceId))
                    {
                        post.Categories.Add(referenceId);
                        wasLoaded       = true;
                    }
                }
            }

            if (commentsIterator != null && commentsIterator.Count > 0)
            {
                while (commentsIterator.MoveNext())
                {
                    BlogMLComment comment   = new BlogMLComment();
                    if (comment.Load(commentsIterator.Current))
                    {
                        post.Comments.Add(comment);
                        wasLoaded           = true;
                    }
                }
            }

            if (trackbacksIterator != null && trackbacksIterator.Count > 0)
            {
                while (trackbacksIterator.MoveNext())
                {
                    BlogMLTrackback trackback   = new BlogMLTrackback();
                    if (trackback.Load(trackbacksIterator.Current))
                    {
                        post.Trackbacks.Add(trackback);
                        wasLoaded               = true;
                    }
                }
            }

            if (attachmentsIterator != null && attachmentsIterator.Count > 0)
            {
                while (attachmentsIterator.MoveNext())
                {
                    BlogMLAttachment attachment = new BlogMLAttachment();
                    if (attachment.Load(attachmentsIterator.Current))
                    {
                        post.Attachments.Add(attachment);
                        wasLoaded               = true;
                    }
                }
            }

            if (authorsIterator != null && authorsIterator.Count > 0)
            {
                while (authorsIterator.MoveNext())
                {
                    string referenceId  = authorsIterator.Current.GetAttribute("ref", String.Empty);
                    if (!String.IsNullOrEmpty(referenceId))
                    {
                        post.Authors.Add(referenceId);
                        wasLoaded       = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Modifies the <see cref="BlogMLPost"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
        /// </summary>
        /// <param name="post">The <see cref="BlogMLPost"/> to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static bool FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(post, "post");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            XPathNodeIterator categoriesIterator    = source.Select("blog:categories/blog:category", manager);
            XPathNodeIterator commentsIterator      = source.Select("blog:comments/blog:comment", manager);
            XPathNodeIterator trackbacksIterator    = source.Select("blog:trackbacks/blog:trackback", manager);
            XPathNodeIterator attachmentsIterator   = source.Select("blog:attachments/blog:attachment", manager);
            XPathNodeIterator authorsIterator       = source.Select("blog:authors/blog:author", manager);

            if (categoriesIterator != null && categoriesIterator.Count > 0)
            {
                while (categoriesIterator.MoveNext())
                {
                    string referenceId  = categoriesIterator.Current.GetAttribute("ref", String.Empty);
                    if (!String.IsNullOrEmpty(referenceId))
                    {
                        post.Categories.Add(referenceId);
                        wasLoaded       = true;
                    }
                }
            }

            if (commentsIterator != null && commentsIterator.Count > 0)
            {
                while (commentsIterator.MoveNext())
                {
                    BlogMLComment comment   = new BlogMLComment();
                    if (comment.Load(commentsIterator.Current, settings))
                    {
                        post.Comments.Add(comment);
                        wasLoaded           = true;
                    }
                }
            }

            if (trackbacksIterator != null && trackbacksIterator.Count > 0)
            {
                while (trackbacksIterator.MoveNext())
                {
                    BlogMLTrackback trackback   = new BlogMLTrackback();
                    if (trackback.Load(trackbacksIterator.Current, settings))
                    {
                        post.Trackbacks.Add(trackback);
                        wasLoaded               = true;
                    }
                }
            }

            if (attachmentsIterator != null && attachmentsIterator.Count > 0)
            {
                while (attachmentsIterator.MoveNext())
                {
                    BlogMLAttachment attachment = new BlogMLAttachment();
                    if (attachment.Load(attachmentsIterator.Current, settings))
                    {
                        post.Attachments.Add(attachment);
                        wasLoaded               = true;
                    }
                }
            }

            if (authorsIterator != null && authorsIterator.Count > 0)
            {
                while (authorsIterator.MoveNext())
                {
                    string referenceId  = authorsIterator.Current.GetAttribute("ref", String.Empty);
                    if (!String.IsNullOrEmpty(referenceId))
                    {
                        post.Authors.Add(referenceId);
                        wasLoaded       = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="BlogMLPost"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="BlogMLPost"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Fragment;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.WriteTo(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            BlogMLPost value  = obj as BlogMLPost;

            if (value != null)
            {
                int result  = BlogMLPost.CompareSequence(this.Attachments, value.Attachments);
                result      = result | ComparisonUtility.CompareSequence(this.Authors, value.Authors, StringComparison.OrdinalIgnoreCase);
                result      = result | ComparisonUtility.CompareSequence(this.Categories, value.Categories, StringComparison.OrdinalIgnoreCase);
                result      = result | BlogMLPost.CompareSequence(this.Comments, value.Comments);
                result      = result | this.Content.CompareTo(value.Content);

                if (this.Excerpt != null)
                {
                    result  = result | this.Excerpt.CompareTo(value.Excerpt);
                }
                else if (value.Excerpt != null)
                {
                    result  = result | -1;
                }

                if (this.Name != null)
                {
                    result  = result | this.Name.CompareTo(value.Name);
                }
                else if (value.Name != null)
                {
                    result  = result | -1;
                }

                result      = result | this.PostType.CompareTo(value.PostType);
                result      = result | BlogMLPost.CompareSequence(this.Trackbacks, value.Trackbacks);
                result      = result | Uri.Compare(this.Url, value.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Views, value.Views, StringComparison.OrdinalIgnoreCase);

                result      = result | BlogMLUtility.CompareCommonObjects(this, value);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is BlogMLPost))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(BlogMLPost first, BlogMLPost second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }
        #endregion

        #region != operator
        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(BlogMLPost first, BlogMLPost second)
        {
            return !(first == second);
        }
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(BlogMLPost first, BlogMLPost second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(BlogMLPost first, BlogMLPost second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
        #endregion
    }
}
