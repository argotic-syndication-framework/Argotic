/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/22/2008	brian.kuhn	Created SyndicationResourceProviderCollection Class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration.Provider;

namespace Argotic.Configuration.Provider
{
    /// <summary>
    /// Represents a collection of provider objects that inherit from <see cref="SyndicationResourceProvider"/>.
    /// </summary>
    public class SyndicationResourceProviderCollection : ProviderCollection, ICollection<SyndicationResourceProvider>
    {
        //============================================================
        //	INDEXERS
        //============================================================
        #region this[string name]
        /// <summary>
        /// Gets the provider with the specified name.
        /// </summary>
        /// <param name="name">The key by which the provider is identified.</param>
        /// <returns>The <see cref="SyndicationResourceProvider"/> with the specified name.</returns>
        public new SyndicationResourceProvider this[string name]
        {
            get
            {
                return (SyndicationResourceProvider)base[name];
            }
        }
        #endregion

        //============================================================
        //	ICOLLECTION<T> IMPLEMENTATION
        //============================================================
        #region Add(SyndicationResourceProvider item)
        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The <see cref="SyndicationResourceProvider"/> to add to the <see cref="SyndicationResourceProviderCollection"/>.</param>
        public void Add(SyndicationResourceProvider item)
        {
            base.Add(item);
        }
        #endregion

        #region Contains(SyndicationResourceProvider item)
        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The <see cref="SyndicationResourceProvider"/> to locate in the <see cref="SyndicationResourceProviderCollection"/>.</param>
        /// <returns><b>true</b> if item is found in the <see cref="SyndicationResourceProviderCollection"/>; otherwise, <b>false</b>.</returns>
        public bool Contains(SyndicationResourceProvider item)
        {
            if (base.Count > 0)
            {
                bool itemExists = false;
                foreach (SyndicationResourceProvider provider in this)
                {
                    if (String.Compare(provider.Name, item.Name, StringComparison.Ordinal) == 0)
                    {
                        itemExists  = true;
                        break;
                    }
                }
                return itemExists;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region CopyTo(SyndicationResourceProvider[] array, int arrayIndex)
        /// <summary>
        /// Copies the elements of the <see cref="SyndicationResourceProviderCollection"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="SyndicationResourceProviderCollection"/>. 
        ///     The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(SyndicationResourceProvider[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }
        #endregion

        #region IsReadOnly
        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <value><b>true</b> if the <see cref="SyndicationResourceProviderCollection"/> is read-only; otherwise, <b>false</b>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Remove(SyndicationResourceProvider item)
        /// <summary>
        /// Removes a specific <see cref="SyndicationResourceProvider"/> from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        ///     <b>true</b> if item was successfully removed from the <see cref="SyndicationResourceProviderCollection"/>; otherwise, <b>false</b>. 
        ///     This method also returns <b>false</b> if item is not found in the collection.
        /// </returns>
        public bool Remove(SyndicationResourceProvider item)
        {
            if (this.Contains(item))
            {
                base.Remove(item.Name);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region GetEnumerator()
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public new IEnumerator<SyndicationResourceProvider> GetEnumerator()
        {
            foreach (SyndicationResourceProvider provider in this)
            {
                yield return provider;
            }
        }
        #endregion
    }
}
