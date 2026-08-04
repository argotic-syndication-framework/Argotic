using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing iTunes podcasting information.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="ITunesSyndicationExtension"/> extends syndicated content to specify iTunes podcasting information. This syndication extension conforms to the 
///         <b>iTunes RSS Tags</b> 1.0 specification, which can be found at <a href="http://www.apple.com/itunes/store/podcaststechspecs.html#rss">http://www.apple.com/itunes/store/podcaststechspecs.html#rss</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ITunesSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\ITunesSyndicationExtensionExample.cs" 
///             region="ITunesSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class ITunesSyndicationExtension : SyndicationExtension, IComparable<ITunesSyndicationExtension>, IEquatable<ITunesSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesSyndicationExtension"/> class.
    /// </summary>
    public ITunesSyndicationExtension()
        : base("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd", new Version("1.0"), new Uri("http://www.apple.com/itunes/store/podcaststechspecs.html#rss"), "Apple iTunes Podcasting Extension", "Extends syndication feeds to provide Apple iTunes podcasting media information.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="ITunesSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="ITunesSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public ITunesSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Returns the cloud protocol identifier for the supplied <see cref="ITunesExplicitMaterial"/>.
    /// </summary>
    /// <param name="material">The <see cref="ITunesExplicitMaterial"/> to get the explicit material identifier for.</param>
    /// <returns>The explicit material identifier for the supplied <paramref name="material"/>, Otherwise, returns an empty string.</returns>
    public static string ExplicitMaterialAsString(ITunesExplicitMaterial material) =>
        EnumerationMetadataAttribute.GetAlternateValue(material);

    /// <summary>
    /// Returns the <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified explicit material name.
    /// </summary>
    /// <param name="name">The name of the explicit material.</param>
    /// <returns>A <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>ITunesExplicitMaterial.None</b>.</returns>
    /// <remarks>This method disregards case of specified explicit material name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static ITunesExplicitMaterial ExplicitMaterialByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, ITunesExplicitMaterial.None);

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is ITunesSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ITunesSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="ITunesSyndicationExtension"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(ITunesSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Context.Author, other.Context.Author, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Categories, other.Context.Categories);
        if (result == 0) result = this.Context.Duration.CompareTo(other.Context.Duration);
        if (result == 0) result = this.Context.ExplicitMaterial.CompareTo(other.Context.ExplicitMaterial);
        if (result == 0) result = Uri.Compare(this.Context.Image, other.Context.Image, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Context.IsBlocked.CompareTo(other.Context.IsBlocked);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Keywords, other.Context.Keywords, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Context.NewFeedUrl, other.Context.NewFeedUrl, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Comparer<ITunesOwner>.Default.Compare(this.Context.Owner, other.Context.Owner);
        if (result == 0) result = string.Compare(this.Context.Subtitle, other.Context.Subtitle, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Summary, other.Context.Summary, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ITunesSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ITunesSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ITunesSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(ITunesSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is ITunesSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Context.Author));
        hash.Add(HashCodeUtility.Component(this.Context.Categories.Count));
        hash.Add(HashCodeUtility.Component(this.Context.Duration));
        hash.Add(HashCodeUtility.Component(this.Context.ExplicitMaterial));
        hash.Add(HashCodeUtility.Component(this.Context.Image));
        hash.Add(HashCodeUtility.Component(this.Context.IsBlocked));
        hash.Add(HashCodeUtility.Component(this.Context.Keywords.Count));
        hash.Add(HashCodeUtility.Component(this.Context.NewFeedUrl));
        hash.Add(HashCodeUtility.Component(this.Context.Owner));
        hash.Add(HashCodeUtility.Component(this.Context.Subtitle));
        hash.Add(HashCodeUtility.Component(this.Context.Summary));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ITunesSyndicationExtension? first, ITunesSyndicationExtension? second)
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
    public static bool operator !=(ITunesSyndicationExtension? first, ITunesSyndicationExtension? second) => !(first == second);

}