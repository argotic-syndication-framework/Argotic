using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

[TestClass]
public class OpmlWindowExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<OpmlWindow>
{
    protected override OpmlWindow CreateInstance()
        => new() { Top = 100, Left = 100, Bottom = 500, Right = 500 };

    protected override OpmlWindow CreateLesserInstance()
        => new() { Top = 50, Left = 50, Bottom = 400, Right = 400 };

    protected override OpmlWindow CreateGreaterInstance()
        => new() { Top = 150, Left = 150, Bottom = 600, Right = 600 };
}
