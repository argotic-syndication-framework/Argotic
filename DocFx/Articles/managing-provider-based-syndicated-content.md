# Managing provider based syndicated content

The example code below shows how to use the static _SyndicationManager_ class to persist, retrieve, update and remove a simple RSS feed whose content is managed by the configured default provider. The task of managing provider-back syndicated content can be performed in a variety of application contexts, including console applications, smart clients, and ASP.NET web applications.

    Guid resourceId = new Guid("3D586B56-1057-4b43-AD2C-9E99C58918E6");

    // Get managed resource
    ISyndicationResource managedResource = SyndicationManager.GetResource(resourceId);

    if (managedResource == null)
    {
        RssFeed rssFeed = new RssFeed(new Uri(String.Format(null, "http://localhost/syndication.axd?id={0}", resourceId.ToString())), "Example RSS Feed");
        rssFeed.Channel.Description = "An example RSS feed.";
        rssFeed.Channel.PublicationDate = new DateTime(2008, 3, 26, 9, 29, 30);
        rssFeed.Channel.LastBuildDate = DateTime.Now;
        rssFeed.Channel.ManagingEditor = "John Doe (john.doe@example.com)";
        rssFeed.Channel.TimeToLive = 60;

        RssItem item = new RssItem();
        item.Author = "Sally Smith (sally.smith@example.com)";
        item.Link = new Uri("http://localhost/2008/03/26/Simple+RSS+Item.aspx");
        item.Title = "Simple RSS Item";
        item.Description = "A basic RSS channel item";

        rssFeed.Channel.AddItem(item);

        // Create managed resource
        SyndicationResourceCreateStatus status  = SyndicationManager.CreateResource(resourceId, rssFeed);

        if (status != SyndicationResourceCreateStatus.Success)
        {
            // Handle creation failure
        }
    }
    else
    {
        RssFeed feed = null;
        if (managedResource.Format == SyndicationContentFormat.Rss)
        {
            feed = (RssFeed)managedResource;
            foreach (RssItem item in feed.Channel.Items)
            {
                // Process current channel item
            }

            //  Update managed resource
            feed.Channel.Language = new CultureInfo("en-US");
            feed.Channel.Categories.Add(new RssCategory("Channel Category"));
            feed.Channel[0].Categories.Add(new RssCategory("Item Category"));
            feed.Channel.LastBuildDate = DateTime.Now;

            SyndicationManager.UpdateResource(resourceId, feed);
        }

        // Get all managed resources based on content format
        Collection <ISyndicationResource> managedResources = SyndicationManager.GetResources(SyndicationContentFormat.Rss);
        foreach (ISyndicationResource resource in managedResources)
        {
            RssFeed currentFeed = resource as RssFeed;

            foreach (RssItem item in currentFeed.Channel.Items)
            {
                // Process current channel item of current feed
            }
        }

        // Remove managed resource
        SyndicationManager.DeleteResource(resourceId);
    }