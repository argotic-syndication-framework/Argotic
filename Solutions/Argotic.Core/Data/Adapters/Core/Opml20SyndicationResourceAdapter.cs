using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="OpmlDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///     This adapter reads OPML 1.0, 1.1 and 2.0, not just the version in its name.
///     <see cref="SyndicationResourceAdapter"/> routes all three here because the document shape —
///     <c>opml</c> with a <c>head</c> and a <c>body</c> of <c>outline</c> elements — is identical across
///     them. What changed between versions is which attributes an <c>outline</c> may carry, and that is
///     <see cref="OpmlOutline"/>'s business, not this adapter's.
///     </para>
///     <para>
///     OPML elements bear no namespace, and the selectors match only the no-namespace partition.
///     </para>
///     <para>
///     Only the outlines directly under <c>body</c> are enumerated here. An OPML outline tree is arbitrarily
///     deep, and the recursion is <see cref="OpmlOutline"/>'s: each one loads its own <c>outline</c>
///     children. <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> therefore caps <i>top-level</i>
///     outlines, and a subscription list nested one level down is not capped at all.
///     </para>
/// </remarks>
public class Opml20SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Opml20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="OpmlDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="OpmlDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Opml20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Loads <c>opml/head</c>, enumerates the top-level <c>opml/body/outline</c> elements, and attaches the document-level syndication extensions found on <c>opml</c>.
    /// </summary>
    /// <param name="resource">The <see cref="OpmlDocument"/> to be filled.</param>
    /// <remarks>
    ///     The counter that <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> is tested against
    ///     advances for every <c>outline</c> element encountered, but the test itself sits inside the branch
    ///     taken only when the outline loaded. A body whose first outlines fail to load therefore yields
    ///     fewer than the limit — the limit counts elements seen, not outlines kept.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(OpmlDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(this.Navigator.NameTable);

        XPathNavigator? documentNavigator = this.Navigator.SelectChildElement("opml");
        if (documentNavigator is not null)
        {
            XPathNavigator? headNavigator = documentNavigator.SelectChildElement("head");
            if (headNavigator is not null)
            {
                resource.Head.Load(headNavigator, this.Settings);
            }

            XPathNodeIterator outlineIterator = documentNavigator.Select("body/outline", manager);
            if (outlineIterator is { Count: > 0 })
            {
                int counter = 0;
                while (outlineIterator.MoveNext())
                {
                    XPathNavigator? outlineNode = outlineIterator.Current;
                    if (outlineNode is null)
                    {
                        continue;
                    }

                    OpmlOutline outline = new();
                    counter++;

                    if (outline.Load(outlineNode, this.Settings))
                    {
                        if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                        {
                            break;
                        }

                        resource.Outlines.Add(outline);
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(documentNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }
}