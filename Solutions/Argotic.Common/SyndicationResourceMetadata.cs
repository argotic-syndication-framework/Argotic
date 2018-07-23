namespace Argotic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Represents metadata associated with a <see cref="ISyndicationResource">syndication resource</see>.
    /// </summary>
    [Serializable]
    public class SyndicationResourceMetadata : IComparable
    {
        /// <summary>
        /// Private member to hold the syndication content format that the syndication resource conforms to.
        /// </summary>
        private SyndicationContentFormat resourceFormat = SyndicationContentFormat.None;

        /// <summary>
        /// Private member to hold the XML namespaces declared in the syndication resource's root element.
        /// </summary>
        private Dictionary<string, string> resourceNamespaces = new Dictionary<string, string>();

        /// <summary>
        /// Private member to hold the version of the syndication specification that the resource conforms to.
        /// </summary>
        private Version resourceVersion;

        /// <summary>
        /// Private member to hold a XPath navigator that can be used to navigate the root element of the syndication resource.
        /// </summary>
        [NonSerialized]
        private XPathNavigator resourceRootNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceMetadata"/> class using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the syndication resource meta-data from.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceMetadata(XPathNavigator navigator)
        {
            Guard.ArgumentNotNull(navigator, "navigator");

            this.Load(navigator);
        }

        /// <summary>
        /// Gets or sets the <see cref="SyndicationContentFormat"/> that the syndication resource conforms to.
        /// </summary>
        /// <value>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value that indicates the syndication specification the resource conforms to.
        ///     If the syndication content format is unable to be determined, returns <see cref="SyndicationContentFormat.None"/>.
        /// </value>
        public SyndicationContentFormat Format
        {
            get
            {
                return this.resourceFormat;
            }

            protected set
            {
                this.resourceFormat = value;
            }
        }

        /// <summary>
        /// Gets a dictionary of the XML namespaces declared in the syndication resource.
        /// </summary>
        /// <value>A dictionary of the resource's XML namespaces, keyed off of the namespace prefix. If no XML namespaces are declared on the root element of the resource, returns an empty dictionary.</value>
        public Dictionary<string, string> Namespaces
        {
            get
            {
                return this.resourceNamespaces;
            }
        }

        /// <summary>
        /// Gets a read-only <see cref="XPathNavigator"/> object that can be used to navigate the root element of the syndication resource.
        /// </summary>
        /// <value>A read-only <see cref="XPathNavigator"/> object that can be used to navigate the root element of the syndication resource.</value>
        public XPathNavigator Resource
        {
            get
            {
                return this.resourceRootNode;
            }
        }

        /// <summary>
        /// Gets the <see cref="Version"/> of the syndication specification that the resource conforms to.
        /// </summary>
        /// <value>The version number of the syndication specification that the resource conforms to. If format version is unable to be determined, returns <b>null</b>.</value>
        public Version Version
        {
            get
            {
                return this.resourceVersion;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(SyndicationResourceMetadata first, SyndicationResourceMetadata second)
        {
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

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(SyndicationResourceMetadata first, SyndicationResourceMetadata second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(SyndicationResourceMetadata first, SyndicationResourceMetadata second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(SyndicationResourceMetadata first, SyndicationResourceMetadata second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceMetadata"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationResourceMetadata"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
        /// </remarks>
        public override string ToString()
        {
            string format = this.Format.ToString();
            string version = this.Version != null ? this.Version.ToString() : string.Empty;
            string namespaces = this.Namespaces != null ? this.Namespaces.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string resource = this.Resource != null ? this.Resource.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;

            return string.Format(null, "[SyndicationResourceMetadata(Format = \"{0}\", Version = \"{1}\", Namespaces = \"{2}\", Resource = \"{3}\")]", format, version, namespaces, resource);
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

            SyndicationResourceMetadata value = obj as SyndicationResourceMetadata;

            if (value != null)
            {
                int result = this.Format.CompareTo(value.Format);

                if (this.Version != null)
                {
                    result = result | this.Version.CompareTo(value.Version);
                }
                else if (value.Version != null)
                {
                    result = result | -1;
                }

                if (this.Namespaces != null && value.Namespaces != null)
                {
                    result = result | ComparisonUtility.CompareSequence(this.Namespaces, value.Namespaces, StringComparison.Ordinal);
                }
                else if (this.Namespaces != null && value.Namespaces == null)
                {
                    result = result | 1;
                }
                else if (this.Namespaces == null && value.Namespaces != null)
                {
                    result = result | -1;
                }

                if (this.Resource != null && value.Resource != null)
                {
                    result = result | string.Compare(this.Resource.OuterXml, value.Resource.OuterXml, StringComparison.OrdinalIgnoreCase);
                }
                else if (this.Resource != null && value.Resource == null)
                {
                    result = result | 1;
                }
                else if (this.Resource == null && value.Resource != null)
                {
                    result = result | -1;
                }

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SyndicationResourceMetadata))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
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
        /// Returns a <see cref="Version"/> object for the value of the XML attribute in <paramref name="navigator"/> with a local name specified by <paramref name="name"/>.
        /// </summary>
        /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the XML attribute value from.</param>
        /// <param name="name">The name of the attribute to parse in the <paramref name="navigator"/>.</param>
        /// <returns>The <see cref="Version"/> represented by the value of the specified XML attribute. If unable to determine version, returns <b>null</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        protected static Version GetVersionFromAttribute(XPathNavigator navigator, string name)
        {
            Version version = null;

            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            string value = navigator.GetAttribute(name, string.Empty);

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    version = new Version(value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
                catch (ArgumentException)
                {
                    return null;
                }
                catch (FormatException)
                {
                    return null;
                }
                catch (OverflowException)
                {
                    return null;
                }
            }

            return version;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Attention Profiling Markup Language (APML) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Attention Profiling Markup Language (APML) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "<Pending>")]
        protected static bool TryParseApmlResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("apml", "http://www.apml.org/apml-0.6");

            version = null;
            if ((navigator = resource.SelectSingleNode("APML", manager)) != null || (navigator = resource.SelectSingleNode("apml:APML", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.apml.org/apml-0.6"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(0, 6);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseAtomResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            manager.AddNamespace("atom03", "http://purl.org/atom/ns#");

            version = null;
            if ((navigator = resource.SelectSingleNode("feed", manager)) != null || (navigator = resource.SelectSingleNode("atom:feed", manager)) != null || (navigator = resource.SelectSingleNode("atom03:feed", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.w3.org/2005/Atom"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 0);
                    }
                }
                else if (namespaces.ContainsValue("http://purl.org/atom/ns#"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(0, 3);
                    }
                }
            }
            else if ((navigator = resource.SelectSingleNode("entry", manager)) != null || (navigator = resource.SelectSingleNode("atom:entry", manager)) != null || (navigator = resource.SelectSingleNode("atom03:entry", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.w3.org/2005/Atom"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 0);
                    }
                }
                else if (namespaces.ContainsValue("http://purl.org/atom/ns#"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(0, 3);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom Publishing Protocol category document formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseAtomPublishingCategoriesResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            manager.AddNamespace("atom03", "http://purl.org/atom/ns#");
            manager.AddNamespace("app", "http://www.w3.org/2007/app");

            version = null;
            if ((navigator = resource.SelectSingleNode("categories", manager)) != null || (navigator = resource.SelectSingleNode("app:categories", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.w3.org/2007/app"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 0);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom Publishing Protocol service document formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseAtomPublishingServiceResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            manager.AddNamespace("atom03", "http://purl.org/atom/ns#");
            manager.AddNamespace("app", "http://www.w3.org/2007/app");

            version = null;
            if ((navigator = resource.SelectSingleNode("service", manager)) != null || (navigator = resource.SelectSingleNode("app:service", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.w3.org/2007/app"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 0);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a  Web Log Markup Language (BlogML) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a  Web Log Markup Language (BlogML) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseBlogMlResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("blogML", "http://www.blogml.com/2006/09/BlogML");

            version = null;
            if ((navigator = resource.SelectSingleNode("blog", manager)) != null || (navigator = resource.SelectSingleNode("blogML:blog", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.blogml.com/2006/09/BlogML"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(2, 0);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Microsummary Generator formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Microsummary Generator formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseMicroSummaryGeneratorResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("micro", "http://www.mozilla.org/microsummaries/0.1");

            version = null;
            if ((navigator = resource.SelectSingleNode("generator", manager)) != null || (navigator = resource.SelectSingleNode("micro:generator", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://www.mozilla.org/microsummaries/0.1"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(0, 1);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a News Markup Language (NewsML) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a News Markup Language (NewsML) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseNewsMlResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;

            Guard.ArgumentNotNull(resource, "resource");

            version = null;
            if ((navigator = resource.SelectSingleNode("NewsML")) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");

                resourceConformsToFormat = true;
                if (version == null)
                {
                    version = new Version(2, 0);
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a OpenSearch Description formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a OpenSearch Description formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseOpenSearchDescriptionResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("search", "http://a9.com/-/spec/opensearch/1.1/");

            version = null;
            if ((navigator = resource.SelectSingleNode("OpenSearchDescription", manager)) != null || (navigator = resource.SelectSingleNode("search:OpenSearchDescription", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://a9.com/-/spec/opensearch/1.1/"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 1);
                    }
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a  Outline Processor Markup Language (OPML) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a  Outline Processor Markup Language (OPML) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseOpmlResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;

            Guard.ArgumentNotNull(resource, "resource");

            version = null;
            if ((navigator = resource.SelectSingleNode("opml")) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");

                resourceConformsToFormat = true;
                if (version == null)
                {
                    version = new Version(2, 0);
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Really Simple Discovery (RSD) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Really Simple Discovery (RSD) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsd")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseRsdResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("rsd", "http://archipelago.phrasewise.com/rsd");

            version = null;
            if ((navigator = resource.SelectSingleNode("rsd", manager)) != null || (navigator = resource.SelectSingleNode("rsd:rsd", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://archipelago.phrasewise.com/rsd"))
                {
                    resourceConformsToFormat = true;
                    if (version == null)
                    {
                        version = new Version(1, 0);
                    }
                }
                else if (string.Compare(navigator.Name, "rsd", StringComparison.OrdinalIgnoreCase) == 0 && version != null)
                {
                    // Most web log software actually fails to provide the default XML namespace per RSD spec, so this is a hack/compromise
                    resourceConformsToFormat = true;
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Determines if the specified <see cref="XPathNavigator"/> represents a Really Simple Syndication (RSS) formatted syndication resource.
        /// </summary>
        /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
        /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if <paramref name="resource"/> represents a Really Simple Syndication (RSS) formatted syndication resource; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected static bool TryParseRssResource(XPathNavigator resource, out XPathNavigator navigator, out Version version)
        {
            bool resourceConformsToFormat = false;
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(resource, "resource");

            manager = new XmlNamespaceManager(resource.NameTable);
            manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            manager.AddNamespace("rss09", "http://my.netscape.com/rdf/simple/0.9/");
            manager.AddNamespace("rss10", "http://purl.org/rss/1.0/");

            version = null;
            if ((navigator = resource.SelectSingleNode("rss", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                resourceConformsToFormat = true;
                if (version == null)
                {
                    version = new Version(2, 0);
                }
            }
            else if ((navigator = resource.SelectSingleNode("rdf:RDF", manager)) != null)
            {
                version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
                Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                if (namespaces.ContainsValue("http://purl.org/rss/1.0/"))
                {
                    resourceConformsToFormat = true;
                    version = new Version(1, 0);
                }
                else if (namespaces.ContainsValue("http://my.netscape.com/rdf/simple/0.9/"))
                {
                    resourceConformsToFormat = true;
                    version = new Version(0, 9);
                }
            }

            return resourceConformsToFormat;
        }

        /// <summary>
        /// Extracts the content format, version, and XML namespaces for a syndication resource from the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="resource">The <see cref="XPathNavigator"/> to extract the syndication resource meta-data from.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        private void Load(XPathNavigator resource)
        {
            XPathNavigator navigator = null;
            Version version = null;

            Guard.ArgumentNotNull(resource, "resource");

            Dictionary<string, string> namespaces = (Dictionary<string, string>)resource.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            foreach (string prefix in namespaces.Keys)
            {
                this.resourceNamespaces.Add(prefix, namespaces[prefix]);
            }

            this.resourceVersion = SyndicationResourceMetadata.GetVersionFromAttribute(resource, "version");

            if (SyndicationResourceMetadata.TryParseApmlResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.Apml;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseAtomResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.Atom;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseAtomPublishingCategoriesResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.AtomCategoryDocument;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseAtomPublishingServiceResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.AtomServiceDocument;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseBlogMlResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.BlogML;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseMicroSummaryGeneratorResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.MicroSummaryGenerator;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseNewsMlResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.NewsML;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseOpenSearchDescriptionResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.OpenSearchDescription;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseOpmlResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.Opml;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseRsdResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.Rsd;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else if (SyndicationResourceMetadata.TryParseRssResource(resource, out navigator, out version))
            {
                this.resourceFormat = SyndicationContentFormat.Rss;
                this.resourceRootNode = navigator;
                this.resourceVersion = version;
            }
            else
            {
                this.resourceFormat = SyndicationContentFormat.None;
                this.resourceRootNode = null;
                this.resourceVersion = null;
            }
        }
    }
}