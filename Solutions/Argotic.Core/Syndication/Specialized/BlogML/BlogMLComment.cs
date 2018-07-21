/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/16/2008	brian.kuhn	Created BlogMLComment Class
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
    /// Represents a post comment.
    /// </summary>
    /// <seealso cref="BlogMLPost.Comments"/>
    [Serializable()]
    public class BlogMLComment : IBlogMLCommonObject, IComparable, IExtensibleSyndicationObject
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
        /// Private member to hold the textual content of the comment.
        /// </summary>
        private BlogMLTextConstruct commentContent                  = new BlogMLTextConstruct();
        /// <summary>
        /// Private member to hold the author's name for the comment.
        /// </summary>
        private string commentUserName                              = String.Empty;
        /// <summary>
        /// Private member to hold the author's email address for the comment.
        /// </summary>
        private string commentUserEmailAddress                      = String.Empty;
        /// <summary>
        /// Private member to hold the author's homepage or web log address for the comment.
        /// </summary>
        private Uri commentUserUrl;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region BlogMLComment()
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogMLComment"/> class.
        /// </summary>
        public BlogMLComment()
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
        #region Content
        /// <summary>
        /// Gets or sets the content of this comment.
        /// </summary>
        /// <value>A <see cref="BlogMLTextConstruct"/> that represents the content of this comment.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public BlogMLTextConstruct Content
        {
            get
            {
                return commentContent;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                commentContent = value;
            }
        }
        #endregion

        #region UserEmailAddress
        /// <summary>
        /// Gets or sets the author's email address for this comment.
        /// </summary>
        /// <value>The author's email address for this comment.</value>
        public string UserEmailAddress
        {
            get
            {
                return commentUserEmailAddress;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    commentUserEmailAddress = String.Empty;
                }
                else
                {
                    commentUserEmailAddress = value.Trim();
                }
            }
        }
        #endregion

        #region UserName
        /// <summary>
        /// Gets or sets the author's name for this comment.
        /// </summary>
        /// <value>The author's name for this comment.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string UserName
        {
            get
            {
                return commentUserName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                commentUserName = value.Trim();
            }
        }
        #endregion

        #region UserUrl
        /// <summary>
        /// Gets or sets the author's homepage or web log for this comment.
        /// </summary>
        /// <value>The author's homepage or web log address for this comment.</value>
        public Uri UserUrl
        {
            get
            {
                return commentUserUrl;
            }

            set
            {
                commentUserUrl = value;
            }
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
        /// Loads this <see cref="BlogMLComment"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="BlogMLComment"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLComment"/>.
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
                string userNameAttribute    = source.GetAttribute("user-name", String.Empty);
                string userEmailAttribute   = source.GetAttribute("user-email", String.Empty);
                string userUrlAttribute     = source.GetAttribute("user-url", String.Empty);

                if (!String.IsNullOrEmpty(userNameAttribute))
                {
                    this.UserName   = userNameAttribute;
                    wasLoaded       = true;
                }

                if (!String.IsNullOrEmpty(userEmailAttribute))
                {
                    this.UserEmailAddress   = userEmailAttribute;
                    wasLoaded               = true;
                }

                if (!String.IsNullOrEmpty(userUrlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.UserUrl    = url;
                        wasLoaded       = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
                if (contentNavigator != null)
                {
                    BlogMLTextConstruct content = new BlogMLTextConstruct();
                    if (content.Load(contentNavigator))
                    {
                        this.Content    = content;
                        wasLoaded       = true;
                    }
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
                string userNameAttribute    = source.GetAttribute("user-name", String.Empty);
                string userEmailAttribute   = source.GetAttribute("user-email", String.Empty);
                string userUrlAttribute     = source.GetAttribute("user-url", String.Empty);

                if (!String.IsNullOrEmpty(userNameAttribute))
                {
                    this.UserName   = userNameAttribute;
                    wasLoaded       = true;
                }

                if (!String.IsNullOrEmpty(userEmailAttribute))
                {
                    this.UserEmailAddress   = userEmailAttribute;
                    wasLoaded               = true;
                }

                if (!String.IsNullOrEmpty(userUrlAttribute))
                {
                    Uri url;
                    if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.UserUrl    = url;
                        wasLoaded       = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
                if (contentNavigator != null)
                {
                    BlogMLTextConstruct content = new BlogMLTextConstruct();
                    if (content.Load(contentNavigator))
                    {
                        this.Content    = content;
                        wasLoaded       = true;
                    }
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
        /// Saves the current <see cref="BlogMLComment"/> to the specified <see cref="XmlWriter"/>.
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
            writer.WriteStartElement("comment", BlogMLUtility.BlogMLNamespace);
            BlogMLUtility.WriteCommonObjectAttributes(this, writer);

            writer.WriteAttributeString("user-name", this.UserName);

            if(!String.IsNullOrEmpty(this.UserEmailAddress))
            {
                writer.WriteAttributeString("user-email", this.UserEmailAddress);
            }

            if (this.UserUrl != null)
            {
                writer.WriteAttributeString("user-url", this.UserUrl.ToString());
            }

            BlogMLUtility.WriteCommonObjectElements(this, writer);
            this.Content.WriteTo(writer, "content");

            //------------------------------------------------------------
            //	Write the syndication extensions of the current instance
            //------------------------------------------------------------
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="BlogMLComment"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="BlogMLComment"/>.</returns>
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
            BlogMLComment value  = obj as BlogMLComment;

            if (value != null)
            {
                int result  = this.Content.CompareTo(value.Content);
                result      = result | String.Compare(this.UserEmailAddress, value.UserEmailAddress, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.UserName, value.UserName, StringComparison.OrdinalIgnoreCase);
                result      = result | Uri.Compare(this.UserUrl, value.UserUrl, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is BlogMLComment))
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
        public static bool operator ==(BlogMLComment first, BlogMLComment second)
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
        public static bool operator !=(BlogMLComment first, BlogMLComment second)
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
        public static bool operator <(BlogMLComment first, BlogMLComment second)
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
        public static bool operator >(BlogMLComment first, BlogMLComment second)
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
