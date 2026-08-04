using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Provides methods that comprise common utility features shared across the Web Log Markup Language (BlogML) syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Web Log Markup Language (BlogML) syndication entities within the framework.</remarks>
internal static class BlogMLUtility
{
    /// <summary>
    /// Private member to hold the Web Log Markup Language (BlogML) 2.0 namespace identifier.
    /// </summary>
    private const string BLOGML_NAMESPACE = "http://www.blogml.com/2006/09/BlogML";

    /// <summary>
    /// Cached mapping from BlogMLApprovalStatus enum values to their string representations.
    /// </summary>
    private static readonly FrozenDictionary<BlogMLApprovalStatus, string> s_statusToString = CreateStatusToStringMap();

    /// <summary>
    /// Cached mapping from string representations to BlogMLApprovalStatus enum values.
    /// </summary>
    private static readonly FrozenDictionary<string, BlogMLApprovalStatus> s_stringToStatus = CreateStringToStatusMap();

    /// <summary>
    /// Gets the XML namespace URI for the Web Log Markup Language (BlogML) 2.0 specification.
    /// </summary>
    /// <value>The XML namespace URI for the Web Log Markup Language (BlogML) 2.0 specification.</value>
    public static string BlogMLNamespace => BLOGML_NAMESPACE;

    /// <summary>
    /// Creates the enum-to-string mapping for BlogMLApprovalStatus.
    /// </summary>
    private static FrozenDictionary<BlogMLApprovalStatus, string> CreateStatusToStringMap()
    {
        var map = new Dictionary<BlogMLApprovalStatus, string>();
        foreach (var fieldInfo in typeof(BlogMLApprovalStatus).GetFields())
        {
            if (fieldInfo.FieldType == typeof(BlogMLApprovalStatus))
            {
                var status = Enum.Parse<BlogMLApprovalStatus>(fieldInfo.Name);

                if (fieldInfo.GetCustomAttribute<EnumerationMetadataAttribute>(inherit: false) is { } enumerationMetadata)
                {
                    map[status] = enumerationMetadata.AlternateValue;
                }
            }
        }
        return map.ToFrozenDictionary();
    }

    /// <summary>
    /// Creates the string-to-enum mapping for BlogMLApprovalStatus.
    /// </summary>
    private static FrozenDictionary<string, BlogMLApprovalStatus> CreateStringToStatusMap()
    {
        var map = new Dictionary<string, BlogMLApprovalStatus>(StringComparer.OrdinalIgnoreCase);
        foreach (var fieldInfo in typeof(BlogMLApprovalStatus).GetFields())
        {
            if (fieldInfo.FieldType == typeof(BlogMLApprovalStatus))
            {
                var status = (BlogMLApprovalStatus)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                var customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                if (customAttributes is { Length: > 0 } && customAttributes[0] is EnumerationMetadataAttribute enumerationMetadata)
                {
                    map[enumerationMetadata.AlternateValue] = status;
                }
            }
        }
        return map.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the approval status identifier for the supplied <see cref="BlogMLApprovalStatus"/>.
    /// </summary>
    /// <param name="status">The <see cref="BlogMLApprovalStatus"/> to get the text construct identifier for.</param>
    /// <returns>The approval status identifier for the supplied <paramref name="status"/>, Otherwise, returns an empty string.</returns>
    public static string ApprovalStatusAsString(BlogMLApprovalStatus status) =>
        s_statusToString.GetValueOrDefault(status, string.Empty);

    /// <summary>
    /// Returns the <see cref="BlogMLApprovalStatus"/> enumeration value that corresponds to the specified approval status value.
    /// </summary>
    /// <param name="value">The value of the approval status identifier.</param>
    /// <returns>A <see cref="BlogMLApprovalStatus"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>BlogMLApprovalStatus.None</b>.</returns>
    /// <remarks>This method disregards case of specified approval status value.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public static BlogMLApprovalStatus ApprovalStatusByValue(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        return s_stringToStatus.GetValueOrDefault(value, BlogMLApprovalStatus.None);
    }

    /// <summary>
    /// Compares objects that implement the <see cref="IBlogMLCommonObject"/> interface.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IBlogMLCommonObject"/> interface to be compared.</param>
    /// <param name="target">A object that implements the <see cref="IBlogMLCommonObject"/> to compare with the <paramref name="source"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public static int CompareCommonObjects(IBlogMLCommonObject? source, IBlogMLCommonObject? target)
    {
        int result = (source, target) switch
        {
            (null, null) => 0,
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0
        };

        if (result != 0 || source is null || target is null) return result;

        result = source.ApprovalStatus.CompareTo(target.ApprovalStatus);
        if (result != 0) return result;

        result = source.CreatedOn.CompareTo(target.CreatedOn);
        if (result != 0) return result;

        result = string.Compare(source.Id, target.Id, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = source.LastModifiedOn.CompareTo(target.LastModifiedOn);
        if (result != 0) return result;

        result = (source.Title, target.Title) switch
        {
            (null, null) => 0,
            (not null, null) => 1,
            (null, not null) => -1,
            var (s, t) => s.CompareTo(t)
        };

        return result;
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within  Web Log Markup Language (BlogML) syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("blog", !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : BLOGML_NAMESPACE);

        return manager;
    }

    /// <summary>
    /// Modifies the <see cref="IBlogMLCommonObject"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IBlogMLCommonObject"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract BlogML common object information from.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public static bool FillCommonObject(IBlogMLCommonObject target, XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string idAttribute = source.GetAttribute("id", string.Empty);
            string dateCreatedAttribute = source.GetAttribute("date-created", string.Empty);
            string dateModifiedAttribute = source.GetAttribute("date-modified", string.Empty);
            string approvedAttribute = source.GetAttribute("approved", string.Empty);

            if (!string.IsNullOrEmpty(idAttribute))
            {
                target.Id = idAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(dateCreatedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out DateTime createdOn))
                {
                    target.CreatedOn = createdOn;
                    wasLoaded = true;
                }
                else if (DateTime.TryParse(dateCreatedAttribute, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out createdOn))
                {
                    target.CreatedOn = createdOn;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(dateModifiedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateModifiedAttribute, out DateTime modifiedOn))
                {
                    target.LastModifiedOn = modifiedOn;
                    wasLoaded = true;
                }
                else if (DateTime.TryParse(dateModifiedAttribute, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out modifiedOn))
                {
                    target.LastModifiedOn = modifiedOn;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(approvedAttribute))
            {
                BlogMLApprovalStatus status = BlogMLUtility.ApprovalStatusByValue(approvedAttribute);
                if (status != BlogMLApprovalStatus.None)
                {
                    target.ApprovalStatus = status;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? titleNavigator = source.SelectSingleNode("blog:title", manager);
            if (titleNavigator is not null)
            {
                BlogMLTextConstruct title = new();
                if (title.Load(titleNavigator))
                {
                    target.Title = title;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Modifies the <see cref="IBlogMLCommonObject"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IBlogMLCommonObject"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract BlogML common object information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public static bool FillCommonObject(IBlogMLCommonObject target, XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string idAttribute = source.GetAttribute("id", string.Empty);
            string dateCreatedAttribute = source.GetAttribute("date-created", string.Empty);
            string dateModifiedAttribute = source.GetAttribute("date-modified", string.Empty);
            string approvedAttribute = source.GetAttribute("approved", string.Empty);

            if (!string.IsNullOrEmpty(idAttribute))
            {
                target.Id = idAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(dateCreatedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out DateTime createdOn))
                {
                    target.CreatedOn = createdOn;
                    wasLoaded = true;
                }
                else if (DateTime.TryParse(dateCreatedAttribute, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out createdOn))
                {
                    target.CreatedOn = createdOn;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(dateModifiedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateModifiedAttribute, out DateTime modifiedOn))
                {
                    target.LastModifiedOn = modifiedOn;
                    wasLoaded = true;
                }
                else if (DateTime.TryParse(dateModifiedAttribute, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out modifiedOn))
                {
                    target.LastModifiedOn = modifiedOn;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(approvedAttribute))
            {
                BlogMLApprovalStatus status = BlogMLUtility.ApprovalStatusByValue(approvedAttribute);
                if (status != BlogMLApprovalStatus.None)
                {
                    target.ApprovalStatus = status;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? titleNavigator = source.SelectSingleNode("blog:title", manager);
            if (titleNavigator is not null)
            {
                BlogMLTextConstruct title = new();
                if (title.Load(titleNavigator, settings))
                {
                    target.Title = title;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="IBlogMLCommonObject"/> attributes to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IBlogMLCommonObject"/> interface to extract BlogML common object information from.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public static void WriteCommonObjectAttributes(IBlogMLCommonObject source, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writer);

        if (!string.IsNullOrEmpty(source.Id))
        {
            writer.WriteAttributeString("id", source.Id);
        }

        if (source.CreatedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("date-created", SyndicationDateTimeUtility.ToRfc3339DateTime(source.CreatedOn));
        }

        if (source.LastModifiedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("date-modified", SyndicationDateTimeUtility.ToRfc3339DateTime(source.LastModifiedOn));
        }

        if (source.ApprovalStatus != BlogMLApprovalStatus.None)
        {
            writer.WriteAttributeString("approved", BlogMLUtility.ApprovalStatusAsString(source.ApprovalStatus));
        }
    }

    /// <summary>
    /// Saves the current <see cref="IBlogMLCommonObject"/> elements to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IBlogMLCommonObject"/> interface to extract BlogML common object information from.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public static void WriteCommonObjectElements(IBlogMLCommonObject source, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writer);
        source.Title?.WriteTo(writer, "title");
    }
}