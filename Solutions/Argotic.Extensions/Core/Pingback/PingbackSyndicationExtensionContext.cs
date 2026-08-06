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
    /// Gets the targets that were pinged in reference.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="Uri"/> objects that represent targets that were pinged in reference.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<Uri> Abouts { get; } = [];

    /// <summary>
    /// Gets or sets the URL of the Pingback server.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the Pingback server.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <value>A <see cref="Uri"/> that represents the value that should be used as the <i>targetURI</i> in a ping.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="PingbackSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="PingbackSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        writer.WriteElementString("server", xmlNamespace, this.Server?.ToString() ?? string.Empty);
        writer.WriteElementString("target", xmlNamespace, this.Target?.ToString() ?? string.Empty);

        foreach (Uri about in this.Abouts)
        {
            if (about is not null)
            {
                writer.WriteElementString("about", xmlNamespace, about.ToString());
            }
        }
    }
}