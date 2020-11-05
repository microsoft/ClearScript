ifdef DEBUG
	CONFIG = Debug
else
	CONFIG = Release
endif

MAKEFILE = $(word $(words $(MAKEFILE_LIST)), $(MAKEFILE_LIST))
MAKEDIR = $(dir $(abspath $(MAKEFILE)))
ROOTDIR = $(abspath $(MAKEDIR)/..)
UNIXDIR = $(ROOTDIR)/Unix
NETCOREDIR = $(ROOTDIR)/NetCore

.PHONY: all test clean cleanv8 zapv8

all:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript/ClearScript.Core.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript/ClearScript.V8.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptConsole/ClearScriptConsole.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptBenchmarks/ClearScriptBenchmarks.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj

test: all
	dotnet test --nologo --verbosity normal -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj

clean:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile clean
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript/ClearScript.Core.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript/ClearScript.V8.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptConsole/ClearScriptConsole.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptBenchmarks/ClearScriptBenchmarks.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj

cleanv8:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile cleanv8

zapv8:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile zapv8
