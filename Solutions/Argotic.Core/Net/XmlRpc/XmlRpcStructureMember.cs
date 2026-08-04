using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a structured list member.
/// </summary>
[Serializable]
public class XmlRpcStructureMember : IComparable<XmlRpcStructureMember>, IEquatable<XmlRpcStructureMember>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcStructureMember"/> class.
    /// </summary>
    public XmlRpcStructureMember()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcStructureMember"/> class using the specified name and value.
    /// </summary>
    /// <param name="name">The name of this structure member.</param>
    /// <param name="value">An object that implements the <see cref="IXmlRpcValue"/> interface that represents the value of this structure member.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public XmlRpcStructureMember(string name, IXmlRpcValue value)
    {
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the name of this structure member.
    /// </summary>
    /// <value>The name of this structure member.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the value of this structure member.
    /// </summary>
    /// <value>An object that implements the <see cref="IXmlRpcValue"/> interface that represents the value of this structure member.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public IXmlRpcValue? Value
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="XmlRpcStructureMember"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="XmlRpcStructureMember"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcStructureMember"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? nameNavigator = source.SelectChildElement("name");
            XPathNavigator? valueNavigator = source.SelectChildElement("value");

            if (nameNavigator is not null && !string.IsNullOrEmpty(nameNavigator.Value))
            {
                this.Name = nameNavigator.Value;
                wasLoaded = true;
            }

            if (valueNavigator is not null)
            {
                if (XmlRpcClient.TryParseValue(valueNavigator, out IXmlRpcValue? value))
                {
                    this.Value = value;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcStructureMember"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("member");

        writer.WriteElementString("name", this.Name);

        if (this.Value is not null)
        {
            this.Value.WriteTo(writer);
        }
        else
        {
            writer.WriteElementString("value", string.Empty);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcStructureMember"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcStructureMember"/>.</returns>
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
    public int CompareTo(XmlRpcStructureMember? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ToString(), other.ToString(), StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcStructureMember"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcStructureMember"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="XmlRpcStructureMember"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(XmlRpcStructureMember? other)
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
    public override bool Equals(object? obj) => obj is XmlRpcStructureMember other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => this.ToString().GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(XmlRpcStructureMember? first, XmlRpcStructureMember? second)
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
    public static bool operator !=(XmlRpcStructureMember? first, XmlRpcStructureMember? second) => !(first == second);
}