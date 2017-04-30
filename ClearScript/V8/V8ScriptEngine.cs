// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Represents an instance of the V8 JavaScript engine.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="WindowsScriptEngine"/> instances, V8ScriptEngine instances do not have
    /// thread affinity. The underlying script engine is not thread-safe, however, so this class
    /// uses internal locks to automatically serialize all script code execution for a given
    /// instance. Script delegates and event handlers are invoked on the calling thread without
    /// marshaling.
    /// </remarks>
    public sealed class V8ScriptEngine : ScriptEngine
    {
        #region data

        private readonly V8ScriptEngineFlags engineFlags;
        private readonly V8ContextProxy proxy;
        private readonly object script;
        private InterlockedDisposedFlag disposedFlag = new InterlockedDisposedFlag();

        private const int continuationInterval = 2000;
        private bool inContinuationTimerScope;

        private readonly HostItemCollateral hostItemCollateral;
        private readonly IUniqueNameManager documentNameManager = new UniqueFileNameManager();
        private List<string> documentNames;
        private bool suppressExtensionMethodEnumeration;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new V8 script engine instance.
        /// </summary>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified resource constraints.
        /// </summary>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(V8RuntimeConstraints constraints)
            : this(null, constraints)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name and resource constraints.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8RuntimeConstraints constraints)
            : this(name, constraints, V8ScriptEngineFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(V8ScriptEngineFlags flags)
            : this(flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified options and debug port.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(V8ScriptEngineFlags flags, int debugPort)
            : this(null, null, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8ScriptEngineFlags flags)
            : this(name, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8ScriptEngineFlags flags, int debugPort)
            : this(name, null, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified resource constraints and options.
        /// </summary>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(V8RuntimeConstraints constraints, V8ScriptEngineFlags flags)
            : this(constraints, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified resource constraints, options, and debug port.
        /// </summary>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(V8RuntimeConstraints constraints, V8ScriptEngineFlags flags, int debugPort)
            : this(null, constraints, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name, resource constraints, and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8RuntimeConstraints constraints, V8ScriptEngineFlags flags)
            : this(name, constraints, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 script engine instance with the specified name, resource constraints, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the V8 runtime (see remarks).</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8RuntimeConstraints constraints, V8ScriptEngineFlags flags, int debugPort)
            : this(null, name, constraints, flags, debugPort)
        {
        }

        internal V8ScriptEngine(V8Runtime runtime, string name, V8RuntimeConstraints constraints, V8ScriptEngineFlags flags, int debugPort)
            : base((runtime != null) ? runtime.Name + ":" + name : name)
        {
            using (var localRuntime = (runtime != null) ? null : new V8Runtime(name, constraints))
            {
                var activeRuntime = runtime ?? localRuntime;
                hostItemCollateral = activeRuntime.HostItemCollateral;

                engineFlags = flags;
                proxy = V8ContextProxy.Create(activeRuntime.IsolateProxy, Name, flags.HasFlag(V8ScriptEngineFlags.EnableDebugging), flags.HasFlag(V8ScriptEngineFlags.DisableGlobalMembers), debugPort);
                script = GetRootItem();

                var engineInternal = Evaluate(
                    MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name),
                    false,
                    @"
                        EngineInternal = (function () {

                            function convertArgs(args) {
                                var result = [];
                                var count = args.Length;
                                for (var i = 0; i < count; i++) {
                                    result.push(args[i]);
                                }
                                return result;
                            }

                            function construct(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) {
                                return new this(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                            }

                            var isHostObjectKey = this.isHostObjectKey;
                            delete this.isHostObjectKey;

                            return {

                                getCommandResult: function (value) {
                                    if (value == null) {
                                        return value;
                                    }
                                    if (typeof(value.hasOwnProperty) != 'function') {
                                        return '[external]';
                                    }
                                    if (value[isHostObjectKey] === true) {
                                        return value;
                                    }
                                    if (typeof(value.toString) != 'function') {
                                        return '[' + typeof(value) + ']';
                                    }
                                    return value.toString();
                                },

                                invokeConstructor: function (constructor, args) {
                                    if (typeof(constructor) != 'function') {
                                        throw new Error('Function expected');
                                    }
                                    return construct.apply(constructor, convertArgs(args));
                                },

                                invokeMethod: function (target, method, args) {
                                    if (typeof(method) != 'function') {
                                        throw new Error('Function expected');
                                    }
                                    return method.apply(target, convertArgs(args));
                                },

                                getStackTrace: function () {
                                    try {
                                        throw new Error('[stack trace]');
                                    }
                                    catch (exception) {
                                        return exception.stack;
                                    }
                                    return '';
                                }
                            };
                        })();
                    "
                );

                ((IDisposable)engineInternal).Dispose();
            }
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets or sets a soft limit for the size of the V8 runtime's heap.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is specified in bytes. When it is set to the default value, heap size
        /// monitoring is disabled, and scripts with memory leaks or excessive memory usage
        /// can cause unrecoverable errors and process termination.
        /// </para>
        /// <para>
        /// A V8 runtime unconditionally terminates the process when it exceeds its resource
        /// constraints (see <see cref="V8RuntimeConstraints"/>). This property enables external
        /// heap size monitoring that can prevent termination in some scenarios. To be effective,
        /// it should be set to a value that is significantly lower than
        /// <see cref="V8RuntimeConstraints.MaxOldSpaceSize"/>. Note that enabling heap size
        /// monitoring results in slower script execution.
        /// </para>
        /// <para>
        /// Exceeding this limit causes the V8 runtime to interrupt script execution and throw an
        /// exception. To re-enable script execution, set this property to a new value.
        /// </para>
        /// </remarks>
        public UIntPtr MaxRuntimeHeapSize
        {
            get
            {
                VerifyNotDisposed();
                return proxy.MaxRuntimeHeapSize;
            }

            set
            {
                VerifyNotDisposed();
                proxy.MaxRuntimeHeapSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum time interval between consecutive heap size samples.
        /// </summary>
        /// <remarks>
        /// This property is effective only when heap size monitoring is enabled (see
        /// <see cref="MaxRuntimeHeapSize"/>).
        /// </remarks>
        public TimeSpan RuntimeHeapSizeSampleInterval
        {
            get
            {
                VerifyNotDisposed();
                return proxy.RuntimeHeapSizeSampleInterval;
            }

            set
            {
                VerifyNotDisposed();
                proxy.RuntimeHeapSizeSampleInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount by which the V8 runtime is permitted to grow the stack during script execution.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is specified in bytes. When it is set to the default value, no stack
        /// usage limit is enforced, and scripts with unchecked recursion or other excessive stack
        /// usage can cause unrecoverable errors and process termination.
        /// </para>
        /// <para>
        /// Note that the V8 runtime does not monitor stack usage while a host call is in progress.
        /// Monitoring is resumed when control returns to the runtime.
        /// </para>
        /// </remarks>
        public UIntPtr MaxRuntimeStackUsage
        {
            get
            {
                VerifyNotDisposed();
                return proxy.MaxRuntimeStackUsage;
            }

            set
            {
                VerifyNotDisposed();
                proxy.MaxRuntimeStackUsage = value;
            }
        }

        /// <summary>
        /// Enables or disables extension method enumeration.
        /// </summary>
        /// <remarks>
        /// By default, all exposed extension methods appear as enumerable properties of all host
        /// objects, regardless of type. Setting this property to <c>true</c> excludes all
        /// extension methods from the list of enumerable properties of all host objects. Note that
        /// doing so affects only property enumeration; extension methods remain both retrievable
        /// and invocable regardless of this property's value.
        /// </remarks>
        public bool SuppressExtensionMethodEnumeration
        {
            get { return suppressExtensionMethodEnumeration; }
            set
            {
                suppressExtensionMethodEnumeration = value;
                RebuildExtensionMethodSummary();
            }
        }

        /// <summary>
        /// Creates a compiled script.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        public V8Script Compile(string code)
        {
            return Compile(null, code);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        public V8Script Compile(string documentName, string code)
        {
            VerifyNotDisposed();

            return ScriptInvoke(() =>
            {
                var uniqueName = documentNameManager.GetUniqueName(documentName, "Script Document");
                return proxy.Compile(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code);
            });
        }

        /// <summary>
        /// Creates a compiled script, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 script
        /// engines and application processes. V8 script engines with debugging enabled cannot
        /// generate cache data.
        /// </remarks>
        /// <seealso cref="Compile(string, V8CacheKind, byte[], out bool)"/>
        public V8Script Compile(string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            return Compile(null, code, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 script
        /// engines and application processes. V8 script engines with debugging enabled cannot
        /// generate cache data.
        /// </remarks>
        /// <seealso cref="Compile(string, string, V8CacheKind, byte[], out bool)"/>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            VerifyNotDisposed();

            V8Script tempScript = null;
            cacheBytes = ScriptInvoke(() =>
            {
                byte[] tempCacheBytes;
                var uniqueName = documentNameManager.GetUniqueName(documentName, "Script Document");
                tempScript = proxy.Compile(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, cacheKind, out tempCacheBytes);
                return tempCacheBytes;
            });

            return tempScript;
        }

        /// <summary>
        /// Creates a compiled script, consuming previously generated cache data.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. V8 script engines with debugging enabled cannot consume cache data.
        /// </remarks>
        /// <seealso cref="Compile(string, V8CacheKind, out byte[])"/>
        public V8Script Compile(string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return Compile(null, code, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, consuming previously generated cache data.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. V8 script engines with debugging enabled cannot consume cache data.
        /// </remarks>
        /// <seealso cref="Compile(string, string, V8CacheKind, out byte[])"/>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            VerifyNotDisposed();

            V8Script tempScript = null;
            cacheAccepted = ScriptInvoke(() =>
            {
                bool tempCacheAccepted;
                var uniqueName = documentNameManager.GetUniqueName(documentName, "Script Document");
                tempScript = proxy.Compile(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, cacheKind, cacheBytes, out tempCacheAccepted);
                return tempCacheAccepted;
            });

            return tempScript;
        }

        // ReSharper disable ParameterHidesMember

        /// <summary>
        /// Evaluates a compiled script.
        /// </summary>
        /// <param name="script">The compiled script to evaluate.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// For information about the types of result values that script code can return, see
        /// <see cref="ScriptEngine.Evaluate(string, bool, string)"/>.
        /// </remarks>
        public object Evaluate(V8Script script)
        {
            return Execute(script, true);
        }

        /// <summary>
        /// Executes a compiled script.
        /// </summary>
        /// <param name="script">The compiled script to execute.</param>
        /// <remarks>
        /// This method is similar to <see cref="Evaluate(V8Script)"/> with the exception that it
        /// does not marshal a result value to the host. It can provide a performance advantage
        /// when the result value is not needed.
        /// </remarks>
        public void Execute(V8Script script)
        {
            Execute(script, false);
        }

        // ReSharper restore ParameterHidesMember

        /// <summary>
        /// Returns memory usage information for the V8 runtime.
        /// </summary>
        /// <returns>A <see cref="V8RuntimeHeapInfo"/> object containing memory usage information for the V8 runtime.</returns>
        public V8RuntimeHeapInfo GetRuntimeHeapInfo()
        {
            VerifyNotDisposed();
            return proxy.GetRuntimeHeapInfo();
        }

        #endregion

        #region internal members

        private object GetRootItem()
        {
            return MarshalToHost(ScriptInvoke(() => proxy.GetRootItem()), false);
        }

        private void BaseScriptInvoke(Action action)
        {
            base.ScriptInvoke(action);
        }

        private T BaseScriptInvoke<T>(Func<T> func)
        {
            return base.ScriptInvoke(func);
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet())
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        // ReSharper disable ParameterHidesMember

        private object Execute(V8Script script, bool evaluate)
        {
            MiscHelpers.VerifyNonNullArgument(script, "script");
            VerifyNotDisposed();

            return MarshalToHost(ScriptInvoke(() =>
            {
                if (inContinuationTimerScope || (ContinuationCallback == null))
                {
                    return proxy.Execute(script, evaluate);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    inContinuationTimerScope = true;
                    try
                    {
                        state[0].Change(continuationInterval, Timeout.Infinite);
                        return proxy.Execute(script, evaluate);
                    }
                    finally
                    {
                        inContinuationTimerScope = false;
                    }
                }
            }), false);
        }

        // ReSharper restore ParameterHidesMember

        private void OnContinuationTimer(Timer timer)
        {
            try
            {
                var callback = ContinuationCallback;
                if ((callback != null) && !callback())
                {
                    Interrupt();
                }
                else
                {
                    timer.Change(continuationInterval, Timeout.Infinite);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        #endregion

        #region ScriptEngine overrides (public members)

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="V8ScriptEngine"/> instances return "js" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get { return "js"; }
        }

        /// <summary>
        /// Allows the host to access script resources directly.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It dynamically supports properties and methods that correspond to global
        /// script objects and functions.
        /// </remarks>
        public override dynamic Script
        {
            get
            {
                VerifyNotDisposed();
                return script;
            }
        }

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// <para>
        /// This method is similar to <see cref="ScriptEngine.Evaluate(string)"/> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
        /// </para>
        /// <para>
        /// The <see cref="V8ScriptEngine"/> version of this method attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/k6xhc6yc(VS.85).aspx">toString</see>
        /// to convert the return value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            return ScriptInvoke(() =>
            {
                Script.EngineInternal.command = command;
                return base.ExecuteCommand("EngineInternal.getCommandResult(eval(EngineInternal.command))");
            });
        }

        /// <summary>
        /// Gets a string representation of the script call stack.
        /// </summary>
        /// <returns>The script call stack formatted as a string.</returns>
        /// <remarks>
        /// This method returns an empty string if the script engine is not executing script code.
        /// The stack trace text format is defined by the script engine.
        /// </remarks>
        public override string GetStackTrace()
        {
            string stackTrace = Script.EngineInternal.getStackTrace();
            var lines = stackTrace.Split('\n');
            return string.Join("\n", lines.Skip(2));
        }

        /// <summary>
        /// Interrupts script execution and causes the script engine to throw an exception.
        /// </summary>
        /// <remarks>
        /// This method can be called safely from any thread.
        /// </remarks>
        public override void Interrupt()
        {
            VerifyNotDisposed();
            proxy.Interrupt();
        }

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        public override void CollectGarbage(bool exhaustive)
        {
            VerifyNotDisposed();
            proxy.CollectGarbage(exhaustive);
        }

        #endregion

        #region ScriptEngine overrides (internal members)

        internal override bool EnumerateExtensionMethods
        {
            get { return base.EnumerateExtensionMethods && !SuppressExtensionMethodEnumeration; }
        }

        internal override void AddHostItem(string itemName, HostItemFlags flags, object item)
        {
            VerifyNotDisposed();

            var globalMembers = flags.HasFlag(HostItemFlags.GlobalMembers);
            if (globalMembers && engineFlags.HasFlag(V8ScriptEngineFlags.DisableGlobalMembers))
            {
                throw new InvalidOperationException("GlobalMembers support is disabled in this script engine");
            }

            MiscHelpers.VerifyNonNullArgument(itemName, "itemName");
            Debug.Assert(item != null);

            ScriptInvoke(() =>
            {
                var marshaledItem = MarshalToScript(item, flags);
                if (!(marshaledItem is HostItem))
                {
                    throw new InvalidOperationException("Invalid host item");
                }

                proxy.AddGlobalItem(itemName, marshaledItem, globalMembers);
            });
        }

        internal override object MarshalToScript(object obj, HostItemFlags flags)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }

            if (obj is Undefined)
            {
                return null;
            }

            if (obj is Nonexistent)
            {
                return obj;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                if ((hostItem.Engine == this) && (hostItem.Flags == flags))
                {
                    return obj;
                }

                obj = hostItem.Target;
            }

            var hostTarget = obj as HostTarget;
            if ((hostTarget != null) && !(hostTarget is IHostVariable))
            {
                obj = hostTarget.Target;
            }

            var scriptItem = obj as ScriptItem;
            if (scriptItem != null)
            {
                if (scriptItem.Engine == this)
                {
                    return scriptItem.Unwrap();
                }
            }

            return HostItem.Wrap(this, hostTarget ?? obj, flags);
        }

        internal override object MarshalToHost(object obj, bool preserveHostTarget)
        {
            if (obj == null)
            {
                return Undefined.Value;
            }

            if (obj is DBNull)
            {
                return null;
            }

            object result;
            if (MiscHelpers.TryMarshalPrimitiveToHost(obj, out result))
            {
                return result;
            }

            var hostTarget = obj as HostTarget;
            if (hostTarget != null)
            {
                return preserveHostTarget ? hostTarget : hostTarget.Target;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                return preserveHostTarget ? hostItem.Target : hostItem.Unwrap();
            }

            if (obj is ScriptItem)
            {
                return obj;
            }

            return V8ScriptItem.Wrap(this, obj);
        }

        internal override object Execute(string documentName, string code, bool evaluate, bool discard)
        {
            VerifyNotDisposed();

            return ScriptInvoke(() =>
            {
                var uniqueName = documentNameManager.GetUniqueName(documentName, "Script Document");
                if (discard)
                {
                    uniqueName += " [temp]";
                }
                else if (documentNames != null)
                {
                    documentNames.Add(uniqueName);
                }

                if (inContinuationTimerScope || (ContinuationCallback == null))
                {
                    return proxy.Execute(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, evaluate, discard);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    inContinuationTimerScope = true;
                    try
                    {
                        state[0].Change(continuationInterval, Timeout.Infinite);
                        return proxy.Execute(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, evaluate, discard);
                    }
                    finally
                    {
                        inContinuationTimerScope = false;
                    }
                }
            });
        }

        internal override HostItemCollateral HostItemCollateral
        {
            get { return hostItemCollateral; }
        }

        internal override void OnAccessSettingsChanged()
        {
            base.OnAccessSettingsChanged();
            ScriptInvoke(() => proxy.OnAccessSettingsChanged());
        }

        #endregion

        #region ScriptEngine overrides (script-side invocation)

        internal override void ScriptInvoke(Action action)
        {
            VerifyNotDisposed();
            proxy.InvokeWithLock(() => BaseScriptInvoke(action));
        }

        internal override T ScriptInvoke<T>(Func<T> func)
        {
            VerifyNotDisposed();
            var result = default(T);
            proxy.InvokeWithLock(() => result = BaseScriptInvoke(func));
            return result;
        }

        #endregion

        #region ScriptEngine overrides (disposal / finalization)

        /// <summary>
        /// Releases the unmanaged resources used by the script engine and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// This method is called by the public <see cref="ScriptEngine.Dispose()"/> method and the
        /// <see cref="ScriptEngine.Finalize">Finalize</see> method.
        /// <see cref="ScriptEngine.Dispose()"/> invokes the protected <c>Dispose(Boolean)</c>
        /// method with the <paramref name="disposing"/> parameter set to <c>true</c>.
        /// <see cref="ScriptEngine.Finalize">Finalize</see> invokes <c>Dispose(Boolean)</c> with
        /// <paramref name="disposing"/> set to <c>false</c>.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposedFlag.Set())
            {
                if (disposing)
                {
                    ((IDisposable)script).Dispose();
                    proxy.Dispose();
                }
            }
        }

        #endregion

        #region unit test support

        internal void EnableDocumentNameTracking()
        {
            documentNames = new List<string>();
        }

        internal IEnumerable<string> GetDocumentNames()
        {
            return documentNames;
        }

        #endregion

    }
}
