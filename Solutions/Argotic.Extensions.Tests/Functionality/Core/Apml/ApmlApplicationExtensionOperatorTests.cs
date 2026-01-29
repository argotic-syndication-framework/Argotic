using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlApplicationExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlApplication>
{
    protected override ApmlApplication CreateInstance()
        => new() { Name = "App B" };

    protected override ApmlApplication CreateLesserInstance()
        => new() { Name = "App A" };

    protected override ApmlApplication CreateGreaterInstance()
        => new() { Name = "App C" };
}