using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a remote procedure call that can be sent using the <see cref="XmlRpcClient"/> class.
/// </summary>
/// <remarks>
///     A <c>&lt;methodCall&gt;</c>: a method name and an ordered, unnamed parameter list. Parameters are
///     positional — XML-RPC has no named arguments, and <c>&lt;params&gt;</c> is omitted entirely rather
///     than written empty when there are none.
/// </remarks>
/// <seealso cref="XmlRpcClient.SendAsync"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs" language="cs" title="The following code example demonstrates the usage of the XmlRpcMessage class." />
/// </example>
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
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="methodName"/> is an empty string.</exception>
    public XmlRpcMessage(string methodName)
    {
        this.MethodName = methodName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcMessage"/> class using the specified method name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method to be called.</param>
    /// <param name="parameters">An <see cref="IEnumerable{T}"/> collection of <see cref="IXmlRpcValue"/> objects that represent the method parameters.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="methodName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="methodName"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="parameters"/> is <see langword="null"/>.</exception>
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
    /// <value>The character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
    /// <remarks>
    ///     Written into the XML declaration of the request body and repeated as the <c>charset</c>
    ///     parameter of its <c>Content-Type</c>. Changing it changes what a server that ignores the
    ///     declaration and trusts the header will read, so the two are kept deliberately in step.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
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
    /// <value>The method name, trimmed. The default value is an <i>empty</i> string, which a server will reject; the setter refuses to restore it.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
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
    /// <returns>
    ///     <c>1</c> if <paramref name="source"/> holds more elements than <paramref name="target"/>;
    ///     <c>-1</c> if it holds fewer; otherwise the lexical relationship between the two sequences,
    ///     compared element by element.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Equal counts are compared <i>positionally</i>, each element against the one at the same
    ///         index, which is the protocol's own notion of sameness: XML-RPC parameters are ordered.
    ///         Elements are ordered by <see cref="IXmlRpcValue.ToString"/> under
    ///         <see cref="StringComparison.Ordinal"/> — the same key
    ///         <see cref="XmlRpcScalarValue.CompareTo"/> and
    ///         <see cref="XmlRpcStructureMember.CompareTo"/> already use, and the only total ordering
    ///         that crosses the interface's three implementations.
    ///     </para>
    ///     <para>
    ///         This used to walk <paramref name="source"/> asking <c>!target.Contains(element)</c>. That
    ///         loop has only two answers — <c>-1</c> when an element is absent, <c>0</c> otherwise — so
    ///         two equal-length collections with disjoint contents each reported <i>themselves</i> the
    ///         lesser, and a reordering compared equal while
    ///         <see cref="XmlRpcArrayValue.GetHashCode"/> folded the values in order.
    ///     </para>
    ///     <para>
    ///         <b>The consequence was a silently arbitrary order, not a crash.</b> On .NET 10 the
    ///         introsort partition loop carries bounds guards, so <see cref="List{T}.Sort()"/> returns
    ///         quietly on such a comparer rather than throwing the
    ///         <see cref="IndexOutOfRangeException"/> older runtimes did — and what it returned depended
    ///         on the order the elements happened to arrive in. That is worse-shaped than a crash,
    ///         because nothing announces it.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<IXmlRpcValue> source, IList<IXmlRpcValue> target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        return ComparisonUtility.CompareSequence(
            source,
            target,
            static (first, second) => string.Compare(first?.ToString(), second?.ToString(), StringComparison.Ordinal));
    }

    /// <summary>
    /// Loads this <see cref="XmlRpcMessage"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcMessage"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcMessage"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    /// <returns>The <c>&lt;methodCall&gt;</c> XML for the current instance, written as a fragment — no XML declaration, and not in <see cref="Encoding"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="XmlRpcMessage"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(XmlRpcMessage? first, XmlRpcMessage? second) => !(first == second);
}