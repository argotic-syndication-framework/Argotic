using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlProfileExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlProfile>
{
    protected override ApmlProfile CreateInstance()
        => new() { Name = "profile-b" };

    protected override ApmlProfile CreateLesserInstance()
        => new() { Name = "profile-a" };

    protected override ApmlProfile CreateGreaterInstance()
        => new() { Name = "profile-c" };
}