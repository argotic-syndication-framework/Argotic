namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Extends syndication specifications to provide a means of supplementing the enclosure capabilities of feeds.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="YahooMediaSyndicationExtension"/> extends syndicated content to extends <i>enclosures</i> to handle other media types,
    ///         such as short films or TV, as well as provide additional metadata with the media. This extension enables content publishers and bloggers
    ///         to syndicate multimedia content such as TV and video clips, movies, images, and audio.. This syndication extension conforms to the
    ///         <b>Media RSS Module</b> 1.1.1 specification, which can be found at <a href="http://search.yahoo.com/mrss">http://search.yahoo.com/mrss</a>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the YahooMediaSyndicationExtension class.">
    ///         <code source="..\..\Argotic.Examples\Extensions\Core\YahooMediaSyndicationExtensionExample.cs" region="YahooMediaSyndicationExtension" />
    ///     </code>
    /// </example>
    [Serializable]
    public class YahooMediaSyndicationExtension : SyndicationExtension, IComparable
    {
        /// <summary>
        /// Private member to hold specific information about the extension.
        /// </summary>
        private YahooMediaSyndicationExtensionContext extensionContext = new YahooMediaSyndicationExtensionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtension"/> class.
        /// </summary>
        public YahooMediaSyndicationExtension()
            : base("media", "http://search.yahoo.com/mrss/", new Version("1.1.1"), new Uri("http://search.yahoo.com/mrss"), "Yahoo! Media", "Extends syndication feeds to provide a means of supplementing the enclosure capabilities of feeds.")
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="YahooMediaSyndicationExtensionContext"/> object associated with this extension.
        /// </summary>
        /// <value>A <see cref="YahooMediaSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
        /// <remarks>
        ///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity.
        ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
        ///     are defined for the custom syndication extension.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public YahooMediaSyndicationExtensionContext Context
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
        public static bool operator ==(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
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
        public static bool operator !=(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
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
        public static bool operator >(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
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
        /// Returns the content expression identifier for the supplied <see cref="YahooMediaExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="YahooMediaExpression"/> to get the content expression identifier for.</param>
        /// <returns>The content expression identifier for the supplied <paramref name="expression"/>, otherwise returns an empty string.</returns>
        public static string ExpressionAsString(YahooMediaExpression expression)
        {
            string name = string.Empty;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaExpression).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaExpression))
                {
                    YahooMediaExpression mediaExpression = (YahooMediaExpression)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (mediaExpression == expression)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Returns the <see cref="YahooMediaExpression"/> enumeration value that corresponds to the specified content expression name.
        /// </summary>
        /// <param name="name">The name of the content expression.</param>
        /// <returns>A <see cref="YahooMediaExpression"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaExpression.None</b>.</returns>
        /// <remarks>This method disregards case of specified content expression name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaExpression ExpressionByName(string name)
        {
            YahooMediaExpression mediaExpression = YahooMediaExpression.None;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaExpression).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaExpression))
                {
                    YahooMediaExpression expression = (YahooMediaExpression)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            mediaExpression = expression;
                            break;
                        }
                    }
                }
            }

            return mediaExpression;
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

            return extension.GetType() == typeof(YahooMediaSyndicationExtension);
        }

        /// <summary>
        /// Returns the content medium identifier for the supplied <see cref="YahooMediaMedium"/>.
        /// </summary>
        /// <param name="medium">The <see cref="YahooMediaMedium"/> to get the content medium identifier for.</param>
        /// <returns>The content medium identifier for the supplied <paramref name="medium"/>, otherwise returns an empty string.</returns>
        public static string MediumAsString(YahooMediaMedium medium)
        {
            string name = string.Empty;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaMedium).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaMedium))
                {
                    YahooMediaMedium mediaMedium = (YahooMediaMedium)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (mediaMedium == medium)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Returns the <see cref="YahooMediaMedium"/> enumeration value that corresponds to the specified content medium name.
        /// </summary>
        /// <param name="name">The name of the content medium.</param>
        /// <returns>A <see cref="YahooMediaMedium"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaMedium.None</b>.</returns>
        /// <remarks>This method disregards case of specified content medium name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaMedium MediumByName(string name)
        {
            YahooMediaMedium mediaMedium = YahooMediaMedium.None;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaMedium).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaMedium))
                {
                    YahooMediaMedium medium = (YahooMediaMedium)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            mediaMedium = medium;
                            break;
                        }
                    }
                }
            }

            return mediaMedium;
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
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
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.</returns>
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

            YahooMediaSyndicationExtension value = obj as YahooMediaSyndicationExtension;

            if (value != null)
            {
                int result = YahooMediaUtility.CompareSequence((Collection<YahooMediaContent>)this.Context.Contents, (Collection<YahooMediaContent>)value.Context.Contents);
                result = result | YahooMediaUtility.CompareSequence((Collection<YahooMediaGroup>)this.Context.Groups, (Collection<YahooMediaGroup>)value.Context.Groups);
                result = result | YahooMediaUtility.CompareCommonObjectEntities(this.Context, value.Context);

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
            if (!(obj is YahooMediaSyndicationExtension))
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