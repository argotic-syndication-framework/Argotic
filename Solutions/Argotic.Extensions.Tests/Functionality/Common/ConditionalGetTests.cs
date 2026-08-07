using System.Net;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the validator a conditional GET puts on the wire, and what it makes of the reply.
/// </summary>
/// <remarks>
///     Four outcomes, and only one of them is an exception: a <c>304</c> and a <c>200</c> are both
///     results, and an error status is a throw. That asymmetry is what distinguishes this fetch from
///     every other one in the library, so it is pinned here rather than assumed.
/// </remarks>
[TestClass]
public class ConditionalGetTests
{
    /// <summary>
    /// Gets or sets the context under which the current test is running.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// A validator of <c>Unspecified</c> kind is sent as <c>If-Modified-Since</c> at UTC, unshifted.
    /// </summary>
    [TestMethod]
    public async Task ConditionalGetAsync_SendsTheSuppliedInstantAsIfModifiedSince()
    {
        // Arrange
        // The validator the caller holds came from a Last-Modified header and is therefore a UTC clock
        // reading; sending it as if it were local time shifts the condition by the machine's offset.
        DateTime lastModified = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Unspecified);
        DateTimeOffset? sent = null;

        using MockHttpMessageHandler handler = new((request, _) =>
        {
            sent = request.Headers.IfModifiedSince;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient httpClient = new(handler);

        // Act
        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            new Uri("http://example.com/feed.xml"), lastModified, null, httpClient, TestContext.CancellationToken);

        // Assert
        sent.ShouldNotBeNull();
        sent.Value.ShouldBe(new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero));
    }

    /// <summary>
    /// A <c>304</c> is delivered as a result reporting the resource unchanged, not as an exception.
    /// </summary>
    [TestMethod]
    public async Task ConditionalGetAsync_WithNotModifiedResponse_ReportsUnmodified()
    {
        // Arrange
        using MockHttpMessageHandler handler = new((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified)));
        using HttpClient httpClient = new(handler);

        // Act
        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            new Uri("http://example.com/feed.xml"),
            new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            null,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        result.WasModified.ShouldBeFalse();
    }

    /// <summary>
    /// A <c>404</c> throws <c>HttpRequestException</c> rather than passing for a cache hit.
    /// </summary>
    [TestMethod]
    public async Task ConditionalGetAsync_WithErrorStatus_ThrowsHttpRequestException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await SyndicationDiscoveryUtility.ConditionalGetAsync(
                new Uri("http://example.com/feed.xml"),
                new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                null,
                httpClient,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A <c>200</c> carrying a <c>Last-Modified</c> newer than the one sent is reported as modified.
    /// </summary>
    [TestMethod]
    public async Task ConditionalGetAsync_WithNewerContent_ReportsModified()
    {
        // Arrange
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("<rss version=\"2.0\"><channel /></rss>"),
            };
            response.Content.Headers.LastModified = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
            return Task.FromResult(response);
        });
        using HttpClient httpClient = new(handler);

        // Act
        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            new Uri("http://example.com/feed.xml"),
            new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            null,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        result.WasModified.ShouldBeTrue();
    }
}