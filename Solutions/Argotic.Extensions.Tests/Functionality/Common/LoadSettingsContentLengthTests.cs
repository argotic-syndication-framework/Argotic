namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the response size cap on <see cref="SyndicationResourceLoadSettings"/>.
/// </summary>
/// <remarks>
///     The value semantics half matters as much as the guard half. This type has hand-written
///     comparison and equality that enumerate every member, so a new member omitted from them is
///     invisible to <c>==</c>, to <c>GetHashCode</c>, and to the diagnostic surface — two settings
///     objects differing only in their cap would compare equal, and one would silently displace the
///     other in a dictionary.
/// </remarks>
[TestClass]
public sealed class LoadSettingsContentLengthTests
{
    /// <summary>
    /// The cap is unset by default, which means the loading type's own default rather than no limit.
    /// </summary>
    [TestMethod]
    public void TheCap_IsUnsetByDefault()
        => new SyndicationResourceLoadSettings().MaxResponseContentLength.ShouldBeNull();

    /// <summary>
    /// Zero is rejected rather than read as "no limit".
    /// </summary>
    /// <remarks>
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> already spends that meaning on
    ///     zero in this same type, so reusing it here would make one member's zero mean "unlimited" and
    ///     another's mean "accept nothing". A cap of zero is a misconfiguration and is treated as one.
    /// </remarks>
    [TestMethod]
    public void AZeroOrNegativeCap_IsRejected()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new SyndicationResourceLoadSettings { MaxResponseContentLength = 0 });
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new SyndicationResourceLoadSettings { MaxResponseContentLength = -1 });
    }

    /// <summary>
    /// <c>Unbounded</c> is accepted, and is how a caller asks for the old behaviour back.
    /// </summary>
    /// <remarks>
    ///     The <c>Unbounded</c> row on its own restated the constant's own definition —
    ///     <c>Unbounded</c> <i>is</i> <see cref="long.MaxValue"/> — so it could not distinguish a working
    ///     setter from a broken one. The row that carries the test is <c>1</c>: it is the smallest legal
    ///     cap, so it fails against a guard written as <c>&lt;= 1024</c> or any other floor above zero,
    ///     which <c>AZeroOrNegativeCap_IsRejected</c> above cannot detect.
    /// </remarks>
    /// <param name="cap">A cap the setter must accept.</param>
    [TestMethod]
    [DataRow(1L)]
    [DataRow(1024L)]
    [DataRow(long.MaxValue)]
    public void TheUnboundedValue_IsAccepted(long cap)
        => new SyndicationResourceLoadSettings { MaxResponseContentLength = cap }
            .MaxResponseContentLength.ShouldBe(cap);

    /// <summary>
    /// Two settings differing only in their cap are not equal.
    /// </summary>
    /// <remarks>
    ///     This fails against a version of the type that adds the property and forgets the three
    ///     hand-written members that enumerate the others, which is the whole reason it exists.
    /// </remarks>
    [TestMethod]
    public void SettingsDifferingOnlyInTheCap_AreNotEqual()
    {
        SyndicationResourceLoadSettings smaller = new() { MaxResponseContentLength = 1_024 };
        SyndicationResourceLoadSettings larger = new() { MaxResponseContentLength = 2_048 };

        (smaller == larger).ShouldBeFalse();
        (smaller != larger).ShouldBeTrue();
        smaller.Equals(larger).ShouldBeFalse();
        smaller.CompareTo(larger).ShouldBeLessThan(0);
        larger.CompareTo(smaller).ShouldBeGreaterThan(0);
        smaller.GetHashCode().ShouldNotBe(larger.GetHashCode());
    }

    /// <summary>
    /// An unset cap sorts before any value, and equals another unset cap.
    /// </summary>
    [TestMethod]
    public void AnUnsetCap_SortsBeforeAnyValue()
    {
        SyndicationResourceLoadSettings unset = new();
        SyndicationResourceLoadSettings capped = new() { MaxResponseContentLength = 1_024 };

        unset.CompareTo(capped).ShouldBeLessThan(0);
        capped.CompareTo(unset).ShouldBeGreaterThan(0);
        unset.CompareTo(new SyndicationResourceLoadSettings()).ShouldBe(0);
    }

    /// <summary>
    /// The cap appears in the diagnostic string, and says "default" when unset.
    /// </summary>
    [TestMethod]
    public void TheCap_AppearsInToString()
    {
        new SyndicationResourceLoadSettings().ToString()
            .ShouldContain("MaxResponseContentLength = \"default\"");

        new SyndicationResourceLoadSettings { MaxResponseContentLength = 4_096 }.ToString()
            .ShouldContain("MaxResponseContentLength = \"4096\"");
    }

    /// <summary>
    /// The published per-format defaults are ordered the way the formats are sized.
    /// </summary>
    /// <remarks>
    ///     Not a tautology: it is the one place the four constants are compared to each other, and it
    ///     says that discovery — which only ever reads a page to find a link in it — gets the smallest
    ///     allowance, while a whole-site export and a 50,000-URL sitemap get the largest.
    /// </remarks>
    [TestMethod]
    public void ThePublishedDefaults_AreOrderedByHowLargeEachFormatLegitimatelyGets()
    {
        SyndicationContentLengthLimits.Discovery.ShouldBeLessThan(SyndicationContentLengthLimits.Feed);
        SyndicationContentLengthLimits.Feed.ShouldBeLessThan(SyndicationContentLengthLimits.Sitemap);
        SyndicationContentLengthLimits.Sitemap.ShouldBe(SyndicationContentLengthLimits.Archive);

        // The sitemap protocol permits 50,000 URLs and caps the document at 50 MB uncompressed.
        SyndicationContentLengthLimits.Sitemap.ShouldBeGreaterThan(50L * 1000 * 1000);
    }
}