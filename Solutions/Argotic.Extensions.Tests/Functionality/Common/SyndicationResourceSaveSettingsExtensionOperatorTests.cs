using Argotic.Common;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationResourceSaveSettingsExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SyndicationResourceSaveSettings>
{
    protected override SyndicationResourceSaveSettings CreateInstance()
        => new() { MinimizeOutputSize = false };

    protected override SyndicationResourceSaveSettings CreateLesserInstance()
        => new() { MinimizeOutputSize = false };

    protected override SyndicationResourceSaveSettings CreateGreaterInstance()
        => new() { MinimizeOutputSize = true };
}