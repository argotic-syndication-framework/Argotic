using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents information that will be used to contact the owner of a podcast for communication specifically about their podcast.
/// </summary>
/// <seealso cref="ITunesSyndicationExtensionContext"/>
[Serializable]
public class ITunesOwner : IComparable<ITunesOwner>, IEquatable<ITunesOwner>, IComparisonOperators
{

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
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the name of this owner.
    /// </summary>
    /// <value>The name of this owner.</value>
    public string Name
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="ITunesOwner"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ITunesOwner"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesOwner"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ITunesSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasChildren)
        {
            XPathNavigator? emailNavigator = source.SelectSingleNode("itunes:email", manager);
            XPathNavigator? nameNavigator = source.SelectSingleNode("itunes:name", manager);

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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ITunesSyndicationExtension extension = new();
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
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ITunesOwner? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.EmailAddress, other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ITunesOwner"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ITunesOwner"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ITunesOwner"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(ITunesOwner? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ITunesOwner other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Name));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ITunesOwner? first, ITunesOwner? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(ITunesOwner? first, ITunesOwner? second)
    {
        return !(first == second);
    }

}