using System.Net;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Tests for <see cref="SyndicationDiscoveryUtility.ConditionalGetAsync(Uri, DateTime, string, HttpClient, CancellationToken)"/>.
/// </summary>
[TestClass]
public class ConditionalGetTests
{
    /// <summary>
    /// Gets or sets the context under which the current test is running.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

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