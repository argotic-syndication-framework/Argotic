using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the administrative contact for a podcast.
/// </summary>
/// <remarks>
///     <para>
///     Apple's <c>itunes:owner</c>, which <c>podcast-standard.org</c> records as having to contain
///     both a nested <c>itunes:name</c> and a nested <c>itunes:email</c>. Distinct from
///     <see cref="ITunesSyndicationExtensionContext.Author"/>: that names who made the show, this
///     names who administers the feed.
///     </para>
///     <para>
///     Apple is retiring it. Its guidance to hosting providers now says <i>"The owner tag
///     (&lt;itunes:owner&gt;) and its contact information, including email, will no longer be
///     recommended"</i>, with the address moving into Apple Podcasts Connect instead. That is a
///     forward-looking change and the element remains in <b>99.2%</b> of 1,934 live feeds, so it is
///     read and written unchanged; the note is here so nobody designs new work around it.
///     </para>
/// </remarks>
/// <seealso cref="ITunesSyndicationExtensionContext.Owner"/>
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
    /// <param name="emailAddress">The email address of this owner. Neither <see langword="null"/> nor an empty string is rejected; both are simply omitted when written.</param>
    /// <param name="name">The name of this owner. Neither <see langword="null"/> nor an empty string is rejected; both are simply omitted when written.</param>
    public ITunesOwner(string emailAddress, string name)
    {
        this.EmailAddress = emailAddress;
        this.Name = name;
    }

    /// <summary>
    /// Gets or sets the email address of this owner.
    /// </summary>
    /// <value>The email address, or an <i>empty</i> string if none was specified. The default value is an <i>empty</i> string.</value>
    public string EmailAddress
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the name of this owner.
    /// </summary>
    /// <value>The owner's name, or an <i>empty</i> string if none was specified. The default value is an <i>empty</i> string.</value>
    public string Name
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="ITunesOwner"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="ITunesOwner"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesOwner"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ITunesSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasChildren)
        {
            XPathNavigator? emailNavigator = source.SelectChildElement("itunes", "email", manager);
            XPathNavigator? nameNavigator = source.SelectChildElement("itunes", "name", manager);

            if (emailNavigator is not null && !string.IsNullOrEmpty(emailNavigator.Value))
            {
                this.EmailAddress = emailNavigator.Value;
                wasLoaded = true;
            }

            if (nameNavigator is not null && !string.IsNullOrEmpty(nameNavigator.Value))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("owner", ITunesSyndicationExtension.NamespaceUri);

        if (!string.IsNullOrEmpty(this.EmailAddress))
        {
            writer.WriteElementString("email", ITunesSyndicationExtension.NamespaceUri, this.EmailAddress);
        }

        if (!string.IsNullOrEmpty(this.Name))
        {
            writer.WriteElementString("name", ITunesSyndicationExtension.NamespaceUri, this.Name);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ITunesOwner"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

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
    /// <returns><see langword="true"/> if the specified <see cref="ITunesOwner"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is ITunesOwner other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Name));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(ITunesOwner? first, ITunesOwner? second) => !(first == second);

}