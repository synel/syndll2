Syndll2
=======

Syndll2 is a reliable, high-performance, thread-safe implementation of the Synel Communications Protocol.
It is used primarily for communicating with Synel time and attendance terminals, such as the SY700 series.
It currently requires .Net 4.0 or 4.5, and can be used synchronously or asynchronously,
using .Net 4.5 async/await patterns.

#### A note on thread safety
 - A connection should be created, used, and disposed in the same thread.  It is best to do this with a `using` block.
 - Multiple threads can be created, each using their own connection.
 - Simultaneous connections to the same terminal from different threads will automatically be queued so they can't interfere with each other.
 - Simultaneous connections to multiple *different* terminals are supported and can be run in parallel.

### Example Usage


**Synchronously**  (.Net 4.0 or 4.5)

    using (var client = SynelClient.Connect(HostAddress))
    {
        // get a data record from the terminal
        var data = client.Terminal.GetData();
        
        // do something with the data here (save it to db, send it over the web, etc.)
        ...
        
        // acknowledge the record so the terminal will clear it
        client.AcknowledgeLastRecord();
    }
    
**Asynchronously**  (requires .Net 4.5)

    using (var client = await SynelClient.ConnectAsync(HostAddress))
    {
        // get a data record from the terminal
        var data = await client.Terminal.GetDataAsync();
        
        // do something with the data here (save it to db, send it over the web, etc.)
        ...
        
        // acknowledge the record so the terminal will clear it
        await client.AcknowledgeLastRecordAsync();
    }
    
### Current Status / Road Map

Not all terminals or functions are currently supported.
This code is safe for production, but should still be considered *unstable*.

**Currently Implemented**
 - Network connection
 - Retreiving basic data
 - Various commands for getting status
 - Setting the terminal's clock
 
**Not yet implemented**
 - Sending and retrieving fingerprint templates.
 - Programming a terminal application.
 - Serial or modem connection.
 - Miscellaneous other lesser-used functions.

### Solution Structure

**Syndll2**  
This is the main library.  

**Syndll2.Tests**  
This project contains unit tests to prove functionality.
Most are "live tests" that require a terminal to test against.

**Syndll2.TimeAmerica**  
This library contains specific code for the Time America SAL, such as the custom parsing of terminal data.
It is included here as an example of code that you might write for your own implementation.


### Questions / Issues

Please contact your Synel support representative for general questions.
You may also post issues to [our issue tracker](https://github.com/synel/syndll2/issues).
