namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Extends syndication specifications to provide a means of describing Slash-based site meta-data.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="SiteSummarySlashSyndicationExtension"/> extends syndicated content to specify meta-data specific to Slash-based sites. This syndication extension conforms to the
    ///         <b>RDF Site Summary 1.0 Modules: Slash</b> 1.0 specification, which can be found at <a href="http://web.resource.org/rss/1.0/modules/slash/">http://web.resource.org/rss/1.0/modules/slash/</a>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the SiteSummarySlashSyndicationExtension class.">
    ///         <code
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Extensions\Core\SiteSummarySlashSyndicationExtensionExample.cs"
    ///             region="SiteSummarySlashSyndicationExtension"
    ///         />
    ///     </code>
    /// </example>
    [Serializable]
    public class SiteSummarySlashSyndicationExtension : SyndicationExtension, IComparable
    {
        /// <summary>
        /// Private member to hold specific information about the extension.
        /// </summary>
        private SiteSummarySlashSyndicationExtensionContext extensionContext = new SiteSummarySlashSyndicationExtensionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSummarySlashSyndicationExtension"/> class.
        /// </summary>
        public SiteSummarySlashSyndicationExtension()
            : base("slash", "http://purl.org/rss/1.0/modules/slash/", new Version("1.0"), new Uri("http://web.resource.org/rss/1.0/modules/slash/"), "RDF Site Summary (Slash)", "Extends syndication feeds to provide a means of describing Slash-based site meta-data.")
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="SiteSummarySlashSyndicationExtensionContext"/> object associated with this extension.
        /// </summary>
        /// <value>A <see cref="SiteSummarySlashSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
        /// <remarks>
        ///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity.
        ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
        ///     are defined for the custom syndication extension.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public SiteSummarySlashSyndicationExtensionContext Context
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
        public static bool operator ==(SiteSummarySlashSyndicationExtension first, SiteSummarySlashSyndicationExtension second)
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
        public static bool operator !=(SiteSummarySlashSyndicationExtension first, SiteSummarySlashSyndicationExtension second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(SiteSummarySlashSyndicationExtension first, SiteSummarySlashSyndicationExtension second)
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
        public static bool operator >(SiteSummarySlashSyndicationExtension first, SiteSummarySlashSyndicationExtension second)
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
            if (extension.GetType() == typeof(SiteSummarySlashSyndicationExtension))
            {
                return true;
            }
            else
            {
                return false;
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

            SiteSummarySlashSyndicationExtension value = obj as SiteSummarySlashSyndicationExtension;

            if (value != null)
            {
                int result = this.Context.Comments.CompareTo(value.Context.Comments);
                result = result | string.Compare(this.Context.Department, value.Context.Department, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Section, value.Context.Section, StringComparison.OrdinalIgnoreCase);
                result = result | ComparisonUtility.CompareSequence(this.Context.HitParade, value.Context.HitParade);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SiteSummarySlashSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummarySlashSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SiteSummarySlashSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummarySlashSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
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
        /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummarySlashSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SiteSummarySlashSyndicationExtension"/>.</returns>
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
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SiteSummarySlashSyndicationExtension))
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
    }
}