﻿namespace Argotic.Net
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Web;

    using Argotic.Common;

    /// <summary>
    /// Represents a Trackback ping request that can be sent using the <see cref="TrackbackClient"/> class.
    /// </summary>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackMessage class.">
    ///         <code source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs" region="TrackbackClient"/>
    ///     </code>
    /// </example>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Trackback")]
    public class TrackbackMessage : IComparable
    {
        /// <summary>
        /// Private member to hold the character encoding of the message.
        /// </summary>
        private Encoding messageEncoding = Encoding.UTF8;

        /// <summary>
        /// Private member to hold an excerpt of the entry.
        /// </summary>
        private string messageExcerpt = string.Empty;

        /// <summary>
        /// Private member to hold the title of the entry.
        /// </summary>
        private string messageTitle = string.Empty;

        /// <summary>
        /// Private member to hold the permalink for the entry.
        /// </summary>
        private Uri messageUrl;

        /// <summary>
        /// Private member to hold the name of the weblog to which the entry was posted.
        /// </summary>
        private string messageWeblogName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessage"/> class.
        /// </summary>
        public TrackbackMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessage"/> class using the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="permalink">A <see cref="Uri"/> that represents the permalink for the entry.</param>
        /// <remarks>
        ///     The <paramref name="permalink"/> should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="permalink"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "permalink")]
        public TrackbackMessage(Uri permalink)
        {
            this.Permalink = permalink;
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding">character encoding</see> of this message.
        /// </summary>
        /// <value>A <see cref="Encoding"/> that specifies the character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Encoding Encoding
        {
            get
            {
                return this.messageEncoding;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.messageEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets an excerpt for the entry.
        /// </summary>
        /// <value>An excerpt of the entry.</value>
        /// <remarks>
        ///     The excerpt <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string Excerpt
        {
            get
            {
                return this.messageExcerpt;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.messageExcerpt = string.Empty;
                }
                else
                {
                    this.messageExcerpt = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the permalink for the entry.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the permalink for the entry.</value>
        /// <remarks>
        ///     The permalink should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Permalink")]
        public Uri Permalink
        {
            get
            {
                return this.messageUrl;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.messageUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the title of the entry.
        /// </summary>
        /// <value>The title of the entry.</value>
        /// <remarks>
        ///     The title <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string Title
        {
            get
            {
                return this.messageTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.messageTitle = string.Empty;
                }
                else
                {
                    this.messageTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the weblog to which the entry was posted.
        /// </summary>
        /// <value>The name of the weblog to which the entry was posted.</value>
        /// <remarks>
        ///     The blog name <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
        /// </remarks>
        public string WeblogName
        {
            get
            {
                return this.messageWeblogName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.messageWeblogName = string.Empty;
                }
                else
                {
                    this.messageWeblogName = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(TrackbackMessage first, TrackbackMessage second)
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
        public static bool operator >(TrackbackMessage first, TrackbackMessage second)
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
        public static bool operator !=(TrackbackMessage first, TrackbackMessage second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(TrackbackMessage first, TrackbackMessage second)
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

            TrackbackMessage value = obj as TrackbackMessage;

            if (value != null)
            {
                int result = string.Compare(
                    this.Encoding.WebName,
                    value.Encoding.WebName,
                    StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Excerpt, value.Excerpt, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(
                             this.Permalink,
                             value.Permalink,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.WeblogName, value.WeblogName, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is TrackbackMessage))
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
        /// Loads this <see cref="TrackbackMessage"/> using the supplied <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="source">The <see cref="NameValueCollection"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="TrackbackMessage"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be the HTTP Request Parameters or a similar subset.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(NameValueCollection source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.Count > 0)
            {
                foreach (string parameterName in source.AllKeys)
                {
                    if (string.Compare(parameterName, "url", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Uri url;
                        if (Uri.TryCreate(source[parameterName], UriKind.RelativeOrAbsolute, out url))
                        {
                            this.Permalink = url;
                            wasLoaded = true;
                        }
                    }
                    else if (string.Compare(parameterName, "title", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!string.IsNullOrEmpty(source[parameterName]))
                        {
                            this.Title = source[parameterName];
                            wasLoaded = true;
                        }
                    }
                    else if (string.Compare(parameterName, "excerpt", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!string.IsNullOrEmpty(source[parameterName]))
                        {
                            this.Excerpt = source[parameterName];
                            wasLoaded = true;
                        }
                    }
                    else if (string.Compare(parameterName, "blog_name", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!string.IsNullOrEmpty(source[parameterName]))
                        {
                            this.WeblogName = source[parameterName];
                            wasLoaded = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
        /// <remarks>
        ///     This method returns the URL-encoded representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, this.Encoding))
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
        /// Saves the current <see cref="TrackbackMessage"/> to the specified <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(StreamWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.Write(
                string.Format(null, "url={0}", this.Permalink != null ? this.Permalink.ToString() : string.Empty));

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.Write(string.Format(null, "&title={0}", HttpUtility.UrlEncode(this.Title)));
            }

            if (!string.IsNullOrEmpty(this.WeblogName))
            {
                writer.Write(string.Format(null, "&blog_name={0}", HttpUtility.UrlEncode(this.WeblogName)));
            }

            if (!string.IsNullOrEmpty(this.Excerpt))
            {
                writer.Write(string.Format(null, "&excerpt={0}", HttpUtility.UrlEncode(this.Excerpt)));
            }
        }
    }
}