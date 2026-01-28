using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class TrackbackResponseExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<TrackbackResponse>
{
    protected override TrackbackResponse CreateInstance()
        => new();

    protected override TrackbackResponse CreateLesserInstance()
        => new();

    protected override TrackbackResponse CreateGreaterInstance()
        => new("Error occurred");
}
