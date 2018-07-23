namespace Argotic.Net
{
    using System;
    using System.Net;
    using Argotic.Common;

    /// <summary>
    /// Provides data for the <see cref="TrackbackClient.SendCompleted"/> event.
    /// </summary>
    /// <remarks>
    ///     A <see cref="TrackbackClient.SendCompleted"/> event occurs whenever the <see cref="TrackbackClient.SendAsync(TrackbackMessage, object)"/> method is called.
    /// </remarks>
    /// <seealso cref="TrackbackClient.SendAsync(TrackbackMessage, object)"/>
    /// <seealso cref="TrackbackClient"/>
    [Serializable()]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public class TrackbackMessageSentEventArgs : EventArgs, IComparable
    {
        /// <summary>
        /// Private member to hold instance of event with no event data.
        /// </summary>
        private static readonly TrackbackMessageSentEventArgs emptyEventArguments = new TrackbackMessageSentEventArgs();

        /// <summary>
        /// Private member to hold the Trackback ping request payload that was sent.
        /// </summary>
        private TrackbackMessage eventMessage;

        /// <summary>
        /// Private member to hold the response to the Trackback ping request.
        /// </summary>
        private TrackbackResponse eventResponse;

        /// <summary>
        /// Private member to hold the location of the host computer that the Trackback ping request was sent to.
        /// </summary>
        private Uri eventHost;

        /// <summary>
        /// Private member to hold the web request options.
        /// </summary>
        private WebRequestOptions eventOptions = new WebRequestOptions();

        /// <summary>
        /// Private member to hold an object containing state information that was passed to the asynchronous send operation.
        /// </summary>
        private object eventUserToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessageSentEventArgs"/> class.
        /// </summary>
        public TrackbackMessageSentEventArgs()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessageSentEventArgs"/> class using the supplied parameters.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for the XML-RPC transaction.</param>
        /// <param name="message">An <see cref="TrackbackMessage"/> that represents the Trackback ping request payload.</param>
        /// <param name="response">An <see cref="TrackbackResponse"/> that represents the response to the Trackback ping request.</param>
        /// <param name="credentials">A <see cref="ICredentials"/> that represents the authentication credentials utilized by the client when making the Trackback ping request. This parameter may be <b>null</b>.</param>
        /// <param name="proxy">A <see cref="IWebProxy"/> that represents the web proxy utilized by the client to proxy the Trackback ping request. This parameter may be <b>null</b>.</param>
        /// <param name="state">A <see cref="object"/> containing state information that was passed to the asynchronous send operation. This parameter may be <b>null</b>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        public TrackbackMessageSentEventArgs(Uri host, TrackbackMessage message, TrackbackResponse response, ICredentials credentials, IWebProxy proxy, object state)
        {
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(response, "response");

            eventHost = host;
            eventMessage = message;
            eventResponse = response;
            eventOptions = new WebRequestOptions(credentials, proxy);
            eventUserToken = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackMessageSentEventArgs"/> class using the supplied parameters.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for the XML-RPC transaction.</param>
        /// <param name="message">An <see cref="TrackbackMessage"/> that represents the Trackback ping request payload.</param>
        /// <param name="response">An <see cref="TrackbackResponse"/> that represents the response to the Trackback ping request.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="state">A <see cref="object"/> containing state information that was passed to the asynchronous send operation. This parameter may be <b>null</b>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        public TrackbackMessageSentEventArgs(Uri host, TrackbackMessage message, TrackbackResponse response, WebRequestOptions options, object state)
        {
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(response, "response");

            eventHost = host;
            eventMessage = message;
            eventResponse = response;
            eventOptions = options ?? new WebRequestOptions();
            eventUserToken = state;
        }

        /// <summary>
        /// Represents an syndication resource loaded event with no event data.
        /// </summary>
        /// <value>An uninitialized instance of the <see cref="TrackbackMessageSentEventArgs"/> class.</value>
        /// <remarks>The value of Empty is a read-only instance of <see cref="TrackbackMessageSentEventArgs"/> equivalent to the result of calling the <see cref="TrackbackMessageSentEventArgs()"/> constructor.</remarks>
        public static new TrackbackMessageSentEventArgs Empty
        {
            get
            {
                return emptyEventArguments;
            }
        }

        /// <summary>
        /// Gets the authentication credentials utilized by the client when making the Trackback ping request.
        /// </summary>
        /// <value>
        ///     A <see cref="ICredentials"/> that represents the authentication credentials utilized by the client when making the Trackback ping request.
        ///     If no credentials were provided, returns <b>null</b>.
        /// </value>
        public ICredentials Credentials
        {
            get
            {
                return eventOptions.Credentials;
            }
        }

        /// <summary>
        /// Gets the the location of the host computer that the Trackback ping request was sent to.
        /// </summary>
        /// <value>
        ///     A <see cref="Uri"/> that represents the URL of the host computer used for the XML-RPC transaction.
        /// </value>
        public Uri Host
        {
            get
            {
                return eventHost;
            }
        }

        /// <summary>
        /// Gets the Trackback ping request payload that was sent.
        /// </summary>
        /// <value>
        ///     An <see cref="TrackbackMessage"/> that represents the Trackback ping request payload.
        /// </value>
        public TrackbackMessage Message
        {
            get
            {
                return eventMessage;
            }
        }

        /// <summary>
        /// Gets the web proxy utilized by the client to proxy the Trackback ping request.
        /// </summary>
        /// <value>
        ///     A <see cref="IWebProxy"/> that represents the web proxy utilized by the client to proxy the Trackback ping request.
        ///     If no proxy was used, returns <b>null</b>.
        /// </value>
        public IWebProxy Proxy
        {
            get
            {
                return eventOptions.Proxy;
            }
        }

        /// <summary>
        /// Gets the response to the Trackback ping request.
        /// </summary>
        /// <value>
        ///     An <see cref="TrackbackResponse"/> that represents the response to the Trackback ping request.
        /// </value>
        public TrackbackResponse Response
        {
            get
            {
                return eventResponse;
            }
        }

        /// <summary>
        /// Gets an <see cref="object"/> containing state information that was passed to the asynchronous send operation.
        /// </summary>
        /// <value>
        ///     A <see cref="object"/> containing state information that was passed to the asynchronous send operation. If no user token provided, returns <b>null</b>.
        /// </value>
        public object State
        {
            get
            {
                return eventUserToken;
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackMessageSentEventArgs"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="TrackbackMessageSentEventArgs"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
        /// </remarks>
        public override string ToString()
        {
            string host = this.Host != null ? this.Host.ToString() : string.Empty;
            string message = this.Message != null ? this.Message.ToString() : string.Empty;
            string response = this.Response != null ? this.Response.ToString() : string.Empty;
            string credentials = this.Credentials != null ? this.Credentials.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string proxy = this.Proxy != null ? this.Proxy.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;
            string state = this.State != null ? this.State.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : string.Empty;

            return string.Format(null, "[TrackbackMessageSentEventArgs(Host = \"{0}\", Message = \"{1}\", Response = \"{2}\", Credentials = \"{3}\", Proxy = \"{4}\", State = \"{5}\")]", host, message, response, credentials, proxy, state);
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

            TrackbackMessageSentEventArgs value = obj as TrackbackMessageSentEventArgs;

            if (value != null)
            {
                int result = 0;
                result = result | this.Message.CompareTo(value.Message);
                result = result | this.Response.CompareTo(value.Response);
                result = result | Uri.Compare(this.Host, value.Host, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is TrackbackMessageSentEventArgs))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
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

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(TrackbackMessageSentEventArgs first, TrackbackMessageSentEventArgs second)
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
        public static bool operator !=(TrackbackMessageSentEventArgs first, TrackbackMessageSentEventArgs second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(TrackbackMessageSentEventArgs first, TrackbackMessageSentEventArgs second)
        {
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

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(TrackbackMessageSentEventArgs first, TrackbackMessageSentEventArgs second)
        {
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
    }
}