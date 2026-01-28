using Argotic.Common;
using Argotic.Extensions.Tests.Functionality.Common;
using Argotic.Net;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class XmlRpcStructureMemberExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<XmlRpcStructureMember>
{
    protected override XmlRpcStructureMember CreateInstance()
        => new("memberB", new XmlRpcScalarValue("valueB"));

    protected override XmlRpcStructureMember CreateLesserInstance()
        => new("memberA", new XmlRpcScalarValue("valueA"));

    protected override XmlRpcStructureMember CreateGreaterInstance()
        => new("memberC", new XmlRpcScalarValue("valueC"));
}
