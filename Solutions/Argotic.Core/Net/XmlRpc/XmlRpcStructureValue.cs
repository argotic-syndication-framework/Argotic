using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure parameter value that holds a set of named members.
/// </summary>
/// <remarks>
///     XML-RPC's <c>&lt;struct&gt;</c>: an unordered bag of name-and-value pairs, of which the
///     <c>faultCode</c>/<c>faultString</c> pair carried by <see cref="XmlRpcResponse.Fault"/> is the one
///     every server emits. The specification neither forbids a repeated member name nor defines what one
///     means, and nothing here rejects it: <see cref="Members"/> keeps both, while the indexer returns
///     the first.
/// </remarks>
/// <seealso cref="XmlRpcMessage.Parameters"/>
/// <seealso cref="IXmlRpcValue"/>
public class XmlRpcStructureValue : IXmlRpcValue, IComparable<XmlRpcStructureValue>, IEquatable<XmlRpcStructureValue>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcStructureValue"/> class.
    /// </summary>
    public XmlRpcStructureValue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcStructureValue"/> class using the supplied <see cref="XPathNodeIterator"/>.
    /// </summary>
    /// <param name="iterator">An iterator over the <c>&lt;member&gt;</c> nodes for the structure. Nodes that will not parse are skipped silently, so a shorter <see cref="Members"/> than the iterator's <c>Count</c> is possible.</param>
    /// <remarks>
    ///     The structure is taken to be at nesting depth zero, exactly as
    ///     <see cref="Load(XPathNavigator)"/> takes its argument to be — an iterator arrives with no
    ///     record of what enclosed it, so there is nothing else to assume. Its members' values are
    ///     therefore bounded by <see cref="XmlRpcClient.MaxValueNestingDepth"/> in the same way.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is <see langword="null"/>.</exception>
    public XmlRpcStructureValue(XPathNodeIterator iterator)
    {
        ArgumentNullException.ThrowIfNull(iterator);

        this.AddMembers(iterator, 0);
    }

    /// <summary>
    /// Gets or sets the <see cref="XmlRpcStructureMember"/> that has the specified name.
    /// </summary>
    /// <param name="name">The member name to look up. Matched without regard to case.</param>
    /// <returns>The first member with that name, or <see langword="null"/> if the structure has none.</returns>
    /// <remarks>
    ///     A set for a name no member carries does nothing — it does not add one, and it does not throw.
    ///     Build a structure through <see cref="Members"/>; use the indexer to replace.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public XmlRpcStructureMember? this[string name]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            XmlRpcStructureMember? result = null;

            foreach (XmlRpcStructureMember member in this.Members)
            {
                if (string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result = member;
                    break;
                }
            }

            return result;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(value);

            for (int i = 0; i < this.Members.Count; i++)
            {
                XmlRpcStructureMember member = this.Members[i];
                if (string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    this.Members[i] = value;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Gets this structure's members.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="XmlRpcStructureMember"/> objects that represent this structure's members.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<XmlRpcStructureMember> Members { get; } = [];

    /// <summary>
    /// Compares two specified <see cref="IList{XmlRpcStructureMember}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>
    ///     <c>1</c> if <paramref name="source"/> holds more members than <paramref name="target"/>;
    ///     <c>-1</c> if it holds fewer, or if the counts match but some member of
    ///     <paramref name="source"/> is absent from <paramref name="target"/>; otherwise, <c>0</c>.
    /// </returns>
    /// <remarks>
    ///     Equal counts are compared as <i>sets</i>, so order is not consulted — which is right here,
    ///     because an XML-RPC structure is unordered.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<XmlRpcStructureMember> source, IList<XmlRpcStructureMember> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                XmlRpcStructureMember member = source[i];
                if (!target.Contains(member))
                {
                    result = -1;
                    break;
                }
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Loads this <see cref="XmlRpcStructureValue"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcStructureValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcStructureValue"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source) => this.Load(source, 0);

    /// <summary>
    /// Loads this <see cref="XmlRpcStructureValue"/> using the supplied <see cref="XPathNavigator"/>, at a known nesting depth.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="depth">How many composite values enclose this one. Zero at the outermost <c>value</c>.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcStructureValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     A <c>member</c> is not itself a nesting level — its <c>value</c> is — so the depth passes
    ///     through <see cref="XmlRpcStructureMember"/> unchanged and is incremented there.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    internal bool Load(XPathNavigator source, int depth)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNodeIterator memberIterator = source.Select("struct/member");
            if (memberIterator is { Count: > 0 })
            {
                wasLoaded = this.AddMembers(memberIterator, depth);
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Adds every member the supplied iterator yields that parses, and reports whether any did.
    /// </summary>
    /// <param name="iterator">An iterator over the <c>member</c> elements of this structure.</param>
    /// <param name="depth">How many composite values enclose this structure.</param>
    /// <returns><see langword="true"/> if at least one member was added; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Shared by <see cref="Load(XPathNavigator, int)"/> and the
    ///     <see cref="XmlRpcStructureValue(XPathNodeIterator)"/> constructor so that the depth bound
    ///     cannot be applied to one and forgotten on the other — which is exactly how the constructor
    ///     came to be a second, unguarded door into the same recursion.
    /// </remarks>
    private bool AddMembers(XPathNodeIterator iterator, int depth)
    {
        bool wasLoaded = false;

        while (iterator.MoveNext())
        {
            XPathNavigator? memberNode = iterator.Current;
            if (memberNode is null)
            {
                continue;
            }

            XmlRpcStructureMember member = new();
            if (member.Load(memberNode, depth))
            {
                this.Members.Add(member);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcStructureValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("value");

        writer.WriteStartElement("struct");
        foreach (XmlRpcStructureMember member in this.Members)
        {
            member.WriteTo(writer);
        }
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcStructureValue"/>.
    /// </summary>
    /// <returns>The <c>&lt;value&gt;&lt;struct&gt;</c> XML for the current instance, written as a fragment — no XML declaration.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(XmlRpcStructureValue? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = XmlRpcStructureValue.CompareSequence(this.Members, other.Members);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcStructureValue"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcStructureValue"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="XmlRpcStructureValue"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(XmlRpcStructureValue? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is XmlRpcStructureValue other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Members.Count));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(XmlRpcStructureValue? first, XmlRpcStructureValue? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(XmlRpcStructureValue? first, XmlRpcStructureValue? second) => !(first == second);

}