// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Represents an instance of the V8 runtime.
    /// </summary>
    public sealed class V8Runtime : IDisposable
    {
        #region data

        private static readonly IUniqueNameManager nameManager = new UniqueNameManager();

        private DocumentSettings documentSettings;
        private readonly DocumentSettings defaultDocumentSettings = new();

        private readonly V8IsolateProxy proxy;
        private readonly InterlockedOneWayFlag disposedFlag = new();

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new V8 runtime instance.
        /// </summary>
        public V8Runtime()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public V8Runtime(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified resource constraints.
        /// </summary>
        /// <param name="constraints">Resource constraints for the instance.</param>
        public V8Runtime(V8RuntimeConstraints constraints)
            : this(null, constraints)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name and resource constraints.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the instance.</param>
        public V8Runtime(string name, V8RuntimeConstraints constraints)
            : this(name, constraints, V8RuntimeFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(V8RuntimeFlags flags)
            : this(flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified options and debug port.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        public V8Runtime(V8RuntimeFlags flags, int debugPort)
            : this(null, null, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(string name, V8RuntimeFlags flags)
            : this(name, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        public V8Runtime(string name, V8RuntimeFlags flags, int debugPort)
            : this(name, null, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified resource constraints and options.
        /// </summary>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(V8RuntimeConstraints constraints, V8RuntimeFlags flags)
            : this(constraints, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified resource constraints, options, and debug port.
        /// </summary>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        public V8Runtime(V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
            : this(null, constraints, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, resource constraints, and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags)
            : this(name, constraints, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, resource constraints, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        public V8Runtime(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
        {
            Name = nameManager.GetUniqueName(name, GetType().GetRootName());
            proxy = V8IsolateProxy.Create(Name, constraints, flags, debugPort);
        }

        #endregion

        #region public members

        /// <summary>
        /// Occurs when a debugger connects to a V8 runtime.
        /// </summary>
        public static event EventHandler<V8RuntimeDebuggerEventArgs> DebuggerConnected;

        /// <summary>
        /// Occurs when a debugger disconnects from a V8 runtime.
        /// </summary>
        public static event EventHandler<V8RuntimeDebuggerEventArgs> DebuggerDisconnected;

        /// <summary>
        /// Gets the name associated with the V8 runtime instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Enables or disables script code formatting.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the V8 runtime may format script code before
        /// executing or compiling it. This is intended to facilitate interactive debugging. The
        /// formatting operation currently includes stripping leading and trailing blank lines and
        /// removing global indentation.
        /// </remarks>
        public bool FormatCode { get; set; }

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
        /// constraints (see <c><see cref="V8RuntimeConstraints"/></c>). This property enables external
        /// heap size monitoring that can prevent termination in some scenarios. To be effective,
        /// it should be set to a value that is significantly lower than
        /// <c><see cref="V8RuntimeConstraints.MaxOldSpaceSize"/></c>. Note that enabling heap size
        /// monitoring results in slower script execution.
        /// </para>
        /// <para>
        /// Exceeding this limit causes the V8 runtime to behave in accordance with
        /// <c><see cref="HeapSizeViolationPolicy"/></c>.
        /// </para>
        /// <para>
        /// Note that
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>
        /// memory is allocated outside the runtime's heap and is therefore not tracked by heap
        /// size monitoring. See <c><see cref="V8RuntimeConstraints.MaxArrayBufferAllocation"/></c> for
        /// additional information.
        /// </para>
        /// </remarks>
        public UIntPtr MaxHeapSize
        {
            get
            {
                VerifyNotDisposed();
                return proxy.MaxHeapSize;
            }

            set
            {
                VerifyNotDisposed();
                proxy.MaxHeapSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum time interval between consecutive heap size samples.
        /// </summary>
        /// <remarks>
        /// This property is effective only when heap size monitoring is enabled (see
        /// <c><see cref="MaxHeapSize"/></c>).
        /// </remarks>
        public TimeSpan HeapSizeSampleInterval
        {
            get
            {
                VerifyNotDisposed();
                return proxy.HeapSizeSampleInterval;
            }

            set
            {
                VerifyNotDisposed();
                proxy.HeapSizeSampleInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount by which the stack is permitted to grow during script execution.
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
        public UIntPtr MaxStackUsage
        {
            get
            {
                VerifyNotDisposed();
                return proxy.MaxStackUsage;
            }

            set
            {
                VerifyNotDisposed();
                proxy.MaxStackUsage = value;
            }
        }

        /// <summary>
        /// Gets or sets the V8 runtime's document settings.
        /// </summary>
        public DocumentSettings DocumentSettings
        {
            get => documentSettings ?? defaultDocumentSettings;
            set => documentSettings = value;
        }

        /// <summary>
        /// Enables or disables interrupt propagation in the V8 runtime.
        /// </summary>
        /// <remarks>
        /// By default, when nested script execution is interrupted via
        /// <c><see cref="ScriptEngine.Interrupt"/></c>, an instance of
        /// <c><see cref="ScriptInterruptedException"/></c>, if not handled by the host, is wrapped and
        /// delivered to the parent script frame as a normal exception that JavaScript code can
        /// catch. Setting this property to <c>true</c> causes the V8 runtime to remain in the
        /// interrupted state until its outermost script frame has been processed.
        /// </remarks>
        public bool EnableInterruptPropagation
        {
            get
            {
                VerifyNotDisposed();
                return proxy.EnableInterruptPropagation;
            }

            set
            {
                VerifyNotDisposed();
                proxy.EnableInterruptPropagation = value;
            }
        }

        /// <summary>
        /// Gets or sets the V8 runtime's behavior in response to a violation of the maximum heap size.
        /// </summary>
        public V8RuntimeViolationPolicy HeapSizeViolationPolicy
        {
            get
            {
                VerifyNotDisposed();
                return proxy.DisableHeapSizeViolationInterrupt ? V8RuntimeViolationPolicy.Exception : V8RuntimeViolationPolicy.Interrupt;
            }

            set
            {
                VerifyNotDisposed();
                switch (value)
                {
                    case V8RuntimeViolationPolicy.Interrupt:
                        proxy.DisableHeapSizeViolationInterrupt = false;
                        return;

                    case V8RuntimeViolationPolicy.Exception:
                        proxy.DisableHeapSizeViolationInterrupt = true;
                        return;

                    default:
                        throw new ArgumentException(MiscHelpers.FormatInvariant("Invalid {0} value", nameof(V8RuntimeViolationPolicy)), nameof(value));
                }
            }
        }

        /// <summary>
        /// Creates a new V8 script engine instance.
        /// </summary>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine()
        {
            return CreateScriptEngine(null);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified name.
        /// </summary>
        /// <param name="engineName">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(string engineName)
        {
            return CreateScriptEngine(engineName, V8ScriptEngineFlags.None);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// <para>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </para>
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(V8ScriptEngineFlags flags)
        {
            return CreateScriptEngine(null, flags);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified options and debug port.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// <para>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </para>
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(V8ScriptEngineFlags flags, int debugPort)
        {
            return CreateScriptEngine(null, flags, debugPort);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified name and options.
        /// </summary>
        /// <param name="engineName">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// <para>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </para>
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(string engineName, V8ScriptEngineFlags flags)
        {
            return CreateScriptEngine(engineName, flags, 0);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified name, options, and debug port.
        /// </summary>
        /// <param name="engineName">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP port on which to listen for a debugger connection.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// <para>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// </para>
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(string engineName, V8ScriptEngineFlags flags, int debugPort)
        {
            VerifyNotDisposed();
            return new V8ScriptEngine(this, engineName, null, flags, debugPort) { FormatCode = FormatCode };
        }

        /// <summary>
        /// Creates a compiled script.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script Compile(string code)
        {
            return Compile(null, code);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script Compile(string documentName, string code)
        {
            return Compile(new DocumentInfo(documentName), code);
        }

        /// <summary>
        /// Creates a compiled script with the specified document meta-information.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script Compile(DocumentInfo documentInfo, string code)
        {
            VerifyNotDisposed();
            return CompileInternal(documentInfo.MakeUnique(DocumentNameManager), code);
        }

        /// <summary>
        /// Creates a compiled script, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        /// <c><seealso cref="Compile(string, V8CacheKind, byte[], out bool)"/></c>
        public V8Script Compile(string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            return Compile(null, code, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, generating cache data for accelerated recompilation.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be generated.</param>
        /// <param name="cacheBytes">Cache data for accelerated recompilation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        /// <c><seealso cref="Compile(string, string, V8CacheKind, byte[], out bool)"/></c>
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
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// The generated cache data can be stored externally and is usable in other V8 runtimes
        /// and application processes.
        /// </remarks>
        /// <c><seealso cref="Compile(DocumentInfo, string, V8CacheKind, byte[], out bool)"/></c>
        public V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            VerifyNotDisposed();
            return CompileInternal(documentInfo.MakeUnique(DocumentNameManager), code, cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Creates a compiled script, consuming previously generated cache data.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
        /// </remarks>
        /// <c><seealso cref="Compile(string, V8CacheKind, out byte[])"/></c>
        public V8Script Compile(string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return Compile(null, code, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, consuming previously generated cache data.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
        /// </remarks>
        /// <c><seealso cref="Compile(string, string, V8CacheKind, out byte[])"/></c>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            return Compile(new DocumentInfo(documentName), code, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Creates a compiled script with the specified document meta-information, consuming previously generated cache data.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
        /// </remarks>
        /// <c><seealso cref="Compile(DocumentInfo, string, V8CacheKind, out byte[])"/></c>
        public V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            VerifyNotDisposed();
            return CompileInternal(documentInfo.MakeUnique(DocumentNameManager), code, cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Creates a compiled script, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script Compile(string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            return Compile(null, code, cacheKind, ref cacheBytes, out cacheResult);
        }

        /// <summary>
        /// Creates a compiled script with an associated document name, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="documentName">A document name for the compiled script. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script Compile(string documentName, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            return Compile(new DocumentInfo(documentName), code, cacheKind, ref cacheBytes, out cacheResult);
        }

        /// <summary>
        /// Creates a compiled script with the specified document meta-information, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to compile.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            VerifyNotDisposed();
            return CompileInternal(documentInfo.MakeUnique(DocumentNameManager), code, cacheKind, ref cacheBytes, out cacheResult);
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
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
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
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents(), cacheKind, out cacheBytes);
        }

        /// <summary>
        /// Loads and compiles a script document, consuming previously generated cache data.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="cacheKind">The kind of cache data to be consumed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
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
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
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
        /// <param name="cacheAccepted"><c>True</c> if <paramref name="cacheBytes"/> was accepted and used to accelerate script compilation, <c>false</c> otherwise.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. Note that script compilation may be bypassed if a suitable compiled
        /// script already exists in the V8 runtime's memory. In that case, the cache data is
        /// ignored and <paramref name="cacheAccepted"/> is set to <c>false</c>.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents(), cacheKind, cacheBytes, out cacheAccepted);
        }

        /// <summary>
        /// Loads and compiles a script document, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            return CompileDocument(specifier, null, cacheKind, ref cacheBytes, out cacheResult);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            return CompileDocument(specifier, category, null, cacheKind, ref cacheBytes, out cacheResult);
        }

        /// <summary>
        /// Loads and compiles a document with the specified category and context callback, consuming previously generated cache data and updating it if necessary.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and compiled.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <param name="cacheKind">The kind of cache data to be processed.</param>
        /// <param name="cacheBytes">Cache data for accelerated compilation.</param>
        /// <param name="cacheResult">The cache data processing result for the operation.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        /// <remarks>
        /// To be accepted, the cache data must have been generated for identical script code by
        /// the same V8 build. If returned, the updated cache data can be stored externally and is
        /// usable in other V8 script engines and application processes.
        /// </remarks>
        public V8Script CompileDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            return Compile(document.Info, document.GetTextContents(), cacheKind, ref cacheBytes, out cacheResult);
        }

        /// <summary>
        /// Returns memory usage information.
        /// </summary>
        /// <returns>A <c><see cref="V8RuntimeHeapInfo"/></c> object containing memory usage information.</returns>
        public V8RuntimeHeapInfo GetHeapInfo()
        {
            VerifyNotDisposed();
            return proxy.GetHeapInfo();
        }

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        public void CollectGarbage(bool exhaustive)
        {
            VerifyNotDisposed();
            proxy.CollectGarbage(exhaustive);
        }

        /// <summary>
        /// Begins collecting a new CPU profile.
        /// </summary>
        /// <param name="name">A name for the profile.</param>
        /// <returns><c>True</c> if the profile was created successfully, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// A V8 runtime can collect multiple CPU profiles simultaneously.
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
        /// A V8 runtime can collect multiple CPU profiles simultaneously.
        /// </remarks>
        public bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            VerifyNotDisposed();
            return proxy.BeginCpuProfile(name, flags);
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
            return proxy.EndCpuProfile(name);
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

        /// <summary>
        /// Writes a heap snapshot to the given stream.
        /// </summary>
        /// <param name="stream">The stream to which to write the heap snapshot.</param>
        /// <remarks>
        /// This method generates a heap snapshot in JSON format with ASCII encoding.
        /// </remarks>
        public void WriteHeapSnapshot(Stream stream)
        {
            MiscHelpers.VerifyNonNullArgument(stream, nameof(stream));
            VerifyNotDisposed();

            using (var engine = CreateScriptEngine(Name))
            {
                engine.ScriptInvoke(static ctx => ctx.proxy.WriteHeapSnapshot(ctx.stream), (proxy, stream));
            }
        }

        #endregion

        #region internal members

        internal readonly UniqueFileNameManager DocumentNameManager = new();

        internal readonly HostItemCollateral HostItemCollateral = new();

        internal static void OnDebuggerConnected(V8RuntimeDebuggerEventArgs args)
        {
            DebuggerConnected?.Invoke(null, args);
        }

        internal static void OnDebuggerDisconnected(V8RuntimeDebuggerEventArgs args)
        {
            DebuggerDisconnected?.Invoke(null, args);
        }

        internal V8IsolateProxy IsolateProxy
        {
            get
            {
                VerifyNotDisposed();
                return proxy;
            }
        }

        internal Statistics GetStatistics()
        {
            VerifyNotDisposed();
            return proxy.GetStatistics();
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }
            else if ((documentInfo.Category != DocumentCategory.Script) && (documentInfo.Category != ModuleCategory.Standard))
            {
                throw new NotSupportedException("The V8 runtime cannot compile documents of type '" + documentInfo.Category + "'");
            }

            return proxy.Compile(documentInfo, code);
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }
            else if ((documentInfo.Category != DocumentCategory.Script) && (documentInfo.Category != ModuleCategory.Standard))
            {
                throw new NotSupportedException("The V8 runtime cannot compile documents of type '" + documentInfo.Category + "'");
            }

            return proxy.Compile(documentInfo, code, cacheKind, out cacheBytes);
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }
            else if ((documentInfo.Category != DocumentCategory.Script) && (documentInfo.Category != ModuleCategory.Standard))
            {
                throw new NotSupportedException("The V8 runtime cannot compile documents of type '" + documentInfo.Category + "'");
            }

            return proxy.Compile(documentInfo, code, cacheKind, cacheBytes, out cacheAccepted);
        }

        private V8Script CompileInternal(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                code = CommonJSManager.Module.GetAugmentedCode(code);
            }
            else if ((documentInfo.Category != DocumentCategory.Script) && (documentInfo.Category != ModuleCategory.Standard))
            {
                throw new NotSupportedException("The V8 runtime cannot compile documents of type '" + documentInfo.Category + "'");
            }

            return proxy.Compile(documentInfo, code, cacheKind, ref cacheBytes, out cacheResult);
        }

        #endregion

        #region IDisposable implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                proxy.Dispose();
            }
        }

        #endregion

        #region Nested type: TaskKind

        internal enum TaskKind : ushort
        {
            Worker,
            DelayedWorker,
            Foreground,
            DelayedForeground,
            NonNestableForeground,
            NonNestableDelayedForeground,
            Count
        }

        #endregion

        #region Nested type: Statistics

        internal sealed class Statistics
        {
            public ulong ScriptCount;
            public ulong ScriptCacheSize;
            public ulong ModuleCount;
            public ulong[] PostedTaskCounts;
            public ulong[] InvokedTaskCounts;
        }

        #endregion
    }
}
