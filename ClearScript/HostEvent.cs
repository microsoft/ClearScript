// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
