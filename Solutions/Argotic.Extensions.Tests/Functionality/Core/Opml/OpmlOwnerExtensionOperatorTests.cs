using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

[TestClass]
public class OpmlOwnerExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<OpmlOwner>
{
    protected override OpmlOwner CreateInstance()
        => new() { Name = "Bob" };

    protected override OpmlOwner CreateLesserInstance()
        => new() { Name = "Alice" };

    protected override OpmlOwner CreateGreaterInstance()
        => new() { Name = "Charlie" };
}