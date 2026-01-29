using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

[TestClass]
public class ITunesOwnerExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ITunesOwner>
{
    protected override ITunesOwner CreateInstance()
        => new() { Name = "Owner B" };

    protected override ITunesOwner CreateLesserInstance()
        => new() { Name = "Owner A" };

    protected override ITunesOwner CreateGreaterInstance()
        => new() { Name = "Owner C" };
}