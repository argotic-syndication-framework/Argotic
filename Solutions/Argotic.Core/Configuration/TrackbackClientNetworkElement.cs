/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/19/2008	brian.kuhn	Created TrackbackClientNetworkElement Class
****************************************************************************/
using System;
using System.ComponentModel;
using System.Configuration;
using System.Net;

namespace Argotic.Configuration
{
    /// <summary>
    /// Represents the the network element in the Trackback <see cref="TrackbackClientSection">client configuration section</see>. This class cannot be inheritied.
    /// </summary>
    /// <seealso cref="TrackbackClientSection"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public sealed class TrackbackClientNetworkElement : ConfigurationElement
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the client host configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionHostProperty                  = new ConfigurationProperty("host", typeof(System.Uri), null, new UriTypeConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client default credentials configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty configurationElementDefaultCredentialsProperty    = new ConfigurationProperty("defaultCredentials", typeof(System.Boolean), false, new BooleanConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client user name configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty configurationElementUserNameProperty              = new ConfigurationProperty("userName", typeof(System.String), String.Empty, new StringConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client password configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty configurationElementPasswordProperty              = new ConfigurationProperty("password", typeof(System.String), String.Empty, new StringConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client domain configuration property for the element.
        /// </summary>
        private static readonly ConfigurationProperty configurationElementDomainProperty                = new ConfigurationProperty("domain", typeof(System.String), String.Empty, new StringConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold a collection of configuration element properties for the element.
        /// </summary>
        private static ConfigurationPropertyCollection configurationElementProperties                   = new ConfigurationPropertyCollection();
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region TrackbackClientNetworkElement()
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClientNetworkElement"/> class.
        /// </summary>
        public TrackbackClientNetworkElement()
        {
            //------------------------------------------------------------
            //	Initialize configuration element properties
            //------------------------------------------------------------
            configurationElementProperties.Add(configurationSectionHostProperty);
            configurationElementProperties.Add(configurationElementDefaultCredentialsProperty);
            configurationElementProperties.Add(configurationElementUserNameProperty);
            configurationElementProperties.Add(configurationElementPasswordProperty);
            configurationElementProperties.Add(configurationElementDomainProperty);
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region DefaultCredentials
        /// <summary>
        /// Gets or sets a <see cref="Boolean"/> value that controls whether the <see cref="System.Net.CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent with requests.
        /// </summary>
        /// <value><b>true</b> indicates that default user credentials will be used to access the Trackback server; otherwise, <b>false</b>.</value>
        [ConfigurationProperty("defaultCredentials", DefaultValue = false, Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(System.Boolean))]
        public bool DefaultCredentials
        {
            get
            {
                return (bool)base[configurationElementDefaultCredentialsProperty];
            }
            set
            {
                base[configurationElementDefaultCredentialsProperty] = value;
            }
        }
        #endregion

        #region Domain
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
                return (string)base[configurationElementDomainProperty];
            }
            set
            {
                base[configurationElementDomainProperty] = value;
            }
        }
        #endregion

        #region Host
        /// <summary>
        /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</value>
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
        #endregion

        #region Password
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
                return (string)base[configurationElementPasswordProperty];
            }
            set
            {
                base[configurationElementPasswordProperty] = value;
            }
        }
        #endregion

        #region UserName
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
                return (string)base[configurationElementUserNameProperty];
            }
            set
            {
                base[configurationElementUserNameProperty] = value;
            }
        }
        #endregion

        //============================================================
        //	UTILITY PROPERTIES
        //============================================================
        #region Credential
        /// <summary>
        /// Returns a <see cref="NetworkCredential"/> for the configured user name, password, and domain.
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
                NetworkCredential credential    = null;

                if (!String.IsNullOrEmpty(this.UserName))
                {
                    if (!String.IsNullOrEmpty(this.Domain))
                    {
                        credential  = new NetworkCredential(this.UserName, this.Password, this.Domain);
                    }
                    else
                    {
                        credential  = new NetworkCredential(this.UserName, this.Password);
                    }
                }

                return credential;
            }
        }
        #endregion

        //============================================================
        //	PROTECTED PROPERTIES
        //============================================================
        #region Properties
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
        #endregion
    }
}
