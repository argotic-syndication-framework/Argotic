using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a product or service data that can be associated to an <see cref="ApmlDocument"/>.
/// </summary>
/// <seealso cref="ApmlDocument.Applications"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlApplication class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Apml\ApmlApplicationExample.cs"
///             region="ApmlApplication"
///         />
///     </code>
/// </example>
[Serializable]
public class ApmlApplication : IComparable, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the unique name for the application.
    /// </summary>
    private string applicationName = string.Empty;
    /// <summary>
    /// Private member to hold the textual data of the application.
    /// </summary>
    private string applicationData = string.Empty;
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlApplication"/> class.
    /// </summary>
    public ApmlApplication()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlApplication"/> class using the supplied name.
    /// </summary>
    /// <param name="name">The unique name for this application.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public ApmlApplication(string name)
    {
        this.Name = name;
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
    /// Gets or sets the textual data of this application.
    /// </summary>
    /// <value>The textual content that represents the data payload for this application.</value>
    /// <remarks>
    ///     Application data <i>may</i> be entity encoded (HTML, XHTML, XML) content.
    /// </remarks>
    public string Data
    {
        get
        {
            return applicationData;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                applicationData = string.Empty;
            }
            else
            {
                applicationData = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets the unique name for this application.
    /// </summary>
    /// <value>The unique name for this application.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get
        {
            return applicationName;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            applicationName = value.Trim();
        }
    }
    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }
    /// <summary>
    /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string nameAttribute = source.GetAttribute("name", string.Empty);

            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
                wasLoaded = true;
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Data = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ApmlApplication"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Application", ApmlUtility.ApmlNamespace);

        writer.WriteAttributeString("name", this.Name);

        if (!string.IsNullOrEmpty(this.Data))
        {
            writer.WriteString(this.Data);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="ApmlApplication"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="ApmlApplication"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }
        ApmlApplication value = obj as ApmlApplication;

        if (value != null)
        {
            int result = string.Compare(this.Data, value.Data, StringComparison.OrdinalIgnoreCase);
            result |= string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);

            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not ApmlApplication)
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
    public static bool operator ==(ApmlApplication first, ApmlApplication second)
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
    public static bool operator !=(ApmlApplication first, ApmlApplication second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(ApmlApplication first, ApmlApplication second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(ApmlApplication first, ApmlApplication second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(ApmlApplication first, ApmlApplication second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(ApmlApplication first, ApmlApplication second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}
