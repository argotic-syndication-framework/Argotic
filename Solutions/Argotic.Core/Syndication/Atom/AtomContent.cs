namespace Argotic.Syndication
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

    /// <summary>
    /// Represents information that contains or links to the content of an <see cref="AtomEntry"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Atom Documents <b>must</b> conform to the following <i>processing model</i> rules. Atom Processors <b>must</b> interpret <see cref="AtomContent"/> according to the first applicable rule.
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                      If the value of the <see cref="ContentType"/> property is <b>text</b>, the value of the <see cref="Content"/> property <b>must not</b> contain child elements.
    ///                      Such text is intended to be presented to humans in a readable fashion. Thus, Atom Processors <i>may</i> collapse white space (including line breaks),
    ///                      and display the text using typographic techniques such as justification and proportional fonts.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      If the value of the <see cref="ContentType"/> property is <b>html</b>, the value of the <see cref="Content"/> property <b>must not</b> contain child elements
    ///                      and <i>should</i> be suitable for handling as HTML. The HTML markup <b>must</b> be escaped. The HTML markup <i>should</i> be such that it could validly appear
    ///                      directly within an HTML <b>div</b> element. Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      If the value of the <see cref="ContentType"/> property is <b>xhtml</b>, the the value of the <see cref="Content"/> property <b>must</b> be a single XHTML div element
    ///                      and <i>should</i> be suitable for handling as XHTML. The XHTML div element itself <b>must not</b> be considered part of the content. Atom Processors that display the
    ///                      content <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      If the value is an <a href="http://www.ietf.org/rfc/rfc3023.txt">XML media type</a> or ends with <b>+xml</b> or <b>/xml</b> (case insensitive),
    ///                     the content <i>may</i> include child elements and <i>should</i> be suitable for handling as the indicated media type.
    ///                     If the <see cref="AtomContent.Source"/> is not provided, this would normally mean that the <see cref="AtomContent.Content"/> would contain a
    ///                     single child element that would serve as the root element of the XML document of the indicated type.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      If the value begins with <b>text/</b> (case insensitive), the <see cref="AtomContent.Content"/> <b>must not</b> contain child elements.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     For all other values , the <see cref="AtomContent.Content"/> <b>must</b> be a valid Base64 encoding, as described in
    ///                     <a href="http://www.ietf.org/rfc/rfc3548.txt">RFC 3548: The Base16, Base32, and Base64 Data Encodings</a>, section 3.
    ///                     When decoded, it <i>should</i> be suitable for handling as the indicated media type. In this case, the characters in
    ///                     the Base64 encoding <i>may</i> be preceded and followed in the atom:content element by white space, and lines are
    ///                     separated by a single newline (U+000A) character.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the AtomContent class.">
    ///         <code
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Atom\AtomContentExample.cs"
    ///             region="AtomContent"
    ///         />
    ///     </code>
    /// </example>
    [Serializable]
    public class AtomContent : IAtomCommonObjectAttributes, IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the base URI other than the base URI of the document or external entity.
        /// </summary>
        private Uri commonObjectBaseUri;

        /// <summary>
        /// Private member to hold the natural or formal language in which the content is written.
        /// </summary>
        private CultureInfo commonObjectLanguage;

        /// <summary>
        /// Private member to hold a value indicating the entity encoding of the content.
        /// </summary>
        private string contentMediaType = string.Empty;

        /// <summary>
        /// Private member to hold an IRI that identifies the remote location of the content.
        /// </summary>
        private Uri contentSource;

        /// <summary>
        /// Private member to hold the local content of the entry.
        /// </summary>
        private string contentValue = string.Empty;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomContent"/> class.
        /// </summary>
        public AtomContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomContent"/> class using the supplied textual content.
        /// </summary>
        /// <param name="content">The local content of the entry.</param>
        public AtomContent(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomContent"/> class using the supplied content and entity encoding.
        /// </summary>
        /// <param name="content">The local content of the entry.</param>
        /// <param name="encoding">A value indicating the entity encoding of the content.</param>
        /// <remarks>
        ///     <para>
        ///         The Atom specification defines three initial values for the type of entry content:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      <i>html</i>: The content <b>must not</b> contain child elements and <i>should</i> be suitable for handling as HTML.
        ///                      The HTML markup <b>must</b> be escaped, and <i>should</i> be such that it could validly appear directly within an HTML <b>div</b> element.
        ///                      Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                      <i>text</i>: The content <b>must not</b> contain child elements. Such text is intended to be presented to humans in a readable fashion.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                      <i>xhtml</i>: The content <b>must</b> be a single XHTML div element and <i>should</i> be suitable for handling as XHTML.
        ///                      The XHTML div element itself <b>must not</b> be considered part of the content. Atom Processors that display the content
        ///                      <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public AtomContent(string content, string encoding)
            : this(content)
        {
            this.ContentType = encoding;
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
        /// Gets or sets the local content of this entry.
        /// </summary>
        /// <value>The local content of this entry.</value>
        /// <remarks>
        ///     The <see cref="Content"/> property is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
        /// </remarks>
        public string Content
        {
            get
            {
                return this.contentValue;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.contentValue = string.Empty;
                }
                else
                {
                    this.contentValue = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the entity encoding of this content.
        /// </summary>
        /// <value>A value indicating the entity encoding of this content.</value>
        /// <remarks>
        ///     <para>
        ///         The Atom specification defines three initial values for the type of entry content:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      <i>html</i>: The content <b>must not</b> contain child elements and <i>should</i> be suitable for handling as HTML.
        ///                      The HTML markup <b>must</b> be escaped, and <i>should</i> be such that it could validly appear directly within an HTML <b>div</b> element.
        ///                      Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                      <i>text</i>: The content <b>must not</b> contain child elements. Such text is intended to be presented to humans in a readable fashion.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                      <i>xhtml</i>: The content <b>must</b> be a single XHTML div element and <i>should</i> be suitable for handling as XHTML.
        ///                      The XHTML div element itself <b>must not</b> be considered part of the content. Atom Processors that display the content
        ///                      <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         If the value is an <a href="http://www.ietf.org/rfc/rfc3023.txt">XML media type</a> or ends with <b>+xml</b> or <b>/xml</b> (case insensitive),
        ///         the content <i>may</i> include child elements and <i>should</i> be suitable for handling as the indicated media type.
        ///         If the <see cref="AtomContent.Source"/> is not provided, this would normally mean that the <see cref="AtomContent.Content"/> would contain a
        ///         single child element that would serve as the root element of the XML document of the indicated type.
        ///     </para>
        ///     <para>
        ///         If the content type is not one of those specified above, it <b>must</b> conform to the syntax of a MIME media type, but <b>must not</b> be a composite type.
        ///         See <a href="http://www.ietf.org/rfc/rfc4288.txt">RFC 4288: Media Type Specifications and Registration Procedures</a> for more details.
        ///     </para>
        ///     <para>
        ///         If the value begins with <b>text/</b> (case insensitive), the <see cref="AtomContent.Content"/> <b>must not</b> contain child elements.
        ///     </para>
        ///     <para>
        ///         For all other values , the <see cref="AtomContent.Content"/> <b>must</b> be a valid Base64 encoding, as described in
        ///         <a href="http://www.ietf.org/rfc/rfc3548.txt">RFC 3548: The Base16, Base32, and Base64 Data Encodings</a>, section 3.
        ///         When decoded, it <i>should</i> be suitable for handling as the indicated media type. In this case, the characters in
        ///         the Base64 encoding <i>may</i> be preceded and followed in the atom:content element by white space, and lines are
        ///         separated by a single newline (U+000A) character.
        ///     </para>
        ///     <para>
        ///         If neither the <see cref="AtomContent.ContentType"/> nor the <see cref="AtomContent.Source"/> is provided,
        ///         Atom Processors <b>must</b> behave as though the<see cref="AtomContent.ContentType"/> property has a value of <i>text</i>.
        ///     </para>
        /// </remarks>
        public string ContentType
        {
            get
            {
                return this.contentMediaType;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.contentMediaType = string.Empty;
                }
                else
                {
                    this.contentMediaType = value.Trim();
                }
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
        /// Gets or sets an IRI that identifies the remote location of this content.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that identifies the remote location of this content.</value>
        /// <remarks>
        ///     <para>
        ///         If a <see cref="AtomContent.Source"/> property is specified, the <see cref="AtomContent.Content"/> property <b>must</b> be empty.
        ///         Atom Processors <i>may</i> use the IRI to retrieve the content and <i>may</i> choose to ignore remote content or to present it in a different manner than local content.
        ///     </para>
        ///     <para>
        ///         If a <see cref="AtomContent.Source"/> property is specified, the <see cref="AtomContent.ContentType"/> <i>should</i> be provided and <b>must</b> be a
        ///         <a href="http://www.ietf.org/rfc/rfc4288.txt">MIME media type</a>, rather than <b>text</b>, <b>html</b>, or <b>xhtml</b>. The value is advisory;
        ///         that is to say, when the corresponding URI (mapped from an IRI, if necessary) is dereferenced, if the server providing that content also provides
        ///         a media type, the server-provided media type is authoritative.
        ///     </para>
        ///     <para>See <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
        ///     <para>See <a href="http://msdn2.microsoft.com/en-us/library/system.uri.aspx">System.Uri</a> for enabling support for IRIs within Microsoft .NET framework applications.</para>
        /// </remarks>
        public Uri Source
        {
            get
            {
                return this.contentSource;
            }

            set
            {
                this.contentSource = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(AtomContent first, AtomContent second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator >(AtomContent first, AtomContent second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator !=(AtomContent first, AtomContent second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(AtomContent first, AtomContent second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
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

            AtomContent value = obj as AtomContent;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(
                             this.ContentType,
                             value.ContentType,
                             StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(
                             this.Source,
                             value.Source,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);

                result = result | AtomUtility.CompareCommonObjectAttributes(this, value);

                return result;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "obj is not of type {0}, type was found to be '{1}'.",
                        this.GetType().FullName,
                        obj.GetType().FullName),
                    "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AtomContent))
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
        /// Loads this <see cref="AtomContent"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="AtomContent"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomContent"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);

            if (AtomUtility.FillCommonObjectAttributes(this, source))
            {
                wasLoaded = true;
            }

            if (source.HasAttributes)
            {
                string typeAttribute = source.GetAttribute("type", string.Empty);
                string sourceAttribute = source.GetAttribute("src", string.Empty);

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    this.ContentType = typeAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(sourceAttribute))
                {
                    Uri src;
                    if (Uri.TryCreate(sourceAttribute, UriKind.RelativeOrAbsolute, out src))
                    {
                        this.Source = src;
                        wasLoaded = true;
                    }
                }
            }

            if (string.Compare(this.ContentType, "xhtml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                XPathNavigator xhtmlDivNavigator = source.SelectSingleNode("xhtml:div", manager);
                if (xhtmlDivNavigator != null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
                {
                    this.Content = xhtmlDivNavigator.InnerXml;
                    wasLoaded = true;
                }
            }
            else if (!string.IsNullOrEmpty(source.Value))
            {
                this.Content = source.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomContent"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomContent"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomContent"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="AtomContent"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AtomContent"/>.</returns>
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
        /// Saves the current <see cref="AtomContent"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("content", AtomUtility.AtomNamespace);
            AtomUtility.WriteCommonObjectAttributes(this, writer);

            if (!string.IsNullOrEmpty(this.ContentType))
            {
                writer.WriteAttributeString("type", this.ContentType);
            }

            if (this.Source != null)
            {
                writer.WriteAttributeString("src", this.Source.ToString());
            }

            if (string.Compare(this.ContentType, "xhtml", StringComparison.OrdinalIgnoreCase) == 0
                && string.IsNullOrEmpty(writer.LookupPrefix(AtomUtility.XhtmlNamespace)))
            {
                writer.WriteAttributeString("xmlns", "xhtml", null, AtomUtility.XhtmlNamespace);
            }

            if (!string.IsNullOrEmpty(this.Content))
            {
                if (string.Compare(this.ContentType, "xhtml", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    writer.WriteStartElement("div", AtomUtility.XhtmlNamespace);
                    writer.WriteString(this.Content);
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteString(this.Content);
                }
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}