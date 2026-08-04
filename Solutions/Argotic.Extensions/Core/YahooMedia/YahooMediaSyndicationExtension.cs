using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of supplementing the enclosure capabilities of feeds.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="YahooMediaSyndicationExtension"/> extends syndicated content to extends <i>enclosures</i> to handle other media types, 
///         such as short films or TV, as well as provide additional metadata with the media. This extension enables content publishers and bloggers 
///         to syndicate multimedia content such as TV and video clips, movies, images, and audio.. This syndication extension conforms to the 
///         <b>Media RSS Module</b> 1.1.1 specification, which can be found at <a href="http://search.yahoo.com/mrss">http://search.yahoo.com/mrss</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the YahooMediaSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\YahooMediaSyndicationExtensionExample.cs" 
///             region="YahooMediaSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class YahooMediaSyndicationExtension : SyndicationExtension, IComparable<YahooMediaSyndicationExtension>, IEquatable<YahooMediaSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtension"/> class.
    /// </summary>
    public YahooMediaSyndicationExtension()
        : base("media", "http://search.yahoo.com/mrss/", new Version("1.1.1"), new Uri("http://search.yahoo.com/mrss"), "Yahoo! Media", "Extends syndication feeds to provide a means of supplementing the enclosure capabilities of feeds.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="YahooMediaSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="YahooMediaSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public YahooMediaSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Returns the content expression identifier for the supplied <see cref="YahooMediaExpression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="YahooMediaExpression"/> to get the content expression identifier for.</param>
    /// <returns>The content expression identifier for the supplied <paramref name="expression"/>, Otherwise, returns an empty string.</returns>
    public static string ExpressionAsString(YahooMediaExpression expression) =>
        EnumerationMetadataAttribute.GetAlternateValue(expression);

    /// <summary>
    /// Returns the <see cref="YahooMediaExpression"/> enumeration value that corresponds to the specified content expression name.
    /// </summary>
    /// <param name="name">The name of the content expression.</param>
    /// <returns>A <see cref="YahooMediaExpression"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>YahooMediaExpression.None</b>.</returns>
    /// <remarks>This method disregards case of specified content expression name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static YahooMediaExpression ExpressionByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaExpression.None);

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
        return extension is YahooMediaSyndicationExtension;
    }

    /// <summary>
    /// Returns the content medium identifier for the supplied <see cref="YahooMediaMedium"/>.
    /// </summary>
    /// <param name="medium">The <see cref="YahooMediaMedium"/> to get the content medium identifier for.</param>
    /// <returns>The content medium identifier for the supplied <paramref name="medium"/>, Otherwise, returns an empty string.</returns>
    public static string MediumAsString(YahooMediaMedium medium) =>
        EnumerationMetadataAttribute.GetAlternateValue(medium);

    /// <summary>
    /// Returns the <see cref="YahooMediaMedium"/> enumeration value that corresponds to the specified content medium name.
    /// </summary>
    /// <param name="name">The name of the content medium.</param>
    /// <returns>A <see cref="YahooMediaMedium"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>YahooMediaMedium.None</b>.</returns>
    /// <remarks>This method disregards case of specified content medium name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static YahooMediaMedium MediumByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaMedium.None);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.</returns>
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
    public int CompareTo(YahooMediaSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = YahooMediaUtility.CompareSequence(this.Context.Contents, other.Context.Contents);
        if (result == 0) result = YahooMediaUtility.CompareSequence(this.Context.Groups, other.Context.Groups);

        if (result == 0) result = YahooMediaUtility.CompareCommonObjectEntities(this.Context, other.Context);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(YahooMediaSyndicationExtension? other)
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
        return obj is YahooMediaSyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Context));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaSyndicationExtension? first, YahooMediaSyndicationExtension? second)
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
    public static bool operator !=(YahooMediaSyndicationExtension? first, YahooMediaSyndicationExtension? second)
    {
        return !(first == second);
    }

}