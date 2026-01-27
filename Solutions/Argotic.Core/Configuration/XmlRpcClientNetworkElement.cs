using System.ComponentModel;
using System.Configuration;

namespace Argotic.Configuration;

/// <summary>
/// Represents the network element in the XML-RPC <see cref="XmlRpcClientSection">client configuration section</see>. This class cannot be inherited.
/// </summary>
/// <seealso cref="XmlRpcClientSection"/>
public sealed class XmlRpcClientNetworkElement : ConfigurationElement
{
    /// <summary>
    /// Private member to hold the client host configuration property for the element.
    /// </summary>
    private static readonly ConfigurationProperty configurationSectionHostProperty = new("host", typeof(System.Uri), null, new UriTypeConverter(), null, ConfigurationPropertyOptions.None);
    /// <summary>
    /// Private member to hold a collection of configuration element properties for the element.
    /// </summary>
    private static readonly ConfigurationPropertyCollection configurationElementProperties = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClientNetworkElement"/> class.
    /// </summary>
    public XmlRpcClientNetworkElement()
    {
        configurationElementProperties.Add(configurationSectionHostProperty);
    }

    /// <summary>
    /// Gets or sets the location of the host computer that client remote procedure calls will be sent to.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</value>
    [ConfigurationProperty("host", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
    [TypeConverter(typeof(System.Uri))]
    public Uri Host
    {
        get
        {
            return (Uri)base[configurationSectionHostProperty];
        }
        set
        {
            base[configurationSectionHostProperty] = value;
        }
    }

    /// <summary>
    /// Gets the configuration properties for this element.
    /// </summary>
    /// <value>A <see cref="ConfigurationPropertyCollection"/> object that represents the configuration properties for this element.</value>
    protected override ConfigurationPropertyCollection Properties
    {
        get
        {
            return configurationElementProperties;
        }
    }
}
