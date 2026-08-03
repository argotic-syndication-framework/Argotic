using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents the response to a Trackback ping request.
/// </summary>
/// <seealso cref="TrackbackClient.Send(TrackbackMessage)"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackResponse class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs"
///             region="TrackbackClient"
///         />
///     </code>
/// </example>
[Serializable]
public class TrackbackResponse : IComparable<TrackbackResponse>, IEquatable<TrackbackResponse>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold a value indicating if the Trackback ping request failed.
    /// </summary>
    private bool responseHasError;

    /// <summary>
    /// Private member to hold information about the cause of the Trackback ping request failure.
    /// </summary>
    private string responseErrorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackResponse"/> class.
    /// </summary>
    /// <remarks>
    ///     The default instance of the <see cref="TrackbackResponse"/> class represents the response to a successful ping request.
    /// </remarks>
    public TrackbackResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackResponse"/> class using the supplied error message.
    /// </summary>
    /// <param name="errorMessage">Information about cause of the Trackback ping request failure.</param>
    /// <remarks>
    ///     The <paramref name="errorMessage"/> <b>must</b> be provided in a <b>UTF-8</b> character encoding.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is an empty string.</exception>
    public TrackbackResponse(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        responseErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TrackbackResponse"/> class asynchronously using the supplied <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="response">An <see cref="HttpResponseMessage"/> object that represents the Trackback server's response to the ping request.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="TrackbackResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content type.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content length.</exception>
    /// <exception cref="XmlException">The <paramref name="response"/> body does not represent a valid XML document, or an error was encountered in the XML data.</exception>
    public static async Task<TrackbackResponse> CreateAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        string? contentType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(contentType, "text/xml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"The HttpResponseMessage content type is invalid. Content type of the response was {contentType}", nameof(response));
        }

        // Note: The Trackback spec does not require Content-Length. HTTP/1.1 (RFC 7230)
        // allows Transfer-Encoding: chunked as an alternative, which doesn't include
        // Content-Length (-1 here). We only reject explicitly empty responses (0).
        long contentLength = response.Content.Headers.ContentLength ?? -1;
        if (contentLength == 0)
        {
            throw new ArgumentException($"The HttpResponseMessage content length is invalid. Content length was {contentLength}. ", nameof(response));
        }

        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(reader);
        XPathNavigator source = document.CreateNavigator();

        TrackbackResponse result = new();
        XPathNavigator responseNavigator = source.SelectSingleNode("response");
        if (responseNavigator != null)
        {
            result.Load(responseNavigator);
        }

        return result;
    }

    /// <summary>
    /// Gets information about cause of the Trackback ping request failure.
    /// </summary>
    /// <value>Information about the cause of the Trackback ping request failure. The default value is an <b>empty</b> string.</value>
    public string ErrorMessage
    {
        get
        {
            return responseErrorMessage;
        }
    }

    /// <summary>
    /// Gets a value indicating if the Trackback ping request failed.
    /// </summary>
    /// <value><b>true</b> if the Trackback ping response contains an error indicator; Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    public bool HasError
    {
        get
        {
            return responseHasError;
        }
    }

    /// <summary>
    /// Loads this <see cref="TrackbackResponse"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="TrackbackResponse"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="TrackbackResponse"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator errorNavigator = source.SelectSingleNode("error");
            XPathNavigator messageNavigator = source.SelectSingleNode("message");

            if (errorNavigator != null)
            {
                if (string.Equals(errorNavigator.Value, "0", StringComparison.OrdinalIgnoreCase))
                {
                    responseHasError = false;
                    wasLoaded = true;
                }
                else if (string.Equals(errorNavigator.Value, "1", StringComparison.OrdinalIgnoreCase))
                {
                    responseHasError = true;
                    wasLoaded = true;
                }
            }

            if (messageNavigator != null)
            {
                responseErrorMessage = !string.IsNullOrEmpty(messageNavigator.Value) ? messageNavigator.Value : string.Empty;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="TrackbackResponse"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("response");

        if (!string.IsNullOrEmpty(this.ErrorMessage))
        {
            writer.WriteElementString("error", "1");
            writer.WriteElementString("message", this.ErrorMessage);
        }
        else
        {
            writer.WriteElementString("error", "0");
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackMessage"/>.</returns>
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
    public int CompareTo(TrackbackResponse? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ErrorMessage, other.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.HasError.CompareTo(other.HasError);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrackbackResponse"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TrackbackResponse"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="TrackbackResponse"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(TrackbackResponse? other)
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
        return obj is TrackbackResponse other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.ErrorMessage), HashCodeUtility.Component(this.HasError));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(TrackbackResponse first, TrackbackResponse second)
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
    public static bool operator !=(TrackbackResponse first, TrackbackResponse second)
    {
        return !(first == second);
    }
}