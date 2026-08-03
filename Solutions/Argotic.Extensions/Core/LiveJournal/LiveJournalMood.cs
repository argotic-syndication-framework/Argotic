using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the current mood of a LiveJournal entry.
/// </summary>
/// <seealso cref="LiveJournalSyndicationExtensionContext.Mood"/>
[Serializable]
public class LiveJournalMood : IComparable<LiveJournalMood>, IEquatable<LiveJournalMood>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalMood"/> class.
    /// </summary>
    public LiveJournalMood()
    {
    }

    /// <summary>
    /// Gets or sets the textual content that describes this mood.
    /// </summary>
    /// <value>The textual or entity encoded content that describes this mood.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Content
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a site specific identifier for this mood.
    /// </summary>
    /// <value>A site specific identifier for this mood. The default value is <see cref="Int32.MinValue"/>, which indicates no identifier was specified.</value>
    public int Id { get; set; } = int.MinValue;

    /// <summary>
    /// Loads this <see cref="LiveJournalMood"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="LiveJournalMood"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalMood"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string idAttribute = source.GetAttribute("id", string.Empty);
            if (!string.IsNullOrEmpty(idAttribute))
            {
                if (int.TryParse(idAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int id))
                {
                    this.Id = id;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="LiveJournalMood"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        LiveJournalSyndicationExtension extension = new();
        writer.WriteStartElement("mood", extension.XmlNamespace);

        if (this.Id != int.MinValue)
        {
            writer.WriteAttributeString("id", this.Id.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        writer.WriteCData(this.Content);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalMood"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="LiveJournalMood"/>.</returns>
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
    public int CompareTo(LiveJournalMood? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Id.CompareTo(other.Id);
        if (result == 0) result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="LiveJournalMood"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="LiveJournalMood"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="LiveJournalMood"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(LiveJournalMood? other)
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
        return obj is LiveJournalMood other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.Content));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(LiveJournalMood first, LiveJournalMood second)
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
    public static bool operator !=(LiveJournalMood first, LiveJournalMood second)
    {
        return !(first == second);
    }

}