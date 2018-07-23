namespace Argotic.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;
    using System.Xml.XPath;

    /// <summary>
    /// Specifies a set of features to support on a <see cref="ISyndicationResource"/> object loaded by the <see cref="ISyndicationResource.Load(IXPathNavigable, SyndicationResourceLoadSettings)"/> method.
    /// </summary>
    [Serializable]
    public sealed class SyndicationResourceLoadSettings : IComparable
    {
        /// <summary>
        /// Private member to hold the character encoding to use when reading the syndication resource.
        /// </summary>
        private Encoding characterEncoding = Encoding.UTF8;

        /// <summary>
        /// Private member to hold a value indicating the maximum number of resource entities to retrieve from a syndication resource.
        /// </summary>
        private int maximumEntitiesToRetrieve;

        /// <summary>
        /// Private member to hold a value that specifies the amount of time after which a asynchronous load operation call times out.
        /// </summary>
        private TimeSpan requestTimeout = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Private member to hold a collection of types that represent the syndication extensions supported by the load operation.
        /// </summary>
        private Collection<Type> supportedSyndicationExtensions;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if auto-detection of supported syndication extensions is enabled.
        /// </summary>
        /// <value>
        ///     <b>true</b> if the syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on a syndication resource; otherwise <b>false</b>.
        ///     The default value is <b>true</b>.
        /// </value>
        /// <remarks>
        ///     Automatic detection of supported syndication extensions will <b>not</b> remove any syndication extensions already added
        ///     to the <see cref="SupportedExtensions"/> collection prior to the load operation execution.
        /// </remarks>
        public bool AutoDetectExtensions { get; set; } = true;

        /// <summary>
        /// Gets or sets the character encoding to use when parsing a syndication resource.
        /// </summary>
        /// <value>A <see cref="Encoding"/> object that indicates the character encoding to use when parsing a syndication resource. The default value is <see cref="Encoding.UTF8"/>.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Encoding CharacterEncoding
        {
            get
            {
                return this.characterEncoding;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.characterEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of resource entities to retrieve from a syndication resource.
        /// </summary>
        /// <value>The maximum number of entities to retrieve from a syndication resource. The default value is 0, which indicates there is <b>no limit</b>.</value>
        /// <remarks>
        ///     This setting is typically used to optimize processing by reducing the number of resource entities that must be parsed.
        ///     Some syndication resources may not utilize this setting if they do not represent a list of retrievable entities.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than zero.</exception>
        public int RetrievalLimit
        {
            get
            {
                return this.maximumEntitiesToRetrieve;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", 0);
                this.maximumEntitiesToRetrieve = value;
            }
        }

        /// <summary>
        /// Gets the syndication extensions to attempt to load from a syndication resource.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="Type"/> objects that represent syndication extension instances to attempt to instantiate during the load operation.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If <see cref="AutoDetectExtensions"/> is <b>true</b>, this collection will be automatically filled during the load operation based on the XML namespaces declared on the syndication resource.
        ///     Automatic detection will <b>not</b> remove any syndication extensions already added to this collection prior to the load operation execution.
        /// </remarks>
        public Collection<Type> SupportedExtensions
        {
            get
            {
                if (this.supportedSyndicationExtensions == null)
                {
                    this.supportedSyndicationExtensions = new Collection<Type>();
                }

                return this.supportedSyndicationExtensions;
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which asynchronous load operations will time out.
        /// </summary>
        /// <value>An <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
        /// <exception cref="ArgumentOutOfRangeException">The time-out period is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The time-out period is greater than a year.</exception>
        public TimeSpan Timeout
        {
            get
            {
                return this.requestTimeout;
            }

            set
            {
                if (value.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value > TimeSpan.FromDays(365))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.requestTimeout = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
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
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
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
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(SyndicationResourceLoadSettings first, SyndicationResourceLoadSettings second)
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
        /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadSettings"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(null, "[SyndicationResourceLoadSettings(CharacterEncoding = \"{0}\", RetrievalLimit = \"{1}\", Timeout = \"{2}\", Autodetect = \"{3}\", SupportedExtensions = \"{4}\")]", this.CharacterEncoding.WebName, this.RetrievalLimit, this.Timeout.TotalMilliseconds, this.AutoDetectExtensions, this.SupportedExtensions.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
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

            SyndicationResourceLoadSettings value = obj as SyndicationResourceLoadSettings;

            if (value != null)
            {
                int result = string.Compare(this.CharacterEncoding.WebName, value.CharacterEncoding.WebName, StringComparison.OrdinalIgnoreCase);
                result = result | this.RetrievalLimit.CompareTo(value.RetrievalLimit);
                result = result | this.Timeout.CompareTo(value.Timeout);
                result = result | this.AutoDetectExtensions.CompareTo(value.AutoDetectExtensions);
                result = result | ComparisonUtility.CompareSequence(this.SupportedExtensions, value.SupportedExtensions);

                return result;
            }

            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SyndicationResourceLoadSettings))
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