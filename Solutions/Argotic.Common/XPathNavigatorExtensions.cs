using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides child-element selection helpers for <see cref="XPathNavigator"/>.
/// </summary>
/// <remarks>
///     <para>
///         Every XPath in the parse path is a single child-axis step, which
///         <see cref="XPathNavigator.SelectChildren(string, string)"/> performs directly. Microsoft's
///         guidance is explicit that SelectChildren, SelectAncestors and SelectDescendants "are
///         optimized for performance and are faster than their corresponding XPath expressions", and
///         <c>SelectSingleNode</c> is doubly wasteful because it evaluates the whole matching node
///         set before returning the first item (dotnet/runtime#27100).
///     </para>
///     <para>
///         Measured over 20,000 lookups, net of navigator construction: the XPath form costs 25.9 ms
///         and 15.8 MB against 1.6 ms and 1.9 MB for the equivalent SelectChildren call.
///     </para>
///     <para>
///         Internal by design, shared with the other assemblies through <c>InternalsVisibleTo</c>.
///         These extend a BCL type, so making them public would add members to every
///         <see cref="XPathNavigator"/> in every consuming codebase.
///     </para>
/// </remarks>
internal static class XPathNavigatorExtensions
{
    extension(XPathNavigator source)
    {
        /// <summary>
        /// Returns the first child element with the supplied local name and no namespace.
        /// </summary>
        /// <param name="localName">The local name of the child element to select.</param>
        /// <returns>The first matching child element, or <b>null</b> if there is none.</returns>
        /// <remarks>
        ///     Equivalent to <c>SelectSingleNode(localName, manager)</c> for an unprefixed name: under
        ///     XPath 1.0 an unprefixed name matches only the no-namespace partition, whatever default
        ///     namespace the manager carries, so the manager was never consulted for these.
        /// </remarks>
        public XPathNavigator? SelectChildElement(string localName)
        {
            XPathNodeIterator children = source.SelectChildren(localName, string.Empty);

            return children.MoveNext() ? children.Current : null;
        }

        /// <summary>
        /// Returns the first child element matching a prefixed name, resolved against a namespace manager.
        /// </summary>
        /// <param name="prefix">The namespace prefix to resolve.</param>
        /// <param name="localName">The local name of the child element to select.</param>
        /// <param name="resolver">The resolver supplying the prefix's namespace.</param>
        /// <returns>The first matching child element, or <b>null</b> if there is none.</returns>
        /// <exception cref="XPathException">The <paramref name="prefix"/> is not defined by the <paramref name="resolver"/>.</exception>
        /// <remarks>
        ///     The throw is deliberate. Evaluating an XPath naming an unregistered prefix raises
        ///     <see cref="XPathException"/>, and resolving to the empty namespace instead would turn
        ///     that failure into a silent selection from the no-namespace partition.
        /// </remarks>
        public XPathNavigator? SelectChildElement(string prefix, string localName, IXmlNamespaceResolver resolver)
        {
            string namespaceUri = resolver.LookupNamespace(prefix)
                ?? throw new XPathException($"Namespace prefix '{prefix}' is not defined.");

            XPathNodeIterator children = source.SelectChildren(localName, namespaceUri);

            return children.MoveNext() ? children.Current : null;
        }
    }
}