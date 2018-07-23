namespace Argotic.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net;

    /// <summary>
    /// Represents the the network element in the Trackback <see cref="TrackbackClientSection">client configuration section</see>. This class cannot be inheritied.
    /// </summary>
    /// <seealso cref="TrackbackClientSection"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Trackback")]
    public sealed class TrackbackClientNetworkElement : ConfigurationElement
    {
        /// <summary>
        /// Private member to hold the client default credentials configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationElementDefaultCredentialsProperty =
            new ConfigurationProperty(
                "defaultCredentials",
                typeof(bool),
                false,
                new BooleanConverter(),
                null,
                ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client domain configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationElementDomainProperty = new ConfigurationProperty(
            "domain",
            typeof(string),
            string.Empty,
            new StringConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client password configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationElementPasswordProperty = new ConfigurationProperty(
            "password",
            typeof(string),
            string.Empty,
            new StringConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client user name configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationElementUserNameProperty = new ConfigurationProperty(
            "userName",
            typeof(string),
            string.Empty,
            new StringConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold the client host configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty ConfigurationSectionHostProperty = new ConfigurationProperty(
            "host",
            typeof(Uri),
            null,
            new UriTypeConverter(),
            null,
            ConfigurationPropertyOptions.None);

        /// <summary>
        /// Private member to hold a collection of configuration element properties for the element.
        /// </summary>
        private static ConfigurationPropertyCollection configurationElementProperties =
            new ConfigurationPropertyCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClientNetworkElement"/> class.
        /// </summary>
        public TrackbackClientNetworkElement()
        {
            configurationElementProperties.Add(ConfigurationSectionHostProperty);
            configurationElementProperties.Add(ConfigurationElementDefaultCredentialsProperty);
            configurationElementProperties.Add(ConfigurationElementUserNameProperty);
            configurationElementProperties.Add(ConfigurationElementPasswordProperty);
            configurationElementProperties.Add(ConfigurationElementDomainProperty);
        }

        /// <summary>
        /// Gets returns a <see cref="NetworkCredential"/> for the configured user name, password, and domain.
        /// </summary>
        /// <returns>
        ///     A <see cref="NetworkCredential"/> object initialized using the curent <see cref="UserName"/>, <see cref="Password"/>, and <see cref="Domain"/>.
        /// </returns>
        /// <remarks>
        ///     If <see cref="UserName"/> is a null or empty string, returns a <b>null</b> reference.
        /// </remarks>
        public NetworkCredential Credential
        {
            get
            {
                NetworkCredential credential = null;

                if (!string.IsNullOrEmpty(this.UserName))
                {
                    if (!string.IsNullOrEmpty(this.Domain))
                    {
                        credential = new NetworkCredential(this.UserName, this.Password, this.Domain);
                    }
                    else
                    {
                        credential = new NetworkCredential(this.UserName, this.Password);
                    }
                }

                return credential;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a <see cref="bool"/> value that controls whether the <see cref="System.Net.CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent with requests.
        /// </summary>
        /// <value><b>true</b> indicates that default user credentials will be used to access the Trackback server; otherwise, <b>false</b>.</value>
        [ConfigurationProperty("defaultCredentials", DefaultValue = false, Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(bool))]
        public bool DefaultCredentials
        {
            get
            {
                return (bool)this[ConfigurationElementDefaultCredentialsProperty];
            }

            set
            {
                this[ConfigurationElementDefaultCredentialsProperty] = value;
            }
        }

        /// <summary>
        /// Gets or sets the domain or computer name that verifies the network credentials.
        /// </summary>
        /// <value>A string that represents the domain or computer name that verifies the network credentials.</value>
        /// <seealso cref="NetworkCredential.Domain"/>
        [ConfigurationProperty("domain", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string Domain
        {
            get
            {
                return (string)this[ConfigurationElementDomainProperty];
            }

            set
            {
                this[ConfigurationElementDomainProperty] = value;
            }
        }

        /// <summary>
        /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</value>
        [ConfigurationProperty("host", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(Uri))]
        public Uri Host
        {
            get
            {
                return (Uri)this[ConfigurationSectionHostProperty];
            }

            set
            {
                this[ConfigurationSectionHostProperty] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user password to use to connect to a Trackback server.
        /// </summary>
        /// <value>A string that represents the password to use to connect to a Trackback server.</value>
        /// <seealso cref="NetworkCredential.Password"/>
        [ConfigurationProperty("password", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string Password
        {
            get
            {
                return (string)this[ConfigurationElementPasswordProperty];
            }

            set
            {
                this[ConfigurationElementPasswordProperty] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name to connect to a Trackback server.
        /// </summary>
        /// <value>A string that represents the user name to connect to a Trackback server.</value>
        /// <seealso cref="NetworkCredential.UserName"/>
        [ConfigurationProperty("userName", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string UserName
        {
            get
            {
                return (string)this[ConfigurationElementUserNameProperty];
            }

            set
            {
                this[ConfigurationElementUserNameProperty] = value;
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
}