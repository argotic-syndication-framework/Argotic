using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RsdDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///     RSD 1.0 defines a <c>rsd</c> root in the <c>http://archipelago.phrasewise.com/rsd</c> namespace,
///     holding one <c>service</c> with <c>engineName</c>, <c>engineLink</c>, <c>homePageLink</c> and an
///     <c>apis</c> list. This adapter reads exactly that, and each <c>api</c> is read by
///     <see cref="RsdApplicationInterface"/> — including its <c>apiLink</c> attribute, which is the name RSD
///     1.0 gave the attribute RSD 0.6 called <c>rpcLink</c>.
///     </para>
///     <para>
///     Almost every selection goes through <c>RsdUtility.SelectSafe</c> or <c>SelectSafeSingleNode</c>,
///     which retry the query with the <c>rsd:</c> prefix stripped when the namespaced form finds nothing.
///     Real RSD documents are frequently served with no namespace at all — this is the concession that
///     reads them, and it costs a second XPath evaluation only on the documents that need it.
///     </para>
///     <para>
///     The <c>service</c> lookup carries a second, narrower concession: dasBlog emits <c>service</c> with an
///     empty default namespace inside a namespaced <c>rsd</c>, so a mixed <c>rsd:rsd/service</c> path is
///     tried when the fully namespaced one misses.
///     </para>
/// </remarks>
public class Rsd10SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rsd10SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RsdDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RsdDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rsd10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads the engine identity and the API list from <c>rsd/service</c>, then attaches the syndication extensions found on <c>rsd</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RsdDocument"/> to be filled.</param>
    /// <remarks>
    ///     Both links are accepted as <see cref="UriKind.RelativeOrAbsolute"/>. An <c>engineLink</c> that is
    ///     not a URI at all is dropped silently rather than failing the load, which is the house treatment
    ///     of a malformed value throughout the adapters.
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
                int counter = 0;
                while (apiIterator.MoveNext())
                {
                    XPathNavigator? apiNode = apiIterator.Current;
                    if (apiNode is null)
                    {
                        continue;
                    }

                    RsdApplicationInterface api = new();
                    counter++;

                    if (api.Load(apiNode, this.Settings))
                    {
                        if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                        {
                            break;
                        }

                        resource.Interfaces.Add(api);
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