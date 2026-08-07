using System.Diagnostics.CodeAnalysis;
using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents the permissible data types for a <see cref="XmlRpcScalarValue"/>.
/// </summary>
/// <remarks>
///     Seven types and no more — XML-RPC has no unsigned, no 64-bit integer and no null. Each member
///     maps to the element name the specification uses, which is not always the member's own name:
///     <see cref="XmlRpcScalarValueType.Integer"/> is <c>int</c>, and
///     <see cref="XmlRpcScalarValueType.DateTime"/> is <c>dateTime.iso8601</c>. Use
///     <see cref="XmlRpcClient.ScalarTypeAsString(XmlRpcScalarValueType)"/> and
///     <see cref="XmlRpcClient.ScalarTypeByName(string)"/> to cross between the two rather than
///     lower-casing a member name.
/// </remarks>
/// <seealso cref="XmlRpcScalarValue.ValueType"/>
/// <seealso cref="XmlRpcScalarValue"/>
[SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification = "The XML-RPC specification names these scalar types <string>, <double>, <int> and <boolean>, and the members map one-to-one onto those wire values via EnumerationMetadata.AlternateValue. Renaming them to satisfy the rule would break the correspondence with the specification that makes this enum readable.")]
public enum XmlRpcScalarValueType
{
    /// <summary>
    /// The element carried no type designator, and by the specification is therefore a string.
    /// </summary>
    /// <remarks>
    ///     Kept distinct from <see cref="XmlRpcScalarValueType.String"/> so that an untyped value
    ///     written back stays untyped.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Binary data, carried as its base64 representation in a <c>base64</c> element.
    /// </summary>
    /// <remarks>
    ///     The only member whose <see cref="XmlRpcScalarValue.Value"/> is a <see cref="byte"/> array
    ///     rather than a primitive.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Base64", AlternateValue = "base64")]
    Base64 = 1,

    /// <summary>
    /// A logical boolean, carried in a <c>boolean</c> element.
    /// </summary>
    /// <remarks>
    ///     The specification spells it <c>1</c> and <c>0</c>, and that is what is written. On the way in
    ///     the words <c>true</c> and <c>false</c> are accepted too, because servers emit them.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Boolean", AlternateValue = "boolean")]
    Boolean = 2,

    /// <summary>
    /// An instant in time, carried in a <c>dateTime.iso8601</c> element.
    /// </summary>
    /// <remarks>
    ///     Written as RFC 3339 rather than as the basic ISO 8601 form the element's name implies, because
    ///     that is what live servers emit. A value read through
    ///     <see cref="XmlRpcClient.TryParseValue"/> accepts both spellings, and a zoneless one comes back
    ///     <see cref="System.DateTimeKind.Unspecified"/> — it names no offset to honour.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "DateTime", AlternateValue = "dateTime.iso8601")]
    DateTime = 3,

    /// <summary>
    /// A double-precision signed floating-point number, carried in a <c>double</c> element.
    /// </summary>
    /// <remarks>
    ///     Parsed and written with the invariant culture, so the decimal separator is always a full stop
    ///     whatever the machine's locale. XML-RPC 1.0 leaves precision and exponent notation explicitly
    ///     open, so what a server will accept is a matter of that server.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Double", AlternateValue = "double")]
    Double = 4,

    /// <summary>
    /// A 32-bit signed integer, carried in an <c>int</c> element.
    /// </summary>
    /// <remarks>
    ///     XML-RPC allows <c>i4</c> as a synonym. It is accepted on the way in and never written on the
    ///     way out, so a round-trip normalises <c>i4</c> to <c>int</c>.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Integer", AlternateValue = "int")]
    Integer = 5,

    /// <summary>
    /// Text, carried in a <c>string</c> element.
    /// </summary>
    /// <remarks>
    ///     Trimmed on both read and write. A string whose leading or trailing whitespace is significant
    ///     will not survive a round-trip.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "String", AlternateValue = "string")]
    String = 6
}