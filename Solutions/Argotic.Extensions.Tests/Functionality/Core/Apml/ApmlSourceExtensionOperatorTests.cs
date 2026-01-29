using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlSourceExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlSource>
{
    protected override ApmlSource CreateInstance()
        => new() { Key = "source-b" };

    protected override ApmlSource CreateLesserInstance()
        => new() { Key = "source-a" };

    protected override ApmlSource CreateGreaterInstance()
        => new() { Key = "source-c" };
}