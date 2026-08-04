using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents the header information for an <see cref="OpmlDocument"/>.
/// </summary>
[Serializable]
public class OpmlHead : IComparable<OpmlHead>, IEquatable<OpmlHead>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlHead"/> class.
    /// </summary>
    public OpmlHead()
    {

    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets a date-time indicating when this document was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> object that indicates when this document was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.</value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the http address of the documentation that this OPML document conforms to.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the http address of the documentation that this OPML document conforms to.
    /// </value>
    public Uri Documentation { get; } = new("http://www.opml.org/spec2");

    /// <summary>
    /// Gets a collection of line numbers that are expanded within the outline.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> of integers that represent the line numbers that are expanded within the outline.</value>
    /// <remarks>
    ///     The line numbers in the collection tell you which headlines to expand. The order is important. 
    ///     For each element in the collection, X, starting at the first summit, navigate flat down X times and expand. Repeat for each element in the collection.
    /// </remarks>
    public IList<int> ExpansionState => field ??= [];

    /// <summary>
    /// Gets or sets a date-time indicating when this document was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> object that indicates when this document was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date was provided.</value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime ModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets information that describes the owner of this document.
    /// </summary>
    /// <value>A <see cref="OpmlOwner"/> object that provides information that describes the owner of this document.</value>
    public OpmlOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets the title of this document.
    /// </summary>
    /// <value>The title of this document.</value>
    public string Title
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a number indicating which line of this outline is displayed on the top line of the window.
    /// </summary>
    /// <value>
    ///     An integer that indicates which line of this outline is displayed on the top line of the window.
    ///     The default value is <see cref="Int32.MinValue"/>, which indicates that no vertical scroll state was provided.
    /// </value>
    public int VerticalScrollState { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets information that describes the pixel location of the edges of the outline window for this document.
    /// </summary>
    /// <value>A <see cref="OpmlWindow"/> object that provides information that describes the pixel location of the edges of the outline window for this document.</value>
    public OpmlWindow? Window { get; set; }

    /// <summary>
    /// Loads this <see cref="OpmlHead"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator? titleNavigator = source.SelectChildElement("title");
        XPathNavigator? dateCreatedNavigator = source.SelectChildElement("dateCreated");
        XPathNavigator? dateModifiedNavigator = source.SelectChildElement("dateModified");
        XPathNavigator? expansionStateNavigator = source.SelectChildElement("expansionState");
        XPathNavigator? verticalScrollStateNavigator = source.SelectChildElement("vertScrollState");

        if (titleNavigator is not null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (dateCreatedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(dateCreatedNavigator.Value, out DateTime createdOn))
            {
                this.CreatedOn = createdOn;
                wasLoaded = true;
            }
        }

        if (dateModifiedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(dateModifiedNavigator.Value, out DateTime modifiedOn))
            {
                this.ModifiedOn = modifiedOn;
                wasLoaded = true;
            }
        }

        OpmlOwner owner = new();
        if (owner.Load(source))
        {
            this.Owner = owner;
        }

        if (expansionStateNavigator is not null && !string.IsNullOrEmpty(expansionStateNavigator.Value))
        {
            if (expansionStateNavigator.Value.Contains(',', StringComparison.Ordinal))
            {
                string[] expansionStates = expansionStateNavigator.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string expansionState in expansionStates)
                {
                    if (int.TryParse(expansionState.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int state))
                    {
                        this.ExpansionState.Add(state);
                        wasLoaded = true;
                    }
                }
            }
            else
            {
                if (int.TryParse(expansionStateNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int expansionState))
                {
                    this.ExpansionState.Add(expansionState);
                    wasLoaded = true;
                }
            }
        }

        if (verticalScrollStateNavigator is not null)
        {
            if (int.TryParse(verticalScrollStateNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int verticalScrollState))
            {
                this.VerticalScrollState = verticalScrollState;
                wasLoaded = true;
            }
        }

        OpmlWindow window = new();
        if (window.Load(source))
        {
            this.Window = window;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="OpmlHead"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="OpmlHead"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("head");

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("title", this.Title);
        }

        if (this.CreatedOn != DateTime.MinValue)
        {
            writer.WriteElementString("dateCreated", SyndicationDateTimeUtility.ToRfc822DateTime(this.CreatedOn));
        }

        if (this.ModifiedOn != DateTime.MinValue)
        {
            writer.WriteElementString("dateModified", SyndicationDateTimeUtility.ToRfc822DateTime(this.ModifiedOn));
        }

        this.Owner?.WriteTo(writer);

        if (this.Documentation is not null)
        {
            writer.WriteElementString("docs", this.Documentation.ToString());
        }

        if (this.ExpansionState.Count > 0)
        {
            // The invariant formatting is kept deliberately: string.Join's IEnumerable<int> overload
            // would format through the current culture, which changes the negative sign in some.
            writer.WriteElementString(
                "expansionState",
                string.Join(",", this.ExpansionState.Select(state => state.ToString(System.Globalization.NumberFormatInfo.InvariantInfo))));
        }

        if (this.VerticalScrollState != int.MinValue)
        {
            writer.WriteElementString("vertScrollState", this.VerticalScrollState.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        this.Window?.WriteTo(writer);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="OpmlHead"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlHead"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(OpmlHead? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = 0; //String.Compare(this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="OpmlHead"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="OpmlHead"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="OpmlHead"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(OpmlHead? other)
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
    public override bool Equals(object? obj) => obj is OpmlHead other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.ModifiedOn), HashCodeUtility.Component(this.VerticalScrollState), HashCodeUtility.Component(this.Owner), HashCodeUtility.Component(this.Window));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(OpmlHead? first, OpmlHead? second)
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
    public static bool operator !=(OpmlHead? first, OpmlHead? second) => !(first == second);
}