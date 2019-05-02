// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// common platform headers
//-----------------------------------------------------------------------------

#include "CommonPlatform.h"

//-----------------------------------------------------------------------------
// C++ support library headers
//-----------------------------------------------------------------------------

#include <msclr\all.h>

//-----------------------------------------------------------------------------
// undefine legacy macros
//-----------------------------------------------------------------------------

#undef min
#undef max

//-----------------------------------------------------------------------------
// assembly references
//-----------------------------------------------------------------------------

#using "ClearScript.dll" as_friend

//-----------------------------------------------------------------------------
// namespace references
//-----------------------------------------------------------------------------

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Collections::ObjectModel;
using namespace System::Globalization;
using namespace System::Runtime::InteropServices;
using namespace System::Text;
using namespace System::Threading;
using namespace Microsoft::ClearScript;
using namespace Microsoft::ClearScript::JavaScript;
using namespace Microsoft::ClearScript::Util;
using namespace Microsoft::ClearScript::V8;

//-----------------------------------------------------------------------------
// global macros
//-----------------------------------------------------------------------------

#define BEGIN_LOCK_SCOPE(OBJECT) \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        msclr::lock t_Lock(OBJECT); \
    __pragma(warning(default:4456))

#define END_LOCK_SCOPE \
        IGNORE_UNUSED(t_Lock); \
    }

//-----------------------------------------------------------------------------

#define ENSURE_INTERNAL_CLASS(NAME) \
    public ref class NAME##Anchor sealed \
    { \
    private: \
        static String^ m_gcName = NAME::typeid->Name; \
    };
