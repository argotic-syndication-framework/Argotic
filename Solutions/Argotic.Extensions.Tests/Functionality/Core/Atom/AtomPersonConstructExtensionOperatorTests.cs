using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

[TestClass]
public class AtomPersonConstructExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<AtomPersonConstruct>
{
    protected override AtomPersonConstruct CreateInstance()
        => new() { Name = "Bob" };

    protected override AtomPersonConstruct CreateLesserInstance()
        => new() { Name = "Alice" };

    protected override AtomPersonConstruct CreateGreaterInstance()
        => new() { Name = "Charlie" };
}