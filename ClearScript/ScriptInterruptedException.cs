// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// The exception that is thrown when script execution is interrupted by the host.
    /// </summary>
    [Serializable]
    public class ScriptInterruptedException : OperationCanceledException, IScriptEngineException
    {
        private readonly string engineName;
        private const string engineNameItemName = "ScriptEngineName";

        private readonly string errorDetails;
        private const string errorDetailsItemName = "ScriptErrorDetails";

        private readonly bool isFatal;
        private const string isFatalItemName = "IsFatal";

        private readonly bool executionStarted;
        private const string executionStartedItemName = "ExecutionStarted";

        private readonly object scriptException;
        private const string defaultMessage = "Script execution was interrupted";

        #region constructors

        /// <summary>
        /// Initializes a new <c><see cref="ScriptInterruptedException"/></c> instance.
        /// </summary>
        public ScriptInterruptedException()
            : base(defaultMessage)
        {
            errorDetails = base.Message;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptInterruptedException"/></c> with the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ScriptInterruptedException(string message)
            : base(message.ToNonBlank(defaultMessage))
        {
            errorDetails = base.Message;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptInterruptedException"/></c> with the specified error message and nested exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception to be thrown.</param>
        public ScriptInterruptedException(string message, Exception innerException)
            : base(message.ToNonBlank(defaultMessage), innerException)
        {
            errorDetails = base.Message;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptInterruptedException"/></c> with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ScriptInterruptedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            engineName = info.GetString(engineNameItemName);
            errorDetails = info.GetString(errorDetailsItemName);
            isFatal = info.GetBoolean(isFatalItemName);
            executionStarted = info.GetBoolean(executionStartedItemName);
        }

        internal ScriptInterruptedException(string engineName, string message, string errorDetails, int errorCode, bool isFatal, bool executionStarted, object scriptException, Exception innerException)
            : base(message.ToNonBlank(defaultMessage), innerException)
        {
            this.engineName = engineName;
            this.errorDetails = errorDetails.ToNonBlank(base.Message);
            this.isFatal = isFatal;
            this.executionStarted = executionStarted;
            this.scriptException = scriptException;

            if (errorCode != 0)
            {
                HResult = errorCode;
            }
        }

        #endregion

        #region IScriptEngineException implementation

        /// <summary>
        /// Gets an <c><see href="http://en.wikipedia.org/wiki/HRESULT">HRESULT</see></c> error code if one is available, zero otherwise.
        /// </summary>
        int IScriptEngineException.HResult => HResult;

        /// <summary>
        /// Gets the name associated with the script engine instance.
        /// </summary>
        public string EngineName => engineName;

        /// <summary>
        /// Gets a detailed error message if one is available, <c>null</c> otherwise.
        /// </summary>
        public string ErrorDetails => errorDetails;

        /// <summary>
        /// Gets a value that indicates whether the exception represents a fatal error.
        /// </summary>
        public bool IsFatal => isFatal;

        /// <summary>
        /// Gets a value that indicates whether script code execution had started before the current exception was thrown.
        /// </summary>
        public bool ExecutionStarted => executionStarted;

        /// <summary>
        /// Gets the script exception that caused the current exception to be thrown, or <c>null</c> if one was not specified.
        /// </summary>
        public dynamic ScriptException => scriptException;

        /// <summary>
        /// Gets the script exception that caused the current exception to be thrown, or <c>null</c> if one was not specified, without engaging the dynamic infrastructure.
        /// </summary>
        public object ScriptExceptionAsObject => scriptException;

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a string that represents the current exception.
        /// </summary>
        /// <returns>A string that represents the current exception.</returns>
        public override string ToString()
        {
            var result = base.ToString();

            if (!string.IsNullOrEmpty(errorDetails) && (errorDetails != Message))
            {
                var details = "   " + errorDetails.Replace("\n", "\n   ");
                result += "\n   --- Script error details follow ---\n" + details;
            }

            return result;
        }

        #endregion

        #region OperationCanceledException overrides

        /// <summary>
        /// Populates a <c><see cref="SerializationInfo"/></c> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <c><see cref="SerializationInfo"/></c> to populate with data.</param>
        /// <param name="context">The destination (see <c><see cref="StreamingContext"/></c>) for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(engineNameItemName, engineName);
            info.AddValue(errorDetailsItemName, errorDetails);
            info.AddValue(isFatalItemName, isFatal);
            info.AddValue(executionStartedItemName, executionStarted);
        }

        #endregion
    }
}
