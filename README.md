Azure Table Storage Multi-Region
===========================

The purpose of this library is to abstract the implementation of a multi-region data center writes to Table Storage. In theory, this would make the application more resilient in case an Azure data center (region) went down.

Currently I am not running this code in production, I am hoping to implement something very similar sometime soon.  
As a guide line I created a demo application that is using this library.  You can find the demo application here: https://github.com/jsucupira/table-storage-geo-redundancy-demo

----------

Basic Information
----------------
The following are the main things the user of this library should be aware:

 - Dependency Injection:
	 - The library is using Managed Extensibility Framework (MEF) IOC framework to solve dependencies
	 - I am using a private NuGet library that implements the dependencies for IAzureTableUtility and IServiceBusContext
		 - You can find the private NuGet repo here: https://www.myget.org/F/jsucupira/api/v2. To make it easier I also included those packages in the source control
	 - There is a class called MefBase that needs to be setup with the MEF dependencies in order for this library to work.  Here is how I am implementing MEF on my demo application:
	 
    ```
    AggregateCatalog catalog = new AggregateCatalog();
    catalog.Catalogs.Add(new AssemblyCatalog(typeof (AzureUtilities.AzureUtilitiesAssembly).Assembly));
    Azure.TableStorage.Redundancy.MefBase.Container = new CompositionContainer(catalog);
    ```
    
	 - The main reason why I am implementing it this way, is for testability.  You can use the library AzureUtilities.Mock to mock the all the table storage calls.
 - The class ReplicationStrategy is supposed to be used on the Service Bus listener.  It should abstract the replication of the item entered in storage
 - Currently this library is also been stored in my private repo (https://www.myget.org/F/jsucupira/api/v2).  The main reason it is there and not in NuGet is because I am not sure if this is useful to anyone else besides me.