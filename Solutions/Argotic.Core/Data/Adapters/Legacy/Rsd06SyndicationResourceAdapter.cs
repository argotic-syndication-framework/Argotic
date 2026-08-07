using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RsdDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///     RSD 0.6 and RSD 1.0 share a document shape and a namespace, and differ in one detail that matters:
///     0.6 names the endpoint attribute on <c>api</c> <c>rpcLink</c>, where 1.0 names it <c>apiLink</c>.
///     <see cref="RsdApplicationInterface"/> reads only <c>apiLink</c>, so this adapter reads <c>rpcLink</c>
///     itself before delegating, and keeps the interface when <c>Load</c> reported nothing but a link was
///     recovered. A 0.6 document whose <c>api</c> elements carry only <c>rpcLink</c> would otherwise load
///     as a document with no interfaces at all.
///     </para>
///     <para>
///     Everything else is the RSD 1.0 walk: <c>rsd/service</c> for the engine identity, <c>apis/api</c> for
///     the interfaces, and the same <c>RsdUtility</c> selectors that retry without the <c>rsd:</c> prefix so
///     that documents served with no namespace still read, plus the dasBlog case where <c>service</c> alone
///     carries an empty default namespace.
///     </para>
/// </remarks>
public sealed class Rsd06SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rsd06SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RsdDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RsdDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rsd06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads the engine identity and the API list from <c>rsd/service</c>, recovering each interface's endpoint from the 0.6 <c>rpcLink</c> attribute, then attaches the syndication extensions found on <c>rsd</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RsdDocument"/> to be filled.</param>
    /// <remarks>
    ///     <c>rpcLink</c> is read <i>before</i> <c>Load</c>, so an <c>apiLink</c> on the same element — read
    ///     inside <c>Load</c> — overwrites it and the newer spelling wins where a document carries both. The
    ///     interface is then kept on <c>api.Load(...) || api.Link is not null</c>, so an <c>api</c> whose
    ///     only usable content was <c>rpcLink</c> survives a <c>Load</c> that reported nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(RsdDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = RsdUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? serviceNavigator = RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/rsd:service", manager) ??
                                          //  dasBlog places an empty default XML namespace on the <service> element, this is a hack/compromise
                                          RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/service", manager);

        if (serviceNavigator is not null)
        {
            XPathNavigator? engineNameNavigator = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:engineName", manager);
            XPathNavigator? engineLinkNavigator = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:engineLink", manager);
            XPathNavigator? homePageLinkNavigator = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:homePageLink", manager);
            XPathNodeIterator apiIterator = RsdUtility.SelectSafe(serviceNavigator, "rsd:apis/rsd:api", manager);

            if (engineNameNavigator is not null && !string.IsNullOrEmpty(engineNameNavigator.Value))
            {
                resource.EngineName = engineNameNavigator.Value;
            }

            if (engineLinkNavigator is not null)
            {
                if (Uri.TryCreate(engineLinkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
                {
                    resource.EngineLink = link;
                }
            }

            if (homePageLinkNavigator is not null)
            {
                if (Uri.TryCreate(homePageLinkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? homepage))
                {
                    resource.Homepage = homepage;
                }
            }

            if (apiIterator is { Count: > 0 })
            {
                int added = 0;
                while (apiIterator.MoveNext())
                {
                    if (this.Settings.RetrievalLimit != 0 && added >= this.Settings.RetrievalLimit)
                    {
                        break;
                    }

                    XPathNavigator? apiNode = apiIterator.Current;
                    if (apiNode is null)
                    {
                        continue;
                    }

                    RsdApplicationInterface api = new();

                    string rpcLinkAttribute = apiNode.GetAttribute("rpcLink", string.Empty);
                    if (Uri.TryCreate(rpcLinkAttribute, UriKind.RelativeOrAbsolute, out Uri? link))
                    {
                        api.Link = link;
                    }

                    if (api.Load(apiNode, this.Settings) || api.Link is not null)
                    {
                        resource.Interfaces.Add(api);
                        added++;
                    }
                }
            }
        }

        XPathNavigator? extensionRoot = RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd", manager);

        if (extensionRoot is null)

        {

            return;

        }


        SyndicationExtensionAdapter adapter = new(extensionRoot, this.Settings);
        adapter.Fill(resource, manager);
    }
}