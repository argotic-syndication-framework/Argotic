using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="TrackbackSyndicationExtension"/>.
/// </summary>
public class TrackbackSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackSyndicationExtensionContext"/> class.
    /// </summary>
    public TrackbackSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets the addresses this item has already pinged.
    /// </summary>
    /// <value>
    ///     One <see cref="Uri"/> per <c>trackback:about</c> element, in document order. The default value
    ///     is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     A record of outbound notifications, and the inverse of <see cref="Ping"/>: that property says
    ///     where others should ping this item, these say where this item pinged others.
    /// </remarks>
    public IList<Uri> Abouts { get; } = [];

    /// <summary>
    /// Gets or sets the address at which this item accepts Trackback pings.
    /// </summary>
    /// <value>
    ///     The <c>trackback:ping</c> URL, or <see langword="null"/> if none was specified.
    /// </value>
    /// <remarks>
    ///     The element is written whether or not this is set — a <see langword="null"/> ping produces an
    ///     empty <c>trackback:ping</c> rather than no element, so every serialized context carries one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Ping
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="TrackbackSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="TrackbackSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? pingNavigator = source.SelectChildElement("trackback", "ping", manager);
            XPathNodeIterator aboutIterator = source.SelectChildElements("trackback", "about", manager);

            if (pingNavigator is not null)
            {
                if (Uri.TryCreate(pingNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? ping))
                {
                    this.Ping = ping;
                    wasLoaded = true;
                }
            }

            if (aboutIterator is { Count: > 0 })
            {
                while (aboutIterator.MoveNext())
                {
                    XPathNavigator? aboutNode = aboutIterator.Current;
                    if (aboutNode is null)
                    {
                        continue;
                    }

                    if (Uri.TryCreate(aboutNode.Value, UriKind.RelativeOrAbsolute, out Uri? about))
                    {
                        this.Abouts.Add(about);
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
        writer.WriteElementString("ping", xmlNamespace, this.Ping?.ToString() ?? string.Empty);

        foreach (Uri about in this.Abouts)
        {
            if (about is not null)
            {
                writer.WriteElementString("about", xmlNamespace, about.ToString());
            }
        }
    }
}