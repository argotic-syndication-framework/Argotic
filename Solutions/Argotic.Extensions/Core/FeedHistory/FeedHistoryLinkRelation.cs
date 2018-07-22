using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents a link relation used to cross-reference linked feed documents.
    /// </summary>
    /// <seealso cref="FeedHistorySyndicationExtensionContext.Relations"/>
    [Serializable()]
    public class FeedHistoryLinkRelation : IComparable
    {

        /// <summary>
        /// Private member to hold an IRI that identifies the location of the link relation.
        /// </summary>
        private Uri linkRelationLocation;
        /// <summary>
        /// Private member to hold a value that indicates the type of the link relation.
        /// </summary>
        private FeedHistoryLinkRelationType linkRelationType    = FeedHistoryLinkRelationType.None;
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedHistoryLinkRelation"/> class.
        /// </summary>
        public FeedHistoryLinkRelation()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedHistoryLinkRelation"/> class using the supplied relation type and location.
        /// </summary>
        /// <param name="relation">A <see cref="FeedHistoryLinkRelationType"/> enumeration value that indicates the link relation type.</param>
        /// <param name="href">A <see cref="Uri"/> that represents the location this link relation points to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference (Nothing in Visual Basic).</exception>
        public FeedHistoryLinkRelation(FeedHistoryLinkRelationType relation, Uri href)
        {
            this.RelationType   = relation;
            this.Uri            = href;
        }
        /// <summary>
        /// Gets or sets a value that indicates the type of this link relation.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedHistoryLinkRelationType"/> enumeration value that indicates the link relation type. 
        ///     The default value is <see cref="FeedHistoryLinkRelationType.None"/>, which indicates no relationship type was specified.
        /// </value>
        public FeedHistoryLinkRelationType RelationType
        {
            get
            {
                return linkRelationType;
            }

            set
            {
                linkRelationType = value;
            }
        }

        /// <summary>
        /// Gets or sets an IRI that identifies the location of this link relation.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a Internationalized Resource Identifier (IRI) that identifies the location of this link relation.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Uri
        {
            get
            {
                return linkRelationLocation;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                linkRelationLocation = value;
            }
        }
        /// <summary>
        /// Loads this <see cref="FeedHistoryLinkRelation"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="FeedHistoryLinkRelation"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedHistoryLinkRelation"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded              = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string hrefAttribute    = source.GetAttribute("href", String.Empty);
                string relAttribute     = source.GetAttribute("rel", String.Empty);

                if (!String.IsNullOrEmpty(hrefAttribute))
                {
                    Uri href;
                    if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out href))
                    {
                        this.Uri        = href;
                        wasLoaded       = true;
                    }
                }

                if (!String.IsNullOrEmpty(relAttribute))
                {
                    FeedHistoryLinkRelationType relationType    = FeedHistorySyndicationExtension.LinkRelationTypeByName(relAttribute);
                    if (relationType != FeedHistoryLinkRelationType.None)
                    {
                        this.RelationType   = relationType;
                        wasLoaded           = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="FeedHistoryLinkRelation"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("link", "http://www.w3.org/2005/Atom");

            writer.WriteAttributeString("href", this.Uri != null ? this.Uri.ToString() : String.Empty);
            writer.WriteAttributeString("rel", this.RelationType != FeedHistoryLinkRelationType.None ? FeedHistorySyndicationExtension.LinkRelationTypeAsString(this.RelationType) : String.Empty);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="FeedHistoryLinkRelation"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="FeedHistoryLinkRelation"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
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
            FeedHistoryLinkRelation value  = obj as FeedHistoryLinkRelation;

            if (value != null)
            {
                int result  = this.RelationType.CompareTo(value.RelationType);
                result      = result | Uri.Compare(this.Uri, value.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            if (!(obj is FeedHistoryLinkRelation))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(FeedHistoryLinkRelation first, FeedHistoryLinkRelation second)
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
        public static bool operator !=(FeedHistoryLinkRelation first, FeedHistoryLinkRelation second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(FeedHistoryLinkRelation first, FeedHistoryLinkRelation second)
        {
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

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(FeedHistoryLinkRelation first, FeedHistoryLinkRelation second)
        {
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
    }
}