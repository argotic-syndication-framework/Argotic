using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaCopyrightExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaCopyright>
{
    protected override YahooMediaCopyright CreateInstance()
        => new() { Text = "Copyright B" };

    protected override YahooMediaCopyright CreateLesserInstance()
        => new() { Text = "Copyright A" };

    protected override YahooMediaCopyright CreateGreaterInstance()
        => new() { Text = "Copyright C" };
}
