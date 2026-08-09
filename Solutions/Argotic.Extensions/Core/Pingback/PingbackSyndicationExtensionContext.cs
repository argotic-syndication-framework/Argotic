using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="PingbackSyndicationExtension"/>.
/// </summary>
public class PingbackSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PingbackSyndicationExtensionContext"/> class.
    /// </summary>
    public PingbackSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets the targets this item has already pinged.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="Uri"/> objects that represent targets that were pinged in reference.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     One <c>pingback:about</c> element per entry, and the direction is the opposite of
    ///     <see cref="Server"/> and <see cref="Target"/>: those two say how to ping <i>this</i> item,
    ///     while these record what this item pinged.
    /// </remarks>
    public IList<Uri> Abouts { get; } = [];

    /// <summary>
    /// Gets or sets the URL of the Pingback server.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the XML-RPC endpoint to call, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     The address a linking site posts its <c>pingback.ping</c> call to. Written as
    ///     <c>pingback:server</c>, and only when it has one: an empty element would name an endpoint of
    ///     the empty string.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Server
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the value that should be used as the <i>targetURI</i> in a ping.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the <i>targetURI</i> to pass in the ping, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     The second argument of <c>pingback.ping</c>, and the reason this module exists: it is the
    ///     canonical address of the item being linked to, which need not be the item's <c>link</c> and
    ///     which a caller would otherwise have to derive by fetching the page. Written only when it has
    ///     one, the same as <see cref="Server"/> and as the <see cref="Abouts"/> loop below them.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Target
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
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="PingbackSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="PingbackSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (source.HasChildren)
        {
            XPathNavigator? serverNavigator = source.SelectChildElement("pingback", "server", manager);
            XPathNavigator? targetNavigator = source.SelectChildElement("pingback", "target", manager);
            XPathNodeIterator aboutIterator = source.SelectChildElements("pingback", "about", manager);

            if (serverNavigator is not null)
            {
                if (Uri.TryCreate(serverNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? server))
                {
                    this.Server = server;
                    wasLoaded = true;
                }
            }

            if (targetNavigator is not null)
            {
                if (Uri.TryCreate(targetNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? target))
                {
                    this.Target = target;
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
        if (this.Server is not null)
        {
            writer.WriteElementString("server", xmlNamespace, this.Server.ToString());
        }

        if (this.Target is not null)
        {
            writer.WriteElementString("target", xmlNamespace, this.Target.ToString());
        }

        foreach (Uri about in this.Abouts)
        {
            if (about is not null)
            {
                writer.WriteElementString("about", xmlNamespace, about.ToString());
            }
        }
    }
}