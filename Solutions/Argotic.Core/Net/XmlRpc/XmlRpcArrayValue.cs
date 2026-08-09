using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure parameter value that holds an ordered collection of data elements.
/// </summary>
/// <remarks>
///     XML-RPC's <c>&lt;array&gt;</c>. The elements are not required to share a type — an array of an
///     <c>int</c>, a <c>string</c> and a nested <c>&lt;struct&gt;</c> is legal — which is why the
///     collection is of <see cref="IXmlRpcValue"/> rather than of anything narrower.
/// </remarks>
/// <seealso cref="XmlRpcMessage.Parameters"/>
/// <seealso cref="IXmlRpcValue"/>
public class XmlRpcArrayValue : IXmlRpcValue, IComparable<XmlRpcArrayValue>, IEquatable<XmlRpcArrayValue>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class.
    /// </summary>
    public XmlRpcArrayValue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcArrayValue"/> class using the supplied <see cref="XPathNodeIterator"/>.
    /// </summary>
    /// <param name="iterator">An iterator over the <c>&lt;value&gt;</c> nodes for the array. Nodes that will not parse are skipped silently, so a shorter <see cref="Values"/> than the iterator's <c>Count</c> is possible.</param>
    /// <remarks>
    ///     The array is taken to be at nesting depth zero, exactly as <see cref="Load(XPathNavigator)"/>
    ///     takes its argument to be — an iterator arrives with no record of what enclosed it, so there is
    ///     nothing else to assume. Its elements are therefore bounded by
    ///     <see cref="XmlRpcClient.MaxValueNestingDepth"/> in the same way.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is <see langword="null"/>.</exception>
    public XmlRpcArrayValue(XPathNodeIterator iterator)
    {
        ArgumentNullException.ThrowIfNull(iterator);

        this.AddValues(iterator, 0);
    }

    /// <summary>
    /// Gets data elements for this array.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the data elements for this array.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<IXmlRpcValue> Values { get; } = [];

    /// <summary>
    /// Loads this <see cref="XmlRpcArrayValue"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcArrayValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcArrayValue"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source) => this.Load(source, 0);

    /// <summary>
    /// Loads this <see cref="XmlRpcArrayValue"/> using the supplied <see cref="XPathNavigator"/>, at a known nesting depth.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="depth">How many composite values enclose this one. Zero at the outermost <c>value</c>.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcArrayValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     The elements sit one level deeper than this array, which is what bounds the cycle back
    ///     through <see cref="XmlRpcClient.MaxValueNestingDepth"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    internal bool Load(XPathNavigator source, int depth)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? dataNavigator = source.SelectSingleNode("array/data");
            if (dataNavigator is { HasChildren: true })
            {
                XPathNodeIterator valueIterator = dataNavigator.Select("value");
                if (valueIterator is { Count: > 0 })
                {
                    wasLoaded = this.AddValues(valueIterator, depth);
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Adds every value the supplied iterator yields that parses, and reports whether any did.
    /// </summary>
    /// <param name="iterator">An iterator over the <c>value</c> elements of this array's <c>data</c>.</param>
    /// <param name="depth">How many composite values enclose this array.</param>
    /// <returns><see langword="true"/> if at least one element was added; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Shared by <see cref="Load(XPathNavigator, int)"/> and the
    ///     <see cref="XmlRpcArrayValue(XPathNodeIterator)"/> constructor so that the depth bound cannot
    ///     be applied to one and forgotten on the other — which is exactly how the constructor came to
    ///     be a second, unguarded door into the same recursion.
    /// </remarks>
    private bool AddValues(XPathNodeIterator iterator, int depth)
    {
        bool wasLoaded = false;

        while (iterator.MoveNext())
        {
            XPathNavigator? valueNode = iterator.Current;
            if (valueNode is null)
            {
                continue;
            }

            if (XmlRpcClient.TryParseValue(valueNode, depth + 1, out IXmlRpcValue? value))
            {
                this.Values.Add(value);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcArrayValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("value");

        writer.WriteStartElement("array");

        writer.WriteStartElement("data");
        foreach (IXmlRpcValue value in this.Values)
        {
            value.WriteTo(writer);
        }
        writer.WriteEndElement();

        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcArrayValue"/>.
    /// </summary>
    /// <returns>The <c>&lt;value&gt;&lt;array&gt;</c> XML for the current instance, written as a fragment — no XML declaration.</returns>
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
    public int CompareTo(XmlRpcArrayValue? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = XmlRpcMessage.CompareSequence(this.Values, other.Values);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcArrayValue"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcArrayValue"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="XmlRpcArrayValue"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(XmlRpcArrayValue? other)
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
    public override bool Equals(object? obj) => obj is XmlRpcArrayValue other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     Folds each value's <see cref="IXmlRpcValue.ToString"/> — the same key
    ///     <see cref="XmlRpcMessage.CompareSequence"/> orders by, so equal arrays hash equally by
    ///     construction rather than by coincidence. Folding the element itself would delegate to its own
    ///     <see cref="object.GetHashCode"/>, which for <see cref="XmlRpcScalarValue"/> is built from
    ///     <see cref="XmlRpcScalarValue.ValueType"/> and <see cref="XmlRpcScalarValue.Value"/> and can
    ///     therefore separate two values whose XML — and so whose equality — is identical.
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Values.Count));
        foreach (IXmlRpcValue value in this.Values)
        {
            hash.Add(HashCodeUtility.Component(value?.ToString()));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(XmlRpcArrayValue? first, XmlRpcArrayValue? second)
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
    public static bool operator !=(XmlRpcArrayValue? first, XmlRpcArrayValue? second) => !(first == second);

}