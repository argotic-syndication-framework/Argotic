using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers what the RSS 0.91 and RSS 0.92 adapters report when a <c>skipHours/hour</c> cannot be used.
/// </summary>
/// <remarks>
///     <para>
///     Both adapters renumber the hour, because the <c>version</c> attribute names Userland RSS 0.91,
///     whose text says <i>a number between 1 and 24</i>, while the object model — like RSS 2.0 — numbers
///     the hours 0 through 23. That renumbering is deliberate and is pinned elsewhere. What is tested here
///     is the diagnostic, which is a bug under every reading of every specification: the value in the
///     warning is the one left after the subtraction, so a document that says <c>0</c> is reported as
///     <c>-1</c> — a number that appears nowhere in the file the reader is being asked to look at.
///     </para>
///     <para>
///     The trace is the only observable difference, so a listener is attached for the duration of each
///     test. The suite runs its methods in parallel, so the assertions filter on the adapter's own message
///     prefix rather than on the whole capture.
///     </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public class SkipHoursTraceTests
{
    private static string Rss091With(string hours) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="0.91">
            <channel>
                <title>Test Channel</title>
                <link>http://example.com</link>
                <description>A test channel.</description>
                <language>en-us</language>
                <skipHours>{hours}</skipHours>
            </channel>
        </rss>
        """;

    private static string Rss092With(string hours) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="0.92">
            <channel>
                <title>Test Channel</title>
                <link>http://example.com</link>
                <description>A test channel.</description>
                <skipHours>{hours}</skipHours>
            </channel>
        </rss>
        """;

    /// <summary>
    /// An hour of <c>0</c> is dropped by the RSS 0.91 adapter and reported as the <c>0</c> the document
    /// wrote, not as the <c>-1</c> the subtraction left behind.
    /// </summary>
    [TestMethod]
    public void Rss091_WithASkipHourOfZero_TracesTheValueTheDocumentWrote()
    {
        // Arrange & Act
        (RssFeed feed, IReadOnlyList<string> messages) = FillRss091(Rss091With("<hour>0</hour>"));

        // Assert
        feed.Channel.SkipHours.Count.ShouldBe(0);
        List<string> warnings = WarningsFrom(messages, "Rss091SyndicationResourceAdapter");
        warnings.Count.ShouldBe(1);
        warnings[0].ShouldContain("value of 0.", Case.Sensitive);
        warnings[0].ShouldNotContain("-1", Case.Sensitive);
    }

    /// <summary>
    /// A repeated hour is reported by the value the document wrote as well, so the warning names an hour
    /// the reader can find in the file.
    /// </summary>
    [TestMethod]
    public void Rss091_WithARepeatedSkipHour_TracesTheValueTheDocumentWrote()
    {
        // Arrange & Act
        (RssFeed feed, IReadOnlyList<string> messages) = FillRss091(Rss091With("<hour>5</hour><hour>5</hour>"));

        // Assert
        feed.Channel.SkipHours.Count.ShouldBe(1);
        feed.Channel.SkipHours.ShouldContain(4);
        List<string> warnings = WarningsFrom(messages, "Rss091SyndicationResourceAdapter");
        warnings.Count.ShouldBe(1);
        warnings[0].ShouldContain("value of 5.", Case.Sensitive);
    }

    /// <summary>
    /// The RSS 0.92 adapter reports the same way from its own copy of the walk.
    /// </summary>
    [TestMethod]
    public void Rss092_WithASkipHourOfZero_TracesTheValueTheDocumentWrote()
    {
        // Arrange & Act
        (RssFeed feed, IReadOnlyList<string> messages) = FillRss092(Rss092With("<hour>0</hour>"));

        // Assert
        feed.Channel.SkipHours.Count.ShouldBe(0);
        List<string> warnings = WarningsFrom(messages, "Rss092SyndicationResourceAdapter");
        warnings.Count.ShouldBe(1);
        warnings[0].ShouldContain("value of 0.", Case.Sensitive);
        warnings[0].ShouldNotContain("-1", Case.Sensitive);
    }

    /// <summary>
    /// The hours Userland RSS 0.91 permits — <c>1</c> through <c>24</c> — are kept, renumbered onto the
    /// <c>0</c> through <c>23</c> the object model uses, and reported nowhere.
    /// </summary>
    /// <remarks>
    ///     A guard on the decision to keep the renumbering. It is green before and after the trace is
    ///     corrected, and it is what would break if a later reader "fixed" the subtraction away.
    /// </remarks>
    [TestMethod]
    public void Rss091_WithTheHoursTheSpecificationPermits_KeepsThemRenumbered()
    {
        // Arrange & Act
        (RssFeed feed, IReadOnlyList<string> messages) = FillRss091(Rss091With("<hour>1</hour><hour>24</hour>"));

        // Assert
        feed.Channel.SkipHours.Count.ShouldBe(2);
        feed.Channel.SkipHours.ShouldContain(0);
        feed.Channel.SkipHours.ShouldContain(23);
        WarningsFrom(messages, "Rss091SyndicationResourceAdapter").ShouldBeEmpty();
    }

    private static (RssFeed Feed, IReadOnlyList<string> Messages) FillRss091(string xml)
    {
        RssFeed feed = new();
        CapturingTraceListener listener = new();
        Trace.Listeners.Add(listener);
        try
        {
            Rss091SyndicationResourceAdapter adapter = new(NavigatorFor(xml), new SyndicationResourceLoadSettings());
            adapter.Fill(feed);
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        return (feed, listener.Messages);
    }

    private static (RssFeed Feed, IReadOnlyList<string> Messages) FillRss092(string xml)
    {
        RssFeed feed = new();
        CapturingTraceListener listener = new();
        Trace.Listeners.Add(listener);
        try
        {
            Rss092SyndicationResourceAdapter adapter = new(NavigatorFor(xml), new SyndicationResourceLoadSettings());
            adapter.Fill(feed);
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        return (feed, listener.Messages);
    }

    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);

        return document.CreateNavigator();
    }

    private static List<string> WarningsFrom(IReadOnlyList<string> messages, string adapterName) =>
        messages.Where(message => message.StartsWith(adapterName + " unable to add", StringComparison.Ordinal)).ToList();

    /// <summary>
    /// A <see cref="TraceListener"/> that keeps every formatted event it is handed.
    /// </summary>
    /// <remarks>
    ///     <see cref="TraceEvent(TraceEventCache, string, TraceEventType, int, string, object[])"/> is
    ///     overridden rather than <see cref="Write(string)"/>, so the capture does not depend on how the
    ///     base class chooses to decompose an event into header, body and footer.
    /// </remarks>
    private sealed class CapturingTraceListener : TraceListener
    {
        private readonly List<string> messages = [];

        public IReadOnlyList<string> Messages
        {
            get
            {
                lock (this.messages)
                {
                    return [.. this.messages];
                }
            }
        }

        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
        {
            string message = format is null
                ? string.Empty
                : args is null ? format : string.Format(CultureInfo.InvariantCulture, format, args);

            lock (this.messages)
            {
                this.messages.Add(message);
            }
        }

        public override void Write(string? message)
        {
        }

        public override void WriteLine(string? message)
        {
        }
    }
}