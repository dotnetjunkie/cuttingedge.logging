Version [Version Number] ([Friendly Version Number]) [Release Date]

Version 1.1.?.? (1.1) ????-??-??
	General description:
		...

	Bugfixes:
		-None.
	
	Code changes:
		-A LoggerExtensions class added containing extension methods for the ILogger interface. These
		 extension method replace the LoggerWrapper class which is now marked as obsolete.
		 
	Changes to project and solutions:
		-Reference to System.Core (.NET 3.5) added. Offically, the project is now dependent on .NET 3.5, but 
		 you it will still work with .NET 2.0. When adding the assembly to a .NET 2.0 project just ignore
		 the warning by choosing "Yes".


Version 1.0.0.0 (1.0) 2008-11-23
	General description:
		Initial release.