using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

[TestClass]
public class LiveJournalMoodExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<LiveJournalMood>
{
    protected override LiveJournalMood CreateInstance()
        => new() { Content = "happy", Id = 2 };

    protected override LiveJournalMood CreateLesserInstance()
        => new() { Content = "calm", Id = 1 };

    protected override LiveJournalMood CreateGreaterInstance()
        => new() { Content = "joyful", Id = 3 };
}
