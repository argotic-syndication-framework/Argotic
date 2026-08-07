using Argotic.Syndication.Specialized;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Advertising an editing endpoint to a desktop blogging client, and reading the advertisement back.
/// </summary>
/// <remarks>
///     <para>
///     An RSD <c>api</c> element carries four attributes and one optional <c>settings</c> block. The
///     attributes name the endpoint; the block is where a service says anything a client could not guess —
///     where its documentation lives, what a human should know before pointing a client at it, and the
///     service-specific key/value pairs the protocol itself does not define.
///     </para>
///     <para>
///     That block did not survive a round trip. <c>RsdApplicationInterface.WriteTo</c> emitted
///     <c>settings</c> directly under <c>api</c>, which is where RSD 1.0 puts it, while <c>Load</c> looked
///     for it at <c>rsd:api/rsd:settings</c> — relative to a navigator already standing on the <c>api</c>
///     element, so an <c>api</c> inside an <c>api</c>. The library could not read its own output, and could
///     not read a conformant document either. Because the four attributes loaded fine, a document came back
///     looking populated with the half that mattered missing.
///     </para>
/// </remarks>
[TestClass]
public class PublishAnRsdDocumentForABlogClient
{
    /// <summary>
    /// An application interface written with documentation, notes and settings arrives with all three.
    /// </summary>
    [TestMethod]
    public void AnApplicationInterfaceCarryingSettings_SurvivesBeingWrittenAndReadBack()
    {
        RsdDocument originalDocument = new()
        {
            EngineName = "Settings CMS",
            EngineLink = new Uri("http://settings.example.com/"),
            Homepage = new Uri("http://settings.example.com/blog"),
        };
        RsdApplicationInterface api = new("Conversant", new Uri("http://example.com/api"), true, "blog123")
        {
            Documentation = new Uri("http://example.com/docs/"),
            Notes = "Test notes for round trip",
        };
        api.Settings.Add("setting1", "value1");
        api.Settings.Add("setting2", "value2");
        originalDocument.Interfaces.Add(api);

        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        RsdDocument loadedDocument = new();
        loadedDocument.Load(stream);

        loadedDocument.Interfaces.Count.ShouldBe(1);
        RsdApplicationInterface loadedApi = loadedDocument.Interfaces[0];
        loadedApi.Name.ShouldBe(api.Name);
        loadedApi.Link.ShouldBe(api.Link);
        loadedApi.IsPreferred.ShouldBe(api.IsPreferred);
        loadedApi.WeblogId.ShouldBe(api.WeblogId);

        loadedApi.Documentation.ShouldBe(api.Documentation);
        loadedApi.Notes.ShouldBe(api.Notes);
        loadedApi.Settings.Count.ShouldBe(2);
        loadedApi.Settings["setting1"].ShouldBe("value1");
        loadedApi.Settings["setting2"].ShouldBe("value2");
    }

    /// <summary>
    /// The <c>settings</c> element is written as a direct child of <c>api</c>, which is where RSD 1.0
    /// puts it and where the loader now looks for it.
    /// </summary>
    /// <remarks>
    ///     The write side was always right; it was the read side that disagreed with it. Pinning the
    ///     document shape as well as the round trip is what stops the pair being "fixed" back into
    ///     agreement at the wrong nesting, which would round-trip perfectly and interoperate with nothing.
    /// </remarks>
    [TestMethod]
    public void AnApplicationInterface_NestsItsSettingsDirectlyUnderTheApiElement()
    {
        RsdDocument document = new()
        {
            EngineName = "Settings CMS",
            EngineLink = new Uri("http://settings.example.com/"),
            Homepage = new Uri("http://settings.example.com/blog"),
        };
        RsdApplicationInterface api = new("Conversant", new Uri("http://example.com/api"), true, "blog123")
        {
            Documentation = new Uri("http://example.com/docs/"),
        };
        api.Settings.Add("auth-type", "oauth2");
        document.Interfaces.Add(api);

        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldContain("<api ", Case.Sensitive);
        xml.ShouldContain("<settings>", Case.Sensitive);
        xml.ShouldNotContain("<api><api", Case.Sensitive);
        xml.IndexOf("<settings>", StringComparison.Ordinal)
            .ShouldBeGreaterThan(xml.IndexOf("<api ", StringComparison.Ordinal));
    }
}