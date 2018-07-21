/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/11/2007	brian.kuhn	Created RssCloud Class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication
{
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
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssCloudExample.cs" 
    ///             region="RssCloud" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public class RssCloud : IComparable, IExtensibleSyndicationObject
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
        /// <summary>
        /// Private member to hold the host name or IP address of the web service that monitors updates to the feed.
        /// </summary>
        private string cloudDomain              = String.Empty;
        /// <summary>
        /// Private member to hold the web service's path.
        /// </summary>
        private string cloudPath                = String.Empty;
        /// <summary>
        /// Private member to hold the web service's TCP port.
        /// </summary>
        private int cloudPort                   = 80;
        /// <summary>
        /// Private member to hold the protocol utilized by the web service.
        /// </summary>
        private RssCloudProtocol cloudProtocol  = RssCloudProtocol.XmlRpc;
        /// <summary>
        /// Private member to hold message format the web service employs.
        /// </summary>
        private string cloudRegisterProcedure   = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region RssCloud()
        /// <summary>
        /// Initializes a new instance of the <see cref="RssCloud"/> class.
        /// </summary>
        public RssCloud()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region RssCloud(string domain, string path, int port, RssCloudProtocol protocol, string registerProcedure)
        /// <summary>
        /// Initializes a new instance of the <see cref="RssCloud"/> class using the supplied domain, path, port, protocol and procedure name.
        /// </summary>
        /// <param name="domain">The host name or IP address of the web service that monitors updates to a feed.</param>
        /// <param name="path">The path of the web service that monitors updates to a feed.</param>
        /// <param name="port">The TCP port of the web service that monitors updates to a feed.</param>
        /// <param name="protocol"> An <see cref="RssCloudProtocol"/> enumeration value that represents the message format utilized by the web service that monitors updates to a feed.</param>
        /// <param name="registerProcedure">The name of the remote procedure to call when requesting notification of feed updates.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="domain"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="domain"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="port"/> is less than <i>zero</i>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="protocol"/> is equal to <see cref="RssCloudProtocol.None"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="registerProcedure"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="registerProcedure"/> is an empty string.</exception>
        public RssCloud(string domain, string path, int port, RssCloudProtocol protocol, string registerProcedure)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Domain             = domain;
            this.Path               = path;
            this.Port               = port;
            this.Protocol           = protocol;
            this.RegisterProcedure  = registerProcedure;
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY PROPERTIES
        //============================================================
        #region Extensions
        /// <summary>
        /// Gets or sets the syndication extensions applied to this syndication entity.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (objectSyndicationExtensions == null)
                {
                    objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }
                return objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                objectSyndicationExtensions = value;
            }
        }
        #endregion

        #region HasExtensions
        /// <summary>
        /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Domain
        /// <summary>
        /// Gets or sets the host name or IP address of the web service that monitors updates to a feed.
        /// </summary>
        /// <value>The host name or IP address of the web service that monitors updates to a feed.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Domain
        {
            get
            {
                return cloudDomain;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                cloudDomain = value.Trim();
            }
        }
        #endregion

        #region Path
        /// <summary>
        /// Gets or sets the path of the web service that monitors updates to a feed.
        /// </summary>
        /// <value>The path of the web service that monitors updates to a feed.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Path
        {
            get
            {
                return cloudPath;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                cloudPath = value.Trim();
            }
        }
        #endregion

        #region Port
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
                Guard.ArgumentNotLessThan(value, "value", 0);
                cloudPort = value;
            }
        }
        #endregion

        #region Protocol
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
                    throw new ArgumentException(String.Format(null, "The specified cloud protocol of {0} is invalid.", value), "value");
                }
                cloudProtocol = value;
            }
        }
        #endregion

        #region RegisterProcedure
        /// <summary>
        /// Gets or sets the name of the remote procedure to call when requesting notification of feed updates.
        /// </summary>
        /// <value>The name of the remote procedure to call when requesting notification of feed updates.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string RegisterProcedure
        {
            get
            {
                return cloudRegisterProcedure;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                cloudRegisterProcedure = value.Trim();
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CloudProtocolAsString(RssCloudProtocol protocol)
        /// <summary>
        /// Returns the cloud protocol identifier for the supplied <see cref="RssCloudProtocol"/>.
        /// </summary>
        /// <param name="protocol">The <see cref="RssCloudProtocol"/> to get the cloud protocol identifier for.</param>
        /// <returns>The cloud protocol identifier for the supplied <paramref name="protocol"/>, otherwise returns an empty string.</returns>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the CloudProtocolAsString method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssCloudExample.cs" 
        ///             region="CloudProtocolAsString(RssCloudProtocol protocol)" 
        ///         />
        ///     </code>
        /// </example>
        public static string CloudProtocolAsString(RssCloudProtocol protocol)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(RssCloudProtocol).GetFields())
            {
                if (fieldInfo.FieldType == typeof(RssCloudProtocol))
                {
                    RssCloudProtocol cloudProtocol  = (RssCloudProtocol)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (cloudProtocol == protocol)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region CloudProtocolByName(string name)
        /// <summary>
        /// Returns the <see cref="RssCloudProtocol"/> enumeration value that corresponds to the specified protocol name.
        /// </summary>
        /// <param name="name">The name of the cloud protocol.</param>
        /// <returns>A <see cref="RssCloudProtocol"/> enumeration value that corresponds to the specified string, otherwise returns <b>RssCloudProtocol.None</b>.</returns>
        /// <remarks>This method disregards case of specified protocol name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the CloudProtocolByName method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssCloudExample.cs" 
        ///             region="CloudProtocolByName(string name)" 
        ///         />
        ///     </code>
        /// </example>
        public static RssCloudProtocol CloudProtocolByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            RssCloudProtocol cloudProtocol  = RssCloudProtocol.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(RssCloudProtocol).GetFields())
            {
                if (fieldInfo.FieldType == typeof(RssCloudProtocol))
                {
                    RssCloudProtocol protocol   = (RssCloudProtocol)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            cloudProtocol   = protocol;
                            break;
                        }
                    }
                }
            }

            return cloudProtocol;
        }
        #endregion

        //============================================================
        //	EXTENSIBILITY METHODS
        //============================================================
        #region AddExtension(ISyndicationExtension extension)
        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded   = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Add syndication extension to collection
            //------------------------------------------------------------
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded    = true;

            return wasAdded;
        }
        #endregion

        #region FindExtension(Predicate<ISyndicationExtension> match)
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
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(match, "match");

            //------------------------------------------------------------
            //	Perform predicate based search
            //------------------------------------------------------------
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }
        #endregion

        #region RemoveExtension(ISyndicationExtension extension)
        /// <summary>
        /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveExtension(ISyndicationExtension extension)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(extension, "extension");

            //------------------------------------------------------------
            //	Remove syndication extension from collection
            //------------------------------------------------------------
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved  = true;
            }

            return wasRemoved;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="RssCloud"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RssCloud"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCloud"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if (source.HasAttributes)
            {
                string domain               = source.GetAttribute("domain", String.Empty);
                string path                 = source.GetAttribute("path", String.Empty);
                string port                 = source.GetAttribute("port", String.Empty);
                string protocol             = source.GetAttribute("protocol", String.Empty);
                string registerProcedure    = source.GetAttribute("registerProcedure", String.Empty);

                if (!String.IsNullOrEmpty(domain))
                {
                    this.Domain             = domain;
                    wasLoaded               = true;
                }

                if (!String.IsNullOrEmpty(path))
                {
                    this.Path               = path;
                    wasLoaded               = true;
                }

                if (!String.IsNullOrEmpty(port))
                {
                    int tcpPort;
                    if (Int32.TryParse(port, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out tcpPort))
                    {
                        if (tcpPort > 0)
                        {
                            this.Port       = tcpPort;
                            wasLoaded       = true;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(protocol))
                {
                    RssCloudProtocol serviceProtocol    = RssCloud.CloudProtocolByName(protocol);
                    if (serviceProtocol != RssCloudProtocol.None)
                    {
                        this.Protocol       = serviceProtocol;
                        wasLoaded           = true;
                    }
                }

                if (!String.IsNullOrEmpty(registerProcedure))
                {
                    this.RegisterProcedure  = registerProcedure;
                    wasLoaded               = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads this <see cref="RssCloud"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssCloud"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCloud"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            wasLoaded   = this.Load(source);

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="RssCloud"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("cloud");

            writer.WriteAttributeString("domain", this.Domain);
            writer.WriteAttributeString("path", this.Path);
            writer.WriteAttributeString("port", this.Port.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            writer.WriteAttributeString("protocol", RssCloud.CloudProtocolAsString(this.Protocol));
            writer.WriteAttributeString("registerProcedure", this.RegisterProcedure);

            //------------------------------------------------------------
            //	Write the syndication extensions of the current instance
            //------------------------------------------------------------
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="RssCloud"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="RssCloud"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Fragment;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.WriteTo(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            RssCloud value  = obj as RssCloud;

            if (value != null)
            {
                int result  = String.Compare(this.Domain, value.Domain, StringComparison.OrdinalIgnoreCase);
                result      = result | String.Compare(this.Path, value.Path, StringComparison.OrdinalIgnoreCase);
                result      = result | this.Port.CompareTo(value.Port);
                result      = result | this.Protocol.CompareTo(value.Protocol);
                result      = result | String.Compare(this.RegisterProcedure, value.RegisterProcedure, StringComparison.OrdinalIgnoreCase);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is RssCloud))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RssCloud first, RssCloud second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }
        #endregion

        #region != operator
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
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RssCloud first, RssCloud second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(RssCloud first, RssCloud second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
        #endregion
    }
}
