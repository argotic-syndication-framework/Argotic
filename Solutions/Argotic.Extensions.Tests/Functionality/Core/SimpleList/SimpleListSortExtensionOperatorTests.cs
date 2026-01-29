using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.SimpleList;

[TestClass]
public class SimpleListSortExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SimpleListSort>
{
    protected override SimpleListSort CreateInstance()
        => new() { Element = "sort-b" };

    protected override SimpleListSort CreateLesserInstance()
        => new() { Element = "sort-a" };

    protected override SimpleListSort CreateGreaterInstance()
        => new() { Element = "sort-c" };
}