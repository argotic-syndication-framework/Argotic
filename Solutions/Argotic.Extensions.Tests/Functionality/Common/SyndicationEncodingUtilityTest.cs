namespace Argotic.Extensions.Tests
{
    using Argotic.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This is a test class for SyndicationEncodingUtilityTest and is intended
    /// to contain all SyndicationEncodingUtilityTest Unit Tests
    /// </summary>
    [TestClass()]
    public class SyndicationEncodingUtilityTest
    {
        /// <summary>
        /// A test for RemoveInvalidXmlHexadecimalCharacters
        /// </summary>
        [TestMethod]
        [DataRow("a", "a")]
        [DataRow("@±あ😀", "@±あ😀")] // Emoji should not be stripped
        [DataRow("a\uFFFEb", "ab")] // FFFE should be stripped
        public void RemoveInvalidXmlHexadecimalCharactersTest(string input, string expected)
        {
            var stripped = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);
            Assert.AreEqual(expected, stripped);
        }
    }
}
