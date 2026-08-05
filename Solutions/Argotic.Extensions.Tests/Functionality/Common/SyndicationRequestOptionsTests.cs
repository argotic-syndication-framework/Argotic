using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationRequestOptionsTests
{
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

    [TestMethod]
    public void ApplyTo_WithMalformedAccept_Throws()
    {
        SyndicationRequestOptions options = new() { Accept = "@@@" };
        using HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");

        Should.Throw<FormatException>(() => options.ApplyTo(request));
    }

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

    [TestMethod]
    public void ApplyTo_WithNullRequest_ThrowsArgumentNullException()
    {
        SyndicationRequestOptions options = new();

        Should.Throw<ArgumentNullException>(() => options.ApplyTo(null!));
    }

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