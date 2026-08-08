namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Proves that selecting a child element is equivalent to the XPath expression it replaces.
/// </summary>
/// <remarks>
///     <para>
///         The parse path replaced 332 single-step XPath expressions with direct child-axis
///         selection. 261 of those sites are executed by the rest of this suite; the remainder are
///         not, so the substitution itself is proved here rather than relying on incidental coverage.
///     </para>
///     <para>
///         The <c>*_Agrees</c> / <c>*_BothSelectNothing</c> cases are an <i>oracle</i>: they assert that
///         <c>SelectSingleNode(xpath, manager)</c> and the child axis agree - both on which node is
///         returned and on returning nothing at all - across the cases that distinguish them. They
///         exercise <c>System.Xml.XPath</c> and no Argotic code, because
///         <c>XPathNavigatorExtensions</c> is <see langword="internal"/> by design and this project
///         deliberately has no <c>InternalsVisibleTo</c>. They record <i>why</i> the substitution is
///         sound; they cannot detect a regression in it.
///     </para>
///     <para>
///         The <c>TheParsePath_*</c> cases below are the ones that can. They drive the same
///         distinguishing document shapes through <see cref="SyndicationResourceMetadata"/>, a public
///         type whose format detection is built from those very
///         <c>SelectChildElement</c> calls - so a substitution that started matching where the
///         expression did not turns them red.
///     </para>
/// </remarks>
[TestClass]
public class XPathChildSelectionEquivalenceTests
{
    private const string Ns = "http://example.com/ns";

    /// <summary>
    /// Parses a document and positions a navigator on its root element, with the prefix <c>p</c> bound.
    /// </summary>
    /// <param name="xml">The document to parse.</param>
    /// <returns>A navigator on the root element, and a manager that binds <c>p</c> to the test namespace.</returns>
    private static (XPathNavigator Navigator, XmlNamespaceManager Manager) Parse(string xml)
    {
        XPathDocument document = new(new StringReader(xml));
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("p", Ns);
        navigator.MoveToFirstChild();

        return (navigator, manager);
    }

    /// <summary>
    /// Asserts that the XPath expression and the child selection reach the same node, or both reach none.
    /// </summary>
    /// <remarks>
    ///     Selecting nothing is compared as carefully as selecting something. A substitution that quietly
    ///     started matching where the expression did not would be a behaviour change no round-trip test sees.
    /// </remarks>
    /// <param name="xml">The document to select over.</param>
    /// <param name="xpath">The single-step expression the parse path used to evaluate.</param>
    /// <param name="localName">The local name handed to <c>SelectChildren</c>.</param>
    /// <param name="namespaceUri">The namespace handed to <c>SelectChildren</c>; empty for the no-namespace partition.</param>
    private static void AssertAgree(string xml, string xpath, string localName, string namespaceUri)
    {
        (XPathNavigator navigator, XmlNamespaceManager manager) = Parse(xml);

        XPathNavigator? viaXPath = navigator.SelectSingleNode(xpath, manager);

        XPathNodeIterator children = navigator.SelectChildren(localName, namespaceUri);
        XPathNavigator? viaChildren = children.MoveNext() ? children.Current : null;

        if (viaXPath is null)
        {
            viaChildren.ShouldBeNull($"XPath '{xpath}' selected nothing, so child selection must not either");
            return;
        }

        viaChildren.ShouldNotBeNull($"XPath '{xpath}' selected a node, so child selection must too");
        viaChildren.LocalName.ShouldBe(viaXPath.LocalName);
        viaChildren.NamespaceURI.ShouldBe(viaXPath.NamespaceURI);
        viaChildren.Value.ShouldBe(viaXPath.Value);
    }

    /// <summary>
    /// The base case: an unprefixed <c>title</c> in no namespace is reached identically by both forms.
    /// </summary>
    [TestMethod]
    public void UnprefixedName_ElementPresent_Agrees() =>
        AssertAgree("<r><title>a</title></r>", "title", "title", string.Empty);

    /// <summary>
    /// When the element is absent, both forms select nothing — the child selection does not fall back to the
    /// sibling that is there.
    /// </summary>
    [TestMethod]
    public void UnprefixedName_ElementAbsent_BothSelectNothing() =>
        AssertAgree("<r><other>a</other></r>", "title", "title", string.Empty);

    /// <summary>
    /// With two matching siblings both forms take the first in document order, so a duplicated element does
    /// not change which value the parse path reads.
    /// </summary>
    [TestMethod]
    public void UnprefixedName_SeveralSiblings_BothTakeTheFirst() =>
        AssertAgree("<r><title>first</title><title>second</title></r>", "title", "title", string.Empty);

    /// <summary>
    /// An element in a namespace is invisible to an unprefixed name under both forms, so a namespaced
    /// <c>p:title</c> is not picked up by a lookup meant for the no-namespace partition.
    /// </summary>
    [TestMethod]
    public void UnprefixedName_ElementIsNamespaced_BothSelectNothing() =>
        // An unprefixed XPath name matches only the no-namespace partition, so neither form matches.
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>a</p:title></r>""", "title", "title", string.Empty);

    /// <summary>
    /// A document carrying a default namespace still yields nothing from an unprefixed name under either
    /// form — the case where a naive substitution could have diverged.
    /// </summary>
    [TestMethod]
    public void UnprefixedName_DocumentHasDefaultNamespace_BothSelectNothing() =>
        // XPath 1.0 does not apply a default namespace to unprefixed names. This is the case where a
        // naive substitution could diverge, and it is why the manager was never consulted for them.
        AssertAgree($"""<r xmlns="{Ns}"><title>a</title></r>""", "title", "title", string.Empty);

    /// <summary>
    /// The base case for a namespaced element: <c>p:title</c> resolved through the manager reaches the same
    /// node as the local name plus namespace.
    /// </summary>
    [TestMethod]
    public void PrefixedName_ElementPresent_Agrees() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>a</p:title></r>""", "p:title", "title", Ns);

    /// <summary>
    /// A different element in the right namespace matches neither form.
    /// </summary>
    [TestMethod]
    public void PrefixedName_ElementAbsent_BothSelectNothing() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:other>a</p:other></r>""", "p:title", "title", Ns);

    /// <summary>
    /// The right local name in the wrong namespace matches neither form, which is the inverse of the
    /// unprefixed-against-namespaced case above.
    /// </summary>
    [TestMethod]
    public void PrefixedName_ElementInNoNamespace_BothSelectNothing() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><title>a</title></r>""", "p:title", "title", Ns);

    /// <summary>
    /// With two matching namespaced siblings both forms take the first in document order.
    /// </summary>
    [TestMethod]
    public void PrefixedName_SeveralSiblings_BothTakeTheFirst() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>first</p:title><p:title>second</p:title></r>""", "p:title", "title", Ns);

    /// <summary>
    /// An element the document spells <c>q:title</c> is still reached by both forms when <c>q</c> and the
    /// expression's <c>p</c> name the same namespace: the binding matches, not the prefix.
    /// </summary>
    [TestMethod]
    public void PrefixedName_ElementBoundByDifferentPrefixSameNamespace_Agrees() =>
        // The namespace, not the prefix, is what matches - both forms must ignore the spelling.
        AssertAgree($"""<r xmlns:q="{Ns}"><q:title>a</q:title></r>""", "p:title", "title", Ns);

    /// <summary>
    /// Builds the metadata for a document, which is what drives the parse path's child selection.
    /// </summary>
    /// <param name="xml">The document to inspect.</param>
    /// <returns>The metadata the library derives from it.</returns>
    private static SyndicationResourceMetadata MetadataFor(string xml) =>
        new(new XPathDocument(new StringReader(xml)).CreateNavigator());

    /// <summary>
    /// The base case through product code: an unprefixed <c>rss</c> in no namespace is recognised.
    /// </summary>
    [TestMethod]
    public void TheParsePath_UnprefixedNameInNoNamespace_IsRecognised() =>
        MetadataFor("""<rss version="2.0"><channel /></rss>""")
            .Format.ShouldBe(SyndicationContentFormat.Rss);

    /// <summary>
    /// A document whose root carries a <i>default namespace</i> is not recognised by the unprefixed
    /// lookup — the case where a naive substitution diverges from the expression it replaced.
    /// </summary>
    /// <remarks>
    ///     XPath 1.0 does not apply a default namespace to an unprefixed name, so
    ///     <c>SelectSingleNode("rss", manager)</c> never matched this document either. A replacement
    ///     that consulted the resolver, or that matched on local name alone, would start recognising it
    ///     — silently changing which format every such document is parsed as.
    /// </remarks>
    [TestMethod]
    public void TheParsePath_UnprefixedNameAgainstADefaultNamespace_IsNotRecognised() =>
        MetadataFor($"""<rss xmlns="{Ns}" version="2.0"><channel /></rss>""")
            .Format.ShouldBe(SyndicationContentFormat.None);

    /// <summary>
    /// The same element bound to a prefix in a foreign namespace is not recognised either.
    /// </summary>
    [TestMethod]
    public void TheParsePath_UnprefixedNameAgainstANamespacedElement_IsNotRecognised() =>
        MetadataFor($"""<x:rss xmlns:x="{Ns}" version="2.0"><x:channel /></x:rss>""")
            .Format.ShouldBe(SyndicationContentFormat.None);

    /// <summary>
    /// A prefixed lookup matches on the namespace the prefix resolves to, not on how the document spells
    /// it: an RSS 1.0 document writing <c>q:RDF</c> is recognised though the resolver binds <c>rdf</c>.
    /// </summary>
    [TestMethod]
    public void TheParsePath_PrefixedNameSpeltWithADifferentPrefix_IsStillRecognised() =>
        MetadataFor(
            """
            <q:RDF xmlns:q="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns="http://purl.org/rss/1.0/">
              <channel />
            </q:RDF>
            """)
            .Format.ShouldBe(SyndicationContentFormat.Rss);

    /// <summary>
    /// The right local name in no namespace at all is not reached by the prefixed lookup, which is the
    /// inverse of the case above.
    /// </summary>
    [TestMethod]
    public void TheParsePath_PrefixedNameAgainstAnElementInNoNamespace_IsNotRecognised() =>
        MetadataFor("<RDF><channel /></RDF>")
            .Format.ShouldBe(SyndicationContentFormat.None);
}