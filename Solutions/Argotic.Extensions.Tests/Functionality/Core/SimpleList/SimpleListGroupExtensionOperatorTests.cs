using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.SimpleList;

[TestClass]
public class SimpleListGroupExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SimpleListGroup>
{
    protected override SimpleListGroup CreateInstance()
        => new() { Element = "group-b" };

    protected override SimpleListGroup CreateLesserInstance()
        => new() { Element = "group-a" };

    protected override SimpleListGroup CreateGreaterInstance()
        => new() { Element = "group-c" };
}
