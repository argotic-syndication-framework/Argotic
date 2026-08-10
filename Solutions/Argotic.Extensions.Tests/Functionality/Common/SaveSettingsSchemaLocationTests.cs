namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins the value semantics of <see cref="SyndicationResourceSaveSettings.WriteXsiSchemaLocation"/>.
/// </summary>
/// <remarks>
///     <c>CompareTo</c>, <c>GetHashCode</c> and <c>ToString</c> enumerate the settings properties by
///     hand. A property that is not threaded through them makes two settings that differ compare
///     equal, silently. These pins fail if the threading is removed.
/// </remarks>
[TestClass]
public class SaveSettingsSchemaLocationTests
{
    /// <summary>
    /// Two settings that differ only in the schemaLocation option are not equal.
    /// </summary>
    [TestMethod]
    public void TwoSettingsDifferingOnlyInWriteXsiSchemaLocation_AreNotEqual()
    {
        SyndicationResourceSaveSettings off = new();
        SyndicationResourceSaveSettings on = new() { WriteXsiSchemaLocation = true };

        off.Equals(on).ShouldBeFalse("the option is not threaded through CompareTo");
        (off == on).ShouldBeFalse();
        off.CompareTo(on).ShouldBeLessThan(0);
        on.CompareTo(off).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// The option participates in the hash code.
    /// </summary>
    [TestMethod]
    public void TheOption_ParticipatesInTheHashCode()
    {
        SyndicationResourceSaveSettings off = new();
        SyndicationResourceSaveSettings on = new() { WriteXsiSchemaLocation = true };

        on.GetHashCode().ShouldBe(new SyndicationResourceSaveSettings { WriteXsiSchemaLocation = true }.GetHashCode(), "equal settings must agree on the hash");
        on.GetHashCode().ShouldNotBe(off.GetHashCode(), "the option is not hashed, so unequal settings collide by construction");
    }

    /// <summary>
    /// The option appears in the diagnostic string.
    /// </summary>
    [TestMethod]
    public void TheOption_AppearsInToString()
    {
        new SyndicationResourceSaveSettings { WriteXsiSchemaLocation = true }
            .ToString()
            .ShouldContain("WriteXsiSchemaLocation = \"True\"");
    }
}