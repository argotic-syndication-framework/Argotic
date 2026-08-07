using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="ApmlDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///     APML 0.6 puts its elements in <c>http://www.apml.org/apml-0.6</c> and — unusually among the formats
///     here — capitalises them: <c>APML</c>, <c>Head</c>, <c>Body</c>, <c>Profile</c>, <c>Applications</c>,
///     <c>Application</c>. XML element names are case sensitive, so these selectors match nothing in a
///     document that spells them in lower case, and there is no fallback that would rescue one.
///     </para>
///     <para>
///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> caps profiles and does not cap
///     applications: the profile loop counts and breaks, the application loop does neither. That is
///     defensible — a profile carries the whole attention graph, an <see cref="ApmlApplication"/> carries
///     a name and a payload string — but it is a difference a caller setting a limit will not expect.
///     </para>
/// </remarks>
public class Apml06SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Apml06SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ApmlDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="ApmlDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Apml06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Loads <c>APML/Head</c>, then the <c>Profile</c> and <c>Applications/Application</c> children of <c>APML/Body</c>, then attaches the syndication extensions found on <c>APML</c>.
    /// </summary>
    /// <param name="resource">The <see cref="ApmlDocument"/> to be filled.</param>
    /// <remarks>
    ///     The <c>defaultprofile</c> attribute on <c>Body</c> — lower case, unlike the elements — names which
    ///     of the profiles is the active one. It is read as a string and never checked against the profiles
    ///     actually present, so <see cref="ApmlDocument.DefaultProfileName"/> may name a profile that is not
    ///     in the document.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(ApmlDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? headNavigator = this.Navigator.SelectSingleNode("apml:APML/apml:Head", manager);
        if (headNavigator is not null)
        {
            resource.Head.Load(headNavigator, this.Settings);
        }

        XPathNavigator? bodyNavigator = this.Navigator.SelectSingleNode("apml:APML/apml:Body", manager);
        if (bodyNavigator is not null)
        {
            if (bodyNavigator.HasAttributes)
            {
                string defaultProfileAttribute = bodyNavigator.GetAttribute("defaultprofile", string.Empty);
                if (!string.IsNullOrEmpty(defaultProfileAttribute))
                {
                    resource.DefaultProfileName = defaultProfileAttribute;
                }
            }

            XPathNodeIterator profileIterator = bodyNavigator.SelectChildElements("apml", "Profile", manager);
            if (profileIterator is { Count: > 0 })
            {
                int counter = 0;
                while (profileIterator.MoveNext())
                {
                    XPathNavigator? profileNode = profileIterator.Current;
                    if (profileNode is null)
                    {
                        continue;
                    }

                    ApmlProfile profile = new();
                    counter++;

                    if (profile.Load(profileNode, this.Settings))
                    {
                        if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                        {
                            break;
                        }

                        resource.Profiles.Add(profile);
                    }
                }
            }

            XPathNodeIterator applicationIterator = bodyNavigator.Select("apml:Applications/apml:Application", manager);
            if (applicationIterator is { Count: > 0 })
            {
                while (applicationIterator.MoveNext())
                {
                    XPathNavigator? applicationNode = applicationIterator.Current;
                    if (applicationNode is null)
                    {
                        continue;
                    }

                    ApmlApplication application = new();
                    if (application.Load(applicationNode, this.Settings))
                    {
                        resource.Applications.Add(application);
                    }
                }
            }
        }

        XPathNavigator? extensionRoot = this.Navigator.SelectChildElement("apml", "APML", manager);

        if (extensionRoot is null)

        {

            return;

        }


        SyndicationExtensionAdapter adapter = new(extensionRoot, this.Settings);
        adapter.Fill(resource, manager);
    }
}