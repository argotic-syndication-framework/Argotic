namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers what <c>SyndicationRequestOptions</c> puts on a request, and what it refuses to send at all.
/// </summary>
/// <remarks>
///     Several rows here record a deliberate reversal, and each says so at its assertion: a value that
///     cannot be sent used to leave the header quietly absent, and now throws. The failure a silent
///     drop produces is a <c>406</c> from a server the caller has no reason to suspect, which is why
///     the noisy option was chosen.
/// </remarks>
[TestClass]
public class SyndicationRequestOptionsTests
{
    /// <summary>
    /// An <c>Accept</c> value reaches the request's <c>Accept</c> header.
    /// </summary>
    [TestMethod]
    public void ApplyTo_SetsAcceptHeader()
    {
        SyndicationRequestOptions options = new()
        {
            Accept = "application/xml"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ToString().ShouldBe("application/xml");
    }

    /// <summary>
    /// A <c>UserAgent</c> value reaches the request's <c>User-Agent</c> header.
    /// </summary>
    [TestMethod]
    public void ApplyTo_SetsUserAgentHeader()
    {
        SyndicationRequestOptions options = new()
        {
            UserAgent = "TestApp/1.0"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.UserAgent.ToString().ShouldBe("TestApp/1.0");
    }

    /// <summary>
    /// An absolute <c>http</c> referer reaches the request's <c>Referer</c> header.
    /// </summary>
    [TestMethod]
    public void ApplyTo_SetsRefererHeader()
    {
        SyndicationRequestOptions options = new()
        {
            Referer = "http://example.com/source"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Referrer.ShouldNotBeNull();
        request.Headers.Referrer.ToString().ShouldBe("http://example.com/source");
    }

    /// <summary>
    /// A referer that is not a URI throws, and the message names the value the caller wrote.
    /// </summary>
    /// <remarks>
    ///     This row used to assert the opposite — no throw, and the header simply left unset. Quoting
    ///     the offending value back is what makes the exception actionable, since the alternative
    ///     surfaces as a status code from a server that had nothing to do with the mistake.
    /// </remarks>
    [TestMethod]
    public void ApplyTo_WithInvalidReferer_Throws()
    {
        SyndicationRequestOptions options = new()
        {
            Referer = "not a valid uri"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        // Was ShouldNotThrow with the header left unset. A value that cannot be sent is a mistake in
        // the caller's configuration, and reporting it as a missing header defers the symptom to a
        // 406 from a server they have no reason to suspect.
        Should.Throw<FormatException>(() => options.ApplyTo(request))
            .Message.ShouldContain("not a valid uri");
    }

    /// <summary>
    /// A relative referer throws, and no header is sent in its place.
    /// </summary>
    /// <remarks>
    ///     The row that was worse than a silent drop. <c>Uri.TryCreate(…, Absolute, …)</c> accepts
    ///     <c>/relative/path</c> on this platform and yields <c>file:///relative/path</c>, so the old
    ///     behaviour disclosed a local-looking path to a remote origin in a header the caller believed
    ///     pointed at a page.
    /// </remarks>
    [TestMethod]
    public void ApplyTo_WithRelativeReferer_Throws()
    {
        SyndicationRequestOptions options = new()
        {
            Referer = "/relative/path"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        // Was ShouldBe("file"). Uri.TryCreate(..., Absolute, ...) accepts /relative/path on this
        // platform and produces file:///relative/path, which was then sent to a remote origin -- a
        // local-looking path disclosed in a header the caller thought pointed at a page.
        Should.Throw<FormatException>(() => options.ApplyTo(request));
        request.Headers.Referrer.ShouldBeNull();
    }

    /// <summary>
    /// An <c>Accept</c> of <c>@@@</c> throws rather than being dropped on the way to the wire.
    /// </summary>
    [TestMethod]
    public void ApplyTo_WithMalformedAccept_Throws()
    {
        SyndicationRequestOptions options = new() { Accept = "@@@" };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        Should.Throw<FormatException>(() => options.ApplyTo(request));
    }

    /// <summary>
    /// An <c>Accept</c> whose q-value is not a number is still sent, as one media type.
    /// </summary>
    /// <remarks>
    ///     The control for the row above: without it, the malformed case is equally consistent with
    ///     "the parser rejects anything unusual", and the header parser is looser than it looks.
    /// </remarks>
    [TestMethod]
    public void ApplyTo_WithATolerableAccept_StillSetsIt()
    {
        // The control, and a reminder that the parser is looser than it looks: a q-value that is not
        // a number passes, so ParseAdd rejects less than one might assume. Without this row, the
        // test above is equally consistent with "ParseAdd rejects anything unusual".
        SyndicationRequestOptions options = new() { Accept = "application/xml; q=not-a-number" };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.Count.ShouldBe(1);
    }

    /// <summary>
    /// A custom header naming a content header throws a message that names <c>Content-Type</c>.
    /// </summary>
    /// <remarks>
    ///     This already threw before the guard was rewritten, which the plan did not record:
    ///     <c>request.Headers.Contains</c> raises on a content header name, so the check written to
    ///     avoid clobbering an existing header was itself the failure — and the platform message talked
    ///     about <c>HttpContent</c> while naming nothing the caller had written.
    /// </remarks>
    [TestMethod]
    public void ApplyTo_WithAContentHeaderInCustomHeaders_ThrowsSomethingLegible()
    {
        SyndicationRequestOptions options = new()
        {
            CustomHeaders = new Dictionary<string, string> { ["Content-Type"] = "application/xml" }
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        // This already threw, which the plan did not record -- request.Headers.Contains raises
        // InvalidOperationException on a content header name, so the guard written to avoid
        // clobbering an existing header was itself the failure. The BCL message talks about
        // HttpContent and names nothing the caller wrote.
        Should.Throw<FormatException>(() => options.ApplyTo(request))
            .Message.ShouldContain("Content-Type");
    }

    /// <summary>
    /// Every custom header pair reaches the request, not just the first.
    /// </summary>
    [TestMethod]
    public void ApplyTo_SetsCustomHeaders()
    {
        SyndicationRequestOptions options = new()
        {
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "custom-value",
                ["X-Another-Header"] = "another-value"
            }
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.GetValues("X-Custom-Header").ShouldContain("custom-value");
        request.Headers.GetValues("X-Another-Header").ShouldContain("another-value");
    }

    /// <summary>
    /// A custom header the request already carries is skipped, leaving the original value alone.
    /// </summary>
    /// <remarks>
    ///     This is what stops custom headers displacing the conditional-GET validators or the
    ///     <c>User-Agent</c>, both of which are set on the request before the options are applied.
    /// </remarks>
    [TestMethod]
    public void ApplyTo_DoesNotOverwriteExistingCustomHeaders()
    {
        SyndicationRequestOptions options = new()
        {
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "new-value"
            }
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");
        request.Headers.TryAddWithoutValidation("X-Custom-Header", "existing-value");

        options.ApplyTo(request);

        // Should keep the existing value, not overwrite
        List<string> values = [.. request.Headers.GetValues("X-Custom-Header")];
        values.ShouldContain("existing-value");
        values.ShouldNotContain("new-value");
    }

    /// <summary>
    /// A null request is rejected with <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void ApplyTo_WithNullRequest_ThrowsArgumentNullException()
    {
        SyndicationRequestOptions options = new();

        Should.Throw<ArgumentNullException>(() => options.ApplyTo(null!));
    }

    /// <summary>
    /// Options with nothing set leave the request's headers exactly as they were.
    /// </summary>
    [TestMethod]
    public void ApplyTo_WithAllNullProperties_DoesNotModifyRequest()
    {
        SyndicationRequestOptions options = new();
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ShouldBeEmpty();
        request.Headers.UserAgent.ShouldBeEmpty();
        request.Headers.Referrer.ShouldBeNull();
    }

    /// <summary>
    /// Accept, User-Agent, Referer and a custom header are all applied in a single pass.
    /// </summary>
    [TestMethod]
    public void ApplyTo_WithAllPropertiesSet_SetsAllHeaders()
    {
        SyndicationRequestOptions options = new()
        {
            Accept = "application/rss+xml",
            UserAgent = "Argotic/1.0",
            Referer = "http://example.com/referrer",
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Test"] = "test-value"
            }
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ToString().ShouldBe("application/rss+xml");
        request.Headers.UserAgent.ToString().ShouldBe("Argotic/1.0");
        request.Headers.Referrer!.ToString().ShouldBe("http://example.com/referrer");
        request.Headers.GetValues("X-Test").ShouldContain("test-value");
    }

    /// <summary>
    /// An <i>empty</i> referer means "send none", and is not treated as a malformed URI.
    /// </summary>
    [TestMethod]
    public void ApplyTo_WithEmptyReferer_DoesNotSetHeader()
    {
        SyndicationRequestOptions options = new()
        {
            Referer = ""
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        // Empty string is not null, but TryCreate will fail for empty string
        options.ApplyTo(request);

        request.Headers.Referrer.ShouldBeNull();
    }

    /// <summary>
    /// A comma-separated <c>Accept</c> becomes one header value per media type, not one long string.
    /// </summary>
    [TestMethod]
    public void ApplyTo_AcceptWithMultipleTypes_SetsAllTypes()
    {
        SyndicationRequestOptions options = new()
        {
            Accept = "application/xml, application/rss+xml"
        };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.Count.ShouldBe(2);
    }

    /// <summary>
    /// A <c>with</c> expression carries the untouched members across and leaves the original intact.
    /// </summary>
    [TestMethod]
    public void Record_WithExpression_CreatesModifiedCopy()
    {
        SyndicationRequestOptions original = new()
        {
            Accept = "application/xml",
            UserAgent = "OriginalAgent/1.0"
        };

        SyndicationRequestOptions modified = original with { UserAgent = "ModifiedAgent/2.0" };

        modified.Accept.ShouldBe("application/xml");
        modified.UserAgent.ShouldBe("ModifiedAgent/2.0");
        original.UserAgent.ShouldBe("OriginalAgent/1.0");
    }

    /// <summary>
    /// Two option sets holding the same values compare equal, by value rather than by reference.
    /// </summary>
    [TestMethod]
    public void Record_Equality_WorksCorrectly()
    {
        SyndicationRequestOptions options1 = new()
        {
            Accept = "application/xml",
            UserAgent = "TestAgent/1.0"
        };
        SyndicationRequestOptions options2 = new()
        {
            Accept = "application/xml",
            UserAgent = "TestAgent/1.0"
        };

        options1.ShouldBe(options2);
        (options1 == options2).ShouldBeTrue();
    }
}