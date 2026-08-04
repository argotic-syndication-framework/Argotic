using System.Net;
using System.Text;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// A mock <see cref="HttpMessageHandler"/> for testing HTTP operations.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="sendAsync">The function to handle requests.</param>
    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _sendAsync(request, cancellationToken);

    /// <summary>
    /// Creates a handler that returns the specified content.
    /// </summary>
    /// <param name="content">The content to return.</param>
    /// <param name="contentType">The content type. Defaults to application/xml.</param>
    /// <returns>A new <see cref="MockHttpMessageHandler"/>.</returns>
    public static MockHttpMessageHandler WithContent(string content, string contentType = "application/xml")
    {
        return new MockHttpMessageHandler((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            });
        });
    }

    /// <summary>
    /// Creates a handler that returns content after a delay.
    /// </summary>
    /// <param name="delay">The delay before returning content.</param>
    /// <param name="content">The content to return.</param>
    /// <returns>A new <see cref="MockHttpMessageHandler"/>.</returns>
    public static MockHttpMessageHandler WithDelay(TimeSpan delay, string content)
    {
        return new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(delay, ct);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/xml")
            };
        });
    }

    /// <summary>
    /// Creates a handler that returns a 404 Not Found response.
    /// </summary>
    /// <returns>A new <see cref="MockHttpMessageHandler"/>.</returns>
    public static MockHttpMessageHandler WithNotFound()
    {
        return new MockHttpMessageHandler((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
            });
        });
    }

    /// <summary>
    /// Creates a handler that throws an exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>A new <see cref="MockHttpMessageHandler"/>.</returns>
    public static MockHttpMessageHandler WithException(Exception exception)
    {
        return new MockHttpMessageHandler((_, _) =>
        {
            throw exception;
        });
    }
}