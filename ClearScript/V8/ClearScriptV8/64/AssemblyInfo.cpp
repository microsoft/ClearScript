// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "VersionSymbols.h"

using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;

[assembly:AssemblyTitle("ClearScript V8 Interface (64-bit)")];
[assembly:AssemblyProduct("ClearScript")];
[assembly:AssemblyCopyright("(c) Microsoft Corporation")]

[assembly:ComVisible(false)];
[assembly:AssemblyVersion(CLEARSCRIPT_VERSION_STRING)];
[assembly:AssemblyFileVersion(CLEARSCRIPT_VERSION_STRING)];
