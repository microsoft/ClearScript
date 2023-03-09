ifdef DEBUG
    CONFIG = Debug
else
    CONFIG = Release
endif

KERNEL = $(shell uname -s)
ifeq ($(KERNEL), Darwin)
    MAKELINUX = true
else ifeq ($(KERNEL), Linux)
    MAKELINUX = make
else
    $(error unsupported operating system '$(KERNEL)')
endif

ARCH = $(shell uname -m)
ifeq ($(ARCH), i386)
    HOSTCPU = x86
else ifeq ($(ARCH), x86_64)
    HOSTCPU = x64
else ifeq ($(ARCH), arm)
    HOSTCPU = arm
else ifeq ($(ARCH), arm32)
    HOSTCPU = arm
else ifeq ($(ARCH), aarch32)
    HOSTCPU = arm
else ifeq ($(ARCH), arm64)
    HOSTCPU = arm64
else ifeq ($(ARCH), aarch64)
    HOSTCPU = arm64
else
    $(error unsupported machine architecture '$(ARCH)')
endif

ifeq ($(HOSTCPU), x64)
    OTHERCPU = arm64
else ifeq ($(HOSTCPU), arm64)
    OTHERCPU = x64
else
    $(error unsupported build machine architecture '$(ARCH)')
endif

MAKEFILE = $(word $(words $(MAKEFILE_LIST)), $(MAKEFILE_LIST))
MAKEDIR = $(dir $(abspath $(MAKEFILE)))
ROOTDIR = $(abspath $(MAKEDIR)/..)
UNIXDIR = $(ROOTDIR)/Unix
NETCOREDIR = $(ROOTDIR)/NetCore
NETSTANDARDDIR = $(ROOTDIR)/NetStandard

.PHONY: all test clean buildv8 cleanv8 zapv8 buildallrelease buildalldebug buildall cleanallrelease cleanalldebug cleanall

all:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptConsole/ClearScriptConsole.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptBenchmarks/ClearScriptBenchmarks.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj
	dotnet build --nologo --verbosity quiet -c $(CONFIG) $(UNIXDIR)/ClearScriptTest.NetStandard/ClearScriptTest.NetStandard.csproj

test: all
	dotnet test --nologo --no-build --verbosity normal -c $(CONFIG) $(UNIXDIR)/ClearScriptTest/ClearScriptTest.csproj
	dotnet test --nologo --no-build --verbosity normal -c $(CONFIG) $(UNIXDIR)/ClearScriptTest.NetStandard/ClearScriptTest.NetStandard.csproj

clean:
	make -f $(UNIXDIR)/ClearScriptV8/Makefile clean
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

buildallrelease:
	make CPU=$(HOSTCPU)
	make CPU=$(OTHERCPU)
	$(MAKELINUX) CPU=arm

buildalldebug:
	make CPU=$(HOSTCPU) DEBUG=1
	make CPU=$(OTHERCPU) DEBUG=1
	$(MAKELINUX) CPU=arm DEBUG=1

buildall: buildallrelease buildalldebug

cleanallrelease:
	make clean CPU=$(HOSTCPU)
	make clean CPU=$(OTHERCPU)
	$(MAKELINUX) clean CPU=arm

cleanalldebug:
	make clean CPU=$(HOSTCPU) DEBUG=1
	make clean CPU=$(OTHERCPU) DEBUG=1
	$(MAKELINUX) clean CPU=arm DEBUG=1

cleanall: cleanallrelease cleanalldebug
