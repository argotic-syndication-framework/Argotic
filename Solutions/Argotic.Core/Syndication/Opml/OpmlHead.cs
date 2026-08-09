using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents the header information for an <see cref="OpmlDocument"/>.
/// </summary>
/// <remarks>
///     Every sub-element of an OPML <c>head</c> is optional and may appear at most once, so nothing here is
///     safe to assume present. Several of them — <see cref="Window"/>, <see cref="VerticalScrollState"/>,
///     <see cref="ExpansionState"/> — describe how an outliner was displaying the file when it saved it, and
///     are inert for anything reading the document as data.
/// </remarks>
/// <seealso cref="OpmlDocument.Head"/>
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
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets a date-time indicating when this document was created.
    /// </summary>
    /// <value>The <c>dateCreated</c> element. The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.</value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time. OPML pins its date-times to
    ///     <a href="https://www.rfc-editor.org/rfc/rfc822.html">RFC 822</a> — Internet Standard STD 11, the same
    ///     one RSS 2.0 uses, and not the later RFC 5322, which forbids the two-digit years OPML permits — and
    ///     the RFC 822 form this library writes ends in a literal <c>GMT</c> that it does not convert to. A
    ///     local-time <see cref="DateTime"/> is therefore republished as though it were UTC, silently and by
    ///     exactly the machine's offset.
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the http address of the documentation that this OPML document conforms to.
    /// </summary>
    /// <value>Always <c>https://opml.org/spec2.opml</c>. This is a constant, not a parsed value: it is not settable and a <c>docs</c> element in a loaded document does not change it.</value>
    /// <remarks>
    ///     <para>
    ///     OPML's <c>docs</c> element exists for the reader who finds the file on a web server years later and
    ///     wants to know what it is. Because this implementation is always OPML 2.0, the answer is always the
    ///     same.
    ///     </para>
    ///     <para>
    ///     It was <c>http://www.opml.org/spec2</c>, which answers 404 — so every OPML document this
    ///     framework has ever saved carries a dead link in the one element whose only job is to be
    ///     followed. <c>opml.org</c> serves the OPML 2.0 specification at <c>spec2.opml</c>, as
    ///     <c>text/html</c>; <c>spec2.html</c> redirects there, so the redirect target is cited directly.
    ///     </para>
    /// </remarks>
    public Uri Documentation { get; } = new("https://opml.org/spec2.opml");

    /// <summary>
    /// Gets a collection of line numbers that are expanded within the outline.
    /// </summary>
    /// <remarks>
    ///     Order is significant, and the numbers are relative, not absolute. Starting at the first top-level
    ///     outline, navigate flat down by the first number and expand; from there navigate down by the second
    ///     and expand; and so on. Reordering the collection changes which nodes it names.
    /// </remarks>
    public IList<int> ExpansionState => field ??= [];

    /// <summary>
    /// Gets or sets a date-time indicating when this document was last modified.
    /// </summary>
    /// <value>The <c>dateModified</c> element. The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date was provided.</value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; the RFC 822 caveat on <see cref="CreatedOn"/> applies
    ///     here too.
    /// </remarks>
    public DateTime ModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets information that describes the owner of this document.
    /// </summary>
    /// <value>The owner, or <see langword="null"/> if the document names none.</value>
    public OpmlOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets the title of this document.
    /// </summary>
    /// <value>The <c>title</c> element, or an <i>empty</i> string if none was specified. The value is trimmed on assignment.</value>
    public string Title
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a number indicating which line of this outline is displayed on the top line of the window.
    /// </summary>
    /// <value>
    ///     A line number, counted with <see cref="ExpansionState"/> already applied.
    ///     The default value is <see cref="Int32.MinValue"/>, which indicates that no vertical scroll state was provided.
    /// </value>
    public int VerticalScrollState { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets information that describes the pixel location of the edges of the outline window for this document.
    /// </summary>
    /// <value>The window geometry, or <see langword="null"/> if the document records none.</value>
    public OpmlWindow? Window { get; set; }

    /// <summary>
    /// Loads this <see cref="OpmlHead"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <returns><see langword="true"/> if the <see cref="OpmlHead"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    /// <remarks>
    ///     <para>
    ///         The comparison folds the same six members <see cref="GetHashCode"/> does, in this order:
    ///         <see cref="Title"/> (case-insensitively, as it is hashed), <see cref="CreatedOn"/>,
    ///         <see cref="ModifiedOn"/>, <see cref="VerticalScrollState"/>, <see cref="Owner"/> and
    ///         <see cref="Window"/>. The last two delegate to their own comparisons, and a
    ///         <see langword="null"/> sorts below a present one.
    ///     </para>
    ///     <para>
    ///         <see cref="ExpansionState"/> is deliberately excluded from both, and the two must stay in
    ///         step. It is a collection, and folding one into <see cref="HashCode.Combine{T1, T2, T3, T4, T5, T6}"/>
    ///         by its instance rather than its elements is the defect that had to be removed from seven
    ///         types; comparing it without hashing it the same way would put this type straight back into
    ///         the state this method was written to leave — equality and hashing disagreeing.
    ///     </para>
    /// </remarks>
    public int CompareTo(OpmlHead? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.CreatedOn.CompareTo(other.CreatedOn);
        if (result == 0) result = this.ModifiedOn.CompareTo(other.ModifiedOn);
        if (result == 0) result = this.VerticalScrollState.CompareTo(other.VerticalScrollState);
        if (result == 0)
        {
            result = (this.Owner, other.Owner) switch
            {
                (null, null) => 0,
                (not null, null) => 1,
                (null, not null) => -1,
                var (mine, theirs) => mine.CompareTo(theirs),
            };
        }

        if (result == 0)
        {
            result = (this.Window, other.Window) switch
            {
                (null, null) => 0,
                (not null, null) => 1,
                (null, not null) => -1,
                var (mine, theirs) => mine.CompareTo(theirs),
            };
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="OpmlHead"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="OpmlHead"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="OpmlHead"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(OpmlHead? first, OpmlHead? second) => !(first == second);
}