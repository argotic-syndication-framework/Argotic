using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationEncodingUtilityTest
{
    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("@±あ😀", "@±あ😀")] // Emoji should not be stripped
    [DataRow("a\uFFFEb", "ab")] // FFFE should be stripped
    public void RemoveInvalidXmlHexadecimalCharactersTest(string input, string expected)
    {
        string stripped = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);
        stripped.ShouldBe(expected);
    }
}
