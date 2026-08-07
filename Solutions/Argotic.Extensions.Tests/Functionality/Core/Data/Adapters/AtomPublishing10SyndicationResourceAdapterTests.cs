using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Publishing;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers how <see cref="AtomPublishing10SyndicationResourceAdapter"/> reads the <c>atom:category</c>
/// children of an <c>app:categories</c> element.
/// </summary>
/// <remarks>
///     RFC 5023 §7.2.1: <i>an atom:category child element that has no "scheme" attribute inherits the
///     attribute from its app:categories parent</i>. The document below is the shape that sentence exists
///     for — a scheme declared once on the parent and omitted on every child.
/// </remarks>
[TestClass]
public class AtomPublishing10SyndicationResourceAdapterTests
{
    private const string CategoryDocumentWithAnInheritedScheme = """
        <?xml version="1.0" encoding="UTF-8"?>
        <app:categories xmlns:app="http://www.w3.org/2007/app"
                        xmlns:atom="http://www.w3.org/2005/Atom"
                        fixed="yes"
                        scheme="http://example.com/cats/big3">
            <atom:category term="animal"/>
            <atom:category term="vegetable"/>
            <atom:category term="mineral" scheme="http://example.org/extra-cats"/>
        </app:categories>
        """;

    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);

        return document.CreateNavigator();
    }

    /// <summary>
    /// A category with no <c>scheme</c> of its own takes the one its <c>app:categories</c> parent declares;
    /// a category that states its own keeps it.
    /// </summary>
    /// <remarks>
    ///     A bare term is only meaningful within a scheme, so without the inheritance a consumer comparing
    ///     these categories against the ones on a member entry has nothing to compare.
    /// </remarks>
    [TestMethod]
    public void Fill_CategoryDocument_InheritsTheSchemeFromItsParent()
    {
        // Arrange
        AtomCategoryDocument document = new();
        AtomPublishing10SyndicationResourceAdapter adapter = new(NavigatorFor(CategoryDocumentWithAnInheritedScheme), new SyndicationResourceLoadSettings());

        // Act
        adapter.Fill(document);

        // Assert
        document.Scheme.ShouldBe(new Uri("http://example.com/cats/big3"));
        document.Categories.Count.ShouldBe(3);
        document.Categories[0].Scheme.ShouldBe(new Uri("http://example.com/cats/big3"));
        document.Categories[1].Scheme.ShouldBe(new Uri("http://example.com/cats/big3"));
        document.Categories[2].Scheme.ShouldBe(new Uri("http://example.org/extra-cats"));
    }

    /// <summary>
    /// Saving a document that inherited its scheme restates that scheme on every child.
    /// </summary>
    /// <remarks>
    ///     The visible cost of the fix, pinned rather than hidden — the same call taken for
    ///     <c>xml:base</c>, where descendants also re-state what they inherited. The written document says
    ///     the same thing as the one that was read; it says it more than once. RFC 5023 §7.2.1 allows a
    ///     child to carry its own <c>scheme</c>, so nothing here is non-conformant, and a reader that never
    ///     implemented the inheritance rule now gets the right answer.
    /// </remarks>
    [TestMethod]
    public void Save_AfterInheritingAScheme_RestatesItOnEveryChild()
    {
        // Arrange
        AtomCategoryDocument document = new();
        AtomPublishing10SyndicationResourceAdapter adapter = new(NavigatorFor(CategoryDocumentWithAnInheritedScheme), new SyndicationResourceLoadSettings());
        adapter.Fill(document);

        // Act
        using MemoryStream destination = new();
        document.Save(destination);
        string written = Encoding.UTF8.GetString(destination.ToArray());

        // Assert
        written.Split("http://example.com/cats/big3").Length.ShouldBe(4);
        written.ShouldContain("http://example.org/extra-cats", Case.Sensitive);
    }
}