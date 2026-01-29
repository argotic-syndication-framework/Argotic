using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

[TestClass]
public class LiveJournalSecurityExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<LiveJournalSecurity>
{
    protected override LiveJournalSecurity CreateInstance()
        => new(LiveJournalSecurityType.Friends, 2);

    protected override LiveJournalSecurity CreateLesserInstance()
        => new(LiveJournalSecurityType.Friends, 1);

    protected override LiveJournalSecurity CreateGreaterInstance()
        => new(LiveJournalSecurityType.Friends, 3);
}