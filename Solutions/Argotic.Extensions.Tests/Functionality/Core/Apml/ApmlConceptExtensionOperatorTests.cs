using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlConceptExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlConcept>
{
    protected override ApmlConcept CreateInstance()
        => new() { Key = "concept-b" };

    protected override ApmlConcept CreateLesserInstance()
        => new() { Key = "concept-a" };

    protected override ApmlConcept CreateGreaterInstance()
        => new() { Key = "concept-c" };
}