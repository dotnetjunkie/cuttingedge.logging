For running the integration tests in this project, you'll need to do the following:

1.	Create a database called 'LoggingTestDatabase' on your (unnamed) local SQL server.
	When creating a local database is not appropriate, you can alter the connection string in the App.config
	file.
2.	Enable the Distributed Transaction Coordinator (MSDTC), by going to Settings / Control Panel / 
	Administrative Tools / Services and starting the service named "Distributed Transaction Coordinator".
	
After these two steps you should be able to successfully run these integration tests.