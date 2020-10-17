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

        internal EventSource(ScriptEngine engine, object source, EventInfo eventInfo)
        {
            MiscHelpers.VerifyNonNullArgument(engine, "engine");
            MiscHelpers.VerifyNonNullArgument(eventInfo, "eventInfo");

            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type", nameof(eventInfo));
            }

            this.engine = engine;
            Source = source;
            EventInfo = eventInfo;
        }

        internal object Source { get; }

        internal EventInfo EventInfo { get; }

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
            return engine.CreateEventConnection<T>(Source, EventInfo, DelegateFactory.CreateDelegate(engine, scriptFunc, typeof(T)));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }

    internal interface IEventConnection
    {
        void Break();
    }

    /// <summary>
    /// Represents a connection between a host event source and a script handler function.
    /// </summary>
    /// <typeparam name="T">The event handler delegate type.</typeparam>
    public class EventConnection<T> : IEventConnection
    {
        private readonly ScriptEngine engine;
        private readonly object source;
        private readonly EventInfo eventInfo;
        private readonly Delegate handler;
        private readonly InterlockedOneWayFlag brokenFlag = new InterlockedOneWayFlag();

        internal EventConnection(ScriptEngine engine, object source, EventInfo eventInfo, Delegate handler)
        {
            MiscHelpers.VerifyNonNullArgument(engine, "engine");
            MiscHelpers.VerifyNonNullArgument(handler, "handler");
            MiscHelpers.VerifyNonNullArgument(eventInfo, "eventInfo");

            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type", nameof(eventInfo));
            }

            this.engine = engine;
            this.source = source;
            this.eventInfo = eventInfo;
            this.handler = handler;

            eventInfo.AddEventHandler(source, handler);
        }

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Disconnects the host event source from the script handler function.
        /// </summary>
        public void disconnect()
        {
            engine.BreakEventConnection(this);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region IEventConnection implementation

        void IEventConnection.Break()
        {
            if (brokenFlag.Set())
            {
                eventInfo.RemoveEventHandler(source, handler);
            }
        }

        #endregion 
    }
}
