ifdef DEBUG
	CONFIG = Debug
	CXXCONFIGFLAGS = -O0 -g
else
	CONFIG = Release
	CXXCONFIGFLAGS = -O3
endif

ifeq ($(shell uname -s), Darwin)
	EXTENSION=.dylib
else
	EXTENSION=.so
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
V8OUTDIR = $(V8DIR)/out/Unix/$(CONFIG)
V8LIBDIR = $(V8OUTDIR)/obj

OUTDIR = $(ROOTDIR)/bin/$(CONFIG)/Unix
OBJDIR = $(OUTDIR)/obj
CLEARSCRIPTV8 = $(OUTDIR)/ClearScriptV8$(EXTENSION)

CXX = clang++
CXXFLAGS = -std=c++17 -fvisibility=hidden -fPIC -fno-rtti $(CXXCONFIGFLAGS) -I$(V8INCDIR)

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

.PHONY: all objdir buildv8 clean cleanv8 zapv8

all: objdir $(CLEARSCRIPTV8)

objdir:
	@mkdir -p $(OBJDIR)

$(CLEARSCRIPTV8): $(V8LIBDIR)/libv8_monolith.a $(OBJECTS)
	$(CXX) $(CXXFLAGS) --shared -L $(V8LIBDIR) $(OBJECTS) -o $@ -lv8_monolith -lpthread

$(OBJDIR)/%.o: $(SRCDIR)/%.cpp $(HEADERS)
	$(CXX) $(CXXFLAGS) -c $< -o $@

$(V8LIBDIR)/libv8_monolith.a:
	cd $(UNIXDIR); ./V8Update.sh -n -y $(CONFIG)

buildv8:
	cd $(UNIXDIR); ./V8Update.sh -n -y $(CONFIG)

clean:
	rm -rf $(CLEARSCRIPTV8) $(OBJDIR)

cleanv8:
	rm -rf $(V8OUTDIR)

zapv8:
	rm -rf $(V8BUILDDIR)
