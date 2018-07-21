/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/13/2008	brian.kuhn	Created XmlRpcStructureValue Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Net
{
    /// <summary>
    /// Represents a remote procedure parameter value that is typically used to encapsulate small groups of related variables.
    /// </summary>
    /// <seealso cref="XmlRpcMessage.Parameters"/>
    /// <seealso cref="IXmlRpcValue"/>
    [Serializable()]
    public class XmlRpcStructureValue : IXmlRpcValue, IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the structure members.
        /// </summary>
        private Collection<XmlRpcStructureMember> structureMembers;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region XmlRpcStructureValue()
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcStructureValue"/> class.
        /// </summary>
        public XmlRpcStructureValue()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region XmlRpcStructureValue(XPathNodeIterator iterator)
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcStructureValue"/> class using the supplied <see cref="XPathNodeIterator"/>.
        /// </summary>
        /// <param name="iterator">A <see cref="XPathNodeIterator"/> that represents the <i>member</i> nodes for the structured list.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="iterator"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcStructureValue(XPathNodeIterator iterator)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(iterator, "iterator");

            //------------------------------------------------------------
            //	Parse iterator nodes to load collection
            //------------------------------------------------------------
            if (iterator.Count > 0)
            {
                while (iterator.MoveNext())
                {
                    XmlRpcStructureMember member    = new XmlRpcStructureMember();
                    if (member.Load(iterator.Current))
                    {
                        this.Members.Add(member);
                    }
                }
            }
        }
        #endregion

        //============================================================
        //	INDEXERS
        //============================================================
        #region this[string name]
        /// <summary>
        /// Gets or sets the <see cref="XmlRpcStructureMember"/> that has the specified name.
        /// </summary>
        /// <param name="name">The name that uniquely identifies the member to get or set.</param>
        /// <returns>The <see cref="XmlRpcStructureMember"/> with the specified name.</returns>
        /// <remarks>
        ///     If no member exists for the specified <paramref name="name"/>, returns a <b>null</b> reference. 
        ///     This indexer uses a <i>case insensitive</i> comparison of the specified member <paramref name="name"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public XmlRpcStructureMember this[string name]
        {
            get
            {
                Guard.ArgumentNotNullOrEmptyString(name, "name");

                XmlRpcStructureMember result    = null;

                foreach (XmlRpcStructureMember member in this.Members)
                {
                    if (String.Compare(member.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        result  = member;
                        break;
                    }
                }

                return result;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(name, "name");
                Guard.ArgumentNotNull(value, "value");
                
                for (int i = 0; i < this.Members.Count; i++)
                {
                    XmlRpcStructureMember member    = this.Members[i];
                    if (String.Compare(member.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Members[i] = value;
                        break;
                    }
                }
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Members
        /// <summary>
        /// Gets this structure's members.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="XmlRpcStructureMember"/> objects that represent this structure's members. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<XmlRpcStructureMember> Members
        {
            get
            {
                if (structureMembers == null)
                {
                    structureMembers = new Collection<XmlRpcStructureMember>();
                }
                return structureMembers;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region CompareSequence(Collection<XmlRpcStructureMember> source, Collection<XmlRpcStructureMember> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{XmlRpcStructureMember}"/> collections.
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
        public static int CompareSequence(Collection<XmlRpcStructureMember> source, Collection<XmlRpcStructureMember> target)
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
                    XmlRpcStructureMember member    = source[i];
                    if (!target.Contains(member))
                    {
                        result  = -1;
                        break;
                    }
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

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="XmlRpcStructureValue"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="XmlRpcStructureValue"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="XmlRpcStructureValue"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
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
            //	Attempt to extract structure information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNodeIterator memberIterator    = source.Select("struct/member");
                if (memberIterator != null && memberIterator.Count > 0)
                {
                    while (memberIterator.MoveNext())
                    {
                        XmlRpcStructureMember member   = new XmlRpcStructureMember();
                        if (member.Load(memberIterator.Current))
                        {
                            this.Members.Add(member);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="XmlRpcStructureValue"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("value");

            writer.WriteStartElement("struct");
            foreach(XmlRpcStructureMember member in this.Members)
            {
                member.WriteTo(writer);
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="XmlRpcStructureValue"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="XmlRpcStructureValue"/>.</returns>
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
            XmlRpcStructureValue value  = obj as XmlRpcStructureValue;

            if (value != null)
            {
                int result  = XmlRpcStructureValue.CompareSequence(this.Members, value.Members);

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
            if (!(obj is XmlRpcStructureValue))
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
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(XmlRpcStructureValue first, XmlRpcStructureValue second)
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
        public static bool operator !=(XmlRpcStructureValue first, XmlRpcStructureValue second)
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
        public static bool operator <(XmlRpcStructureValue first, XmlRpcStructureValue second)
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
        public static bool operator >(XmlRpcStructureValue first, XmlRpcStructureValue second)
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
