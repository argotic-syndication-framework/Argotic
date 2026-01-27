using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationRequestOptionsTests
{
    [TestMethod]
    public void ApplyTo_SetsAcceptHeader()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Accept = "application/xml"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ToString().ShouldBe("application/xml");
    }

    [TestMethod]
    public void ApplyTo_SetsUserAgentHeader()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            UserAgent = "TestApp/1.0"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.UserAgent.ToString().ShouldBe("TestApp/1.0");
    }

    [TestMethod]
    public void ApplyTo_SetsRefererHeader()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Referer = "http://example.com/source"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Referrer.ShouldNotBeNull();
        request.Headers.Referrer.ToString().ShouldBe("http://example.com/source");
    }

    [TestMethod]
    public void ApplyTo_WithInvalidReferer_DoesNotThrow()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Referer = "not a valid uri"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Should not throw
        Should.NotThrow(() => options.ApplyTo(request));

        // Referrer should remain null
        request.Headers.Referrer.ShouldBeNull();
    }

    [TestMethod]
    public void ApplyTo_WithRelativeReferer_SetsFileUri()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Referer = "/relative/path"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        // Uri.TryCreate with UriKind.Absolute interprets /relative/path as a file:// URI
        request.Headers.Referrer.ShouldNotBeNull();
        request.Headers.Referrer!.Scheme.ShouldBe("file");
    }

    [TestMethod]
    public void ApplyTo_SetsCustomHeaders()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "custom-value",
                ["X-Another-Header"] = "another-value"
            }
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.GetValues("X-Custom-Header").ShouldContain("custom-value");
        request.Headers.GetValues("X-Another-Header").ShouldContain("another-value");
    }

    [TestMethod]
    public void ApplyTo_DoesNotOverwriteExistingCustomHeaders()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "new-value"
            }
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        request.Headers.TryAddWithoutValidation("X-Custom-Header", "existing-value");

        options.ApplyTo(request);

        // Should keep the existing value, not overwrite
        List<string> values = request.Headers.GetValues("X-Custom-Header").ToList();
        values.ShouldContain("existing-value");
        values.ShouldNotContain("new-value");
    }

    [TestMethod]
    public void ApplyTo_WithNullRequest_ThrowsArgumentNullException()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions();

        Should.Throw<ArgumentNullException>(() => options.ApplyTo(null!));
    }

    [TestMethod]
    public void ApplyTo_WithAllNullProperties_DoesNotModifyRequest()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ShouldBeEmpty();
        request.Headers.UserAgent.ShouldBeEmpty();
        request.Headers.Referrer.ShouldBeNull();
    }

    [TestMethod]
    public void ApplyTo_WithAllPropertiesSet_SetsAllHeaders()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Accept = "application/rss+xml",
            UserAgent = "Argotic/1.0",
            Referer = "http://example.com/referrer",
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Test"] = "test-value"
            }
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.ToString().ShouldBe("application/rss+xml");
        request.Headers.UserAgent.ToString().ShouldBe("Argotic/1.0");
        request.Headers.Referrer!.ToString().ShouldBe("http://example.com/referrer");
        request.Headers.GetValues("X-Test").ShouldContain("test-value");
    }

    [TestMethod]
    public void ApplyTo_WithEmptyReferer_DoesNotSetHeader()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Referer = ""
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Empty string is not null, but TryCreate will fail for empty string
        options.ApplyTo(request);

        request.Headers.Referrer.ShouldBeNull();
    }

    [TestMethod]
    public void ApplyTo_AcceptWithMultipleTypes_SetsAllTypes()
    {
        SyndicationRequestOptions options = new SyndicationRequestOptions
        {
            Accept = "application/xml, application/rss+xml"
        };
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        options.ApplyTo(request);

        request.Headers.Accept.Count.ShouldBe(2);
    }

    [TestMethod]
    public void Record_WithExpression_CreatesModifiedCopy()
    {
        SyndicationRequestOptions original = new SyndicationRequestOptions
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
        SyndicationRequestOptions options1 = new SyndicationRequestOptions
        {
            Accept = "application/xml",
            UserAgent = "TestAgent/1.0"
        };
        SyndicationRequestOptions options2 = new SyndicationRequestOptions
        {
            Accept = "application/xml",
            UserAgent = "TestAgent/1.0"
        };

        options1.ShouldBe(options2);
        (options1 == options2).ShouldBeTrue();
    }
}