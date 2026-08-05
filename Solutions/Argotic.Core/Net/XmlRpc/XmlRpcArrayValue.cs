using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure parameter value that represents a collection of data elements.
/// </summary>
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
    /// <param name="iterator">A <see cref="XPathNodeIterator"/> that represents the <i>value</i> nodes for the array.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is a null reference.</exception>
    public XmlRpcArrayValue(XPathNodeIterator iterator)
    {
        ArgumentNullException.ThrowIfNull(iterator);

        if (iterator.Count > 0)
        {
            while (iterator.MoveNext())
            {
                XPathNavigator? iteratorNode = iterator.Current;
                if (iteratorNode is null)
                {
                    continue;
                }

                if (XmlRpcClient.TryParseValue(iteratorNode, out IXmlRpcValue? value))
                {
                    this.Values.Add(value);
                }
            }
        }
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
    /// <returns><b>true</b> if the <see cref="XmlRpcArrayValue"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcArrayValue"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
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
                    while (valueIterator.MoveNext())
                    {
                        XPathNavigator? valueNode = valueIterator.Current;
                        if (valueNode is null)
                        {
                            continue;
                        }

                        if (XmlRpcClient.TryParseValue(valueNode, out IXmlRpcValue? value))
                        {
                            this.Values.Add(value);
                            wasLoaded = true;
                        }
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcArrayValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcArrayValue"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    /// <returns><b>true</b> if the specified <see cref="XmlRpcArrayValue"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is XmlRpcArrayValue other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(HashCodeUtility.Component(this.Values.Count));
        foreach (var value in this.Values)
        {
            hash.Add(HashCodeUtility.Component(value));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(XmlRpcArrayValue? first, XmlRpcArrayValue? second) => !(first == second);

}