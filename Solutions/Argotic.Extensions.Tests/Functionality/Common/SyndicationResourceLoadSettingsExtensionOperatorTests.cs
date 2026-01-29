using Argotic.Common;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationResourceLoadSettingsExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SyndicationResourceLoadSettings>
{
    protected override SyndicationResourceLoadSettings CreateInstance()
        => new() { RetrievalLimit = 50 };

    protected override SyndicationResourceLoadSettings CreateLesserInstance()
        => new() { RetrievalLimit = 10 };

    protected override SyndicationResourceLoadSettings CreateGreaterInstance()
        => new() { RetrievalLimit = 100 };
}