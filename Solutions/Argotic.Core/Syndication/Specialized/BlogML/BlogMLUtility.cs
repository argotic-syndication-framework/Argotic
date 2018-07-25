﻿namespace Argotic.Syndication.Specialized
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;

    /// <summary>
    /// Provides methods that comprise common utility features shared across the Web Log Markup Language (BlogML) syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Web Log Markup Language (BlogML) syndication entities within the framework.</remarks>
    internal static class BlogMLUtility
    {
        /// <summary>
        /// Private member to hold the Web Log Markup Language (BlogML) 2.0 namespace identifier.
        /// </summary>
        private const string InternalBlogmlNamespace = "http://www.blogml.com/2006/09/BlogML";

        /// <summary>
        /// Gets the XML namespace URI for the Web Log Markup Language (BlogML) 2.0 specification.
        /// </summary>
        /// <value>The XML namespace URI for the Web Log Markup Language (BlogML) 2.0 specification.</value>
        public static string BlogMLNamespace => InternalBlogmlNamespace;

        /// <summary>
        /// Returns the approval status identifier for the supplied <see cref="BlogMLApprovalStatus"/>.
        /// </summary>
        /// <param name="status">The <see cref="BlogMLApprovalStatus"/> to get the text construct identifier for.</param>
        /// <returns>The approval status identifier for the supplied <paramref name="status"/>, otherwise returns an empty string.</returns>
        public static string ApprovalStatusAsString(BlogMLApprovalStatus status)
        {
            string name = string.Empty;
            foreach (FieldInfo fieldInfo in typeof(BlogMLApprovalStatus).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLApprovalStatus))
                {
                    BlogMLApprovalStatus approvalStatus =
                        (BlogMLApprovalStatus)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (approvalStatus == status)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(
                            typeof(EnumerationMetadataAttribute),
                            false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata =
                                customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Returns the <see cref="BlogMLApprovalStatus"/> enumeration value that corresponds to the specified approval status value.
        /// </summary>
        /// <param name="value">The value of the approval status identifier.</param>
        /// <returns>A <see cref="BlogMLApprovalStatus"/> enumeration value that corresponds to the specified string, otherwise returns <b>BlogMLApprovalStatus.None</b>.</returns>
        /// <remarks>This method disregards case of specified approval status value.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public static BlogMLApprovalStatus ApprovalStatusByValue(string value)
        {
            BlogMLApprovalStatus approvalStatus = BlogMLApprovalStatus.None;
            Guard.ArgumentNotNullOrEmptyString(value, "value");
            foreach (FieldInfo fieldInfo in typeof(BlogMLApprovalStatus).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLApprovalStatus))
                {
                    BlogMLApprovalStatus status = (BlogMLApprovalStatus)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(
                        typeof(EnumerationMetadataAttribute),
                        false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata =
                            customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(
                                value,
                                enumerationMetadata.AlternateValue,
                                StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            approvalStatus = status;
                            break;
                        }
                    }
                }
            }

            return approvalStatus;
        }

        /// <summary>
        /// Compares objects that implement the <see cref="IBlogMLCommonObject"/> interface.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IBlogMLCommonObject"/> interface to be compared.</param>
        /// <param name="target">A object that implements the <see cref="IBlogMLCommonObject"/> to compare with the <paramref name="source"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public static int CompareCommonObjects(IBlogMLCommonObject source, IBlogMLCommonObject target)
        {
            int result = 0;
            if (source == null && target == null)
            {
                return 0;
            }

            if (source != null && target == null)
            {
                return 1;
            }

            if (source == null && target != null)
            {
                return -1;
            }

            result = source.ApprovalStatus.CompareTo(target.ApprovalStatus);
            result = result | source.CreatedOn.CompareTo(target.CreatedOn);
            result = result | string.Compare(source.Id, target.Id, StringComparison.OrdinalIgnoreCase);
            result = result | source.LastModifiedOn.CompareTo(target.LastModifiedOn);

            if (source.Title != null && target.Title != null)
            {
                result = result | source.Title.CompareTo(target.Title);
            }
            else if (source.Title != null && target.Title == null)
            {
                result = result | 1;
            }
            else if (source.Title == null && target.Title != null)
            {
                result = result | -1;
            }

            return result;
        }

        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within  Web Log Markup Language (BlogML) syndication entities.
        /// </summary>
        /// <param name="nameTable">The table of atomized string objects.</param>
        /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference (Nothing in Visual Basic).</exception>
        public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
        {
            XmlNamespaceManager manager = null;
            Guard.ArgumentNotNull(nameTable, "nameTable");
            manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace(
                "blog",
                !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : InternalBlogmlNamespace);

            return manager;
        }

        /// <summary>
        /// Modifies the <see cref="IBlogMLCommonObject"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IBlogMLCommonObject"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract BlogML common object information from.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool FillCommonObject(IBlogMLCommonObject target, XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = CreateNamespaceManager(source.NameTable);
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
                    DateTime createdOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out createdOn))
                    {
                        target.CreatedOn = createdOn;
                        wasLoaded = true;
                    }
                    else if (DateTime.TryParse(
                        dateCreatedAttribute,
                        DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out createdOn))
                    {
                        target.CreatedOn = createdOn;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(dateModifiedAttribute))
                {
                    DateTime modifiedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateModifiedAttribute, out modifiedOn))
                    {
                        target.LastModifiedOn = modifiedOn;
                        wasLoaded = true;
                    }
                    else if (DateTime.TryParse(
                        dateModifiedAttribute,
                        DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out modifiedOn))
                    {
                        target.LastModifiedOn = modifiedOn;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(approvedAttribute))
                {
                    BlogMLApprovalStatus status = ApprovalStatusByValue(approvedAttribute);
                    if (status != BlogMLApprovalStatus.None)
                    {
                        target.ApprovalStatus = status;
                        wasLoaded = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator titleNavigator = source.SelectSingleNode("blog:title", manager);
                if (titleNavigator != null)
                {
                    BlogMLTextConstruct title = new BlogMLTextConstruct();
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
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool FillCommonObject(
            IBlogMLCommonObject target,
            XPathNavigator source,
            SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");
            XmlNamespaceManager manager = CreateNamespaceManager(source.NameTable);
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
                    DateTime createdOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out createdOn))
                    {
                        target.CreatedOn = createdOn;
                        wasLoaded = true;
                    }
                    else if (DateTime.TryParse(
                        dateCreatedAttribute,
                        DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out createdOn))
                    {
                        target.CreatedOn = createdOn;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(dateModifiedAttribute))
                {
                    DateTime modifiedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateModifiedAttribute, out modifiedOn))
                    {
                        target.LastModifiedOn = modifiedOn;
                        wasLoaded = true;
                    }
                    else if (DateTime.TryParse(
                        dateModifiedAttribute,
                        DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out modifiedOn))
                    {
                        target.LastModifiedOn = modifiedOn;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(approvedAttribute))
                {
                    BlogMLApprovalStatus status = ApprovalStatusByValue(approvedAttribute);
                    if (status != BlogMLApprovalStatus.None)
                    {
                        target.ApprovalStatus = status;
                        wasLoaded = true;
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNavigator titleNavigator = source.SelectSingleNode("blog:title", manager);
                if (titleNavigator != null)
                {
                    BlogMLTextConstruct title = new BlogMLTextConstruct();
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteCommonObjectAttributes(IBlogMLCommonObject source, XmlWriter writer)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(writer, "writer");

            if (!string.IsNullOrEmpty(source.Id))
            {
                writer.WriteAttributeString("id", source.Id);
            }

            if (source.CreatedOn != DateTime.MinValue)
            {
                writer.WriteAttributeString(
                    "date-created",
                    SyndicationDateTimeUtility.ToRfc3339DateTime(source.CreatedOn));
            }

            if (source.LastModifiedOn != DateTime.MinValue)
            {
                writer.WriteAttributeString(
                    "date-modified",
                    SyndicationDateTimeUtility.ToRfc3339DateTime(source.LastModifiedOn));
            }

            if (source.ApprovalStatus != BlogMLApprovalStatus.None)
            {
                writer.WriteAttributeString("approved", ApprovalStatusAsString(source.ApprovalStatus));
            }
        }

        /// <summary>
        /// Saves the current <see cref="IBlogMLCommonObject"/> elements to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IBlogMLCommonObject"/> interface to extract BlogML common object information from.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteCommonObjectElements(IBlogMLCommonObject source, XmlWriter writer)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(writer, "writer");
            if (source.Title != null)
            {
                source.Title.WriteTo(writer, "title");
            }
        }
    }
}