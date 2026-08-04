using System.Xml;
using System.Xml.XPath;
using Shouldly;

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
///         Each case asserts that <c>SelectSingleNode(xpath, manager)</c> and
///         <c>SelectChildren(localName, namespaceUri)</c> agree - both on which node is returned and
///         on returning nothing at all - across the cases that distinguish them: an absent element,
///         several matching siblings, a namespaced element addressed by prefix, an element in the
///         wrong namespace, and a document carrying a default namespace.
///     </para>
/// </remarks>
[TestClass]
public class XPathChildSelectionEquivalenceTests
{
    private const string Ns = "http://example.com/ns";

    private static (XPathNavigator Navigator, XmlNamespaceManager Manager) Parse(string xml)
    {
        XPathDocument document = new(new StringReader(xml));
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("p", Ns);
        navigator.MoveToFirstChild();

        return (navigator, manager);
    }

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

    [TestMethod]
    public void UnprefixedName_ElementPresent_Agrees() =>
        AssertAgree("<r><title>a</title></r>", "title", "title", string.Empty);

    [TestMethod]
    public void UnprefixedName_ElementAbsent_BothSelectNothing() =>
        AssertAgree("<r><other>a</other></r>", "title", "title", string.Empty);

    [TestMethod]
    public void UnprefixedName_SeveralSiblings_BothTakeTheFirst() =>
        AssertAgree("<r><title>first</title><title>second</title></r>", "title", "title", string.Empty);

    [TestMethod]
    public void UnprefixedName_ElementIsNamespaced_BothSelectNothing() =>
        // An unprefixed XPath name matches only the no-namespace partition, so neither form matches.
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>a</p:title></r>""", "title", "title", string.Empty);

    [TestMethod]
    public void UnprefixedName_DocumentHasDefaultNamespace_BothSelectNothing() =>
        // XPath 1.0 does not apply a default namespace to unprefixed names. This is the case where a
        // naive substitution could diverge, and it is why the manager was never consulted for them.
        AssertAgree($"""<r xmlns="{Ns}"><title>a</title></r>""", "title", "title", string.Empty);

    [TestMethod]
    public void PrefixedName_ElementPresent_Agrees() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>a</p:title></r>""", "p:title", "title", Ns);

    [TestMethod]
    public void PrefixedName_ElementAbsent_BothSelectNothing() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:other>a</p:other></r>""", "p:title", "title", Ns);

    [TestMethod]
    public void PrefixedName_ElementInNoNamespace_BothSelectNothing() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><title>a</title></r>""", "p:title", "title", Ns);

    [TestMethod]
    public void PrefixedName_SeveralSiblings_BothTakeTheFirst() =>
        AssertAgree($"""<r xmlns:p="{Ns}"><p:title>first</p:title><p:title>second</p:title></r>""", "p:title", "title", Ns);

    [TestMethod]
    public void PrefixedName_ElementBoundByDifferentPrefixSameNamespace_Agrees() =>
        // The namespace, not the prefix, is what matches - both forms must ignore the spelling.
        AssertAgree($"""<r xmlns:q="{Ns}"><q:title>a</q:title></r>""", "p:title", "title", Ns);
}