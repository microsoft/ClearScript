// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Represents a compiled script that can be executed multiple times without recompilation.
    /// </summary>
    public abstract class V8Script : IDisposable
    {
        private readonly DocumentInfo documentInfo;

        internal V8Script(DocumentInfo documentInfo)
        {
            this.documentInfo = documentInfo;
        }

        /// <summary>
        /// Gets the document name associated with the compiled script.
        /// </summary>
        [Obsolete("Use DocumentInfo instead.")]
        public string Name
        {
            get { return documentInfo.UniqueName; }
        }

        /// <summary>
        /// Gets the document information associated with the compiled script.
        /// </summary>
        public DocumentInfo DocumentInfo
        {
            get { return documentInfo; }
        }

        #region IDisposable implementation (abstract)

        /// <summary>
        /// Releases all resources used by the compiled script.
        /// </summary>
        /// <remarks>
        /// Call <c>Dispose()</c> when you are finished using the compiled script. <c>Dispose()</c>
        /// leaves the compiled script in an unusable state. After calling <c>Dispose()</c>, you
        /// must release all references to the compiled script so the garbage collector can reclaim
        /// the memory that the compiled script was occupying.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This class is almost purely abstract; the implementation class uses the C++/CLI disposal pattern.")]
        public abstract void Dispose();

        #endregion
    }
}
