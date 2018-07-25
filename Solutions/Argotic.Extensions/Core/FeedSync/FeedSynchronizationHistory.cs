﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents information about updates to a <see cref="FeedSynchronizationItem"/>.
    /// </summary>
    /// <seealso cref="FeedSynchronizationItem.Histories"/>
    /// <seealso cref="FeedSynchronizationItem"/>
    [Serializable]
    public class FeedSynchronizationHistory : IComparable
    {
        /// <summary>
        /// Private member to hold the sequencing of individual updates for the purposes of conflict detection.
        /// </summary>
        private int historySequence = 1;

        /// <summary>
        /// Private member to hold the text value that uniquely identifies the endpoint that made the modification.
        /// </summary>
        private string historyBy = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationHistory"/> class.
        /// </summary>
        public FeedSynchronizationHistory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationHistory"/> class using the supplied sequence number.
        /// </summary>
        /// <param name="sequence">An integer that is used in the sequencing of individual updates for the purposes of conflict detection.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="sequence"/> is less than <b>1</b>.</exception>
        public FeedSynchronizationHistory(int sequence)
        {
            this.Sequence = sequence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationHistory"/> class using the supplied parameters.
        /// </summary>
        /// <param name="sequence">An integer that is used in the sequencing of individual updates for the purposes of conflict detection.</param>
        /// <param name="utcWhen">A <see cref="DateTime"/> that represents the date-time for the device that performed the item modification.</param>
        /// <param name="by">A text value that uniquely identifies the endpoint that made the modification.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="sequence"/> is less than <b>1</b>.</exception>
        public FeedSynchronizationHistory(int sequence, DateTime utcWhen, string by)
            : this(sequence)
        {
            this.When = utcWhen;
            this.By = by;
        }

        /// <summary>
        /// Gets or sets the text value that uniquely identifies the endpoint that made the modification.
        /// </summary>
        /// <value>A text value that uniquely identifies the endpoint that made the modification.</value>
        /// <remarks>
        ///     <para>
        ///         Either or both of the <see cref="When"/> or <see cref="By"/> properties <b>must</b> be present; it is invalid to have neither.
        ///         It is <i>recommended</i> that implementers specify both <see cref="When"/> and <see cref="By"/> properties whenever possible.
        ///     </para>
        ///     <para>
        ///         Implementations <b>should not</b> assume that the <see cref="By"/> property will be a human-readable indication of the author of the item;
        ///         it is simply intended as a unique identifier for use in the synchronization algorithm.
        ///         The precise entity represented by an endpoint will vary depending on the application.
        ///     </para>
        /// </remarks>
        public string By
        {
            get
            {
                return this.historyBy;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.historyBy = string.Empty;
                }
                else
                {
                    this.historyBy = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the sequencing number used for the purpose of conflict detection.
        /// </summary>
        /// <value>An integer that is used in the sequencing of individual updates for the purposes of conflict detection. The default value is <b>1</b>.</value>
        /// <remarks>
        ///     The sequence number is typically assigned by copying the <see cref="FeedSynchronizationItem.Updates"/> value on <see cref="FeedSynchronizationItem"/>,
        ///     after it has been incremented at the time of an update.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <b>1</b>.</exception>
        public int Sequence
        {
            get
            {
                return this.historySequence;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", 1);
                this.historySequence = value;
            }
        }

        /// <summary>
        /// Gets or sets the date-time for the device that performed the item modification.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that represents the date-time for the device that performed the item modification.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no date-time was specified.
        /// </value>
        /// <remarks>
        ///     <para>The value for this property <i>should</i> be interpreted as a best effort, uncalibrated value.</para>
        ///     <para>
        ///         Either or both of the <see cref="When"/> or <see cref="By"/> properties <b>must</b> be present; it is invalid to have neither.
        ///         It is <i>recommended</i> that implementers specify both <see cref="When"/> and <see cref="By"/> properties whenever possible.
        ///     </para>
        ///     <para>The <see cref="DateTime"/> value should be provided in Coordinated Universal Time (UTC).</para>
        /// </remarks>
        public DateTime When { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(FeedSynchronizationHistory first, FeedSynchronizationHistory second)
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
        public static bool operator !=(FeedSynchronizationHistory first, FeedSynchronizationHistory second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(FeedSynchronizationHistory first, FeedSynchronizationHistory second)
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
        public static bool operator >(FeedSynchronizationHistory first, FeedSynchronizationHistory second)
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
        /// Loads this <see cref="FeedSynchronizationHistory"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="FeedSynchronizationHistory"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationHistory"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string sequenceAttribute = source.GetAttribute("sequence", string.Empty);
                string whenAttribute = source.GetAttribute("when", string.Empty);
                string byAttribute = source.GetAttribute("by", string.Empty);

                if (!string.IsNullOrEmpty(sequenceAttribute))
                {
                    int sequence;
                    if (int.TryParse(sequenceAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out sequence))
                    {
                        this.Sequence = sequence;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(whenAttribute))
                {
                    DateTime when;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(whenAttribute, out when))
                    {
                        this.When = when;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(byAttribute))
                {
                    this.By = byAttribute;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="FeedSynchronizationHistory"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            FeedSynchronizationSyndicationExtension extension = new FeedSynchronizationSyndicationExtension();

            writer.WriteStartElement("history", extension.XmlNamespace);
            writer.WriteAttributeString("sequence", this.Sequence.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));

            if (this.When != DateTime.MinValue)
            {
                writer.WriteAttributeString("when", SyndicationDateTimeUtility.ToRfc3339DateTime(this.When));
            }

            if (!string.IsNullOrEmpty(this.By))
            {
                writer.WriteAttributeString("when", this.By);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="FeedSynchronizationHistory"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="FeedSynchronizationHistory"/>.</returns>
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

            FeedSynchronizationHistory value = obj as FeedSynchronizationHistory;

            if (value != null)
            {
                int result = string.Compare(this.By, value.By, StringComparison.OrdinalIgnoreCase);
                result = result | this.Sequence.CompareTo(value.Sequence);
                result = result | this.When.CompareTo(value.When);

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
            if (!(obj is FeedSynchronizationHistory))
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