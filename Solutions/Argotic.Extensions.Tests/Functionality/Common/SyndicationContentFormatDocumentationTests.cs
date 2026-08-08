using System.Reflection;

using Argotic.Publishing;
namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the <see cref="MimeMediaTypeAttribute.Documentation"/> link on every
/// <see cref="SyndicationContentFormat"/> member and on the two attributed publishing types.
/// </summary>
/// <remarks>
///     <para>
///     These links are inert: <see cref="MimeMediaTypeAttribute.Documentation"/> is read only by the
///     attribute's own <c>ToString</c>, <c>CompareTo</c> and <c>GetHashCode</c>, and nothing in the
///     framework dereferences it or matches a content type against it. Being inert is exactly why they
///     rotted unnoticed — <c>apml.org</c> now resolves to a farm shop, <c>blogml.org</c> to an unrelated
///     commercial site, and <c>newsml.org</c> to an S3 bucket with all access disabled.
///     </para>
///     <para>
///     Two of them cited fragments that never existed at the target: <c>#iana-atomcat</c> and
///     <c>#iana-atomsvc</c> come from the pre-publication draft on <c>bitworking.org</c>, and RFC 5023
///     uses neither anchor name. They are re-derived here to the sections that actually register the
///     media types, §16.1 and §16.2.
///     </para>
/// </remarks>
[TestClass]
public sealed class SyndicationContentFormatDocumentationTests
{
    /// <summary>
    /// Hosts that no longer serve the specification they were cited for, and that must therefore never
    /// reappear in a <see cref="MimeMediaTypeAttribute.Documentation"/>.
    /// </summary>
    /// <remarks>
    ///     Each was fetched. <c>apml.org</c> and <c>blogml.org</c> resolve to unrelated commercial sites;
    ///     <c>newsml.org</c> is an S3 bucket answering <c>AllAccessDisabled</c>; <c>atomenabled.org</c>
    ///     answers 403; <c>opensearch.org</c> answers 404 for the specification path;
    ///     <c>bitworking.org</c> moved the document and dropped the anchors that were cited into it.
    ///     <para>
    ///     Only genuinely dead, hijacked or emptied hosts belong here. A host that merely redirects
    ///     correctly — <c>cyber.law.harvard.edu</c> — is not listed, because banning it would also reject
    ///     a future correct link; the pinned rows above cover those.
    ///     </para>
    /// </remarks>
    private static readonly string[] HostsThatNoLongerServeTheirSpecification =
    [
        "apml.org",
        "www.apml.org",
        "blogml.org",
        "www.blogml.org",
        "atomenabled.org",
        "www.atomenabled.org",
        "newsml.org",
        "www.newsml.org",
        "opensearch.org",
        "www.opensearch.org",
        "bitworking.org",
        "www.bitworking.org",
    ];

    /// <summary>
    /// Gets every <see cref="MimeMediaTypeAttribute"/> the framework applies, paired with the name of
    /// what it is applied to.
    /// </summary>
    /// <remarks>
    ///     Swept rather than listed, so a new attributed format is covered the moment it compiles. The
    ///     attribute targets fields and classes, so both are scanned.
    /// </remarks>
    private static IEnumerable<(string Target, MimeMediaTypeAttribute Attribute)> AllMimeMediaTypes
    {
        get
        {
            Assembly[] assemblies =
            [
                typeof(MimeMediaTypeAttribute).Assembly,
                typeof(AtomCategoryDocument).Assembly,
                typeof(Argotic.Extensions.SyndicationExtension).Assembly,
            ];

            foreach (Type type in assemblies.Distinct().SelectMany(assembly => assembly.GetExportedTypes()))
            {
                if (type.GetCustomAttribute<MimeMediaTypeAttribute>() is { } onType)
                {
                    yield return (type.Name, onType);
                }

                // Not named `field`: inside a property accessor that is a C# 14 keyword bound to the
                // synthesized backing field, which makes this loop variable a compile error.
                foreach (FieldInfo member in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (member.GetCustomAttribute<MimeMediaTypeAttribute>() is { } onField)
                    {
                        yield return ($"{type.Name}.{member.Name}", onField);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Every syndication content format cites exactly this documentation link.
    /// </summary>
    [TestMethod]
    [DataRow(SyndicationContentFormat.Apml, "https://github.com/apml/spec-0.6")]
    [DataRow(SyndicationContentFormat.Atom, "https://www.rfc-editor.org/rfc/rfc4287.html")]
    [DataRow(SyndicationContentFormat.BlogML, "https://web.archive.org/web/20110725082012/http://blogml.org/")]
    [DataRow(SyndicationContentFormat.MicroSummaryGenerator, "https://web.archive.org/web/20121018220937/https://developer.mozilla.org/en-US/docs/Microsummary_XML_grammar_reference")]
    [DataRow(SyndicationContentFormat.NewsML, "https://iptc.org/standards/newsml-g2/")]
    [DataRow(SyndicationContentFormat.OpenSearchDescription, "https://github.com/dewitt/opensearch/blob/master/opensearch-1-1-draft-6.md#opensearch-description-document")]
    [DataRow(SyndicationContentFormat.Opml, "https://opml.org/spec2.opml")]
    [DataRow(SyndicationContentFormat.Rsd, "https://cyber.harvard.edu/blogs/gems/tech/rsd.html")]
    [DataRow(SyndicationContentFormat.Rss, "https://www.rssboard.org/rss-specification")]
    [DataRow(SyndicationContentFormat.Rdf, "https://www.w3.org/TR/2003/WD-rdf-concepts-20030123/#ref-rdf-mime-type")]
    [DataRow(SyndicationContentFormat.AtomCategoryDocument, "https://datatracker.ietf.org/doc/html/rfc5023#section-16.1")]
    [DataRow(SyndicationContentFormat.AtomServiceDocument, "https://datatracker.ietf.org/doc/html/rfc5023#section-16.2")]
    [DataRow(SyndicationContentFormat.Sitemap, "https://www.sitemaps.org/protocol.html")]
    [DataRow(SyndicationContentFormat.SitemapIndex, "https://www.sitemaps.org/protocol.html")]
    [DataRow(SyndicationContentFormat.AtomEntryDocument, "https://datatracker.ietf.org/doc/html/rfc4287#section-2")]
    public void EverySyndicationContentFormat_PinsItsDocumentationLink(SyndicationContentFormat format, string expectedDocumentation)
    {
        // Arrange & Act
        MimeMediaTypeAttribute? attribute = typeof(SyndicationContentFormat)
            .GetField(format.ToString())!
            .GetCustomAttribute<MimeMediaTypeAttribute>();

        // Assert
        attribute.ShouldNotBeNull();
        attribute!.Documentation.ShouldBe(expectedDocumentation);
    }

    /// <summary>
    /// The two publishing document types carry the same link as the format they correspond to.
    /// </summary>
    /// <remarks>
    ///     The attribute is declared twice for each — once on the enumeration member, once on the type —
    ///     so the two can drift apart. They describe the same media type and must agree.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(AtomCategoryDocument), SyndicationContentFormat.AtomCategoryDocument, "https://datatracker.ietf.org/doc/html/rfc5023#section-16.1")]
    [DataRow(typeof(AtomServiceDocument), SyndicationContentFormat.AtomServiceDocument, "https://datatracker.ietf.org/doc/html/rfc5023#section-16.2")]
    public void APublishingDocumentType_CitesTheSameLinkAsItsFormat(Type documentType, SyndicationContentFormat format, string expectedDocumentation)
    {
        // Arrange & Act
        MimeMediaTypeAttribute? onType = documentType.GetCustomAttribute<MimeMediaTypeAttribute>();
        MimeMediaTypeAttribute? onFormat = typeof(SyndicationContentFormat)
            .GetField(format.ToString())!
            .GetCustomAttribute<MimeMediaTypeAttribute>();

        // Assert
        onType.ShouldNotBeNull();
        onFormat.ShouldNotBeNull();
        onType!.Documentation.ShouldBe(expectedDocumentation);
        onType.Documentation.ShouldBe(onFormat!.Documentation);
    }

    /// <summary>
    /// Every applied media type cites its specification over <c>https</c>, at an absolute URL.
    /// </summary>
    [TestMethod]
    public void EveryAppliedMimeMediaType_CitesAnAbsoluteHttpsUrl()
    {
        // Arrange & Act
        (string Target, MimeMediaTypeAttribute Attribute)[] applied = AllMimeMediaTypes.ToArray();

        // Assert
        applied.ShouldNotBeEmpty();
        foreach ((string target, MimeMediaTypeAttribute attribute) in applied)
        {
            attribute.Documentation.ShouldNotBeNullOrEmpty($"{target} has no documentation link.");
            Uri.TryCreate(attribute.Documentation, UriKind.Absolute, out Uri? link).ShouldBeTrue($"{target} has a relative documentation link.");
            link!.Scheme.ShouldBe(Uri.UriSchemeHttps, $"{target} does not cite its specification over https.");
        }
    }

    /// <summary>
    /// No applied media type points at a host that stopped serving the specification.
    /// </summary>
    [TestMethod]
    public void NoAppliedMimeMediaType_PointsAtAHostThatStoppedServingIt()
    {
        // Arrange & Act
        (string Target, MimeMediaTypeAttribute Attribute)[] applied = AllMimeMediaTypes.ToArray();

        // Assert
        applied.ShouldNotBeEmpty();
        foreach ((string target, MimeMediaTypeAttribute attribute) in applied)
        {
            Uri.TryCreate(attribute.Documentation, UriKind.Absolute, out Uri? link).ShouldBeTrue($"{target} has a relative documentation link.");
            HostsThatNoLongerServeTheirSpecification.ShouldNotContain(
                link!.Host,
                $"{target} cites {link.Host}, which no longer serves the specification.");
        }
    }
}