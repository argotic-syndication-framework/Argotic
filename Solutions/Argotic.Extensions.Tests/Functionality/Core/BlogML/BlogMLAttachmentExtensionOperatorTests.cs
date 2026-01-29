using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

[TestClass]
public class BlogMLAttachmentExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<BlogMLAttachment>
{
    protected override BlogMLAttachment CreateInstance()
        => new() { MimeType = "image/png", Url = new Uri("http://example.com/b.png") };

    protected override BlogMLAttachment CreateLesserInstance()
        => new() { MimeType = "image/jpeg", Url = new Uri("http://example.com/a.jpg") };

    protected override BlogMLAttachment CreateGreaterInstance()
        => new() { MimeType = "image/webp", Url = new Uri("http://example.com/c.webp") };
}