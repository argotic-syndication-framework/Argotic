﻿using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication
{
    /// <summary>
    /// Represents the owner of an <see cref="OpmlDocument"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    [Serializable()]
    public class OpmlOwner : IComparable
    {

        /// <summary>
        /// Private member to hold the name of the owner of the document.
        /// </summary>
        private string ownerName    = String.Empty;
        /// <summary>
        /// Private member to hold email address of the owner of the document.
        /// </summary>
        private string ownerEmail   = String.Empty;
        /// <summary>
        /// Private member to hold the http address of a web page that contains information that allows a human reader to communicate with the author of the document via email or other means.
        /// </summary>
        private Uri ownerId;
        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOwner"/> class.
        /// </summary>
        public OpmlOwner()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name.
        /// </summary>
        /// <param name="name">The name of the owner of this document.</param>
        public OpmlOwner(string name)
        {
            this.Name   = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name and email address.
        /// </summary>
        /// <param name="name">The name of the owner of this document.</param>
        /// <param name="emailAddress">The email address of the owner of this document.</param>
        public OpmlOwner(string name, string emailAddress) : this(name)
        {
            this.EmailAddress   = emailAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name, email address, and web page.
        /// </summary>
        /// <param name="name">The name of the owner of this document.</param>
        /// <param name="emailAddress">The email address of the owner of this document.</param>
        /// <param name="id">
        ///     A <see cref="Uri"/> that represents the http address of a web page that contains information 
        ///     that allows a human reader to communicate with the author of the document via email or other means.
        /// </param>
        public OpmlOwner(string name, string emailAddress, Uri id) : this(name, emailAddress)
        {
            this.Id             = id;
        }
        /// <summary>
        /// Gets or sets the email address of the owner of this document.
        /// </summary>
        /// <value>The email address of the owner of this document.</value>
        public string EmailAddress
        {
            get
            {
                return ownerEmail;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ownerEmail = String.Empty;
                }
                else
                {
                    ownerEmail = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the http address of a web page that contains information that allows a human reader to communicate with the author of the document via email or other means.
        /// </summary>
        /// <value>
        ///     A <see cref="Uri"/> that represents the http address of a web page that contains information 
        ///     that allows a human reader to communicate with the author of the document via email or other means.
        /// </value>
        /// <remarks>
        ///     The owner identifier may also may be used to identify the author. No two authors should have the same identifier.
        /// </remarks>
        public Uri Id
        {
            get
            {
                return ownerId;
            }

            set
            {
                ownerId = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the owner of this document.
        /// </summary>
        /// <value>The name of the owner of this document.</value>
        public string Name
        {
            get
            {
                return ownerName;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    ownerName   = String.Empty;
                }
                else
                {
                    ownerName   = value.Trim();
                }
            }
        }
        /// <summary>
        /// Loads this <see cref="OpmlOwner"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="OpmlOwner"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded              = false;
            Guard.ArgumentNotNull(source, "source");
            XPathNavigator ownerNameNavigator   = source.SelectSingleNode("ownerName");
            XPathNavigator ownerEmailNavigator  = source.SelectSingleNode("ownerEmail");
            XPathNavigator ownerIdNavigator     = source.SelectSingleNode("ownerId");

            if (ownerNameNavigator != null)
            {
                this.Name           = ownerNameNavigator.Value;
                wasLoaded           = true;
            }

            if (ownerEmailNavigator != null)
            {
                this.EmailAddress   = ownerEmailNavigator.Value;
                wasLoaded           = true;
            }

            if (ownerIdNavigator != null)
            {
                Uri id;
                if (Uri.TryCreate(ownerIdNavigator.Value, UriKind.RelativeOrAbsolute, out id))
                {
                    this.Id     = id;
                    wasLoaded   = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="OpmlOwner"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            if(!String.IsNullOrEmpty(this.Name))
            {
                writer.WriteElementString("ownerName", this.Name);
            }

            if (!String.IsNullOrEmpty(this.EmailAddress))
            {
                writer.WriteElementString("ownerEmail", this.EmailAddress);
            }

            if (this.Id != null)
            {
                writer.WriteElementString("ownerId", this.Id.ToString());
            }
        }
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="OpmlOwner"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="OpmlOwner"/>.</returns>
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
            OpmlOwner value  = obj as OpmlOwner;

            if (value != null)
            {
                int result  = String.Compare(this.EmailAddress, value.EmailAddress, StringComparison.OrdinalIgnoreCase);
                result      = result | Uri.Compare(this.Id, value.Id, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is OpmlOwner))
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
        public static bool operator ==(OpmlOwner first, OpmlOwner second)
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
        public static bool operator !=(OpmlOwner first, OpmlOwner second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(OpmlOwner first, OpmlOwner second)
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
        public static bool operator >(OpmlOwner first, OpmlOwner second)
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