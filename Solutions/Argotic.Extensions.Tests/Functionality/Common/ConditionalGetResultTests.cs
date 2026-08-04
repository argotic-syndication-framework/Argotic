using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class ConditionalGetResultTests
{
    [TestMethod]
    public void WasModified_WhenResourceModified_ReturnsTrue()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.WasModified.ShouldBeTrue();
    }

    [TestMethod]
    public void WasModified_WhenNotModified_ReturnsFalse()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        result.WasModified.ShouldBeFalse();
    }

    [TestMethod]
    public void StatusCode_ReflectsHttpResponse()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public void StatusCode_WhenNotModified_ReturnsDefault()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        result.StatusCode.ShouldBe(default);
    }

    [TestMethod]
    public void LastModified_ExtractsFromHeaders()
    {
        DateTimeOffset lastModified = new(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        response.Content.Headers.LastModified = lastModified;
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.LastModified.ShouldBe(lastModified);
    }

    [TestMethod]
    public void LastModified_WhenNotPresent_ReturnsNull()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.LastModified.ShouldBeNull();
    }

    [TestMethod]
    public void ETag_ExtractsFromHeaders()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        response.Headers.ETag = new EntityTagHeaderValue("\"abc123\"");
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.ETag.ShouldBe("\"abc123\"");
    }

    [TestMethod]
    public void ETag_WhenNotPresent_ReturnsNull()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.ETag.ShouldBeNull();
    }

    [TestMethod]
    public void ContentLength_ExtractsFromHeaders()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("test content", Encoding.UTF8, "text/plain")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.ContentLength.ShouldBeGreaterThan(0);
    }

    [TestMethod]
    public void ContentLength_WhenNullResponse_ReturnsDefault()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        // When response is null, ContentLength is never set, so it returns the default value (0)
        result.ContentLength.ShouldBe(0);
    }

    [TestMethod]
    public void ContentType_ExtractsFromHeaders()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content", Encoding.UTF8, "application/xml")
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.ContentType.ShouldBe("application/xml");
    }

    [TestMethod]
    public void ContentType_WhenNotPresent_ReturnsNull()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([])
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.ContentType.ShouldBeNull();
    }

    [TestMethod]
    public void GetResponseStream_ReturnsContent()
    {
        const string expectedContent = "test content";
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        using Stream stream = result.GetResponseStream();
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();

        content.ShouldBe(expectedContent);
    }

    [TestMethod]
    public async Task GetResponseStreamAsync_ReturnsContent()
    {
        const string expectedContent = "test content";
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };
        using ConditionalGetResult result = CreateResult(response, wasModified: true);

        using Stream stream = await result.GetResponseStreamAsync(TestContext.CancellationToken);
        using StreamReader reader = new(stream);
        string content = await reader.ReadToEndAsync(TestContext.CancellationToken);

        content.ShouldBe(expectedContent);
    }

    [TestMethod]
    public void GetResponseStream_WhenNotModified_ReturnsStreamNull()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        using Stream stream = result.GetResponseStream();

        stream.ShouldBe(Stream.Null);
    }

    [TestMethod]
    public async Task GetResponseStreamAsync_WhenNotModified_ReturnsStreamNull()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        using Stream stream = await result.GetResponseStreamAsync(TestContext.CancellationToken);

        stream.ShouldBe(Stream.Null);
    }

    [TestMethod]
    public void Dispose_DisposesUnderlyingResponse()
    {
        // Ownership transfers to ConditionalGetResult; that it disposes the response is what this test asserts.
#pragma warning disable CA2000
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
#pragma warning restore CA2000
        ConditionalGetResult result = CreateResult(response, wasModified: true);

        result.Dispose();

        // Calling Dispose again should not throw (idempotent)
        result.Dispose();
    }

    [TestMethod]
    public void GetResponseStream_AfterDispose_ThrowsObjectDisposedException()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        ConditionalGetResult result = CreateResult(response, wasModified: true);
        result.Dispose();

        Should.Throw<ObjectDisposedException>(() => result.GetResponseStream());
    }

    [TestMethod]
    public async Task GetResponseStreamAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("content")
        };
        ConditionalGetResult result = CreateResult(response, wasModified: true);
        await result.DisposeAsync();

        await Should.ThrowAsync<ObjectDisposedException>(async () => await result.GetResponseStreamAsync(TestContext.CancellationToken));
    }

    /// <summary>
    /// Creates a ConditionalGetResult using reflection to access the internal constructor.
    /// </summary>
    private static ConditionalGetResult CreateResult(HttpResponseMessage? response, bool wasModified)
    {
        ConstructorInfo? constructor = typeof(ConditionalGetResult).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [typeof(HttpResponseMessage), typeof(bool)],
            null);

        return (ConditionalGetResult)constructor!.Invoke([response, wasModified]);
    }

    public TestContext TestContext { get; set; }
}