namespace Argotic.Examples
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;
    using Argotic.Extensions;

#pragma warning disable SA1649 // File name should match first type name
                              /// <summary>
                              /// Provides a simple example of a custom syndication extension.
                              /// </summary>
    public class MyCustomSyndicationExtension : SyndicationExtension, IComparable
#pragma warning restore SA1649 // File name should match first type name
    {
        /// <summary>
        /// Private member to hold the value of the custom syndication extension's attribute.
        /// </summary>
        private string customExtensionAttribute = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyCustomSyndicationExtension"/> class.
        /// </summary>
        public MyCustomSyndicationExtension()
            : base("myPrefix", "http://www.example.com/2008/03/custom", new Version("1.0"), new Uri("http://www.example.com/spec"), "My Extension", "Example of a custom syndication extension.")
        {
            // Class state initialized by abstract SyndicationExtension base class
        }

        /// <summary>
        /// Gets or sets the value of the extension attribute.
        /// </summary>
        /// <value>The value of the extension attribute.</value>
        public string MyAttribute
        {
            get
            {
                return this.customExtensionAttribute;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.customExtensionAttribute = string.Empty;
                }
                else
                {
                    this.customExtensionAttribute = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(MyCustomSyndicationExtension first, MyCustomSyndicationExtension second)
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
        public static bool operator !=(MyCustomSyndicationExtension first, MyCustomSyndicationExtension second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(MyCustomSyndicationExtension first, MyCustomSyndicationExtension second)
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
        public static bool operator >(MyCustomSyndicationExtension first, MyCustomSyndicationExtension second)
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
        /// represents the same <see cref="Type"/> as this <see cref="MyCustomSyndicationExtension"/>.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
        /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="MyCustomSyndicationExtension"/>; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool MatchByType(ISyndicationExtension extension)
        {
            Guard.ArgumentNotNull(extension, "extension");

            if (extension.GetType() == typeof(MyCustomSyndicationExtension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="MyCustomSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="MyCustomSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(IXPathNavigable source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XPathNavigator navigator = source.CreateNavigator();
            if (navigator.HasAttributes)
            {
                string myAttribute = navigator.GetAttribute("someAttribute", string.Empty);
                if (!string.IsNullOrEmpty(myAttribute))
                {
                    this.MyAttribute = myAttribute;
                    wasLoaded = true;
                }
            }

            SyndicationExtensionLoadedEventArgs args = new SyndicationExtensionLoadedEventArgs(source, this);
            this.OnExtensionLoaded(args);

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="MyCustomSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="MyCustomSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
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

            writer.WriteStartElement("CustomExtension", this.XmlNamespace);

            if (!string.IsNullOrEmpty(this.MyAttribute))
            {
                writer.WriteAttributeString("someAttribute", this.MyAttribute);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="MyCustomSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="MyCustomSyndicationExtension"/>.</returns>
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

            MyCustomSyndicationExtension value = obj as MyCustomSyndicationExtension;

            if (value != null)
            {
                // Base class properties
                int result = string.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result = result | this.Version.CompareTo(value.Version);
                result = result | string.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
                result = result | string.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);

                // Custom extension properties
                result = result | string.Compare(this.MyAttribute, value.MyAttribute, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is MyCustomSyndicationExtension))
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