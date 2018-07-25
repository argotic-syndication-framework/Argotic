namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a categorization taxonomy that can be applied to a podcast.
    /// </summary>
    /// <seealso cref="ITunesSyndicationExtensionContext"/>
    [Serializable]
    public class ITunesCategory : IComparable
    {
        /// <summary>
        /// Private member to hold the name of the category.
        /// </summary>
        private string categoryText = string.Empty;

        /// <summary>
        /// Private member to hold a collection of sub-categories of the category.
        /// </summary>
        private Collection<ITunesCategory> categorySubcategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesCategory"/> class.
        /// </summary>
        public ITunesCategory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesCategory"/> class using the supplied text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public ITunesCategory(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets the sub-categories of this category.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="ITunesCategory"/> objects that represent the sub-categories of this category. The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<ITunesCategory> Categories
        {
            get
            {
                if (this.categorySubcategories == null)
                {
                    this.categorySubcategories = new Collection<ITunesCategory>();
                }

                return this.categorySubcategories;
            }
        }

        /// <summary>
        /// Gets or sets the name of this category.
        /// </summary>
        /// <value>The name of this category.</value>
        /// <remarks>
        ///     The category text <i>may</i> be entity encoded.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Text
        {
            get
            {
                return this.categoryText;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.categoryText = value.Trim();
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(ITunesCategory first, ITunesCategory second)
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
        public static bool operator !=(ITunesCategory first, ITunesCategory second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(ITunesCategory first, ITunesCategory second)
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
        public static bool operator >(ITunesCategory first, ITunesCategory second)
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
        /// Loads this <see cref="ITunesCategory"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ITunesCategory"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesCategory"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            ITunesSyndicationExtension extension = new ITunesSyndicationExtension();
            XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
            if (source.HasAttributes)
            {
                string textAttribute = source.GetAttribute("text", string.Empty);
                if (!string.IsNullOrEmpty(textAttribute))
                {
                    this.Text = textAttribute;
                    wasLoaded = true;
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator categoryIterator = source.Select("itunes:category", manager);

                if (categoryIterator != null && categoryIterator.Count > 0)
                {
                    while (categoryIterator.MoveNext())
                    {
                        ITunesCategory category = new ITunesCategory();
                        if (category.Load(categoryIterator.Current))
                        {
                            this.Categories.Add(category);
                            wasLoaded = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="ITunesCategory"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            ITunesSyndicationExtension extension = new ITunesSyndicationExtension();
            writer.WriteStartElement("category", extension.XmlNamespace);

            writer.WriteAttributeString("text", this.Text);

            if (this.Categories.Count > 0)
            {
                foreach (ITunesCategory category in this.Categories)
                {
                    category.WriteTo(writer);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="ITunesCategory"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ITunesCategory"/>.</returns>
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

            ITunesCategory value = obj as ITunesCategory;

            if (value != null)
            {
                int result = string.Compare(this.Text, value.Text, StringComparison.OrdinalIgnoreCase);
                result = result | ITunesSyndicationExtension.CompareSequence(this.Categories, value.Categories);

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
            if (!(obj is ITunesCategory))
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