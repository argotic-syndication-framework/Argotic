namespace Argotic.Publishing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;
    using Argotic.Syndication;

    /// <summary>
    /// Describes the location and capabilities of a discoverable resource that contains a set of member resources.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomMemberResources"/> class implements the <i>app:collection</i> element of the <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a>.
    ///     </para>
    ///     <para>
    ///         The <see cref="AtomMemberResources"/> describes a <see cref="AtomFeed"/>. The <see cref="AtomMemberResources"/> <b>must</b> specify a <see cref="Title"/> and <see cref="AtomMemberResources.Uri"/>.
    ///     </para>
    ///     <para>
    ///         The <see cref="AtomMemberResources"/> <i>may</i> contain any number of <see cref="AtomAcceptedMediaRange">accept</see> entities,
    ///         indicating the types of representations accepted by the <see cref="AtomMemberResources">collection</see>. The order of such elements is <u>not</u> significant.
    ///         Additionally, the <see cref="AtomMemberResources">collection</see> <i>may</i> contain any number of <see cref="AtomCategoryDocument">categories</see>.
    ///     </para>
    ///     <para>
    ///         The <see cref="AtomMemberResources"/> <i>may</i> appear as a child of an <see cref="AtomFeed"/> or <see cref="AtomSource"/> element in an <see cref="AtomFeed"/> document.
    ///         Its content identifies a collection by which new entries can be added to appear in the feed.
    ///     </para>
    /// </remarks>
    /// <seealso cref="ISyndicationExtension"/>
    /// <seealso cref="SyndicationExtension"/>
    [Serializable]
    public class AtomMemberResources : SyndicationExtension,
                                       IComparable,
                                       IExtensibleSyndicationObject,
                                       IAtomCommonObjectAttributes
    {
        /// <summary>
        /// Private member to hold.
        /// </summary>
        private Collection<AtomAcceptedMediaRange> collectionAcceptedMediaRanges;

        /// <summary>
        /// Private member to hold a list of categories that can be applied to members of the collection.
        /// </summary>
        private Collection<AtomCategoryDocument> collectionCategories;

        /// <summary>
        /// Private member to hold an IRI that identifies the location of the collection.
        /// </summary>
        private Uri collectionResourceLocation;

        /// <summary>
        /// Private member to hold a human-readable title for the collection.
        /// </summary>
        private AtomTextConstruct collectionTitle = new AtomTextConstruct();

        /// <summary>
        /// Private member to hold the base URI other than the base URI of the document or external entity.
        /// </summary>
        private Uri commonObjectBaseUri;

        /// <summary>
        /// Private member to hold the natural or formal language in which the content is written.
        /// </summary>
        private CultureInfo commonObjectLanguage;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomMemberResources"/> class.
        /// </summary>
        public AtomMemberResources()
            : base(
                "app",
                "http://www.w3.org/2007/app",
                new Version("1.0"),
                new Uri("http://bitworking.org/projects/atom/rfc5023.html"),
                "Atom Publishing Protocol Collection",
                "Extends syndication resource memebers to provide a means of specifying a collection by which new entries may be added to a feed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomMemberResources"/> class using the supplied <see cref="AtomTextConstruct"/>.
        /// </summary>
        /// <param name="href">A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that identifies the location of the collection.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the collection.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomMemberResources(Uri href, AtomTextConstruct title)
            : this()
        {
            this.Uri = href;
            this.Title = title;
        }

        /// <summary>
        /// Gets a list of media ranges that are accepted by this collection.
        /// </summary>
        /// <value>A <see cref="Collection{AtomAcceptedMediaRange}"/> of <see cref="AtomAcceptedMediaRange"/> objects that represent a list of media ranges that this collection will accept from clients.</value>
        /// <remarks>
        ///     <para>
        ///         A value of <b>application/atom+xml;type=entry</b> <i>may</i> appear in any <see cref="AtomAcceptedMediaRange">accept</see> list of media ranges
        ///         and indicates that <see cref="AtomEntry">Atom Entry Documents</see> can be added to the <see cref="AtomMemberResources"/>.
        ///         If no <see cref="AtomAcceptedMediaRange"/> is present, clients <i>should</i> treat this as equivalent to an <see cref="AtomAcceptedMediaRange"/> with the content <b>application/atom+xml;type=entry</b>.
        ///         The <see cref="AtomAcceptedMediaRange"/> class exposes a static string property named <see cref="AtomAcceptedMediaRange.AtomEntryMediaRange"/> that can be used to assign the
        ///         <b>application/atom+xml;type=entry</b> media range value.
        ///     </para>
        ///     <para>
        ///         If one <see cref="AtomAcceptedMediaRange"/> exists and is empty, clients <i>should</i> assume
        ///         that the <see cref="AtomMemberResources"/> <b>does not</b> support the creation of new <see cref="AtomFeed.Entries"/>.
        ///     </para>
        /// </remarks>
        public Collection<AtomAcceptedMediaRange> Accepts
        {
            get
            {
                if (this.collectionAcceptedMediaRanges == null)
                {
                    this.collectionAcceptedMediaRanges = new Collection<AtomAcceptedMediaRange>();
                }

                return this.collectionAcceptedMediaRanges;
            }
        }

        /// <summary>
        /// Gets or sets the base URI other than the base URI of the document or external entity.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
        ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
        /// </remarks>
        public Uri BaseUri
        {
            get
            {
                return this.commonObjectBaseUri;
            }

            set
            {
                this.commonObjectBaseUri = value;
            }
        }

        /// <summary>
        /// Gets a list of categories that can be applied to members of this collection.
        /// </summary>
        /// <value>A <see cref="Collection{AtomCategoryDocument}"/> of <see cref="AtomCategoryDocument"/> objects that represent a list of categories that can be applied to members of this collection.</value>
        /// <remarks>
        ///     The server <i>may</i> reject attempts to create or store members whose categories are not present in its categories list.
        ///     A <see cref="AtomMemberResources"/> that indicates the category set is open <b>should not</b> reject otherwise acceptable members whose categories are not in its categories list.
        ///     The absence of <see cref="Categories"/> means that the category handling of the <see cref="AtomMemberResources"/> is unspecified.
        ///     A <see cref="AtomCategoryDocument.IsFixed">fixed</see> category list that contains zero categories indicates the <see cref="AtomMemberResources"/> does not accept category data.
        /// </remarks>
        public Collection<AtomCategoryDocument> Categories
        {
            get
            {
                if (this.collectionCategories == null)
                {
                    this.collectionCategories = new Collection<AtomCategoryDocument>();
                }

                return this.collectionCategories;
            }
        }

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
                if (this.objectSyndicationExtensions == null)
                {
                    this.objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }

                return this.objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.objectSyndicationExtensions = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the natural or formal language in which the content is written.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
        ///     </para>
        /// </remarks>
        public CultureInfo Language
        {
            get
            {
                return this.commonObjectLanguage;
            }

            set
            {
                this.commonObjectLanguage = value;
            }
        }

        /// <summary>
        /// Gets or sets information that conveys a human-readable title for this collection.
        /// </summary>
        /// <value>
        ///     A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this collection.
        ///     The default value is an empty <see cref="AtomTextConstruct"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomTextConstruct Title
        {
            get
            {
                return this.collectionTitle;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.collectionTitle = value;
            }
        }

        /// <summary>
        /// Gets or sets an IRI that identifies the location of this <see cref="AtomMemberResources"/>.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that identifies the location of this <see cref="AtomMemberResources"/>.</value>
        /// <remarks>
        ///     <para>See <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
        ///     <para>See <a href="http://msdn2.microsoft.com/en-us/library/system.uri.aspx">System.Uri</a> for enabling support for IRIs within Microsoft .NET framework applications.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Uri
        {
            get
            {
                return this.collectionResourceLocation;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.collectionResourceLocation = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(AtomMemberResources first, AtomMemberResources second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(AtomMemberResources first, AtomMemberResources second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(AtomMemberResources first, AtomMemberResources second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(AtomMemberResources first, AtomMemberResources second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Compares two specified <see cref="Collection{AtomAcceptedMediaRange}"/> collections.
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
        public static int CompareSequence(
            Collection<AtomAcceptedMediaRange> source,
            Collection<AtomAcceptedMediaRange> target)
        {
            int result = 0;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result = result | source[i].CompareTo(target[i]);
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

        /// <summary>
        /// Creates a new <see cref="AtomLink"/> that can be used to retrieve, update, and delete the Resource represented by an editable <see cref="AtomEntry"/> using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="href">A <see cref="Uri"/> that represents the IRI of an editable <see cref="AtomEntry"/>.</param>
        /// <returns>A <see cref="AtomLink"/> object that can be used to retrieve, update, and delete the Resource represented by an editable <see cref="AtomEntry"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <b>edit</b>. The value of <i>edit</i> specifies
        ///         that the value of the <paramref name="href"/> attribute is the IRI of an editable <see cref="AtomEntry"/>.
        ///     </para>
        ///     <para>An <see cref="AtomEntry"/> <b>must not</b> contain more than one <i>edit</i> link relation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        public static AtomLink CreateEditLink(Uri href)
        {
            Guard.ArgumentNotNull(href, "href");

            return new AtomLink(href, "edit");
        }

        /// <summary>
        /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
        /// <returns>A <see cref="AtomLink"/> object that can be can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <b>edit-media</b>. The value of <i>edit-media</i> specifies
        ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
        ///     </para>
        ///     <para>
        ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
        ///         An <see cref="AtomEntry"/> <b>must not</b> contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
        ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
        ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
        ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
        ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
        ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
        ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        public static AtomLink CreateEditMediaLink(Uri href)
        {
            Guard.ArgumentNotNull(href, "href");

            return new AtomLink(href, "edit-media");
        }

        /// <summary>
        /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied parameters.
        /// </summary>
        /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
        /// <param name="contentType">An advisory MIME media type that provides a hint about the type of the representation that is expected to be returned by the Web resource.</param>
        /// <returns>A <see cref="AtomLink"/> object that can be can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <b>edit-media</b>. The value of <i>edit-media</i> specifies
        ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
        ///     </para>
        ///     <para>
        ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
        ///         An <see cref="AtomEntry"/> <b>must not</b> contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
        ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
        ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
        ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
        ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
        ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
        ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        public static AtomLink CreateEditMediaLink(Uri href, string contentType)
        {
            AtomLink link = CreateEditMediaLink(href);
            link.ContentType = contentType;
            return link;
        }

        /// <summary>
        /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied parameters.
        /// </summary>
        /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
        /// <param name="contentType">An advisory MIME media type that provides a hint about the type of the representation that is expected to be returned by the Web resource.</param>
        /// <param name="contentLanguage">A <see cref="CultureInfo"/> that represents the natural or formal language in which this resource content is written.</param>
        /// <returns>A <see cref="AtomLink"/> object that can be can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <b>edit-media</b>. The value of <i>edit-media</i> specifies
        ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
        ///     </para>
        ///     <para>
        ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
        ///         An <see cref="AtomEntry"/> <b>must not</b> contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
        ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
        ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
        ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
        ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
        ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
        ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Globalization",
            "CA1304:SpecifyCultureInfo",
            MessageId = "Argotic.Publishing.AtomMemberResources.CreateEditMediaLink(System.Uri,System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Globalization",
            "CA1304:SpecifyCultureInfo",
            MessageId = "Argotic.Publishing.AtomMemberResources.CreateMemberEntryEditMediaLink(System.Uri,System.String)")]
        public static AtomLink CreateEditMediaLink(Uri href, string contentType, CultureInfo contentLanguage)
        {
            AtomLink link = CreateEditMediaLink(href, contentType);
            link.ContentLanguage = contentLanguage;
            return link;
        }

        /// <summary>
        /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
        /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
        /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool MatchByType(ISyndicationExtension extension)
        {
            Guard.ArgumentNotNull(extension, "extension");

            if (extension.GetType() == typeof(AtomMemberResources))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Decodes the percent-encoded value of the UTF-8 encoding of a character sequence that represents a Slug HTTP entity-header value.
        /// </summary>
        /// <param name="slug">The percent-encoded value of the UTF-8 encoding of a character sequence to be included in a <see cref="Uri"/>.</param>
        /// <returns>Returns a decoded url.</returns>
        /// <remarks>
        ///     <para>
        ///         <i>Slug</i> is an HTTP entity-header whose presence in a POST to a <see cref="AtomMemberResources"/> constitutes a request by the client
        ///         to use the header's value as part of any URIs that would normally be used to retrieve the to-be-created Entry or Media Resources.
        ///     </para>
        ///     <para>
        ///         Servers <i>may</i> use the value of the Slug header when creating the Member URI of the newly created Resource, for instance,
        ///         by using some or all of the words in the value for the last URI segment. Servers <i>may</i> also use the value when creating
        ///         the <see cref="AtomId"/>, or as the <see cref="AtomEntry.Title">title</see> of a Media Link Entry.
        ///     </para>
        ///     <para>
        ///         Servers <i>may</i> choose to ignore the Slug entity-header. Servers <I>may</I> alter the header value before using it.
        ///         For instance, a server might filter out some characters or replace accented letters with non-accented ones, replace spaces with underscores, change case, and so on.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="slug"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="slug"/> is an empty string.</exception>
        public static string SlugDecode(string slug)
        {
            Guard.ArgumentNotNullOrEmptyString(slug, "slug");

            return System.Web.HttpUtility.UrlDecode(slug, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Encodes a sequence of characters that may be safely used as the value of a Slug HTTP entity-header.
        /// </summary>
        /// <param name="characterSequence">
        ///     A sequence of characters that constitutes a request by a client to use the <paramref name="characterSequence"/> as part of any <see cref="Uri"/>
        ///     that would normally be used to retrieve the web resource.
        /// </param>
        /// <returns>The percent-encoded value of the UTF-8 encoding of the <paramref name="characterSequence"/> to be included in a <see cref="Uri"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         <i>Slug</i> is an HTTP entity-header whose presence in a POST to a <see cref="AtomMemberResources"/> constitutes a request by the client
        ///         to use the header's value as part of any URIs that would normally be used to retrieve the to-be-created Entry or Media Resources.
        ///     </para>
        ///     <para>
        ///         Servers <i>may</i> use the value of the Slug header when creating the Member URI of the newly created Resource, for instance,
        ///         by using some or all of the words in the value for the last URI segment. Servers <i>may</i> also use the value when creating
        ///         the <see cref="AtomId"/>, or as the <see cref="AtomEntry.Title">title</see> of a Media Link Entry.
        ///     </para>
        ///     <para>
        ///         Servers <i>may</i> choose to ignore the Slug entity-header. Servers <I>may</I> alter the header value before using it.
        ///         For instance, a server might filter out some characters or replace accented letters with non-accented ones, replace spaces with underscores, change case, and so on.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="characterSequence"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="characterSequence"/> is an empty string.</exception>
        public static string SlugEncode(string characterSequence)
        {
            Guard.ArgumentNotNullOrEmptyString(characterSequence, "characterSequence");

            return System.Web.HttpUtility.UrlEncode(characterSequence, System.Text.Encoding.UTF8).Replace("+", " ");
        }

        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            bool wasAdded = false;

            Guard.ArgumentNotNull(extension, "extension");

            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded = true;

            return wasAdded;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            AtomMemberResources value = obj as AtomMemberResources;

            if (value != null)
            {
                int result = Uri.Compare(
                    this.Uri,
                    value.Uri,
                    UriComponents.AbsoluteUri,
                    UriFormat.SafeUnescaped,
                    StringComparison.OrdinalIgnoreCase);
                result = result | this.Title.CompareTo(value.Title);
                result = result | CompareSequence(this.Accepts, value.Accepts);
                result = result | AtomCategoryDocument.CompareSequence(this.Categories, value.Categories);

                return result;
            }

            throw new ArgumentException(
                string.Format(
                    null,
                    "obj is not of type {0}, type was found to be '{1}'.",
                    this.GetType().FullName,
                    obj.GetType().FullName),
                "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AtomMemberResources))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

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
            Guard.ArgumentNotNull(match, "match");

            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <see cref="IXPathNavigable"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomMemberResources"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(IXPathNavigable source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            XPathNavigator navigator = source.CreateNavigator();

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(navigator.NameTable);

            if (AtomUtility.FillCommonObjectAttributes(this, navigator))
            {
                wasLoaded = true;
            }

            if (navigator.HasAttributes)
            {
                string hrefAttribute = navigator.GetAttribute("href", string.Empty);

                if (!string.IsNullOrEmpty(hrefAttribute))
                {
                    Uri href;
                    if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out href))
                    {
                        this.Uri = href;
                        wasLoaded = true;
                    }
                }
            }

            if (navigator.HasChildren)
            {
                XPathNavigator titleNavigator = navigator.SelectSingleNode("atom:title", manager);
                XPathNodeIterator acceptIterator = navigator.Select("app:accept", manager);
                XPathNodeIterator categoriesIterator = navigator.Select("app:categories", manager);

                if (titleNavigator != null)
                {
                    this.Title = new AtomTextConstruct();
                    if (this.Title.Load(titleNavigator))
                    {
                        wasLoaded = true;
                    }
                }

                if (acceptIterator != null && acceptIterator.Count > 0)
                {
                    while (acceptIterator.MoveNext())
                    {
                        AtomAcceptedMediaRange mediaRange = new AtomAcceptedMediaRange();
                        if (mediaRange.Load(acceptIterator.Current))
                        {
                            this.Accepts.Add(mediaRange);
                            wasLoaded = true;
                        }
                    }
                }

                if (categoriesIterator != null && categoriesIterator.Count > 0)
                {
                    while (categoriesIterator.MoveNext())
                    {
                        AtomCategoryDocument categories = new AtomCategoryDocument();
                        categories.Load(categoriesIterator.Current);
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomMemberResources"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            wasLoaded = this.Load(source);

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="AtomMemberResources"/>.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(XmlReader reader)
        {
            Guard.ArgumentNotNull(reader, "reader");

            return this.Load(reader, null);
        }

        /// <summary>
        /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XmlReader"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="AtomMemberResources"/>.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomMemberResources"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XmlReader reader, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(reader, "reader");

            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            XPathDocument document = new XPathDocument(reader);

            return this.Load(document.CreateNavigator(), settings);
        }

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
            bool wasRemoved = false;

            Guard.ArgumentNotNull(extension, "extension");

            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved = true;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="AtomMemberResources"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AtomMemberResources"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
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

        /// <summary>
        /// Saves the current <see cref="AtomMemberResources"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public override void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("collection", AtomUtility.AtomPublishingNamespace);
            AtomUtility.WriteCommonObjectAttributes(this, writer);

            if (this.Uri != null)
            {
                writer.WriteAttributeString("href", this.Uri.ToString());
            }

            if (this.Title != null)
            {
                this.Title.WriteTo(writer, "title");
            }

            foreach (AtomAcceptedMediaRange mediaRange in this.Accepts)
            {
                mediaRange.WriteTo(writer);
            }

            foreach (AtomCategoryDocument category in this.Categories)
            {
                category.Save(writer);
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}