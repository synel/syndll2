Syndll2
=======

Syndll2 is a reliable, high-performance, thread-safe implementation of the Synel Communications Protocol.
It is used primarily for communicating with Synel time and attendance terminals, such as the SY700 series.

This library supports .Net 4.0 and higher.  However, if you are using .Net 4.5, you can optionally take
advantage of asynchronous methods, using async/await patterns.

#### A note on thread safety
 - A connection should be created, used, and disposed in the same thread.  It is best to do this with a `using` block.
 - Multiple threads can be created, each using their own connection.
 - Simultaneous connections to the same terminal from different threads will automatically be queued so they can't interfere with each other.
 - Simultaneous connections to multiple *different* terminals are supported and can be run in parallel.

### Installation

The best and easiest way to use Syndll2 is by adding a NuGet package reference.

    PM> Install-Package Syndll2 -Pre
    
*Note that the package is currently in prerelease status.*
    
This will add a compiled version of Syndll2.dll to your project, and add a project reference automatically.

 - Nuget makes it very simple to update your project to the latest version of this library.
 - NuGet is available for both Visual Studio 2010 and 2012.
 - You can use the Package Manager console within Visual Studio, or you can also use a graphical interface if you prefer.
 - If you are not familiar with Nuget, please [start here](http://docs.nuget.org/).
 - You can also visit the [Syndll2 Nuget Feed](https://nuget.org/packages/Syndll2/) directly.

##### Staying Current

As changes are made to Syndll2, you can use Nuget to keep your project updated.

    PM> Update-Package Syndll2 -Pre
    
*Again, note that the `-Pre` flag is required since the library is currently in prerelease status.
After a stable version is published, you will no longer have to pass the `-Pre` flag, and this page will be updated.*


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
