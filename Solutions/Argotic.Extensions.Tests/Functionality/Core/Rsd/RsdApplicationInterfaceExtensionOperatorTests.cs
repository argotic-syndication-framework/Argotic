using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Rsd;

[TestClass]
public class RsdApplicationInterfaceExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RsdApplicationInterface>
{
    protected override RsdApplicationInterface CreateInstance()
        => new() { Name = "API B" };

    protected override RsdApplicationInterface CreateLesserInstance()
        => new() { Name = "API A" };

    protected override RsdApplicationInterface CreateGreaterInstance()
        => new() { Name = "API C" };
}