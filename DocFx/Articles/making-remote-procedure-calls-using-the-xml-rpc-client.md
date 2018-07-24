# Extensible Markup Language Remote Procedure Call (XML-RPC) Client

XML-RPC is a Remote Procedure Calling protocol that works over the Internet. An XML-RPC message is an HTTP-POST request. The body of the request is in XML. A procedure executes on the server and the value it returns is also formatted in XML. Procedure parameters can be scalars, numbers, strings, dates, etc. and can also be complex record and list structures.

The Argotic framework supports the XML-RPC communication protocol through the _XmlRpcClient_ class, which allows applications to send remote procedure calls by using the Extensible Markup Language Remote Procedure Call (XML-RPC) protocol. The network client supports both synchronous and asynchronous communication.

    using Argotic.Net;

    // Initialize the XML-RPC client
    XmlRpcClient client = new XmlRpcClient();
    client.Host = new Uri("http://bob.example.net/xmlrpcserver");

    // Construct a Pingback peer-to-peer notification XML-RPC message
    XmlRpcMessage message = new XmlRpcMessage("pingback.ping");
    message.Encoding = Encoding.UTF8;
    message.Parameters.Add(new XmlRpcScalarValue("http://alice.example.org/#p123")); // sourceURI
    message.Parameters.Add(new XmlRpcScalarValue("http://bob.example.net/#foo")); // targetURI

    // Send a synchronous pingback ping
    XmlRpcResponse response = client.Send(message);

    // Verify response to the trackback ping
    if (response != null)
    {
        if (response.Fault != null)
        {
            XmlRpcStructureMember faultCode = response.Fault["faultCode"];
            XmlRpcStructureMember faultMessage = response.Fault["faultString"];

            if (faultCode != null && faultMessage != null)
            {
                // Handle the pingback ping error condition that occurred
            }
        }
        else
        {
            XmlRpcScalarValue successInformation = response.Parameter as XmlRpcScalarValue;
            if (successInformation != null)
            {
                // Pingback request was successful, return should be a string containing information the server deems useful.
            }
        }
    }

The example above is a describes how to use the XmlRpcClient to send a Pingback peer-to-peer notification ping.