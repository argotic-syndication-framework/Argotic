/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/19/2008	brian.kuhn	Created TrackbackClientSection Class
****************************************************************************/
using System;
using System.ComponentModel;
using System.Configuration;

namespace Argotic.Configuration
{
    /// <summary>
    /// Represents the configuration section used to declarativly configure the <see cref="Argotic.Net.TrackbackClient"/> class. This class cannot be inheritied.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public sealed class TrackbackClientSection : ConfigurationSection
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the client timeout configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionTimeoutProperty   = new ConfigurationProperty("timeout", typeof(System.TimeSpan), TimeSpan.FromSeconds(15), new TimeSpanConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client user agent configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionUserAgentProperty = new ConfigurationProperty("agent", typeof(System.String), String.Empty, new StringConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the client network configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionNetworkProperty   = new ConfigurationProperty("network", typeof(TrackbackClientNetworkElement), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold a collection of configuration properties for the section.
        /// </summary>
        private static ConfigurationPropertyCollection configurationSectionProperties       = new ConfigurationPropertyCollection();
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region TrackbackClientSection()
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClientSection"/> class.
        /// </summary>
        public TrackbackClientSection()
        {
            //------------------------------------------------------------
            //	Initialize configuration section properties
            //------------------------------------------------------------
            configurationSectionProperties.Add(configurationSectionTimeoutProperty);
            configurationSectionProperties.Add(configurationSectionUserAgentProperty);
            configurationSectionProperties.Add(configurationSectionNetworkProperty);
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Network
        /// <summary>
        /// Gets the network connection information.
        /// </summary>
        /// <value>A <see cref="TrackbackClientNetworkElement"/> object that represents the configured network connection information.</value>
        [ConfigurationProperty("network", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public TrackbackClientNetworkElement Network
        {
            get
            {
                return (TrackbackClientNetworkElement)base[configurationSectionNetworkProperty];
            }
        }
        #endregion

        #region Timeout
        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
        [ConfigurationProperty("timeout", DefaultValue = "0:0:15.0", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(System.TimeSpan))]
        public TimeSpan Timeout
        {
            get
            {
                return (TimeSpan)base[configurationSectionTimeoutProperty];
            }
            set
            {
                base[configurationSectionTimeoutProperty] = value;
            }
        }
        #endregion

        #region UserAgent
        /// <summary>
        /// Gets or sets information such as the client application name, version, host operating system, and language. 
        /// </summary>
        /// <value>A string that represents information such as the client application name, version, host operating system, and language.</value>
        [ConfigurationProperty("agent", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string UserAgent
        {
            get
            {
                return (string)base[configurationSectionUserAgentProperty];
            }
            set
            {
                base[configurationSectionUserAgentProperty] = value;
            }
        }
        #endregion

        //============================================================
        //	PROTECTED PROPERTIES
        //============================================================
        #region Properties
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
        #endregion
    }
}
