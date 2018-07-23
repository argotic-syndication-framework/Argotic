namespace Argotic.Common
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Provides methods for extracting peer-to-peer auto-discovery and resource information from syndicated content. This class cannot be inherited.
    /// </summary>
    public static class SyndicationDiscoveryUtility
    {
        /// <summary>
        /// Private member to hold the default user agent sent by the framework when making HTTP web requests.
        /// </summary>
        private static string frameworkUserAgent = String.Format(null, "Argotic-Syndication-Framework/{0}", System.Reflection.Assembly.GetAssembly(typeof(SyndicationDiscoveryUtility)).GetName().Version.ToString(4));

        /// <summary>
        /// Gets the raw user agent string used by the framework when sending web requests.
        /// </summary>
        /// <value>A string that represents information such as the client application name, version, host operating system, and language.</value>
        public static string FrameworkUserAgent
        {
            get
            {
                return frameworkUserAgent;
            }
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified format name.
        /// </summary>
        /// <param name="name">The name of the syndication content format.</param>
        /// <returns>A <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified string, otherwise returns <b>SyndicationContentFormat.None</b>.</returns>
        /// <remarks>This method disregards case of specified format name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static SyndicationContentFormat SyndicationContentFormatByName(string name)
        {
            SyndicationContentFormat syndicationFormat = SyndicationContentFormat.None;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
            {
                if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                {
                    SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            syndicationFormat = format;
                            break;
                        }
                    }
                }
            }

            return syndicationFormat;
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource located at the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> of the syndication resource to determine syndication content format for.</param>
        /// <returns>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
        ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the SyndicationContentFormatGet method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="SyndicationContentFormatGet(Uri source)"
        ///         />
        ///     </code>
        /// </example>
        public static SyndicationContentFormat SyndicationContentFormatGet(Uri source)
        {
            return SyndicationDiscoveryUtility.SyndicationContentFormatGet(source, null);
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource located at the specified <see cref="Uri"/> using the supplied <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> of the syndication resource to determine syndication content format for.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
        ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static SyndicationContentFormat SyndicationContentFormatGet(Uri source, ICredentials credentials)
        {
            Guard.ArgumentNotNull(source, "source");

            using (WebResponse response = SyndicationEncodingUtility.CreateWebResponse(source, new WebRequestOptions(credentials)))
            {
                if (response != null)
                {
                    return SyndicationDiscoveryUtility.SyndicationContentFormatGet(response.GetResponseStream());
                }
                else
                {
                    return SyndicationContentFormat.None;
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents the XML data for the syndicated resource.</param>
        /// <returns>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
        ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        public static SyndicationContentFormat SyndicationContentFormatGet(Stream stream)
        {
            Guard.ArgumentNotNull(stream, "stream");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                return SyndicationDiscoveryUtility.SyndicationContentFormatGet(reader);
            }
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> that represents the XML data for the syndicated resource.</param>
        /// <returns>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
        ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public static SyndicationContentFormat SyndicationContentFormatGet(XmlReader reader)
        {
            SyndicationContentFormat syndicationFormat = SyndicationContentFormat.None;

            Guard.ArgumentNotNull(reader, "reader");

            XmlDocument document = new XmlDocument();
            document.Load(reader);

            string rootElementName = document.DocumentElement.LocalName;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
            {
                if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                {
                    SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(rootElementName, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            syndicationFormat = format;
                            break;
                        }
                    }
                }
            }

            return syndicationFormat;
        }

        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="navigator">A <see cref="XPathNavigator"/> that represents the XML data for the syndicated resource.</param>
        /// <returns>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
        ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        public static SyndicationContentFormat SyndicationContentFormatGet(XPathNavigator navigator)
        {
            SyndicationContentFormat syndicationFormat = SyndicationContentFormat.None;
            XPathNavigator source = null;

            Guard.ArgumentNotNull(navigator, "navigator");

            source = navigator.CreateNavigator();
            if (String.IsNullOrEmpty(source.LocalName))
            {
                source.MoveToRoot();
                source.MoveToChild(XPathNodeType.Element);
            }

            string rootElementName = source.LocalName;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
            {
                if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                {
                    SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(rootElementName, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            syndicationFormat = format;
                            break;
                        }
                    }
                }
            }

            return syndicationFormat;
        }

        /// <summary>
        /// Returns a <see cref="Hashtable"/> of the HTML attribute name/value pairs for the supplied content.
        /// </summary>
        /// <param name="content">The HTML content to parse.</param>
        /// <returns>A <see cref="Hashtable"/> of the HTML attribute name/value pairs extracted the supplied <paramref name="content"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        private static Hashtable ExtractHtmlAttributes(string content)
        {
            Hashtable hashtable = new Hashtable();
            Regex attributePattern = new Regex("([a-zA-Z]+)=[\"']([^\"']+)[\"']|([a-zA-Z]+)=([^\"'>\r\n\t ]+)", RegexOptions.IgnoreCase);

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            MatchCollection attributes = attributePattern.Matches(content);

            foreach (Match attribute in attributes)
            {
                if (attribute.Groups != null && attribute.Groups.Count > 0)
                {
                    string name = attribute.Groups[1].Value;
                    string value = String.Empty;

                    if (!String.IsNullOrEmpty(name))
                    {
                        value = attribute.Groups[2].Value;
                    }
                    else
                    {
                        name = attribute.Groups[3].Value;
                        value = attribute.Groups[4].Value;
                    }

                    name = name.ToUpperInvariant().Trim();
                    value = value.Trim();

                    if (!hashtable.ContainsKey(name))
                    {
                        hashtable.Add(name, value);
                    }
                }
            }

            return hashtable;
        }

        /// <summary>
        /// Returns a collection of <see cref="Uri"/> instances that represent HTML header links and/or anchor tags in the supplied HTML markup.
        /// </summary>
        /// <param name="content">The HTML markup to parse.</param>
        /// <returns>A collection of <see cref="Uri"/> instances that represent HTML anchor elements and header links in the supplied HTML markup.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        public static Collection<Uri> ExtractUrls(string content)
        {
            Collection<Uri> results = new Collection<Uri>();
            Regex linkPattern = new Regex("<link[^>]+", RegexOptions.IgnoreCase);
            Regex anchorPattern = new Regex("<a[^>]+", RegexOptions.IgnoreCase);

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            MatchCollection links = linkPattern.Matches(content);

            foreach (Match link in links)
            {
                Hashtable linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

                if (linkAttributes.ContainsKey("HREF"))
                {
                    Uri uri;
                    if (Uri.TryCreate((string)linkAttributes["HREF"], UriKind.RelativeOrAbsolute, out uri))
                    {
                        results.Add(uri);
                    }
                }
            }

            MatchCollection anchors = anchorPattern.Matches(content);

            foreach (Match anchor in anchors)
            {
                Hashtable anchorAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(anchor.Value);

                if (anchorAttributes.ContainsKey("HREF"))
                {
                    Uri uri;
                    if (Uri.TryCreate((string)anchorAttributes["HREF"], UriKind.RelativeOrAbsolute, out uri))
                    {
                        results.Add(uri);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns a <see cref="Uri"/> that represents the absolute base URI of the supplied <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to generate an absolute base <see cref="Uri"/> for.</param>
        /// <returns>
        ///     A <see cref="Uri"/> that represents the absolute base URI of the supplied <see cref="HttpRequest"/>.
        ///     If unable to build an absolute base <see cref="Uri"/>, returns the absolute URI of the supplied <see cref="HttpRequest"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> is a null reference (Nothing in Visual Basic).</exception>
        /*public static Uri GetAbsoluteBaseUri(HttpRequest xrequest)
        {
            Uri baseUri = null;

            Guard.ArgumentNotNull(request, "request");

            string baseUrlString    = request.Url.AbsoluteUri.Replace(request.Url.PathAndQuery, String.Empty);
            baseUrlString           = String.Concat(baseUrlString, "/", request.ApplicationPath.TrimStart('/'));

            if (!Uri.TryCreate(baseUrlString, UriKind.Absolute, out baseUri))
            {
                baseUri             = new Uri(request.Url.AbsoluteUri);
            }

            return baseUri;
        }*/

        /// <summary>
        /// Returns a value indicating if the source <see cref="Uri"/> references the target <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the source web resource that will be searched.</param>
        /// <param name="target">A <see cref="Uri"/> that represents the target web resource being searched for.</param>
        /// <returns><b>true</b> if the <paramref name="source"/> contains at least one link to the <paramref name="target"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the SourceReferencesTarget method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="SourceReferencesTarget(Uri source, Uri target)"
        ///         />
        ///     </code>
        /// </example>
        public static bool SourceReferencesTarget(Uri source, Uri target)
        {
            return SyndicationDiscoveryUtility.SourceReferencesTarget(source, target, null);
        }

        /// <summary>
        /// Returns a value indicating if the source <see cref="Uri"/> references the target <see cref="Uri"/>, using the specifed <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the source web resource that will be searched.</param>
        /// <param name="target">A <see cref="Uri"/> that represents the target web resource being searched for.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="source"/> contains at least one link to the <paramref name="target"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool SourceReferencesTarget(Uri source, Uri target, ICredentials credentials)
        {
            bool sourceContainsLinkToTarget = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            using (WebResponse response = SyndicationEncodingUtility.CreateWebResponse(source, new WebRequestOptions(credentials)))
            {
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            Collection<Uri> links = SyndicationDiscoveryUtility.ExtractUrls(reader.ReadToEnd());

                            if (links != null && links.Count > 0)
                            {
                                foreach (Uri link in links)
                                {
                                    if (Uri.Compare(link, target, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        sourceContainsLinkToTarget = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return sourceContainsLinkToTarget;
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> exists.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <returns><b>true</b> if the <paramref name="uri"/> exists, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method will return <b>false</b> if the <paramref name="uri"/> is a null reference or the <paramref name="uri"/> is otherwise inaccessible.
        /// </remarks>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the UriExists method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="UriExists(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        public static bool UriExists(Uri uri)
        {
            return SyndicationDiscoveryUtility.UriExists(uri, null);
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> exists using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="uri"/> exists, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method will return <b>false</b> if the <paramref name="uri"/> is a null reference or the <paramref name="uri"/> is otherwise inaccessible.
        /// </remarks>
        public static bool UriExists(Uri uri, ICredentials credentials)
        {
            bool uriExists = false;

            if (uri == null)
            {
                return false;
            }

            try
            {
                using (WebResponse response = SyndicationEncodingUtility.CreateWebResponse(uri, new WebRequestOptions(credentials)))
                {
                    if (response != null && response.ContentLength > 0)
                    {
                        uriExists = true;
                    }
                }
            }
            catch (WebException)
            {
                uriExists = false;
            }

            return uriExists;
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/> and entity tag.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <returns>A <see cref="HttpWebResponse"/> for the <paramref name="source"/> if it has been modfied, otherwise <b>null</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the ConditionalGet method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="ConditionalGet(Uri source, DateTime lastModified, string entityTag)"
        ///         />
        ///     </code>
        /// </example>
        public static HttpWebResponse ConditionalGet(Uri source, DateTime lastModified, string entityTag)
        {
            return SyndicationDiscoveryUtility.ConditionalGet(source, lastModified, entityTag, new WebRequestOptions());
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns>A <see cref="HttpWebResponse"/> for the <paramref name="source"/> if it has been modfied, otherwise <b>null</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static HttpWebResponse ConditionalGet(Uri source, DateTime lastModified, string entityTag, ICredentials credentials)
        {
            HttpWebResponse response = null;

            if (SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, credentials, out response))
            {
                return response;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <returns>A <see cref="HttpWebResponse"/> for the <paramref name="source"/> if it has been modfied, otherwise <b>null</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static HttpWebResponse ConditionalGet(Uri source, DateTime lastModified, string entityTag, ICredentials credentials, IWebProxy proxy)
        {
            return SyndicationDiscoveryUtility.ConditionalGet(source, lastModified, entityTag, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>A <see cref="HttpWebResponse"/> for the <paramref name="source"/> if it has been modfied, otherwise <b>null</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static HttpWebResponse ConditionalGet(Uri source, DateTime lastModified, string entityTag, WebRequestOptions options)
        {
            HttpWebResponse response = null;

            if (SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, options, out response))
            {
                return response;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/> and entity tag.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="response">
        ///     When this method returns, contains the <see cref="HttpWebResponse"/> for the supplied <paramref name="source"/>, if the web resource has been modified, or <b>null</b> if the web resource has <u>not</u> been modified.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="source"/> has been modified, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the TryConditionalGet method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="TryConditionalGet(Uri source, DateTime lastModified, string entityTag, out WebResponse response)"
        ///         />
        ///     </code>
        /// </example>
        public static bool TryConditionalGet(Uri source, DateTime lastModified, string entityTag, out HttpWebResponse response)
        {
            return SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, new WebRequestOptions(), out response);
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <param name="response">
        ///     When this method returns, contains the <see cref="HttpWebResponse"/> for the supplied <paramref name="source"/>, if the web resource has been modified, or <b>null</b> if the web resource has <u>not</u> been modified.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="source"/> has been modified, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool TryConditionalGet(Uri source, DateTime lastModified, string entityTag, ICredentials credentials, out HttpWebResponse response)
        {
            bool sourceHasBeenModified = false;

            Guard.ArgumentNotNull(source, "source");

            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(source);
            httpRequest.UserAgent = frameworkUserAgent;
            httpRequest.IfModifiedSince = lastModified;
            httpRequest.Headers.Add(HttpRequestHeader.IfNoneMatch, entityTag);
            new WebRequestOptions(credentials).ApplyOptions(httpRequest);

            try
            {
                response = (HttpWebResponse)httpRequest.GetResponse();

                if (DateTime.Compare(response.LastModified, lastModified) != 0)
                {
                    sourceHasBeenModified = true;
                }
            }
            catch (WebException webException)
            {
                if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotModified)
                {
                    sourceHasBeenModified = false;
                    response = null;
                }
                else
                {
                    throw;
                }
            }

            return sourceHasBeenModified;
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="response">
        ///     When this method returns, contains the <see cref="HttpWebResponse"/> for the supplied <paramref name="source"/>, if the web resource has been modified, or <b>null</b> if the web resource has <u>not</u> been modified.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="source"/> has been modified, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool TryConditionalGet(Uri source, DateTime lastModified, string entityTag, ICredentials credentials, IWebProxy proxy, out HttpWebResponse response)
        {
            return SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, new WebRequestOptions(credentials, proxy), out response);
        }

        /// <summary>
        /// Performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag and <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
        /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
        /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="response">
        ///     When this method returns, contains the <see cref="HttpWebResponse"/> for the supplied <paramref name="source"/>, if the web resource has been modified, or <b>null</b> if the web resource has <u>not</u> been modified.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="source"/> has been modified, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool TryConditionalGet(Uri source, DateTime lastModified, string entityTag, WebRequestOptions options, out HttpWebResponse response)
        {
            bool sourceHasBeenModified = false;

            Guard.ArgumentNotNull(source, "source");

            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(source);
            httpRequest.UserAgent = frameworkUserAgent;
            httpRequest.IfModifiedSince = lastModified;
            httpRequest.Headers.Add(HttpRequestHeader.IfNoneMatch, entityTag);
            if (options != null) options.ApplyOptions(httpRequest);

            try
            {
                response = (HttpWebResponse)httpRequest.GetResponse();

                if (DateTime.Compare(response.LastModified, lastModified) != 0)
                {
                    sourceHasBeenModified = true;
                }
            }
            catch (WebException webException)
            {
                if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotModified)
                {
                    sourceHasBeenModified = false;
                    response = null;
                }
                else
                {
                    throw;
                }
            }

            return sourceHasBeenModified;
        }

        /// <summary>
        /// Extracts auto-discoverable syndication endpoints from the supplied HTML markup.
        /// </summary>
        /// <param name="content">The HTML markup to parse.</param>
        /// <returns>
        ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints contained within the <paramref name="content"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
        ///     further information about the auto-discovery of syndicated content.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        public static Collection<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(string content)
        {
            Collection<DiscoverableSyndicationEndpoint> results = new Collection<DiscoverableSyndicationEndpoint>();
            Regex linkPattern = new Regex("<link[^>]+", RegexOptions.IgnoreCase);

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            MatchCollection links = linkPattern.Matches(content);

            foreach (Match link in links)
            {
                Hashtable linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

                if (linkAttributes.ContainsKey("HREF") && linkAttributes.ContainsKey("REL") && linkAttributes.ContainsKey("TYPE"))
                {
                    string href = (string)linkAttributes["HREF"];
                    string rel = (string)linkAttributes["REL"];
                    string type = (string)linkAttributes["TYPE"];

                    if (String.Compare(rel, "alternate", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Uri url;
                        if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out url))
                        {
                            DiscoverableSyndicationEndpoint endpoint = new DiscoverableSyndicationEndpoint();
                            endpoint.Source = url;
                            if (!String.IsNullOrEmpty(type))
                            {
                                endpoint.ContentType = type;
                            }

                            if (linkAttributes.ContainsKey("TITLE"))
                            {
                                string title = (string)linkAttributes["TITLE"];
                                if (!String.IsNullOrEmpty(title))
                                {
                                    endpoint.Title = title;
                                }
                            }

                            results.Add(endpoint);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Extracts auto-discoverable syndication endpoints from the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents the HTML markup to parse.</param>
        /// <returns>
        ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints contained within the <paramref name="stream"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
        ///     further information about the auto-discovery of syndicated content.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Collection<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(Stream stream)
        {
            Guard.ArgumentNotNull(stream, "stream");

            using (StreamReader reader = new StreamReader(stream))
            {
                return SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> that represents the URL of the  web resource to parse.</param>
        /// <returns>
        ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the web resource located at the <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
        ///     further information about the auto-discovery of syndicated content.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the LocateDiscoverableSyndicationEndpoints method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="LocateDiscoverableSyndicationEndpoints(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        public static Collection<DiscoverableSyndicationEndpoint> LocateDiscoverableSyndicationEndpoints(Uri uri)
        {
            return SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(uri, null);
        }

        /// <summary>
        /// Returns a collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the supplied <see cref="Uri"/>
        /// using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the web resource located at the <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
        ///     further information about the auto-discovery of syndicated content.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        public static Collection<DiscoverableSyndicationEndpoint> LocateDiscoverableSyndicationEndpoints(Uri uri, ICredentials credentials)
        {
            Guard.ArgumentNotNull(uri, "uri");

            using (WebResponse webResponse = SyndicationEncodingUtility.CreateWebResponse(uri, new WebRequestOptions(credentials)))
            {
                if (webResponse == null)
                {
                    return new Collection<DiscoverableSyndicationEndpoint>();
                }

                using (Stream stream = webResponse.GetResponseStream())
                {
                    return SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(stream);
                }
            }
        }

        /// <summary>
        /// Extracts a <see cref="HtmlAnchor"/> that represents a pingback auto-discovery link from the supplied HTML markup.
        /// </summary>
        /// <param name="content">The HTML markup to parse.</param>
        /// <returns>
        ///     A <see cref="HtmlAnchor"/> that represents the pingback auto-discovery link extracted from the <paramref name="html"/>.
        ///     If no pingback auto-discovery link was found, returns <b>null</b>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         Pingback enabled resources that utilize the link mechanism will contain a
        ///         &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
        ///     </para>
        ///     <para>
        ///         The <see cref="HtmlAnchor"/> that is returned will have an <i>Href</i> property that points to the
        ///         absolute URI of the pingback XML-RPC server, and a <i>rel</i> attribute of pingback.
        ///         The <i>Title</i> property and <i>type</i> attribute will also be extracted if available.
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
        ///         for more information about the pingback notification mechanism.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static HtmlAnchor ExtractPingbackNotificationServer(string content)
        {
            HtmlAnchor pingbackAnchor = null;
            Regex linkPattern = new Regex("<link[^>]+", RegexOptions.IgnoreCase);

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            MatchCollection links = linkPattern.Matches(content);

            foreach (Match link in links)
            {
                Hashtable linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

                if (linkAttributes.ContainsKey("HREF") && linkAttributes.ContainsKey("REL"))
                {
                    string href = (string)linkAttributes["HREF"];
                    string rel = (string)linkAttributes["REL"];

                    if (String.Compare(rel, "pingback", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Uri uri;
                        if (Uri.TryCreate(href, UriKind.Absolute, out uri))
                        {
                            pingbackAnchor = new HtmlAnchor();
                            pingbackAnchor.HRef = href;
                            pingbackAnchor.Attributes.Add("rel", rel);

                            if (linkAttributes.ContainsKey("TYPE"))
                            {
                                string type = (string)linkAttributes["TYPE"];
                                if (!String.IsNullOrEmpty(type))
                                {
                                    pingbackAnchor.Attributes.Add("type", type);
                                }
                            }
                            if (linkAttributes.ContainsKey("TITLE"))
                            {
                                string title = (string)linkAttributes["TITLE"];
                                if (!String.IsNullOrEmpty(title))
                                {
                                    pingbackAnchor.Title = title;
                                }
                            }
                        }
                    }
                }
            }

            return pingbackAnchor;
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> is a pingback enabled web resource.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <returns><b>true</b> if the <paramref name="uri"/> is pingback enabled, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>
        ///         There are two mechanisms used when determining if a web resource is pingback enabled;
        ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
        ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
        ///         either or both of these mechanisms.
        ///     </para>
        ///     <para>
        ///         To conform to the Pingback 1.0 specification, the following information should apply to a pingback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the link mechanism will contain a
        ///                     &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the HTTP header mechanism will contain an
        ///                     HTTP header named <i>X-Pingback</i> whose value is the absolute URI of the pingback XML-RPC server.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         This method is optimized to determine pingback enablement by examining the HTTP headers
        ///         before attempting to parse the response data for an pingback XML-RPC server link.
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
        ///         for more information about the pingback notification mechanism.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the IsPingbackEnabled method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="IsPingbackEnabled(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static bool IsPingbackEnabled(Uri uri)
        {
            return SyndicationDiscoveryUtility.IsPingbackEnabled(uri, null);
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> is a pingback enabled web resource using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="uri"/> is pingback enabled, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>
        ///         There are two mechanisms used when determining if a web resource is pingback enabled;
        ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
        ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
        ///         either or both of these mechanisms.
        ///     </para>
        ///     <para>
        ///         To conform to the Pingback 1.0 specification, the following information should apply to a pingback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the link mechanism will contain a
        ///                     &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the HTTP header mechanism will contain an
        ///                     HTTP header named <i>X-Pingback</i> whose value is the absolute URI of the pingback XML-RPC server.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         This method is optimized to determine pingback enablement by examining the HTTP headers
        ///         before attempting to parse the response data for an pingback XML-RPC server link.
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
        ///         for more information about the pingback notification mechanism.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static bool IsPingbackEnabled(Uri uri, ICredentials credentials)
        {
            bool isPingbackEnabled = false;

            Guard.ArgumentNotNull(uri, "uri");

            using (WebResponse webResponse = SyndicationEncodingUtility.CreateWebResponse(uri, new WebRequestOptions(credentials)))
            {
                if (webResponse == null)
                {
                    return false;
                }

                if (webResponse.Headers != null && webResponse.Headers.Count > 0)
                {
                    for (int i = 0; i < webResponse.Headers.Count; i++)
                    {
                        string name = webResponse.Headers.Keys[i];
                        string value = webResponse.Headers[i];

                        if (String.Compare(name, "X-Pingback", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Uri pingbackXmlRpcServer;
                            if (Uri.TryCreate(value, UriKind.Absolute, out pingbackXmlRpcServer))
                            {
                                isPingbackEnabled = true;
                            }
                            break;
                        }
                    }
                }

                if (!isPingbackEnabled)
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        HtmlAnchor link = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(reader.ReadToEnd());

                        if (link != null)
                        {
                            isPingbackEnabled = true;
                        }
                    }
                }
            }

            return isPingbackEnabled;
        }

        /// <summary>
        /// Returns a <see cref="Uri"/> that represents a pingback XML-RPC server endpoint using the Pingback server auto-discovery mechanisms for the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of a web resource to perform pingback auto-discovery against.</param>
        /// <returns>
        ///     A <see cref="Uri"/> that represents the absolute URI of the pingback XML-RPC server.
        ///     If pingback server auto-discovery fails to locate a pingback XML-RPC server endpoint, returns <b>null</b>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         There are two mechanisms used when determining if a web resource is pingback enabled;
        ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
        ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
        ///         either or both of these mechanisms.
        ///     </para>
        ///     <para>
        ///         To conform to the Pingback 1.0 specification, the following information should apply to a pingback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the link mechanism will contain a
        ///                     &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the HTTP header mechanism will contain an
        ///                     HTTP header named <i>X-Pingback</i> whose value is the absolute URI of the pingback XML-RPC server.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         This method is optimized to locate the pingback XML-RPC server endpoint within the HTTP headers
        ///         before attempting to parse the response data for an pingback XML-RPC server link.
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
        ///         for more information about the pingback notification mechanism.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the LocatePingbackNotificationServer method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="LocatePingbackNotificationServer(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static Uri LocatePingbackNotificationServer(Uri uri)
        {
            return SyndicationDiscoveryUtility.LocatePingbackNotificationServer(uri, null);
        }

        /// <summary>
        /// Returns a <see cref="Uri"/> that represents a pingback XML-RPC server endpoint using the Pingback server auto-discovery mechanisms for the supplied <see cref="Uri"/>
        /// using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of a web resource to perform pingback auto-discovery against.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns>
        ///     A <see cref="Uri"/> that represents the absolute URI of the pingback XML-RPC server.
        ///     If pingback server auto-discovery fails to locate a pingback XML-RPC server endpoint, returns <b>null</b>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         There are two mechanisms used when determining if a web resource is pingback enabled;
        ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
        ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
        ///         either or both of these mechanisms.
        ///     </para>
        ///     <para>
        ///         To conform to the Pingback 1.0 specification, the following information should apply to a pingback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the link mechanism will contain a
        ///                     &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Pingback enabled resources that utilize the HTTP header mechanism will contain an
        ///                     HTTP header named <i>X-Pingback</i> whose value is the absolute URI of the pingback XML-RPC server.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         This method is optimized to locate the pingback XML-RPC server endpoint within the HTTP headers
        ///         before attempting to parse the response data for an pingback XML-RPC server link.
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
        ///         for more information about the pingback notification mechanism.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static Uri LocatePingbackNotificationServer(Uri uri, ICredentials credentials)
        {
            Uri pingbackXmlRpcServer = null;

            Guard.ArgumentNotNull(uri, "uri");

            using (WebResponse webResponse = SyndicationEncodingUtility.CreateWebResponse(uri, new WebRequestOptions(credentials)))
            {
                if (webResponse == null)
                {
                    return null;
                }

                if (webResponse.Headers != null && webResponse.Headers.Count > 0)
                {
                    for (int i = 0; i < webResponse.Headers.Count; i++)
                    {
                        string name = webResponse.Headers.Keys[i];
                        string value = webResponse.Headers[i];

                        if (String.Compare(name, "X-Pingback", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Uri url;
                            if (Uri.TryCreate(value, UriKind.Absolute, out url))
                            {
                                pingbackXmlRpcServer = url;
                            }
                            break;
                        }
                    }
                }

                if (pingbackXmlRpcServer == null)
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        HtmlAnchor link = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(reader.ReadToEnd());

                        if (link != null)
                        {
                            Uri href;
                            if (Uri.TryCreate(link.HRef, UriKind.Absolute, out href))
                            {
                                pingbackXmlRpcServer = href;
                            }
                        }
                    }
                }
            }

            return pingbackXmlRpcServer;
        }

        /// <summary>
        /// Extracts embedded RDF Trackback discovery meta-data from the supplied HTML markup.
        /// </summary>
        /// <param name="content">The HTML markup to parse.</param>
        /// <returns>
        ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs contained within the <paramref name="content"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///     further information about the auto-discovery of Trackback ping URLs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static Collection<TrackbackDiscoveryMetadata> ExtractTrackbackNotificationServers(string content)
        {
            Collection<TrackbackDiscoveryMetadata> results = new Collection<TrackbackDiscoveryMetadata>();
            Regex rdfPattern = new Regex("<rdf:RDF\b[^>]*>(.*?)</rdf:RDF>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());

            Guard.ArgumentNotNullOrEmptyString(content, "content");

            manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            manager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            manager.AddNamespace("trackback", "http://madskills.com/public/xml/rss/module/trackback/");

            MatchCollection embeddedRdfs = rdfPattern.Matches(content);

            foreach (Match embeddedRdf in embeddedRdfs)
            {
                using (StringReader reader = new StringReader(embeddedRdf.Value))
                {
                    XPathDocument document = new XPathDocument(reader);
                    XPathNavigator navigator = document.CreateNavigator();

                    TrackbackDiscoveryMetadata trackbackMetadata = new TrackbackDiscoveryMetadata();
                    if (trackbackMetadata.Load(navigator))
                    {
                        results.Add(trackbackMetadata);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Extracts embedded RDF Trackback discovery meta-data from the supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents the HTML markup to parse.</param>
        /// <returns>
        ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs contained within the <paramref name="stream"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///     further information about the auto-discovery of Trackback ping URLs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static Collection<TrackbackDiscoveryMetadata> ExtractTrackbackNotificationServers(Stream stream)
        {
            Guard.ArgumentNotNull(stream, "stream");

            using (StreamReader reader = new StreamReader(stream))
            {
                return SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> is a trackback enabled web resource.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <returns><b>true</b> if the <paramref name="uri"/> is trackback enabled, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>
        ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
        ///     </para>
        ///     <para>
        ///         To conform to the Trackback 1.2 specification, the following information should apply to a trackback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Trackback enabled resources will contain one or more embedded RDF elements that describe where to send pings for web log entries.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     A sample embedded RDF element looks like this:
        ///                     <para>
        ///                         &lt;rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/"&gt;
        ///                             &lt;rdf:Description
        ///                                 rdf:about="http://www.foo.com/archive.html#foo"
        ///                                 dc:identifier="http://www.foo.com/archive.html#foo"
        ///                                 dc:title="Foo Bar"
        ///                                 trackback:ping="http://www.foo.com/tb.cgi/5"
        ///                             /&gt;
        ///                         &lt;/rdf:RDF&gt;
        ///                     </para>
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///         further information about the auto-discovery of Trackback ping URLs.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the IsTrackbackEnabled method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="IsTrackbackEnabled(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static bool IsTrackbackEnabled(Uri uri)
        {
            return SyndicationDiscoveryUtility.IsTrackbackEnabled(uri, null);
        }

        /// <summary>
        /// Returns a value indicating if the supplied <see cref="Uri"/> is a trackback enabled web resource using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to validate.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="uri"/> is trackback enabled, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>
        ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
        ///     </para>
        ///     <para>
        ///         To conform to the Trackback 1.2 specification, the following information should apply to a trackback enabled resource:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Trackback enabled resources will contain one or more embedded RDF elements that describe where to send pings for web log entries.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     A sample embedded RDF element looks like this:
        ///                     <para>
        ///                         &lt;rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/"&gt;
        ///                             &lt;rdf:Description
        ///                                 rdf:about="http://www.foo.com/archive.html#foo"
        ///                                 dc:identifier="http://www.foo.com/archive.html#foo"
        ///                                 dc:title="Foo Bar"
        ///                                 trackback:ping="http://www.foo.com/tb.cgi/5"
        ///                             /&gt;
        ///                         &lt;/rdf:RDF&gt;
        ///                     </para>
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///         further information about the auto-discovery of Trackback ping URLs.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static bool IsTrackbackEnabled(Uri uri, ICredentials credentials)
        {
            Guard.ArgumentNotNull(uri, "uri");

            return (SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(uri, credentials).Count > 0);
        }

        /// <summary>
        /// Returns a collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent trackback ping URL endpoints for the supplied <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> that represents the URL of the  web resource to parse.</param>
        /// <returns>
        ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs for the web resource located at the <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///     further information about the auto-discovery of Trackback ping URLs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the LocateTrackbackNotificationServers method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Common\SyndicationDiscoveryUtilityExample.cs"
        ///             region="LocateTrackbackNotificationServers(Uri uri)"
        ///         />
        ///     </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static Collection<TrackbackDiscoveryMetadata> LocateTrackbackNotificationServers(Uri uri)
        {
            return SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(uri, null);
        }

        /// <summary>
        /// Returns a collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent trackback ping URL endpoints for the supplied <see cref="Uri"/>
        /// using the specified <see cref="ICredentials">credentials</see>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the web resource when required.
        ///     If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs for the web resource located at the <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
        ///     further information about the auto-discovery of Trackback ping URLs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference (Nothing in Visual Basic).</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static Collection<TrackbackDiscoveryMetadata> LocateTrackbackNotificationServers(Uri uri, ICredentials credentials)
        {
            Guard.ArgumentNotNull(uri, "uri");

            using (WebResponse webResponse = SyndicationEncodingUtility.CreateWebResponse(uri, new WebRequestOptions(credentials)))
            {
                if (webResponse == null)
                {
                    return new Collection<TrackbackDiscoveryMetadata>();
                }

                using (Stream stream = webResponse.GetResponseStream())
                {
                    return SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(stream);
                }
            }
        }
    }
}
