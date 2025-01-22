// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    internal enum DocumentKind
    {
        // IMPORTANT: maintain bitwise equivalence with native enum DocumentKind
        Script,
        JavaScriptModule,
        CommonJSModule,
        Json
    }
}
