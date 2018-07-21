/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created DublinCoreElementSetSyndicationExtension Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
	/// <summary>
	/// Extends syndication specifications to provide a meta-data element resource description vocabulary.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The <see cref="DublinCoreElementSetSyndicationExtension"/> extends syndicated content to specify a vocabulary of fifteen properties for use in resource description. 
	///         This syndication extension conforms to the <b>Dublin Core Metadata Element Set</b> 1.1 specification, which can be found 
	///         at <a href="http://dublincore.org/documents/dces/">http://dublincore.org/documents/dces/</a>.
	///     </para>
	/// </remarks>
	/// <example>
	///     <code lang="cs" title="The following code example demonstrates the usage of the DublinCoreElementSetSyndicationExtension class.">
	///         <code 
	///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Extensions\Core\DublinCoreElementSetSyndicationExtensionExample.cs" 
	///             region="DublinCoreElementSetSyndicationExtension"
	///         />
	///     </code>
	/// </example>
	[Serializable()]
	public class DublinCoreElementSetSyndicationExtension : SyndicationExtension, IComparable
	{
		//============================================================
		//	PUBLIC/PRIVATE/PROTECTED MEMBERS
		//============================================================
		#region PRIVATE/PROTECTED/PUBLIC MEMBERS
		/// <summary>
		/// Private member to hold specific information about the extension.
		/// </summary>
		private DublinCoreElementSetSyndicationExtensionContext extensionContext = new DublinCoreElementSetSyndicationExtensionContext();
		#endregion

		//============================================================
		//	CONSTRUCTORS
		//============================================================
		#region DublinCoreElementSetSyndicationExtension()
		/// <summary>
		/// Initializes a new instance of the <see cref="DublinCoreElementSetSyndicationExtension"/> class.
		/// </summary>
		public DublinCoreElementSetSyndicationExtension()
			: base("dc", "http://purl.org/dc/elements/1.1/", new Version("1.1"), new Uri("http://dublincore.org/documents/dces/"), "Dublin Core Metadata Element Set", "Extends syndication feeds to provide a meta-data element resource description vocabulary.")
		{
			//------------------------------------------------------------
			//	Initialization handled by base class
			//------------------------------------------------------------
		}
		#endregion

		//============================================================
		//	PUBLIC PROPERTIES
		//============================================================
		#region Context
		/// <summary>
		/// Gets or sets the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> object associated with this extension.
		/// </summary>
		/// <value>A <see cref="DublinCoreElementSetSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
		/// <remarks>
		///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity. 
		///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
		///     are defined for the custom syndication extension.
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
		public DublinCoreElementSetSyndicationExtensionContext Context
		{
			get
			{
				return extensionContext;
			}

			set
			{
				Guard.ArgumentNotNull(value, "value");
				extensionContext = value;
			}
		}
		#endregion

		//============================================================
		//	STATIC METHODS
		//============================================================
		#region MatchByType(ISyndicationExtension extension)
		/// <summary>
		/// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
		/// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
		/// </summary>
		/// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
		/// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
		public static bool MatchByType(ISyndicationExtension extension)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(extension, "extension");

			//------------------------------------------------------------
			//	Determine if search condition was met 
			//------------------------------------------------------------
			if (extension.GetType() == typeof(DublinCoreElementSetSyndicationExtension))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary)
		/// <summary>
		/// Returns the type vocabulary identifier for the supplied <see cref="DublinCoreTypeVocabularies"/>.
		/// </summary>
		/// <param name="vocabulary">The <see cref="DublinCoreTypeVocabularies"/> to get the type vocabulary identifier for.</param>
		/// <returns>The type vocabulary identifier for the supplied <paramref name="vocabulary"/>, otherwise returns an empty string.</returns>
		public static string TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			string name = String.Empty;

			//------------------------------------------------------------
			//	Return alternate value based on supplied protocol
			//------------------------------------------------------------
			foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
			{
				if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
				{
					DublinCoreTypeVocabularies typeVocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

					if (typeVocabulary == vocabulary)
					{
						object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

						if (customAttributes != null && customAttributes.Length > 0)
						{
							EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

							name    = enumerationMetadata.AlternateValue;
							break;
						}
					}
				}
			}

			return name;
		}
		#endregion

		#region TypeVocabularyByName(string name)
		/// <summary>
		/// Returns the <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified type vocabulary name.
		/// </summary>
		/// <param name="name">The name of the type vocabulary.</param>
		/// <returns>A <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified string, otherwise returns <b>DublinCoreTypeVocabularies.None</b>.</returns>
		/// <remarks>This method disregards case of specified type vocabulary name.</remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
		public static DublinCoreTypeVocabularies TypeVocabularyByName(string name)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			DublinCoreTypeVocabularies typeVocabulary = DublinCoreTypeVocabularies.None;

			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNullOrEmptyString(name, "name");

			//------------------------------------------------------------
			//	Determine syndication content format based on supplied name
			//------------------------------------------------------------
			foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
			{
				if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
				{
					DublinCoreTypeVocabularies vocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
					object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

					if (customAttributes != null && customAttributes.Length > 0)
					{
						EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

						if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
						{
							typeVocabulary  = vocabulary;
							break;
						}
					}
				}
			}

			return typeVocabulary;
		}
		#endregion

		//============================================================
		//	PUBLIC METHODS
		//============================================================
		#region Load(IXPathNavigable source)
		/// <summary>
		/// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
		/// </summary>
		/// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
		public override bool Load(IXPathNavigable source)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			bool wasLoaded  = false;

			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(source, "source");

			//------------------------------------------------------------
			//	Attempt to extract syndication extension information
			//------------------------------------------------------------
			XPathNavigator navigator    = source.CreateNavigator();
			wasLoaded                   = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));

			//------------------------------------------------------------
			//	Raise extension loaded event
			//------------------------------------------------------------
			SyndicationExtensionLoadedEventArgs args    = new SyndicationExtensionLoadedEventArgs(source, this);
			this.OnExtensionLoaded(args);

			return wasLoaded;
		}
		#endregion

		#region Load(XmlReader reader)
		/// <summary>
		/// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
		/// </summary>
		/// <param name="reader">The <b>XmlReader</b> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
		public override bool Load(XmlReader reader)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(reader, "reader");

			//------------------------------------------------------------
			//	Create navigator against reader and pass to load method
			//------------------------------------------------------------
			XPathDocument document  = new XPathDocument(reader);

			return this.Load(document.CreateNavigator());
		}
		#endregion

		#region WriteTo(XmlWriter writer)
		/// <summary>
		/// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
		/// </summary>
		/// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
		public override void WriteTo(XmlWriter writer)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(writer, "writer");

			//------------------------------------------------------------
			//	Write current extension details to the writer
			//------------------------------------------------------------
			this.Context.WriteTo(writer, this.XmlNamespace);
		}
		#endregion

		//============================================================
		//	PUBLIC OVERRIDES
		//============================================================
		#region ToString()
		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.</returns>
		/// <remarks>
		///     This method returns the XML representation for the current instance.
		/// </remarks>
		public override string ToString()
		{
			//------------------------------------------------------------
			//	Build the string representation
			//------------------------------------------------------------
			using(MemoryStream stream = new MemoryStream())
			{
				XmlWriterSettings settings  = new XmlWriterSettings();
				settings.ConformanceLevel   = ConformanceLevel.Fragment;
				settings.Indent             = true;
				settings.OmitXmlDeclaration = true;

				using(XmlWriter writer = XmlWriter.Create(stream, settings))
				{
					this.WriteTo(writer);
				}

				stream.Seek(0, SeekOrigin.Begin);

				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
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
			DublinCoreElementSetSyndicationExtension value  = obj as DublinCoreElementSetSyndicationExtension;

			if (value != null)
			{
				int result  = String.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
				result      = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Version.CompareTo(value.Version);
				result      = result | String.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
				result      = result | String.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);

				result      = result | String.Compare(this.Context.Contributor, value.Context.Contributor, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Coverage, value.Context.Coverage, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Creator, value.Context.Creator, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Context.Date.CompareTo(value.Context.Date);
				result      = result | String.Compare(this.Context.Description, value.Context.Description, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Format, value.Context.Format, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Identifier, value.Context.Identifier, StringComparison.Ordinal);

				if (this.Context.Language != null)
				{
					if (value.Context.Language != null)
					{
						result  = result | String.Compare(this.Context.Language.Name, value.Context.Language.Name, StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						result  = result | 1;
					}
				}
				else if (this.Context.Language == null && value.Context.Language != null)
				{
					result      = result | -1;
				}

				result      = result | String.Compare(this.Context.Publisher, value.Context.Publisher, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Relation, value.Context.Relation, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Rights, value.Context.Rights, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Source, value.Context.Source, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Subject, value.Context.Subject, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Title, value.Context.Title, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Context.TypeVocabulary.CompareTo(value.Context.TypeVocabulary);

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
			if (!(obj is DublinCoreElementSetSyndicationExtension))
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
			return this.ToString().GetHashCode();
		}
		#endregion

		#region == operator
		/// <summary>
		/// Determines if operands are equal.
		/// </summary>
		/// <param name="first">Operand to be compared.</param>
		/// <param name="second">Operand to compare to.</param>
		/// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
		public static bool operator ==(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
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
		public static bool operator !=(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
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
		public static bool operator <(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
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
		public static bool operator >(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
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
