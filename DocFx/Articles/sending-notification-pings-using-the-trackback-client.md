# Trackback Peer-to-Peer Notification Client

TrackBack is a framework for peer-to-peer communication and notifications between web sites. The central idea behind TrackBack is the idea of a _TrackBack ping_, a request saying, essentially, "resource A is related/linked to resource B." TrackBack uses a REST model, where requests are made through standard HTTP calls. 

To send a TrackBack ping, the client makes a standard HTTP request to the server, and receives a response in a simple XML format. 

To send a ping, the client sends an HTTP POST request to the TrackBack Ping URL. 

A TrackBack _resource_ is represented by a TrackBack Ping URL, which is just a standard URI. Using TrackBack, sites can communicate about related resources. For example, if Weblogger A wishes to notify Weblogger B that he has written something interesting/related/shocking, A sends a TrackBack ping to B. This accomplishes two things:

Weblogger B can automatically list all sites that have referenced a particular post on his site, allowing visitors to his site to read all related posts around the web, including Weblogger A's.

The ping provides a firm, explicit link between his entry and yours, as opposed to an implicit link (like a referrer log) that depends upon outside action (someone clicking on the link).

The Argotic framework supports Trackback peer-to-peer notification through the _TrackbackClient_ class, which allows applications to send and received notification pings by using the Trackback peer-to-peer notification protocol. The network client supports both synchronous and asynchronous communication.

	using Argotic.Net;

	// Initialize the Trackback peer-to-peer notification protocol client
	TrackbackClient client = new TrackbackClient();
	client.Host = new Uri("http://www.example.com/trackback/5");

	// Construct the trackback message to be sent
	TrackbackMessage message = new TrackbackMessage(new Uri("http://www.bar.com/"));
	message.Encoding = Encoding.UTF8;
	message.WeblogName = "Foo";
	message.Title = "Foo Bar";
	message.Excerpt = "My Excerpt";

	// Send a synchronous trackback ping
	TrackbackResponse response  = client.Send(message);

	// Verify response to the trackback ping
	if (response != null)
	{
	    if (response.HasError)
	    {
	        // Use the TrackbackResponse.ErrorMessage property to determine the reason the trackback ping failed
	    }
	}

The example above describes how to synchronously send a Trackback notification ping.