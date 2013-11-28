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
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Represents an instance of the V8 runtime.
    /// </summary>
    public sealed class V8Runtime : IDisposable
    {
        #region data

        private readonly string name;
        private static readonly IUniqueNameManager nameManager = new UniqueNameManager();

        private readonly IUniqueNameManager documentNameManager = new UniqueFileNameManager();

        private readonly V8IsolateProxy proxy;
        private bool disposed;

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
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        public V8Runtime(V8RuntimeFlags flags, int debugPort)
            : this(null, null, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(string name, V8RuntimeFlags flags)
            : this(name, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
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
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        public V8Runtime(V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
            : this(null, constraints, flags, debugPort)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, resource constraints, and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public V8Runtime(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags)
            : this(name, constraints, flags, 0)
        {
        }

        /// <summary>
        /// Initializes a new V8 runtime instance with the specified name, resource constraints, options, and debug port.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="constraints">Resource constraints for the instance.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        public V8Runtime(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
        {
            this.name = nameManager.GetUniqueName(name, GetType().GetRootName());
            proxy = V8IsolateProxy.Create(this.name, constraints, flags.HasFlag(V8RuntimeFlags.EnableDebugging), debugPort);
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets the name associated with the V8 runtime instance.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

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
        /// Gets or sets the maximum amount by which the stack is permitted to grow during script execution.
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
        /// <param name="engineName">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP/IP port, nor can they enable script debugging on a different port.
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
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP/IP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(V8ScriptEngineFlags flags, int debugPort)
        {
            return CreateScriptEngine(null, flags, debugPort);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified name and options.
        /// </summary>
        /// <param name="engineName">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP/IP port, nor can they enable script debugging on a different port.
        /// </para>
        /// </remarks>
        public V8ScriptEngine CreateScriptEngine(string engineName, V8ScriptEngineFlags flags)
        {
            return CreateScriptEngine(engineName, flags, 0);
        }

        /// <summary>
        /// Creates a new V8 script engine instance with the specified name, options, and debug port.
        /// </summary>
        /// <param name="engineName">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="debugPort">A TCP/IP port on which to listen for a debugger connection.</param>
        /// <returns>A new V8 script engine instance.</returns>
        /// <remarks>
        /// The new script engine instance shares the V8 runtime with other instances created by
        /// this method and any of its overloads.
        /// <para>
        /// V8 supports one script debugger per runtime. If script debugging has been enabled in
        /// the current runtime, additional script engine instances cannot disable it or change its
        /// TCP/IP port, nor can they enable script debugging on a different port.
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
        /// <param name="documentName">A document name for the compiled script. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to compile.</param>
        /// <returns>A compiled script that can be executed by multiple V8 script engine instances.</returns>
        public V8Script Compile(string documentName, string code)
        {
            VerifyNotDisposed();
            var uniqueName = name + ":" + documentNameManager.GetUniqueName(documentName, "Script Document");
            return proxy.Compile(uniqueName, FormatCode ? MiscHelpers.FormatCode(code) : code);
        }

        /// <summary>
        /// Returns memory usage information.
        /// </summary>
        /// <returns>A <see cref="V8RuntimeHeapInfo"/> object containing memory usage information.</returns>
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

        #endregion

        #region internal members

        internal V8IsolateProxy IsolateProxy
        {
            get
            {
                VerifyNotDisposed();
                return proxy;
            }
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Releases all resources used by the V8 runtime.
        /// </summary>
        /// <remarks>
        /// Call <c>Dispose()</c> when you are finished using the V8 runtime. <c>Dispose()</c>
        /// leaves the V8 runtime in an unusable state. After calling <c>Dispose()</c>, you must
        /// release all references to the V8 runtime so the garbage collector can reclaim the
        /// memory that the V8 runtime was occupying.
        /// </remarks>
        public void Dispose()
        {
            if (!disposed)
            {
                proxy.Dispose();
                disposed = true;
            }
        }

        #endregion
    }
}
