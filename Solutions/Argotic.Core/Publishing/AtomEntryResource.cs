/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
07/16/2008	brian.kuhn	Created AtomEntryResource class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Net;
using Argotic.Syndication;
using Argotic.Extensions;
using Argotic.Extensions.Core;

namespace Argotic.Publishing
{
    /// <summary>
    /// Represents a resource whose IRI is listed in a <see cref="AtomFeed"/> and uses <see cref="AtomEntry"/> as its representation.
    /// </summary>
    /// <seealso cref="AtomEntry"/>
    [Serializable()]
    public class AtomEntryResource : AtomEntry
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous load operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;
        /// <summary>
        /// Private member to hold the last time the entry was edited. If the entry has not been edited yet, indicates the time the entry was created.
        /// </summary>
        private DateTime entryResourceEditedOn  = DateTime.MinValue;
        /// <summary>
        /// Private member to hold a value indicating if the client is requesting to control the visibility of the entry.
        /// </summary>
        private bool entryResourceIsDraft;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region AtomEntryResource()
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntryResource"/> class.
        /// </summary>
        public AtomEntryResource() : base()
        {
            //------------------------------------------------------------
            //	Initialization handled by base class
            //------------------------------------------------------------
        }
        #endregion

        #region AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn)
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
        /// </summary>
        /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
        /// <param name="updatedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant. 
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn) : base(id, title, updatedOn)
        {
            //------------------------------------------------------------
            //	Initialization handled by base class
            //------------------------------------------------------------
        }
        #endregion

        #region AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn)
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
        /// </summary>
        /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
        /// <param name="updatedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant. 
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <param name="editedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited. 
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn) : this(id, title, updatedOn)
        {
            //------------------------------------------------------------
            //	Initialize class state using properties
            //------------------------------------------------------------
            this.EditedOn   = editedOn;
        }
        #endregion

        #region AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn, bool isDraft)
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
        /// </summary>
        /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
        /// <param name="updatedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant. 
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <param name="editedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited. 
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <param name="isDraft">A value indicating if client has requested to control the visibility of the entry.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn, bool isDraft) : this(id, title, updatedOn, editedOn)
        {
            //------------------------------------------------------------
            //	Initialize class state using properties
            //------------------------------------------------------------
            this.IsDraft    = isDraft;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region EditedOn
        /// <summary>
        /// Gets or sets a date-time indicating the most recent instant in time when this entry was edited.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited. 
        ///     If the entry has not been edited yet, indicates the time the entry was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no edit time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        /// <seealso cref="AtomPublishingEditedSyndicationExtension"/>
        public DateTime EditedOn
        {
            get
            {
                return entryResourceEditedOn;
            }

            set
            {
                entryResourceEditedOn = value;
            }
        }
        #endregion

        #region IsDraft
        /// <summary>
        /// Gets or sets a value indicating if client has requested to control the visibility of this entry.
        /// </summary>
        /// <value><b>true</b> if the client is requesting to control the visibility of this entry; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        /// <seealso cref="AtomPublishingControlSyndicationExtension"/>
        public bool IsDraft
        {
            get
            {
                return entryResourceIsDraft;
            }

            set
            {
                entryResourceIsDraft = value;
            }
        }
        #endregion

        //============================================================
        //	ASYNC METHODS
        //============================================================
        #region LoadAsync(Uri source, SyndicationResourceLoadSettings settings, ICredentials credentials, IWebProxy proxy, Object userToken)
        /// <summary>
        /// Loads this <see cref="AtomEntry"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="AtomEntry.Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, 
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="AtomEntry.LoadAsync(Uri, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomEntry"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        public new void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, ICredentials credentials, IWebProxy proxy, Object userToken)
        {
            this.LoadAsync(source, settings, new WebRequestOptions(credentials, proxy), userToken);
        }
        #endregion
        #region LoadAsync(Uri source, SyndicationResourceLoadSettings settings, WebRequestOptions options, Object userToken)
        /// <summary>
        /// Loads this <see cref="AtomEntry"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="AtomEntry.Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, 
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="AtomEntry.LoadAsync(Uri, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomEntry"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        public new void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, WebRequestOptions options, Object userToken)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Use default settings if none specified by the caller
            //------------------------------------------------------------
            if (settings == null)
            {
                settings    = new SyndicationResourceLoadSettings();
            }

            //------------------------------------------------------------
            //	Validate syndication resource state
            //------------------------------------------------------------
            if (this.LoadOperationInProgress)
            {
                throw new InvalidOperationException();
            }

            //------------------------------------------------------------
            //	Indicate that a load operation is in progress
            //------------------------------------------------------------
            this.LoadOperationInProgress    = true;

            //------------------------------------------------------------
            //	Reset the asynchronous load operation cancelled indicator
            //------------------------------------------------------------
            this.AsyncLoadHasBeenCancelled  = false;

            //------------------------------------------------------------
            //	Build HTTP web request used to retrieve the syndication resource
            //------------------------------------------------------------
            asyncHttpWebRequest         = SyndicationEncodingUtility.CreateWebRequest(source, options);
            asyncHttpWebRequest.Timeout = Convert.ToInt32(settings.Timeout.TotalMilliseconds, System.Globalization.NumberFormatInfo.InvariantInfo);

            //------------------------------------------------------------
            //	Get the async response to the web request
            //------------------------------------------------------------
            object[] state      = new object[6] { asyncHttpWebRequest, this, source, settings, options, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncLoadCallback), state);

            //------------------------------------------------------------
            //  Register the timeout callback
            //------------------------------------------------------------
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncTimeoutCallback), state, settings.Timeout, true);
        }
        #endregion

        #region LoadAsyncCancel()
        /// <summary>
        /// Cancels an asynchronous operation to load this syndication resource.
        /// </summary>
        /// <remarks>
        ///     Use the LoadAsyncCancel method to cancel a pending <see cref="AtomEntry.LoadAsync(Uri, Object)"/> operation. 
        ///     If there is a load operation in progress, this method releases resources used to execute the load operation. 
        ///     If there is no load operation pending, this method does nothing.
        /// </remarks>
        public new void LoadAsyncCancel()
        {
            //------------------------------------------------------------
            //	Determine if load of syndication resource call in progress 
            //  and the async operation has not already been cancelled
            //------------------------------------------------------------
            if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
            {
                //------------------------------------------------------------
                //	Set async operation cancelled indicator
                //------------------------------------------------------------
                this.AsyncLoadHasBeenCancelled = true;

                //------------------------------------------------------------
                //	Cancel the async load operation
                //------------------------------------------------------------
                asyncHttpWebRequest.Abort();
            }
        }
        #endregion

        //============================================================
        //	CALLBACK DELEGATE METHODS
        //============================================================
        #region AsyncLoadCallback(IAsyncResult result)
        /// <summary>
        /// Called when a corresponding asynchronous load operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncLoadCallback(IAsyncResult result)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            System.Text.Encoding encoding               = System.Text.Encoding.UTF8;
            XPathNavigator navigator                    = null;
            WebRequest httpWebRequest                   = null;
            AtomEntryResource entry                     = null;
            Uri source                                  = null;
            WebRequestOptions options                   = null;
            SyndicationResourceLoadSettings settings    = null;

            //------------------------------------------------------------
            //	Determine if the async send operation completed
            //------------------------------------------------------------
            if (result.IsCompleted)
            {
                //------------------------------------------------------------
                //	Extract the send operations parameters from the user state
                //------------------------------------------------------------
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest      = parameters[0] as WebRequest;
                entry               = parameters[1] as AtomEntryResource;
                source              = parameters[2] as Uri;
                settings            = parameters[3] as SyndicationResourceLoadSettings;
                options             = parameters[4] as WebRequestOptions;
                object userToken    = parameters[5];

                //------------------------------------------------------------
                //	Verify expected parameters were found
                //------------------------------------------------------------
                if (entry != null)
                {
                    //------------------------------------------------------------
                    //	Get the response to the syndication resource request
                    //------------------------------------------------------------
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);

                    //------------------------------------------------------------
                    //	Load syndication resource
                    //------------------------------------------------------------
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (settings != null)
                        {
                            encoding    = settings.CharacterEncoding;
                        }

                        using (StreamReader streamReader = new StreamReader(stream, encoding))
                        {
                            XmlReaderSettings readerSettings    = new XmlReaderSettings();
                            readerSettings.IgnoreComments       = true;
                            readerSettings.IgnoreWhitespace     = true;
                            readerSettings.DtdProcessing = DtdProcessing.Ignore;

                            using (XmlReader reader = XmlReader.Create(streamReader, readerSettings))
                            {
                                if (encoding == System.Text.Encoding.UTF8)
                                {
                                    navigator   = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
                                }
                                else
                                {
                                    navigator   = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
                                }

                                //------------------------------------------------------------
                                //	Load syndication resource using the framework adapters
                                //------------------------------------------------------------
                                SyndicationResourceAdapter adapter  = new SyndicationResourceAdapter(navigator, settings);
                                adapter.Fill(entry, SyndicationContentFormat.Atom);

                                //------------------------------------------------------------
                                //	Extract member resource information from extensions
                                //------------------------------------------------------------
                                AtomPublishingEditedSyndicationExtension editedExtension    = entry.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;
                                if (editedExtension != null)
                                {
                                    entry.EditedOn  = editedExtension.Context.EditedOn;
                                }

                                AtomPublishingControlSyndicationExtension controlExtension  = entry.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension;
                                if (controlExtension != null)
                                {
                                    entry.IsDraft   = controlExtension.Context.IsDraft;
                                }

                                //------------------------------------------------------------
                                //	Raise Loaded event to notify registered handlers of state change
                                //------------------------------------------------------------
                                entry.OnEntryLoaded(new SyndicationResourceLoadedEventArgs(navigator, source, options, userToken));
                            }
                        }
                    }

                    //------------------------------------------------------------
                    //	Reset load operation in progress indicator
                    //------------------------------------------------------------
                    entry.LoadOperationInProgress    = false;
                }
            }
        }
        #endregion

        #region AsyncTimeoutCallback(object state, bool timedOut)
        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            //------------------------------------------------------------
            //	Determine if asynchronous load operation timed out
            //------------------------------------------------------------
            if (timedOut)
            {
                //------------------------------------------------------------
                //	Abort asynchronous load operation
                //------------------------------------------------------------
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            //------------------------------------------------------------
            //	Reset load operation in progress indicator
            //------------------------------------------------------------
            this.LoadOperationInProgress    = false;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
        public new void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
        {
             //------------------------------------------------------------
            //	Load syndication resource using base entry implementation
            //------------------------------------------------------------
            base.Load(source, settings);

            //------------------------------------------------------------
            //	Extract member resource information from extensions
            //------------------------------------------------------------
            AtomPublishingEditedSyndicationExtension editedExtension    = this.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;
            if (editedExtension != null)
            {
                this.EditedOn   = editedExtension.Context.EditedOn;
            }

            AtomPublishingControlSyndicationExtension controlExtension  = this.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension;
            if (controlExtension != null)
            {
                this.IsDraft    = controlExtension.Context.IsDraft;
            }
        }
        #endregion

        #region Load(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/> 
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used. 
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
        public new void Load(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        {
            this.Load(source, new WebRequestOptions(credentials, proxy), settings);
        }
        #endregion

        #region Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/> 
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used. 
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
        public new void Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Load syndication resource using base entry implementation
            //------------------------------------------------------------
            base.Load(source, options, settings);

            //------------------------------------------------------------
            //	Extract member resource information from extensions
            //------------------------------------------------------------
            AtomPublishingEditedSyndicationExtension editedExtension    = this.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;
            if (editedExtension != null)
            {
                this.EditedOn   = editedExtension.Context.EditedOn;
            }

            AtomPublishingControlSyndicationExtension controlExtension  = this.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension;
            if (controlExtension != null)
            {
                this.IsDraft    = controlExtension.Context.IsDraft;
            }
        }
        #endregion

        #region Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
        /// <summary>
        /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistance of the <see cref="AtomEntry"/> instance.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="XmlException">The operation would not result in well formed XML for the syndication resource.</exception>
        public new void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Modify syndication extensions based on properties
            //------------------------------------------------------------
            List<ISyndicationExtension> list    = new List<ISyndicationExtension>(this.Extensions);

            if(this.EditedOn != DateTime.MinValue)
            {
                if (!list.Exists(AtomPublishingEditedSyndicationExtension.MatchByType))
                {
                    AtomPublishingEditedSyndicationExtension editedExtension    = new AtomPublishingEditedSyndicationExtension();
                    editedExtension.Context.EditedOn                            = this.EditedOn;
                    this.AddExtension(editedExtension);
                }
            }

            if(this.IsDraft)
            {
                if (!list.Exists(AtomPublishingControlSyndicationExtension.MatchByType))
                {
                    AtomPublishingControlSyndicationExtension controlExtension  = new AtomPublishingControlSyndicationExtension();
                    controlExtension.Context.IsDraft                            = this.IsDraft;
                    this.AddExtension(controlExtension);
                }
            }

            //------------------------------------------------------------
            //	Save member resource
            //------------------------------------------------------------
            base.Save(writer, settings);
        }
        #endregion
    }
}
