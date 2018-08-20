// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

struct V8DocumentInfo
{
    StdString ResourceName;
    StdString SourceMapUrl;

#ifdef _M_CEE

    explicit V8DocumentInfo(DocumentInfo documentInfo):
		ResourceName(MiscHelpers::GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName)),
		SourceMapUrl(MiscHelpers::GetUrlOrPath(documentInfo.SourceMapUri, String::Empty))
	{
    }

#endif // _M_CEE

};
