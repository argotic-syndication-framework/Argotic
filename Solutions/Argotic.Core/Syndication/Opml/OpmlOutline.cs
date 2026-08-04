using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a discrete entity within an <see cref="OpmlDocument"/>.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the OpmlOutline class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Opml\OpmlOutlineExample.cs" 
///             region="OpmlOutline" 
///         />
///     </code>
/// </example>
[Serializable]
public class OpmlOutline : IComparable<OpmlOutline>, IEquatable<OpmlOutline>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOutline"/> class.
    /// </summary>
    public OpmlOutline()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOutline"/> class using the supplied text.
    /// </summary>
    /// <param name="text">The textual content of this outline.</param>
    /// <remarks>
    ///     Textual values <i>may</i> contain encoded HTML markup.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
    public OpmlOutline(string text)
    {
        this.Text = text;
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
    /// Gets a collection of key/value string pairs that represent custom attributes applied to this outline.
    /// </summary>
    /// <value>A <see cref="Dictionary{T, T}"/> of strings that represent custom attributes applied to this outline.</value>
    /// <remarks>
    ///     The attributes <b>text</b>, <b>type</b>, <b>isComment</b>, <b>isBreakpoint</b>, <b>created</b>, and <b>category</b> are treated as special
    ///     within the OPML specification. Use the class properties that represent these attributes instead of adding them to this collection.
    /// </remarks>
    public Dictionary<string, string> Attributes { get; } = [];

    /// <summary>
    /// Gets a collection that describes the categorization taxonomy applied to this outline.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> of strings that represent the categorization taxonomy applied to this outline.</value>
    /// <remarks>
    ///     Categories are represented as slash-delimited strings, in the format defined by the <a href="http://cyber.law.harvard.edu/rss/rss.html#ltcategorygtSubelementOfLtitemgt">RSS 2.0 category element</a>.
    ///     To represent a <i>tag</i>, the category string should contain <u>no</u> slashes.
    /// </remarks>
    public IList<string> Categories { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating how this outline's attributes should be interpreted.
    /// </summary>
    /// <value>A value indicating how this outline's attributes should be interpreted.</value>
    public string ContentType
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating when this outline was created.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates when this outline was created.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets a value indicating if a breakpoint is set on this outline.
    /// </summary>
    /// <value><b>true</b> if a breakpoint is set on this outline; Otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     This property is mainly necessary for outlines used to edit scripts. If it's not present, the value is <b>false</b>.
    /// </remarks>
    public bool HasBreakpoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this outline is commented.
    /// </summary>
    /// <value><b>true</b> if this outline is commented; Otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     By convention if an outline is commented, all subordinate outlines are considered to also be commented. If it's not present, the value is <b>false</b>.
    /// </remarks>
    public bool IsCommented { get; set; }

    /// <summary>
    /// Gets a value indicating if this outline represents an inclusion.
    /// </summary>
    /// <value><b>true</b> if the <see cref="ContentType"/> is <i>include</i> or <i>link</i>; Otherwise, <b>false</b>.</value>
    /// <seealso cref="OpmlOutline.CreateInclusionOutline(string, Uri)"/>
    public bool IsInclusionOutline =>
        string.Equals(this.ContentType, "include", StringComparison.OrdinalIgnoreCase) || string.Equals(this.ContentType, "link", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating if this outline represents a subscription list.
    /// </summary>
    /// <value><b>true</b> if the <see cref="ContentType"/> is <i>rss</i> or <i>feed</i>; Otherwise, <b>false</b>.</value>
    /// <seealso cref="OpmlOutline.CreateSubscriptionListOutline(string, string, Uri)"/>
    public bool IsSubscriptionListOutline =>
        string.Equals(this.ContentType, "rss", StringComparison.OrdinalIgnoreCase) || string.Equals(this.ContentType, "feed", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a collection of outlines that are children of this outline.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> of <see cref="OpmlOutline"/> objects that represent the children of this outline.</value>
#pragma warning disable CA5362 // OPML specification requires outlines to contain sub-outlines
    public IList<OpmlOutline> Outlines { get; } = [];
#pragma warning restore CA5362

    /// <summary>
    /// Gets or sets the textual content of this outline.
    /// </summary>
    /// <value>The textual content of this outline.</value>
    /// <remarks>
    ///     Textual values <i>may</i> contain encoded HTML markup.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Text
    {
        get => field;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            XPathNavigator attributesNavigator = source.CreateNavigator();
            if (attributesNavigator.MoveToFirstAttribute())
            {
                if (this.LoadAttribute(attributesNavigator))
                {
                    wasLoaded = true;
                }
                while (attributesNavigator.MoveToNextAttribute())
                {
                    if (this.LoadAttribute(attributesNavigator))
                    {
                        wasLoaded = true;
                    }
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNodeIterator outlinesIterator = source.Select("outline");
            if (outlinesIterator is { Count: > 0 })
            {
                while (outlinesIterator.MoveNext())
                {
                    XPathNavigator? outlinesNode = outlinesIterator.Current;
                    if (outlinesNode == null)
                    {
                        continue;
                    }

                    OpmlOutline outline = new();
                    if (outline.Load(outlinesNode))
                    {
                        this.Outlines.Add(outline);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            XPathNavigator attributesNavigator = source.CreateNavigator();
            if (attributesNavigator.MoveToFirstAttribute())
            {
                if (this.LoadAttribute(attributesNavigator))
                {
                    wasLoaded = true;
                }
                while (attributesNavigator.MoveToNextAttribute())
                {
                    if (this.LoadAttribute(attributesNavigator))
                    {
                        wasLoaded = true;
                    }
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNodeIterator outlinesIterator = source.Select("outline");
            if (outlinesIterator is { Count: > 0 })
            {
                while (outlinesIterator.MoveNext())
                {
                    XPathNavigator? outlinesNode = outlinesIterator.Current;
                    if (outlinesNode == null)
                    {
                        continue;
                    }

                    OpmlOutline outline = new();
                    if (outline.Load(outlinesNode, settings))
                    {
                        this.Outlines.Add(outline);
                        wasLoaded = true;
                    }
                }
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="OpmlOutline"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("outline");

        writer.WriteAttributeString("text", this.Text);

        if (!string.IsNullOrEmpty(this.ContentType))
        {
            writer.WriteAttributeString("type", this.ContentType);
        }

        if (this.IsCommented)
        {
            writer.WriteAttributeString("isComment", "true");
        }

        if (this.HasBreakpoint)
        {
            writer.WriteAttributeString("isBreakpoint", "true");
        }

        if (this.CreatedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("created", SyndicationDateTimeUtility.ToRfc822DateTime(this.CreatedOn));
        }

        if (this.Categories.Count > 0)
        {
            string[] categories = new string[this.Categories.Count];
            this.Categories.CopyTo(categories, 0);

            writer.WriteAttributeString("category", string.Join(",", categories));
        }

        if (this.Attributes.Count > 0)
        {
            foreach (string name in this.Attributes.Keys)
            {
                writer.WriteAttributeString(name, this.Attributes[name]);
            }
        }

        foreach (OpmlOutline outline in this.Outlines)
        {
            outline.WriteTo(writer);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Creates a new <see cref="OpmlOutline"/> that represents an inclusion outline using the supplied parameters.
    /// </summary>
    /// <param name="text">The textual content of the outline.</param>
    /// <param name="url">A <see cref="Uri"/> that represents an http address.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents an inclusion outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         When a outline is expanded in an outliner, if the <paramref name="url"/> ends with <i>.opml</i>, the outline expands in place. This is called <b>inclusion</b>. 
    ///     </para>
    ///     <para>
    ///         If the <paramref name="url"/> does not end with <i>.opml</i>, the link is assumed to point to something that can be displayed in a web browser.
    ///     </para>
    ///     <para>The difference between <b>link</b> and <b>include</b> is that <i>link</i> may point to something that is displayed in a web browser, and <i>include</i> always points to an OPML file.</para>
    ///     <para>
    ///         This method will create an <see cref="OpmlOutline"/> with a <see cref="ContentType"/> of <b>include</b> if the <paramref name="url"/> ends with <i>.opml</i>, 
    ///         Otherwise, the <see cref="ContentType"/> will have a value of <b>link</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference.</exception>
    public static OpmlOutline CreateInclusionOutline(string text, Uri url)
    {
        OpmlOutline outline = new();
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentNullException.ThrowIfNull(url);

        outline.Text = text;
        if (url.ToString().EndsWith(".opml", StringComparison.OrdinalIgnoreCase))
        {
            outline.ContentType = "include";
        }
        else
        {
            outline.ContentType = "link";
        }
        outline.Attributes.Add("url", url.ToString());

        return outline;
    }

    /// <summary>
    /// Creates a new <see cref="OpmlOutline"/> that represents a subscription list outline using the supplied parameters.
    /// </summary>
    /// <param name="text">The textual content of the outline.</param>
    /// <param name="type">The syndication format of the feed being pointed to. Permissible values include <i>rss</i> or <i>feed</i>.</param>
    /// <param name="xmlUrl">A <see cref="Uri"/> that represents the http address of the feed.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         A subscription list is a possibly multiple-level list of subscriptions to feeds. Each sub-element of the body of the OPML document 
    ///         is a node of type <i>rss</i> or an outline element that contains nodes of type <i>rss</i>.
    ///     </para>
    ///     <para>
    ///         Today, most subscription lists are a flat sequence of <i>rss</i> nodes, but some aggregators allow categorized subscription lists 
    ///         that are arbitrarily structured. A validator may flag these files, warning that some processors may not understand and preserve the structure.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is a null reference.</exception>
    public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl)
    {
        return OpmlOutline.CreateSubscriptionListOutline(text, type, xmlUrl, null, string.Empty, string.Empty, string.Empty, null);
    }

    /// <summary>
    /// Creates a new <see cref="OpmlOutline"/> that represents a subscription list outline using the supplied parameters.
    /// </summary>
    /// <param name="text">The textual content of the outline.</param>
    /// <param name="type">The syndication format of the feed being pointed to. Permissible values include <i>rss</i> or <i>feed</i>.</param>
    /// <param name="xmlUrl">A <see cref="Uri"/> that represents the http address of the feed.</param>
    /// <param name="htmlUrl">A <see cref="Uri"/> that represents the website that hosts the feed. This value can be <b>null</b>.</param>
    /// <param name="version">
    ///     The version of the syndication format for the feed that's being pointed to. 
    ///     Permissible values include <i>RSS</i>, <i>RSS1</i>, <i>scriptingNews</i>, or a custom version identifier for the feed. 
    ///     This value can be an empty string.
    /// </param>
    /// <param name="title">The title of the feed. This value can be an empty string.</param>
    /// <param name="description">The description of the feed. This value can be an empty string.</param>
    /// <param name="language">A <see cref="CultureInfo"/> that represents the natural or formal language in which the feed is written. This value can be <b>null</b>.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         A subscription list is a possibly multiple-level list of subscriptions to feeds. Each sub-element of the body of the OPML document 
    ///         is a node of type <i>rss</i> or an outline element that contains nodes of type <i>rss</i>.
    ///     </para>
    ///     <para>
    ///         Today, most subscription lists are a flat sequence of <i>rss</i> nodes, but some aggregators allow categorized subscription lists 
    ///         that are arbitrarily structured. A validator may flag these files, warning that some processors may not understand and preserve the structure.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is a null reference.</exception>
    public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl, Uri htmlUrl, string version, string title, string description, CultureInfo language)
    {
        OpmlOutline outline = new();
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentNullException.ThrowIfNull(xmlUrl);

        outline.Text = text;
        outline.ContentType = type;
        outline.Attributes.Add("xmlUrl", xmlUrl.ToString());

        if (htmlUrl != null)
        {
            outline.Attributes.Add("htmlUrl", htmlUrl.ToString());
        }

        if (!string.IsNullOrEmpty(version))
        {
            outline.Attributes.Add("version", version.Trim());
        }

        if (!string.IsNullOrEmpty(title))
        {
            outline.Attributes.Add("title", title.Trim());
        }

        if (!string.IsNullOrEmpty(description))
        {
            outline.Attributes.Add("description", description.Trim());
        }

        if (language != null)
        {
            outline.Attributes.Add("language", language.Name);
        }

        return outline;
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="OpmlOutline"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlOutline"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(OpmlOutline? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.CreatedOn.CompareTo(other.CreatedOn);
        if (result == 0) result = this.HasBreakpoint.CompareTo(other.HasBreakpoint);
        if (result == 0) result = this.IsCommented.CompareTo(other.IsCommented);
        if (result == 0) result = string.Compare(this.Text, other.Text, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = ComparisonUtility.CompareSequence(this.Attributes, other.Attributes, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Outlines, other.Outlines);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="OpmlOutline"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="OpmlOutline"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="OpmlOutline"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(OpmlOutline? other)
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
        return obj is OpmlOutline other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.ContentType), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.HasBreakpoint), HashCodeUtility.Component(this.IsCommented), HashCodeUtility.Component(this.Text));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(OpmlOutline? first, OpmlOutline? second)
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
    public static bool operator !=(OpmlOutline? first, OpmlOutline? second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Loads this <see cref="OpmlOutline"/> using attributes defined on the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="attribute"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="attribute"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/> attribute.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="attribute"/> is a null reference.</exception>
    private bool LoadAttribute(XPathNavigator attribute)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(attribute);

        if (string.IsNullOrEmpty(attribute.Value))
        {
            return false;
        }

        if (string.Equals(attribute.Name, "text", StringComparison.OrdinalIgnoreCase))
        {
            this.Text = attribute.Value;
            wasLoaded = true;
        }
        else if (string.Equals(attribute.Name, "type", StringComparison.OrdinalIgnoreCase))
        {
            this.ContentType = attribute.Value;
            wasLoaded = true;
        }
        else if (string.Equals(attribute.Name, "isComment", StringComparison.OrdinalIgnoreCase))
        {
            if (bool.TryParse(attribute.Value, out bool isComment))
            {
                this.IsCommented = isComment;
                wasLoaded = true;
            }
        }
        else if (string.Equals(attribute.Name, "isBreakpoint", StringComparison.OrdinalIgnoreCase))
        {
            if (bool.TryParse(attribute.Value, out bool isBreakpoint))
            {
                this.HasBreakpoint = isBreakpoint;
                wasLoaded = true;
            }
        }
        else if (string.Equals(attribute.Name, "created", StringComparison.OrdinalIgnoreCase))
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(attribute.Value, out DateTime created))
            {
                this.CreatedOn = created;
                wasLoaded = true;
            }
        }
        else if (string.Equals(attribute.Name, "category", StringComparison.OrdinalIgnoreCase))
        {
            if (attribute.Value.Contains(',', StringComparison.Ordinal))
            {
                string[] categories = attribute.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string category in categories)
                {
                    this.Categories.Add(category);
                }
            }
            else
            {
                this.Categories.Add(attribute.Value);
            }
            wasLoaded = true;
        }
        else
        {
            if (!this.Attributes.ContainsKey(attribute.Name))
            {
                this.Attributes.Add(attribute.Name, attribute.Value);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }
}