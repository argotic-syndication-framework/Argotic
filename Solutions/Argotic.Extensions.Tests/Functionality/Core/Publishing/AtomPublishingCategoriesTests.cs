using System.Text;
using System.Xml;

using Argotic.Publishing;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// Covers the <c>app:categories</c> element in both the places RFC 5023 puts it — nested inside a
/// collection in a service document, and standing alone as a category document.
/// </summary>
/// <remarks>
///     <para>
///     Written because writing <c>AtomPublishingLoadBenchmarks</c> could not produce a service-document
///     arm carrying categories: <b>every</b> such document threw. The four real service documents in the
///     corpus load only because the server that produced them emits no categories, so nothing in the
///     repository — suite, examples or harness — had ever put the two together.
///     </para>
///     <para>
///     Two distinct defects meet here, and the tests are written so that fixing one cannot appear to fix
///     the other. <see cref="TheServiceDocumentOfRfc5023_LoadsWithItsCategories"/> covers the first: a
///     navigator positioned <em>on</em> the <c>app:categories</c> element was handed to a loader whose
///     format sniff looks for a <em>child</em> of that name.
///     <see cref="AnOutOfLineCategoriesElement_KeepsTheThreeAttributesItCarries"/> covers the second: the
///     attribute read sat inside <c>if (HasChildren)</c>, and an out-of-line categories element is by
///     definition childless.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomPublishingCategoriesTests
{
    /// <summary>
    /// The service document printed in RFC 5023 §8.3.3, verbatim.
    /// </summary>
    /// <remarks>
    ///     Reproduced from the specification rather than written for the test, because the point of the
    ///     first assertion is that the document the specification offers as its own example must load.
    ///     It carries both spellings: an out-of-line <c>categories</c> with an <c>href</c>, and an inline
    ///     <c>categories</c> with <c>fixed="yes"</c> and two child categories.
    /// </remarks>
    private const string Rfc5023ServiceDocument = """
        <?xml version="1.0" encoding='utf-8'?>
        <service xmlns="http://www.w3.org/2007/app"
                 xmlns:atom="http://www.w3.org/2005/Atom">
          <workspace>
            <atom:title>Main Site</atom:title>
            <collection href="http://example.org/blog/main" >
              <atom:title>My Blog Entries</atom:title>
              <categories href="http://example.com/cats/forMain.cats" />
            </collection>
            <collection href="http://example.org/blog/pic" >
              <atom:title>Pictures</atom:title>
              <accept>image/png</accept>
              <accept>image/jpeg</accept>
              <accept>image/gif</accept>
            </collection>
          </workspace>
          <workspace>
            <atom:title>Sidebar Blog</atom:title>
            <collection href="http://example.org/sidebar/list" >
              <atom:title>Remaindered Links</atom:title>
              <accept>application/atom+xml;type=entry</accept>
              <categories fixed="yes">
                <atom:category scheme="http://example.org/extra-cats/" term="joke" />
                <atom:category scheme="http://example.org/extra-cats/" term="serious" />
              </categories>
            </collection>
          </workspace>
        </service>
        """;

    private static XmlReader Reader(string xml) =>
        XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    /// <summary>
    /// The specification's own example service document loads, and its categories arrive with it.
    /// </summary>
    [TestMethod]
    public void TheServiceDocumentOfRfc5023_LoadsWithItsCategories()
    {
        AtomServiceDocument document = new();
        using XmlReader reader = Reader(Rfc5023ServiceDocument);

        document.Load(reader);

        document.Workspaces.Count.ShouldBe(2);

        AtomWorkspace sidebar = document.Workspaces.Last();
        AtomMemberResources collection = sidebar.Collections.Single();

        collection.Categories.Count.ShouldBe(1, "the inline categories element is part of the collection");
        collection.Categories[0].IsFixed.ShouldBeTrue();
        collection.Categories[0].Categories.Select(category => category.Term)
            .ShouldBe(["joke", "serious"]);
    }

    /// <summary>
    /// A collection carrying categories loads whichever of the two spellings RFC 5023 permits it uses.
    /// </summary>
    /// <remarks>
    ///     The third row is the control. Without it, the first two passing is equally consistent with
    ///     "collections no longer load at all", which would be a larger and worse change.
    /// </remarks>
    [TestMethod]
    [DataRow("""<categories href="http://example.com/c.cats" />""", "out-of-line")]
    [DataRow("""<categories fixed="yes"><atom:category term="x" /></categories>""", "inline")]
    [DataRow("", "no categories at all — the control")]
    public void ACollection_LoadsWhicheverCategoriesSpellingItUses(string categories, string spelling)
    {
        string xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
              <workspace>
                <atom:title>W</atom:title>
                <collection href="http://example.org/a"><atom:title>A</atom:title>{categories}</collection>
              </workspace>
            </service>
            """;

        AtomServiceDocument document = new();
        using XmlReader reader = Reader(xml);

        document.Load(reader);

        document.Workspaces.Single().Collections.Single().Uri
            .ShouldBe(new Uri("http://example.org/a"), spelling);
    }

    /// <summary>
    /// An out-of-line categories element inside a service document arrives with its <c>href</c> intact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The end-to-end case, and the one that needs all three fixes at once: the format sniff has to
    ///     recognise a navigator positioned on the element, the attribute read has to happen for a
    ///     childless element, and <c>AtomMemberResources</c> has to keep the document it builds.
    ///     </para>
    ///     <para>
    ///     Any one of the three still broken leaves this failing, which is why it is worth having
    ///     alongside the three narrower tests rather than instead of them.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnOutOfLineCategoriesElementInAServiceDocument_ArrivesWithItsHref()
    {
        const string Xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
              <workspace>
                <atom:title>W</atom:title>
                <collection href="http://example.org/blog/main">
                  <atom:title>My Blog Entries</atom:title>
                  <categories href="http://example.com/cats/forMain.cats" />
                </collection>
              </workspace>
            </service>
            """;

        AtomServiceDocument document = new();
        using XmlReader reader = Reader(Xml);

        document.Load(reader);

        AtomCategoryDocument categories = document.Workspaces.Single().Collections.Single()
            .Categories.ShouldHaveSingleItem();

        categories.Uri.ShouldBe(new Uri("http://example.com/cats/forMain.cats"));
        categories.Categories.ShouldBeEmpty("an out-of-line reference carries no inline categories");
    }

    /// <summary>
    /// An out-of-line categories element keeps <c>fixed</c>, <c>scheme</c> and <c>href</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The whole point of an out-of-line categories element is the <c>href</c> that says where the
    ///     real list lives, and it is the one shape that carries no children. The attribute read was
    ///     nested inside <c>if (documentNavigator.HasChildren)</c>, so precisely the document whose
    ///     attributes matter most lost all three of them.
    ///     </para>
    ///     <para>
    ///     Asserting all three rather than just <c>href</c> is deliberate: they were dropped by one
    ///     guard, and a fix that restores only the one named in the report would leave the other two
    ///     cold and the guard still wrong.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnOutOfLineCategoriesElement_KeepsTheThreeAttributesItCarries()
    {
        const string Xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <app:categories xmlns:app="http://www.w3.org/2007/app"
                            xmlns:atom="http://www.w3.org/2005/Atom"
                            fixed="yes"
                            scheme="http://example.com/cats/big3"
                            href="http://example.com/cats/forMain.atomcat" />
            """;

        AtomCategoryDocument document = new();
        using XmlReader reader = Reader(Xml);

        document.Load(reader);

        document.IsFixed.ShouldBeTrue();
        document.Scheme.ShouldBe(new Uri("http://example.com/cats/big3"));
        document.Uri.ShouldBe(new Uri("http://example.com/cats/forMain.atomcat"));
        document.Categories.ShouldBeEmpty("an out-of-line document has no inline categories");
    }

    /// <summary>
    /// The same attributes on a categories element that does have children are unaffected.
    /// </summary>
    /// <remarks>
    ///     The control for the test above: this shape already worked, and moving the attribute read out
    ///     of the <c>HasChildren</c> guard must not disturb it.
    /// </remarks>
    [TestMethod]
    public void AnInlineCategoriesDocument_StillKeepsItsAttributesAndItsCategories()
    {
        const string Xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <app:categories xmlns:app="http://www.w3.org/2007/app"
                            xmlns:atom="http://www.w3.org/2005/Atom"
                            fixed="yes"
                            scheme="http://example.com/cats/big3">
              <atom:category term="animal" />
              <atom:category term="vegetable" label="Vegetable" />
            </app:categories>
            """;

        AtomCategoryDocument document = new();
        using XmlReader reader = Reader(Xml);

        document.Load(reader);

        document.IsFixed.ShouldBeTrue();
        document.Scheme.ShouldBe(new Uri("http://example.com/cats/big3"));
        document.Categories.Select(category => category.Term).ShouldBe(["animal", "vegetable"]);
    }
}