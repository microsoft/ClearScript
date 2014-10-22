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
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an instance of the VBScript engine.
    /// </summary>
    public class VBScriptEngine : WindowsScriptEngine
    {
        #region data

        private static readonly Dictionary<int, string> runtimeErrorMap = new Dictionary<int, string>
        {
            // http://msdn.microsoft.com/en-us/library/xe43cc8d(VS.84).aspx
            { 429, "ActiveX component can't create object" },
            { 507, "An exception occurred" },
            { 449, "Argument not optional" },
            { 17, "Can't perform requested operation" },
            { 430, "Class doesn't support Automation" },
            { 506, "Class not defined" },
            { 11, "Division by zero" },
            { 48, "Error in loading DLL" },
            { 5020, "Expected ')' in regular expression" },
            { 5019, "Expected ']' in regular expression" },
            { 432, "File name or class name not found during Automation operation" },
            { 92, "For loop not initialized" },
            { 5008, "Illegal assignment" },
            { 51, "Internal error" },
            { 505, "Invalid or unqualified reference" },
            { 481, "Invalid picture" },
            { 5, "Invalid procedure call or argument" },
            { 5021, "Invalid range in character set" },
            { 94, "Invalid use of Null" },
            { 448, "Named argument not found" },
            { 447, "Object doesn't support current locale setting" },
            { 445, "Object doesn't support this action" },
            { 438, "Object doesn't support this property or method" },
            { 451, "Object not a collection" },
            { 504, "Object not safe for creating" },
            { 503, "Object not safe for initializing" },
            { 502, "Object not safe for scripting" },
            { 424, "Object required" },
            { 91, "Object variable not set" },
            { 7, "Out of Memory" },
            { 28, "Out of stack space" },
            { 14, "Out of string space" },
            { 6, "Overflow" },
            { 35, "Sub or function not defined" },
            { 9, "Subscript out of range" },
            { 5017, "Syntax error in regular expression" },
            { 462, "The remote server machine does not exist or is unavailable" },
            { 10, "This array is fixed or temporarily locked" },
            { 13, "Type mismatch" },
            { 5018, "Unexpected quantifier" },
            { 500, "Variable is undefined" },
            { 458, "Variable uses an Automation type not supported in VBScript" },
            { 450, "Wrong number of arguments or invalid property assignment" }
        };

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new VBScript engine instance.
        /// </summary>
        public VBScriptEngine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public VBScriptEngine(string name)
            : this(name, WindowsScriptEngineFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(string name, WindowsScriptEngineFlags flags)
            : this("VBScript", name, flags)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified programmatic
        /// identifier, name, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the VBScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected VBScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
            : base(progID, name, flags)
        {
            Execute(
                MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name),
                @"
                    class EngineInternalImpl
                        public function getCommandResult(value)
                            if IsObject(value) then
                                if value is nothing then
                                    getCommandResult = ""[nothing]""
                                else
                                    dim valueTypeName
                                    valueTypeName = TypeName(value)
                                    if (valueTypeName = ""Object"" or valueTypeName = ""Unknown"") then
                                        set getCommandResult = value
                                    else
                                        getCommandResult = ""[ScriptObject:"" & valueTypeName & ""]""
                                    end if
                                end if
                            elseif IsArray(value) then
                                getCommandResult = ""[array]""
                            elseif IsNull(value) then
                                getCommandResult = ""[null]""
                            elseif IsEmpty(value) then
                                getCommandResult = ""[empty]""
                            else
                                getCommandResult = CStr(value)
                            end if
                        end function
                        public function invokeConstructor(constructor, args)
                            Err.Raise 445
                        end function
                        public function invokeMethod(target, method, args)
                            if IsObject(target) then
                                if target is nothing then
                                else
                                    Err.Raise 445
                                end if
                            elseif IsNull(target) then
                            elseif IsEmpty(target) then
                            else
                                Err.Raise 445
                            end if
                            dim count
                            if IsArray(args) then
                                count = UBound(args) + 1
                                if count < 1 then
                                    invokeMethod = method()
                                elseif count = 1 then
                                    invokeMethod = method(args(0))
                                elseif count = 2 then
                                    invokeMethod = method(args(0), args(1))
                                elseif count = 3 then
                                    invokeMethod = method(args(0), args(1), args(2))
                                elseif count = 4 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3))
                                elseif count = 5 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4))
                                elseif count = 6 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5))
                                elseif count = 7 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6))
                                elseif count = 8 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7))
                                elseif count = 9 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8))
                                elseif count = 10 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9))
                                elseif count = 11 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10))
                                elseif count = 12 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10), args(11))
                                elseif count = 13 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10), args(11), args(12))
                                elseif count = 14 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10), args(11), args(12), args(13))
                                elseif count = 15 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10), args(11), args(12), args(13), args(14))
                                elseif count = 16 then
                                    invokeMethod = method(args(0), args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9), args(10), args(11), args(12), args(13), args(14), args(15))
                                else
                                    Err.Raise 450
                                end if
                            else
                                count = args.Length
                                if count < 1 then
                                    invokeMethod = method()
                                elseif count = 1 then
                                    invokeMethod = method(args.GetValue(0))
                                elseif count = 2 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1))
                                elseif count = 3 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2))
                                elseif count = 4 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3))
                                elseif count = 5 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4))
                                elseif count = 6 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5))
                                elseif count = 7 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6))
                                elseif count = 8 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7))
                                elseif count = 9 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8))
                                elseif count = 10 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9))
                                elseif count = 11 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10))
                                elseif count = 12 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10), args.GetValue(11))
                                elseif count = 13 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10), args.GetValue(11), args.GetValue(12))
                                elseif count = 14 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10), args.GetValue(11), args.GetValue(12), args.GetValue(13))
                                elseif count = 15 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10), args.GetValue(11), args.GetValue(12), args.GetValue(13), args.GetValue(14))
                                elseif count = 16 then
                                    invokeMethod = method(args.GetValue(0), args.GetValue(1), args.GetValue(2), args.GetValue(3), args.GetValue(4), args.GetValue(5), args.GetValue(6), args.GetValue(7), args.GetValue(8), args.GetValue(9), args.GetValue(10), args.GetValue(11), args.GetValue(12), args.GetValue(13), args.GetValue(14), args.GetValue(15))
                                else
                                    Err.Raise 450
                                end if
                            end if
                        end function
                    end class
                    set EngineInternal = new EngineInternalImpl
                "
            );
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="VBScriptEngine"/> instances return "vbs" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get { return "vbs"; }
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
        /// The <see cref="VBScriptEngine"/> version of this method supports both expressions and
        /// statements. If the specified command begins with "eval " (not case-sensitive), the
        /// engine executes the remainder as an expression and attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/0zk841e9(VS.85).aspx">CStr</see>
        /// to convert the result value. Otherwise, it executes the command as a statement and does
        /// not return a value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            var trimmedCommand = command.Trim();
            if (trimmedCommand.StartsWith("eval ", StringComparison.OrdinalIgnoreCase))
            {
                var expression = MiscHelpers.FormatInvariant("EngineInternal.getCommandResult({0})", trimmedCommand.Substring(5));
                return GetCommandResultString(Evaluate("Expression", true, expression, false));
            }

            Execute("Command", true, trimmedCommand);
            return null;
        }

        internal override IDictionary<int, string> RuntimeErrorMap
        {
            get { return runtimeErrorMap; }
        }

        #endregion
    }
}
