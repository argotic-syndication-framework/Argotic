using System.Text;
using System.Xml;
using Argotic.Publishing;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// Tests that every <c>Load</c> entry point on <see cref="AtomEntryResource"/> populates the
/// Atom Publishing Protocol members, not just the overloads the class chooses to shadow.
/// </summary>
[TestClass]
public class AtomEntryResourceLoadTests
{
    private const string PublishingEntry = """
        <?xml version="1.0" encoding="utf-8"?>
        <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
          <id>urn:uuid:0a1b2c3d</id>
          <title>A draft entry</title>
          <updated>2024-01-15T12:00:00Z</updated>
          <app:edited>2024-02-20T08:30:00Z</app:edited>
          <app:control><app:draft>yes</app:draft></app:control>
        </entry>
        """;

    private static readonly DateTime ExpectedEditedOn = new(2024, 2, 20, 8, 30, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Load_FromStream_PopulatesPublishingMembers()
    {
        // Arrange
        AtomEntryResource entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(PublishingEntry));

        // Act
        entry.Load(stream);

        // Assert
        entry.EditedOn.ShouldBe(ExpectedEditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_FromXmlReader_PopulatesPublishingMembers()
    {
        // Arrange
        AtomEntryResource entry = new();
        using StringReader stringReader = new(PublishingEntry);
        using XmlReader reader = XmlReader.Create(stringReader);

        // Act
        entry.Load(reader);

        // Assert
        entry.EditedOn.ShouldBe(ExpectedEditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_FromNavigable_PopulatesPublishingMembers()
    {
        // Arrange
        AtomEntryResource entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(PublishingEntry));
        var navigator = Argotic.Common.SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Act
        entry.Load(navigator);

        // Assert
        entry.EditedOn.ShouldBe(ExpectedEditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_ThenSave_RoundTripsThePublishingMembers()
    {
        // Arrange
        AtomEntryResource entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(PublishingEntry));
        entry.Load(stream);

        // Act
        StringBuilder builder = new();
        using (XmlWriter writer = XmlWriter.Create(builder))
        {
            entry.Save(writer);
        }

        string saved = builder.ToString();

        // Assert
        saved.ShouldContain("edited");
        saved.ShouldContain("draft");
    }
}
