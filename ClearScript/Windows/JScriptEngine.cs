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

using System.Collections.Generic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an instance of the JScript engine.
    /// </summary>
    public class JScriptEngine : WindowsScriptEngine
    {
        #region data

        private static readonly Dictionary<int, string> runtimeErrorMap = new Dictionary<int, string>
        {
            // http://msdn.microsoft.com/en-us/library/1dk3k160(VS.84).aspx
            { 5029, "Array length must be a finite positive integer" },
            { 5030, "Array length must be assigned a finite positive number" },
            { 5028, "Array or arguments object expected" },
            { 5010, "Boolean expected" },
            { 5003, "Cannot assign to a function result" },
            { 5000, "Cannot assign to 'this'" },
            { 5034, "Circular reference in value argument not supported" },
            { 5006, "Date object expected" },
            { 5015, "Enumerator object expected" },
            { 5022, "Exception thrown and not caught" },
            { 5020, "Expected ')' in regular expression" },
            { 5019, "Expected ']' in regular expression" },
            { 5023, "Function does not have a valid prototype object" },
            { 5002, "Function expected" },
            { 5008, "Illegal assignment" },
            { 5021, "Invalid range in character set" },
            { 5035, "Invalid replacer argument" },
            { 5014, "JScript object expected" },
            { 5001, "Number expected" },
            { 5007, "Object expected" },
            { 5012, "Object member expected" },
            { 5016, "Regular Expression object expected" },
            { 5005, "String expected" },
            { 5017, "Syntax error in regular expression" },
            { 5026, "The number of fractional digits is out of range" },
            { 5027, "The precision is out of range" },
            { 5025, "The URI to be decoded is not a valid encoding" },
            { 5024, "The URI to be encoded contains an invalid character" },
            { 5009, "Undefined identifier" },
            { 5018, "Unexpected quantifier" },
            { 5013, "VBArray expected" }
        };

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new JScript engine instance.
        /// </summary>
        public JScriptEngine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public JScriptEngine(string name)
            : this(name, WindowsScriptEngineFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(string name, WindowsScriptEngineFlags flags)
            : this("JScript", name, flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected JScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
            : base(progID, name, flags)
        {
            Execute(
                MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name),
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
                                    if ((typeof(value) == 'object') || (typeof(value) == 'function')) {
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
                            }
                        };
                    })();
                "
            );
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="JScriptEngine"/> instances return "js" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get { return "js"; }
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
        /// The <see cref="JScriptEngine"/> version of this method attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/k6xhc6yc(VS.85).aspx">toString</see>
        /// to convert the return value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            Script.EngineInternal.command = command;
            return base.ExecuteCommand("EngineInternal.getCommandResult(eval(EngineInternal.command))");
        }

        internal override IDictionary<int, string> RuntimeErrorMap
        {
            get { return runtimeErrorMap; }
        }

        #endregion
    }
}
