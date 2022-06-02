// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows.Core
{
    /// <summary>
    /// Represents an instance of the VBScript engine.
    /// </summary>
    /// <remarks>
    /// This class can be used in non-desktop environments such as server applications. An
    /// implementation of <c><see cref="ISyncInvoker"/></c> is required to enforce thread affinity.
    /// </remarks>
    public class VBScriptEngine : WindowsScriptEngine, IVBScriptEngineTag
    {
        #region data

        internal static readonly Dictionary<int, string> StaticRuntimeErrorMap = new Dictionary<int, string>
        {
            // https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/xe43cc8d(v=vs.84)
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

        internal static readonly Dictionary<int, string> StaticSyntaxErrorMap = new Dictionary<int, string>
        {
            // https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/2z84dwk8(v=vs.84)
            { 1052, "Cannot have multiple default property/method in a Class" },
            { 1044, "Cannot use parentheses when calling a Sub" },
            { 1053, "Class initialize or terminate do not have arguments" },
            { 1058, "'Default' specification can only be on Property Get" },
            { 1057, "'Default' specification must also specify 'Public'" },
            { 1005, "Expected '('" },
            { 1006, "Expected ')'" },
            { 1007, "Expected ']'" },                   // missing in the online documentation
            { 1011, "Expected '='" },
            { 1021, "Expected 'Case'" },
            { 1047, "Expected 'Class'" },
            { 1025, "Expected end of statement" },
            { 1014, "Expected 'End'" },
            { 1023, "Expected expression" },
            { 1015, "Expected 'Function'" },
            { 1010, "Expected identifier" },
            { 1012, "Expected 'If'" },
            { 1046, "Expected 'In'" },
            { 1026, "Expected integer constant" },
            { 1049, "Expected Let or Set or Get in property declaration" },
            { 1045, "Expected literal constant" },
            { 1019, "Expected 'Loop'" },
            { 1020, "Expected 'Next'" },
            { 1050, "Expected 'Property'" },
            { 1022, "Expected 'Select'" },
            { 1024, "Expected statement" },
            { 1016, "Expected 'Sub'" },
            { 1017, "Expected 'Then'" },
            { 1013, "Expected 'To'" },
            { 1018, "Expected 'Wend'" },
            { 1027, "Expected 'While' or 'Until'" },
            { 1028, "Expected 'While,' 'Until,' or end of statement" },
            { 1029, "Expected 'With'" },
            { 1030, "Identifier too long" },
            { 1032, "Invalid character" },              // incorrectly listed as 1014 in the online documentation
            { 1039, "Invalid 'exit' statement" },
            { 1040, "Invalid 'for' loop control variable" },
            { 1031, "Invalid number" },                 // incorrectly listed as 1013 in the online documentation
            { 1037, "Invalid use of 'Me' keyword" },
            { 1038, "'loop' without 'do'" },
            { 1048, "Must be defined inside a Class" },
            { 1042, "Must be first statement on the line" },
            { 1041, "Name redefined" },
            { 1051, "Number of arguments must be consistent across properties specification" },
            { 1001, "Out of Memory" },
            { 1054, "Property Set or Let must have at least one argument" },
            { 1002, "Syntax error" },
            { 1055, "Unexpected 'Next'" },
            { 1033, "Unterminated string constant" }    // incorrectly listed as 1015 in the online documentation
        };

        internal const string InitScript = @"
            class EngineInternalImpl
                public function getCommandResult(value)
                    if IsObject(value) then
                        if value is nothing then
                            getCommandResult = ""[nothing]""
                        else
                            dim valueTypeName
                            valueTypeName = TypeName(value)
                            if valueTypeName = ""Object"" or valueTypeName = ""Unknown"" then
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
                public function isPromise(value)
                    isPromise = false
                end function
                public function isHostObject(value)
                    isHostObject = IsObject(value) and TypeName(value) = ""Object"" and VarType(value) = 9
                end function
                public function throwValue(value)
                    Err.Raise 445
                end function
            end class
            set EngineInternal = new EngineInternalImpl
        ";

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new VBScript engine instance.
        /// </summary>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public VBScriptEngine(ISyncInvoker syncInvoker)
            : this(null, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public VBScriptEngine(string name, ISyncInvoker syncInvoker)
            : this(name, WindowsScriptEngineFlags.None, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public VBScriptEngine(WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : this(null, flags, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name, options, and synchronous invoker.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public VBScriptEngine(string name, WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : this("VBScript", name, "vbs", flags, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified programmatic
        /// identifier, name, list of supported file name extensions, options, and synchronous
        /// invoker.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the VBScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected VBScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : base(progID, name, fileNameExtensions, flags, syncInvoker)
        {
            Execute(MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name), InitScript);
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <c><see cref="VBScriptEngine"/></c> instances return "vbs" for this property.
        /// </remarks>
        public override string FileNameExtension => "vbs";

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// <para>
        /// This method is similar to <c><see cref="ScriptEngine.Evaluate(string)"/></c> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
        /// </para>
        /// <para>
        /// The <c><see cref="VBScriptEngine"/></c> version of this method supports both expressions and
        /// statements. If the specified command begins with "eval " (not case-sensitive), the
        /// engine executes the remainder as an expression and attempts to use
        /// <c><see href="https://docs.microsoft.com/en-us/previous-versions//0zk841e9(v=vs.85)">CStr</see></c>
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
                var documentInfo = new DocumentInfo("Expression") { Flags = DocumentFlags.IsTransient };
                return GetCommandResultString(Evaluate(documentInfo.MakeUnique(this), expression, false));
            }

            Execute("Command", true, trimmedCommand);
            return null;
        }

        internal override IDictionary<int, string> RuntimeErrorMap => StaticRuntimeErrorMap;

        internal override IDictionary<int, string> SyntaxErrorMap => StaticSyntaxErrorMap;

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category != DocumentCategory.Script)
            {
                throw new NotSupportedException("The script engine cannot execute documents of type '" + documentInfo.Category + "'");
            }

            return base.Execute(documentInfo, code, evaluate);
        }

        #endregion
    }
}
