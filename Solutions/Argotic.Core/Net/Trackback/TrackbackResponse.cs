using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents the response to a Trackback ping request.
/// </summary>
/// <remarks>
///     The whole protocol response is two elements: <c>&lt;error&gt;</c>, holding <c>0</c> or <c>1</c>,
///     and an optional <c>&lt;message&gt;</c> explaining a rejection. There is no status code beyond
///     that, and a server that rejects a ping still answers <c>200 OK</c> — so a send that did not throw
///     tells you nothing about whether the ping was accepted. Read <see cref="HasError"/>.
/// </remarks>
/// <seealso cref="TrackbackClient.SendAsync(TrackbackMessage, CancellationToken)"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs" language="cs" title="The following code example demonstrates the usage of the TrackbackResponse class." />
/// </example>
public class TrackbackResponse : IComparable<TrackbackResponse>, IEquatable<TrackbackResponse>, IComparisonOperators
{

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
    /// <param name="errorMessage">Information about the cause of the Trackback ping request failure.</param>
    /// <remarks>
    ///     This constructor sets <see cref="ErrorMessage"/> but leaves <see cref="HasError"/> at
    ///     <see langword="false"/>. That mismatch is invisible on the wire — <see cref="WriteTo"/>
    ///     decides the <c>error</c> element from the message, not from the flag — but it is visible to a
    ///     caller who inspects the object, so treat a non-empty <see cref="ErrorMessage"/> as the failure
    ///     signal on an instance you constructed yourself.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="errorMessage"/> is an empty string.</exception>
    public TrackbackResponse(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TrackbackResponse"/> class asynchronously using the supplied <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="response">The Trackback server's response to the ping request. Its media type must be <c>text/xml</c>, and its content length must not be explicitly <c>0</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result is the parsed <see cref="TrackbackResponse"/>. A well-formed document with
    ///     no <c>response</c> element yields a default instance rather than an error, because that is
    ///     indistinguishable from a successful ping on this protocol.
    /// </returns>
    /// <remarks>
    ///     An absent <c>Content-Length</c> is accepted: the Trackback specification does not require the
    ///     header, and chunked transfer encoding omits it. Only an explicit <c>0</c> is rejected.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> media type is not <c>text/xml</c>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> declares a content length of <c>0</c>.</exception>
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

        // These are public and take a response the caller may have obtained under headers-read, in
        // which case the stream below is the socket and XmlReader.Create would read it synchronously.
        // Their own callers use content-read, so this is not a live defect - it is a public method that
        // stops being able to become one.
        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using Stream stream = body.AsStream();
        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(reader);
        XPathNavigator source = document.CreateNavigator();

        TrackbackResponse result = new();
        XPathNavigator? responseNavigator = source.SelectChildElement("response");
        if (responseNavigator is not null)
        {
            result.Load(responseNavigator);
        }

        return result;
    }

    /// <summary>
    /// Gets information about the cause of the Trackback ping request failure.
    /// </summary>
    /// <value>
    ///     The server's <c>message</c> element. The default value is <see langword="null"/>, and a
    ///     response that carried an empty <c>message</c> yields an <i>empty</i> string — the two are
    ///     distinguishable, and only <see langword="null"/> means the element was absent.
    /// </value>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the Trackback ping request failed.
    /// </summary>
    /// <value><see langword="true"/> if the response's <c>error</c> element held <c>1</c>; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.</value>
    /// <remarks>
    ///     A response carrying neither <c>0</c> nor <c>1</c> — or no <c>error</c> element at all — leaves
    ///     this <see langword="false"/>, so an unparseable response is indistinguishable from success
    ///     here. <see cref="Load(XPathNavigator)"/> returning <see langword="false"/> is what separates
    ///     them.
    /// </remarks>
    public bool HasError { get; private set; }

    /// <summary>
    /// Loads this <see cref="TrackbackResponse"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="TrackbackResponse"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="TrackbackResponse"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? errorNavigator = source.SelectChildElement("error");
            XPathNavigator? messageNavigator = source.SelectChildElement("message");

            if (errorNavigator is not null)
            {
                if (string.Equals(errorNavigator.Value, "0", StringComparison.OrdinalIgnoreCase))
                {
                    HasError = false;
                    wasLoaded = true;
                }
                else if (string.Equals(errorNavigator.Value, "1", StringComparison.OrdinalIgnoreCase))
                {
                    HasError = true;
                    wasLoaded = true;
                }
            }

            if (messageNavigator is not null)
            {
                ErrorMessage = !string.IsNullOrEmpty(messageNavigator.Value) ? messageNavigator.Value : string.Empty;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="TrackbackResponse"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <remarks>
    ///     The <c>error</c> element is derived from <see cref="ErrorMessage"/>, not from
    ///     <see cref="HasError"/>: a non-empty message writes <c>1</c> and the message, anything else
    ///     writes <c>0</c> alone. A response loaded with <c>error</c> <c>1</c> and no <c>message</c>
    ///     therefore writes back as a success.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackResponse"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance, as <see cref="WriteTo(XmlWriter)"/> would write it.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="TrackbackResponse"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is TrackbackResponse other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.ErrorMessage), HashCodeUtility.Component(this.HasError));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(TrackbackResponse? first, TrackbackResponse? second)
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
    public static bool operator !=(TrackbackResponse? first, TrackbackResponse? second) => !(first == second);
}