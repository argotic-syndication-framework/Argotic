using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="LiveJournalSyndicationExtension"/>.
/// </summary>
public class LiveJournalSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalSyndicationExtensionContext"/> class.
    /// </summary>
    public LiveJournalSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating if the author has requested that the entry HTML be displayed without modification.
    /// </summary>
    /// <value><see langword="true"/> if the author has requested that the entry HTML be displayed without modification; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     If <see langword="false"/>, newlines within the entry must be expanded into visual newlines (using the <c>br</c> tag) to be displayed properly.
    ///     On the wire the element is a bare <c>lj:preformatted</c> with no content, so its presence is the whole of the signal and there is no way to say "no".
    /// </remarks>
    public bool IsPreformatted { get; set; }

    /// <summary>
    /// Gets or sets the current mood.
    /// </summary>
    /// <value>A <see cref="LiveJournalMood"/> object that represents the current mood, or <see langword="null"/> if none was specified.</value>
    public LiveJournalMood? Mood { get; set; }

    /// <summary>
    /// Gets or sets the current music.
    /// </summary>
    /// <value>The music the entry was written to, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     One free-text string. The module defines no structure for artist, album or track, so
    ///     anything a consumer wants to break out of it, it has to parse itself.
    /// </remarks>
    public string Music
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the access level.
    /// </summary>
    /// <value>A <see cref="LiveJournalSecurity"/> object that represents the access level, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     If absent, the entry is <see cref="LiveJournalSecurityType.Public">publicly</see> accessible.
    ///     A feed requested without authentication contains <i>only</i> public entries, so an unauthenticated
    ///     consumer will never see this element carry anything else.
    /// </remarks>
    public LiveJournalSecurity? Security { get; set; }

    /// <summary>
    /// Gets or sets the associated user picture.
    /// </summary>
    /// <value>A <see cref="LiveJournalUserPicture"/> object that represents the associated user picture, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     If omitted, the LiveJournal entry uses the feed-level default picture.
    /// </remarks>
    public LiveJournalUserPicture? UserPicture { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="LiveJournalSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="LiveJournalSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? musicNavigator = source.SelectChildElement("lj", "music", manager);
            XPathNavigator? moodNavigator = source.SelectChildElement("lj", "mood", manager);
            XPathNavigator? securityNavigator = source.SelectChildElement("lj", "security", manager);
            XPathNavigator? userPictureNavigator = source.SelectChildElement("lj", "userpic", manager);
            XPathNavigator? preformattedNavigator = source.SelectChildElement("lj", "preformatted", manager);

            if (musicNavigator is not null && !string.IsNullOrEmpty(musicNavigator.Value))
            {
                this.Music = musicNavigator.Value;
                wasLoaded = true;
            }

            if (moodNavigator is not null)
            {
                LiveJournalMood mood = new();
                if (mood.Load(moodNavigator))
                {
                    this.Mood = mood;
                    wasLoaded = true;
                }
            }

            if (securityNavigator is not null)
            {
                LiveJournalSecurity security = new();
                if (security.Load(securityNavigator))
                {
                    this.Security = security;
                    wasLoaded = true;
                }
            }

            if (userPictureNavigator is not null)
            {
                LiveJournalUserPicture userPicture = new();
                if (userPicture.Load(userPictureNavigator))
                {
                    this.UserPicture = userPicture;
                    wasLoaded = true;
                }
            }

            if (preformattedNavigator is not null)
            {
                this.IsPreformatted = true;
                wasLoaded = true;
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
        if (!string.IsNullOrEmpty(this.Music))
        {
            writer.WriteStartElement("music", xmlNamespace);
            writer.WriteCData(this.Music);
            writer.WriteEndElement();
        }

        this.Mood?.WriteTo(writer);

        this.Security?.WriteTo(writer);

        this.UserPicture?.WriteTo(writer);

        if (this.IsPreformatted)
        {
            writer.WriteElementString("preformatted", xmlNamespace, string.Empty);
        }
    }
}