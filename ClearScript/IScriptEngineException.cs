// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines common script engine exception properties.
    /// </summary>
    public interface IScriptEngineException
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets an <c><see href="http://en.wikipedia.org/wiki/HRESULT">HRESULT</see></c> error code if one is available, zero otherwise.
        /// </summary>
        int HResult { get; }

        /// <summary>
        /// Gets the name associated with the script engine instance.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// Gets a detailed error message if one is available, <c>null</c> otherwise.
        /// </summary>
        string ErrorDetails { get; }

        /// <summary>
        /// Gets a value that indicates whether the exception represents a fatal error.
        /// </summary>
        bool IsFatal { get; }

        /// <summary>
        /// Gets a value that indicates whether script code execution had started before the current exception was thrown.
        /// </summary>
        bool ExecutionStarted { get; }

        /// <summary>
        /// Gets the script exception that caused the current exception to be thrown, or <c>null</c> if one was not specified.
        /// </summary>
        dynamic ScriptException { get; }

        /// <summary>
        /// Gets the script exception that caused the current exception to be thrown, or <c>null</c> if one was not specified, without engaging the dynamic infrastructure.
        /// </summary>
        object ScriptExceptionAsObject { get; }

        /// <summary>
        /// Gets the host exception that caused the current exception to be thrown, or <c>null</c> if one was not specified.
        /// </summary>
        Exception InnerException { get; }
    }
}
