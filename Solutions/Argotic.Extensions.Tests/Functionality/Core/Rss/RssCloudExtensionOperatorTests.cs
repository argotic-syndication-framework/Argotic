using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

[TestClass]
public class RssCloudExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<RssCloud>
{
    protected override RssCloud CreateInstance()
        => new() { Domain = "cloud-b.example.com" };

    protected override RssCloud CreateLesserInstance()
        => new() { Domain = "cloud-a.example.com" };

    protected override RssCloud CreateGreaterInstance()
        => new() { Domain = "cloud-c.example.com" };
}