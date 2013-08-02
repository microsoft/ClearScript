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
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a host event source.
    /// </summary>
    /// <typeparam name="T">The event handler delegate type.</typeparam>
    public class EventSource<T>
    {
        private readonly ScriptEngine engine;
        private readonly object source;
        private readonly EventInfo eventInfo;

        internal EventSource(ScriptEngine engine, object source, EventInfo eventInfo)
        {
            MiscHelpers.VerifyNonNullArgument(engine, "engine");
            MiscHelpers.VerifyNonNullArgument(eventInfo, "eventInfo");
            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type", "eventInfo");
            }

            this.engine = engine;
            this.source = source;
            this.eventInfo = eventInfo;
        }

        internal object Source
        {
            get { return source; }
        }

        internal EventInfo EventInfo
        {
            get { return eventInfo; }
        }

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Connects the host event source to the specified script handler function.
        /// </summary>
        /// <param name="scriptFunc">The script function that will handle the event.</param>
        /// <returns>An <see cref="EventConnection{T}"/> that represents the connection.</returns>
        public EventConnection<T> connect(object scriptFunc)
        {
            MiscHelpers.VerifyNonNullArgument(scriptFunc, "scriptFunc");
            var handler = DelegateFactory.CreateDelegate(engine, scriptFunc, eventInfo.EventHandlerType);
            eventInfo.AddEventHandler(source, handler);
            return new EventConnection<T>(source, eventInfo, handler);
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }

    /// <summary>
    /// Represents a connection between a host event source and a script handler function.
    /// </summary>
    /// <typeparam name="T">The event handler delegate type.</typeparam>
    public class EventConnection<T>
    {
        private object source;
        private EventInfo eventInfo;
        private Delegate handler;

        internal EventConnection(object source, EventInfo eventInfo, Delegate handler)
        {
            MiscHelpers.VerifyNonNullArgument(handler, "handler");
            MiscHelpers.VerifyNonNullArgument(eventInfo, "eventInfo");
            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type", "eventInfo");
            }

            this.source = source;
            this.eventInfo = eventInfo;
            this.handler = handler;
        }

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Disconnects the host event source from the script handler function.
        /// </summary>
        public void disconnect()
        {
            if (handler != null)
            {
                eventInfo.RemoveEventHandler(source, handler);
                source = null;
                eventInfo = null;
                handler = null;
            }
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
