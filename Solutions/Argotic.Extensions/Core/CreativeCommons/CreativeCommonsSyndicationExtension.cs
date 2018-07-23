namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Extends syndication specifications to provide a means of specifying which Creative Commons licenses are applicable to published content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="CreativeCommonsSyndicationExtension"/> extends syndicated content to specify which <a href="http://creativecommons.org/license/">Creative Commons license</a> is applicable.
    ///         This syndication extension conforms to the <a>creativeCommons RSS Module</a> 1.0 specification, which can be found
    ///         at <a href="http://backend.userland.com/creativeCommonsRssModule">http://backend.userland.com/creativeCommonsRssModule</a>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CreativeCommonsSyndicationExtension class.">
    ///         <code source="..\..\Argotic.Examples\Extensions\Core\CreativeCommonsSyndicationExtensionExample.cs" region="CreativeCommonsSyndicationExtension" />
    ///     </code>
    /// </example>
    [Serializable]
    public class CreativeCommonsSyndicationExtension : SyndicationExtension, IComparable
    {
        /// <summary>
        /// Private member to hold specific information about the extension.
        /// </summary>
        private CreativeCommonsSyndicationExtensionContext extensionContext = new CreativeCommonsSyndicationExtensionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreativeCommonsSyndicationExtension"/> class.
        /// </summary>
        public CreativeCommonsSyndicationExtension()
            : base("creativeCommons", "http://backend.userland.com/creativeCommonsRssModule", new Version("1.0"), new Uri("http://backend.userland.com/creativeCommonsRssModule"), "Creative Commons Licensing", "Extends syndication feeds to provide a means of specifying which Creative Commons licenses are applicable.")
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="CreativeCommonsSyndicationExtensionContext"/> object associated with this extension.
        /// </summary>
        /// <value>A <see cref="CreativeCommonsSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
        /// <remarks>
        ///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity.
        ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
        ///     are defined for the custom syndication extension.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public CreativeCommonsSyndicationExtensionContext Context
        {
            get
            {
                return this.extensionContext;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.extensionContext = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(CreativeCommonsSyndicationExtension first, CreativeCommonsSyndicationExtension second)
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
        public static bool operator !=(CreativeCommonsSyndicationExtension first, CreativeCommonsSyndicationExtension second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(CreativeCommonsSyndicationExtension first, CreativeCommonsSyndicationExtension second)
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
        public static bool operator >(CreativeCommonsSyndicationExtension first, CreativeCommonsSyndicationExtension second)
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
        /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
        /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
        /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool MatchByType(ISyndicationExtension extension)
        {
            Guard.ArgumentNotNull(extension, "extension");

            return extension.GetType() == typeof(CreativeCommonsSyndicationExtension);
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="CreativeCommonsSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="CreativeCommonsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(IXPathNavigable source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            XPathNavigator navigator = source.CreateNavigator();
            wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
            SyndicationExtensionLoadedEventArgs args = new SyndicationExtensionLoadedEventArgs(source, this);
            this.OnExtensionLoaded(args);

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="CreativeCommonsSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="CreativeCommonsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(XmlReader reader)
        {
            Guard.ArgumentNotNull(reader, "reader");

            XPathDocument document = new XPathDocument(reader);

            return this.Load(document.CreateNavigator());
        }

        /// <summary>
        /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public override void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            this.Context.WriteTo(writer, this.XmlNamespace);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="CreativeCommonsSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="CreativeCommonsSyndicationExtension"/>.</returns>
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

            CreativeCommonsSyndicationExtension value = obj as CreativeCommonsSyndicationExtension;

            if (value != null)
            {
                int result = string.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result = result | this.Version.CompareTo(value.Version);
                result = result | string.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
                result = result | string.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);
                result = result | ComparisonUtility.CompareSequence(this.Context.Licenses, value.Context.Licenses, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is CreativeCommonsSyndicationExtension))
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
            return this.ToString().GetHashCode();
        }
    }
}