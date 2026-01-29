using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlHeadExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlHead>
{
    protected override ApmlHead CreateInstance()
        => new() { Title = "Head B" };

    protected override ApmlHead CreateLesserInstance()
        => new() { Title = "Head A" };

    protected override ApmlHead CreateGreaterInstance()
        => new() { Title = "Head C" };
}