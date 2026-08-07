using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill an <see cref="AtomFeed"/> or <see cref="AtomEntry"/>.
/// </summary>
/// <remarks>
///     <para>
///     <b>Atom 0.3 is not an earlier draft of Atom 1.0; it is a different format that lost.</b> It is an
///     IETF-era snapshot that never became an RFC, in namespace <c>http://purl.org/atom/ns#</c>, and
///     RFC 4287 renamed much of it. The renames are the whole reason this adapter exists:
///     </para>
///     <para>
///     <c>modified</c> became <c>updated</c>; <c>issued</c> became <c>published</c>; <c>tagline</c> became
///     <c>subtitle</c>; <c>copyright</c> became <c>rights</c>; and a person's <c>url</c> became <c>uri</c>.
///     None of these are read by <see cref="Atom10SyndicationResourceAdapter"/>, and none of the 1.0 names
///     are read here. Atom 0.3 also has no <c>category</c>, no <c>icon</c> and no <c>logo</c> — which is why
///     the collection walk below reads three element types where the 1.0 one reads five.
///     </para>
///     <para>
///     <b>Text is typed by <c>mode</c>, not by <c>type</c>.</b> Atom 1.0 says <c>type="text|html|xhtml"</c>;
///     0.3 says <c>mode="xml|escaped|base64"</c> with a separate MIME <c>type</c> beside it.
///     <c>CreateTextContent</c> maps <c>escaped</c> to <see cref="AtomTextConstructType.Html"/>, <c>xml</c>
///     to <see cref="AtomTextConstructType.Xhtml"/>, and everything else — including <c>base64</c> — to
///     <see cref="AtomTextConstructType.Text"/>. <b>Base64 content is not decoded</b>: it is stored exactly
///     as the document spelled it, and a caller that wants the bytes must decode them.
///     </para>
///     <para>
///     Because 0.3 predates RFC 4287's stricter model, the walk is written to salvage rather than to
///     validate: an <c>xml</c>-mode construct falls back to the element's own text when no <c>xhtml:div</c>
///     is found, a date that will not parse leaves the property at its default, and no element is required.
///     </para>
/// </remarks>
public class Atom03SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Atom03SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Atom03SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Creates a namespace manager binding <c>atom</c> to the Atom 0.3 namespace and <c>xhtml</c> to XHTML.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A manager resolving <c>atom</c> to <c>http://purl.org/atom/ns#</c> and <c>xhtml</c> to the XHTML namespace.</returns>
    /// <remarks>
    ///     <para>
    ///     This exists because <c>AtomUtility.CreateNamespaceManager</c> binds <c>atom</c> to the Atom
    ///     <i>1.0</i> namespace. Selection matches on the resolved namespace URI, so a manager built there
    ///     resolves nothing in an Atom 0.3 document.
    ///     </para>
    ///     <para>
    ///     <c>atom</c> is bound to the constant and never to whatever default namespace the document
    ///     declares. That is the invariant, not an omission: binding the document's own default would make
    ///     a feed in some unrelated namespace parse as though it were Atom 0.3.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is <see langword="null"/>.</exception>
    protected static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);

        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("atom", "http://purl.org/atom/ns#");
        manager.AddNamespace("xhtml", AtomUtility.XhtmlNamespace);

        return manager;
    }

    /// <summary>
    /// Fills the entry from the <c>entry</c> root of a stand-alone entry document.
    /// </summary>
    /// <param name="resource">The <see cref="AtomEntry"/> to be filled.</param>
    /// <remarks>
    ///     The <c>entry</c> elements <i>inside</i> a feed document take the same private walk, reached from
    ///     <see cref="Fill(AtomFeed)"/>. The namespace manager is this class's own
    ///     <see cref="CreateNamespaceManager"/>, the one <see cref="Fill(AtomFeed)"/> uses. It has to be:
    ///     selection matches on the resolved namespace URI, so the manager <c>AtomUtility</c> builds —
    ///     which binds <c>atom</c> to the Atom <i>1.0</i> namespace — cannot match an element of an Atom
    ///     0.3 document, and this overload threw for every input it exists to handle.
    /// </remarks>
    /// <exception cref="FormatException">No <c>entry</c> root was found in the Atom 0.3 namespace — most often because the document is a feed document.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(AtomEntry resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = Atom03SyndicationResourceAdapter.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? entryNavigator = this.Navigator.SelectChildElement("atom", "entry", manager);

        if (entryNavigator is null)
        {
            throw new FormatException(AtomUtility.WrongDocumentShape("entry", "feed"));
        }

        Atom03SyndicationResourceAdapter.FillEntry(resource, entryNavigator, manager, this.Settings);
    }

    /// <summary>
    /// Fills the feed from the <c>feed</c> root: its common attributes, its <c>id</c>, <c>title</c> and <c>modified</c>, then its optional elements, its collections including every entry, and its syndication extensions.
    /// </summary>
    /// <param name="resource">The <see cref="AtomFeed"/> to be filled.</param>
    /// <remarks>
    ///     <c>modified</c>, not <c>updated</c> — Atom 0.3's spelling — read into
    ///     <see cref="AtomFeed.UpdatedOn"/> because that is the property RFC 4287's name maps to. Its value
    ///     is parsed as RFC 3339, which Atom 0.3 already required.
    /// </remarks>
    /// <exception cref="FormatException">The document has no <c>feed</c> root — most often because it is a stand-alone entry document.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(AtomFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = Atom03SyndicationResourceAdapter.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? feedNavigator = this.Navigator.SelectChildElement("atom", "feed", manager);

        if (feedNavigator is null)
        {
            throw new FormatException(AtomUtility.WrongDocumentShape("feed", "entry"));
        }

        AtomUtility.FillCommonObjectAttributes(resource, feedNavigator);

        XPathNavigator? idNavigator = feedNavigator.SelectChildElement("atom", "id", manager);
        XPathNavigator? titleNavigator = feedNavigator.SelectChildElement("atom", "title", manager);
        XPathNavigator? modifiedNavigator = feedNavigator.SelectChildElement("atom", "modified", manager);

        if (idNavigator is not null)
        {
            resource.Id = new AtomId();
            resource.Id.Load(idNavigator, this.Settings);
        }

        if (titleNavigator is not null)
        {
            resource.Title = Atom03SyndicationResourceAdapter.CreateTextContent(titleNavigator, manager, this.Settings);
        }

        if (modifiedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out DateTime updatedOn))
            {
                resource.UpdatedOn = updatedOn;
            }
        }

        Atom03SyndicationResourceAdapter.FillFeedOptionals(resource, feedNavigator, manager, this.Settings);
        Atom03SyndicationResourceAdapter.FillFeedCollections(resource, feedNavigator, manager, this.Settings);

        SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
        adapter.Fill(resource, manager);
    }

    /// <summary>
    /// Reads an Atom 0.3 <c>content</c> element into an <see cref="AtomContent"/>, unwrapping the <c>xhtml:div</c> when the element is in <c>xml</c> mode.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomContent"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Content construct.
    ///     </para>
    ///     <para>
    ///     Atom 0.3 carries two attributes where Atom 1.0 carries one: <c>type</c> is a MIME media type and
    ///     goes to <see cref="AtomContent.ContentType"/>, while <c>mode</c> says how the payload is encoded
    ///     and is used only to decide how to read it. In <c>xml</c> mode the content is the text of the
    ///     <c>xhtml:div</c> wrapper, with the element's own text as the fallback when there is no wrapper —
    ///     0.3-era publishers did not reliably emit one. In any other mode, including <c>base64</c>, the
    ///     element's text is taken verbatim and <b>not decoded</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static AtomContent CreateContent(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        AtomContent content = new();
        string modeAttribute = string.Empty;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(content, source);

        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            modeAttribute = source.GetAttribute("mode", string.Empty);

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                content.ContentType = typeAttribute;
            }
        }

        if (string.Equals(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase))
        {
            XPathNavigator? xhtmlDivNavigator = source.SelectChildElement("xhtml", "div", manager);
            if (xhtmlDivNavigator is not null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                content.Content = xhtmlDivNavigator.Value;
            }
            else if (!string.IsNullOrEmpty(source.Value))
            {
                content.Content = source.Value;
            }
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            content.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(content, manager);

        return content;
    }

    /// <summary>
    /// Reads an Atom 0.3 <c>generator</c> element — its <c>url</c> and <c>version</c> attributes and its text — into an <see cref="AtomGenerator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomGenerator"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 generator element.
    ///     </para>
    ///     <para>
    ///     The attribute is <c>url</c>; RFC 4287 renamed it <c>uri</c>. Both land on
    ///     <see cref="AtomGenerator.Uri"/>, so the difference is invisible downstream and visible only here.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static AtomGenerator CreateGenerator(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        AtomGenerator generator = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(generator, source);

        if (source.HasAttributes)
        {
            string urlAttribute = source.GetAttribute("url", string.Empty);
            string versionAttribute = source.GetAttribute("version", string.Empty);

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? uri))
                {
                    generator.Uri = uri;
                }
            }

            if (!string.IsNullOrEmpty(versionAttribute))
            {
                generator.Version = versionAttribute;
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            generator.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(generator, manager);

        return generator;
    }

    /// <summary>
    /// Reads an Atom 0.3 Person construct — <c>name</c>, <c>url</c> and <c>email</c> — into an <see cref="AtomPersonConstruct"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomPersonConstruct"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Person construct.
    ///     </para>
    ///     <para>
    ///     The child is <c>url</c>, which RFC 4287 renamed <c>uri</c>. Nothing else in the construct changed,
    ///     which is why the whole method exists to read one differently-spelled element.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static AtomPersonConstruct CreatePerson(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        AtomPersonConstruct person = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(person, source);

        XPathNavigator? nameNavigator = source.SelectChildElement("atom", "name", manager);
        XPathNavigator? urlNavigator = source.SelectChildElement("atom", "url", manager);
        XPathNavigator? emailNavigator = source.SelectChildElement("atom", "email", manager);

        if (nameNavigator is not null)
        {
            person.Name = nameNavigator.Value;
        }

        if (urlNavigator is not null)
        {
            if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                person.Uri = uri;
            }
        }

        if (emailNavigator is not null)
        {
            person.EmailAddress = emailNavigator.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(person, manager);

        return person;
    }

    /// <summary>
    /// Reads an Atom 0.3 Text construct, mapping its <c>mode</c> attribute onto <see cref="AtomTextConstructType"/> and unwrapping the <c>xhtml:div</c> where there is one.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomTextConstruct"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Content construct.
    ///     </para>
    ///     <para>
    ///     The mapping is <c>escaped</c> to <see cref="AtomTextConstructType.Html"/>, <c>xml</c> to
    ///     <see cref="AtomTextConstructType.Xhtml"/>, and anything else — <c>base64</c>, an unrecognised
    ///     value, or no <c>mode</c> at all — to <see cref="AtomTextConstructType.Text"/>. Base64 payloads are
    ///     therefore labelled as plain text and left encoded, which is lossless but not what the label says.
    ///     </para>
    ///     <para>
    ///     Only the <c>xhtml</c> mapping changes how the value is read: it takes the <c>xhtml:div</c>'s text,
    ///     falling back to the element's own text when the wrapper is missing.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static AtomTextConstruct CreateTextContent(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        AtomTextConstruct content = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(content, source);

        if (source.HasAttributes)
        {
            string modeAttribute = source.GetAttribute("mode", string.Empty);
            if (!string.IsNullOrEmpty(modeAttribute))
            {
                if (string.Equals(modeAttribute, "base64", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Text;
                }
                else if (string.Equals(modeAttribute, "escaped", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Html;
                }
                else if (string.Equals(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Xhtml;
                }
                else
                {
                    content.TextType = AtomTextConstructType.Text;
                }
            }
        }

        if (content.TextType == AtomTextConstructType.Xhtml)
        {
            XPathNavigator? xhtmlDivNavigator = source.SelectChildElement("xhtml", "div", manager);
            if (xhtmlDivNavigator is not null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                content.Content = xhtmlDivNavigator.Value;
            }
            else if (!string.IsNullOrEmpty(source.Value))
            {
                content.Content = source.Value;
            }
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            content.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(content, manager);

        return content;
    }

    /// <summary>
    /// Reads one Atom 0.3 <c>entry</c> subtree in full: common attributes, <c>id</c>, <c>title</c>, <c>modified</c>, the optional elements, the collections, and the entry's own syndication extensions.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEntry(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(entry, source);

        XPathNavigator? idNavigator = source.SelectChildElement("atom", "id", manager);
        XPathNavigator? titleNavigator = source.SelectChildElement("atom", "title", manager);
        XPathNavigator? modifiedNavigator = source.SelectChildElement("atom", "modified", manager);

        if (idNavigator is not null)
        {
            entry.Id = new AtomId();
            entry.Id.Load(idNavigator, settings);
        }

        if (titleNavigator is not null)
        {
            entry.Title = Atom03SyndicationResourceAdapter.CreateTextContent(titleNavigator, manager, settings);
        }

        if (modifiedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out DateTime updatedOn))
            {
                entry.UpdatedOn = updatedOn;
            }
        }

        Atom03SyndicationResourceAdapter.FillEntryOptionals(entry, source, manager, settings);
        Atom03SyndicationResourceAdapter.FillEntryCollections(entry, source, manager, settings);

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(entry, manager);
    }

    /// <summary>
    /// Reads the repeatable children of an Atom 0.3 <c>entry</c> — <c>author</c>, <c>contributor</c> and <c>link</c>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <remarks>
    ///     Three, where the Atom 1.0 walk reads four: 0.3 has no <c>category</c> element, so
    ///     <see cref="AtomEntry.Categories"/> is always empty on this path. Persons go through
    ///     <c>CreatePerson</c> and are added unconditionally, while links go through
    ///     <see cref="AtomLink"/>'s own loader and are added only when it reports success — a difference in
    ///     strictness between two adjacent loops in the same method.
    /// </remarks>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEntryCollections(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.SelectChildElements("atom", "author", manager);
        XPathNodeIterator contributorIterator = source.SelectChildElements("atom", "contributor", manager);
        XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                XPathNavigator? authorNode = authorIterator.Current;
                if (authorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct author = Atom03SyndicationResourceAdapter.CreatePerson(authorNode, manager, settings);
                entry.Authors.Add(author);
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                XPathNavigator? contributorNode = contributorIterator.Current;
                if (contributorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct contributor = Atom03SyndicationResourceAdapter.CreatePerson(contributorNode, manager, settings);
                entry.Contributors.Add(contributor);
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                AtomLink link = new();
                if (link.Load(linkNode, settings))
                {
                    entry.Links.Add(link);
                }
            }
        }
    }

    /// <summary>
    /// Reads the single-valued optional children of an Atom 0.3 <c>entry</c> — <c>content</c>, <c>summary</c>, and the publication date from <c>issued</c> or, failing that, <c>created</c>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEntryOptionals(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? contentNavigator = source.SelectChildElement("atom", "content", manager);

        // atom:issued first. Atom 0.3 requires issued and modified and makes created optional, and it
        // is issued -- the time the entry was issued -- that RFC 4287 carried forward as atom:published.
        // Reading only created meant an entry carrying the conformant minimum lost its publication date
        // silently: 28 of the 70 Atom 0.3 entries in the real-world corpus, including every entry of
        // Sam Ruby's feed and 23 of Brad Fitzpatrick's. created is still read when issued is absent, so
        // nothing that produced a date before stops doing so.
        XPathNavigator? issuedNavigator = source.SelectChildElement("atom", "issued", manager)
            ?? source.SelectChildElement("atom", "created", manager);

        XPathNavigator? summaryNavigator = source.SelectChildElement("atom", "summary", manager);

        if (contentNavigator is not null)
        {
            entry.Content = Atom03SyndicationResourceAdapter.CreateContent(contentNavigator, manager, settings);
        }

        if (issuedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(issuedNavigator.Value, out DateTime publishedOn))
            {
                entry.PublishedOn = publishedOn;
            }
        }

        if (summaryNavigator is not null)
        {
            entry.Summary = Atom03SyndicationResourceAdapter.CreateTextContent(summaryNavigator, manager, settings);
        }
    }

    /// <summary>
    /// Reads the repeatable children of an Atom 0.3 <c>feed</c> — <c>author</c>, <c>contributor</c>, <c>link</c> and <c>entry</c> — recursing into each entry's own subtree and extensions.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    ///     </para>
    ///     <para>
    ///     No <c>category</c>: Atom 0.3 has none, and subject terms in 0.3-era feeds arrive instead through
    ///     Dublin Core, as syndication extensions.
    ///     </para>
    ///     <para>
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> is tested at the top of the entry
    ///     loop, against the number of entries <i>kept</i>, so the entry that would trip the limit is never
    ///     parsed.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillFeedCollections(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.SelectChildElements("atom", "author", manager);
        XPathNodeIterator contributorIterator = source.SelectChildElements("atom", "contributor", manager);
        XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);
        XPathNodeIterator entryIterator = source.SelectChildElements("atom", "entry", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                XPathNavigator? authorNode = authorIterator.Current;
                if (authorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct author = Atom03SyndicationResourceAdapter.CreatePerson(authorNode, manager, settings);
                feed.Authors.Add(author);
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                XPathNavigator? contributorNode = contributorIterator.Current;
                if (contributorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct contributor = Atom03SyndicationResourceAdapter.CreatePerson(contributorNode, manager, settings);
                feed.Contributors.Add(contributor);
            }
        }

        if (entryIterator is { Count: > 0 })
        {
            int added = 0;
            while (entryIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && added >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? entryNode = entryIterator.Current;
                if (entryNode is null)
                {
                    continue;
                }

                AtomEntry entry = new();
                Atom03SyndicationResourceAdapter.FillEntry(entry, entryNode, manager, settings);

                feed.Entries.Add(entry);
                added++;
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                AtomLink link = new();
                if (link.Load(linkNode, settings))
                {
                    feed.Links.Add(link);
                }
            }
        }
    }

    /// <summary>
    /// Reads the three optional feed-level elements Atom 0.3 defines — <c>generator</c>, <c>copyright</c> and <c>tagline</c>.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    ///     </para>
    ///     <para>
    ///     Two of the three are renames rather than new properties: <c>copyright</c> fills
    ///     <see cref="AtomFeed.Rights"/> and <c>tagline</c> fills <see cref="AtomFeed.Subtitle"/>, which are
    ///     the RFC 4287 names for the same things. There is no <c>icon</c> and no <c>logo</c> in Atom 0.3,
    ///     so those properties stay null on this path.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillFeedOptionals(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? generatorNavigator = source.SelectChildElement("atom", "generator", manager);
        XPathNavigator? copyrightNavigator = source.SelectChildElement("atom", "copyright", manager);
        XPathNavigator? taglineNavigator = source.SelectChildElement("atom", "tagline", manager);

        if (generatorNavigator is not null)
        {
            feed.Generator = Atom03SyndicationResourceAdapter.CreateGenerator(generatorNavigator, manager, settings);
        }

        if (copyrightNavigator is not null)
        {
            feed.Rights = Atom03SyndicationResourceAdapter.CreateTextContent(copyrightNavigator, manager, settings);
        }

        if (taglineNavigator is not null)
        {
            feed.Subtitle = Atom03SyndicationResourceAdapter.CreateTextContent(taglineNavigator, manager, settings);
        }
    }
}