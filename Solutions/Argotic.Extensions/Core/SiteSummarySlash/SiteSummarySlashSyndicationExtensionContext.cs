using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="SiteSummarySlashSyndicationExtension"/>.
/// </summary>
public class SiteSummarySlashSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummarySlashSyndicationExtensionContext"/> class.
    /// </summary>
    public SiteSummarySlashSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the number of comments on the item.
    /// </summary>
    /// <value>
    ///     The comment count. The default value is <see cref="int.MinValue"/>, which stands in for "absent"
    ///     and is the one value <see cref="WriteTo"/> will not write.
    /// </value>
    /// <remarks>
    ///     The most portable element in the module, because RSS has no comment count of its own. Nothing
    ///     rejects a negative count other than the sentinel itself, so treat any value below zero as
    ///     suspect rather than meaningful.
    /// </remarks>
    public int Comments { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the department the item was filed under.
    /// </summary>
    /// <value>
    ///     The department name, trimmed. The default value is an <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     Slashdot's "from the department" tagline. Setting <see langword="null"/> or an empty string
    ///     clears the value rather than throwing.
    /// </remarks>
    public string Department
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets the hit parade identifiers.
    /// </summary>
    /// <value>
    ///     The identifiers, in the order they appeared. The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Serialized as one comma-delimited <c>slash:hit_parade</c> element, not as repeated elements, and
    ///     split back apart on load. Entries that will not parse as an <see cref="int"/> are skipped
    ///     silently, so a malformed list loads short rather than failing.
    /// </remarks>
    public IList<int> HitParade { get; } = [];

    /// <summary>
    /// Gets or sets the section of the site the item belongs to.
    /// </summary>
    /// <value>The section name, trimmed. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Setting <see langword="null"/> or an empty string clears the value rather than throwing.
    /// </remarks>
    public string Section
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SiteSummarySlashSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummarySlashSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? sectionNavigator = source.SelectChildElement("slash", "section", manager);
            XPathNavigator? departmentNavigator = source.SelectChildElement("slash", "department", manager);
            XPathNavigator? commentsNavigator = source.SelectChildElement("slash", "comments", manager);
            XPathNavigator? hitParadeNavigator = source.SelectChildElement("slash", "hit_parade", manager);

            if (sectionNavigator is not null && !string.IsNullOrEmpty(sectionNavigator.Value))
            {
                this.Section = sectionNavigator.Value;
                wasLoaded = true;
            }

            if (departmentNavigator is not null && !string.IsNullOrEmpty(departmentNavigator.Value))
            {
                this.Department = departmentNavigator.Value;
                wasLoaded = true;
            }

            if (commentsNavigator is not null)
            {
                if (int.TryParse(commentsNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int comments))
                {
                    this.Comments = comments;
                    wasLoaded = true;
                }
            }

            if (hitParadeNavigator is not null && !string.IsNullOrEmpty(hitParadeNavigator.Value))
            {
                if (hitParadeNavigator.Value.Contains(',', StringComparison.Ordinal))
                {
                    string[] identifiers = hitParadeNavigator.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (identifiers is { Length: > 0 })
                    {
                        foreach (string identifier in identifiers)
                        {
                            if (int.TryParse(identifier, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int paradeId))
                            {
                                this.HitParade.Add(paradeId);
                                wasLoaded = true;
                            }
                        }
                    }
                }
                else
                {
                    if (int.TryParse(hitParadeNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int hitParade))
                    {
                        this.HitParade.Add(hitParade);
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
        if (!string.IsNullOrEmpty(this.Section))
        {
            writer.WriteStartElement("section", xmlNamespace);
            writer.WriteCData(this.Section);
            writer.WriteEndElement();
        }

        if (!string.IsNullOrEmpty(this.Department))
        {
            writer.WriteStartElement("department", xmlNamespace);
            writer.WriteCData(this.Department);
            writer.WriteEndElement();
        }

        if (this.Comments != int.MinValue)
        {
            writer.WriteElementString("comments", xmlNamespace, this.Comments.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        if (this.HitParade.Count > 0)
        {
            // The invariant formatting is kept deliberately: string.Join's IEnumerable<int> overload
            // would format through the current culture, which changes the negative sign in some.
            writer.WriteElementString(
                "hit_parade",
                xmlNamespace,
                string.Join(",", this.HitParade.Select(hit => hit.ToString(System.Globalization.NumberFormatInfo.InvariantInfo))));
        }
    }
}