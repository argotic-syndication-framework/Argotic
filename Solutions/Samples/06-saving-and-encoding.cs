#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 06 -- Where an object graph becomes bytes
//
//     dotnet run --file Solutions/Samples/06-saving-and-encoding.cs
//
// Everything so far has treated saving as an afterthought: build the graph, call Save, count the
// bytes. That works until somebody else has to read what you wrote, and then a set of questions
// arrives that the object model cannot answer, because they are not about the model at all.
//
// Which encoding? Is there a byte-order mark? What happens to a character XML cannot represent?
// Who closes the stream? None of these have a right answer the library could pick for you, which
// is why the save surface is smaller than people expect and hands most of it back to you.
//
// This sample is the boundary. After it, samples 07 and 08 finish the round trip -- what happens
// to dates on the way out, and what survives being read back in.
// ---------------------------------------------------------------------------------------------

using System.Text;
using System.Xml;

using Argotic.Common;
using Argotic.Syndication;

RssFeed feed = new();
feed.Channel.Title = "endjin blog";
feed.Channel.Link = new Uri("https://endjin.com/blog/");
feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";
feed.Channel.Items.Add(new RssItem
{
    Title = "Naïve Bayes, résumés and £100 -- a title with non-ASCII in it",
    Link = new Uri("https://endjin.com/blog/"),
    Description = "Deliberately awkward: three characters outside ASCII, in one title.",
    PublicationDate = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
});

// ---------------------------------------------------------------------------------------------
// 1. Four overloads, and three deliberate absences
//
// Save comes in exactly four shapes: a Stream or an XmlWriter, each with or without settings.
// What is missing is more informative than what is there.
//
// There is no Save(string path). A path would mean the library opens the file, picks the share
// mode, decides what to do when the directory is missing, and owns the disposal -- four decisions
// it has no basis to make. You open the stream, so you choose all four, and you find out about
// the failure at the point where you can do something about it.
//
// There is no SaveAsync. Serialisation is CPU work over an in-memory graph; the only part that
// blocks is the stream you supplied, and you can make that async yourself by writing to a
// MemoryStream and copying it. An async wrapper would suggest the expensive part was I/O.
//
// And there is no Save that returns a string. That absence is the subject of section 3.
// ---------------------------------------------------------------------------------------------

Heading("The four overloads");
Console.WriteLine($"  Save(Stream)                      {Bytes(SaveWith(feed, null))}");
Console.WriteLine($"  Save(Stream, settings)            {Bytes(SaveWith(feed, new SyndicationResourceSaveSettings()))}");
Console.WriteLine($"  Save(XmlWriter)                   -- you own the writer, so you own its settings");
Console.WriteLine($"  Save(XmlWriter, settings)         -- and the two sets of settings then overlap; see section 4");

// ---------------------------------------------------------------------------------------------
// 2. Encoding, and the mark you did not ask for
//
// SyndicationResourceSaveSettings.CharacterEncoding defaults to Encoding.UTF8, which is almost
// always what you want -- and which has a preamble. Encoding.UTF8 is not "UTF-8"; it is "UTF-8,
// and emit a byte-order mark". So every document this library writes with default settings begins
// EF BB BF.
//
// A BOM is legal in XML and every conforming parser skips it, so this is not a correctness
// problem on the way out. It is a problem on the way back in, and only if you handle the bytes
// yourself: Encoding.UTF8.GetString hands you a string whose first character is U+FEFF, an
// invisible zero-width character that then travels into whatever you concatenate it into. It
// surfaces days later as a mysterious leading character in an HTTP header, a filename, or a diff.
//
// If you do not want it, say so: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).
// ---------------------------------------------------------------------------------------------

byte[] withMark = SaveWith(feed, new SyndicationResourceSaveSettings());
byte[] withoutMark = SaveWith(feed, new SyndicationResourceSaveSettings
{
    CharacterEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
});

Heading("Byte-order marks");
Console.WriteLine($"  Encoding.UTF8                     starts {Hex(withMark, 3)}   {withMark.Length:N0} bytes");
Console.WriteLine($"  new UTF8Encoding(false)           starts {Hex(withoutMark, 3)}   {withoutMark.Length:N0} bytes");
Console.WriteLine($"  difference                        {withMark.Length - withoutMark.Length} bytes -- exactly the mark");
Console.WriteLine();
Console.WriteLine($"  Encoding.UTF8.GetString first char   U+{(int)Encoding.UTF8.GetString(withMark)[0]:X4}  <- invisible, and now in your string");
Console.WriteLine($"  read through a StreamReader instead  U+{(int)ReadText(withMark)[0]:X4}  <- the mark was consumed");

// The declaration follows the encoding you chose, which is the part that makes a non-UTF-8 save
// safe: a reader is told what to decode with before it decodes anything.
Heading("The declaration follows the encoding");
foreach (Encoding encoding in new Encoding[] { Encoding.UTF8, Encoding.Unicode, Encoding.Latin1 })
{
    byte[] document = SaveWith(feed, new SyndicationResourceSaveSettings { CharacterEncoding = encoding });
    Console.WriteLine($"  {encoding.WebName,-12} declares {Quote(SyndicationEncodingUtility.GetXmlEncoding(document).WebName),-12} {document.Length,7:N0} bytes");
}

// ---------------------------------------------------------------------------------------------
// 3. What a narrow encoding does with what it cannot represent
//
// Latin-1 above raises the obvious worry. It has 256 code points; the description below contains
// three CJK ideographs and an em dash, none of which are among them. So what happens -- an
// exception, a question mark, silent mojibake?
//
// None of those. XmlWriter escapes anything the encoding cannot represent as a numeric character
// reference: 日 becomes &#x65E5;. The document stays well-formed, stays entirely lossless, and
// reads back byte-for-character identical -- which is proved below rather than asserted, because
// "lossless" is exactly the sort of claim that deserves to be checked.
//
// This is worth knowing because it inverts the instinct. The encoding you choose is a size and
// compatibility decision, not a correctness one; XML has an escape hatch for every character, and
// the writer uses it. What it costs you is bytes -- see how the Latin-1 document compares once the
// escaping is counted.
//
// It also explains why there is no Save that returns a string. A string has no encoding; it is
// UTF-16 code units in memory. Returning one would mean choosing an encoding on your behalf and
// then writing a declaration that names it, and there is no default that is right for everybody.
// ---------------------------------------------------------------------------------------------

RssFeed unrepresentable = new();
unrepresentable.Channel.Title = "endjin blog";
unrepresentable.Channel.Link = new Uri("https://endjin.com/blog/");
unrepresentable.Channel.Description = "A description containing 日本語 and an em dash — neither of which is Latin-1.";

Heading("Characters the encoding has no room for");
byte[] asLatin1 = SaveWith(unrepresentable, new SyndicationResourceSaveSettings { CharacterEncoding = Encoding.Latin1 });
byte[] asUtf8 = SaveWith(unrepresentable, new SyndicationResourceSaveSettings());

Console.WriteLine($"  as written        {unrepresentable.Channel.Description}");
Console.WriteLine($"  through UTF-8     {Between(ReadText(asUtf8), "<description>", "</description>")}");
Console.WriteLine($"  through Latin-1   {Between(ReadText(asLatin1, Encoding.Latin1), "<description>", "</description>")}");
Console.WriteLine($"  cost              {asUtf8.Length:N0} bytes UTF-8, {asLatin1.Length:N0} bytes Latin-1 -- escaping is not free");

// The proof. Load the Latin-1 document back and compare against the string that went in.
RssFeed reloaded = new();
using (MemoryStream stream = new(asLatin1))
{
    reloaded.Load(stream);
}

bool identical = string.Equals(reloaded.Channel.Description, unrepresentable.Channel.Description, StringComparison.Ordinal);
Console.WriteLine($"  round-tripped     {(identical ? "identical to the original string" : "CHANGED -- this comment is wrong")}");

// ---------------------------------------------------------------------------------------------
// 4. Characters XML itself forbids
//
// Encoding is not the only way a string can fail to be XML. XML 1.0 excludes most C0 control
// characters outright -- a NUL or a 0x08 is not an escapable character, it is simply not
// permitted in a document, and no amount of entity encoding rescues it. This matters more than it
// sounds because such characters arrive routinely: from a database column that once held binary,
// from a badly decoded upload, from a CMS that stored a stray backspace.
//
// The failure mode without a guard is an XmlException at save time, deep inside a writer, naming
// a character position rather than a field. SyndicationEncodingUtility.RemoveInvalidXmlHexadecimal-
// Characters strips them first, so you can sanitise at the boundary where you still know which
// field the text came from.
// ---------------------------------------------------------------------------------------------

const string Dirty = "A title with a NUL   and a backspace  in it";
string clean = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(Dirty);

Heading("Control characters");
Console.WriteLine($"  before  {Dirty.Length} chars, {Dirty.Count(char.IsControl)} of them control characters");
Console.WriteLine($"  after   {clean.Length} chars, {clean.Count(char.IsControl)} of them control characters");
Console.WriteLine($"  result  {clean}");

// ---------------------------------------------------------------------------------------------
// 5. When you own the writer
//
// Save(XmlWriter) exists for the case where the feed is part of something larger, or where you
// need writer settings the save settings do not expose -- a specific new-line handling, say, or
// writing into an existing document.
//
// The rule for the overlap is simple once stated: XmlWriterSettings governs how characters become
// bytes, and SyndicationResourceSaveSettings governs what the library writes. So the encoding of
// a document written through an XmlWriter is the writer's, not the save settings' -- passing both
// does not make CharacterEncoding win, and expecting it to is the one real trap here.
//
// SyndicationEncodingUtility.CreateDocumentXmlWriterSettings is the shortcut: the same settings
// Save(Stream) would have built, so you start from the library's defaults and change one thing.
// ---------------------------------------------------------------------------------------------

XmlWriterSettings writerSettings = SyndicationEncodingUtility.CreateDocumentXmlWriterSettings(Encoding.Latin1);
writerSettings.Indent = true;
writerSettings.NewLineChars = "\n";

using MemoryStream ownStream = new();
using (XmlWriter writer = XmlWriter.Create(ownStream, writerSettings))
{
    // The save settings ask for UTF-8; the writer was built for Latin-1. The writer wins.
    feed.Save(writer, new SyndicationResourceSaveSettings { CharacterEncoding = Encoding.UTF8 });
}

Heading("Writer settings win over save settings");
Console.WriteLine($"  XmlWriterSettings.Encoding        {writerSettings.Encoding.WebName}");
Console.WriteLine($"  SyndicationResourceSaveSettings   {Encoding.UTF8.WebName}");
Console.WriteLine($"  document declares                 {Quote(SyndicationEncodingUtility.GetXmlEncoding(ownStream.ToArray()).WebName)}");

Console.WriteLine();
Console.WriteLine("Next: 07-dates-and-time-zones.cs -- the two date formats syndication uses, and the one that will bite you.");

static byte[] SaveWith(RssFeed feed, SyndicationResourceSaveSettings? settings)
{
    using MemoryStream stream = new();
    if (settings is null)
    {
        feed.Save(stream);
    }
    else
    {
        feed.Save(stream, settings);
    }

    return stream.ToArray();
}

static string ReadText(byte[] document, Encoding? encoding = null)
{
    using MemoryStream stream = new(document);
    using StreamReader reader = new(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

    return reader.ReadToEnd();
}

static string Between(string document, string open, string close)
{
    int start = document.IndexOf(open, StringComparison.Ordinal) + open.Length;
    int end = document.IndexOf(close, start, StringComparison.Ordinal);

    return end < 0 ? "(not found)" : document[start..end];
}

static string Hex(byte[] data, int count) => string.Join(' ', data.Take(count).Select(b => b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));

static string Bytes(byte[] data) => $"{data.Length:N0} bytes";

static string Quote(string value) => $"\"{value}\"";

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}