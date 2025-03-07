// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides the base implementation for a host event source.
    /// </summary>
    public abstract class EventSource
    {
        internal EventSource(ScriptEngine engine, object source, EventInfo eventInfo)
        {
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));
            MiscHelpers.VerifyNonNullArgument(eventInfo, nameof(eventInfo));

            Engine = engine;
            Source = source;
            EventInfo = eventInfo;
        }

        internal ScriptEngine Engine { get; }

        internal abstract Type HandlerType { get; }

        internal object Source { get; }

        internal EventInfo EventInfo { get; }

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Connects the host event source to the specified script handler function.
        /// </summary>
        /// <param name="scriptFunc">The script function that will handle the event.</param>
        /// <returns>An <c><see cref="EventConnection"/></c> that represents the connection.</returns>
        public EventConnection connect(object scriptFunc)
        {
            MiscHelpers.VerifyNonNullArgument(scriptFunc, nameof(scriptFunc));
            return Engine.CreateEventConnection(HandlerType, Source, EventInfo, DelegateFactory.CreateDelegate(Engine, scriptFunc, HandlerType));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }

    /// <summary>
    /// Represents a host event source.
    /// </summary>
    /// <typeparam name="T">The event handler delegate type.</typeparam>
    public sealed class EventSource<T> : EventSource
    {
        internal EventSource(ScriptEngine engine, object source, EventInfo eventInfo)
            : base(engine, source, eventInfo)
        {
            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type (handler type mismatch)", nameof(eventInfo));
            }
        }

        #region EventSource overrides

        internal override Type HandlerType => typeof(T);

        #endregion

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Connects the host event source to the specified script handler function.
        /// </summary>
        /// <param name="scriptFunc">The script function that will handle the event.</param>
        /// <returns>An <c><see cref="EventConnection{T}"/></c> that represents the connection.</returns>
        public new EventConnection<T> connect(object scriptFunc)
        {
            MiscHelpers.VerifyNonNullArgument(scriptFunc, nameof(scriptFunc));
            return Engine.CreateEventConnection<T>(Source, EventInfo, DelegateFactory.CreateDelegate(Engine, scriptFunc, typeof(T)));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }

    /// <summary>
    /// Provides the base implementation for a connection between a host event source and a script handler function.
    /// </summary>
    public abstract class EventConnection
    {
        private readonly ScriptEngine engine;
        private readonly object source;
        private readonly MethodInfo removeMethod;
        private readonly object[] parameters;
        private readonly InterlockedOneWayFlag brokenFlag = new();

        internal EventConnection(ScriptEngine engine, object source, EventInfo eventInfo, Delegate handler)
        {
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));
            MiscHelpers.VerifyNonNullArgument(handler, nameof(handler));
            MiscHelpers.VerifyNonNullArgument(eventInfo, nameof(eventInfo));

            if (!MiscHelpers.Try(out var addMethod, static eventInfo => eventInfo.GetAddMethod(true), eventInfo) || (addMethod is null))
            {
                throw new ArgumentException("Invalid event type (no accessible add method)", nameof(eventInfo));
            }

            if (!MiscHelpers.Try(out removeMethod, static eventInfo => eventInfo.GetRemoveMethod(true), eventInfo) || (removeMethod is null))
            {
                throw new ArgumentException("Invalid event type (no accessible remove method)", nameof(eventInfo));
            }

            this.engine = engine;
            this.source = source;

            addMethod.Invoke(source, parameters = new object[] { handler });
        }

        internal void Break()
        {
            if (brokenFlag.Set())
            {
                removeMethod.Invoke(source, parameters);
            }
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
    }

    /// <summary>
    /// Represents a connection between a host event source and a script handler function.
    /// </summary>
    /// <typeparam name="T">The event handler delegate type.</typeparam>
    public sealed class EventConnection<T> : EventConnection
    {
        internal EventConnection(ScriptEngine engine, object source, EventInfo eventInfo, Delegate handler)
            : base(engine, source, eventInfo, handler)
        {
            if (eventInfo.EventHandlerType != typeof(T))
            {
                throw new ArgumentException("Invalid event type (handler type mismatch)", nameof(eventInfo));
            }
        }
    }
}
