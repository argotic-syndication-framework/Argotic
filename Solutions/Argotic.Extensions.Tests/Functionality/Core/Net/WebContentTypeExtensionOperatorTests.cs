using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class WebContentTypeExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<WebContentType>
{
    protected override WebContentType CreateInstance()
        => new("application", "xml");

    protected override WebContentType CreateLesserInstance()
        => new("application", "atom+xml");  // "atom+xml" < "xml"

    protected override WebContentType CreateGreaterInstance()
        => new("application", "zip");  // "zip" > "xml"
}
