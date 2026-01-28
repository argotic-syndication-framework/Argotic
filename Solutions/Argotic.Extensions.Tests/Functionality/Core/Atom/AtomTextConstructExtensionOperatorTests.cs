using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

[TestClass]
public class AtomTextConstructExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<AtomTextConstruct>
{
    protected override AtomTextConstruct CreateInstance()
        => new("Text B");

    protected override AtomTextConstruct CreateLesserInstance()
        => new("Text A");

    protected override AtomTextConstruct CreateGreaterInstance()
        => new("Text C");
}
