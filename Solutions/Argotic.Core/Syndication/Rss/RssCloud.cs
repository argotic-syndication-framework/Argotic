using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents the meta-data necessary for monitoring updates to an <see cref="RssFeed"/> using a web service that implements the RssCloud application programming interface.
/// </summary>
/// <seealso cref="RssChannel.Cloud"/>
/// <remarks>
///     For more information about the RssCloud application programming interface, see <a href="http://www.rssboard.org/rsscloud-interface">http://www.rssboard.org/rsscloud-interface</a>.
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssCloud class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssCloudExample.cs" 
///             region="RssCloud" 
///         />
///     </code>
/// </example>
[Serializable]
public class RssCloud : IComparable<RssCloud>, IEquatable<RssCloud>, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the host name or IP address of the web service that monitors updates to the feed.
    /// </summary>
    private string cloudDomain = string.Empty;
    /// <summary>
    /// Private member to hold the web service's path.
    /// </summary>
    private string cloudPath = string.Empty;
    /// <summary>
    /// Private member to hold the web service's TCP port.
    /// </summary>
    private int cloudPort = 80;
    /// <summary>
    /// Private member to hold the protocol utilized by the web service.
    /// </summary>
    private RssCloudProtocol cloudProtocol = RssCloudProtocol.XmlRpc;
    /// <summary>
    /// Private member to hold message format the web service employs.
    /// </summary>
    private string cloudRegisterProcedure = string.Empty;
    /// <summary>
    /// Initializes a new instance of the <see cref="RssCloud"/> class.
    /// </summary>
    public RssCloud()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCloud"/> class using the supplied domain, path, port, protocol and procedure name.
    /// </summary>
    /// <param name="domain">The host name or IP address of the web service that monitors updates to a feed.</param>
    /// <param name="path">The path of the web service that monitors updates to a feed.</param>
    /// <param name="port">The TCP port of the web service that monitors updates to a feed.</param>
    /// <param name="protocol"> An <see cref="RssCloudProtocol"/> enumeration value that represents the message format utilized by the web service that monitors updates to a feed.</param>
    /// <param name="registerProcedure">The name of the remote procedure to call when requesting notification of feed updates.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="domain"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="domain"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="path"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="path"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="port"/> is less than <i>zero</i>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="protocol"/> is equal to <see cref="RssCloudProtocol.None"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="registerProcedure"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="registerProcedure"/> is an empty string.</exception>
    public RssCloud(string domain, string path, int port, RssCloudProtocol protocol, string registerProcedure)
    {
        this.Domain = domain;
        this.Path = path;
        this.Port = port;
        this.Protocol = protocol;
        this.RegisterProcedure = registerProcedure;
    }
    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions
    {
        get
        {
            return this.Extensions.Count > 0;
        }
    }
    /// <summary>
    /// Gets or sets the host name or IP address of the web service that monitors updates to a feed.
    /// </summary>
    /// <value>The host name or IP address of the web service that monitors updates to a feed.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Domain
    {
        get
        {
            return cloudDomain;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            cloudDomain = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the path of the web service that monitors updates to a feed.
    /// </summary>
    /// <value>The path of the web service that monitors updates to a feed.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Path
    {
        get
        {
            return cloudPath;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            cloudPath = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the TCP port of the web service that monitors updates to a feed.
    /// </summary>
    /// <value>The TCP port of the web service that monitors updates to a feed. The default value is <b>80</b>.</value>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <i>zero</i>.</exception>
    public int Port
    {
        get
        {
            return cloudPort;
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            cloudPort = value;
        }
    }

    /// <summary>
    /// Gets or sets the message format utilized by the web service that monitors updates to a feed.
    /// </summary>
    /// <value>
    ///     An <see cref="RssCloudProtocol"/> enumeration value that represents the message format utilized by the web service that monitors updates to a feed. 
    ///     The default value is <see cref="RssCloudProtocol.XmlRpc"/>.
    /// </value>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is equivalent to <see cref="RssCloudProtocol.None"/>.</exception>
    public RssCloudProtocol Protocol
    {
        get
        {
            return cloudProtocol;
        }

        set
        {
            if (value == RssCloudProtocol.None)
            {
                throw new ArgumentException(string.Format(null, "The specified cloud protocol of {0} is invalid.", value), nameof(value));
            }
            cloudProtocol = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the remote procedure to call when requesting notification of feed updates.
    /// </summary>
    /// <value>The name of the remote procedure to call when requesting notification of feed updates.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string RegisterProcedure
    {
        get
        {
            return cloudRegisterProcedure;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            cloudRegisterProcedure = value.Trim();
        }
    }
    /// <summary>
    /// Returns the cloud protocol identifier for the supplied <see cref="RssCloudProtocol"/>.
    /// </summary>
    /// <param name="protocol">The <see cref="RssCloudProtocol"/> to get the cloud protocol identifier for.</param>
    /// <returns>The cloud protocol identifier for the supplied <paramref name="protocol"/>, Otherwise, returns an empty string.</returns>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CloudProtocolAsString method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\Rss\RssCloudExample.cs" 
    ///             region="CloudProtocolAsString(RssCloudProtocol protocol)" 
    ///         />
    ///     </code>
    /// </example>
    public static string CloudProtocolAsString(RssCloudProtocol protocol)
    {
        string name = string.Empty;
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(RssCloudProtocol).GetFields())
        {
            if (fieldInfo.FieldType == typeof(RssCloudProtocol))
            {
                RssCloudProtocol cloudProtocol = (RssCloudProtocol)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                if (cloudProtocol == protocol)
                {
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes is { Length: > 0 })
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        name = enumerationMetadata.AlternateValue;
                        break;
                    }
                }
            }
        }

        return name;
    }

    /// <summary>
    /// Returns the <see cref="RssCloudProtocol"/> enumeration value that corresponds to the specified protocol name.
    /// </summary>
    /// <param name="name">The name of the cloud protocol.</param>
    /// <returns>A <see cref="RssCloudProtocol"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>RssCloudProtocol.None</b>.</returns>
    /// <remarks>This method disregards case of specified protocol name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CloudProtocolByName method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\Rss\RssCloudExample.cs" 
    ///             region="CloudProtocolByName(string name)" 
    ///         />
    ///     </code>
    /// </example>
    public static RssCloudProtocol CloudProtocolByName(string name)
    {
        RssCloudProtocol cloudProtocol = RssCloudProtocol.None;
        ArgumentException.ThrowIfNullOrEmpty(name);
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(RssCloudProtocol).GetFields())
        {
            if (fieldInfo.FieldType == typeof(RssCloudProtocol))
            {
                RssCloudProtocol protocol = (RssCloudProtocol)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                if (customAttributes is { Length: > 0 })
                {
                    EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                    if (string.Equals(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        cloudProtocol = protocol;
                        break;
                    }
                }
            }
        }

        return cloudProtocol;
    }
    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (ISyndicationExtension extension in this.Extensions)
        {
            if (match(extension))
            {
                return extension;
            }
        }

        return null;
    }
    /// <summary>
    /// Loads this <see cref="RssCloud"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RssCloud"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCloud"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string domain = source.GetAttribute("domain", string.Empty);
            string path = source.GetAttribute("path", string.Empty);
            string port = source.GetAttribute("port", string.Empty);
            string protocol = source.GetAttribute("protocol", string.Empty);
            string registerProcedure = source.GetAttribute("registerProcedure", string.Empty);

            if (!string.IsNullOrEmpty(domain))
            {
                this.Domain = domain;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(path))
            {
                this.Path = path;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(port))
            {
                if (int.TryParse(port, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int tcpPort))
                {
                    if (tcpPort > 0)
                    {
                        this.Port = tcpPort;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(protocol))
            {
                RssCloudProtocol serviceProtocol = RssCloud.CloudProtocolByName(protocol);
                if (serviceProtocol != RssCloudProtocol.None)
                {
                    this.Protocol = serviceProtocol;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(registerProcedure))
            {
                this.RegisterProcedure = registerProcedure;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssCloud"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssCloud"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCloud"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="RssCloud"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("cloud");

        writer.WriteAttributeString("domain", this.Domain);
        writer.WriteAttributeString("path", this.Path);
        writer.WriteAttributeString("port", this.Port.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        writer.WriteAttributeString("protocol", RssCloud.CloudProtocolAsString(this.Protocol));
        writer.WriteAttributeString("registerProcedure", this.RegisterProcedure);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="RssCloud"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="RssCloud"/>.</returns>
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
    /// <param name="other">The <see cref="RssCloud"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssCloud? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Path, other.Path, StringComparison.OrdinalIgnoreCase);
        result |= this.Port.CompareTo(other.Port);
        result |= this.Protocol.CompareTo(other.Protocol);
        result |= string.Compare(this.RegisterProcedure, other.RegisterProcedure, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssCloud"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssCloud"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="RssCloud"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(RssCloud? other)
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
        return obj is RssCloud other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Domain ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Path ?? string.Empty),
            this.Port,
            this.Protocol,
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.RegisterProcedure ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RssCloud first, RssCloud second)
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
    public static bool operator !=(RssCloud first, RssCloud second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(RssCloud first, RssCloud second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(RssCloud first, RssCloud second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(RssCloud first, RssCloud second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(RssCloud first, RssCloud second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}