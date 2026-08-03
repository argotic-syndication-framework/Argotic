using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents an attention profile that can be associated to an <see cref="ApmlDocument"/>.
/// </summary>
/// <seealso cref="ApmlDocument.Profiles"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlProfile class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Apml\ApmlProfileExample.cs"
///             region="ApmlProfile"
///         />
///     </code>
/// </example>
[Serializable]
public class ApmlProfile : IComparable<ApmlProfile>, IEquatable<ApmlProfile>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlProfile"/> class.
    /// </summary>
    public ApmlProfile()
    {
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the explicit concepts of this profile.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlConcept"/> objects that represent the explicit concepts of this profile.</value>
    public IList<ApmlConcept> ExplicitConcepts { get; } = [];

    /// <summary>
    /// Gets the explicit sources of this profile.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlSource"/> objects that represent the explicit sources of this profile.</value>
    public IList<ApmlSource> ExplicitSources { get; } = [];

    /// <summary>
    /// Gets the implicit concepts of this profile.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlConcept"/> objects that represent the implicit concepts of this profile.</value>
    public IList<ApmlConcept> ImplicitConcepts { get; } = [];

    /// <summary>
    /// Gets the implicit sources of this profile.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlSource"/> objects that represent the implicit sources of this profile.</value>
    public IList<ApmlSource> ImplicitSources { get; } = [];

    /// <summary>
    /// Gets or sets the name of this profile.
    /// </summary>
    /// <value>The unique name of this profile.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="ApmlProfile"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ApmlProfile"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlProfile"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string nameAttribute = source.GetAttribute("name", string.Empty);
            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator implicitDataNavigator = source.SelectSingleNode("apml:ImplicitData", manager);
            XPathNavigator explicitDataNavigator = source.SelectSingleNode("apml:ExplicitData", manager);

            if (implicitDataNavigator != null)
            {
                XPathNodeIterator conceptsIterator = implicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                if (conceptsIterator is { Count: > 0 })
                {
                    while (conceptsIterator.MoveNext())
                    {
                        ApmlConcept concept = new();
                        if (concept.Load(conceptsIterator.Current))
                        {
                            this.ImplicitConcepts.Add(concept);
                            wasLoaded = true;
                        }
                    }
                }

                XPathNodeIterator sourcesIterator = implicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                if (sourcesIterator is { Count: > 0 })
                {
                    while (sourcesIterator.MoveNext())
                    {
                        ApmlSource attentionSource = new();
                        if (attentionSource.Load(sourcesIterator.Current))
                        {
                            this.ImplicitSources.Add(attentionSource);
                            wasLoaded = true;
                        }
                    }
                }
            }

            if (explicitDataNavigator != null)
            {
                XPathNodeIterator conceptsIterator = explicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                if (conceptsIterator is { Count: > 0 })
                {
                    while (conceptsIterator.MoveNext())
                    {
                        ApmlConcept concept = new();
                        if (concept.Load(conceptsIterator.Current))
                        {
                            this.ExplicitConcepts.Add(concept);
                            wasLoaded = true;
                        }
                    }
                }

                XPathNodeIterator sourcesIterator = explicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                if (sourcesIterator is { Count: > 0 })
                {
                    while (sourcesIterator.MoveNext())
                    {
                        ApmlSource attentionSource = new();
                        if (attentionSource.Load(sourcesIterator.Current))
                        {
                            this.ExplicitSources.Add(attentionSource);
                            wasLoaded = true;
                        }
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlProfile"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="ApmlProfile"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlProfile"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string nameAttribute = source.GetAttribute("name", string.Empty);
            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator implicitDataNavigator = source.SelectSingleNode("apml:ImplicitData", manager);
            XPathNavigator explicitDataNavigator = source.SelectSingleNode("apml:ExplicitData", manager);

            if (implicitDataNavigator != null)
            {
                XPathNodeIterator conceptsIterator = implicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                if (conceptsIterator is { Count: > 0 })
                {
                    while (conceptsIterator.MoveNext())
                    {
                        ApmlConcept concept = new();
                        if (concept.Load(conceptsIterator.Current, settings))
                        {
                            this.ImplicitConcepts.Add(concept);
                            wasLoaded = true;
                        }
                    }
                }

                XPathNodeIterator sourcesIterator = implicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                if (sourcesIterator is { Count: > 0 })
                {
                    while (sourcesIterator.MoveNext())
                    {
                        ApmlSource attentionSource = new();
                        if (attentionSource.Load(sourcesIterator.Current, settings))
                        {
                            this.ImplicitSources.Add(attentionSource);
                            wasLoaded = true;
                        }
                    }
                }
            }

            if (explicitDataNavigator != null)
            {
                XPathNodeIterator conceptsIterator = explicitDataNavigator.Select("apml:Concepts/apml:Concept", manager);
                if (conceptsIterator is { Count: > 0 })
                {
                    while (conceptsIterator.MoveNext())
                    {
                        ApmlConcept concept = new();
                        if (concept.Load(conceptsIterator.Current, settings))
                        {
                            this.ExplicitConcepts.Add(concept);
                            wasLoaded = true;
                        }
                    }
                }

                XPathNodeIterator sourcesIterator = explicitDataNavigator.Select("apml:Sources/apml:Source", manager);
                if (sourcesIterator is { Count: > 0 })
                {
                    while (sourcesIterator.MoveNext())
                    {
                        ApmlSource attentionSource = new();
                        if (attentionSource.Load(sourcesIterator.Current, settings))
                        {
                            this.ExplicitSources.Add(attentionSource);
                            wasLoaded = true;
                        }
                    }
                }
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ApmlProfile"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Profile", ApmlUtility.ApmlNamespace);

        writer.WriteAttributeString("name", this.Name);

        if (this.ImplicitConcepts.Count > 0 || this.ImplicitSources.Count > 0)
        {
            writer.WriteStartElement("ImplicitData", ApmlUtility.ApmlNamespace);

            if (this.ImplicitConcepts.Count > 0)
            {
                writer.WriteStartElement("Concepts", ApmlUtility.ApmlNamespace);
                foreach (ApmlConcept concept in this.ImplicitConcepts)
                {
                    concept.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            if (this.ImplicitSources.Count > 0)
            {
                writer.WriteStartElement("Sources", ApmlUtility.ApmlNamespace);
                foreach (ApmlSource source in this.ImplicitSources)
                {
                    source.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        if (this.ExplicitConcepts.Count > 0 || this.ExplicitSources.Count > 0)
        {
            writer.WriteStartElement("ExplicitData", ApmlUtility.ApmlNamespace);

            if (this.ExplicitConcepts.Count > 0)
            {
                writer.WriteStartElement("Concepts", ApmlUtility.ApmlNamespace);
                foreach (ApmlConcept concept in this.ExplicitConcepts)
                {
                    concept.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            if (this.ExplicitSources.Count > 0)
            {
                writer.WriteStartElement("Sources", ApmlUtility.ApmlNamespace);
                foreach (ApmlSource source in this.ExplicitSources)
                {
                    source.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ApmlProfile"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlProfile"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ApmlProfile? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.ExplicitConcepts, other.ExplicitConcepts);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.ExplicitSources, other.ExplicitSources);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.ImplicitConcepts, other.ImplicitConcepts);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.ImplicitSources, other.ImplicitSources);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApmlProfile"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ApmlProfile"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ApmlProfile"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(ApmlProfile? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ApmlProfile other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.ExplicitConcepts.Count),
            HashCodeUtility.Component(this.ExplicitSources.Count),
            HashCodeUtility.Component(this.ImplicitConcepts.Count),
            HashCodeUtility.Component(this.ImplicitSources.Count),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ApmlProfile first, ApmlProfile second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(ApmlProfile first, ApmlProfile second)
    {
        return !(first == second);
    }
}