using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// The data type a sortable list property should be compared as.
/// </summary>
/// <remarks>
///     It tells a client how to order the values, not what they are: <c>10</c> sorts after <c>9</c> as a
///     <see cref="Number"/> and before it as <see cref="Text"/>. Getting it wrong produces an order that
///     looks plausible and is wrong only in the middle of the list.
/// </remarks>
/// <seealso cref="SimpleListSort.DataType"/>
/// <seealso cref="SimpleListSort.DataTypeAsString(SimpleListDataType)"/>
/// <seealso cref="SimpleListSort.DataTypeByName(string)"/>
public enum SimpleListDataType
{
    /// <summary>
    /// No data type. Suppresses the attribute on write; a client should fall back to <see cref="Text"/>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Sort chronologically. Written as <c>date</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Date", AlternateValue = "date")]
    Date = 1,

    /// <summary>
    /// Sort numerically. Written as <c>number</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Number", AlternateValue = "number")]
    Number = 2,

    /// <summary>
    /// Sort lexicographically. Written as <c>text</c>, and the assumed default when none is given.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Text", AlternateValue = "text")]
    Text = 3
}