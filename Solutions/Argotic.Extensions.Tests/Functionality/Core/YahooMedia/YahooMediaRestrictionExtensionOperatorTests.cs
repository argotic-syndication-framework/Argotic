using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.Functionality.Common;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaRestrictionExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<YahooMediaRestriction>
{
    protected override YahooMediaRestriction CreateInstance()
    {
        var restriction = new YahooMediaRestriction();
        restriction.Entities.Add("us");
        restriction.Entities.Add("gb");
        return restriction;
    }

    protected override YahooMediaRestriction CreateLesserInstance()
    {
        var restriction = new YahooMediaRestriction();
        restriction.Entities.Add("de");
        restriction.Entities.Add("fr");
        return restriction;
    }

    protected override YahooMediaRestriction CreateGreaterInstance()
    {
        var restriction = new YahooMediaRestriction();
        restriction.Entities.Add("us");
        restriction.Entities.Add("gb");
        restriction.Entities.Add("ca");
        return restriction;
    }
}