using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net;

/// <summary>
/// Represents the response to an XML remote procedure call.
/// </summary>
/// <seealso cref="XmlRpcClient.SendAsync(XmlRpcMessage, CancellationToken)"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcResponse class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs"
///             region="XmlRpcClient"
///         />
///     </code>
/// </example>
[Serializable]
public class XmlRpcResponse : IComparable<XmlRpcResponse>, IEquatable<XmlRpcResponse>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the response value that was returned for the remote procedure call.
    /// </summary>
    private IXmlRpcValue responseParameter;

    /// <summary>
    /// Private member to hold the response fault information.
    /// </summary>
    private XmlRpcStructureValue responseFault;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class.
    /// </summary>
    public XmlRpcResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="IXmlRpcValue"/>.
    /// </summary>
    /// <param name="parameter">A <see cref="IXmlRpcValue"/> that represents the response value that was returned for the remote procedure call.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="parameter"/> is a null reference.</exception>
    public XmlRpcResponse(IXmlRpcValue parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        responseParameter = parameter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied <see cref="XmlRpcStructureValue"/>.
    /// </summary>
    /// <param name="fault">A <see cref="XmlRpcStructureValue"/> that represents the response to the remote procedure call.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="fault"/> is a null reference.</exception>
    public XmlRpcResponse(XmlRpcStructureValue fault)
    {
        ArgumentNullException.ThrowIfNull(fault);

        responseFault = fault;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcResponse"/> class using the supplied fault code and message.
    /// </summary>
    /// <param name="faultCode">Machine-readable code that identifies the reason the remote procedure call failed.</param>
    /// <param name="faultMessage">Human-readable information about the reason the remote procedure call failed.</param>
    public XmlRpcResponse(int faultCode, string faultMessage)
    {
        XmlRpcStructureValue faultStructure = new();
        XmlRpcStructureMember codeMember = new("faultCode", new XmlRpcScalarValue(faultCode));
        XmlRpcStructureMember stringMember = new("faultString", new XmlRpcScalarValue(faultMessage));
        faultStructure.Members.Add(codeMember);
        faultStructure.Members.Add(stringMember);

        responseFault = faultStructure;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="XmlRpcResponse"/> class asynchronously using the supplied <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="response">An <see cref="HttpResponseMessage"/> object that represents the XML-RPC server's response to the remote procedure call.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="XmlRpcResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content type.</exception>
    /// <exception cref="ArgumentException">The <paramref name="response"/> has an invalid content length.</exception>
    /// <exception cref="XmlException">The <paramref name="response"/> body does not represent a valid XML document, or an error was encountered in the XML data.</exception>
    public static async Task<XmlRpcResponse> CreateAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        string? contentType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(contentType, "text/xml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"The HttpResponseMessage content type is invalid. Content type of the response was {contentType}", nameof(response));
        }

        // Note: The original XML-RPC spec (xmlrpc.com/spec.md) requires Content-Length,
        // but this predates HTTP/1.1 chunked transfer encoding (RFC 7230). Modern servers
        // commonly use Transfer-Encoding: chunked without Content-Length (-1 here).
        // We only reject explicitly empty responses (0) which would be invalid XML.
        long contentLength = response.Content.Headers.ContentLength ?? -1;
        if (contentLength == 0)
        {
            throw new ArgumentException($"The HttpResponseMessage content length is invalid. Content length was {contentLength}. ", nameof(response));
        }

        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(reader);
        XPathNavigator source = document.CreateNavigator();

        XmlRpcResponse result = new();
        XPathNavigator? methodResponseNavigator = source.SelectSingleNode("methodResponse");
        if (methodResponseNavigator != null)
        {
            result.Load(methodResponseNavigator);
        }

        return result;
    }

    /// <summary>
    /// Gets the fault information that was returned for the remote procedure call.
    /// </summary>
    /// <value>
    ///     A <see cref="XmlRpcStructureValue"/> that represents the fault information that was returned for the remote procedure call.
    ///     If the remote procedure call executed without errors, will return <b>null</b>.
    /// </value>
    /// <seealso cref="XmlRpcResponse(XmlRpcStructureValue)"/>
    /// <seealso cref="XmlRpcResponse(int, string)"/>
    public XmlRpcStructureValue Fault
    {
        get
        {
            return responseFault;
        }
    }

    /// <summary>
    /// Gets the response information that was returned for the remote procedure call.
    /// </summary>
    /// <value>
    ///     A <see cref="IXmlRpcValue"/> that represents the response value that was returned for the remote procedure call.
    ///     If the remote procedure call raised an exception, will return <b>null</b> and the <see cref="Fault"/> <i>should</i> be populated.
    /// </value>
    /// <seealso cref="XmlRpcResponse(IXmlRpcValue)"/>
    public IXmlRpcValue Parameter
    {
        get
        {
            return responseParameter;
        }
    }

    /// <summary>
    /// Loads this <see cref="XmlRpcResponse"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="XmlRpcResponse"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcResponse"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        if (source.HasChildren)
        {
            XPathNavigator? parametersNavigator = source.SelectSingleNode("params");
            XPathNavigator? faultNavigator = source.SelectSingleNode("fault");

            if (parametersNavigator != null)
            {
                XPathNavigator? valueNavigator = parametersNavigator.SelectSingleNode("param/value");
                if (valueNavigator != null)
                {
                    if (XmlRpcClient.TryParseValue(valueNavigator, out IXmlRpcValue value))
                    {
                        responseParameter = value;
                        wasLoaded = true;
                    }
                }
            }

            if (faultNavigator != null)
            {
                XPathNavigator? structNavigator = faultNavigator.SelectSingleNode("value");
                if (structNavigator != null)
                {
                    XmlRpcStructureValue structure = new();
                    if (structure.Load(structNavigator))
                    {
                        responseFault = structure;
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="XmlRpcResponse"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("methodResponse");

        if (this.Parameter != null)
        {
            writer.WriteStartElement("params");
            writer.WriteStartElement("param");
            this.Parameter.WriteTo(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        if (this.Fault != null)
        {
            writer.WriteStartElement("fault");
            this.Fault.WriteTo(writer);
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
    public int CompareTo(XmlRpcResponse? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = 0;

        if (this.Fault != null)
        {
            if (other.Fault != null)
            {
                if (result == 0) result = this.Fault.CompareTo(other.Fault);
            }
            else
            {
                if (result == 0) result = 1;
            }
        }
        else if (other.Fault != null)
        {
            if (result == 0) result = -1;
        }

        if (this.Parameter != null)
        {
            if (other.Parameter != null)
            {
                if (result == 0) result = string.Compare(this.Parameter.ToString(), other.Parameter.ToString(), StringComparison.Ordinal);
            }
            else
            {
                if (result == 0) result = 1;
            }
        }
        else if (other.Parameter != null)
        {
            if (result == 0) result = -1;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="XmlRpcResponse"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="XmlRpcResponse"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="XmlRpcResponse"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(XmlRpcResponse? other)
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
        return obj is XmlRpcResponse other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Fault), HashCodeUtility.Component(this.Parameter?.ToString()));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(XmlRpcResponse? first, XmlRpcResponse? second)
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
    public static bool operator !=(XmlRpcResponse? first, XmlRpcResponse? second)
    {
        return !(first == second);
    }
}