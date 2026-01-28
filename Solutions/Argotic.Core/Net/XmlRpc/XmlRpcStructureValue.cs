using System.Xml;
using System.Xml.XPath;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure parameter value that is typically used to encapsulate small groups of related variables.
/// </summary>
/// <seealso cref="XmlRpcMessage.Parameters"/>
/// <seealso cref="IXmlRpcValue"/>
[Serializable]
public class XmlRpcStructureValue : IXmlRpcValue, IComparable<XmlRpcStructureValue>, IEquatable<XmlRpcStructureValue>
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
    /// <param name="iterator">A <see cref="XPathNodeIterator"/> that represents the <i>member</i> nodes for the structured list.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is a null reference.</exception>
    public XmlRpcStructureValue(XPathNodeIterator iterator)
    {
        ArgumentNullException.ThrowIfNull(iterator);

        if (iterator.Count > 0)
        {
            while (iterator.MoveNext())
            {
                XmlRpcStructureMember member = new();
                if (member.Load(iterator.Current))
                {
                    this.Members.Add(member);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="XmlRpcStructureMember"/> that has the specified name.
    /// </summary>
    /// <param name="name">The name that uniquely identifies the member to get or set.</param>
    /// <returns>The <see cref="XmlRpcStructureMember"/> with the specified name.</returns>
    /// <remarks>
    ///     If no member exists for the specified <paramref name="name"/>, returns a <b>null</b> reference.
    ///     This indexer uses a <i>case-insensitive</i> comparison of the specified member <paramref name="name"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public XmlRpcStructureMember this[string name]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            XmlRpcStructureMember result = null;

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
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
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
    /// <returns><b>true</b> if the <see cref="XmlRpcStructureValue"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcStructureValue"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNodeIterator memberIterator = source.Select("struct/member");
            if (memberIterator is { Count: > 0 })
            {
                while (memberIterator.MoveNext())
                {
                    XmlRpcStructureMember member = new();
                    if (member.Load(memberIterator.Current))
                    {
                        this.Members.Add(member);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcStructureValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcStructureValue"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcStructureValue"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

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
    /// <returns><b>true</b> if the specified <see cref="XmlRpcStructureValue"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is XmlRpcStructureValue other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Members.Count);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(XmlRpcStructureValue first, XmlRpcStructureValue second)
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
    public static bool operator !=(XmlRpcStructureValue first, XmlRpcStructureValue second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(XmlRpcStructureValue first, XmlRpcStructureValue second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(XmlRpcStructureValue first, XmlRpcStructureValue second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(XmlRpcStructureValue first, XmlRpcStructureValue second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(XmlRpcStructureValue first, XmlRpcStructureValue second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}