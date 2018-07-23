namespace Argotic.Syndication
{
    using System;
    using System.Collections.ObjectModel;

    using Argotic.Common;

    /// <summary>
    /// Represents a format agnostic view of the discrete content for a syndication feed.
    /// </summary>
    /// <seealso cref="GenericSyndicationFeed.Items"/>
    [Serializable]
    public class GenericSyndicationItem : IComparable
    {
        /// <summary>
        /// Private member to hold the collection of categories associated with the item.
        /// </summary>
        private Collection<GenericSyndicationCategory> itemCategories = new Collection<GenericSyndicationCategory>();

        /// <summary>
        /// Private member to hold the publication date of the item.
        /// </summary>
        private DateTime itemPublishedOn = DateTime.MinValue;

        /// <summary>
        /// Private member to hold the summary of the syndication item.
        /// </summary>
        private string itemSummary = string.Empty;

        /// <summary>
        /// Private member to hold the title of the syndication item.
        /// </summary>
        private string itemTitle = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="AtomEntry"/>.
        /// </summary>
        /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationItem(AtomEntry entry)
        {
            Guard.ArgumentNotNull(entry, "entry");
            this.LoadFrom(entry);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="RssItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="RssItem"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationItem(RssItem item)
        {
            Guard.ArgumentNotNull(item, "item");
            this.LoadFrom(item);
        }

        /// <summary>
        /// Gets the categories associated with this item.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="GenericSyndicationCategory"/> objects that represent the categories associated with this item.
        /// </value>
        public Collection<GenericSyndicationCategory> Categories
        {
            get
            {
                if (this.itemCategories == null)
                {
                    this.itemCategories = new Collection<GenericSyndicationCategory>();
                }

                return this.itemCategories;
            }
        }

        /// <summary>
        /// Gets a date-time indicating an instant in time associated with an event early in the life cycle of this item.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> object that represents a date-time indicating an instant in time associated with an event early in the life cycle of this item.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that publication date was specified.
        /// </value>
        /// <remarks>
        ///     When an <see cref="AtomEntry"/> is being abstracted by this generic item, the <see cref="PublishedOn"/> will represent
        ///     the <see cref="AtomEntry.PublishedOn"/> property value if present. If no summary was specified for the <see cref="AtomEntry"/>,
        ///     the <see cref="AtomEntry.UpdatedOn"/> property value will be used if present.
        /// </remarks>
        public DateTime PublishedOn
        {
            get
            {
                return this.itemPublishedOn;
            }
        }

        /// <summary>
        /// Gets a short summary, abstract, or excerpt for this item.
        /// </summary>
        /// <value>
        ///     A short summary, abstract, or excerpt for this item.
        ///     The default value is an <b>empty</b> string, which indicates that no excerpt was specified.
        /// </value>
        /// <remarks>
        ///     When an <see cref="AtomEntry"/> is being abstracted by this generic item, the <see cref="Summary"/> will represent
        ///     the <see cref="AtomEntry.Summary"/> property value if present. If no summary was specified for the <see cref="AtomEntry"/>,
        ///     the <see cref="AtomEntry.Content"/> property value will be used if present.
        /// </remarks>
        public string Summary
        {
            get
            {
                return this.itemSummary;
            }
        }

        /// <summary>
        /// Gets the human-readable title for this item.
        /// </summary>
        /// <value>
        ///     The human-readable title for this item.
        ///     The default value is an <b>empty</b> string, which indicates that no title was specified.
        /// </value>
        public string Title
        {
            get
            {
                return this.itemTitle;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(GenericSyndicationItem first, GenericSyndicationItem second)
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
        public static bool operator >(GenericSyndicationItem first, GenericSyndicationItem second)
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
        public static bool operator !=(GenericSyndicationItem first, GenericSyndicationItem second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(GenericSyndicationItem first, GenericSyndicationItem second)
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

            GenericSyndicationItem value = obj as GenericSyndicationItem;

            if (value != null)
            {
                int result = GenericSyndicationFeed.CompareSequence(this.Categories, value.Categories);
                result = result | string.Compare(this.Summary, value.Summary, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is GenericSyndicationItem))
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

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="GenericSyndicationItem"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="GenericSyndicationItem"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(
                null,
                "GenericSyndicationItem(Title = {0}, Summary = {1}, PublishedOn = {2})",
                this.Title,
                this.Summary,
                this.PublishedOn != DateTime.MinValue ? this.PublishedOn.ToLongDateString() : string.Empty);
        }

        /// <summary>
        /// Loads the generic syndication item using the supplied <see cref="AtomEntry"/>.
        /// </summary>
        /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        private void LoadFrom(AtomEntry entry)
        {
            Guard.ArgumentNotNull(entry, "entry");

            if (entry.Title != null && !string.IsNullOrEmpty(entry.Title.Content))
            {
                this.itemTitle = entry.Title.Content.Trim();
            }

            if (entry.PublishedOn != DateTime.MinValue)
            {
                this.itemPublishedOn = entry.PublishedOn;
            }
            else if (entry.UpdatedOn != DateTime.MinValue)
            {
                this.itemPublishedOn = entry.UpdatedOn;
            }

            if (entry.Summary != null && !string.IsNullOrEmpty(entry.Summary.Content))
            {
                this.itemSummary = entry.Summary.Content.Trim();
            }
            else if (entry.Content != null && !string.IsNullOrEmpty(entry.Content.Content))
            {
                this.itemSummary = entry.Content.Content.Trim();
            }

            foreach (AtomCategory category in entry.Categories)
            {
                GenericSyndicationCategory genericCategory = new GenericSyndicationCategory(category);
                this.itemCategories.Add(genericCategory);
            }
        }

        /// <summary>
        /// Loads the generic syndication item using the supplied <see cref="RssItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="RssItem"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference (Nothing in Visual Basic).</exception>
        private void LoadFrom(RssItem item)
        {
            Guard.ArgumentNotNull(item, "item");
            if (!string.IsNullOrEmpty(item.Title))
            {
                this.itemTitle = item.Title.Trim();
            }

            if (item.PublicationDate != DateTime.MinValue)
            {
                this.itemPublishedOn = item.PublicationDate;
            }

            if (!string.IsNullOrEmpty(item.Description))
            {
                this.itemSummary = item.Description.Trim();
            }

            foreach (RssCategory category in item.Categories)
            {
                GenericSyndicationCategory genericCategory = new GenericSyndicationCategory(category);
                this.itemCategories.Add(genericCategory);
            }
        }
    }
}