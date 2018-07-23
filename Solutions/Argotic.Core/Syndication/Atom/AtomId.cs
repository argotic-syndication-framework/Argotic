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
    /// Represents a permanent, universally unique identifier for an <see cref="AtomEntry"/> or <see cref="AtomFeed"/>.
    /// </summary>
    /// <seealso cref="AtomEntry.Id"/>
    /// <seealso cref="AtomFeed.Id"/>
    /// <remarks>
    ///     <para>
    ///         When an <i>Atom Document</i> is relocated, migrated, syndicated, republished, exported, or imported, the content of its universally unique identifier <b>must not</b> change.
    ///         Put another way, an <see cref="AtomId"/> pertains to all instantiations of a particular <see cref="AtomEntry"/> or <see cref="AtomFeed"/>; revisions retain the same
    ///         content in their <see cref="AtomId"/> properties. It is suggested that the<see cref="AtomId"/> be stored along with the associated resource.
    ///     </para>
    ///     <para>
    ///         The content of an <see cref="AtomId"/> <b>must</b> be created in a way that assures uniqueness.
    ///         Because of the risk of confusion between IRIs that would be equivalent if they were mapped to URIs and dereferenced,
    ///         the following normalization strategy <i>should</i> be applied when generating unique identifiers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                      Provide the scheme in lowercase characters.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Provide the host, if any, in lowercase characters.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                    Only perform percent-encoding where it is essential.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Use uppercase A through F characters when percent-encoding.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                    Prevent dot-segments from appearing in paths.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     For schemes that define a default authority, use an empty authority if the default is desired.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                    For schemes that define an empty path to be equivalent to a path of "/", use "/".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     For schemes that define a port, use an empty port if the default is desired.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                    Preserve empty fragment identifiers and queries.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                    Ensure that all components of the IRI are appropriately character normalized, e.g., by using NFC or NFKC.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Instances of <see cref="AtomId"/> objects can be compared to determine whether an entry or feed is the same as one seen before.
    ///         Processors <b>must</b> compare <see cref="AtomId"/> objects on a character-by-character basis (in a case-sensitive fashion).
    ///         Comparison operations <b>must</b> be based solely on the IRI character strings and <b>must not</b> rely on dereferencing the IRIs or URIs mapped from them.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the AtomId class.">
    ///         <code
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Atom\AtomIdExample.cs"
    ///             region="AtomId"
    ///         />
    ///     </code>
    /// </example>
    [Serializable]
    public class AtomId : IAtomCommonObjectAttributes, IComparable, IExtensibleSyndicationObject
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
        /// Private member to hold an IRI that represents a permanent, universally unique identifier for the entity.
        /// </summary>
        private Uri idUri;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomId"/> class.
        /// </summary>
        public AtomId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomId"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that represents a permanent, universally unique identifier for this entity.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomId(Uri uri)
        {
            this.Uri = uri;
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
        /// Gets or sets an IRI that represents a permanent, universally unique identifier for this entity.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that represents a permanent, universally unique identifier for this entity.</value>
        /// <remarks>
        ///     <para>This <see cref="Uri"/> <b>must</b> represent an <i>absolute</i> URI.</para>
        ///     <para>See <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
        ///     <para>See <a href="http://msdn2.microsoft.com/en-us/library/system.uri.aspx">System.Uri</a> for enabling support for IRIs within Microsoft .NET framework applications.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Uri
        {
            get
            {
                return this.idUri;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.idUri = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(AtomId first, AtomId second)
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
        public static bool operator >(AtomId first, AtomId second)
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
        public static bool operator !=(AtomId first, AtomId second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(AtomId first, AtomId second)
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

            AtomId value = obj as AtomId;

            if (value != null)
            {
                int result = Uri.Compare(
                    this.Uri,
                    value.Uri,
                    UriComponents.AbsoluteUri,
                    UriFormat.Unescaped,
                    StringComparison.Ordinal);

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
            if (!(obj is AtomId))
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
        /// Loads this <see cref="AtomId"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="AtomId"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomId"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (AtomUtility.FillCommonObjectAttributes(this, source))
            {
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(source.Value))
            {
                Uri uri;
                if (Uri.TryCreate(source.Value, UriKind.Absolute, out uri))
                {
                    this.Uri = uri;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomId"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomId"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomId"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="AtomId"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AtomId"/>.</returns>
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
        /// Saves the current <see cref="AtomId"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("id", AtomUtility.AtomNamespace);
            AtomUtility.WriteCommonObjectAttributes(this, writer);

            writer.WriteString(this.Uri != null ? this.Uri.ToString() : string.Empty);
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}