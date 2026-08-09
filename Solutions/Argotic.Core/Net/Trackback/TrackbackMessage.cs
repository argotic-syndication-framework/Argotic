using System.Collections.Specialized;
using System.Text;
using System.Web;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a Trackback ping request that can be sent using the <see cref="TrackbackClient"/> class.
/// </summary>
/// <remarks>
///     Four fields, written as an <c>application/x-www-form-urlencoded</c> body: <c>url</c> from
///     <see cref="Permalink"/>, and the optional <c>title</c>, <c>blog_name</c> and <c>excerpt</c>.
///     Only <c>url</c> is always emitted; the rest are omitted when empty rather than sent blank.
///     <see cref="Load(NameValueCollection)"/> reads the same four names back, case-insensitively.
/// </remarks>
/// <seealso cref="TrackbackClient.SendAsync"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs" language="cs" title="The following code example demonstrates the usage of the TrackbackMessage class." />
/// </example>
public class TrackbackMessage : IComparable<TrackbackMessage>, IEquatable<TrackbackMessage>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackMessage"/> class.
    /// </summary>
    public TrackbackMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackMessage"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="permalink">A <see cref="Uri"/> that represents the permalink for the entry.</param>
    /// <remarks>
    ///     The <paramref name="permalink"/> should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="permalink"/> is <see langword="null"/>.</exception>
    public TrackbackMessage(Uri permalink)
    {
        this.Permalink = permalink;
    }

    /// <summary>
    /// Gets or sets the <see cref="Encoding">character encoding</see> of this message.
    /// </summary>
    /// <value>The character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
    /// <remarks>
    ///     Travels as the <c>charset</c> parameter of the request's <c>Content-Type</c>, and encodes the
    ///     body <see cref="TrackbackClient"/> writes. A byte-order mark is stripped before the body is
    ///     sent: emitted, it would arrive as part of the name of the first field.
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
    /// Gets or sets an excerpt for the entry.
    /// </summary>
    /// <value>The excerpt, trimmed. The default value is an <i>empty</i> string, and setting <see langword="null"/> restores it rather than throwing.</value>
    /// <remarks>Written percent-encoded as the <c>excerpt</c> field, and omitted entirely when empty.</remarks>
    public string Excerpt
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the permalink for the entry.
    /// </summary>
    /// <value>The permalink for the entry. The default value is <see langword="null"/>, which writes an <i>empty</i> <c>url</c> field — the one field always emitted.</value>
    /// <remarks>
    ///     The permalink should point as closely as possible to the actual entry on the HTML page, as it
    ///     will be used when linking to the entry in question. It is percent-encoded on the way out: a
    ///     permalink carrying an ordinary query string would otherwise split into extra form fields and
    ///     arrive truncated at the first ampersand.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Permalink
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the entry.
    /// </summary>
    /// <value>The title, trimmed. The default value is an <i>empty</i> string, and setting <see langword="null"/> restores it rather than throwing.</value>
    /// <remarks>Written percent-encoded as the <c>title</c> field, and omitted entirely when empty.</remarks>
    public string Title
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the weblog to which the entry was posted.
    /// </summary>
    /// <value>The weblog name, trimmed. The default value is an <i>empty</i> string, and setting <see langword="null"/> restores it rather than throwing.</value>
    /// <remarks>Written percent-encoded as the <c>blog_name</c> field — note the underscore — and omitted entirely when empty.</remarks>
    public string WeblogName
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="TrackbackMessage"/> using the supplied <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="TrackbackMessage"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This is the receiving half: the collection is the form fields of an inbound ping, such as
    ///         an ASP.NET request's parsed body. Keys are matched case-insensitively against <c>url</c>,
    ///         <c>title</c>, <c>excerpt</c> and <c>blog_name</c>; anything else present is ignored, and a
    ///         field whose value is empty leaves the corresponding property alone.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(NameValueCollection source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.Count > 0)
        {
            foreach (string? parameterName in source.AllKeys)
            {
                if (string.Equals(parameterName, "url", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(source[parameterName], UriKind.RelativeOrAbsolute, out Uri? url))
                    {
                        this.Permalink = url;
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "title", StringComparison.OrdinalIgnoreCase))
                {
                    string? titleValue = source[parameterName];
                    if (!string.IsNullOrEmpty(titleValue))
                    {
                        this.Title = titleValue;
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "excerpt", StringComparison.OrdinalIgnoreCase))
                {
                    string? excerptValue = source[parameterName];
                    if (!string.IsNullOrEmpty(excerptValue))
                    {
                        this.Excerpt = excerptValue;
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "blog_name", StringComparison.OrdinalIgnoreCase))
                {
                    string? weblogNameValue = source[parameterName];
                    if (!string.IsNullOrEmpty(weblogNameValue))
                    {
                        this.WeblogName = weblogNameValue;
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="TrackbackMessage"/> to the specified <see cref="StreamWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="StreamWriter"/> to which you want to save. Its encoding, not <see cref="Encoding"/>, is what reaches the stream.</param>
    /// <remarks>
    ///     Writes a form body, not XML: <c>url=…&amp;title=…&amp;blog_name=…&amp;excerpt=…</c>, each value
    ///     percent-encoded, with the optional three omitted when empty. Nothing is flushed here.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(StreamWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // The permalink is form-encoded like every other field. Written raw, a permalink carrying an
        // ordinary query string ("?id=1&ref=weekly") split into extra form fields and arrived at the
        // receiver truncated at the first ampersand.
        writer.Write($"url={HttpUtility.UrlEncode(this.Permalink?.ToString() ?? string.Empty)}");

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.Write($"&title={HttpUtility.UrlEncode(this.Title)}");
        }

        if (!string.IsNullOrEmpty(this.WeblogName))
        {
            writer.Write($"&blog_name={HttpUtility.UrlEncode(this.WeblogName)}");
        }

        if (!string.IsNullOrEmpty(this.Excerpt))
        {
            writer.Write($"&excerpt={HttpUtility.UrlEncode(this.Excerpt)}");
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.
    /// </summary>
    /// <returns>The form-encoded body for the current instance, as <see cref="WriteTo(StreamWriter)"/> would write it.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        using (StreamWriter writer = new(stream, this.Encoding))
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
    public int CompareTo(TrackbackMessage? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Encoding.WebName, other.Encoding.WebName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Excerpt, other.Excerpt, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Permalink, other.Permalink, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.WeblogName, other.WeblogName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrackbackMessage"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TrackbackMessage"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="TrackbackMessage"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(TrackbackMessage? other)
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
    public override bool Equals(object? obj) => obj is TrackbackMessage other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.Encoding?.WebName),
            HashCodeUtility.Component(this.Excerpt),
            HashCodeUtility.Component(this.Permalink),
            HashCodeUtility.Component(this.Title),
            HashCodeUtility.Component(this.WeblogName));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(TrackbackMessage? first, TrackbackMessage? second)
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
    public static bool operator !=(TrackbackMessage? first, TrackbackMessage? second) => !(first == second);
}