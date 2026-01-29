using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssEnclosureExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssEnclosure>
{
    protected override RssEnclosure CreateInstance()
        => new(2000, "audio/mpeg", new Uri("http://example.com/b.mp3"));

    protected override RssEnclosure CreateLesserInstance()
        => new(1000, "audio/mpeg", new Uri("http://example.com/a.mp3"));

    protected override RssEnclosure CreateGreaterInstance()
        => new(3000, "audio/mpeg", new Uri("http://example.com/c.mp3"));
}