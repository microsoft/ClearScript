ifdef DEBUG
    CONFIG = Debug
    CXXCONFIGFLAGS = -O0 -g -D_DEBUG
else
    CONFIG = Release
    CXXCONFIGFLAGS = -O3 -DNDEBUG
endif

KERNEL = $(shell uname -s)
ifeq ($(KERNEL), Darwin)
    OS = osx
    EXTENSION = dylib
    TARGETOS = darwin
    TARGETARCH_X86 = i386
    TARGETARCH_X64 = x86_64
    TARGETARCH_ARM = arm
    TARGETARCH_ARM64 = arm64
    MAKELINUX = true
else ifeq ($(KERNEL), Linux)
    OS = linux
    EXTENSION = so
    TARGETOS = linux
    TARGETARCH_X86 = i386
    TARGETARCH_X64 = x86_64
    TARGETARCH_ARM = arm
    TARGETARCH_ARM64 = aarch64
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

ifndef CPU
    CPU = $(HOSTCPU)
endif

ifeq ($(CPU), x86)
    TARGETARCH = $(TARGETARCH_X86)
else ifeq ($(CPU), x64)
    TARGETARCH = $(TARGETARCH_X64)
else ifeq ($(CPU), arm)
    TARGETARCH = $(TARGETARCH_ARM)
    TARGETSUFFIX = eabihf
else ifeq ($(CPU), arm64)
    TARGETARCH = $(TARGETARCH_ARM64)
else
    $(error unsupported target CPU '$(CPU)')
endif

STRIP = true
CODESIGN = true
ifeq ($(TARGETOS), linux)
    TARGET = $(TARGETARCH)-$(TARGETOS)-gnu$(TARGETSUFFIX)
    ifndef DEBUG
        CXXLINKFLAGS := -s
    endif
    CXXLINKFLAGS := $(CXXLINKFLAGS) -static-libstdc++ -static-libgcc -ldl -lrt --no-undefined
    ifneq ($(HOSTCPU), $(CPU))
        CXXCROSSFLAGS = -I/usr/$(TARGET)/include
        CXXLINKFLAGS := -fuse-ld=/usr/$(TARGET)/bin/ld $(CXXLINKFLAGS)
    endif
else ifeq ($(TARGETOS), darwin)
    TARGET = $(TARGETARCH)-$(TARGETOS)
    ifndef DEBUG
        STRIP = strip -r -u
        ENTITLEMENTS = $(MAKEDIR)/entitlements.xml
        CODESIGN = codesign --sign - --options linker-signed --entitlements $(ENTITLEMENTS) --force
    endif
else
    $(error unsupported target operating system '$(TARGETOS)')
endif

MAKEFILE = $(word $(words $(MAKEFILE_LIST)), $(MAKEFILE_LIST))
MAKEDIR = $(dir $(abspath $(MAKEFILE)))
ROOTDIR = $(abspath $(MAKEDIR)/../..)
SRCDIR = $(ROOTDIR)/ClearScriptV8
UNIXDIR = $(ROOTDIR)/Unix
EXPORTSDIR = $(ROOTDIR)/ClearScript/Exports

V8ROOTDIR = $(ROOTDIR)/V8
V8BUILDDIR = $(V8ROOTDIR)/build
V8DIR = $(V8BUILDDIR)/v8
V8INCDIR = $(V8DIR)/include
V8OUTDIR = $(V8DIR)/out/$(CPU)/$(CONFIG)
V8LIBDIR = $(V8OUTDIR)/obj

JSONDIR = $(V8BUILDDIR)/json
JSONINCDIR = $(JSONDIR)/single_include

OUTDIR = $(ROOTDIR)/bin/$(CONFIG)/Unix
OBJDIR = $(OUTDIR)/obj/$(CPU)
CLEARSCRIPTV8 = $(OUTDIR)/ClearScriptV8.$(OS)-$(CPU).$(EXTENSION)

CXX = clang++
CXXFLAGS = --target=$(TARGET) -std=c++20 -fvisibility=default -fPIC -fno-rtti -Wno-deprecated-declarations -Wno-ignored-attributes $(CXXCONFIGFLAGS) -I$(V8INCDIR) -I$(JSONINCDIR) $(CXXCROSSFLAGS)

HEADERS = $(wildcard $(SRCDIR)/*.h) $(wildcard $(EXPORTSDIR)/*.h)
OBJECTS = \
    $(OBJDIR)/HighResolutionClock.o \
    $(OBJDIR)/HighResolutionClock.Unix.o \
    $(OBJDIR)/HostObjectHolderImpl.o \
    $(OBJDIR)/HostObjectUtil.o \
    $(OBJDIR)/Mutex.o \
    $(OBJDIR)/RefCount.o \
    $(OBJDIR)/StdString.o \
    $(OBJDIR)/V8Context.o \
    $(OBJDIR)/V8ContextImpl.o \
    $(OBJDIR)/V8Isolate.o \
    $(OBJDIR)/V8IsolateImpl.o \
    $(OBJDIR)/V8ObjectHelpers.o \
    $(OBJDIR)/V8ObjectHolderImpl.o \
    $(OBJDIR)/V8ScriptHolderImpl.o \
    $(OBJDIR)/V8SplitProxyManaged.o \
    $(OBJDIR)/V8SplitProxyNative.o

.PHONY: all objdir clean buildv8 cleanv8 zapv8 buildallrelease buildalldebug buildall cleanallrelease cleanalldebug cleanall

all: objdir buildv8 $(CLEARSCRIPTV8)

objdir:
	@mkdir -p $(OBJDIR)

$(CLEARSCRIPTV8): $(OBJECTS) $(V8LIBDIR)/libv8_monolith.a $(ENTITLEMENTS)
	$(CXX) $(CXXFLAGS) $(CXXLINKFLAGS) --shared -L$(V8LIBDIR) $(OBJECTS) -o $@ -pthread -lv8_monolith && $(STRIP) $@ && $(CODESIGN) $@

$(OBJDIR)/%.o: $(SRCDIR)/%.cpp $(HEADERS)
	$(CXX) $(CXXFLAGS) -c $< -o $@

clean:
	rm -rf $(CLEARSCRIPTV8) $(OBJDIR)

buildv8:
	cd $(UNIXDIR); ./V8Update.sh -n -y $(CPU) $(CONFIG)

cleanv8:
	rm -rf $(V8OUTDIR)

zapv8:
	rm -rf $(V8BUILDDIR)

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
