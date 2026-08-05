using System.Globalization;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Writing and re-reading a feed in which every optional element is populated.
/// </summary>
/// <remarks>
///     <para>
///     Of the 627 one-sided guards in this library - branches whose failure path has never fired - the
///     larger share are not tolerance code at all. They are shaped like
///     <c>this.Cloud?.WriteTo(writer)</c> and <c>if (this.Source is not null)</c>, and they are cold
///     for one reason: no fixture ever populated the element.
///     </para>
///     <para>
///     The synthetic corpus omits four channel elements the real sample has - cloud, textInput,
///     skipHours and skipDays - and the inline literals average ten XML elements per document. So the
///     write side of each of those elements had never executed, in either direction.
///     </para>
/// </remarks>
[TestClass]
public class RoundTripAMaximalFeed
{
    /// <summary>
    /// An RSS channel with every optional element survives being written and read back.
    /// </summary>
    [TestMethod]
    public void AMaximalRssChannel_SurvivesBeingWrittenAndReadBack()
    {
        RssFeed written = BuildMaximalRssFeed();

        RssChannel channel = SaveAndReload(written).Channel;

        channel.Title.ShouldBe("A maximal feed");
        channel.Description.ShouldBe("Every optional element populated");
        channel.Link.ShouldBe(new Uri("http://example.com/"));
        channel.Copyright.ShouldBe("Copyright 2024");
        channel.Generator.ShouldBe("Argotic");
        channel.ManagingEditor.ShouldBe("editor@example.com");
        channel.Webmaster.ShouldBe("webmaster@example.com");
        channel.Language?.Name.ShouldBe("en-GB");
        channel.TimeToLive.ShouldBe(60);
        channel.SelfLink.ShouldBe(new Uri("http://example.com/feed.xml"));

        channel.Cloud.ShouldNotBeNull();
        channel.Cloud.Domain.ShouldBe("rpc.example.com");
        channel.Cloud.Path.ShouldBe("/RPC2");
        channel.Cloud.Port.ShouldBe(80);
        channel.Cloud.RegisterProcedure.ShouldBe("cloud.notify");

        channel.Image.ShouldNotBeNull();
        channel.Image.Title.ShouldBe("A logo");
        channel.Image.Url.ShouldBe(new Uri("http://example.com/logo.png"));
        channel.Image.Link.ShouldBe(new Uri("http://example.com/"));

        channel.TextInput.ShouldNotBeNull();
        channel.TextInput.Title.ShouldBe("Search");
        channel.TextInput.Name.ShouldBe("q");
        channel.TextInput.Description.ShouldBe("Search this site");
        channel.TextInput.Link.ShouldBe(new Uri("http://example.com/search"));

        channel.SkipDays.ShouldContain(DayOfWeek.Saturday);
        channel.SkipDays.ShouldContain(DayOfWeek.Sunday);
        channel.SkipHours.ShouldContain(0);
        channel.SkipHours.ShouldContain(23);
        channel.Categories.Select(c => c.Value).ShouldContain("News");
    }

    /// <summary>
    /// An RSS item with every optional element survives being written and read back.
    /// </summary>
    [TestMethod]
    public void AMaximalRssItem_SurvivesBeingWrittenAndReadBack()
    {
        RssItem item = SaveAndReload(BuildMaximalRssFeed()).Channel.Items.Single();

        item.Title.ShouldBe("An item");
        item.Description.ShouldBe("An item description");
        item.Link.ShouldBe(new Uri("http://example.com/item/1"));
        item.Author.ShouldBe("author@example.com");
        item.Comments.ShouldBe(new Uri("http://example.com/item/1/comments"));

        item.Guid.ShouldNotBeNull();
        item.Guid.Value.ShouldBe("http://example.com/item/1");
        item.Guid.IsPermanentLink.ShouldBeTrue();

        item.Enclosures.Count.ShouldBe(1);
        item.Enclosures[0].Url.ShouldBe(new Uri("http://example.com/audio.mp3"));
        item.Enclosures[0].ContentType.ShouldBe("audio/mpeg");
        item.Enclosures[0].Length.ShouldBe(123456L);

        item.Source.ShouldNotBeNull();
        item.Source.Title.ShouldBe("The origin feed");
        item.Source.Url.ShouldBe(new Uri("http://origin.example.com/feed.xml"));

        item.Categories.Select(c => c.Value).ShouldContain("Analysis");
    }

    /// <summary>
    /// An Atom entry with every optional element survives being written and read back.
    /// </summary>
    [TestMethod]
    public void AMaximalAtomEntry_SurvivesBeingWrittenAndReadBack()
    {
        AtomEntry entry = SaveAndReload(BuildMaximalAtomFeed()).Entries.Single();

        entry.Title?.Content.ShouldBe("An entry");
        entry.Summary?.Content.ShouldBe("An entry summary");
        entry.Content?.Content.ShouldBe("An entry body");

        entry.Rights.ShouldNotBeNull();
        entry.Rights.Content.ShouldBe("Copyright 2024");

        entry.Source.ShouldNotBeNull();
        entry.Source.Title?.Content.ShouldBe("The origin feed");

        entry.Authors.Count.ShouldBe(1);
        entry.Authors[0].Name.ShouldBe("An Author");
        entry.Contributors.Count.ShouldBe(1);
        entry.Contributors[0].Name.ShouldBe("A Contributor");
        entry.Categories.Select(c => c.Term).ShouldContain("news");
        entry.Links.ShouldNotBeEmpty();
    }

    private static RssFeed BuildMaximalRssFeed()
    {
        RssFeed feed = new(new Uri("http://example.com/"), "A maximal feed");
        RssChannel channel = feed.Channel;

        channel.Description = "Every optional element populated";
        channel.Copyright = "Copyright 2024";
        channel.Generator = "Argotic";
        channel.ManagingEditor = "editor@example.com";
        channel.Webmaster = "webmaster@example.com";
        channel.Language = CultureInfo.GetCultureInfo("en-GB");
        channel.TimeToLive = 60;
        channel.PublicationDate = new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        channel.LastBuildDate = new DateTime(2024, 1, 20, 13, 0, 0, DateTimeKind.Utc);
        channel.SelfLink = new Uri("http://example.com/feed.xml");

        channel.Cloud = new RssCloud("rpc.example.com", "/RPC2", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
        channel.Image = new RssImage(new Uri("http://example.com/"), "A logo", new Uri("http://example.com/logo.png"));
        channel.TextInput = new RssTextInput("Search this site", new Uri("http://example.com/search"), "q", "Search");

        channel.SkipDays.Add(DayOfWeek.Saturday);
        channel.SkipDays.Add(DayOfWeek.Sunday);
        channel.SkipHours.Add(0);
        channel.SkipHours.Add(23);
        channel.Categories.Add(new RssCategory { Value = "News" });

        RssItem item = new()
        {
            Title = "An item",
            Description = "An item description",
            Link = new Uri("http://example.com/item/1"),
            Author = "author@example.com",
            Comments = new Uri("http://example.com/item/1/comments"),
            PublicationDate = new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc),
            Guid = new RssGuid("http://example.com/item/1", isPermanentUrl: true),
            Source = new RssSource(new Uri("http://origin.example.com/feed.xml"), "The origin feed"),
        };
        item.Enclosures.Add(new RssEnclosure(123456L, "audio/mpeg", new Uri("http://example.com/audio.mp3")));
        item.Categories.Add(new RssCategory { Value = "Analysis" });

        channel.Items.Add(item);

        return feed;
    }

    private static AtomFeed BuildMaximalAtomFeed()
    {
        DateTime updated = new(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        AtomFeed feed = new(new AtomId(new Uri("urn:example:feed")), new AtomTextConstruct("A maximal feed"), updated);

        AtomEntry entry = new(new AtomId(new Uri("urn:example:entry:1")), new AtomTextConstruct("An entry"), updated)
        {
            Summary = new AtomTextConstruct("An entry summary"),
            Content = new AtomContent("An entry body", "text"),
            Rights = new AtomTextConstruct("Copyright 2024"),
            Source = new AtomSource(
                new AtomId(new Uri("urn:example:origin")),
                new AtomTextConstruct("The origin feed"),
                updated),
        };
        entry.Authors.Add(new AtomPersonConstruct("An Author"));
        entry.Contributors.Add(new AtomPersonConstruct("A Contributor"));
        entry.Categories.Add(new AtomCategory("news"));
        entry.Links.Add(new AtomLink(new Uri("http://example.com/entry/1")));

        feed.Entries.Add(entry);

        return feed;
    }

    private static RssFeed SaveAndReload(RssFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        RssFeed read = new();
        read.Load(stream);
        return read;
    }

    private static AtomFeed SaveAndReload(AtomFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        AtomFeed read = new();
        read.Load(stream);
        return read;
    }
}