using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

[TestClass]
public class ITunesCategoryExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ITunesCategory>
{
    protected override ITunesCategory CreateInstance()
        => new() { Text = "Category B" };

    protected override ITunesCategory CreateLesserInstance()
        => new() { Text = "Category A" };

    protected override ITunesCategory CreateGreaterInstance()
        => new() { Text = "Category C" };
}