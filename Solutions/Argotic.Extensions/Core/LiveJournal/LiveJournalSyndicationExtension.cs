using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide LiveJournal specific meta-data.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="LiveJournalSyndicationExtension"/> extends syndicated content to specify <a href="https://lj-dev.livejournal.com/">LiveJournal</a> specific metadata for entries.
///         This syndication extension conforms to the <b>LiveJournal RSS Module</b> 2.0 specification, which can be currently found
///         at <a href="https://web.archive.org/web/20080710121013/http://neugierig.org/drop/lj/rss/">https://web.archive.org/web/20080710121013/http://neugierig.org/drop/lj/rss/</a>.
///     </para>
///     <para>
///     <b>This extension matches nothing.</b> It declares the namespace
///     <c>http://livejournal.org/rss/lj/2.0/</c>; live LiveJournal RSS declares
///     <c>http://www.livejournal.org/rss/lj/1.0/</c> — a different host <i>and</i> a different version
///     — and LiveJournal's Atom feeds declare a third spelling again. Verified against three real
///     documents. Extension selection keys on the namespace URI, so no real feed has ever attached
///     this type.
///     </para>
///     <para>
///     Correcting the URI would not be enough on its own. Real feeds use nine <c>lj:</c> elements —
///     <c>journal</c>, <c>journalid</c>, <c>journaltype</c>, <c>mood</c>, <c>music</c>, <c>poster</c>,
///     <c>posterid</c>, <c>reply-count</c> and <c>security</c> — of which this extension models three;
///     it also models <c>preformatted</c> and <c>userpic</c>, which no observed document emits. Both
///     halves are recorded as an open decision in §5.1 of <c>docs/build-warnings.md</c> rather than
///     changed here, because an extension's namespace is its identity and moving it is a public
///     behaviour change.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\LiveJournalSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the LiveJournalSyndicationExtension class." />
/// </example>
public class LiveJournalSyndicationExtension : SyndicationExtension, IComparable<LiveJournalSyndicationExtension>, IEquatable<LiveJournalSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalSyndicationExtension"/> class.
    /// </summary>
    public LiveJournalSyndicationExtension()
        : base("lj", "http://livejournal.org/rss/lj/2.0/", new Version("2.0"), new Uri("http://neugierig.org/drop/lj/rss/"), "LiveJournal", "Extends syndication feeds to provide LiveJournal specific meta-data.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="LiveJournalSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="LiveJournalSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public LiveJournalSyndicationExtensionContext Context
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is LiveJournalSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="LiveJournalSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="LiveJournalSyndicationExtension"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="LiveJournalSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="LiveJournalSyndicationExtension"/> was initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalSyndicationExtension"/>.
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
    public int CompareTo(LiveJournalSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Context.IsPreformatted.CompareTo(other.Context.IsPreformatted);

        if (result == 0) result = (this.Context.Mood, other.Context.Mood) switch
        {
            (LiveJournalMood mood, LiveJournalMood otherMood) => mood.CompareTo(otherMood),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = string.Compare(this.Context.Music, other.Context.Music, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = (this.Context.Security, other.Context.Security) switch
        {
            (LiveJournalSecurity security, LiveJournalSecurity otherSecurity) => security.CompareTo(otherSecurity),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = (this.Context.UserPicture, other.Context.UserPicture) switch
        {
            (LiveJournalUserPicture userPicture, LiveJournalUserPicture otherUserPicture) => userPicture.CompareTo(otherUserPicture),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="LiveJournalSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="LiveJournalSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="LiveJournalSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(LiveJournalSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is LiveJournalSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Context.IsPreformatted), HashCodeUtility.Component(this.Context.Mood), HashCodeUtility.Component(this.Context.Music), HashCodeUtility.Component(this.Context.Security), HashCodeUtility.Component(this.Context.UserPicture));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(LiveJournalSyndicationExtension? first, LiveJournalSyndicationExtension? second)
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
    public static bool operator !=(LiveJournalSyndicationExtension? first, LiveJournalSyndicationExtension? second) => !(first == second);

}