using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure call that can be sent using the <see cref="XmlRpcClient"/> class.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcMessage class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs"
///             region="XmlRpcClient"
///         />
///     </code>
/// </example>
[Serializable]
public class XmlRpcMessage : IComparable<XmlRpcMessage>, IEquatable<XmlRpcMessage>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class.
    /// </summary>
    public XmlRpcMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class using the specified method name.
    /// </summary>
    /// <param name="methodName">The name of the method to be called.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is an empty string.</exception>
    public XmlRpcMessage(string methodName)
    {
        this.MethodName = methodName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class using the specified method name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method to be called.</param>
    /// <param name="parameters">An <see cref="IEnumerable{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the method parameters.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="parameters"/> is a null reference.</exception>
    public XmlRpcMessage(string methodName, IEnumerable<IXmlRpcValue> parameters) : this(methodName)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        foreach (IXmlRpcValue parameter in parameters)
        {
            this.Parameters.Add(parameter);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Encoding">character encoding</see> of this message.
    /// </summary>
    /// <value>A <see cref="Encoding"/> that specifies the character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Encoding Encoding
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the name of the method to be called.
    /// </summary>
    /// <value>The name of the method to be called.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string MethodName
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets the method parameters.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the method parameters.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<IXmlRpcValue> Parameters { get; } = [];

    /// <summary>
    /// Compares two specified <see cref="IList{IXmlRpcValue}"/> collections.
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
    public static int CompareSequence(IList<IXmlRpcValue> source, IList<IXmlRpcValue> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                IXmlRpcValue value = source[i];
                if (!target.Contains(value))
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
    /// Loads this <see cref="XmlRpcMessage"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="XmlRpcMessage"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcMessage"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? methodNameNavigator = source.SelectChildElement("methodName");
            XPathNavigator? parametersNavigator = source.SelectChildElement("params");

            if (methodNameNavigator is not null && !string.IsNullOrEmpty(methodNameNavigator.Value))
            {
                this.MethodName = methodNameNavigator.Value;
                wasLoaded = true;
            }

            if (parametersNavigator is not null)
            {
                XPathNodeIterator valueIterator = parametersNavigator.Select("param/value");
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
                            this.Parameters.Add(value);
                            wasLoaded = true;
                        }
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcMessage"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("methodCall");

        writer.WriteElementString("methodName", this.MethodName);

        if (this.Parameters.Count > 0)
        {
            writer.WriteStartElement("params");
            foreach (IXmlRpcValue value in this.Parameters)
            {
                writer.WriteStartElement("param");
                value.WriteTo(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcMessage"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="XmlRpcMessage"/>.</returns>
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
    public int CompareTo(XmlRpcMessage? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Encoding.WebName, other.Encoding.WebName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.MethodName, other.MethodName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = XmlRpcMessage.CompareSequence(this.Parameters, other.Parameters);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcMessage"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcMessage"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="XmlRpcMessage"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(XmlRpcMessage? other)
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
    public override bool Equals(object? obj) => obj is XmlRpcMessage other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Encoding?.WebName), HashCodeUtility.Component(this.MethodName), HashCodeUtility.Component(this.Parameters.Count));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(XmlRpcMessage? first, XmlRpcMessage? second)
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
    public static bool operator !=(XmlRpcMessage? first, XmlRpcMessage? second) => !(first == second);
}