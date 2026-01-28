using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class XmlRpcMessageExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<XmlRpcMessage>
{
    protected override XmlRpcMessage CreateInstance()
        => new("methodB");

    protected override XmlRpcMessage CreateLesserInstance()
        => new("methodA");

    protected override XmlRpcMessage CreateGreaterInstance()
        => new("methodC");
}
