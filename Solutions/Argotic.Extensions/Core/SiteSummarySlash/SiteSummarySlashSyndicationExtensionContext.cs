using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="SiteSummarySlashSyndicationExtension"/>.
/// </summary>
[Serializable]
public class SiteSummarySlashSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummarySlashSyndicationExtensionContext"/> class.
    /// </summary>
    public SiteSummarySlashSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the number of comments.
    /// </summary>
    /// <value>The number of comments. The default value is <see cref="Int32.MinValue"/>, which indicates that no comment count was specified.</value>
    public int Comments { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the name of the department.
    /// </summary>
    /// <value>The name of the department.</value>
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
    ///     A <see cref="IList{T}"/> collection of <see cref="Int32"/> objects that represent the hit parade identifiers.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<int> HitParade { get; } = [];

    /// <summary>
    /// Gets or sets the name of the section.
    /// </summary>
    /// <value>The name of the section.</value>
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
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummarySlashSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="SiteSummarySlashSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? sectionNavigator = source.SelectSingleNode("slash:section", manager);
            XPathNavigator? departmentNavigator = source.SelectSingleNode("slash:department", manager);
            XPathNavigator? commentsNavigator = source.SelectSingleNode("slash:comments", manager);
            XPathNavigator? hitParadeNavigator = source.SelectSingleNode("slash:hit_parade", manager);

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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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