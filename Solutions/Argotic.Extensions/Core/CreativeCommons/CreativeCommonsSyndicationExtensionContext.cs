using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="CreativeCommonsSyndicationExtension"/>.
/// </summary>
public class CreativeCommonsSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreativeCommonsSyndicationExtensionContext"/> class.
    /// </summary>
    public CreativeCommonsSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets the creative commons licenses that apply to the published content.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="Uri"/> objects that represent the creative commons licenses that apply to the published content.</value>
    /// <remarks>
    ///     See <a href="https://creativecommons.org/licenses/">https://creativecommons.org/licenses/</a> for a listing of the current Creative Commons licenses.
    /// </remarks>
    public IList<Uri> Licenses { get; } = [];

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="CreativeCommonsSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="CreativeCommonsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNodeIterator licenseIterator = source.SelectChildElements("creativeCommons", "license", manager);
            if (licenseIterator is { Count: > 0 })
            {
                while (licenseIterator.MoveNext())
                {
                    XPathNavigator? licenseNode = licenseIterator.Current;
                    if (licenseNode is null)
                    {
                        continue;
                    }

                    if (Uri.TryCreate(licenseNode.Value, UriKind.RelativeOrAbsolute, out Uri? license))
                    {
                        this.Licenses.Add(license);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        if (this.Licenses.Count > 0)
        {
            foreach (Uri license in this.Licenses)
            {
                writer.WriteElementString("license", xmlNamespace, license.ToString());
            }
        }
    }
}