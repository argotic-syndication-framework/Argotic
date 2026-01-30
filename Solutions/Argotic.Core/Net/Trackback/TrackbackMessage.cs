using System.Collections.Specialized;
using System.Text;
using System.Web;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents a Trackback ping request that can be sent using the <see cref="TrackbackClient"/> class.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackMessage class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs"
///             region="TrackbackClient"
///         />
///     </code>
/// </example>
[Serializable]
public class TrackbackMessage : IComparable<TrackbackMessage>, IEquatable<TrackbackMessage>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the character encoding of the message.
    /// </summary>
    private Encoding messageEncoding = Encoding.UTF8;

    /// <summary>
    /// Private member to hold the title of the entry.
    /// </summary>
    private string messageTitle = string.Empty;

    /// <summary>
    /// Private member to hold an excerpt of the entry.
    /// </summary>
    private string messageExcerpt = string.Empty;

    /// <summary>
    /// Private member to hold the name of the weblog to which the entry was posted.
    /// </summary>
    private string messageWeblogName = string.Empty;

    /// <summary>
    /// Private member to hold the permalink for the entry.
    /// </summary>
    private Uri messageUrl;

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
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public TrackbackMessage(Uri permalink)
    {
        this.Permalink = permalink;
    }

    /// <summary>
    /// Gets or sets the <see cref="Encoding">character encoding</see> of this message.
    /// </summary>
    /// <value>A <see cref="Encoding"/> that specifies the character encoding of this message. The default value is <see cref="UTF8Encoding">UTF-8</see>.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Encoding Encoding
    {
        get
        {
            return messageEncoding;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            messageEncoding = value;
        }
    }

    /// <summary>
    /// Gets or sets an excerpt for the entry.
    /// </summary>
    /// <value>An excerpt of the entry.</value>
    /// <remarks>
    ///     The excerpt <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
    /// </remarks>
    public string Excerpt
    {
        get
        {
            return messageExcerpt;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                messageExcerpt = string.Empty;
            }
            else
            {
                messageExcerpt = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets the permalink for the entry.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the permalink for the entry.</value>
    /// <remarks>
    ///     The permalink should point as closely as possible to the actual entry on the HTML page, as it will be used when linking to the entry in question.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Permalink
    {
        get
        {
            return messageUrl;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            messageUrl = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the entry.
    /// </summary>
    /// <value>The title of the entry.</value>
    /// <remarks>
    ///     The title <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
    /// </remarks>
    public string Title
    {
        get
        {
            return messageTitle;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                messageTitle = string.Empty;
            }
            else
            {
                messageTitle = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the weblog to which the entry was posted.
    /// </summary>
    /// <value>The name of the weblog to which the entry was posted.</value>
    /// <remarks>
    ///     The blog name <b>must</b> be in the character encoding specified by the <see cref="Encoding"/> property.
    /// </remarks>
    public string WeblogName
    {
        get
        {
            return messageWeblogName;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                messageWeblogName = string.Empty;
            }
            else
            {
                messageWeblogName = value.Trim();
            }
        }
    }

    /// <summary>
    /// Loads this <see cref="TrackbackMessage"/> using the supplied <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="TrackbackMessage"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be the <see cref="HttpRequest.Params">HTTP Request Parameters</see> or a similar subset.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(NameValueCollection source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.Count > 0)
        {
            foreach (string parameterName in source.AllKeys)
            {
                if (string.Equals(parameterName, "url", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(source[parameterName], UriKind.RelativeOrAbsolute, out Uri url))
                    {
                        this.Permalink = url;
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "title", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(source[parameterName]))
                    {
                        this.Title = source[parameterName];
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "excerpt", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(source[parameterName]))
                    {
                        this.Excerpt = source[parameterName];
                        wasLoaded = true;
                    }
                }
                else if (string.Equals(parameterName, "blog_name", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(source[parameterName]))
                    {
                        this.WeblogName = source[parameterName];
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
    /// <param name="writer">The <see cref="StreamWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(StreamWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write($"url={this.Permalink?.ToString() ?? string.Empty}");

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
    /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
    /// <remarks>
    ///     This method returns the URL-encoded representation for the current instance.
    /// </remarks>
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
        result |= string.Compare(this.Excerpt, other.Excerpt, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Permalink, other.Permalink, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.WeblogName, other.WeblogName, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrackbackMessage"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TrackbackMessage"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="TrackbackMessage"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is TrackbackMessage other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            this.Encoding?.WebName,
            this.Excerpt,
            this.Permalink,
            this.Title,
            this.WeblogName);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(TrackbackMessage first, TrackbackMessage second)
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
    public static bool operator !=(TrackbackMessage first, TrackbackMessage second)
    {
        return !(first == second);
    }
}