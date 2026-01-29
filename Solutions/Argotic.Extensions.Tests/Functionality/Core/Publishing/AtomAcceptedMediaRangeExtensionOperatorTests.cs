using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Publishing;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

[TestClass]
public class AtomAcceptedMediaRangeExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<AtomAcceptedMediaRange>
{
    protected override AtomAcceptedMediaRange CreateInstance()
        => new() { MediaRange = "application/xml" };

    protected override AtomAcceptedMediaRange CreateLesserInstance()
        => new() { MediaRange = "application/atom+xml" };

    protected override AtomAcceptedMediaRange CreateGreaterInstance()
        => new() { MediaRange = "text/xml" };
}