// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.ClearScript.JavaScript;
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
    public sealed class V8ScriptEngine : ScriptEngine, IJavaScriptEngine
    {
        #region data

        private static readonly DocumentInfo initScriptInfo = new DocumentInfo(MiscHelpers.FormatInvariant("{0} [internal]", typeof(V8ScriptEngine).Name));

        private readonly V8ScriptEngineFlags engineFlags;
        private readonly V8ContextProxy proxy;
        private readonly object script;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        private const int continuationInterval = 2000;
        private bool inContinuationTimerScope;
        private bool awaitDebuggerAndPause;

        private readonly HostItemCollateral hostItemCollateral;
        private readonly IUniqueNameManager documentNameManager;
        private List<string> documentNames;
        private bool suppressInstanceMethodEnumeration;
        private bool suppressExtensionMethodEnumeration;

        private CommonJSManager commonJSManager;

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
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
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
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
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
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
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
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        /// <remarks>
        /// A separate V8 runtime is created for the new script engine instance.
        /// </remarks>
        public V8ScriptEngine(string name, V8RuntimeConstraints constraints, V8ScriptEngineFlags flags, int debugPort)
            : this(null, name, constraints, flags, debugPort)
        {
        }

        internal V8ScriptEngine(V8Runtime runtime, string name, V8RuntimeConstraints constraints, V8ScriptEngineFlags flags, int debugPort)
            : base((runtime != null) ? runtime.Name + ":" + name : name, "js")
        {
            using (var localRuntime = (runtime != null) ? null : new V8Runtime(name, constraints))
            {
                var activeRuntime = runtime ?? localRuntime;
                documentNameManager = activeRuntime.DocumentNameManager;
                hostItemCollateral = activeRuntime.HostItemCollateral;

                engineFlags = flags;
                proxy = V8ContextProxy.Create(activeRuntime.IsolateProxy, Name, flags, debugPort);
                script = GetRootItem();

                Execute(initScriptInfo,
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
                                        if (value[Symbol.toStringTag] == 'Module') {
                                            return '[module]';
                                        }
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

                if (flags.HasFlag(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart))
                {
                    awaitDebuggerAndPause = true;
                }
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
        /// Enables or disables instance method enumeration.
        /// </summary>
        /// <remarks>
        /// By default, a host object's instance methods are exposed as enumerable properties.
        /// Setting this property to <c>true</c> causes instance methods to be excluded from
        /// property enumeration. This affects all host objects exposed in the current script
        /// engine. Note that instance methods remain both retrievable and invocable regardless of
        /// this property's value.
        /// </remarks>
        public bool SuppressInstanceMethodEnumeration
        {
            get { return suppressInstanceMethodEnumeration; }
            set
            {
                suppressInstanceMethodEnumeration = value;
                OnEnumerationSettingsChanged();
            }
        }

        /// <summary>
        /// Enables or disables extension method enumeration.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, all exposed extension methods appear as enumerable properties of all host
        /// objects, regardless of type. Setting this property to <c>true</c> causes extension
        /// methods to be excluded from property enumeration. This affects all host objects exposed
        /// in the current script engine. Note that extension methods remain both retrievable and
        /// invocable regardless of this property's value.
        /// </para>
        /// <para>
        /// This property has no effect if <see cref="SuppressInstanceMethodEnumeration"/> is set
        /// to <c>true</c>.
        /// </para>
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
            return Compile(new DocumentInfo(documentName), code);
        }

        /// <summary>
        /// Creates a compiled script with the specified document meta-information.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        public V8Script Compile(DocumentInfo documentInfo, string code)
        {
            VerifyNotDisposed();
            return ScriptInvoke(() => CompileInternal(documentInfo.MakeUnique(this), code));
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
        /// engines and application processes.
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
        /// engines and application processes.
        /// </remarks>
        /// <seealso cref="Compile(string, string, V8CacheKind, byte[], out bool)"/>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            return Compile(new DocumentInfo(documentName), code, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Creates a compiled script with the specified document meta-information, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 script
        /// engines and application processes.
        /// </remarks>
        /// <seealso cref="Compile(DocumentInfo, string, V8CacheKind, byte[], out bool)"/>
        public V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            VerifyNotDisposed();

            V8Script tempScript = null;
            cacheBytes = ScriptInvoke(() =>
            {
                byte[] tempCacheBytes;
                tempScript = CompileInternal(documentInfo.MakeUnique(this), code, cacheKind, out tempCacheBytes);
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
        /// the same V8 build.
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
        /// the same V8 build.
        /// </remarks>
        /// <seealso cref="Compile(string, string, V8CacheKind, out byte[])"/>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return Compile(new DocumentInfo(documentName), code, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, consuming previously generated cache data.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed multiple times without recompilation.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build.
        /// </remarks>
        /// <seealso cref="Compile(DocumentInfo, string, V8CacheKind, out byte[])"/>
        public V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            VerifyNotDisposed();

            V8Script tempScript = null;
            cacheAccepted = ScriptInvoke(() =>
            {
                bool tempCacheAccepted;
                tempScript = CompileInternal(documentInfo.MakeUnique(this), code, cacheKind, cacheBytes, out tempCacheAccepted);
                return tempCacheAccepted;
            });

            return tempScript;
        }

        /// <summary>
        /// Loads and compiles a script document.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script CompileDocument(string specifier)
        {
            return CompileDocument(specifier, null);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script CompileDocument(string specifier, DocumentCategory category)
        {
            return CompileDocument(specifier, category, null);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category and context callback.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script CompileDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, "specifier", "Invalid document specifier");
            var document = DocumentSettings.Loader.LoadDocument(DocumentSettings, null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents());
        }

        /// <summary>
        /// Loads and compiles a script document, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            return CompileDocument(specifier, null, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            return CompileDocument(specifier, category, null, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category and context callback, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, "specifier", "Invalid document specifier");
            var document = DocumentSettings.Loader.LoadDocument(DocumentSettings, null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents(), cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Loads and compiles a script document, consuming previously generated cache data.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build.
        /// </remarks>
        public V8Script CompileDocument(string specifier, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return CompileDocument(specifier, null, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category, consuming previously generated cache data.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return CompileDocument(specifier, category, null, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category and context callback, consuming previously generated cache data.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, "specifier", "Invalid document specifier");
            var document = DocumentSettings.Loader.LoadDocument(DocumentSettings, null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents(), cacheKind, cacheBytes, out cacheAccepted);
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

        /// <summary>
        /// Begins collecting a new CPU profile.
        /// </summary>
        /// <param name="name">A name for the profile.</param>
        /// <returns><c>True</c> if the profile was created successfully, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// A V8 script engine can collect multiple CPU profiles simultaneously.
        /// </remarks>
        public bool BeginCpuProfile(string name)
        {
            return BeginCpuProfile(name, V8CpuProfileFlags.None);
        }

        /// <summary>
        /// Begins collecting a new CPU profile with the specified options.
        /// </summary>
        /// <param name="name">A name for the profile.</param>
        /// <param name="flags">Options for creating the profile.</param>
        /// <returns><c>True</c> if the profile was created successfully, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// A V8 script engine can collect multiple CPU profiles simultaneously.
        /// </remarks>
        public bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            VerifyNotDisposed();
            return proxy.BeginCpuProfile(Name + ':' + name, flags);
        }

        /// <summary>
        /// Completes and returns a CPU profile.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <returns>The profile if it was found and completed successfully, <c>null</c> otherwise.</returns>
        /// <remarks>
        /// An empty <paramref name="name"/> argument selects the most recently created CPU profile.
        /// </remarks>
        public V8CpuProfile EndCpuProfile(string name)
        {
            VerifyNotDisposed();
            return proxy.EndCpuProfile(Name + ':' + name);
        }

        /// <summary>
        /// Collects a sample in all CPU profiles active in the V8 runtime.
        /// </summary>
        public void CollectCpuProfileSample()
        {
            VerifyNotDisposed();
            proxy.CollectCpuProfileSample();
        }

        /// <summary>
        /// Gets or sets the time interval between automatic CPU profile samples, in microseconds.
        /// </summary>
        /// <remarks>
        /// Assigning this property has no effect on CPU profiles already active in the V8 runtime.
        /// The default value is 1000.
        /// </remarks>
        public uint CpuProfileSampleInterval
        {
            get
            {
                VerifyNotDisposed();
                return proxy.CpuProfileSampleInterval;
            }

            set
            {
                VerifyNotDisposed();
                proxy.CpuProfileSampleInterval = value;
            }
        }

        #endregion

        #region internal members

        internal V8Runtime.Statistics GetRuntimeStatistics()
        {
            VerifyNotDisposed();
            return proxy.GetRuntimeStatistics();
        }

        internal Statistics GetStatistics()
        {
            VerifyNotDisposed();
            return ScriptInvoke(() =>
            {
                var statistics = proxy.GetStatistics();

                if (commonJSManager != null)
                {
                    statistics.CommonJSModuleCacheSize = CommonJSManager.ModuleCacheSize;
                }
                
                return statistics;
            });
        }

        private CommonJSManager CommonJSManager
        {
            get
            {
                if (commonJSManager == null)
                {
                    commonJSManager = new CommonJSManager(this);
                }

                return commonJSManager;
            }
        }

        private object GetRootItem()
        {
            return MarshalToHost(ScriptInvoke(() => proxy.GetRootItem()), false);
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet)
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
                    if (MiscHelpers.Exchange(ref awaitDebuggerAndPause, false))
                    {
                        proxy.AwaitDebuggerAndPause();
                    }

                    return ExecuteInternal(script, evaluate);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    inContinuationTimerScope = true;
                    try
                    {
                        state[0].Change(continuationInterval, Timeout.Infinite);

                        if (MiscHelpers.Exchange(ref awaitDebuggerAndPause, false))
                        {
                            proxy.AwaitDebuggerAndPause();
                        }

                        return ExecuteInternal(script, evaluate);
                    }
                    finally
                    {
                        inContinuationTimerScope = false;
                    }
                }
            }), false);
        }

        // ReSharper restore ParameterHidesMember

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            CommonJSManager.Module module = null;
            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }

            // ReSharper disable once LocalVariableHidesMember
            var script = proxy.Compile(documentInfo, code);
            if (module != null)
            {
                module.Evaluator = () => proxy.Execute(script, true);
            }

            return script;
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            CommonJSManager.Module module = null;
            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }

            // ReSharper disable once LocalVariableHidesMember
            var script = proxy.Compile(documentInfo, code, cacheKind, out cacheBytes);
            if (module != null)
            {
                module.Evaluator = () => proxy.Execute(script, true);
            }

            return script;
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            CommonJSManager.Module module = null;
            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }

            // ReSharper disable once LocalVariableHidesMember
            var script = proxy.Compile(documentInfo, code, cacheKind, cacheBytes, out cacheAccepted);
            if (module != null)
            {
                module.Evaluator = () => proxy.Execute(script, true);
            }

            return script;
        }

        private object ExecuteInternal(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                var module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                return module.Process();
            }

            return ExecuteRaw(documentInfo, code, evaluate);
        }

        // ReSharper disable ParameterHidesMember

        private object ExecuteInternal(V8Script script, bool evaluate)
        {
            if (script.UniqueDocumentInfo.Category == ModuleCategory.CommonJS)
            {
                var module = CommonJSManager.GetOrCreateModule(script.UniqueDocumentInfo, script.CodeDigest, () => proxy.Execute(script, evaluate));
                return module.Process();
            }

            return proxy.Execute(script, evaluate);
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

        internal override IUniqueNameManager DocumentNameManager
        {
            get { return documentNameManager; }
        }

        internal override bool EnumerateInstanceMethods
        {
            get { return base.EnumerateInstanceMethods && !SuppressInstanceMethodEnumeration; }
        }

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

            if (obj is Nothing)
            {
                return null;
            }

            if (engineFlags.HasFlag(V8ScriptEngineFlags.EnableDateTimeConversion) && (obj is DateTime))
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

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            VerifyNotDisposed();

            return ScriptInvoke(() =>
            {
                if ((documentNames != null) && !documentInfo.Flags.GetValueOrDefault().HasFlag(DocumentFlags.IsTransient))
                {
                    documentNames.Add(documentInfo.UniqueName);
                }

                if (inContinuationTimerScope || (ContinuationCallback == null))
                {
                    if (MiscHelpers.Exchange(ref awaitDebuggerAndPause, false))
                    {
                        proxy.AwaitDebuggerAndPause();
                    }

                    return ExecuteInternal(documentInfo, code, evaluate);
                }

                var state = new Timer[] { null };
                using (state[0] = new Timer(unused => OnContinuationTimer(state[0]), null, Timeout.Infinite, Timeout.Infinite))
                {
                    inContinuationTimerScope = true;
                    try
                    {
                        state[0].Change(continuationInterval, Timeout.Infinite);

                        if (MiscHelpers.Exchange(ref awaitDebuggerAndPause, false))
                        {
                            proxy.AwaitDebuggerAndPause();
                        }

                        return ExecuteInternal(documentInfo, code, evaluate);
                    }
                    finally
                    {
                        inContinuationTimerScope = false;
                    }
                }
            });
        }

        internal override object ExecuteRaw(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            return proxy.Execute(documentInfo, code, evaluate);
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
            using (CreateEngineScope())
            {
                proxy.InvokeWithLock(() => ScriptInvokeInternal(action));
            }
        }

        internal override T ScriptInvoke<T>(Func<T> func)
        {
            VerifyNotDisposed();
            using (CreateEngineScope())
            {
                var result = default(T);
                proxy.InvokeWithLock(() => result = ScriptInvokeInternal(func));
                return result;
            }
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

        #region IJavaScriptEngine implementation

        uint IJavaScriptEngine.BaseLanguageVersion
        {
            get { return 8; }
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

        #region Nested type: Statistics

        internal sealed class Statistics
        {
            public ulong ScriptCount;
            public ulong ModuleCount;
            public ulong ModuleCacheSize;
            public int CommonJSModuleCacheSize;
        }

        #endregion
    }
}
