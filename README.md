Syndll2
=======

Syndll2 is a reliable, high-performance, thread-aware implementation of the Synel Communications Protocol.
It is used primarily for communicating with Synel time and attendance terminals, such as the SY700 series.

### Compatibility

- This library supports .Net 4.0 and higher.  However, if you are using .Net 4.5, you can optionally take
advantage of asynchronous methods, using async/await patterns.

- It has also been tested under Mono on Linux without issue.

- It does not currently support being called via COM Interop.  If you are using C++, VB6, FoxPro, or another
  COM-based platform, then you will not be able to use this library.  You might, however, be able to interact
  with the [SynUtil](https://github.com/synel/SynUtil) command-line wrapper via system shell calls.

#### A note on thread safety
 - The library is "thread-aware" in that it can be used in a multithreaded application.
   - However, an individual `SynelClient` class is *single-threaded*.
   - A connection should be created, used, and disposed in the same thread.
   - It is best to do this with a `using` block.
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

##### Manual Installation

If you choose not to use NuGet, you can simply download the latest release from our [Releases Page](https://github.com/synel/syndll2/releases).

- Unzip the package and choose either the `Net40` or `Net45` subdirectory, depending on which version of .Net Framework you are using.
- Place the files somewhere in your solution directory.
- Add a reference to the `Syndll2.dll` file in your project.
- Only the `Syndll2.dll` file needs to be distributed with your application.  The `.pdb` and `.xml` files are for debugging and Intellisense support.

### Example Usage


**Synchronously**  (.Net 4.0 or 4.5)

```csharp
using (var client = SynelClient.Connect(HostAddress))
{
    // get a data record from the terminal
    var data = client.Terminal.GetData();
    
    // do something with the data here (save it to db, send it over the web, etc.)
    ...
    
    // acknowledge the record so the terminal will clear it
    client.AcknowledgeLastRecord();
}
```
    
**Asynchronously**  (requires .Net 4.5)

```csharp
using (var client = await SynelClient.ConnectAsync(HostAddress))
{
    // get a data record from the terminal
    var data = await client.Terminal.GetDataAsync();
    
    // do something with the data here (save it to db, send it over the web, etc.)
    ...
    
    // acknowledge the record so the terminal will clear it
    await client.AcknowledgeLastRecordAsync();
}
```
    
### Current Status / Road Map

Not all terminals or functions are currently supported.
This code is safe for production, but should still be considered *unstable*.

**Currently Implemented**
 - Network connection
 - Retreiving basic data
 - Various commands for getting status
 - Setting the terminal's clock
 - Programming operations, such as uploading a terminal program.
 - Sending and retrieving fingerprint templates.
 - Server to recieve data when Polling is enabled.
 - Miscellaneous other functions.
 
**Not yet implemented**
 - Serial or modem connection.
 - Firmware Updating
 - Online Mode (Host Query)
 
### Solution Structure

**Syndll2**  
This is the main project for the library, targeting .Net 4.5.

**Syndll2_Net40**  
This is a compatibility project, targeting .Net 4.0.
All source files are shared as links from the main project.

**Syndll2.Tests**  
This project contains unit tests to prove functionality.
Most are "live tests" that require a terminal to test against.


### Questions / Issues

Please contact your Synel support representative for general questions.
You may also post issues to [our issue tracker](https://github.com/synel/syndll2/issues).
