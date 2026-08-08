using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers what <c>ConditionalGetResult</c> projects from a response, and what it does once disposed.
/// </summary>
/// <remarks>
///     Both arms of the type are exercised through one reflective helper: a result built over a
///     response, which is what a <c>200</c> produces, and a result built over none, which is the shape
///     the caller sees for a <c>304</c>. The constructors are internal and this assembly has no
///     <c>InternalsVisibleTo</c> — see the remarks on <c>CreateResult</c> for what that costs.
/// </remarks>
[TestClass]
public class ConditionalGetResultTests
{
    /// <summary>
    /// A result built over a response reports the resource as modified.
    /// </summary>
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

    /// <summary>
    /// A result built with no response reports the resource as unchanged.
    /// </summary>
    [TestMethod]
    public void WasModified_WhenNotModified_ReturnsFalse()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        result.WasModified.ShouldBeFalse();
    }

    /// <summary>
    /// The status the origin sent is carried through to the caller rather than inferred.
    /// </summary>
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

    /// <summary>
    /// A result built with no response reports <see langword="null"/> for the status code.
    /// </summary>
    [TestMethod]
    public void StatusCode_WhenNotModified_ReturnsDefault()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        result.StatusCode.ShouldBe(default);
    }

    /// <summary>
    /// The <c>Last-Modified</c> the origin sent is surfaced with its offset intact.
    /// </summary>
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

    /// <summary>
    /// A response carrying no <c>Last-Modified</c> reports <see langword="null"/>, not a sentinel date.
    /// </summary>
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

    /// <summary>
    /// The entity tag is reported with its quotes intact, which is how it has to be sent back.
    /// </summary>
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

    /// <summary>
    /// A response carrying no <c>ETag</c> reports <see langword="null"/> rather than an empty tag.
    /// </summary>
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

    /// <summary>
    /// A response that declares a length reports it.
    /// </summary>
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

    /// <summary>
    /// A result built with no response reports a content length of <c>0</c>.
    /// </summary>
    /// <remarks>
    ///     Worth pinning because it is the one place the property is not the origin's answer: with no
    ///     response the constructor never assigns it, so this is the field default rather than the
    ///     <c>-1</c> that a real response declaring no length reports.
    /// </remarks>
    [TestMethod]
    public void ContentLength_WhenNullResponse_ReturnsDefault()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        // When response is null, ContentLength is never set, so it returns the default value (0)
        result.ContentLength.ShouldBe(0);
    }

    /// <summary>
    /// The media type is reported on its own, without the <c>charset</c> parameter beside it.
    /// </summary>
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

    /// <summary>
    /// A response carrying no <c>Content-Type</c> reports <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// The body accessor hands back a stream over the response content, byte for byte.
    /// </summary>
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

    /// <summary>
    /// The asynchronous body accessor delivers the same content as the synchronous one.
    /// </summary>
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

    /// <summary>
    /// A result with no response hands back <c>Stream.Null</c> rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void GetResponseStream_WhenNotModified_ReturnsStreamNull()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        using Stream stream = result.GetResponseStream();

        stream.ShouldBe(Stream.Null);
    }

    /// <summary>
    /// The asynchronous accessor also hands back <c>Stream.Null</c> when there is no response.
    /// </summary>
    [TestMethod]
    public async Task GetResponseStreamAsync_WhenNotModified_ReturnsStreamNull()
    {
        using ConditionalGetResult result = CreateResult(null, wasModified: false);

        using Stream stream = await result.GetResponseStreamAsync(TestContext.CancellationToken);

        stream.ShouldBe(Stream.Null);
    }

    /// <summary>
    /// Disposing a result that owns a response is idempotent: a second call is a no-op, not a throw.
    /// </summary>
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

        // The method had no Should* call on any path, so the only failure it could express was an
        // unhandled throw — a ConditionalGetResult that leaked the response entirely passed it. The
        // response is observed instead: disposal must have reached the content it owns.
        Should.Throw<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.CancellationToken));

        // And a second call is a no-op rather than a throw.
        Should.NotThrow(result.Dispose);
    }

    /// <summary>
    /// Reaching for the body after disposal throws rather than handing out a dead stream.
    /// </summary>
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

    /// <summary>
    /// The asynchronous dispose closes the object just as firmly as the synchronous one.
    /// </summary>
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
    /// <remarks>
    ///     <para>
    ///     All nineteen tests in this class route through here, and the signature is hard-coded. Adding
    ///     a parameter to that constructor makes <c>GetConstructor</c> return null — and a
    ///     <c>constructor!</c> would then throw <see cref="NullReferenceException"/> nineteen times
    ///     with no compile-time signal and nothing naming the cause.
    ///     </para>
    ///     <para>
    ///     The null-forgiving operator is what made that failure mode silent, so it is replaced by an
    ///     assertion that says what went wrong. <c>ConditionalGetResult</c> is at 100% line and 95.5%
    ///     branch — the best-covered type in this modernisation — and that coverage rests entirely on
    ///     one reflective lookup that the compiler cannot check.
    ///     </para>
    /// </remarks>
    private static ConditionalGetResult CreateResult(HttpResponseMessage? response, bool wasModified)
    {
        ConstructorInfo? constructor = typeof(ConditionalGetResult).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [typeof(HttpResponseMessage), typeof(bool)],
            null);

        constructor.ShouldNotBeNull(
            "ConditionalGetResult(HttpResponseMessage?, bool) was not found. Its signature has changed, "
            + "and every test in this class binds to it by reflection — update this helper to match.");

        return (ConditionalGetResult)constructor.Invoke([response, wasModified]);
    }

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; }
}