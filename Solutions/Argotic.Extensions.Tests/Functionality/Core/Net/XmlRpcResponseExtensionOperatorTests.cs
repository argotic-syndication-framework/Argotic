using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class XmlRpcResponseExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<XmlRpcResponse>
{
    protected override XmlRpcResponse CreateInstance()
        => new();

    protected override XmlRpcResponse CreateLesserInstance()
        => new();

    protected override XmlRpcResponse CreateGreaterInstance()
        => new(new XmlRpcScalarValue("value"));
}
