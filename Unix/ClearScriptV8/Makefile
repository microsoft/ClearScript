ifdef DEBUG
    CONFIG = Debug
    CXXCONFIGFLAGS = -O0 -g
else
    CONFIG = Release
    CXXCONFIGFLAGS = -O3
endif

KERNEL = $(shell uname -s)

TARGETSUFFIX = gnu
TARGETARCHSUFFIX =
TARGETSUFFIXVER =
UPDATEFLAGS =

ifeq ($(KERNEL), Darwin)
    OS = osx
    EXTENSION = dylib
    TARGETOS = darwin
    TARGETARCH_X86 = i386
    TARGETARCH_X64 = x86_64
    TARGETARCH_ARM = arm
    TARGETARCH_ARM64 = arm64
    MAKELINUX = true
    ifdef ANDROID
        $(error Linux is required to build android)
    endif
else ifeq ($(KERNEL), Linux)
    OS = linux
    EXTENSION = so
    TARGETOS = linux
    TARGETARCH_X86 = i386
    TARGETARCH_X64 = x86_64
    TARGETARCH_ARM = arm
    TARGETARCH_ARM64 = aarch64
    MAKELINUX = make
    
    ifdef ANDROID
        OS = android
        TARGETARCH_X86 = i686
        TARGETARCH_X64 = x86_64
        TARGETARCH_ARM = arm
        TARGETARCH_ARM64 = aarch64
        TARGETSUFFIX = android
        TARGETSUFFIXVER = 21
        UPDATEFLAGS = -a
    endif
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
    
ifndef CPU
    CPU = $(HOSTCPU)
endif

ifeq ($(CPU), x86)
    TARGETARCH = $(TARGETARCH_X86)
else ifeq ($(CPU), x64)
    TARGETARCH = $(TARGETARCH_X64)
else ifeq ($(CPU), arm)
    TARGETARCH = $(TARGETARCH_ARM)
    TARGETSUFFIX = gnueabihf
    ifdef ANDROID
        TARGETARCHSUFFIX = v7a
        TARGETSUFFIX = androideabi
    endif
else ifeq ($(CPU), arm64)
    TARGETARCH = $(TARGETARCH_ARM64)
else
    $(error unsupported target CPU '$(CPU)')
endif


MAKEFILE = $(word $(words $(MAKEFILE_LIST)), $(MAKEFILE_LIST))
MAKEDIR = $(dir $(abspath $(MAKEFILE)))
ROOTDIR = $(abspath $(MAKEDIR)/../..)
SRCDIR = $(ROOTDIR)/ClearScriptV8
UNIXDIR = $(ROOTDIR)/Unix

V8ROOTDIR = $(ROOTDIR)/V8
V8BUILDDIR = $(V8ROOTDIR)/build
V8DIR = $(V8BUILDDIR)/v8
V8INCDIR = $(V8DIR)/include
V8OUTDIR = $(V8DIR)/out/$(CPU)/$(CONFIG)

ifdef ANDROID
    V8OUTDIR = $(V8DIR)/out/android_$(CPU)/$(CONFIG)
endif

V8LIBDIR = $(V8OUTDIR)/obj
CXX = clang++

NDK = $(V8DIR)/third_party/android_ndk
LLVM = $(NDK)/toolchains/llvm/prebuilt/$(TARGETOS)-$(ARCH)
TOOLS = $(TARGETARCH)-$(TARGETOS)-$(TARGETSUFFIX)

STRIP = true
ifeq ($(TARGETOS), linux)
    TARGET = $(TARGETARCH)$(TARGETARCHSUFFIX)-$(TARGETOS)-$(TARGETSUFFIX)$(TARGETSUFFIXVER)
    ifndef DEBUG
        CXXLINKFLAGS := -s
    endif
    CXXLINKFLAGS := $(CXXLINKFLAGS) -static-libstdc++ -static-libgcc
    ifdef ANDROID
        CXXLINKFLAGS := -fuse-ld=$(LLVM)/$(TOOLS)/bin/ld $(CXXLINKFLAGS)
        CXX = $(LLVM)/bin/$(TARGET)-clang++
    else ifneq ($(HOSTCPU), $(CPU))
        CXXCROSSFLAGS = -I/usr/$(TARGET)/include
        CXXLINKFLAGS := -fuse-ld=/usr/$(TARGET)/bin/ld $(CXXLINKFLAGS)
    endif
else ifeq ($(TARGETOS), darwin)
    TARGET = $(TARGETARCH)-$(TARGETOS)
    ifndef DEBUG
        STRIP = strip -r -u
    endif
else
    $(error unsupported target operating system '$(TARGETOS)')
endif

JSONDIR = $(V8BUILDDIR)/json
JSONINCDIR = $(JSONDIR)/single_include

OUTDIR = $(ROOTDIR)/bin/$(CONFIG)/Unix
OBJDIR = $(OUTDIR)/obj/$(CPU)
CLEARSCRIPTV8 = $(OUTDIR)/ClearScriptV8.$(OS)-$(CPU).$(EXTENSION)

CXXFLAGS = --target=$(TARGET) -std=c++17 -fvisibility=default -fPIC -fno-rtti -Wno-ignored-attributes $(CXXCONFIGFLAGS) -I$(V8INCDIR) -I$(JSONINCDIR) $(CXXCROSSFLAGS)

HEADERS = $(wildcard $(SRCDIR)/*.h)
OBJECTS = \
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

.PHONY: all objdir clean buildv8 cleanv8 zapv8 buildall cleanall

all: objdir $(CLEARSCRIPTV8)

objdir:
	@mkdir -p $(OBJDIR)

$(CLEARSCRIPTV8): $(V8LIBDIR)/libv8_monolith.a $(OBJECTS)
	$(CXX) $(CXXFLAGS) $(CXXLINKFLAGS) --shared -L$(V8LIBDIR) $(OBJECTS) -o $@ -pthread -lv8_monolith && $(STRIP) $@

$(OBJDIR)/%.o: $(SRCDIR)/%.cpp $(HEADERS)
	$(CXX) $(CXXFLAGS) -c $< -o $@

$(V8LIBDIR)/libv8_monolith.a:
	cd $(UNIXDIR); ./V8Update.sh -n -y $(UPDATEFLAGS) $(CPU) $(CONFIG)

clean:
	rm -rf $(CLEARSCRIPTV8) $(OBJDIR)

buildv8:
	cd $(UNIXDIR); ./V8Update.sh -n -y $(UPDATEFLAGS) $(CPU) $(CONFIG)

cleanv8:
	rm -rf $(V8OUTDIR)

zapv8:
	rm -rf $(V8BUILDDIR)

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
