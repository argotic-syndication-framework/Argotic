using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

[TestClass]
public class LiveJournalUserPictureExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<LiveJournalUserPicture>
{
    protected override LiveJournalUserPicture CreateInstance()
        => new() { Keyword = "pic-b", Url = new Uri("http://example.com/b.png") };

    protected override LiveJournalUserPicture CreateLesserInstance()
        => new() { Keyword = "pic-a", Url = new Uri("http://example.com/a.png") };

    protected override LiveJournalUserPicture CreateGreaterInstance()
        => new() { Keyword = "pic-c", Url = new Uri("http://example.com/c.png") };
}