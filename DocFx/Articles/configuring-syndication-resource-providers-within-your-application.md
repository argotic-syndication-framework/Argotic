# Configuring syndication resource providers within your application

The first step is add the framework configuration sections for the features you wish to utilize within your App.config or Web.config configuration file:

	<configSections>
	  <!-- Syndication resources configuration section -->
	  <section name="argotic.syndication" type="Argotic.Configuration.Provider.SyndicationResourceSection, Argotic.Core"/>
	
	  <!-- Syndication HTTP Handler configuration section -->
	  <section name="argotic.web.httpHandler" type="Argotic.Configuration.SyndicationResourceHandlerSection, Argotic.Web"/>
	</configSections>

In the example above, the core _argotic.syndication_ configuration section that most framework features are dependent upon is defined. In addition the configuration section for the syndication HTTP handler as been included.


The next step is to define the referenced configuration sections:

	<!-- Configure provider(s) used to store syndicated content and indicate the default provider to use -->
	<argotic.syndication defaultProvider="MySyndicationResourceProvider">
	  <providers>
	    <add name="MySyndicationResourceProvider" type="Argotic.Configuration.Provider.XmlSyndicationResourceProvider, Argotic.Core" applicationName="/" path="~/App_Data/Syndication" />
	  </providers>
	</argotic.syndication>

In the configuration section above, a single syndication resource provider is configured, in this case the XML file-based provider that is included in the framework.

	<!-- Configure syndication HTTP handler (typically used by ASP.NET applications to act as a generic endpoint for managed syndicated content) -->
	<argotic.web.httpHandler
	  enableCaching="true"
	  format="Rss"
	  updatableWithin="0:15:0.0"
	  validFor="0:59:0.0" 
	/>

In the configuration section above, the HTTP handler has been configured to serve RSS feeds by default and has had caching enabled to improve scalability. By default the first (and sometime only) syndication resource managed by the provider for the specified default content format is served by the HTTP handler. If you wish to specify a specific managed resource, you may use the _id_ attribute to explicitly indicate which syndication resource to retrieve from the provider.

If you choose to use the flexible HTTP handler that the framework provides, you must perform the additional step of mapping a path to the handler. This step varies depending on whether you use the classic or integrated managed pipeline mode:

Classic:

	<httpHandlers>
	  <add verb="GET,HEAD" path="syndication.axd" type="Argotic.Web.SyndicationResourceHandler, Argotic.Web" validate="false" />
	</httpHandlers>

Integrated:

	<handlers>
	  <add name="SyndicationHandler" verb="GET,HEAD" path="syndication.axd" type="Argotic.Web.SyndicationResourceHandler, Argotic.Web" />
	</handlers>

In the example above, we have specified that the syndication resource HTTP handler will be mapped to an endpoint path of _syndication.axd_ and will accept the GET and HEAD verbs.