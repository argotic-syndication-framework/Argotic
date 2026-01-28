using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

[TestClass]
public class AtomWorkspaceExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<AtomWorkspace>
{
    protected override AtomWorkspace CreateInstance()
        => new() { Title = new AtomTextConstruct("Workspace B") };

    protected override AtomWorkspace CreateLesserInstance()
        => new() { Title = new AtomTextConstruct("Workspace A") };

    protected override AtomWorkspace CreateGreaterInstance()
        => new() { Title = new AtomTextConstruct("Workspace C") };
}
