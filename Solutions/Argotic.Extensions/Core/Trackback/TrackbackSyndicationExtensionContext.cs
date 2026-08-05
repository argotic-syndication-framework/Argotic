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
    /// Gets the trackbacks that were pinged in reference.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="Uri"/> objects that represent trackbacks that were pinged in reference.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<Uri> Abouts { get; } = [];

    /// <summary>
    /// Gets or sets the TrackBack URL.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the item's TrackBack URL.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="TrackbackSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="TrackbackSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? pingNavigator = source.SelectChildElement("trackback", "ping", manager);
            XPathNodeIterator aboutIterator = source.Select("trackback:about", manager);

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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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