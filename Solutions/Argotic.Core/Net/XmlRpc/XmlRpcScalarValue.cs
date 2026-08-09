using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a scalar remote procedure parameter value.
/// </summary>
/// <remarks>
///     The leaf of the value tree: one of the seven types <see cref="XmlRpcScalarValueType"/> names, or
///     an untyped string. <see cref="Value"/> is <see cref="object"/> and <see cref="ValueType"/> is set
///     independently of it, so the pair can be made inconsistent; the typed constructors set both
///     together and are the way to avoid it.
/// </remarks>
/// <seealso cref="XmlRpcMessage.Parameters"/>
/// <seealso cref="IXmlRpcValue"/>
public class XmlRpcScalarValue : IXmlRpcValue, IComparable<XmlRpcScalarValue>, IEquatable<XmlRpcScalarValue>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class.
    /// </summary>
    public XmlRpcScalarValue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified array of bytes.
    /// </summary>
    /// <param name="value">An array of 8-bit unsigned integers.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Base64"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is <see langword="null"/>.</exception>
    public XmlRpcScalarValue(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        this.ValueType = XmlRpcScalarValueType.Base64;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified boolean value.
    /// </summary>
    /// <param name="value">A boolean value.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Boolean"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    /// </remarks>
    public XmlRpcScalarValue(bool value)
    {
        this.ValueType = XmlRpcScalarValueType.Boolean;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified instance in time.
    /// </summary>
    /// <param name="value">The instant. Give it a <see cref="DateTimeKind"/> you mean: a <see cref="DateTimeKind.Local"/> value is written with its numeric offset, and anything else — <see cref="DateTimeKind.Utc"/> and <see cref="DateTimeKind.Unspecified"/> alike — is written with a <c>Z</c>. An <see cref="DateTimeKind.Unspecified"/> local time is therefore published as UTC, silently.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.DateTime"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    ///     Despite the element being named <c>dateTime.iso8601</c>, what is written is
    ///     <a href="https://www.rfc-editor.org/rfc/rfc3339.html">RFC 3339</a> — a deliberate departure,
    ///     because that is what live servers emit despite the name, and it is what the reader tries first.
    /// </remarks>
    public XmlRpcScalarValue(DateTime value)
    {
        this.ValueType = XmlRpcScalarValueType.DateTime;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified double-precision signed floating-point number.
    /// </summary>
    /// <param name="value">A double-precision signed floating-point number.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Double"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    /// </remarks>
    public XmlRpcScalarValue(double value)
    {
        this.ValueType = XmlRpcScalarValueType.Double;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified 32-bit signed integer.
    /// </summary>
    /// <param name="value">A 32-bit signed integer.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.Integer"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    /// </remarks>
    public XmlRpcScalarValue(int value)
    {
        this.ValueType = XmlRpcScalarValueType.Integer;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcScalarValue"/> class using the specified series of characters.
    /// </summary>
    /// <param name="value">A series of characters.</param>
    /// <remarks>
    ///     This constructor sets the <see cref="ValueType"/> property to be <see cref="XmlRpcScalarValueType.String"/>,
    ///     and sets the <see cref="Value"/> property using the supplied <paramref name="value"/>.
    /// </remarks>
    public XmlRpcScalarValue(string value)
    {
        this.ValueType = XmlRpcScalarValueType.String;
        this.Value = !string.IsNullOrEmpty(value) ? value : string.Empty;
    }

    /// <summary>
    /// Gets or sets the value of this parameter.
    /// </summary>
    /// <value>
    ///     The value, boxed. The default value is <see langword="null"/>; the setter refuses to restore
    ///     it, so <see langword="null"/> means only "never set".
    /// </value>
    /// <remarks>
    ///     The runtime type is not checked against <see cref="ValueType"/> here — it is checked when the
    ///     value is written, by <see cref="Convert"/>, and a mismatch surfaces there as an
    ///     <see cref="InvalidCastException"/> or <see cref="FormatException"/> from
    ///     <see cref="WriteTo(XmlWriter)"/> rather than from the assignment that caused it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public object? Value
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of scalar value this parameter represents.
    /// </summary>
    /// <value>
    ///     The scalar type. The default value is <see cref="XmlRpcScalarValueType.None"/>, meaning the
    ///     element carried no type designator.
    /// </value>
    /// <remarks>
    ///     <see cref="XmlRpcScalarValueType.None"/> is not the same as
    ///     <see cref="XmlRpcScalarValueType.String"/>, even though the specification says an untyped
    ///     value is a string. <see cref="WriteTo(XmlWriter)"/> writes a bare
    ///     <c>&lt;value&gt;text&lt;/value&gt;</c> for <c>None</c> and wraps the text in
    ///     <c>&lt;string&gt;</c> otherwise, so keeping the distinction is what lets an untyped value
    ///     round-trip as the untyped value it arrived as.
    /// </remarks>
    /// <seealso cref="XmlRpcClient.ScalarTypeAsString(XmlRpcScalarValueType)"/>
    /// <seealso cref="XmlRpcClient.ScalarTypeByName(string)"/>
    public XmlRpcScalarValueType ValueType { get; set; } = XmlRpcScalarValueType.None;

    /// <summary>
    /// Loads this <see cref="XmlRpcScalarValue"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from, positioned on a <c>value</c> element.</param>
    /// <returns><see langword="true"/> if the <see cref="XmlRpcScalarValue"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML
    ///     element that represents a <see cref="XmlRpcScalarValue"/> — that is, on a <c>value</c>. A
    ///     navigator positioned anywhere else fails rather than guessing.
    ///     </para>
    ///     <para>
    ///     The parsing is <see cref="XmlRpcClient.TryParseValue(XPathNavigator, out IXmlRpcValue?)"/>'s,
    ///     deliberately. This method used to carry a second implementation, and the two disagreed on six
    ///     of the seven scalar types: four of this one's arms reached <c>int.Parse</c>,
    ///     <c>double.Parse</c>, <c>Convert.FromBase64String</c> and RFC 3339 date parsing, all of which
    ///     <i>throw</i> — out of a method whose contract is a <see cref="bool"/> — while the other
    ///     stored the empty string for an unparseable boolean and reported success. The date table was
    ///     the plainest symptom: it could not read <c>19980717T14:08:55</c>, the spelling XML-RPC 1.0
    ///     prints in its own example.
    ///     </para>
    ///     <para>
    ///     One consequence of the delegation is worth stating, because it is visible: this method no
    ///     longer trims a <c>string</c> value on the way in, matching the parser everything else uses.
    ///     <see cref="WriteTo(XmlWriter)"/> has always trimmed on the way out, so the wire form is
    ///     unchanged.
    ///     </para>
    ///     <para>
    ///     A <c>struct</c> or <c>array</c> is not a scalar and fails here, as it always did.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!XmlRpcClient.TryParseValue(source, out IXmlRpcValue? parsed) || parsed is not XmlRpcScalarValue scalar)
        {
            return false;
        }

        // The one place the two paths must still differ. TryParseValue types an untyped
        // <value>text</value> as String; this type keeps None for it, because WriteTo emits a bare
        // <value>text</value> for None and a <string> wrapper otherwise -- so keeping None is what
        // lets an untyped value round-trip as the untyped value it arrived as. MoveToChild(Element)
        // on a private navigator is how "was there a type designator" is asked without disturbing
        // the caller's position.
        bool wasTyped = source.CreateNavigator().MoveToChild(XPathNodeType.Element);

        this.ValueType = wasTyped ? scalar.ValueType : XmlRpcScalarValueType.None;
        if (scalar.Value is not null)
        {
            this.Value = scalar.Value;
        }

        return true;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcScalarValue"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("value");

        if (this.ValueType != XmlRpcScalarValueType.None)
        {
            writer.WriteStartElement(XmlRpcClient.ScalarTypeAsString(this.ValueType));
            writer.WriteString(XmlRpcScalarValue.ValueAsString(this.ValueType, this.Value));
            writer.WriteEndElement();
        }
        else
        {
            writer.WriteString(this.Value?.ToString() ?? string.Empty);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="XmlRpcScalarValue"/>.
    /// </summary>
    /// <returns>The <c>&lt;value&gt;</c> XML for the current instance, written as a fragment — no XML declaration.</returns>
    /// <remarks>
    ///     This is also the basis of equality and ordering for the type: <see cref="CompareTo"/> works
    ///     from this string, so a value's declared type is part of what makes it equal to another.
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
    public int CompareTo(XmlRpcScalarValue? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ToString(), other.ToString(), StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcScalarValue"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcScalarValue"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="XmlRpcScalarValue"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(XmlRpcScalarValue? other)
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
    public override bool Equals(object? obj) => obj is XmlRpcScalarValue other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.ValueType), HashCodeUtility.Component(this.Value));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(XmlRpcScalarValue? first, XmlRpcScalarValue? second)
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
    public static bool operator !=(XmlRpcScalarValue? first, XmlRpcScalarValue? second) => !(first == second);

    /// <summary>
    /// Returns the string representation of the supplied scalar value using the specified <see cref="XmlRpcScalarValueType"/>.
    /// </summary>
    /// <param name="type">The data type used to determine string representation.</param>
    /// <param name="scalar">The scalar value to convert.</param>
    /// <returns>The string representation of the current instance's <see cref="Value"/>, based on its <see cref="ValueType"/>.</returns>
    private static string? ValueAsString(XmlRpcScalarValueType type, object? scalar)
    {
        if (scalar is null)
        {
            return string.Empty;
        }

        // The discard arm preserves the original behaviour: an unrecognised type fell through
        // the switch and returned the empty string that `value` was initialised to.
        return type switch
        {
            XmlRpcScalarValueType.Base64 => scalar is byte[] data
                ? Convert.ToBase64String(data, Base64FormattingOptions.None)
                : Convert.ToString(scalar, CultureInfo.InvariantCulture) ?? string.Empty,
            XmlRpcScalarValueType.Boolean => Convert.ToBoolean(scalar, CultureInfo.InvariantCulture) ? "1" : "0",
            XmlRpcScalarValueType.DateTime => SyndicationDateTimeUtility.ToRfc3339DateTime(Convert.ToDateTime(scalar, DateTimeFormatInfo.InvariantInfo)),
            XmlRpcScalarValueType.Double => Convert.ToDouble(scalar, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo),
            XmlRpcScalarValueType.Integer => Convert.ToInt32(scalar, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo),
            XmlRpcScalarValueType.String => (Convert.ToString(scalar, CultureInfo.InvariantCulture) ?? string.Empty).Trim(),
            _ => string.Empty,
        };
    }
}