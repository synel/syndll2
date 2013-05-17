Syndll2
=======

Syndll2 is a reliable, high-performance, thread-safe implementation of the Synel Communications Protocol.

It is used primarily for communicating with Synel time and attendance terminals, such as the SY700 series.

It requires .Net 4.0 or 4.5, and can be used synchronously or asynchronously, using .Net 4.5 async/await patterns.

Example Usage
-------------

    // synchronously
    using (var client = SynelClient.Connect(HostAddress))
    {
        // get a data record from the terminal
        var data = client.Terminal.GetData();
        
        // do something with the data here (save it to db, send it over the web, etc.)
        ...
        
        // acknowledge the record so the terminal will clear it
        client.AcknowledgeLastRecord();
    }
    
    // asynchronously
    using (var client = await SynelClient.ConnectAsync(HostAddress))
    {
        // get a data record from the terminal
        var data = await client.Terminal.GetDataAsync();
        
        // do something with the data here (save it to db, send it over the web, etc.)
        ...
        
        // acknowledge the record so the terminal will clear it
        await client.AcknowledgeLastRecordAsync();
    }
