using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a discrete entity within an <see cref="OpmlDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///         An outline is a tree node carrying a set of named string attributes, and it is very nearly the
///         whole of OPML's data model: an <c>outline</c> may nest <c>outline</c> children to any depth, and
///         the specification documents no limit on the depth, the number of children, or the number of
///         attributes. Only <see cref="Text"/> is required — OPML 2.0 calls a missing <c>text</c> attribute
///         an error, because an outliner that opens the file has nothing to display without it.
///     </para>
///     <para>
///         Everything else is untyped. <see cref="ContentType"/> — the <c>type</c> attribute — says how the
///         remaining attributes are to be read, and anything this library does not model as a property is
///         preserved verbatim in <see cref="Attributes"/> rather than dropped. That is deliberate: the
///         specification tells processors to ignore attributes they do not understand, and new <c>type</c>
///         values are the format's sanctioned extension point.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Opml\OpmlOutlineExample.cs" language="cs" title="The following code example demonstrates the usage of the OpmlOutline class." />
/// </example>
public class OpmlOutline : IComparable<OpmlOutline>, IEquatable<OpmlOutline>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// The greatest number of nested <c>outline</c> levels a load will descend through.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         OPML documents no limit on nesting depth, but reading one costs a stack frame per level, so
    ///         "no limit" in the format cannot mean "no limit" in a parser. <c>Load</c> is self-recursive and
    ///         not tail-recursive — the call sits inside the child iterator's loop with work after it — and
    ///         it is reachable from <see cref="OpmlDocument.Load(Stream)"/>, whose content allowance is
    ///         8 MiB. An <c>&lt;outline&gt;</c> start tag costs nine bytes, so that allowance pays for
    ///         hundreds of thousands of levels: far more than any stack absorbs, and the resulting overflow
    ///         terminates the process rather than raising something a caller could catch.
    ///     </para>
    ///     <para>
    ///         256 is chosen to sit far above any real subscription list or outline document — a hand-built
    ///         outline nests single digits deep, and a generated one rarely reaches double — while keeping
    ///         the worst-case recursion to a few tens of kilobytes of stack. Exceeding it skips the outline
    ///         rather than raising, which is how every other unusable value on this path is handled: an
    ///         attribute that fails to parse is skipped too, and a malformed depth should not draw a louder
    ///         failure than a malformed integer.
    ///     </para>
    /// </remarks>
    public const int MaxOutlineNestingDepth = 256;

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
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public OpmlOutline(string text)
    {
        this.Text = text;
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
    /// Gets a collection of key/value string pairs that represent custom attributes applied to this outline.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every attribute the parser does not recognise lands here, keyed by its unprefixed local name.
    ///         The six OPML 2.0 gives a defined meaning to — <c>text</c>, <c>type</c>, <c>isComment</c>,
    ///         <c>isBreakpoint</c>, <c>created</c> and <c>category</c> — are modelled as properties instead and
    ///         are never put here by the parser. Do not add them yourself either: the writer emits the
    ///         properties first and this collection afterwards, so the attribute would be written twice, and
    ///         OPML forbids an outline from repeating an attribute.
    ///     </para>
    ///     <para>
    ///         The attributes that carry the payload of a real outline mostly live here, because the
    ///         specification defines them per <c>type</c> rather than globally: <c>xmlUrl</c>, <c>htmlUrl</c>,
    ///         <c>description</c>, <c>language</c>, <c>title</c> and <c>version</c> for a subscription list,
    ///         and <c>url</c> for a <c>link</c> or <c>include</c> outline.
    ///     </para>
    /// </remarks>
    public Dictionary<string, string> Attributes { get; } = [];

    /// <summary>
    /// Gets a collection that describes the categorization taxonomy applied to this outline.
    /// </summary>
    /// <remarks>
    ///     One OPML <c>category</c> attribute holds a comma-separated list, so this is a collection of one
    ///     attribute's worth of values rather than of attributes. Each entry is a slash-delimited path in the
    ///     format defined by the <a href="https://cyber.harvard.edu/rss/rss.html#ltcategorygtSubelementOfLtitemgt">RSS 2.0 category element</a> —
    ///     <c>/Harvard/Berkman</c>. An entry containing no slash is a plain tag.
    /// </remarks>
    public IList<string> Categories { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating how this outline's attributes should be interpreted.
    /// </summary>
    /// <value>The <c>type</c> attribute, or an <i>empty</i> string if the outline declares none.</value>
    /// <remarks>
    ///     OPML 2.0 defines three values of its own — <c>rss</c> for a subscription-list entry, and <c>link</c>
    ///     and <c>include</c> for the two flavours of inclusion — and expects everything else to come from
    ///     extensions, which is why this is a free string rather than an enumeration. Comparison is
    ///     case-insensitive by specification: <c>type="LINK"</c> means what <c>type="link"</c> means, and
    ///     <see cref="IsInclusionOutline"/> and <see cref="IsSubscriptionListOutline"/> compare accordingly.
    /// </remarks>
    public string ContentType
    {
        get;

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
    ///     Supply this in Coordinated Universal Time. OPML pins its date-times to
    ///     <a href="https://www.rfc-editor.org/rfc/rfc822.html">RFC 822</a> — Internet Standard STD 11, the same
    ///     one RSS 2.0 uses, and not the later RFC 5322, which forbids the two-digit years OPML permits — and
    ///     the RFC 822 form this library writes ends in a literal <c>GMT</c> that it does not convert to. A
    ///     local-time <see cref="DateTime"/> is therefore republished as though it were UTC, silently and by
    ///     exactly the machine's offset.
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets a value indicating if a breakpoint is set on this outline.
    /// </summary>
    /// <value><see langword="true"/> if a breakpoint is set on this outline; otherwise, <see langword="false"/>. An absent <c>isBreakpoint</c> attribute means <see langword="false"/>.</value>
    /// <remarks>
    ///     This matters only for outlines used to edit scripts, which is what OPML was originally the file
    ///     format for.
    /// </remarks>
    public bool HasBreakpoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this outline is commented.
    /// </summary>
    /// <value><see langword="true"/> if this outline is commented; otherwise, <see langword="false"/>. An absent <c>isComment</c> attribute means <see langword="false"/>.</value>
    /// <remarks>
    ///     By convention a commented outline comments out everything beneath it as well. The flag is not
    ///     propagated to the children in <see cref="Outlines"/>, so a consumer deciding what to display has to
    ///     carry it down the tree itself.
    /// </remarks>
    public bool IsCommented { get; set; }

    /// <summary>
    /// Gets a value indicating if this outline represents an inclusion.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="ContentType"/> is <c>include</c> or <c>link</c>, compared without regard to case; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     Such an outline must carry a <c>url</c> attribute, which is reached through
    ///     <see cref="Attributes"/>; this property does not check that it is there.
    /// </remarks>
    /// <seealso cref="OpmlOutline.CreateInclusionOutline(string, Uri)"/>
    public bool IsInclusionOutline =>
        string.Equals(this.ContentType, "include", StringComparison.OrdinalIgnoreCase) || string.Equals(this.ContentType, "link", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating if this outline represents a subscription list.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="ContentType"/> is <c>rss</c> or <c>feed</c>, compared without regard to case; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     OPML 2.0 names only <c>rss</c>; <c>feed</c> is accepted here as well because aggregators use it for
    ///     Atom subscriptions. A subscription-list outline must carry <c>xmlUrl</c>, the address of the feed
    ///     itself, which is reached through <see cref="Attributes"/> — this property does not check that it is
    ///     there, and an outline claiming the type without the address is the common malformation.
    /// </remarks>
    /// <seealso cref="OpmlOutline.CreateSubscriptionListOutline(string, string, Uri)"/>
    public bool IsSubscriptionListOutline =>
        string.Equals(this.ContentType, "rss", StringComparison.OrdinalIgnoreCase) || string.Equals(this.ContentType, "feed", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a collection of outlines that are children of this outline.
    /// </summary>
    /// <remarks>
    ///     Nesting is unbounded and the recursion is genuine — an outline may hold outlines that hold
    ///     outlines. Nothing here guards against a cycle assembled in memory, and
    ///     <see cref="WriteTo(XmlWriter)"/> walks children unconditionally, so one would not terminate.
    /// </remarks>
#pragma warning disable CA5362 // OPML specification requires outlines to contain sub-outlines
    public IList<OpmlOutline> Outlines { get; } = [];
#pragma warning restore CA5362

    /// <summary>
    /// Gets or sets the textual content of this outline.
    /// </summary>
    /// <value>The <c>text</c> attribute — what an outliner displays for this node. The value is trimmed on assignment.</value>
    /// <remarks>
    ///     The one attribute OPML 2.0 requires of every outline, which is why the setter rejects null and
    ///     empty rather than storing them. The value may contain encoded HTML markup; the remaining outline
    ///     attributes may not, unless their own definition says so.
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
    /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source) => this.Load(source, 0);

    /// <summary>
    /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/>, refusing to
    /// descend past <see cref="MaxOutlineNestingDepth"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="depth">The nesting depth of this outline, counting the outermost one as zero.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    internal bool Load(XPathNavigator source, int depth)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (depth >= MaxOutlineNestingDepth)
        {
            return false;
        }

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
                    if (outlinesNode is null)
                    {
                        continue;
                    }

                    OpmlOutline outline = new();
                    if (outline.Load(outlinesNode, depth + 1))
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
    /// <returns><see langword="true"/> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings) => this.Load(source, settings, 0);

    /// <summary>
    /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/> and
    /// <see cref="SyndicationResourceLoadSettings"/>, refusing to descend past
    /// <see cref="MaxOutlineNestingDepth"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <param name="depth">The nesting depth of this outline, counting the outermost one as zero.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    internal bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings, int depth)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (depth >= MaxOutlineNestingDepth)
        {
            return false;
        }

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
                    if (outlinesNode is null)
                    {
                        continue;
                    }

                    OpmlOutline outline = new();
                    if (outline.Load(outlinesNode, settings, depth + 1))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
            writer.WriteAttributeString("category", string.Join(",", this.Categories));
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
    /// <param name="url">The http address to point at. It is stored under the <c>url</c> attribute the specification requires of both inclusion types.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents an inclusion outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         The two inclusion types differ in what they may point at: <c>include</c> always points to an
    ///         OPML file, which an outliner expands in place, and <c>link</c> may point to anything a web
    ///         browser can display.
    ///     </para>
    ///     <para>
    ///         The type is chosen here from the extension alone: a <paramref name="url"/> ending in
    ///         <c>.opml</c> produces <c>include</c>, anything else produces <c>link</c>. A URL that serves OPML
    ///         from a path with no extension — a query-string endpoint, say — is therefore typed <c>link</c>,
    ///         and expands only if the outliner sniffs the content. Set <see cref="ContentType"/> yourself when
    ///         that matters.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
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
    /// <param name="type">The syndication format of the feed being pointed to. OPML 2.0 defines only <c>rss</c>; <c>feed</c> is the convention aggregators use for Atom.</param>
    /// <param name="xmlUrl">The http address of the feed itself. It is stored under the <c>xmlUrl</c> attribute, which the specification requires of a subscription-list outline.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         A subscription list is the export format every feed reader speaks: each child of the document
    ///         body is either an <c>rss</c> outline or an outline that groups them. Grouping is legal but not
    ///         universally honoured — most lists in the wild are flat, and a reader that only understands the
    ///         flat shape will drop the categories rather than reject the file.
    ///     </para>
    ///     <para>
    ///         The optional attributes — <c>description</c>, <c>htmlUrl</c>, <c>language</c>, <c>title</c> and
    ///         <c>version</c> — are all copies of what the feed itself says, kept so a list can be shown to a
    ///         user without fetching every feed in it. They go stale; <c>xmlUrl</c> is the only one worth
    ///         trusting.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is <see langword="null"/>.</exception>
    public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl) => OpmlOutline.CreateSubscriptionListOutline(text, type, xmlUrl, null, string.Empty, string.Empty, string.Empty, null);

    /// <summary>
    /// Creates a new <see cref="OpmlOutline"/> that represents a subscription list outline using the supplied parameters.
    /// </summary>
    /// <param name="text">The textual content of the outline.</param>
    /// <param name="type">The syndication format of the feed being pointed to. OPML 2.0 defines only <c>rss</c>; <c>feed</c> is the convention aggregators use for Atom.</param>
    /// <param name="xmlUrl">The http address of the feed itself. It is stored under the <c>xmlUrl</c> attribute, which the specification requires of a subscription-list outline.</param>
    /// <param name="htmlUrl">A <see cref="Uri"/> that represents the website that hosts the feed. This value can be <see langword="null"/>.</param>
    /// <param name="version">
    ///     The version of the syndication format the feed uses. OPML 2.0 names three values: <c>RSS1</c> for
    ///     RSS 1.0, <c>RSS</c> for 0.91, 0.92 or 2.0 alike, and <c>scriptingNews</c>. It defines none for Atom.
    ///     This value can be an empty string, and usually should be — the attribute was invented for
    ///     processors that handled only certain versions, which turned out not to exist.
    /// </param>
    /// <param name="title">The title of the feed. This value can be an empty string.</param>
    /// <param name="description">The description of the feed. This value can be an empty string.</param>
    /// <param name="language">A <see cref="CultureInfo"/> that represents the natural or formal language in which the feed is written. This value can be <see langword="null"/>.</param>
    /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
    /// <remarks>
    ///     <para>
    ///         A subscription list is the export format every feed reader speaks: each child of the document
    ///         body is either an <c>rss</c> outline or an outline that groups them. Grouping is legal but not
    ///         universally honoured — most lists in the wild are flat, and a reader that only understands the
    ///         flat shape will drop the categories rather than reject the file.
    ///     </para>
    ///     <para>
    ///         The optional attributes — <c>description</c>, <c>htmlUrl</c>, <c>language</c>, <c>title</c> and
    ///         <c>version</c> — are all copies of what the feed itself says, kept so a list can be shown to a
    ///         user without fetching every feed in it. They go stale; <c>xmlUrl</c> is the only one worth
    ///         trusting.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is <see langword="null"/>.</exception>
    public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl, Uri? htmlUrl, string version, string title, string description, CultureInfo? language)
    {
        OpmlOutline outline = new();
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentNullException.ThrowIfNull(xmlUrl);

        outline.Text = text;
        outline.ContentType = type;
        outline.Attributes.Add("xmlUrl", xmlUrl.ToString());

        if (htmlUrl is not null)
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

        if (language is not null)
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
    /// <returns><see langword="true"/> if the specified <see cref="OpmlOutline"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is OpmlOutline other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.ContentType), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.HasBreakpoint), HashCodeUtility.Component(this.IsCommented), HashCodeUtility.Component(this.Text));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(OpmlOutline? first, OpmlOutline? second) => !(first == second);

    /// <summary>
    /// Loads a single outline attribute from the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="attribute">The <see cref="XPathNavigator"/> to extract information from, positioned on one attribute of an <c>outline</c> element.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="attribute"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Attribute names are matched without regard to case, and anything not among the six the
    ///     specification defines is added to <see cref="Attributes"/> under its own name. An attribute with an
    ///     empty value, or a duplicate of one already collected, is skipped and reported as not loaded.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="attribute"/> is <see langword="null"/>.</exception>
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