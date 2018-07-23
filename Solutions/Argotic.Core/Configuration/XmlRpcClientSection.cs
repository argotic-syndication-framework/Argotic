namespace Argotic.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    /// <summary>
    /// Represents the configuration section used to declarativly configure the <see cref="Argotic.Net.XmlRpcClient"/> class. This class cannot be inheritied.
    /// </summary>
    public sealed class XmlRpcClientSection : ConfigurationSection
    {
        /// <summary>
        /// Private member to hold the client network configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationSectionNetworkProperty = new ConfigurationProperty(
            "network",
            typeof(XmlRpcClientNetworkElement),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client timeout configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationSectionTimeoutProperty = new ConfigurationProperty(
            "timeout",
            typeof(TimeSpan),
            TimeSpan.FromSeconds(15),
            new TimeSpanConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client user agent configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationSectionUserAgentProperty = new ConfigurationProperty(
            "agent",
            typeof(string),
            string.Empty,
            new StringConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold a collection of configuration properties for the section.
        /// </summary>
        private static ConfigurationPropertyCollection configurationSectionProperties =
            new ConfigurationPropertyCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcClientSection"/> class.
        /// </summary>
        public XmlRpcClientSection()
        {
            configurationSectionProperties.Add(ConfigurationSectionTimeoutProperty);
            configurationSectionProperties.Add(ConfigurationSectionUserAgentProperty);
            configurationSectionProperties.Add(ConfigurationSectionNetworkProperty);
        }

        /// <summary>
        /// Gets the network connection information.
        /// </summary>
        /// <value>A <see cref="XmlRpcClientNetworkElement"/> object that represents the configured network connection information.</value>
        [ConfigurationProperty("network", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public XmlRpcClientNetworkElement Network
        {
            get
            {
                return (XmlRpcClientNetworkElement)this[ConfigurationSectionNetworkProperty];
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
        [ConfigurationProperty("timeout", DefaultValue = "0:0:15.0", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(TimeSpan))]
        public TimeSpan Timeout
        {
            get
            {
                return (TimeSpan)this[ConfigurationSectionTimeoutProperty];
            }

            set
            {
                this[ConfigurationSectionTimeoutProperty] = value;
            }
        }

        /// <summary>
        /// Gets or sets information such as the client application name, version, host operating system, and language.
        /// </summary>
        /// <value>A string that represents information such as the client application name, version, host operating system, and language.</value>
        [ConfigurationProperty("agent", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string UserAgent
        {
            get
            {
                return (string)this[ConfigurationSectionUserAgentProperty];
            }

            set
            {
                this[ConfigurationSectionUserAgentProperty] = value;
            }
        }

        /// <summary>
        /// Gets the configuration properties for this section.
        /// </summary>
        /// <value>A <see cref="ConfigurationPropertyCollection"/> object that represents the configuration properties for this section.</value>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return configurationSectionProperties;
            }
        }
    }
}