// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Runtime.Serialization;

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

        #region constructors

        /// <summary>
        /// Initializes a new <see cref="ScriptInterruptedException"/> instance.
        /// </summary>
        public ScriptInterruptedException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptInterruptedException"/> with the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ScriptInterruptedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptInterruptedException"/> with the specified error message and nested exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception to be thrown.</param>
        public ScriptInterruptedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptInterruptedException"/> with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ScriptInterruptedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            engineName = info.GetString(engineNameItemName);
            errorDetails = info.GetString(errorDetailsItemName);
            isFatal = info.GetBoolean(isFatalItemName);
        }

        internal ScriptInterruptedException(string engineName, string message, string errorDetails, int errorCode, bool isFatal, Exception innerException)
            : base(message, innerException)
        {
            this.engineName = engineName;
            this.errorDetails = errorDetails;
            this.isFatal = isFatal;

            if (errorCode != 0)
            {
                HResult = errorCode;
            }
        }

        #endregion

        #region IScriptEngineException implementation

        /// <summary>
        /// Gets an <see href="http://en.wikipedia.org/wiki/HRESULT">HRESULT</see> error code if one is available, zero otherwise.
        /// </summary>
        int IScriptEngineException.HResult
        {
            get { return HResult; }
        }

        /// <summary>
        /// Gets the name associated with the script engine instance.
        /// </summary>
        public string EngineName
        {
            get { return engineName; }
        }

        /// <summary>
        /// Gets a detailed error message if one is available, <c>null</c> otherwise.
        /// </summary>
        public string ErrorDetails
        {
            get { return errorDetails; }
        }

        /// <summary>
        /// Gets a value that indicates whether the exception represents a fatal error.
        /// </summary>
        public bool IsFatal
        {
            get { return isFatal; }
        }

        #endregion

        #region OperationCanceledException overrides

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(engineNameItemName, engineName);
            info.AddValue(errorDetailsItemName, errorDetails);
            info.AddValue(isFatalItemName, isFatal);
        }

        #endregion
    }
}
