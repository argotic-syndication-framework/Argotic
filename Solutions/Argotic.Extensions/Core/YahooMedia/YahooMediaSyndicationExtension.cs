/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created YahooMediaSyndicationExtension Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
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
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Extensions\Core\YahooMediaSyndicationExtensionExample.cs" 
    ///             region="YahooMediaSyndicationExtension"
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    public class YahooMediaSyndicationExtension : SyndicationExtension, IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold specific information about the extension.
        /// </summary>
        private YahooMediaSyndicationExtensionContext extensionContext = new YahooMediaSyndicationExtensionContext();
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaSyndicationExtension()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtension"/> class.
        /// </summary>
        public YahooMediaSyndicationExtension()
            : base("media", "http://search.yahoo.com/mrss/", new Version("1.1.1"), new Uri("http://search.yahoo.com/mrss"), "Yahoo! Media", "Extends syndication feeds to provide a means of supplementing the enclosure capabilities of feeds.")
        {
            //------------------------------------------------------------
            //	Initialization handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Context
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
                return extensionContext;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionContext = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region ExpressionAsString(YahooMediaExpression expression)
        /// <summary>
        /// Returns the content expression identifier for the supplied <see cref="YahooMediaExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="YahooMediaExpression"/> to get the content expression identifier for.</param>
        /// <returns>The content expression identifier for the supplied <paramref name="expression"/>, otherwise returns an empty string.</returns>
        public static string ExpressionAsString(YahooMediaExpression expression)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaExpression).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaExpression))
                {
                    YahooMediaExpression mediaExpression    = (YahooMediaExpression)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (mediaExpression == expression)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region ExpressionByName(string name)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            YahooMediaExpression mediaExpression    = YahooMediaExpression.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content expression based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaExpression).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaExpression))
                {
                    YahooMediaExpression expression = (YahooMediaExpression)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes       = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            mediaExpression = expression;
                            break;
                        }
                    }
                }
            }

            return mediaExpression;
        }
        #endregion

        #region MatchByType(ISyndicationExtension extension)
        /// <summary>
        /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
        /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
        /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool MatchByType(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Determine if search condition was met 
            //------------------------------------------------------------
            if (extension.GetType() == typeof(YahooMediaSyndicationExtension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region MediumAsString(YahooMediaMedium medium)
        /// <summary>
        /// Returns the content medium identifier for the supplied <see cref="YahooMediaMedium"/>.
        /// </summary>
        /// <param name="medium">The <see cref="YahooMediaMedium"/> to get the content medium identifier for.</param>
        /// <returns>The content medium identifier for the supplied <paramref name="medium"/>, otherwise returns an empty string.</returns>
        public static string MediumAsString(YahooMediaMedium medium)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaMedium).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaMedium))
                {
                    YahooMediaMedium mediaMedium    = (YahooMediaMedium)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (mediaMedium == medium)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region MediumByName(string name)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            YahooMediaMedium mediaMedium    = YahooMediaMedium.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content medium based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaMedium).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaMedium))
                {
                    YahooMediaMedium medium     = (YahooMediaMedium)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            mediaMedium = medium;
                            break;
                        }
                    }
                }
            }

            return mediaMedium;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(IXPathNavigable source)
        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(IXPathNavigable source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            XPathNavigator navigator    = source.CreateNavigator();
            wasLoaded                   = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));

            //------------------------------------------------------------
            //	Raise extension loaded event
            //------------------------------------------------------------
            SyndicationExtensionLoadedEventArgs args    = new SyndicationExtensionLoadedEventArgs(source, this);
            this.OnExtensionLoaded(args);

            return wasLoaded;
        }
        #endregion

        #region Load(XmlReader reader)
        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(XmlReader reader)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(reader, "reader");

            //------------------------------------------------------------
            //	Create navigator against reader and pass to load method
            //------------------------------------------------------------
            XPathDocument document  = new XPathDocument(reader);

            return this.Load(document.CreateNavigator());
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public override void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            this.Context.WriteTo(writer, this.XmlNamespace);
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Fragment;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
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
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            YahooMediaSyndicationExtension value  = obj as YahooMediaSyndicationExtension;

            if (value != null)
            {
                int result  = YahooMediaUtility.CompareSequence((Collection<YahooMediaContent>)this.Context.Contents, (Collection<YahooMediaContent>)value.Context.Contents);
                result      = result | YahooMediaUtility.CompareSequence((Collection<YahooMediaGroup>)this.Context.Groups, (Collection<YahooMediaGroup>)value.Context.Groups);
                
                result      = result | YahooMediaUtility.CompareCommonObjectEntities(this.Context, value.Context);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is YahooMediaSyndicationExtension))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
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
        #endregion

        #region != operator
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
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(YahooMediaSyndicationExtension first, YahooMediaSyndicationExtension second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
        #endregion
    }
}
