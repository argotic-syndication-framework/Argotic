﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents information that will be used to contact the owner of a podcast for communication specifically about their podcast.
    /// </summary>
    /// <seealso cref="ITunesSyndicationExtensionContext"/>
    [Serializable]
    public class ITunesOwner : IComparable
    {
        /// <summary>
        /// Private member to hold the email address of the owner.
        /// </summary>
        private string ownerEmailAddress = string.Empty;

        /// <summary>
        /// Private member to hold the name of the owner.
        /// </summary>
        private string ownerName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesOwner"/> class.
        /// </summary>
        public ITunesOwner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesOwner"/> class using the supplied email address and name.
        /// </summary>
        /// <param name="emailAddress">The email address of this owner.</param>
        /// <param name="name">The name of this owner.</param>
        public ITunesOwner(string emailAddress, string name)
        {
            this.EmailAddress = emailAddress;
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the email address of this owner.
        /// </summary>
        /// <value>The email address of this owner.</value>
        public string EmailAddress
        {
            get
            {
                return this.ownerEmailAddress;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.ownerEmailAddress = string.Empty;
                }
                else
                {
                    this.ownerEmailAddress = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of this owner.
        /// </summary>
        /// <value>The name of this owner.</value>
        public string Name
        {
            get
            {
                return this.ownerName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.ownerName = string.Empty;
                }
                else
                {
                    this.ownerName = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(ITunesOwner first, ITunesOwner second)
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
        public static bool operator !=(ITunesOwner first, ITunesOwner second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(ITunesOwner first, ITunesOwner second)
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
        public static bool operator >(ITunesOwner first, ITunesOwner second)
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
        /// Loads this <see cref="ITunesOwner"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ITunesOwner"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesOwner"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            ITunesSyndicationExtension extension = new ITunesSyndicationExtension();
            XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
            if (source.HasChildren)
            {
                XPathNavigator emailNavigator = source.SelectSingleNode("itunes:email", manager);
                XPathNavigator nameNavigator = source.SelectSingleNode("itunes:name", manager);

                if (emailNavigator != null && !string.IsNullOrEmpty(emailNavigator.Value))
                {
                    this.EmailAddress = emailNavigator.Value;
                    wasLoaded = true;
                }

                if (nameNavigator != null && !string.IsNullOrEmpty(nameNavigator.Value))
                {
                    this.Name = nameNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="ITunesOwner"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            ITunesSyndicationExtension extension = new ITunesSyndicationExtension();
            writer.WriteStartElement("owner", extension.XmlNamespace);

            if (!string.IsNullOrEmpty(this.EmailAddress))
            {
                writer.WriteElementString("email", extension.XmlNamespace, this.EmailAddress);
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                writer.WriteElementString("name", extension.XmlNamespace, this.Name);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="ITunesOwner"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ITunesOwner"/>.</returns>
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

            ITunesOwner value = obj as ITunesOwner;

            if (value != null)
            {
                int result = string.Compare(this.EmailAddress, value.EmailAddress, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is ITunesOwner))
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
