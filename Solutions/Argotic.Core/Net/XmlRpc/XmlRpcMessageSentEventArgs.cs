/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/14/2008	brian.kuhn	Created XmlRpcMessageSentEventArgs Class
****************************************************************************/
using System;
using System.Net;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Provides data for the <see cref="XmlRpcClient.SendCompleted"/> event.
    /// </summary>
    /// <remarks>
    ///     A <see cref="XmlRpcClient.SendCompleted"/> event occurs whenever the <see cref="XmlRpcClient.SendAsync(XmlRpcMessage, Object)"/> method is called.
    /// </remarks>
    /// <seealso cref="XmlRpcClient.SendAsync(XmlRpcMessage, Object)"/>
    /// <seealso cref="XmlRpcClient"/>
    [Serializable()]
    public class XmlRpcMessageSentEventArgs : EventArgs, IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold instance of event with no event data.
        /// </summary>
        private static readonly XmlRpcMessageSentEventArgs emptyEventArguments  = new XmlRpcMessageSentEventArgs();
        /// <summary>
        /// Private member to hold the remote procedure call payload that was sent.
        /// </summary>
        private XmlRpcMessage eventMessage;
        /// <summary>
        /// Private member to hold the response to the remote procedure call.
        /// </summary>
        private XmlRpcResponse eventResponse;
        /// <summary>
        /// Private member to hold the location of the host computer that the remote procedure call was sent to.
        /// </summary>
        private Uri eventHost;
        /// <summary>
        /// Private member to hold the web request options.
        /// </summary>
        private WebRequestOptions eventOptions = new WebRequestOptions();
        /// <summary>
        /// Private member to hold an object containing state information that was passed to the asynchronous send operation.
        /// </summary>
        private Object eventUserToken;
        #endregion
        
        //============================================================
		//	CONSTRUCTORS
        //============================================================
        #region XmlRpcMessageSentEventArgs()
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessageSentEventArgs"/> class.
        /// </summary>
        public XmlRpcMessageSentEventArgs()
		{
			//------------------------------------------------------------
			//	
			//------------------------------------------------------------
		}
		#endregion

        #region XmlRpcMessageSentEventArgs(Uri host, XmlRpcMessage message, XmlRpcResponse response, ICredentials credentials, IWebProxy proxy, Object state)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessageSentEventArgs"/> class using the supplied parameters.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for the XML-RPC transaction.</param>
        /// <param name="message">An <see cref="XmlRpcMessage"/> that represents the remote procedure call payload.</param>
        /// <param name="response">An <see cref="XmlRpcResponse"/> that represents the response to the remote procedure call.</param>
        /// <param name="credentials">A <see cref="ICredentials"/> that represents the authentication credentials utilized by the client when making the remote procedure call. This parameter may be <b>null</b>.</param>
        /// <param name="proxy">A <see cref="IWebProxy"/> that represents the web proxy utilized by the client to proxy the remote procedure call. This parameter may be <b>null</b>.</param>
        /// <param name="state">A <see cref="Object"/> containing state information that was passed to the asynchronous send operation. This parameter may be <b>null</b>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcMessageSentEventArgs(Uri host, XmlRpcMessage message, XmlRpcResponse response, ICredentials credentials, IWebProxy proxy, Object state)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(response, "response");

            //------------------------------------------------------------
            //	Initialize class members
            //------------------------------------------------------------
            eventHost           = host;
            eventMessage        = message;
            eventResponse       = response;
            eventOptions        = new WebRequestOptions(credentials, proxy);
            eventUserToken      = state;
        }
        #endregion

        #region XmlRpcMessageSentEventArgs(Uri host, XmlRpcMessage message, XmlRpcResponse response, WebRequestOptions options, Object state)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcMessageSentEventArgs"/> class using the supplied parameters.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for the XML-RPC transaction.</param>
        /// <param name="message">An <see cref="XmlRpcMessage"/> that represents the remote procedure call payload.</param>
        /// <param name="response">An <see cref="XmlRpcResponse"/> that represents the response to the remote procedure call.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="state">A <see cref="Object"/> containing state information that was passed to the asynchronous send operation. This parameter may be <b>null</b>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcMessageSentEventArgs(Uri host, XmlRpcMessage message, XmlRpcResponse response, WebRequestOptions options, Object state)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(response, "response");

            //------------------------------------------------------------
            //	Initialize class members
            //------------------------------------------------------------
            eventHost           = host;
            eventMessage        = message;
            eventResponse       = response;
            eventOptions        = options ?? new WebRequestOptions();
            eventUserToken      = state;
        }
        #endregion

        //============================================================
        //	STATIC PROPERTIES
        //============================================================
        #region Empty
        /// <summary>
        /// Represents an syndication resource loaded event with no event data.
        /// </summary>
        /// <value>An uninitialized instance of the <see cref="XmlRpcMessageSentEventArgs"/> class.</value>
        /// <remarks>The value of Empty is a read-only instance of <see cref="XmlRpcMessageSentEventArgs"/> equivalent to the result of calling the <see cref="XmlRpcMessageSentEventArgs()"/> constructor.</remarks>
        public static new XmlRpcMessageSentEventArgs Empty
        {
            get
            {
                return emptyEventArguments;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Credentials
        /// <summary>
        /// Gets the authentication credentials utilized by the client when making the remote procedure call.
        /// </summary>
        /// <value>
        ///     A <see cref="ICredentials"/> that represents the authentication credentials utilized by the client when making the remote procedure call. 
        ///     If no credentials were provided, returns <b>null</b>.
        /// </value>
        public ICredentials Credentials
        {
            get
            {
                return eventOptions.Credentials;
            }
        }
        #endregion

        #region Host
        /// <summary>
        /// Gets the the location of the host computer that the remote procedure call was sent to.
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
        #endregion

        #region Message
        /// <summary>
        /// Gets the remote procedure call payload that was sent.
        /// </summary>
        /// <value>
        ///     An <see cref="XmlRpcMessage"/> that represents the remote procedure call payload.
        /// </value>
        public XmlRpcMessage Message
        {
            get
            {
                return eventMessage;
            }
        }
        #endregion

        #region Proxy
        /// <summary>
        /// Gets the web proxy utilized by the client to proxy the remote procedure call.
        /// </summary>
        /// <value>
        ///     A <see cref="IWebProxy"/> that represents the web proxy utilized by the client to proxy the remote procedure call. 
        ///     If no proxy was used, returns <b>null</b>.
        /// </value>
        public IWebProxy Proxy
        {
            get
            {
                return eventOptions.Proxy;
            }
        }
        #endregion

        #region Response
        /// <summary>
        /// Gets the response to the remote procedure call.
        /// </summary>
        /// <value>
        ///     An <see cref="XmlRpcResponse"/> that represents the response to the remote procedure call.
        /// </value>
        public XmlRpcResponse Response
        {
            get
            {
                return eventResponse;
            }
        }
        #endregion

        #region State
        /// <summary>
        /// Gets an <see cref="Object"/> containing state information that was passed to the asynchronous send operation.
        /// </summary>
        /// <value>
        ///     A <see cref="Object"/> containing state information that was passed to the asynchronous send operation. If no user token provided, returns <b>null</b>.
        /// </value>
        public Object State
        {
            get
            {
                return eventUserToken;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcMessageSentEventArgs"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcMessageSentEventArgs"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            string host         = this.Host != null ? this.Host.ToString() : String.Empty;
            string message      = this.Message != null ? this.Message.ToString() : String.Empty;
            string response     = this.Response != null ? this.Response.ToString() : String.Empty;
            string credentials  = this.Credentials != null ? this.Credentials.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : String.Empty;
            string proxy        = this.Proxy != null ? this.Proxy.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : String.Empty;
            string state        = this.State != null ? this.State.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : String.Empty;

            return String.Format(null, "[XmlRpcMessageSentEventArgs(Host = \"{0}\", Message = \"{1}\", Response = \"{2}\", Credentials = \"{3}\", Proxy = \"{4}\", State = \"{5}\")]", host, message, response, credentials, proxy, state);
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
            XmlRpcMessageSentEventArgs value  = obj as XmlRpcMessageSentEventArgs;

            if (value != null)
            {
                int result  = 0;
                result      = result | this.Message.CompareTo(value.Message);
                result      = result | this.Response.CompareTo(value.Response);
                result      = result | Uri.Compare(this.Host, value.Host, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is XmlRpcMessageSentEventArgs))
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
        public static bool operator ==(XmlRpcMessageSentEventArgs first, XmlRpcMessageSentEventArgs second)
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
        public static bool operator !=(XmlRpcMessageSentEventArgs first, XmlRpcMessageSentEventArgs second)
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
        public static bool operator <(XmlRpcMessageSentEventArgs first, XmlRpcMessageSentEventArgs second)
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
        public static bool operator >(XmlRpcMessageSentEventArgs first, XmlRpcMessageSentEventArgs second)
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