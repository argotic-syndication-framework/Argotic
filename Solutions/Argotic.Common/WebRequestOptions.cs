using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Argotic.Common;

/// <summary>
/// Holds options that should be applied to web requests.
/// </summary>
[Serializable]
public class WebRequestOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebRequestOptions"/> class.
    /// </summary>
    public WebRequestOptions() : this(null, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebRequestOptions"/> class using the specified <see cref="ICredentials">credentials</see>.
    /// </summary>
    /// <param name="credentials">
    ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the source resource when required. This value can be <b>null</b>.
    /// </param>
    public WebRequestOptions(ICredentials credentials) : this(credentials, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebRequestOptions"/> class using the specified <see cref="ICredentials">credentials</see> and  <see cref="IWebProxy">proxy</see>.
    /// </summary>
    /// <param name="credentials">
    ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the source resource when required. This value can be <b>null</b>.
    /// </param>
    /// <param name="proxy">
    ///     A <see cref="IWebProxy"/> that provides proxy access to the source resource when required. This value can be <b>null</b>.
    /// </param>
    public WebRequestOptions(ICredentials credentials, IWebProxy proxy)
    {
        _allowAutoRedirect = true;
        _keepAlive = true;
        _credentials = credentials;
        _proxy = proxy;

        if (credentials == null)
        {
            _useDefaultCredentials = true;
        }
    }

    [NonSerialized] private RequestCachePolicy _cachePolicy;
    private ICredentials _credentials;
    private IWebProxy _proxy;
    private bool? _useDefaultCredentials;

    /// <summary>Gets or sets values indicating the level of authentication and impersonation used for this request.</summary>
    public AuthenticationLevel? AuthenticationLevel { get; set; }

    /// <summary>Gets or sets the cache policy for this request.</summary>
    public RequestCachePolicy CachePolicy
    {
        get { return _cachePolicy; }
        set { _cachePolicy = value; }
    }

    /// <summary>Gets or sets the name of the connection group for the request.</summary>
    public string ConnectionGroupName { get; set; }

    /// <summary>Gets or sets the network credentials used for authenticating the request with the Internet resource.</summary>
    public ICredentials Credentials
    {
        get { return _credentials; }
        set { _credentials = value; }
    }

    /// <summary>Gets the collection of header name/value pairs associated with the request.</summary>
    public WebHeaderCollection Headers { get; } = [];

    /// <summary>Gets or sets the impersonation level for the current request.</summary>
    public TokenImpersonationLevel? ImpersonationLevel { get; set; }

    /// <summary>Indicates whether to pre-authenticate the request.</summary>
    public bool? PreAuthenticate { get; set; }

    /// <summary>Gets or sets the network proxy to use to access this Internet resource.</summary>
    public IWebProxy Proxy
    {
        get { return _proxy; }
        set { _proxy = value; }
    }

    /// <summary>Gets or sets the length of time before the request times out.</summary>
    public int? Timeout { get; set; }

    /// <summary>Gets or sets a <see cref="T:System.Boolean"/> value that controls whether <see cref="P:System.Net.CredentialCache.DefaultCredentials"></see> are sent with requests.</summary>
    public bool? UseDefaultCredentials
    {
        get { return _useDefaultCredentials; }
        set { _useDefaultCredentials = value; }
    }

    /// <summary>Gets or sets a byte offset into the file being downloaded by this request.</summary>
    public long? ContentOffset { get; set; }

    /// <summary>Gets or sets a <see cref="T:System.Boolean"/> that specifies that an SSL connection should be used.</summary>
    public bool? EnableSsl { get; set; }

    /// <summary>Gets or sets the new name of a file being renamed.</summary>
    public string RenameTo { get; set; }

    /// <summary>Gets or sets a <see cref="T:System.Boolean"/> value that specifies the data type for file transfers.</summary>
    public bool? UseBinary { get; set; }

    /// <summary>Gets or sets the behavior of a client application's data transfer process.</summary>
    public bool? UsePassive { get; set; }

    private bool? _allowAutoRedirect;

    /// <summary>Gets or sets the value of the Accept HTTP header.</summary>
    public string Accept { get; set; }

    /// <summary>Gets or sets a value that indicates whether the request should follow redirection responses.</summary>
    public bool? AllowAutoRedirect
    {
        get { return _allowAutoRedirect; }
        set { _allowAutoRedirect = value; }
    }

    /// <summary>Gets or sets a value that indicates whether to buffer the data sent to the Internet resource.</summary>
    public bool? AllowWriteStreamBuffering { get; set; }

    /// <summary>Gets or sets the type of decompression that is used.</summary>
    public DecompressionMethods? AutomaticDecompression { get; set; }

    /// <summary>Gets or sets the value of the Connection HTTP header.</summary>
    public string Connection { get; set; }

    /// <summary>Gets or sets the delegate method called when an HTTP 100-continue response is received from the Internet resource.</summary>
    public HttpContinueDelegate ContinueDelegate { get; set; }

    /// <summary>Gets or sets the cookies associated with the request.</summary>
    public CookieContainer CookieContainer { get; set; }

    /// <summary>Gets or sets the value of the Expect HTTP header.</summary>
    public string Expect { get; set; }

    /// <summary>Gets or sets the maximum number of redirects that the request follows.</summary>
    public int? MaximumAutomaticRedirections { get; set; }

    /// <summary>Gets or sets the maximum allowed length of the response headers.</summary>
    public int? MaximumResponseHeadersLength { get; set; }

    /// <summary>Gets or sets the media type of the request.</summary>
    public string MediaType { get; set; }

    /// <summary>Gets or sets a value that indicates whether to pipeline the request to the Internet resource.</summary>
    public bool? Pipelined { get; set; }

    /// <summary>Gets or sets the version of HTTP to use for the request.</summary>
    public Version ProtocolVersion { get; set; }

    /// <summary>Gets or sets the value of the Referer HTTP header.</summary>
    public string Referer { get; set; }

    /// <summary>Gets or sets a value that indicates whether to send data in segments to the Internet resource.</summary>
    public bool? SendChunked { get; set; }

    /// <summary>Gets or sets the value of the Transfer-encoding HTTP header.</summary>
    public string TransferEncoding { get; set; }

    /// <summary>Gets or sets a value that indicates whether to allow high-speed NTLM-authenticated connection sharing.</summary>
    public bool? UnsafeAuthenticatedConnectionSharing { get; set; }

    /// <summary>Gets or sets the value of the User-agent HTTP header.</summary>
    public string UserAgent { get; set; }

    private bool? _keepAlive;

    /// <summary>Gets the collection of security certificates that are associated with this request.</summary>
    public X509CertificateCollection ClientCertificates { get; } = [];

    /// <summary>Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.</summary>
    public bool? KeepAlive
    {
        get { return _keepAlive; }
        set { _keepAlive = value; }
    }

    /// <summary>Gets or sets a time-out when writing to or reading from a stream.</summary>
    public int? ReadWriteTimeout { get; set; }

    /// <summary>
    /// Applies all options on the current instance to the supplied <see cref="WebRequest"/>.
    /// </summary>
    /// <param name="request">A <see cref="WebRequest"/> that should be configured.</param>
    public void ApplyOptions(WebRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (AuthenticationLevel != null) request.AuthenticationLevel = AuthenticationLevel.Value;
        if (CachePolicy != null) request.CachePolicy = CachePolicy;
        if (ConnectionGroupName != null) request.ConnectionGroupName = ConnectionGroupName;
        if (Credentials != null) request.Credentials = Credentials;
        if (Headers.Count > 0) request.Headers = Headers;
        if (ImpersonationLevel != null) request.ImpersonationLevel = ImpersonationLevel.Value;
        if (PreAuthenticate != null) request.PreAuthenticate = PreAuthenticate.Value;
        if (Proxy != null) request.Proxy = Proxy;
        if (Timeout != null) request.Timeout = Timeout.Value;
        if (UseDefaultCredentials != null) request.UseDefaultCredentials = UseDefaultCredentials.Value;

        if (request is FtpWebRequest ftpRequest)
        {
            ApplyFtpOptions(ftpRequest);
        }

        if (request is HttpWebRequest httpRequest)
        {
            ApplyHttpOptions(httpRequest);
        }
    }

    private void ApplyFtpOptions(FtpWebRequest ftpRequest)
    {
        if (ContentOffset != null) ftpRequest.ContentOffset = ContentOffset.Value;
        if (EnableSsl != null) ftpRequest.EnableSsl = EnableSsl.Value;
        if (RenameTo != null) ftpRequest.RenameTo = RenameTo;
        if (UseBinary != null) ftpRequest.UseBinary = UseBinary.Value;
        if (UsePassive != null) ftpRequest.UsePassive = UsePassive.Value;

        if (ClientCertificates.Count > 0) ftpRequest.ClientCertificates = ClientCertificates;
        if (KeepAlive != null) ftpRequest.KeepAlive = KeepAlive.Value;
        if (ReadWriteTimeout != null) ftpRequest.ReadWriteTimeout = ReadWriteTimeout.Value;
    }

    private void ApplyHttpOptions(HttpWebRequest httpRequest)
    {
        if (Accept != null) httpRequest.Accept = Accept;
        if (AllowAutoRedirect != null) httpRequest.AllowAutoRedirect = AllowAutoRedirect.Value;
        if (AllowWriteStreamBuffering != null) httpRequest.AllowWriteStreamBuffering = AllowWriteStreamBuffering.Value;
        if (AutomaticDecompression != null) httpRequest.AutomaticDecompression = AutomaticDecompression.Value;
        if (Connection != null) httpRequest.Connection = Connection;
        if (ContinueDelegate != null) httpRequest.ContinueDelegate = ContinueDelegate;
        if (CookieContainer != null) httpRequest.CookieContainer = CookieContainer;
        if (Expect != null) httpRequest.Expect = Expect;
        if (MaximumAutomaticRedirections != null) httpRequest.MaximumAutomaticRedirections = MaximumAutomaticRedirections.Value;
        if (MaximumResponseHeadersLength != null) httpRequest.MaximumResponseHeadersLength = MaximumResponseHeadersLength.Value;
        if (MediaType != null) httpRequest.MediaType = MediaType;
        if (Pipelined != null) httpRequest.Pipelined = Pipelined.Value;
        if (ProtocolVersion != null) httpRequest.ProtocolVersion = ProtocolVersion;
        if (Referer != null) httpRequest.Referer = Referer;
        if (SendChunked != null) httpRequest.SendChunked = SendChunked.Value;
        if (TransferEncoding != null) httpRequest.TransferEncoding = TransferEncoding;
        if (UnsafeAuthenticatedConnectionSharing != null) httpRequest.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing.Value;
        if (UserAgent != null) httpRequest.UserAgent = UserAgent;

        if (ClientCertificates.Count > 0) httpRequest.ClientCertificates = ClientCertificates;
        if (KeepAlive != null) httpRequest.KeepAlive = KeepAlive.Value;
        if (ReadWriteTimeout != null) httpRequest.ReadWriteTimeout = ReadWriteTimeout.Value;
    }
}