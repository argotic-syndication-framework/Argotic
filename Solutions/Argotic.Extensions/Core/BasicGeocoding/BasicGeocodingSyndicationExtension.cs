/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created BasicGeocodingSyndicationExtension Class
****************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
	/// <summary>
	/// Extends syndication specifications to provide a means of representing latitude, longitude and other information about spatially-located things.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The <see cref="BasicGeocodingSyndicationExtension"/> extends syndicated content to specify a basic RDF vocabulary that provides the Semantic Web community 
	///         with a namespace for representing latitude, longitude and other information about spatially-located things. This syndication extension conforms to the 
	///         Basic Geo (WGS84 lat/long) Vocabulary specification, which can be found at <a href="http://www.w3.org/2003/01/geo/">http://www.w3.org/2003/01/geo/</a>.
	///     </para>
	/// </remarks>
	/// <example>
	///     <code lang="cs" title="The following code example demonstrates the usage of the BasicGeocodingSyndicationExtension class.">
	///         <code 
	///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Extensions\Core\BasicGeocodingSyndicationExtensionExample.cs" 
	///             region="BasicGeocodingSyndicationExtension"
	///         />
	///     </code>
	/// </example>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Geocoding")]
	[Serializable()]
	public class BasicGeocodingSyndicationExtension : SyndicationExtension, IComparable
	{
		//============================================================
		//	PUBLIC/PRIVATE/PROTECTED MEMBERS
		//============================================================
		#region PRIVATE/PROTECTED/PUBLIC MEMBERS
		/// <summary>
		/// Private member to hold specific information about the extension.
		/// </summary>
		private BasicGeocodingSyndicationExtensionContext extensionContext = new BasicGeocodingSyndicationExtensionContext();
		#endregion

		//============================================================
		//	CONSTRUCTORS
		//============================================================
		#region BasicGeocodingSyndicationExtension()
		/// <summary>
		/// Initializes a new instance of the <see cref="BasicGeocodingSyndicationExtension"/> class.
		/// </summary>
		public BasicGeocodingSyndicationExtension()
			: base("geo", "http://www.w3.org/2003/01/geo/wgs84_pos#", new Version("1.0"), new Uri("http://www.w3.org/2003/01/geo/"), "Basic Geocoding Vocabulary", "Extends syndication feeds to provide a means of representing latitude, longitude and other information about spatially-located things.")
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
		/// Gets or sets the <see cref="BasicGeocodingSyndicationExtensionContext"/> object associated with this extension.
		/// </summary>
		/// <value>A <see cref="BasicGeocodingSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
		/// <remarks>
		///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity. 
		///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
		///     are defined for the custom syndication extension.
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
		public BasicGeocodingSyndicationExtensionContext Context
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
		#region ConvertDecimalToDegreesMinutesSeconds(decimal value)
		/// <summary>
		/// Converts the supplied decimal value to an equivalent degrees, minutes, seconds string representation.
		/// </summary>
		/// <param name="value">A <see cref="Decimal"/> value that represents the degrees, minutes, and seconds of a spacial coordinate.</param>
		/// <returns>A string in the format ###°##'##.##" that represents the decimal value.</returns>
		/// <seealso cref="ConvertDegreesMinutesSecondsToDecimal(string)"/>
		public static string ConvertDecimalToDegreesMinutesSeconds(decimal value)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			string degreesPart  = String.Empty;
			string minutesPart  = String.Empty;
			string secondsPart  = String.Empty;
			decimal multiplier  = (decimal)60;
			decimal fractionalValue;

			string degreesAsString  = value.ToString(NumberFormatInfo.InvariantInfo);
			
			if(degreesAsString.Contains("."))
			{
				string[] degreesParts   = degreesAsString.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (degreesParts.Length == 2)
				{
					degreesPart = degreesParts[0];

					if (Decimal.TryParse("." + degreesParts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out fractionalValue))
					{
						decimal minutes = Decimal.Multiply(fractionalValue, multiplier);

						string minutesAsString  = minutes.ToString(NumberFormatInfo.InvariantInfo);
						if (minutesAsString.Contains("."))
						{
							string[] minutesParts   = minutesAsString.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							if (minutesParts.Length == 2)
							{
								minutesPart = minutesParts[0];

								if (Decimal.TryParse("." + minutesParts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out fractionalValue))
								{
									decimal seconds = Decimal.Multiply(fractionalValue, multiplier);
									secondsPart     = Decimal.Round(seconds, 2).ToString(NumberFormatInfo.InvariantInfo);
								}
							}
						}
					}
				}
			}

			return String.Format(null, "{0}°{1}'{2}\"", degreesPart, minutesPart, secondsPart);
		}
		#endregion

		#region ConvertDegreesMinutesSecondsToDecimal(string degreesMinutesSeconds)
		/// <summary>
		/// Converts the supplied degrees, minutes, and seconds spacial coordinate string to its equivalent decimal value.
		/// </summary>
		/// <param name="degreesMinutesSeconds">A degrees, minutes, and seconds of a spacial coordinate in the format ###°##'##.##".</param>
		/// <returns>A <see cref="Decimal"/> value that represents the supplied degrees, minutes, and seconds.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is a null reference (Nothing in Visual Basic).</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is an empty string.</exception>
		/// <seealso cref="ConvertDecimalToDegreesMinutesSeconds(decimal)"/>
		public static decimal ConvertDegreesMinutesSecondsToDecimal(string degreesMinutesSeconds)
		{
			//------------------------------------------------------------
			//	Local members
			//------------------------------------------------------------
			decimal degrees;
			decimal minutes;
			decimal seconds;

			//------------------------------------------------------------
			//	Validate parameter
			//------------------------------------------------------------
			Guard.ArgumentNotNullOrEmptyString(degreesMinutesSeconds, "degreesMinutesSeconds");

			//------------------------------------------------------------
			//	Validate string format
			//------------------------------------------------------------
			if (!degreesMinutesSeconds.Contains("°"))
			{
				throw new FormatException(String.Format(null, "The supplied degrees, minutes, seconds of {0} does not contain a ° degrees delimiter.", degreesMinutesSeconds));
			}
			else if (!degreesMinutesSeconds.Contains("'"))
			{
				throw new FormatException(String.Format(null, "The supplied degrees, minutes, seconds of {0} does not contain a ' minutes delimiter.", degreesMinutesSeconds));
			}
			else if (!degreesMinutesSeconds.Contains("\""))
			{
				throw new FormatException(String.Format(null, "The supplied degrees, minutes, seconds of {0} does not contain a \" seconds delimiter.", degreesMinutesSeconds));
			}

			//------------------------------------------------------------
			//	Extract and clean up delimited values
			//------------------------------------------------------------
			string degreesValue = degreesMinutesSeconds.Substring(0, degreesMinutesSeconds.IndexOf("°", StringComparison.OrdinalIgnoreCase));
			string minutesValue = degreesMinutesSeconds.Substring(degreesMinutesSeconds.IndexOf("°", StringComparison.OrdinalIgnoreCase) + 1, degreesMinutesSeconds.IndexOf("'", StringComparison.OrdinalIgnoreCase) - degreesMinutesSeconds.IndexOf("°", StringComparison.OrdinalIgnoreCase) - 1);
			string secondsValue = degreesMinutesSeconds.Substring(degreesMinutesSeconds.IndexOf("'", StringComparison.OrdinalIgnoreCase) + 1, degreesMinutesSeconds.IndexOf("\"", StringComparison.OrdinalIgnoreCase) - degreesMinutesSeconds.IndexOf("'", StringComparison.OrdinalIgnoreCase) - 1);
			
			degreesValue        = degreesValue.Trim();
			minutesValue        = minutesValue.Trim();
			secondsValue        = secondsValue.Replace("N", String.Empty).Replace("S", String.Empty).Replace("E", String.Empty).Replace("W", String.Empty);
			secondsValue        = secondsValue.Trim();

			//------------------------------------------------------------
			//	Validate extracted delimited values
			//------------------------------------------------------------
			if (!Decimal.TryParse(degreesValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out degrees))
			{
				throw new FormatException(String.Format(null, "The supplied degrees of {0} does not represent an integer.", degreesValue));
			}
			if (!Decimal.TryParse(minutesValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out minutes))
			{
				throw new FormatException(String.Format(null, "The supplied minutes of {0} does not represent an integer.", minutesValue));
			}
			if (!Decimal.TryParse(secondsValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out seconds))
			{
				throw new FormatException(String.Format(null, "The supplied seconds of {0} does not represent a floating point number.", secondsValue));
			}

			//------------------------------------------------------------
			//	Calculate decimal representation
			//------------------------------------------------------------
			return (degrees + (minutes/60) + (seconds/3600));
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
			if (extension.GetType() == typeof(BasicGeocodingSyndicationExtension))
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
		/// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
		/// <param name="reader">The <b>XmlReader</b> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
		/// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
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
//			return this.Load(document);
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
		/// Returns a <see cref="String"/> that represents the current <see cref="BasicGeocodingSyndicationExtension"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="BasicGeocodingSyndicationExtension"/>.</returns>
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
			BasicGeocodingSyndicationExtension value  = obj as BasicGeocodingSyndicationExtension;

			if (value != null)
			{
				int result  = String.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
				result      = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
				result      = result | String.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
				result      = result | this.Version.CompareTo(value.Version);
				result      = result | String.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
				result      = result | String.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);

				result      = result | this.Context.Latitude.CompareTo(value.Context.Latitude);
				result      = result | this.Context.Longitude.CompareTo(value.Context.Longitude);

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
			if (!(obj is BasicGeocodingSyndicationExtension))
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
		public static bool operator ==(BasicGeocodingSyndicationExtension first, BasicGeocodingSyndicationExtension second)
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
		public static bool operator !=(BasicGeocodingSyndicationExtension first, BasicGeocodingSyndicationExtension second)
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
		public static bool operator <(BasicGeocodingSyndicationExtension first, BasicGeocodingSyndicationExtension second)
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
		public static bool operator >(BasicGeocodingSyndicationExtension first, BasicGeocodingSyndicationExtension second)
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
