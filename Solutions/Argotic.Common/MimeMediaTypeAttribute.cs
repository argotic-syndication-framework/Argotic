namespace Argotic.Common
{
    using System;

    /// <summary>
    /// Associates IANA MIME media type information with a target element. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     See <a href="http://www.iana.org/assignments/media-types">http://www.iana.org/assignments/media-types</a> for a listing of the registered IANA MIME media types and sub-types.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false)]
    [Serializable]
    public sealed class MimeMediaTypeAttribute : Attribute, IComparable
    {
        /// <summary>
        /// Private member to hold the MIME media type name.
        /// </summary>
        private string mimeMediaTypeName = string.Empty;

        /// <summary>
        /// Private member to hold the MIME media sub-type name.
        /// </summary>
        private string mimeMediaSubTypeName = string.Empty;

        /// <summary>
        /// Private member to hold a URI that points to the documentation the describes the MIME media type.
        /// </summary>
        private Uri mimeMediaDocumentation;

        /// <summary>
        /// Gets or sets a URI that points to the documentation the describes the MIME media type for the attributed field.
        /// </summary>
        /// <value>A <see cref="Uri"/> that points to the documentation the describes the MIME media type for the attributed field.</value>
        public string Documentation
        {
            get
            {
                return this.mimeMediaDocumentation != null ? this.mimeMediaDocumentation.ToString() : string.Empty;
            }

            set
            {
                if (value == null)
                {
                    this.mimeMediaDocumentation = null;
                }
                else
                {
                    Uri url;
                    if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out url))
                    {
                        this.mimeMediaDocumentation = url;
                    }
                    else
                    {
                        this.mimeMediaDocumentation = null;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the MIME media type name for the attributed field.
        /// </summary>
        /// <value>The MIME media type name for the attributed field.</value>
        public string Name
        {
            get
            {
                return this.mimeMediaTypeName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.mimeMediaTypeName = string.Empty;
                }
                else
                {
                    this.mimeMediaTypeName = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the MIME media sub-type name for the attributed field.
        /// </summary>
        /// <value>The MIME media sub-type name for the attributed field.</value>
        public string SubName
        {
            get
            {
                return this.mimeMediaSubTypeName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.mimeMediaSubTypeName = string.Empty;
                }
                else
                {
                    this.mimeMediaSubTypeName = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
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
        public static bool operator !=(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
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
        public static bool operator >(MimeMediaTypeAttribute first, MimeMediaTypeAttribute second)
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
        /// Returns a <see cref="string"/> that represents the current <see cref="MimeMediaTypeAttribute"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="MimeMediaTypeAttribute"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(null, "[MimeMediaType(Name = \"{0}\", SubName = \"{1}\", Documentation = \"{2}\")]", this.Name, this.SubName, this.Documentation != null ? this.Documentation : string.Empty);
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

            MimeMediaTypeAttribute value = obj as MimeMediaTypeAttribute;

            if (value != null)
            {
                int result = string.Compare(this.Documentation, value.Documentation, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.SubName, value.SubName, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is MimeMediaTypeAttribute))
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