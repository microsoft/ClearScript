ifdef DEBUG
    CONFIG = Debug
else
    CONFIG = Release
endif

KERNEL = $(shell uname -s)
ifeq ($(KERNEL), Linux)
    MAKELINUX = make
else
    MAKELINUX = true
endif

MAKEFILE = $(word $(words $(MAKEFILE_LIST)), $(MAKEFILE_LIST))
MAKEDIR = $(dir $(abspath $(MAKEFILE)))
ROOTDIR = $(abspath $(MAKEDIR)/..)
UNIXDIR = $(ROOTDIR)/Unix
NETCOREDIR = $(ROOTDIR)/NetCore
NETSTANDARDDIR = $(ROOTDIR)/NetStandard

.PHONY: all test clean buildv8 cleanv8 zapv8 buildall cleanall

all:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript.Core/ClearScript.Core.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript.V8/ClearScript.V8.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETSTANDARDDIR)/ClearScript.Core/ClearScript.Core.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(NETSTANDARDDIR)/ClearScript.V8/ClearScript.V8.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptConsole/ClearScriptConsole.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptBenchmarks/ClearScriptBenchmarks.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest.NetStandard/ClearScriptTest.NetStandard.csproj

test: all
	dotnet test --nologo --no-build --verbosity normal -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj
	dotnet test --nologo --no-build --verbosity normal -c $(CONFIG) $(UNIXDIR)/ClearScriptTest.NetStandard/ClearScriptTest.NetStandard.csproj

clean:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile clean
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript.Core/ClearScript.Core.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETCOREDIR)/ClearScript.V8/ClearScript.V8.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETSTANDARDDIR)/ClearScript.Core/ClearScript.Core.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(NETSTANDARDDIR)/ClearScript.V8/ClearScript.V8.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptConsole/ClearScriptConsole.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptBenchmarks/ClearScriptBenchmarks.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj
	dotnet clean --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest.NetStandard/ClearScriptTest.NetStandard.csproj

buildv8:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile buildv8

cleanv8:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile cleanv8

zapv8:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile zapv8

buildall:
	make CPU=x64
	make CPU=arm64
	$(MAKELINUX) CPU=arm
	make CPU=x64 DEBUG=1
	make CPU=arm64 DEBUG=1
	$(MAKELINUX) CPU=arm DEBUG=1

cleanall:
	make clean CPU=x64
	make clean CPU=arm64
	$(MAKELINUX) clean CPU=arm
	make clean CPU=x64 DEBUG=1
	make clean CPU=arm64 DEBUG=1
	$(MAKELINUX) clean CPU=arm DEBUG=1
