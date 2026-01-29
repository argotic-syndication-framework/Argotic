using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a categorization taxonomy that can be applied to a podcast.
/// </summary>
/// <seealso cref="ITunesSyndicationExtensionContext"/>
[Serializable]
public class ITunesCategory : IComparable<ITunesCategory>, IEquatable<ITunesCategory>, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the name of the category.
    /// </summary>
    private string categoryText = string.Empty;
    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesCategory"/> class.
    /// </summary>
    public ITunesCategory()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesCategory"/> class using the supplied text.
    /// </summary>
    /// <param name="text"></param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
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
#pragma warning disable CA5362 // iTunes specification requires categories to contain subcategories
    public IList<ITunesCategory> Categories { get; } = [];
#pragma warning restore CA5362

    /// <summary>
    /// Gets or sets the name of this category.
    /// </summary>
    /// <value>The name of this category.</value>
    /// <remarks>
    ///     The category text <i>may</i> be entity encoded.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Text
    {
        get
        {
            return categoryText;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            categoryText = value.Trim();
        }
    }

    /// <summary>
    /// Loads this <see cref="ITunesCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ITunesCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ITunesCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
            XPathNodeIterator categoryIterator = source.Select("itunes:category", manager);

            if (categoryIterator is { Count: > 0 })
            {
                while (categoryIterator.MoveNext())
                {
                    ITunesCategory category = new();
                    if (category.Load(categoryIterator.Current))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// Returns a <see cref="String"/> that represents the current <see cref="ITunesCategory"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="ITunesCategory"/>.</returns>
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
    public int CompareTo(ITunesCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Text, other.Text, StringComparison.OrdinalIgnoreCase);
        result |= ComparisonUtility.CompareSequence(this.Categories, other.Categories);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ITunesCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ITunesCategory"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ITunesCategory"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ITunesCategory other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Text, this.Categories.Count);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ITunesCategory first, ITunesCategory second)
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
    public static bool operator !=(ITunesCategory first, ITunesCategory second)
    {
        return !(first == second);
    }

}