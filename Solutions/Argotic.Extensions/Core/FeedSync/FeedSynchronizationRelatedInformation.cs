/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created FeedSynchronizationRelatedInformation Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents information about related feeds or locations.
    /// </summary>
    /// <remarks>
    ///     In the case where a publisher’s feed has incorporated items from other feeds, it can be useful for subscribers to see more detailed information about the other feeds. 
    ///     In the case of feed sharing as envisioned by the <i>FeedSync</i> specification, this class can also be used to notify subscribing feeds of the feeds of other participants 
    ///     which they might also wish to subscribe to.
    /// </remarks>
    /// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
    [Serializable()]
    public class FeedSynchronizationRelatedInformation : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the URI for the related feed.
        /// </summary>
        private Uri relatedInformationLink;
        /// <summary>
        /// Private member to hold the name or description of the related feed.
        /// </summary>
        private string relatedInformationTitle                                      = String.Empty;
        /// <summary>
        /// Private member to hold the type of the related feed.
        /// </summary>
        private FeedSynchronizationRelatedInformationType relatedInformationType    = FeedSynchronizationRelatedInformationType.None;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region FeedSynchronizationRelatedInformation()
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class.
        /// </summary>
        public FeedSynchronizationRelatedInformation()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type)
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
        /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Link           = link;
            this.RelationType   = type;
        }
        #endregion

        #region FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type, string title)
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
        /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
        /// <param name="title">The name or description of this related feed.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type, string title) : this(link, type)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Title  = title;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Link
        /// <summary>
        /// Gets or sets the URI for this related feed.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URI for this related feed.</value>
        /// <remarks>
        ///     The value <b>must not</b> be a relative reference.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Link
        {
            get
            {
                return relatedInformationLink;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                relatedInformationLink = value;
            }
        }
        #endregion

        #region RelationType
        /// <summary>
        /// Gets or sets the type of the related feed.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed. 
        ///     The default value is <see cref="FeedSynchronizationRelatedInformationType.None"/>, which indicates that no relation type has been specified.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window. 
        ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
        ///     </para>
        ///     <para>
        ///         In the feed sharing context new subscribers, or existing subscribers failing to subscribe within the published feed window, will need to initially 
        ///         copy a complete set of items from a publisher before being in a position to process incremental updates. As such, the specification provides for the 
        ///         ability for the latter feed to reference the complete feed.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentException">The <paramref name="value"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformationType RelationType
        {
            get
            {
                return relatedInformationType;
            }

            set
            {
                if (value == FeedSynchronizationRelatedInformationType.None)
                {
                    throw new ArgumentException(String.Format(null, "The specified relation type of {0} is invalid.", value), "value");
                }
                relatedInformationType = value;
            }
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets the name or description of this related feed.
        /// </summary>
        /// <value>The name or description of this related feed.</value>
        public string Title
        {
            get
            {
                return relatedInformationTitle;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    relatedInformationTitle = String.Empty;
                }
                else
                {
                    relatedInformationTitle = value.Trim();
                }
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region RelationTypeAsString(FeedSynchronizationRelatedInformationType type)
        /// <summary>
        /// Returns the relation type identifier for the supplied <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="type">The <see cref="FeedSynchronizationRelatedInformationType"/> to get the relation type identifier for.</param>
        /// <returns>The relation type identifier for the supplied <paramref name="vocabulary"/>, otherwise returns an empty string.</returns>
        public static string RelationTypeAsString(FeedSynchronizationRelatedInformationType type)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationRelatedInformationType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationRelatedInformationType))
                {
                    FeedSynchronizationRelatedInformationType relationType  = (FeedSynchronizationRelatedInformationType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (relationType == type)
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

        #region RelationTypeByName(string name)
        /// <summary>
        /// Returns the <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified relation type name.
        /// </summary>
        /// <param name="name">The name of the relation type.</param>
        /// <returns>A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified string, otherwise returns <b>FeedSynchronizationRelatedInformationType.None</b>.</returns>
        /// <remarks>This method disregards case of specified relation type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static FeedSynchronizationRelatedInformationType RelationTypeByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            FeedSynchronizationRelatedInformationType relationType  = FeedSynchronizationRelatedInformationType.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationRelatedInformationType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationRelatedInformationType))
                {
                    FeedSynchronizationRelatedInformationType type  = (FeedSynchronizationRelatedInformationType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes                       = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            relationType    = type;
                            break;
                        }
                    }
                }
            }

            return relationType;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="FeedSynchronizationRelatedInformation"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="FeedSynchronizationRelatedInformation"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationRelatedInformation"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if (source.HasAttributes)
            {
                string linkAttribute    = source.GetAttribute("link", String.Empty);
                string titleAttribute   = source.GetAttribute("title", String.Empty);
                string typeAttribute    = source.GetAttribute("type", String.Empty);

                if (!String.IsNullOrEmpty(linkAttribute))
                {
                    Uri link;
                    if (Uri.TryCreate(linkAttribute, UriKind.Absolute, out link))
                    {
                        this.Link   = link;
                        wasLoaded   = true;
                    }
                }

                if (!String.IsNullOrEmpty(titleAttribute))
                {
                    this.Title  = titleAttribute;
                    wasLoaded   = true;
                }

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    FeedSynchronizationRelatedInformationType type  = FeedSynchronizationRelatedInformation.RelationTypeByName(typeAttribute);
                    if (type != FeedSynchronizationRelatedInformationType.None)
                    {
                        this.RelationType   = type;
                        wasLoaded           = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="FeedSynchronizationRelatedInformation"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Create extension instance to retrieve XML namespace
            //------------------------------------------------------------
            FeedSynchronizationSyndicationExtension extension   = new FeedSynchronizationSyndicationExtension();

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("related", extension.XmlNamespace);

            writer.WriteAttributeString("link", extension.XmlNamespace, this.Link != null ? this.Link.ToString() : String.Empty);
            if(!String.IsNullOrEmpty(this.Title))
            {
                writer.WriteAttributeString("title", extension.XmlNamespace, this.Title);
            }
            writer.WriteAttributeString("type", extension.XmlNamespace, FeedSynchronizationRelatedInformation.RelationTypeAsString(this.RelationType));

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.</returns>
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
            FeedSynchronizationRelatedInformation value  = obj as FeedSynchronizationRelatedInformation;

            if (value != null)
            {
                int result  = Uri.Compare(this.Link, value.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result      = result | this.RelationType.CompareTo(value.RelationType);

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
            if (!(obj is FeedSynchronizationRelatedInformation))
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
        public static bool operator ==(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        public static bool operator !=(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        public static bool operator <(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        public static bool operator >(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
