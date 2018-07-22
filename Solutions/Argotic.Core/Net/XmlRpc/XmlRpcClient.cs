using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Configuration;

namespace Argotic.Net
{
    /// <summary>
    /// Allows applications to send remote procedure calls by using the Extensible Markup Language Remote Procedure Call (XML-RPC) protocol.
    /// </summary>
    /// <remarks>
    ///     <para>This implementation of XML-RPC is based on the XML-RPC 1.0 specification which can be found at <a href="http://www.xmlrpc.com/spec">http://www.xmlrpc.com/spec</a>.</para>
    ///     <para><b>XML-RPC</b> is a Remote Procedure Calling protocol that works over the Internet.</para>
    ///     <para>
    ///         An XML-RPC <i>message</i> is an HTTP-POST request. The body of the request is in XML.
    ///         A procedure executes on the server and the value it returns is also formatted in XML.
    ///     </para>
    ///     <para>Procedure parameters can be scalars, numbers, strings, dates and other simple types; and can also be complex record and list structures.</para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcClient class.">
    ///         <code
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\XmlRpcClientExample.cs"
    ///             region="XmlRpcClient"
    ///         />
    ///     </code>
    /// </example>
    public class XmlRpcClient
    {
        /// <summary>
        /// Private member to hold the location of the host computer that client XML-RPC calls will be sent to.
        /// </summary>
        private Uri clientHost;
        /// <summary>
        /// Private member to hold information such as the application name, version, host operating system, and language.
        /// </summary>
        private string clientUserAgent  = String.Format(null, "Argotic-Syndication-Framework/{0}", System.Reflection.Assembly.GetAssembly(typeof(XmlRpcClient)).GetName().Version.ToString(4));
        /// <summary>
        /// Private member to hold the web request options.
        /// </summary>
        private WebRequestOptions clientOptions = new WebRequestOptions();
        /// <summary>
        /// Private member to hold a value that specifies the amount of time after which an asynchronous send operation times out.
        /// </summary>
        private TimeSpan clientTimeout  = TimeSpan.FromSeconds(15);
        /// <summary>
        /// Private member to hold a value that indictaes if the client sends default credentials when making an XML-RPC call.
        /// </summary>
        private bool clientUsesDefaultCredentials;
        /// <summary>
        /// Private member to hold a value indicating if the client is in the process of sending a remote procedure call.
        /// </summary>
        private bool clientIsSending;
        /// <summary>
        /// Private member to hold a value indicating if the client asynchronous send operation was cancelled.
        /// </summary>
        private bool clientAsyncSendCancelled;
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous send operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcClient"/> class.
        /// </summary>
        public XmlRpcClient()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcClient"/> class that sends remote procedure calls using the specified XML-RPC server.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcClient(Uri host)
        {
            this.Initialize();

            this.Host   = host;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcClient"/> class that sends remote procedure calls using the specified XML-RPC server and user agent.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
        /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is an empty string.</exception>
        public XmlRpcClient(Uri host, string userAgent) : this(host)
        {
            this.UserAgent  = userAgent;
        }

        /// <summary>
        /// Occurs when an asynchronous remote procedure call send operation completes.
        /// </summary>
        /// <seealso cref="SendAsync(XmlRpcMessage, Object)"/>
        public event EventHandler<XmlRpcMessageSentEventArgs> SendCompleted;

        /// <summary>
        /// Raises the <see cref="SendCompleted"/> event.
        /// </summary>
        /// <param name="e">A <see cref="XmlRpcMessageSentEventArgs"/> that contains the event data.</param>
        /// <remarks>
        ///     <para>
        ///         Classes that inherit from the <see cref="XmlRpcClient"/> class can override the <see cref="OnMessageSent(XmlRpcMessageSentEventArgs)"/> method
        ///         to perform additional tasks when the <see cref="SendCompleted"/> event occurs.
        ///     </para>
        ///     <para>
        ///         <see cref="OnMessageSent(XmlRpcMessageSentEventArgs)"/> also allows derived classes to handle <see cref="SendCompleted"/> without attaching a delegate.
        ///         This is the preferred technique for handling <see cref="SendCompleted"/> in a derived class.
        ///     </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnMessageSent(XmlRpcMessageSentEventArgs e)
        {
            EventHandler<XmlRpcMessageSentEventArgs> handler    = null;

            handler = this.SendCompleted;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Gets or sets the authentication credentials utilized by this client when making remote procedure calls.
        /// </summary>
        /// <value>
        ///     A <see cref="ICredentials"/> object that represents the authentication credentials provided by this client when making remote procedure calls.
        ///     The default is a null reference (Nothing in Visual Basic), which indicates no authentication information will be supplied to identify the maker of the request.
        /// </value>
        public ICredentials Credentials
        {
            get
            {
                return clientOptions.Credentials;
            }

            set
            {
                clientOptions.Credentials = value;
            }
        }

        /// <summary>
        /// Gets or sets the location of the host computer that client remote procedure calls will be sent to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</value>
        /// <remarks>
        ///     If <see cref="Host"/> is a null reference (Nothing in Visual Basic), <see cref="Host"/> is initialized using the settings in the application or machine configuration files.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Host
        {
            get
            {
                return clientHost;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                clientHost = value;
            }
        }

        /// <summary>
        /// Gets or sets the web proxy utilized by this client to proxy remote procedure calls.
        /// </summary>
        /// <value>
        ///     A <see cref="IWebProxy"/> object that represents the web proxy utilized by this client to proxy remote procedure calls.
        ///     The default is a null reference (Nothing in Visual Basic), which indicates no proxy will be used to proxy the request.
        /// </value>
        public IWebProxy Proxy
        {
            get
            {
                return clientOptions.Proxy;
            }

            set
            {
                clientOptions.Proxy = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
        /// <exception cref="ArgumentOutOfRangeException">The time out period is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The time out period is greater than a year.</exception>
        public TimeSpan Timeout
        {
            get
            {
                return clientTimeout;
            }

            set
            {
                if (value.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else if (value > TimeSpan.FromDays(365))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else
                {
                    clientTimeout   = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Boolean"/> value that controls whether the <see cref="CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent when making remote procedure calls.
        /// </summary>
        /// <value><b>true</b> if the default credentials are used; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        /// <remarks>
        ///     <para>
        ///         Some XML-RPC servers require that the client be authenticated before the server executes remote procedures on its behalf.
        ///         Set this property to <b>true</b> when this <see cref="XmlRpcClient"/> object should, if requested by the server, authenticate using the
        ///         default credentials of the currently logged on user. For client applications, this is the desired behavior in most scenarios.
        ///     </para>
        ///     <para>
        ///         Credentials information can also be specified using the application and machine configuration files.
        ///         For more information, see <see cref="Argotic.Configuration.XmlRpcClientNetworkElement"/> Element (Network Settings).
        ///     </para>
        ///     <para>
        ///         If the UseDefaultCredentials property is set to <b>false</b>, then the value set in the <see cref="Credentials"/> property
        ///         will be used for the credentials when connecting to the server. If the UseDefaultCredentials property is set to <b>false</b>
        ///         and the <see cref="Credentials"/> property has not been set, then remote procedure calls are sent to the server anonymously.
        ///     </para>
        /// </remarks>
        public bool UseDefaultCredentials
        {
            get
            {
                return clientUsesDefaultCredentials;
            }

            set
            {
                clientUsesDefaultCredentials = value;
            }
        }

        /// <summary>
        /// Gets or sets information such as the client application name, version, host operating system, and language.
        /// </summary>
        /// <value>Information such as the client application name, version, host operating system, and language. The default value is an agent that describes this syndication framework.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string UserAgent
        {
            get
            {
                return clientUserAgent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                clientUserAgent = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the client asynchronous send operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if client asynchronous send operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncSendHasBeenCancelled
        {
            get
            {
                return clientAsyncSendCancelled;
            }

            set
            {
                clientAsyncSendCancelled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the client is in the process of sending a remote procedure call.
        /// </summary>
        /// <value><b>true</b> if client is in the process of sending a remote procedure call, otherwise <b>false</b>.</value>
        internal bool SendOperationInProgress
        {
            get
            {
                return clientIsSending;
            }

            set
            {
                clientIsSending = value;
            }
        }

        /// <summary>
        /// Returns the scalar type identifier for the supplied <see cref="XmlRpcScalarValueType"/>.
        /// </summary>
        /// <param name="type">The <see cref="XmlRpcScalarValueType"/> to get the scalar type identifier for.</param>
        /// <returns>The scalar type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        /// <example>
        ///     <code
        ///         lang="cs"
        ///         title="The following code example demonstrates the usage of the ScalarTypeAsString method."
        ///     />
        /// </example>
        public static string ScalarTypeAsString(XmlRpcScalarValueType type)
        {
            string name = String.Empty;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(XmlRpcScalarValueType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(XmlRpcScalarValueType))
                {
                    XmlRpcScalarValueType valueType = (XmlRpcScalarValueType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (valueType == type)
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

        /// <summary>
        /// Returns the <see cref="XmlRpcScalarValueType"/> enumeration value that corresponds to the specified scalar type name.
        /// </summary>
        /// <param name="name">The name of the scalar type.</param>
        /// <returns>A <see cref="XmlRpcScalarValueType"/> enumeration value that corresponds to the specified string, otherwise returns <b>XmlRpcScalarValueType.None</b>.</returns>
        /// <remarks>This method disregards case of specified scalar type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <example>
        ///     <code
        ///         lang="cs"
        ///         title="The following code example demonstrates the usage of the ScalarTypeByName method."
        ///     />
        /// </example>
        public static XmlRpcScalarValueType ScalarTypeByName(string name)
        {
            XmlRpcScalarValueType valueType = XmlRpcScalarValueType.None;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(XmlRpcScalarValueType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(XmlRpcScalarValueType))
                {
                    XmlRpcScalarValueType type  = (XmlRpcScalarValueType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            valueType   = type;
                            break;
                        }
                    }
                }
            }

            return valueType;
        }

        /// <summary>
        /// Constructs a new <see cref="IXmlRpcValue"/> object from the specified <see cref="XPathNavigator"/>.
        /// Parameters specify the XML data source and the variable where the new <see cref="IXmlRpcValue"/> object is returned.
        /// </summary>
        /// <param name="source">A <see cref="XPathNavigator"/> that represents the XML data source to be parsed.</param>
        /// <param name="value">
        ///     When this method returns, contains an object that represents the <see cref="IXmlRpcValue"/> specified by the <paramref name="source"/>, or <b>null</b> if the conversion failed.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <b>true</b> if <paramref name="source"/> was converted successfully; otherwise, <b>false</b>.
        ///     This operation returns <b>false</b> if the <paramref name="source"/> parameter is a null reference (Nothing in Visual Basic),
        ///     or represents XML data that is not in the expected format.
        /// </returns>
        /// <remarks>
        ///     The <paramref name="source"/> is expected to represent an XML-RPC <b>value</b> node.
        /// </remarks>
        public static bool TryParseValue(XPathNavigator source, out IXmlRpcValue value)
        {
            if (source == null || String.Compare(source.Name, "value", StringComparison.OrdinalIgnoreCase) != 0)
            {
                value   = null;
                return false;
            }

            if(source.HasChildren)
            {
                XPathNavigator navigator    = source.CreateNavigator();
                if (navigator.MoveToFirstChild())
                {
                    if (String.Compare(navigator.Name, "i4", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        int scalar;
                        if (Int32.TryParse(navigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out scalar))
                        {
                            value   = new XmlRpcScalarValue(scalar);
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "int", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        int scalar;
                        if (Int32.TryParse(navigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out scalar))
                        {
                            value   = new XmlRpcScalarValue(scalar);
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "boolean", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bool scalar;
                        if (XmlRpcClient.TryParseBoolean(navigator.Value, out scalar))
                        {
                            value   = new XmlRpcScalarValue(scalar);
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "string", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        value   = new XmlRpcScalarValue(navigator.Value);
                        return true;
                    }
                    else if (String.Compare(navigator.Name, "double", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        double scalar;
                        if (Double.TryParse(navigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out scalar))
                        {
                            value   = new XmlRpcScalarValue(scalar);
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "dateTime.iso8601", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DateTime scalar;
                        if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(navigator.Value, out scalar))
                        {
                            value   = new XmlRpcScalarValue(scalar);
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "base64", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if(!String.IsNullOrEmpty(navigator.Value))
                        {
                            try
                            {
                                byte[] data = Convert.FromBase64String(navigator.Value);
                                value       = new XmlRpcScalarValue(data);
                                return true;
                            }
                            catch(FormatException)
                            {
                                value = null;
                                return false;
                            }
                        }
                    }
                    else if (String.Compare(navigator.Name, "struct", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        XmlRpcStructureValue structure  = new XmlRpcStructureValue();
                        if (structure.Load(source))
                        {
                            value   = structure;
                            return true;
                        }
                    }
                    else if (String.Compare(navigator.Name, "array", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        XmlRpcArrayValue array  = new XmlRpcArrayValue();
                        if (array.Load(source))
                        {
                            value   = array;
                            return true;
                        }
                    }
                }
            }
            else if (!String.IsNullOrEmpty(source.Value))
            {
                value   = new XmlRpcScalarValue(source.Value);
                return true;
            }

            value   = null;
            return false;
        }

        /// <summary>
        /// Converts the specified string representation of a logical value to its <see cref="Boolean"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">A string containing the value to convert.</param>
        /// <param name="result">
        ///     When this method returns, if the conversion succeeded, contains <b>true</b> if value is equivalent to <i>1</i>, <i>true</i> or <i>True</i>;
        ///     or <b>false</b> if value is equivalent to <i>0</i>, <i>false</i> or <i>False</i>. If the conversion failed, contains <b>false</b>.
        ///     The conversion fails if value is a null reference (Nothing in Visual Basic) or is not equivalent to <i>1</i>, <i>true</i>, <i>True</i>, <i>0</i>, <i>false</i> or <i>False</i>.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if <paramref name="value"/> was converted successfully; otherwise, <b>false</b>.</returns>
        internal static bool TryParseBoolean(string value, out bool result)
        {
            if (String.Compare(value, "1", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result  = true;
                return true;
            }
            else if (String.Compare(value, "0", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result  = false;
                return true;
            }
            else if (String.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result  = true;
                return true;
            }
            else if (String.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result = false;
                return true;
            }
            else
            {
                result  = false;
                return false;
            }
        }

        /// <summary>
        /// Called when a corresponding asynchronous send operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncSendCallback(IAsyncResult result)
        {
            XmlRpcResponse response         = null;
            WebRequest httpWebRequest       = null;
            XmlRpcClient client             = null;
            Uri host                        = null;
            XmlRpcMessage message           = null;
            WebRequestOptions options       = null;
            object userToken                = null;

            if (result.IsCompleted)
            {
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest      = parameters[0] as WebRequest;
                client              = parameters[1] as XmlRpcClient;
                host                = parameters[2] as Uri;
                message             = parameters[3] as XmlRpcMessage;
                options             = parameters[4] as WebRequestOptions;
                userToken           = parameters[5];

                if (client != null)
                {
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);

                    response    = new XmlRpcResponse(httpWebResponse);

                    client.OnMessageSent(new XmlRpcMessageSentEventArgs(host, message, response, options, userToken));

                    client.SendOperationInProgress  = false;
                }
            }
        }

        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            this.SendOperationInProgress    = false;
        }

        /// <summary>
        /// Sends the specified message to an XML-RPC server to execute a remote procedure call.
        /// </summary>
        /// <param name="message">A <see cref="XmlRpcMessage"/> that represents the information needed to execute the remote procedure call.</param>
        /// <returns>A <see cref="XmlRpcResponse"/> that represents the server's response to the remote procedure call.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">This <see cref="XmlRpcClient"/> has a <see cref="SendAsync(XmlRpcMessage, Object)"/> call in progress.</exception>
        public XmlRpcResponse Send(XmlRpcMessage message)
        {
            XmlRpcResponse response = null;

            Guard.ArgumentNotNull(message, "message");

            if(this.Host == null)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send XML-RPC message. The Host property has not been initialized. \n\r Message payload: {0}", message));
            }
            else if (this.SendOperationInProgress)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send XML-RPC message. The XmlRpcClient has a SendAsync call in progress. \n\r Message payload: {0}", message));
            }

            WebRequest webRequest   = XmlRpcClient.CreateWebRequest(this.Host, this.UserAgent, message, this.UseDefaultCredentials, this.clientOptions);

            using (WebResponse webResponse = (WebResponse)webRequest.GetResponse())
            {
                response    = new XmlRpcResponse(webResponse);
            }

            return response;
        }

        /// <summary>
        /// Sends the specified message to an XML-RPC server to execute a remote procedure call.
        /// This method does not block the calling thread and allows the caller to pass an object to the method that is invoked when the operation completes.
        /// </summary>
        /// <param name="message">A <see cref="XmlRpcMessage"/> that represents the information needed to execute the remote procedure call.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the remote procedure call has been sent or the operation has been cancelled, add an event handler to the <see cref="SendCompleted"/> event.
        ///         You can cancel a <see cref="SendAsync(XmlRpcMessage, Object)"/> operation by calling the <see cref="SendAsyncCancel()"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">This <see cref="XmlRpcClient"/> has a <see cref="SendAsync(XmlRpcMessage, Object)"/> call in progress.</exception>
        //[HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
        public void SendAsync(XmlRpcMessage message, Object userToken)
        {
            Guard.ArgumentNotNull(message, "message");

            if (this.Host == null)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send XML-RPC message. The Host property has not been initialized. \n\r Message payload: {0}", message));
            }
            else if (this.SendOperationInProgress)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send XML-RPC message. The XmlRpcClient has a SendAsync call in progress. \n\r Message payload: {0}", message));
            }

            this.SendOperationInProgress    = true;
            this.AsyncSendHasBeenCancelled  = false;

            asyncHttpWebRequest = XmlRpcClient.CreateWebRequest(this.Host, this.UserAgent, message, this.UseDefaultCredentials, this.clientOptions);

            object[] state      = new object[6] { asyncHttpWebRequest, this, this.Host, message, this.clientOptions, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncSendCallback), state);

            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncTimeoutCallback), state, this.Timeout, true);
        }

        /// <summary>
        /// Cancels an asynchronous operation to send a remote procedure call.
        /// </summary>
        /// <remarks>
        ///     Use the <see cref="SendAsyncCancel()"/> method to cancel a pending <see cref="SendAsync(XmlRpcMessage, Object)"/> operation.
        ///     If there is a remote procedure call waiting to be sent, this method releases resources used to execute the send operation and cancels the pending operation.
        ///     If there is no send operation pending, this method does nothing.
        /// </remarks>
        public void SendAsyncCancel()
        {
            if (this.SendOperationInProgress && !this.AsyncSendHasBeenCancelled)
            {
                this.AsyncSendHasBeenCancelled  = true;
                asyncHttpWebRequest.Abort();
            }
        }

        /// <summary>
        /// Initializes a new <see cref="WebRequest"/> suitable for sending a remote procedure call using the supplied host, user agent, message, credentials, and proxy.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
        /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
        /// <param name="message">A <see cref="XmlRpcMessage"/> that represents the information needed to execute the remote procedure call.</param>
        /// <param name="useDefaultCredentials">
        ///     Controls whether the <see cref="CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent when making remote procedure calls.
        /// </param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        private static WebRequest CreateWebRequest(Uri host, string userAgent, XmlRpcMessage message, bool useDefaultCredentials, WebRequestOptions options)
        {
            HttpWebRequest httpRequest  = null;
            byte[] payloadData;

            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNullOrEmptyString(userAgent, "userAgent");
            Guard.ArgumentNotNull(message, "message");

            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Document;
                settings.Encoding           = message.Encoding;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = false;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    message.WriteTo(writer);
                    writer.Flush();
                }

                stream.Seek(0, SeekOrigin.Begin);
                payloadData                 = message.Encoding.GetBytes((new StreamReader(stream)).ReadToEnd());
            }

            httpRequest                     = (HttpWebRequest)HttpWebRequest.Create(host);
            httpRequest.Method              = "POST";
            httpRequest.ContentLength       = payloadData.Length;
            httpRequest.ContentType         = String.Format(null, "text/xml; charset={0}", message.Encoding.WebName);
            httpRequest.UserAgent           = userAgent;
            if (options != null) options.ApplyOptions(httpRequest);

            if(useDefaultCredentials)
            {
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            using (Stream stream = httpRequest.GetRequestStream())
            {
                stream.Write(payloadData, 0, payloadData.Length);
            }

            return httpRequest;
        }

        /// <summary>
        /// Initializes the current instance using the application configuration settings.
        /// </summary>
        /// <seealso cref="XmlRpcClientSection"/>
        private void Initialize()
        {
            XmlRpcClientSection clientConfiguration = PrivilegedConfigurationManager.GetXmlRpcClientSection();

            if (clientConfiguration != null)
            {
                if(clientConfiguration.Timeout.TotalMilliseconds > 0 && clientConfiguration.Timeout < TimeSpan.FromDays(365))
                {
                    this.Timeout    = clientConfiguration.Timeout;
                }

                if (!String.IsNullOrEmpty(clientConfiguration.UserAgent))
                {
                    this.UserAgent  = clientConfiguration.UserAgent;
                }

                if (clientConfiguration.Network != null)
                {
                    this.UseDefaultCredentials  = clientConfiguration.Network.DefaultCredentials;

                    if (clientConfiguration.Network.Credential != null)
                    {
                        this.Credentials    = clientConfiguration.Network.Credential;
                    }

                    if (clientConfiguration.Network.Host != null)
                    {
                        this.Host   = clientConfiguration.Network.Host;
                    }
                }
            }
        }
    }
}