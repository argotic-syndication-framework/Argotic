using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

[TestClass]
public class SitemapNewsPublicationExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SitemapNewsPublication>
{
    protected override SitemapNewsPublication CreateInstance()
        => new() { Name = "Publication B", Language = "en" };

    protected override SitemapNewsPublication CreateLesserInstance()
        => new() { Name = "Publication A", Language = "en" };

    protected override SitemapNewsPublication CreateGreaterInstance()
        => new() { Name = "Publication C", Language = "en" };
}