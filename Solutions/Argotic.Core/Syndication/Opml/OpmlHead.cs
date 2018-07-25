﻿namespace Argotic.Syndication
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
    /// Represents the header information for an <see cref="OpmlDocument"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Opml")]
    [Serializable]
    public class OpmlHead : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the http address of the documentation the OPML document conforms to.
        /// </summary>
        private Uri headDocumentation = new Uri("http://www.opml.org/spec2");

        /// <summary>
        /// Private member to hold a collection of line numbers that are expanded.
        /// </summary>
        private Collection<int> headExpansionState;

        /// <summary>
        /// Private member to hold information that describes the owner of the document.
        /// </summary>
        private OpmlOwner headOwner;

        /// <summary>
        /// Private member to hold the title of the document.
        /// </summary>
        private string headTitle = string.Empty;

        /// <summary>
        /// Private member to hold information that describes the pixel locations of the outline window.
        /// </summary>
        private OpmlWindow headWindow;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlHead"/> class.
        /// </summary>
        public OpmlHead()
        {
        }

        /// <summary>
        /// Gets or sets a date-time indicating when this document was created.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that indicates when this document was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.</value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime CreatedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the http address of the documentation that this OPML document conforms to.
        /// </summary>
        /// <value>
        ///     A <see cref="Uri"/> that represents the http address of the documentation that this OPML document conforms to.
        /// </value>
        public Uri Documentation => this.headDocumentation;

        /// <summary>
        /// Gets a collection of line numbers that are expanded within the outline.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> of integers that represent the line numbers that are expanded within the outline.</value>
        /// <remarks>
        ///     The line numbers in the collection tell you which headlines to expand. The order is important.
        ///     For each element in the collection, X, starting at the first summit, navigate flatdown X times and expand. Repeat for each element in the collection.
        /// </remarks>
        public Collection<int> ExpansionState
        {
            get
            {
                if (this.headExpansionState == null)
                {
                    this.headExpansionState = new Collection<int>();
                }

                return this.headExpansionState;
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
        /// Gets or sets a date-time indicating when this document was created.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that indicates when this document was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date was provided.</value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime ModifiedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets information that describes the owner of this document.
        /// </summary>
        /// <value>A <see cref="OpmlOwner"/> object that provides information that describes the owner of this document.</value>
        public OpmlOwner Owner
        {
            get
            {
                return this.headOwner;
            }

            set
            {
                if (value == null)
                {
                    this.headOwner = null;
                }
                else
                {
                    this.headOwner = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the title of this document.
        /// </summary>
        /// <value>The title of this document.</value>
        public string Title
        {
            get
            {
                return this.headTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.headTitle = string.Empty;
                }
                else
                {
                    this.headTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a number indicating which line of this outline is displayed on the top line of the window.
        /// </summary>
        /// <value>
        ///     An integer that indicates which line of this outline is displayed on the top line of the window.
        ///     The default value is <see cref="int.MinValue"/>, which indicates that no vertical scroll state was provided.
        /// </value>
        public int VerticalScrollState { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets information that describes the pixel location of the edges of the outline window for this document.
        /// </summary>
        /// <value>A <see cref="OpmlWindow"/> object that provides information that describes the pixel location of the edges of the outline window for this document.</value>
        public OpmlWindow Window
        {
            get
            {
                return this.headWindow;
            }

            set
            {
                if (value == null)
                {
                    this.headWindow = null;
                }
                else
                {
                    this.headWindow = value;
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(OpmlHead first, OpmlHead second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator >(OpmlHead first, OpmlHead second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator !=(OpmlHead first, OpmlHead second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(OpmlHead first, OpmlHead second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
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

            OpmlHead value = obj as OpmlHead;

            if (value != null)
            {
                int result = 0; // String.Compare(this.Domain, value.Domain, StringComparison.OrdinalIgnoreCase);

                return result;
            }

            throw new ArgumentException(
                string.Format(
                    null,
                    "obj is not of type {0}, type was found to be '{1}'.",
                    this.GetType().FullName,
                    obj.GetType().FullName),
                "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is OpmlHead))
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
        /// Loads this <see cref="OpmlHead"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XPathNavigator titleNavigator = source.SelectSingleNode("title");
            XPathNavigator dateCreatedNavigator = source.SelectSingleNode("dateCreated");
            XPathNavigator dateModifiedNavigator = source.SelectSingleNode("dateModified");
            XPathNavigator expansionStateNavigator = source.SelectSingleNode("expansionState");
            XPathNavigator verticalScrollStateNavigator = source.SelectSingleNode("vertScrollState");

            if (titleNavigator != null)
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            if (dateCreatedNavigator != null)
            {
                DateTime createdOn;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(dateCreatedNavigator.Value, out createdOn))
                {
                    this.CreatedOn = createdOn;
                    wasLoaded = true;
                }
            }

            if (dateModifiedNavigator != null)
            {
                DateTime modifiedOn;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(dateModifiedNavigator.Value, out modifiedOn))
                {
                    this.ModifiedOn = modifiedOn;
                    wasLoaded = true;
                }
            }

            OpmlOwner owner = new OpmlOwner();
            if (owner.Load(source))
            {
                this.Owner = owner;
            }

            if (expansionStateNavigator != null && !string.IsNullOrEmpty(expansionStateNavigator.Value))
            {
                if (expansionStateNavigator.Value.Contains(","))
                {
                    string[] expansionStates = expansionStateNavigator.Value.Split(
                        ",".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string expansionState in expansionStates)
                    {
                        int state;
                        if (int.TryParse(
                            expansionState.Trim(),
                            System.Globalization.NumberStyles.Integer,
                            System.Globalization.NumberFormatInfo.InvariantInfo,
                            out state))
                        {
                            this.ExpansionState.Add(state);
                            wasLoaded = true;
                        }
                    }
                }
                else
                {
                    int expansionState;
                    if (int.TryParse(
                        expansionStateNavigator.Value,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.NumberFormatInfo.InvariantInfo,
                        out expansionState))
                    {
                        this.ExpansionState.Add(expansionState);
                        wasLoaded = true;
                    }
                }
            }

            if (verticalScrollStateNavigator != null)
            {
                int verticalScrollState;
                if (int.TryParse(
                    verticalScrollStateNavigator.Value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.NumberFormatInfo.InvariantInfo,
                    out verticalScrollState))
                {
                    this.VerticalScrollState = verticalScrollState;
                    wasLoaded = true;
                }
            }

            OpmlWindow window = new OpmlWindow();
            if (window.Load(source))
            {
                this.Window = window;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="OpmlHead"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="OpmlHead"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlHead"/>.</returns>
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
        /// Saves the current <see cref="OpmlHead"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("head");

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("title", this.Title);
            }

            if (this.CreatedOn != DateTime.MinValue)
            {
                writer.WriteElementString("dateCreated", SyndicationDateTimeUtility.ToRfc822DateTime(this.CreatedOn));
            }

            if (this.ModifiedOn != DateTime.MinValue)
            {
                writer.WriteElementString("dateModified", SyndicationDateTimeUtility.ToRfc822DateTime(this.ModifiedOn));
            }

            if (this.Owner != null)
            {
                this.Owner.WriteTo(writer);
            }

            if (this.Documentation != null)
            {
                writer.WriteElementString("docs", this.Documentation.ToString());
            }

            if (this.ExpansionState.Count > 0)
            {
                string[] values = new string[this.ExpansionState.Count];
                int[] expansionStates = new int[this.ExpansionState.Count];
                this.ExpansionState.CopyTo(expansionStates, 0);

                for (int i = 0; i < expansionStates.Length; i++)
                {
                    values[i] = expansionStates[i].ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                }

                writer.WriteElementString("expansionState", string.Join(",", values));
            }

            if (this.VerticalScrollState != int.MinValue)
            {
                writer.WriteElementString(
                    "vertScrollState",
                    this.VerticalScrollState.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if (this.Window != null)
            {
                this.Window.WriteTo(writer);
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}