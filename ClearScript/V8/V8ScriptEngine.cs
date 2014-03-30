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

        private const int continuationInterval = 2000;

        private readonly V8ScriptEngineFlags engineFlags;
        private readonly V8ContextProxy proxy;
        private readonly dynamic script;
        private bool disposed;

        private readonly IUniqueNameManager documentNameManager = new UniqueFileNameManager();
        private readonly List<string> documentNames = new List<string>();

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

                            return {

                                getCommandResult: function (value) {
                                    if (value != null) {
                                        if (((typeof(value) == 'object') && !value.hasOwnProperty('{c2cf47d3-916b-4a3f-be2a-6ff567425808}')) || (typeof(value) == 'function')) {
                                            if (typeof(value.toString) == 'function') {
                                                return value.toString();
                                            }
                                        }
                                    }
                                    return value;
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
        /// Gets or sets the maximum amount by which the V8 runtime is permitted to grow the stack during script execution.
        /// </summary>
        /// <remarks>
        /// This property is specified in bytes. When it is set to the default value, no stack
        /// usage limit is enforced, and unchecked recursion or other stack usage may lead to
        /// unrecoverable errors and process termination.
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
            MiscHelpers.VerifyNonNullArgument(script, "script");
            VerifyNotDisposed();

            return MarshalToHost(ScriptInvoke(() =>
            {
                if (ContinuationCallback == null)
                {
                    return proxy.Execute(script, true);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    state[0].Change(continuationInterval, Timeout.Infinite);
                    return proxy.Execute(script, true);
                }
            }), false);
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
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
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
        /// This method is similar to <see cref="ScriptEngine.Evaluate(string)"/> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
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
            if (hostTarget != null)
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
                else if (engineFlags.HasFlag(V8ScriptEngineFlags.EnableDebugging))
                {
                    documentNames.Add(uniqueName);
                }

                if (ContinuationCallback == null)
                {
                    return proxy.Execute(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, evaluate, discard);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    state[0].Change(continuationInterval, Timeout.Infinite);
                    return proxy.Execute(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code, evaluate, discard);
                }
            });
        }

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

        #region ScriptEngine overrides (disposition / finalization)

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
            if (!disposed)
            {
                if (disposing)
                {
                    ((IDisposable)script).Dispose();
                    proxy.Dispose();
                }

                disposed = true;
            }
        }

        #endregion

        #region unit test support

        internal IEnumerable<string> GetDebugDocumentNames()
        {
            return documentNames;
        }

        #endregion

    }
}
