using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class TrackbackMessageExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<TrackbackMessage>
{
    protected override TrackbackMessage CreateInstance()
        => new(new Uri("http://example.com/post/2")) { Title = "Post 2" };

    protected override TrackbackMessage CreateLesserInstance()
        => new(new Uri("http://example.com/post/1")) { Title = "Post 1" };

    protected override TrackbackMessage CreateGreaterInstance()
        => new(new Uri("http://example.com/post/3")) { Title = "Post 3" };
}