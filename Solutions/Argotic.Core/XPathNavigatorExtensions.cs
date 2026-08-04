using System.Xml.XPath;

namespace Argotic.Syndication;

/// <summary>
/// Provides child-element selection helpers for <see cref="XPathNavigator"/>.
/// </summary>
/// <remarks>
///     <para>
///         Every XPath in this assembly's parse path is a single child-axis step, which
///         <see cref="XPathNavigator.SelectChildren(string, string)"/> performs directly.
///         Microsoft's guidance is explicit that SelectChildren, SelectAncestors and
///         SelectDescendants "are optimized for performance and are faster than their corresponding
///         XPath expressions", and <c>SelectSingleNode</c> is doubly wasteful because it evaluates
///         the whole matching node set before returning the first item (dotnet/runtime#27100).
///     </para>
///     <para>
///         Measured over 20,000 lookups, net of navigator construction: the XPath form costs
///         25.9 ms and 15.8 MB against 1.6 ms and 1.9 MB for the equivalent SelectChildren call.
///     </para>
///     <para>
///         Internal by design. These extend a BCL type, so making them public would add members to
///         every <see cref="XPathNavigator"/> in every consuming codebase.
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
        /// Returns the first child element with the supplied local name and namespace.
        /// </summary>
        /// <param name="localName">The local name of the child element to select.</param>
        /// <param name="namespaceUri">The namespace URI of the child element to select.</param>
        /// <returns>The first matching child element, or <b>null</b> if there is none.</returns>
        public XPathNavigator? SelectChildElement(string localName, string namespaceUri)
        {
            XPathNodeIterator children = source.SelectChildren(localName, namespaceUri);

            return children.MoveNext() ? children.Current : null;
        }
    }
}