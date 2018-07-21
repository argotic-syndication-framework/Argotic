/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
07/16/2008	brian.kuhn	Created AtomPublishingControlSyndicationExtension Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
	/// <summary>
	/// Extends syndication specifications to provide a means of specifying publishing control of published resources.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The <see cref="AtomPublishingControlSyndicationExtension"/> extends syndicated content to specify publishing control of published resources. 
	///         This syndication extension conforms to the <b>Atom Publishing Protocol</b> 1.0 specification, which can be found 
	///         at <a href="http://bitworking.org/projects/atom/rfc5023.html">http://bitworking.org/projects/atom/rfc5023.html</a>.
	///     </para>
	///     <para>
	///         The <see cref="AtomPublishingControlSyndicationExtension"/> class implements the <i>app:control</i> element of the <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a>.
	///     </para>
	///     <para>
	///         The <see cref="AtomPublishingControlSyndicationExtension"/> <i>may</i> appear as a child of an Atom Entry that is being created or updated via the Atom Publishing Protocol. 
	///         The <see cref="AtomPublishingControlSyndicationExtension"/> <b>must</b> appear only once in an Atom Entry.
	///     </para>
	/// </remarks>
	[Serializable()]
	public class AtomPublishingControlSyndicationExtension : SyndicationExtension, IComparable
	{
		//============================================================
		//	PUBLIC/PRIVATE/PROTECTED MEMBERS
		//============================================================
		#region PRIVATE/PROTECTED/PUBLIC MEMBERS
		/// <summary>
		/// Private member to hold specific information about the extension.
		/// </summary>
		private AtomPublishingControlSyndicationExtensionContext extensionContext   = new AtomPublishingControlSyndicationExtensionContext();
		#endregion

		//============================================================
		//	CONSTRUCTORS
		//============================================================
		#region AtomPublishingControlSyndicationExtension()
		/// <summary>
		/// Initializes a new instance of the <see cref="AtomPublishingControlSyndicationExtension"/> class.
		/// </summary>
		public AtomPublishingControlSyndicationExtension()
			: base("app", "http://www.w3.org/2007/app", new Version("1.0"), new Uri("http://bitworking.org/projects/atom/rfc5023.html"), "Atom Publishing Protocol Control", "Extends syndication resource memebers to provide a means of specifying publishing control of published resources.")
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
		/// Gets or sets the <see cref="AtomPublishingControlSyndicationExtensionContext"/> object associated with this extension.
		/// </summary>
		/// <value>A <see cref="AtomPublishingControlSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
		/// <remarks>
		///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity. 
		///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
		///     are defined for the custom syndication extension.
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
		public AtomPublishingControlSyndicationExtensionContext Context
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
			if (extension.GetType() == typeof(AtomPublishingControlSyndicationExtension))
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
		/// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="AtomPublishingControlSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="AtomPublishingControlSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
		public override bool Load(IXPathNavigable source)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(source, "source");

			//------------------------------------------------------------
			//	Pass to load method with no explicit settings
			//------------------------------------------------------------
			return this.Load(source, null);
		}
		#endregion

		#region Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
		/// <summary>
		/// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
		/// </summary>
		/// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="AtomPublishingControlSyndicationExtension"/>.</param>
		/// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
		/// <returns><b>true</b> if the <see cref="AtomPublishingControlSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
		public bool Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
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
			if (settings == null)
			{
				settings    = new SyndicationResourceLoadSettings();
			}

			XPathNavigator navigator    = source.CreateNavigator();
			wasLoaded                   = this.Context.Load(navigator, this.CreateNamespaceManager(navigator), settings);

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
		/// <param name="reader">The <b>XmlReader</b> used to load this <see cref="AtomPublishingControlSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="AtomPublishingControlSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
		public override bool Load(XmlReader reader)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(reader, "reader");

			//------------------------------------------------------------
			//	Pass to load method with no explicit settings
			//------------------------------------------------------------
			return this.Load(reader, null);
		}
		#endregion

		#region Load(XmlReader reader, SyndicationResourceLoadSettings settings)
		/// <summary>
		/// Initializes the syndication extension using the supplied <see cref="XmlReader"/> and <see cref="SyndicationResourceLoadSettings"/>.
		/// </summary>
		/// <param name="reader">The <b>XmlReader</b> used to load this <see cref="AtomPublishingControlSyndicationExtension"/>.</param>
		/// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
		/// <returns><b>true</b> if the <see cref="AtomPublishingControlSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
		public bool Load(XmlReader reader, SyndicationResourceLoadSettings settings)
		{
			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNull(reader, "reader");

			//------------------------------------------------------------
			//	Create navigator against reader and pass to load method
			//------------------------------------------------------------
			if (settings == null)
			{
				settings = new SyndicationResourceLoadSettings();
			}
			XPathDocument document  = new XPathDocument(reader);

			return this.Load(document.CreateNavigator(), settings);
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
		/// Returns a <see cref="String"/> that represents the current <see cref="AtomPublishingControlSyndicationExtension"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="AtomPublishingControlSyndicationExtension"/>.</returns>
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
			AtomPublishingControlSyndicationExtension value  = obj as AtomPublishingControlSyndicationExtension;

			if (value != null)
			{
				int result  = String.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
				result      = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Version.CompareTo(value.Version);
				result      = result | String.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
				result      = result | String.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);

				result      = result | AtomPublishingUtility.CompareCommonObjectAttributes(this.Context, value.Context);
				result      = result | this.Context.IsDraft.CompareTo(value.Context.IsDraft);

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
			if (!(obj is AtomPublishingControlSyndicationExtension))
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
			//	Generate hash code using unique value of ToString() method
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
		public static bool operator ==(AtomPublishingControlSyndicationExtension first, AtomPublishingControlSyndicationExtension second)
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
		public static bool operator !=(AtomPublishingControlSyndicationExtension first, AtomPublishingControlSyndicationExtension second)
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
		public static bool operator <(AtomPublishingControlSyndicationExtension first, AtomPublishingControlSyndicationExtension second)
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
		public static bool operator >(AtomPublishingControlSyndicationExtension first, AtomPublishingControlSyndicationExtension second)
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
