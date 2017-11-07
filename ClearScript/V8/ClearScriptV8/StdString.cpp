// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// StdString implementation
//-----------------------------------------------------------------------------

std::wstring StdString::GetValue(const v8_inspector::StringView& stringView)
{
    auto length = stringView.length();
    if (length < 1)
    {
        return std::wstring();
    }

    if (!stringView.is8Bit())
    {
        return std::wstring(stringView.characters16(), length);
    }

    auto pFirst = reinterpret_cast<const char*>(stringView.characters8());
    return std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>>().from_bytes(pFirst, pFirst + length);
}
