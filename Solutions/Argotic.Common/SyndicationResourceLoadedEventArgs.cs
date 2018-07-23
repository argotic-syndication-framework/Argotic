namespace Argotic.Common
{
    using System;
    using System.Net;
    using System.Xml.XPath;

    /// <summary>
    /// Provides data for the <see cref="ISyndicationResource.Loaded"/> event.
    /// </summary>
    /// <remarks>
    ///     A <see cref="ISyndicationResource.Loaded"/> event occurs whenever the <see cref="ISyndicationResource.Load(System.Xml.XmlReader)"/>
    ///     or <see cref="ISyndicationResource.Load(System.Xml.XPath.IXPathNavigable)"/> methods are called.
    /// </remarks>
    /// <seealso cref="ISyndicationResource"/>
    /// <seealso cref="ISyndicationResource.Load(System.Xml.XPath.IXPathNavigable)"/>
    /// <seealso cref="ISyndicationResource.Load(System.Xml.XmlReader)"/>
    [Serializable]
    public class SyndicationResourceLoadedEventArgs : EventArgs, IComparable
    {
        /// <summary>
        /// Private member to hold instance of event with no event data.
        /// </summary>
        private static readonly SyndicationResourceLoadedEventArgs EmptyEventArguments = new SyndicationResourceLoadedEventArgs();

        /// <summary>
        /// Private member to hold read-only XPathNavigator object for navigating the XML data used to load the syndication resource.
        /// </summary>
        [NonSerialized]
        private XPathNavigator eventNavigator;

        /// <summary>
        /// Private member to hold the URI that the syndication resource information was retrieved from.
        /// </summary>
        private Uri eventSource;

        /// <summary>
        /// Private member to hold the web request options.
        /// </summary>
        private WebRequestOptions eventOptions = new WebRequestOptions();

        /// <summary>
        /// Private member to hold an object containing state information that was passed to the asynchronous load operation.
        /// </summary>
        private object eventUserToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class.
        /// </summary>
        public SyndicationResourceLoadedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceLoadedEventArgs(IXPathNavigable data)
            : this()
        {
            Guard.ArgumentNotNull(data, "data");

            this.eventNavigator = data.CreateNavigator();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>, <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
        /// <param name="source">
        ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="credentials">
        ///    The <see cref="ICredentials"/> that were used to authenticate the request to an Internet resource. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="proxy">
        ///     The <see cref="IWebProxy"/> used to access the Internet resource. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source, ICredentials credentials, IWebProxy proxy)
            : this(data)
        {
            Guard.ArgumentNotNull(source, "source");

            this.eventSource = source;
            this.eventOptions = new WebRequestOptions(credentials, proxy);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>, <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
        /// <param name="source">
        ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source, WebRequestOptions options)
            : this(data)
        {
            Guard.ArgumentNotNull(source, "source");

            this.eventSource = source;
            this.eventOptions = options ?? new WebRequestOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>, <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and user token.
        /// </summary>
        /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
        /// <param name="source">
        ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="credentials">
        ///    The <see cref="ICredentials"/> that were used to authenticate the request to an Internet resource. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="proxy">
        ///     The <see cref="IWebProxy"/> used to access the Internet resource. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="state">The user-defined object that was passed to the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source, ICredentials credentials, IWebProxy proxy, object state)
            : this(data, source, credentials, proxy)
        {
            this.eventUserToken = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>, <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and user token.
        /// </summary>
        /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
        /// <param name="source">
        ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from. Can be <b>null</b> if syndication resource was not loaded using an Internet resource.
        /// </param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="state">The user-defined object that was passed to the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source, WebRequestOptions options, object state)
            : this(data, source, options)
        {
            this.eventUserToken = state;
        }

        /// <summary>
        /// Gets represents an syndication resource loaded event with no event data.
        /// </summary>
        /// <value>An uninitialized instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class.</value>
        /// <remarks>The value of Empty is a read-only instance of <see cref="SyndicationResourceLoadedEventArgs"/> equivalent to the result of calling the <see cref="SyndicationResourceLoadedEventArgs()"/> constructor.</remarks>
        public static new SyndicationResourceLoadedEventArgs Empty
        {
            get
            {
                return EmptyEventArguments;
            }
        }

        /// <summary>
        /// Gets the network credentials used for authenticating the request to the Internet resource that the syndication resource was loaded from.
        /// </summary>
        /// <value>
        ///     The <see cref="ICredentials"/> that were used to authenticate the request to an Internet resource.
        ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource or no credentials were provided, returns <b>null</b>.
        /// </value>
        /// <seealso cref="ISyndicationResource.Load(Uri, ICredentials, IWebProxy)"/>
        public ICredentials Credentials
        {
            get
            {
                return this.eventOptions.Credentials;
            }
        }

        /// <summary>
        /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
        /// </summary>
        /// <value>
        ///     A read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
        /// </value>
        public XPathNavigator Data
        {
            get
            {
                return this.eventNavigator;
            }
        }

        /// <summary>
        /// Gets the network proxy used to access the Internet resource that the syndication resource was loaded from.
        /// </summary>
        /// <value>
        ///     The <see cref="IWebProxy"/> used to access the Internet resource.
        ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource or no proxy was specified, returns <b>null</b>.
        /// </value>
        /// <seealso cref="ISyndicationResource.Load(Uri, ICredentials, IWebProxy)"/>
        public IWebProxy Proxy
        {
            get
            {
                return this.eventOptions.Proxy;
            }
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
        /// </summary>
        /// <value>
        ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
        ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource, returns <b>null</b>.
        /// </value>
        /// <seealso cref="ISyndicationResource.Load(Uri, ICredentials, IWebProxy)"/>
        public Uri Source
        {
            get
            {
                return this.eventSource;
            }
        }

        /// <summary>
        /// Gets an <see cref="object"/> containing state information that was passed to the asynchronous load operation.
        /// </summary>
        /// <value>
        ///     A <see cref="object"/> containing state information that was passed to the asynchronous load operation.
        ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource or no user token provided, returns <b>null</b>.
        /// </value>
        /// <seealso cref="ISyndicationResource.LoadAsync(Uri, object)"/>
        public object State
        {
            get
            {
                return this.eventUserToken;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
        {
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

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadedEventArgs"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadedEventArgs"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
        /// </remarks>
        public override string ToString()
        {
            string source = this.Source != null ? this.Source.ToString() : string.Empty;
            string data = this.Data != null ? this.Data.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string credentials = this.Credentials != null ? this.Credentials.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string proxy = this.Proxy != null ? this.Proxy.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string state = this.State != null ? this.State.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;

            return string.Format(null, "[SyndicationResourceLoadedEventArgs(Source = \"{0}\", Data = \"{1}\", Credentials = \"{2}\", Proxy = \"{3}\", State = \"{4}\")]", source, data, credentials, proxy, state);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            SyndicationResourceLoadedEventArgs value = obj as SyndicationResourceLoadedEventArgs;

            if (value != null)
            {
                int result = 0;
                result = result | string.Compare(this.Data.OuterXml, value.Data.OuterXml, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Source, value.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SyndicationResourceLoadedEventArgs))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
    }
}