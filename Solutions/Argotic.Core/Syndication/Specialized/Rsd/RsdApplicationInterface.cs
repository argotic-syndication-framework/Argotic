namespace Argotic.Syndication.Specialized
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents a discoverable application programming interface (API) that provides services to web log clients.
    /// </summary>
    /// <seealso cref="RsdDocument.Interfaces"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the RsdApplicationInterface class.">
    ///         <code source="..\..\Argotic.Examples\Core\Rsd\RsdApplicationInterfaceExample.cs" region="RsdApplicationInterface" />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsd")]
    [Serializable]
    public class RsdApplicationInterface : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the communication endpoint of the application interface.
        /// </summary>
        private Uri interfaceLink;

        /// <summary>
        /// Private member to hold the name of the application interface.
        /// </summary>
        private string interfaceName = string.Empty;

        /// <summary>
        /// Private member to hold human readable text that explains the features and settings for the application interface.
        /// </summary>
        private string interfaceNotes = string.Empty;

        /// <summary>
        /// Private member to hold service specific settings for the application interface.
        /// </summary>
        private Dictionary<string, string> interfaceSettings;

        /// <summary>
        /// Private member to hold custom data that is passed to the application interface.
        /// </summary>
        private string interfaceWeblogId = string.Empty;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsdApplicationInterface"/> class.
        /// </summary>
        public RsdApplicationInterface()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsdApplicationInterface"/> class using the supplied parameters.
        /// </summary>
        /// <param name="name">The name of this application interface.</param>
        /// <param name="link">A <see cref="Uri"/> that represents the endpoint of this application interface clients should use to communication with the service.</param>
        /// <param name="isPreferred">A value indicating if this application interface is the preferred service.</param>
        /// <param name="weblogId">A custom web log identifier utilized by this application interface. Can be an empty string.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        public RsdApplicationInterface(string name, Uri link, bool isPreferred, string weblogId)
        {
            this.IsPreferred = isPreferred;
            this.Link = link;
            this.Name = name;
            this.WeblogId = weblogId;
        }

        /// <summary>
        /// Gets or sets the location of the documentation for this application interface.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the location of the documentation for this application interface.</value>
        public Uri Documentation { get; set; }

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
                return this.objectSyndicationExtensions ?? (this.objectSyndicationExtensions = new Collection<ISyndicationExtension>());
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
        /// Gets or sets a value indicating whether gets or sets a value indicating if this application interface is preferred.
        /// </summary>
        /// <value><b>true</b> if this application interface is the preferred service; otherwise <b>false</b>.</value>
        public bool IsPreferred { get; set; }

        /// <summary>
        /// Gets or sets the communication endpoint of this application interface.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the endpoint of this application interface clients should use to communication with the service.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Link
        {
            get
            {
                return this.interfaceLink;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.interfaceLink = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of this application interface.
        /// </summary>
        /// <value>The name of this application interface.</value>
        /// <remarks>
        ///     Well known application interface names include the following:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><b>Antville</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>Blogger</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>Conversant</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>LiveJournal</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>Manila</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>MetaWeblog</b></description>
        ///         </item>
        ///         <item>
        ///             <description><b>MetaWiki</b></description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Name
        {
            get
            {
                return this.interfaceName;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.interfaceName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets human readable text that explains the features and settings for this application interface.
        /// </summary>
        /// <value>Human readable text that explains the features and settings for this application interface.</value>
        public string Notes
        {
            get
            {
                return this.interfaceNotes;
            }

            set
            {
                this.interfaceNotes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets a collection of service specific settings for this application interface.
        /// </summary>
        /// <value>A <see cref="Dictionary{T, T}"/> collection of service specific settings for this application interface.</value>
        public Dictionary<string, string> Settings
        {
            get
            {
                return this.interfaceSettings ?? (this.interfaceSettings = new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Gets or sets a custom web log identifier utilized by this application interface.
        /// </summary>
        /// <value>A custom web log identifier utilized by this application interface.</value>
        public string WeblogId
        {
            get
            {
                return this.interfaceWeblogId;
            }

            set
            {
                this.interfaceWeblogId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RsdApplicationInterface first, RsdApplicationInterface second)
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
        public static bool operator >(RsdApplicationInterface first, RsdApplicationInterface second)
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
        public static bool operator !=(RsdApplicationInterface first, RsdApplicationInterface second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RsdApplicationInterface first, RsdApplicationInterface second)
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

            RsdApplicationInterface value = obj as RsdApplicationInterface;

            if (value != null)
            {
                int result = Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | this.IsPreferred.CompareTo(value.IsPreferred);
                result = result | Uri.Compare(this.Link, value.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Notes, value.Notes, StringComparison.OrdinalIgnoreCase);
                result = result | ComparisonUtility.CompareSequence(this.Settings, value.Settings, StringComparison.Ordinal);
                result = result | string.Compare(this.WeblogId, value.WeblogId, StringComparison.OrdinalIgnoreCase);

                return result;
            }

            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RsdApplicationInterface))
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
        /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            XmlNamespaceManager manager = RsdUtility.CreateNamespaceManager(source.NameTable);

            if (source.HasAttributes)
            {
                string nameAttribute = source.GetAttribute("name", string.Empty);
                string preferredAttribute = source.GetAttribute("preferred", string.Empty);
                string apiLinkAttribute = source.GetAttribute("apiLink", string.Empty);
                string blogIdAttribute = source.GetAttribute("blogID", string.Empty);

                if (!string.IsNullOrEmpty(nameAttribute))
                {
                    this.Name = nameAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(preferredAttribute))
                {
                    bool isPreferred;
                    if (bool.TryParse(preferredAttribute, out isPreferred))
                    {
                        this.IsPreferred = isPreferred;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(apiLinkAttribute))
                {
                    Uri link;
                    if (Uri.TryCreate(apiLinkAttribute, UriKind.RelativeOrAbsolute, out link))
                    {
                        this.Link = link;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(blogIdAttribute))
                {
                    this.WeblogId = blogIdAttribute;
                    wasLoaded = true;
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator settingsNavigator = RsdUtility.SelectSafeSingleNode(source, "rsd:api/rsd:settings", manager);

                if (settingsNavigator != null)
                {
                    XPathNavigator docsNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:docs", manager);
                    XPathNavigator notesNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:notes", manager);
                    XPathNodeIterator settingIterator = RsdUtility.SelectSafe(settingsNavigator, "rsd:setting", manager);

                    if (docsNavigator != null)
                    {
                        if (Uri.TryCreate(docsNavigator.Value, UriKind.RelativeOrAbsolute, out var documentation))
                        {
                            this.Documentation = documentation;
                            wasLoaded = true;
                        }
                    }

                    if (notesNavigator != null)
                    {
                        this.Notes = notesNavigator.Value;
                        wasLoaded = true;
                    }

                    if (settingIterator != null && settingIterator.Count > 0)
                    {
                        while (settingIterator.MoveNext())
                        {
                            string settingName = settingIterator.Current.GetAttribute("name", string.Empty);
                            string settingValue = settingIterator.Current.Value;

                            if (!this.Settings.ContainsKey(settingName))
                            {
                                this.Settings.Add(settingName, settingValue);
                                wasLoaded = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="RsdApplicationInterface"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="RsdApplicationInterface"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Indent = true, OmitXmlDeclaration = true };

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
        /// Saves the current <see cref="RsdApplicationInterface"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("api", RsdUtility.RsdNamespace);
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("preferred", this.IsPreferred ? "true" : "false");
            writer.WriteAttributeString("apiLink", this.Link != null ? this.Link.ToString() : string.Empty);
            writer.WriteAttributeString("blogID", this.WeblogId);

            if (this.Documentation != null || !string.IsNullOrEmpty(this.Notes) || this.Settings.Count > 0)
            {
                writer.WriteStartElement("settings", RsdUtility.RsdNamespace);

                if (this.Documentation != null)
                {
                    writer.WriteElementString("docs", RsdUtility.RsdNamespace, this.Documentation.ToString());
                }

                if (!string.IsNullOrEmpty(this.Notes))
                {
                    writer.WriteElementString("notes", RsdUtility.RsdNamespace, this.Notes);
                }

                foreach (string settingName in this.Settings.Keys)
                {
                    writer.WriteStartElement("setting", RsdUtility.RsdNamespace);
                    writer.WriteAttributeString("name", settingName);
                    writer.WriteString(this.Settings[settingName]);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}