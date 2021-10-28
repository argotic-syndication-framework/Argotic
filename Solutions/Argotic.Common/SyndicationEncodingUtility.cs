using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.XPath;

namespace Argotic.Common
{
    /// <summary>
    /// Provides methods for encoding and decoding information exposed by syndicated content. This class cannot be inherited.
    /// </summary>
    public static class SyndicationEncodingUtility
    {
        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied XML data.
        /// </summary>
        /// <param name="xml">The XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied XML data.
        ///     The supplied <paramref name="xml"/> data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="xml"/> data is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xml"/> data is an empty string.</exception>
        public static XPathNavigator CreateSafeNavigator(string xml)
        {
            XPathNavigator navigator    = null;

            Guard.ArgumentNotNullOrEmptyString(xml, "xml");

            string safeXml  = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(xml);

            using(StringReader reader = new StringReader(safeXml))
            {
                XPathDocument document  = new XPathDocument(reader);
                navigator               = document.CreateNavigator();
            }

            return navigator;
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="stream"/>.
        ///     The supplied <paramref name="stream"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <remarks>
        ///     The character encoding of the supplied <paramref name="stream"/> is automatically determined based on the <i>encoding</i> attribute of the XML document declaration.
        ///     If the character encoding cannot be determined, a default encoding of <see cref="Encoding.UTF8"/> is used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Stream stream)
        {
            Encoding encoding   = Encoding.UTF8;
            byte[] buffer       = null;

            Guard.ArgumentNotNull(stream, "stream");

            buffer      = SyndicationEncodingUtility.GetStreamBytes(stream);

            encoding    = SyndicationEncodingUtility.GetXmlEncoding(buffer);

            using(MemoryStream memoryStream = new MemoryStream(buffer))
            {
                return SyndicationEncodingUtility.CreateSafeNavigator(memoryStream, encoding);
            }
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/> using the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <param name="encoding">A <see cref="Encoding"/> object that indicates the character encoding to use when reading the supplied <paramref name="stream"/>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="stream"/>.
        ///     The supplied <paramref name="stream"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="encoding"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Stream stream, Encoding encoding)
        {
            Guard.ArgumentNotNull(stream, "stream");
            Guard.ArgumentNotNull(encoding, "encoding");

            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                return SyndicationEncodingUtility.CreateSafeNavigator(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="reader"/>.
        ///     The supplied <paramref name="reader"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(TextReader reader)
        {
            Guard.ArgumentNotNull(reader, "reader");

            return SyndicationEncodingUtility.CreateSafeNavigator(reader.ReadToEnd());
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required.
        ///     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        /// </param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="source"/>.
        ///     The supplied <paramref name="source"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            return SyndicationEncodingUtility.CreateSafeNavigator(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="source"/>.
        ///     The supplied <paramref name="source"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Uri source, WebRequestOptions options)
        {
            return SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required.
        ///     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        /// </param>
        /// <param name="encoding">A <see cref="Encoding"/> object that indicates the expected character encoding of the supplied <paramref name="source"/>. This value can be <b>null</b>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="source"/>.
        ///     The supplied <paramref name="source"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <remarks>
        ///     If the <paramref name="encoding"/> is <b>null</b>, the character encoding of the supplied <paramref name="source"/> is determined automatically.
        ///     Otherwise the specified <paramref name="encoding"/> is used when reading the XML data represented by the supplied <paramref name="source"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Uri source, ICredentials credentials, IWebProxy proxy, Encoding encoding)
        {
            return SyndicationEncodingUtility.CreateSafeNavigator(source, new WebRequestOptions(credentials, proxy), encoding);
        }

        /// <summary>
        /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="encoding">A <see cref="Encoding"/> object that indicates the expected character encoding of the supplied <paramref name="source"/>. This value can be <b>null</b>.</param>
        /// <returns>
        ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="source"/>.
        ///     The supplied <paramref name="source"/> XML data is parsed to remove invalid XML characters that would normally prevent
        ///     a navigator from being created.
        /// </returns>
        /// <remarks>
        ///     If the <paramref name="encoding"/> is <b>null</b>, the character encoding of the supplied <paramref name="source"/> is determined automatically.
        ///     Otherwise the specified <paramref name="encoding"/> is used when reading the XML data represented by the supplied <paramref name="source"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XPathNavigator CreateSafeNavigator(Uri source, WebRequestOptions options, Encoding encoding)
        {
            Guard.ArgumentNotNull(source, "source");

            using (WebResponse response = SyndicationEncodingUtility.CreateWebResponse(source, options))
            {
                Stream stream = null;
                HttpWebResponse httpResponse = response as HttpWebResponse;

                if (httpResponse != null)
                {
                    string contentEncoding = httpResponse.ContentEncoding?.ToUpperInvariant();

                    if(string.IsNullOrEmpty(contentEncoding))
                    {
                        stream = response.GetResponseStream();
                    }
                    else
                    {
                        if (contentEncoding.Contains("GZIP"))
                        {
                            stream = new GZipStream(httpResponse.GetResponseStream(), CompressionMode.Decompress);
                        }
                        else if (contentEncoding.Contains("DEFLATE"))
                        {
                            stream = new DeflateStream(httpResponse.GetResponseStream(), CompressionMode.Decompress);
                        }
                        else
                        {
                            stream = httpResponse.GetResponseStream();
                        }
                    }
                }
                else
                {
                    stream = response.GetResponseStream();
                }

                if (encoding != null)
                {
                    return SyndicationEncodingUtility.CreateSafeNavigator(stream, encoding);
                }
                else
                {
                    return SyndicationEncodingUtility.CreateSafeNavigator(stream);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="WebRequest"/> that makes a request for a resource located at the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials if supported by the underlying protocol.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required.
        ///     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings if supported by the underlying protocol.
        /// </param>
        /// <returns>
        ///     An <see cref="WebRequest"/> that makes a request to the <paramref name="source"/>. If unable to create a <see cref="WebRequest"/> for
        ///     the specified <paramref name="source"/>, returns a <b>null</b> reference (Nothing in Visual Basic).
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static WebRequest CreateWebRequest(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            return SyndicationEncodingUtility.CreateWebRequest(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Returns a <see cref="WebRequest"/> that makes a request for a resource located at the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>
        ///     An <see cref="WebRequest"/> that makes a request to the <paramref name="source"/>. If unable to create a <see cref="WebRequest"/> for
        ///     the specified <paramref name="source"/>, returns a <b>null</b> reference (Nothing in Visual Basic).
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static WebRequest CreateWebRequest(Uri source, WebRequestOptions options)
        {
            WebRequest request  = null;

            Guard.ArgumentNotNull(source, "source");

            request             = WebRequest.Create(source);

            if(source.IsAbsoluteUri)
            {
                if (String.Compare(source.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(source.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    HttpWebRequest httpRequest      = (HttpWebRequest)request;
                    httpRequest.UserAgent           = SyndicationDiscoveryUtility.FrameworkUserAgent;
                    request                         = httpRequest;
                }
            }

            if (options != null) options.ApplyOptions(request);
            return request;
        }

        /// <summary>
        /// Returns the <see cref="WebResponse"/> to a request for a resource located at the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials if supported by the underlying protocol.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required.
        ///     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings if supported by the underlying protocol.
        /// </param>
        /// <returns>
        ///     An <see cref="WebResponse"/> that contains the response from the requested resource. If unable to create a <see cref="WebResponse"/> for
        ///     the requested <paramref name="source"/>, returns a <b>null</b> reference (Nothing in Visual Basic).
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static WebResponse CreateWebResponse(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            return SyndicationEncodingUtility.CreateWebResponse(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Returns the <see cref="WebResponse"/> to a request for a resource located at the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>
        ///     An <see cref="WebResponse"/> that contains the response from the requested resource. If unable to create a <see cref="WebResponse"/> for
        ///     the requested <paramref name="source"/>, returns a <b>null</b> reference (Nothing in Visual Basic).
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static WebResponse CreateWebResponse(Uri source, WebRequestOptions options)
        {
            WebResponse response    = null;

            Guard.ArgumentNotNull(source, "source");

            WebRequest webRequest   = SyndicationEncodingUtility.CreateWebRequest(source, options);
            if (webRequest != null)
            {
                response    = webRequest.GetResponse();
            }

            return response;
        }

        /// <summary>
        /// Decodes a base64 encoded string.
        /// </summary>
        /// <param name="encodedValue">The base64 encoded string to decode.</param>
        /// <returns>A <see cref="Stream"/> the represents the decoded result of the base64 encoded value.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="encodedValue"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="encodedValue"/> is an empty string.</exception>
        public static Stream DecodeBase64String(string encodedValue)
        {
            MemoryStream stream = null;

            Guard.ArgumentNotNullOrEmptyString(encodedValue, "encodedValue");

            byte[] data = Convert.FromBase64String(encodedValue);
            stream      = new MemoryStream(data);

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }

        /// <summary>
        /// Decodes an HTML escaped string.
        /// </summary>
        /// <param name="escapedValue">The HTML escaped string to decode.</param>
        /// <returns>A string the represents the unescaped result of the HTML escaped value.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="escapedValue"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="escapedValue"/> is an empty string.</exception>
        public static string DecodeHtmlEscapedString(string escapedValue)
        {
            string decodedResult = String.Empty;

            Guard.ArgumentNotNullOrEmptyString(escapedValue, "escapedValue");

            decodedResult   = System.Web.HttpUtility.HtmlDecode(escapedValue);
            decodedResult   = System.Web.HttpUtility.UrlDecode(decodedResult);

            return decodedResult;
        }

        /// <summary>
        /// Encodes the supplied string so that it can be safely represented in XML.
        /// </summary>
        /// <param name="content">A string that represents the XML data to parse for invalid XML hexadecimal characters.</param>
        /// <returns>A string that has been encoded to be safe for XML.</returns>
        /// <remarks>
        ///     <para>The encoding process replaces invalid XML hexadecimal characters with their equivalent decimal representation.</para>
        ///     <para>
        ///         Hexadecimal characters that are valid include: #x9, #xA, #xD, [#x20-#xD7FF], [#xE000-#xFFFD], [#x10000-#x10FFFF],
        ///         and any Unicode character; excluding the surrogate blocks FFFE and FFFF.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        public static string EncodeInvalidXmlHexadecimalCharacters(string content)
        {
            Regex invalidXmlUnicodeCharacters   = new Regex(@"[\x01-\x08\x0B-\x0C\x0E-\x1F\xD800-\xDFFF\xFFFE-\xFFFF]");
            string encodedContent               = String.Empty;

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            encodedContent  = content;

            MatchCollection matches = invalidXmlUnicodeCharacters.Matches(encodedContent);
            foreach (Match match in matches)
            {
                encodedContent  = encodedContent.Replace(match.Value, Convert.ToUInt32(match.Value, 16).ToString(NumberFormatInfo.InvariantInfo));
            }

            return encodedContent;
        }

        /// <summary>
        /// Extracts the character encoding for the content type of the supplied <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The HTTP values sent by a client during a Web request.</param>
        /// <returns>
        ///     A <see cref="Encoding"/> that represents character encoding of the Content-Type <i>charset</i> attribute.
        ///     If the <i>charset</i> attribute is unavailable or invalid, returns <b>null</b>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> is a null reference (Nothing in Visual Basic).</exception>
        /*public static Encoding GetCharacterEncoding(HttpRequest request)
        {
            Encoding contentEncoding    = null;

            Guard.ArgumentNotNull(request, "request");

            if (!String.IsNullOrEmpty(request.ContentType))
            {
                if (request.ContentType.Contains(";"))
                {
                    string[] contentTypeParts   = request.ContentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (contentTypeParts != null && contentTypeParts.Length > 0)
                    {
                        for (int i = 0; i < contentTypeParts.Length; i++)
                        {
                            string typePart = contentTypeParts[i].Trim();
                            if (typePart.Contains("="))
                            {
                                string[] nameValuePair  = typePart.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                if (nameValuePair != null && nameValuePair.Length == 2)
                                {
                                    string name     = nameValuePair[0].Trim();
                                    string value    = nameValuePair[1].Trim();

                                    if (String.Compare(name, "charset", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        try
                                        {
                                            contentEncoding = Encoding.GetEncoding(value);
                                        }
                                        catch (ArgumentException)
                                        {
                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return contentEncoding;
        }*/

        /// <summary>
        /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied array of bytes.
        /// </summary>
        /// <param name="data">An array of bytes that represents an XML data source to determine the character encoding for.</param>
        /// <returns>
        ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data source.
        ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Encoding GetXmlEncoding(byte[] data)
        {
            Guard.ArgumentNotNull(data, "data");

            using (MemoryStream stream = new MemoryStream(data))
            {
                return SyndicationEncodingUtility.GetXmlEncoding(stream);
            }
        }

        /// <summary>
        /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents an XML data source to determine the character encoding for.</param>
        /// <returns>
        ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data source.
        ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Encoding GetXmlEncoding(Stream stream)
        {
            Guard.ArgumentNotNull(stream, "stream");

            using (StreamReader reader = new StreamReader(stream))
            {
                return SyndicationEncodingUtility.GetXmlEncoding(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied content.
        /// </summary>
        /// <param name="content">A string that represents the XML data to determine the character encoding for.</param>
        /// <returns>
        ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data.
        ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        public static Encoding GetXmlEncoding(string content)
        {
            Encoding encoding       = Encoding.UTF8;
            string encodingPattern  = @"^<\?xml.+?encoding\s*=\s*(?:""(?<webName>[^""]*)""|(?<webName>\S+)).*?\?>";

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            Match encodingMatch = Regex.Match(content, encodingPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (encodingMatch != null && encodingMatch.Groups.Count > 0)
            {
                Group group = encodingMatch.Groups["webName"];
                if (group != null)
                {
                    try
                    {
                        encoding    = Encoding.GetEncoding(group.Value);
                    }
                    catch (ArgumentException)
                    {
                        encoding    = Encoding.UTF8;
                    }
                }
            }

            return encoding;
        }

        /// <summary>
        /// Sanitizes the supplied string so that it can be safely represented in XML.
        /// </summary>
        /// <param name="content">A string that represents the XML data to parse for invalid XML hexadecimal characters.</param>
        /// <returns>A string that has been sanitized to be safe for XML.</returns>
        /// <remarks>
        ///     <para>The sanitation process removes characters that are invalid for XML encoding.</para>
        ///     <para>
        ///         Hexadecimal characters that are valid include: #x9, #xA, #xD, [#x20-#xD7FF], [#xE000-#xFFFD], [#x10000-#x10FFFF],
        ///         and any Unicode character; excluding the surrogate blocks FFFE and FFFF.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        public static string RemoveInvalidXmlHexadecimalCharacters(string content)
        {
            Regex invalidXmlHexadecimalCharacters = new Regex(@"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]");

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            return invalidXmlHexadecimalCharacters.Replace(content, String.Empty);
        }

        /// <summary>
        /// Converts a string into a value that can be safely used as a <see cref="Directory">directory</see> name.
        /// </summary>
        /// <param name="name">The directory name to encode.</param>
        /// <returns>A string that can be safely used as an argument when <see cref="Directory.CreateDirectory(string)">creating a directory</see>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static string EncodeSafeDirectoryName(string name)
        {
            string directoryName    = String.Empty;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            directoryName   = name.Replace("\\", String.Empty);
            directoryName   = directoryName.Replace("/", String.Empty);
            directoryName   = directoryName.Replace(":", String.Empty);
            directoryName   = directoryName.Replace("*", String.Empty);
            directoryName   = directoryName.Replace("?", String.Empty);
            directoryName   = directoryName.Replace("<", String.Empty);
            directoryName   = directoryName.Replace(">", String.Empty);
            directoryName   = directoryName.Replace("|", String.Empty);

            return directoryName;
        }

        /// <summary>
        /// Gets an array of bytes that represent the data of the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get an array of bytes for.</param>
        /// <returns>An array of bytes that represent the data of the supplied <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        private static byte[] GetStreamBytes(Stream stream)
        {
            int initialLength   = 32768;
            int read            = 0;
            int chunk;

            Guard.ArgumentNotNull(stream, "stream");

            byte[] buffer   = new byte[initialLength];

            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte    = stream.ReadByte();

                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    byte[] newBuffer    = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read]     = (byte)nextByte;
                    buffer              = newBuffer;
                    read++;
                }
            }

            byte[] result  = new byte[read];
            Array.Copy(buffer, result, read);

            return result;
        }
    }
}
