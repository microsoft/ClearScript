// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8ScriptImpl : V8.V8Script
    {
        private V8EntityHolder holder;

        public V8Script.Handle Handle => (V8Script.Handle)holder.Handle;

        public V8ScriptImpl(UniqueDocumentInfo documentInfo, UIntPtr codeDigest, V8Script.Handle hScript)
            : base(documentInfo, codeDigest)
        {
            holder = new V8EntityHolder("V8 compiled script", () => hScript);
        }

        #region disposal / finalization

        public override void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~V8ScriptImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}