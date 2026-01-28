using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

[TestClass]
public class OpmlOutlineExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<OpmlOutline>
{
    protected override OpmlOutline CreateInstance()
        => new() { Text = "Outline B" };

    protected override OpmlOutline CreateLesserInstance()
        => new() { Text = "Outline A" };

    protected override OpmlOutline CreateGreaterInstance()
        => new() { Text = "Outline C" };
}
