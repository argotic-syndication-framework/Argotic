/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created ITunesSyndicationExtension Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
	/// <summary>
	/// Extends syndication specifications to provide a means of describing iTunes podcasting information.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The <see cref="ITunesSyndicationExtension"/> extends syndicated content to specify iTunes podcasting information. This syndication extension conforms to the 
	///         <b>iTunes RSS Tags</b> 1.0 specification, which can be found at <a href="http://www.apple.com/itunes/store/podcaststechspecs.html#rss">http://www.apple.com/itunes/store/podcaststechspecs.html#rss</a>.
	///     </para>
	/// </remarks>
	/// <example>
	///     <code lang="cs" title="The following code example demonstrates the usage of the ITunesSyndicationExtension class.">
	///         <code 
	///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Extensions\Core\ITunesSyndicationExtensionExample.cs" 
	///             region="ITunesSyndicationExtension"
	///         />
	///     </code>
	/// </example>
	[Serializable()]
	public class ITunesSyndicationExtension : SyndicationExtension, IComparable
	{
		//============================================================
		//	PUBLIC/PRIVATE/PROTECTED MEMBERS
		//============================================================
		#region PRIVATE/PROTECTED/PUBLIC MEMBERS
		/// <summary>
		/// Private member to hold specific information about the extension.
		/// </summary>
		private ITunesSyndicationExtensionContext extensionContext  = new ITunesSyndicationExtensionContext();
		#endregion

		//============================================================
		//	CONSTRUCTORS
		//============================================================
		#region ITunesSyndicationExtension()
		/// <summary>
		/// Initializes a new instance of the <see cref="ITunesSyndicationExtension"/> class.
		/// </summary>
		public ITunesSyndicationExtension()
			: base("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd", new Version("1.0"), new Uri("http://www.apple.com/itunes/store/podcaststechspecs.html#rss"), "Apple iTunes Podcasting Extension", "Extends syndication feeds to provide Apple iTunes podcasting media information.")
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
		/// Gets or sets the <see cref="ITunesSyndicationExtensionContext"/> object associated with this extension.
		/// </summary>
		/// <value>A <see cref="ITunesSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
		/// <remarks>
		///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity. 
		///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
		///     are defined for the custom syndication extension.
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
		public ITunesSyndicationExtensionContext Context
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
		#region CompareSequence(Collection<ITunesCategory> source, Collection<ITunesCategory> target)
		/// <summary>
		/// Compares two specified <see cref="Collection{ITunesCategory}"/> collections.
		/// </summary>
		/// <param name="source">The first collection.</param>
		/// <param name="target">The second collection.</param>
		/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
		/// <remarks>
		///     <para>
		///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
		///     </para>
		///     <para>
		///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
		///     </para>
		///     <para>
		///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
		public static int CompareSequence(Collection<ITunesCategory> source, Collection<ITunesCategory> target)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			int result  = 0;

			//------------------------------------------------------------
			//	Validate parameters
			//------------------------------------------------------------
			Guard.ArgumentNotNull(source, "source");
			Guard.ArgumentNotNull(target, "target");

			if (source.Count == target.Count)
			{
				for (int i = 0; i < source.Count; i++)
				{
					result  = result | source[i].CompareTo(target[i]);
				}
			}
			else if (source.Count > target.Count)
			{
				return 1;
			}
			else if (source.Count < target.Count)
			{
				return -1;
			}

			return result;
		}
		#endregion

		#region ExplicitMaterialAsString(ITunesExplicitMaterial material)
		/// <summary>
		/// Returns the cloud protocol identifier for the supplied <see cref="ITunesExplicitMaterial"/>.
		/// </summary>
		/// <param name="material">The <see cref="ITunesExplicitMaterial"/> to get the explicit material identifier for.</param>
		/// <returns>The explicit material identifier for the supplied <paramref name="material"/>, otherwise returns an empty string.</returns>
		public static string ExplicitMaterialAsString(ITunesExplicitMaterial material)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			string name = String.Empty;

			//------------------------------------------------------------
			//	Return alternate value based on supplied protocol
			//------------------------------------------------------------
			foreach (System.Reflection.FieldInfo fieldInfo in typeof(ITunesExplicitMaterial).GetFields())
			{
				if (fieldInfo.FieldType == typeof(ITunesExplicitMaterial))
				{
					ITunesExplicitMaterial explicitMaterial = (ITunesExplicitMaterial)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

					if (explicitMaterial == material)
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

		#region ExplicitMaterialByName(string name)
		/// <summary>
		/// Returns the <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified explicit material name.
		/// </summary>
		/// <param name="name">The name of the explicit material.</param>
		/// <returns>A <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified string, otherwise returns <b>ITunesExplicitMaterial.None</b>.</returns>
		/// <remarks>This method disregards case of specified explicit material name.</remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
		public static ITunesExplicitMaterial ExplicitMaterialByName(string name)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			ITunesExplicitMaterial explicitMaterial = ITunesExplicitMaterial.None;

			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNullOrEmptyString(name, "name");

			//------------------------------------------------------------
			//	Determine syndication content format based on supplied name
			//------------------------------------------------------------
			foreach (System.Reflection.FieldInfo fieldInfo in typeof(ITunesExplicitMaterial).GetFields())
			{
				if (fieldInfo.FieldType == typeof(ITunesExplicitMaterial))
				{
					ITunesExplicitMaterial material = (ITunesExplicitMaterial)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
					object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

					if (customAttributes != null && customAttributes.Length > 0)
					{
						EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

						if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
						{
							explicitMaterial    = material;
							break;
						}
					}
				}
			}

			return explicitMaterial;
		}
		#endregion

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
			if (extension.GetType() == typeof(ITunesSyndicationExtension))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		//============================================================
		//	PUBLIC METHODS
		//============================================================
		#region Load(IXPathNavigable source)
		/// <summary>
		/// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
		/// </summary>
		/// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="ITunesSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
		/// <param name="reader">The <b>XmlReader</b> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="ITunesSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
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
		/// Returns a <see cref="String"/> that represents the current <see cref="ITunesSyndicationExtension"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="ITunesSyndicationExtension"/>.</returns>
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
			ITunesSyndicationExtension value  = obj as ITunesSyndicationExtension;

			if (value != null)
			{
				int result  = String.Compare(this.Context.Author, value.Context.Author, StringComparison.OrdinalIgnoreCase);
				result      = result | ITunesSyndicationExtension.CompareSequence(this.Context.Categories, value.Context.Categories);
				result      = result | this.Context.Duration.CompareTo(value.Context.Duration);
				result      = result | this.Context.ExplicitMaterial.CompareTo(value.Context.ExplicitMaterial);
				result      = result | Uri.Compare(this.Context.Image, value.Context.Image, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Context.IsBlocked.CompareTo(value.Context.IsBlocked);
				result      = result | ComparisonUtility.CompareSequence(this.Context.Keywords, value.Context.Keywords, StringComparison.OrdinalIgnoreCase);
				result      = result | Uri.Compare(this.Context.NewFeedUrl, value.Context.NewFeedUrl, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Context.Owner.CompareTo(value.Context.Owner);
				result      = result | String.Compare(this.Context.Subtitle, value.Context.Subtitle, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Context.Summary, value.Context.Summary, StringComparison.OrdinalIgnoreCase);

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
			if (!(obj is ITunesSyndicationExtension))
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
		public static bool operator ==(ITunesSyndicationExtension first, ITunesSyndicationExtension second)
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
		public static bool operator !=(ITunesSyndicationExtension first, ITunesSyndicationExtension second)
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
		public static bool operator <(ITunesSyndicationExtension first, ITunesSyndicationExtension second)
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
		public static bool operator >(ITunesSyndicationExtension first, ITunesSyndicationExtension second)
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
