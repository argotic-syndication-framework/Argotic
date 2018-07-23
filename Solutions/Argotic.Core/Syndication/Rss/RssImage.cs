namespace Argotic.Syndication
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
    /// Represents a graphical logo for an <see cref="RssFeed"/>.
    /// </summary>
    /// <seealso cref="RssChannel.Image"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the RssImage class.">
    ///         <code
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssImageExample.cs"
    ///             region="RssImage"
    ///         />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    [Serializable]
    public class RssImage : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold default height of an image.
        /// </summary>
        private const int DefaultHeight = 31;

        /// <summary>
        /// Private member to hold default width of an image.
        /// </summary>
        private const int DefaultWidth = 88;

        /// <summary>
        /// Private member to hold maximum permissible height of an image.
        /// </summary>
        private const int MaxHeight = 400;

        /// <summary>
        /// Private member to hold maximum permissible width of an image.
        /// </summary>
        private const int MaxWidth = 144;

        /// <summary>
        /// Private member to hold character data that provides a human-readable characterization of the site linked to the image.
        /// </summary>
        private string imageDescription = string.Empty;

        /// <summary>
        /// Private member to hold the height, in pixels, of the image.
        /// </summary>
        private int imageHeight = int.MinValue;

        /// <summary>
        /// Private member to hold the URL of the web site represented by the image.
        /// </summary>
        private Uri imageLink;

        /// <summary>
        /// Private member to hold character data that provides a human-readable description of the image.
        /// </summary>
        private string imageTitle = string.Empty;

        /// <summary>
        /// Private member to hold the URL of the image.
        /// </summary>
        private Uri imageUrl;

        /// <summary>
        /// Private member to hold the width, in pixels, of the image.
        /// </summary>
        private int imageWidth = int.MinValue;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssImage"/> class.
        /// </summary>
        public RssImage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssImage"/> class using the supplied link, title, and url.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URL of the web site represented by this image.</param>
        /// <param name="title">Character data that provides a human-readable description of this image.</param>
        /// <param name="url">A <see cref="Uri"/> that represents the URL of this image.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public RssImage(Uri link, string title, Uri url)
        {
            this.Link = link;
            this.Title = title;
            this.Url = url;
        }

        /// <summary>
        /// Gets the default height that should be assumed for images that do not explicitly define a height.
        /// </summary>
        /// <value>The default height, in pixels, that should be assumed for images that do not explicitly define a height.</value>
        public static int HeightDefault
        {
            get
            {
                return DefaultHeight;
            }
        }

        /// <summary>
        /// Gets the maximum permissible height for an image.
        /// </summary>
        /// <value>The maximum permissible height, in pixels, for an image.</value>
        public static int HeightMaximum
        {
            get
            {
                return MaxHeight;
            }
        }

        /// <summary>
        /// Gets the default width that should be assumed for images that do not explicitly define a width.
        /// </summary>
        /// <value>The default width, in pixels, that should be assumed for images that do not explicitly define a width.</value>
        public static int WidthDefault
        {
            get
            {
                return DefaultWidth;
            }
        }

        /// <summary>
        /// Gets the maximum permissible width for an image.
        /// </summary>
        /// <value>The maximum permissible width, in pixels, for an image.</value>
        public static int WidthMaximum
        {
            get
            {
                return MaxWidth;
            }
        }

        /// <summary>
        /// Gets or sets character data that provides a human-readable characterization of the site linked to this image.
        /// </summary>
        /// <value>Character data that provides a human-readable characterization of the site linked to this image.</value>
        /// <remarks>
        ///     The value of this property <i>should</i> be suitable for use as the <i>title</i> attribute of the <b>a</b> tag in an HTML rendering.
        /// </remarks>
        public string Description
        {
            get
            {
                return this.imageDescription;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.imageDescription = string.Empty;
                }
                else
                {
                    this.imageDescription = value.Trim();
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
        /// Gets or sets the height of this image.
        /// </summary>
        /// <value>The height, in pixels, of this image. The default value is <see cref="int.MinValue"/>, which indicates no height was specified.</value>
        /// <remarks>
        ///     If no height is specified for the image, the image is assumed to be 31 pixels tall.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <i>400</i>.</exception>
        public int Height
        {
            get
            {
                return this.imageHeight;
            }

            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", MaxHeight);
                this.imageHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL of the web site represented by this image.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the web site represented by this image.</value>
        /// <remarks>
        ///     The value of this property <i>should</i> be the same URL as the channel's <see cref="RssChannel.Link">link</see> property.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Link
        {
            get
            {
                return this.imageLink;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.imageLink = value;
            }
        }

        /// <summary>
        /// Gets or sets character data that provides a human-readable description of this image.
        /// </summary>
        /// <value>Character data that provides a human-readable description of this image.</value>
        /// <remarks>
        ///     The value of this property <i>should</i> be the same as the channel's <see cref="RssChannel.Title">title</see> property
        ///     and be suitable for use as the <i>alt</i> attribute of the <b>img</b> tag in an HTML rendering.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Title
        {
            get
            {
                return this.imageTitle;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.imageTitle = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the URL of this image.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of this image.</value>
        /// <remarks>
        ///     The image <b>must</b> be in the <i>GIF</i>, <i>JPEG</i> or <i>PNG</i> formats.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Url
        {
            get
            {
                return this.imageUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.imageUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of this image.
        /// </summary>
        /// <value>The width, in pixels, of this image. The default value is <see cref="int.MinValue"/>, which indicates no width was specified.</value>
        /// <remarks>
        ///     If no width is specified for the image, the image is assumed to be 88 pixels wide.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <i>144</i>.</exception>
        public int Width
        {
            get
            {
                return this.imageWidth;
            }

            set
            {
                Guard.ArgumentNotGreaterThan(value, "value", MaxWidth);
                this.imageWidth = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RssImage first, RssImage second)
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
        public static bool operator >(RssImage first, RssImage second)
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
        public static bool operator !=(RssImage first, RssImage second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RssImage first, RssImage second)
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

            RssImage value = obj as RssImage;

            if (value != null)
            {
                int result = string.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
                result = result | this.Height.CompareTo(value.Height);
                result = result | Uri.Compare(
                             this.Link,
                             value.Link,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(
                             this.Url,
                             value.Url,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);
                result = result | this.Width.CompareTo(value.Width);

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
            if (!(obj is RssImage))
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
        /// Loads this <see cref="RssImage"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RssImage"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssImage"/>.</para>
        ///     <para>If the specified height or width of the image exceeeds the maximum permissble values defined in the specification, the maximum value is used instead of the non-conformant value.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = new XmlNamespaceManager(source.NameTable);
            XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);
            XPathNavigator urlNavigator = source.SelectSingleNode("url", manager);

            XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
            XPathNavigator heightNavigator = source.SelectSingleNode("height", manager);
            XPathNavigator widthNavigator = source.SelectSingleNode("width", manager);

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    this.Link = link;
                    wasLoaded = true;
                }
            }

            if (titleNavigator != null)
            {
                if (!string.IsNullOrEmpty(titleNavigator.Value))
                {
                    this.Title = titleNavigator.Value;
                    wasLoaded = true;
                }
            }

            if (urlNavigator != null)
            {
                Uri url;
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (descriptionNavigator != null)
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }

            if (heightNavigator != null)
            {
                int height;
                if (int.TryParse(
                    heightNavigator.Value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.NumberFormatInfo.InvariantInfo,
                    out height))
                {
                    this.Height = height < HeightMaximum ? height : HeightMaximum;
                    wasLoaded = true;
                }
            }

            if (widthNavigator != null)
            {
                int width;
                if (int.TryParse(
                    widthNavigator.Value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.NumberFormatInfo.InvariantInfo,
                    out width))
                {
                    this.Width = width < WidthMaximum ? width : WidthMaximum;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="RssImage"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssImage"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssImage"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="RssImage"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="RssImage"/>.</returns>
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
        /// Saves the current <see cref="RssImage"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("image");

            writer.WriteElementString("link", this.Link != null ? this.Link.ToString() : string.Empty);
            writer.WriteElementString("title", this.Title);
            writer.WriteElementString("url", this.Url != null ? this.Url.ToString() : string.Empty);

            if (!string.IsNullOrEmpty(this.Description))
            {
                writer.WriteElementString("description", this.Description);
            }

            if (this.Height != int.MinValue)
            {
                writer.WriteElementString(
                    "height",
                    this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Width != int.MinValue)
            {
                writer.WriteElementString(
                    "width",
                    this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}