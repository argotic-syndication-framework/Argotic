using Argotic.Data.Adapters;
namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers the two properties every adapter inherits from <see cref="SyndicationResourceAdapterBase"/>, and
/// the constructor guards that decide whether they can ever be unset.
/// </summary>
[TestClass]
public class SyndicationResourceAdapterTests
{
    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);

        return document.CreateNavigator();
    }

    /// <summary>
    /// <see cref="SyndicationResourceAdapter.Settings"/> is the instance the constructor was handed, not a
    /// copy of it and not a default.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation. The property carries a field initialiser that no input can
    ///     observe: the sole constructor either throws on a null argument or assigns both properties, and
    ///     field initialisers run before the constructor body, so the initialised instance is always
    ///     overwritten before anything can read it. Deleting the initialiser is therefore observationally
    ///     inert, and this test is green on both sides of that edit. What it does pin is the property that
    ///     matters to a caller — a <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> set on the
    ///     caller's settings is the one the element walk reads.
    /// </remarks>
    [TestMethod]
    public void Settings_IsTheInstancePassedToTheConstructor()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new() { RetrievalLimit = 7 };
        XPathNavigator navigator = NavigatorFor("<rss version=\"2.0\"><channel><title>Title</title></channel></rss>");

        // Act
        SyndicationResourceAdapter adapter = new(navigator, settings);

        // Assert
        adapter.Settings.ShouldBeSameAs(settings);
        adapter.Settings.RetrievalLimit.ShouldBe(7);
        adapter.Navigator.ShouldBeSameAs(navigator);
    }

    /// <summary>
    /// A null <see cref="SyndicationResourceLoadSettings"/> is rejected rather than replaced with a default.
    /// </summary>
    /// <remarks>
    ///     This is the other half of the argument above: because the constructor throws instead of falling
    ///     back, there is no path on which the field initialiser survives.
    /// </remarks>
    [TestMethod]
    public void Constructor_WithNullSettings_Throws()
    {
        // Arrange
        XPathNavigator navigator = NavigatorFor("<rss version=\"2.0\"><channel><title>Title</title></channel></rss>");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new SyndicationResourceAdapter(navigator, null!));
    }
}