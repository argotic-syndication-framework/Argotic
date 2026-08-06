using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Generates OPML subscription lists shaped like the ones this library is actually pointed at.
/// </summary>
/// <remarks>
///     <para>
///     Calibrated against three production newsletter lists — Azure Weekly, Fabric Weekly and Power BI
///     Weekly — rather than invented. Measured across all three: <b>every</b> outline carries exactly
///     <b>eight</b> attributes, averages <b>253 bytes</b>, and the body is <b>flat</b>: 478 of 478
///     self-closing, zero containers. The three real sizes are 88, 299 and 478 subscriptions.
///     </para>
///     <para>
///     The attribute mix is what makes this corpus worth having. Only <c>text</c> and <c>type</c> are
///     names <c>OpmlOutline.LoadAttribute</c> recognises; the other six — <c>author</c>,
///     <c>authorType</c>, <c>dateCreated</c>, <c>id</c>, <c>includeAllContent</c>, <c>xmlUrl</c> —
///     fall through every comparison in the chain and land in the extensibility dictionary. A
///     generator that used only recognised names would exercise the short arm of the branch that
///     dominates this workload, and would have been measuring the wrong thing.
///     </para>
///     <para>
///     The lists are not in the repository: they are a customer's subscription data and live outside
///     it. What is committed is their measured shape, which is the part a benchmark needs.
///     </para>
/// </remarks>
internal static class OpmlCorpus
{
    /// <summary>
    /// The three sizes the production lists actually are.
    /// </summary>
    public static int[] RealSizes => [88, 299, 478];

    /// <summary>
    /// Generates a subscription list with <paramref name="subscriptionCount"/> feed outlines.
    /// </summary>
    /// <param name="subscriptionCount">The number of <c>outline</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes, as it would arrive from disk or the wire.</returns>
    public static byte[] GenerateSubscriptionListUtf8(int subscriptionCount)
    {
        StringBuilder builder = new(capacity: 512 + (subscriptionCount * 280));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<opml version=\"2.0\">\n");
        builder.Append("  <head>\n");
        builder.Append("    <title>Benchmark Weekly</title>\n");
        builder.Append("    <dateCreated>Fri, 01 Jan 2021 06:30:00 GMT</dateCreated>\n");
        builder.Append("    <ownerId>urn:uuid:50774e3c-3e7d-4cda-8edc-cf714c55832d</ownerId>\n");
        builder.Append("    <docs>http://www.opml.org/spec2</docs>\n");
        builder.Append("  </head>\n");
        builder.Append("  <body>\n");

        for (int i = 0; i < subscriptionCount; i++)
        {
            builder.Append(CultureInfo.InvariantCulture, $"    <outline text=\"The Example Blog {i}\" type=\"RSS\" author=\"Author {i}\" ");
            builder.Append(CultureInfo.InvariantCulture, $"authorType=\"Single\" dateCreated=\"2026-05-26T13:10:31.674329{i % 10}+00:00\" ");
            builder.Append(CultureInfo.InvariantCulture, $"id=\"e0de18dc-2824-4516-a7ef-ef5d8f7bcb{i % 100:D2}\" includeAllContent=\"False\" ");
            builder.Append(CultureInfo.InvariantCulture, $"xmlUrl=\"https://example{i}.invalid/blog/feed/atom.xml\" />\n");
        }

        builder.Append("  </body>\n");
        builder.Append("</opml>");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}