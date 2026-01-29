using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

[TestClass]
public class ApmlAuthorExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<ApmlAuthor>
{
    protected override ApmlAuthor CreateInstance()
        => new() { Key = "author-b" };

    protected override ApmlAuthor CreateLesserInstance()
        => new() { Key = "author-a" };

    protected override ApmlAuthor CreateGreaterInstance()
        => new() { Key = "author-c" };
}