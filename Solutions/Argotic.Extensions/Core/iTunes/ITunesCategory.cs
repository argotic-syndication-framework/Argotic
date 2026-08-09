using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents one entry from the taxonomy Apple categorises podcasts by.
/// </summary>
/// <remarks>
///     <para>
///     <see cref="Text"/> is not free prose. Apple publishes a closed list of categories and
///     subcategories, and a show that spells one of them differently is categorised as nothing at all
///     — Apple's own example of the trap is casing: <c>&lt;itunes:category text="Kids &amp;amp;
///     Family" /&gt;</c> is correct and <c>"Kids &amp;amp; family"</c> is not.
///     </para>
///     <para>
///     A subcategory is a nested <c>itunes:category</c>, not an attribute and not a sibling, which is
///     why this type contains a collection of itself. Apple's guidance is that <i>"You can choose up
///     to two categories per show — primary and secondary — plus subcategories for each, if
///     available."</i> Nothing here enforces either the taxonomy or that limit.
///     </para>
/// </remarks>
/// <seealso cref="ITunesSyndicationExtensionContext.Categories"/>
public class ITunesCategory : IComparable<ITunesCategory>, IEquatable<ITunesCategory>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesCategory"/> class.
    /// </summary>
    public ITunesCategory()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesCategory"/> class using the supplied text.
    /// </summary>
    /// <param name="text">The category name, spelled exactly as Apple's taxonomy spells it.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public ITunesCategory(string text)
    {
        this.Text = text;
    }

    /// <summary>
    /// Gets the sub-categories of this category.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="ITunesCategory"/> objects that represent the sub-categories of this category. The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Apple's taxonomy nests one level: a category has subcategories, a subcategory has none.
    ///     Both the loader and the writer recurse without limit, so a deeper tree round-trips rather
    ///     than being truncated — it is simply not something Apple will read.
    /// </remarks>
#pragma warning disable CA5362 // iTunes specification requires categories to contain subcategories
    public IList<ITunesCategory> Categories { get; } = [];
#pragma warning restore CA5362

    /// <summary>
    /// Gets or sets the name of this category.
    /// </summary>
    /// <value>The category name, matched against Apple's taxonomy character for character. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Several category names contain an ampersand — <c>Kids &amp; Family</c>,
    ///     <c>Health &amp; Fitness</c>, <c>Society &amp; Culture</c>. Set this property to the
    ///     <i>decoded</i> text, with a literal <c>&amp;</c>; the writer escapes it. Storing the
    ///     already-escaped form produces <c>&amp;amp;amp;</c> in the feed and a category that matches
    ///     nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Text
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="ITunesCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="ITunesCategory"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ITunesSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasAttributes)
        {
            string textAttribute = source.GetAttribute("text", string.Empty);
            if (!string.IsNullOrEmpty(textAttribute))
            {
                this.Text = textAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNodeIterator categoryIterator = source.SelectChildElements("itunes", "category", manager);

            if (categoryIterator is { Count: > 0 })
            {
                while (categoryIterator.MoveNext())
                {
                    XPathNavigator? categoryNode = categoryIterator.Current;
                    if (categoryNode is null)
                    {
                        continue;
                    }

                    ITunesCategory category = new();
                    if (category.Load(categoryNode))
                    {
                        this.Categories.Add(category);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ITunesCategory"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ITunesSyndicationExtension extension = new();
        writer.WriteStartElement("category", extension.XmlNamespace);

        writer.WriteAttributeString("text", this.Text);

        if (this.Categories.Count > 0)
        {
            foreach (ITunesCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ITunesCategory"/>.
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
    public int CompareTo(ITunesCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Text, other.Text, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ITunesCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ITunesCategory"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="ITunesCategory"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ITunesCategory? other)
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
    public override bool Equals(object? obj) => obj is ITunesCategory other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Text), HashCodeUtility.Component(this.Categories.Count));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(ITunesCategory? first, ITunesCategory? second)
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
    public static bool operator !=(ITunesCategory? first, ITunesCategory? second) => !(first == second);

}