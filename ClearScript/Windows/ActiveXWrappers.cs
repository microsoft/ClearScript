// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    #region ActiveScriptWrapper

    internal abstract class ActiveScriptWrapper
    {
        public static ActiveScriptWrapper Create(string progID, WindowsScriptEngineFlags flags)
        {
            if (Environment.Is64BitProcess)
            {
                return new ActiveScriptWrapper64(progID, flags);
            }

            return new ActiveScriptWrapper32(progID, flags);
        }

        public abstract void SetScriptSite(IActiveScriptSite site);

        public abstract void SetScriptState(ScriptState state);

        public abstract void GetScriptState(out ScriptState state);

        public abstract void InitNew();

        public abstract void GetScriptDispatch(string itemName, out object dispatch);

        public abstract void AddNamedItem(string name, ScriptItemFlags flags);

        public abstract void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo);

        public abstract void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags);

        public abstract void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts);

        public abstract void EnumStackFrames(out IEnumDebugStackFrames enumFrames);

        public abstract void CollectGarbage(ScriptGCType type);

        public abstract void Close();

        #region Nested type: DummyEnumDebugStackFrames

        protected class DummyEnumDebugStackFrames : IEnumDebugStackFrames
        {
            #region IEnumDebugStackFrames implementation

            public void Next(uint count, out DebugStackFrameDescriptor descriptor, out uint countFetched)
            {
                descriptor = default(DebugStackFrameDescriptor);
                countFetched = 0;
            }

            public void Skip(uint count)
            {
            }

            public void Reset()
            {
            }

            public void Clone(out IEnumDebugStackFrames enumFrames)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }

    internal sealed class ActiveScriptWrapper32 : ActiveScriptWrapper
    {
        // ReSharper disable NotAccessedField.Local

        private IntPtr pActiveScript;
        private IntPtr pActiveScriptParse;
        private IntPtr pActiveScriptDebug;
        private IntPtr pActiveScriptGarbageCollector;
        private IntPtr pDebugStackFrameSniffer;

        private IActiveScript activeScript;
        private IActiveScriptParse32 activeScriptParse;
        private IActiveScriptDebug32 activeScriptDebug;
        private IActiveScriptGarbageCollector activeScriptGarbageCollector;
        private IDebugStackFrameSnifferEx32 debugStackFrameSniffer;

        // ReSharper restore NotAccessedField.Local

        private delegate uint RawInterruptScriptThread(
            [In] IntPtr pThis,
            [In] uint scriptThreadID,
            [In] ref EXCEPINFO excepInfo,
            [In] ScriptInterruptFlags flags
        );

        private delegate uint RawEnumCodeContextsOfPosition(
            [In] IntPtr pThis,
            [In] uint sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );

        public ActiveScriptWrapper32(string progID, WindowsScriptEngineFlags flags)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            pActiveScript = RawCOMHelpers.CreateInstance<IActiveScript>(progID);
            pActiveScriptParse = RawCOMHelpers.QueryInterface<IActiveScriptParse32>(pActiveScript);
            pActiveScriptDebug = RawCOMHelpers.QueryInterface<IActiveScriptDebug32>(pActiveScript);
            pActiveScriptGarbageCollector = RawCOMHelpers.QueryInterfaceNoThrow<IActiveScriptGarbageCollector>(pActiveScript);
            pDebugStackFrameSniffer = RawCOMHelpers.QueryInterfaceNoThrow<IDebugStackFrameSnifferEx32>(pActiveScript);

            activeScript = (IActiveScript)Marshal.GetObjectForIUnknown(pActiveScript);
            activeScriptParse = (IActiveScriptParse32)activeScript;
            activeScriptDebug = (IActiveScriptDebug32)activeScript;
            activeScriptGarbageCollector = activeScript as IActiveScriptGarbageCollector;
            debugStackFrameSniffer = activeScript as IDebugStackFrameSnifferEx32;

            if (flags.HasFlag(WindowsScriptEngineFlags.EnableStandardsMode))
            {
                var activeScriptProperty = activeScript as IActiveScriptProperty;
                if (activeScriptProperty != null)
                {
                    object name;
                    activeScriptProperty.GetProperty(ScriptProp.Name, IntPtr.Zero, out name);
                    if (Equals(name, "JScript"))
                    {
                        object value = ScriptLanguageVersion.Standards;
                        activeScriptProperty.SetProperty(ScriptProp.InvokeVersioning, IntPtr.Zero, ref value);
                    }
                }

                if (!flags.HasFlag(WindowsScriptEngineFlags.DoNotEnableVTablePatching) && MiscHelpers.IsX86InstructionSet())
                {
                    HostItem.EnableVTablePatching = true;
                }
            }

            // ReSharper restore SuspiciousTypeConversion.Global
        }

        public override void SetScriptSite(IActiveScriptSite site)
        {
            activeScript.SetScriptSite(site);
        }

        public override void SetScriptState(ScriptState state)
        {
            activeScript.SetScriptState(state);
        }

        public override void GetScriptState(out ScriptState state)
        {
            activeScript.GetScriptState(out state);
        }

        public override void InitNew()
        {
            activeScriptParse.InitNew();
        }

        public override void GetScriptDispatch(string itemName, out object dispatch)
        {
            activeScript.GetScriptDispatch(itemName, out dispatch);
        }

        public override void AddNamedItem(string name, ScriptItemFlags flags)
        {
            activeScript.AddNamedItem(name, flags);
        }

        public override void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo)
        {
            activeScriptParse.ParseScriptText(code, itemName, context, delimiter, sourceContext.ToUInt32(), startingLineNumber, flags, pVarResult, out excepInfo);
        }

        public override void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawInterruptScriptThread>(pActiveScript, 14);
            del(pActiveScript, scriptThreadID, ref excepInfo, flags);
        }

        public override void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawEnumCodeContextsOfPosition>(pActiveScriptDebug, 5);
            RawCOMHelpers.HResult.Check(del(pActiveScriptDebug, sourceContext.ToUInt32(), offset, length, out enumContexts));
        }

        public override void EnumStackFrames(out IEnumDebugStackFrames enumFrames)
        {
            if (debugStackFrameSniffer != null)
            {
                debugStackFrameSniffer.EnumStackFrames(out enumFrames);
            }
            else
            {
                enumFrames = new DummyEnumDebugStackFrames();
            }
        }

        public override void CollectGarbage(ScriptGCType type)
        {
            if (activeScriptGarbageCollector != null)
            {
                activeScriptGarbageCollector.CollectGarbage(type);
            }
        }

        public override void Close()
        {
            debugStackFrameSniffer = null;
            activeScriptGarbageCollector = null;
            activeScriptDebug = null;
            activeScriptParse = null;

            RawCOMHelpers.ReleaseAndEmpty(ref pDebugStackFrameSniffer);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptGarbageCollector);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptDebug);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptParse);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScript);

            activeScript.Close();
            Marshal.FinalReleaseComObject(activeScript);
            activeScript = null;
        }
    }

    internal sealed class ActiveScriptWrapper64 : ActiveScriptWrapper
    {
        // ReSharper disable NotAccessedField.Local

        private IntPtr pActiveScript;
        private IntPtr pActiveScriptParse;
        private IntPtr pActiveScriptDebug;
        private IntPtr pActiveScriptGarbageCollector;
        private IntPtr pDebugStackFrameSniffer;

        private IActiveScript activeScript;
        private IActiveScriptParse64 activeScriptParse;
        private IActiveScriptDebug64 activeScriptDebug;
        private IActiveScriptGarbageCollector activeScriptGarbageCollector;
        private IDebugStackFrameSnifferEx64 debugStackFrameSniffer;

        // ReSharper restore NotAccessedField.Local

        private delegate uint RawInterruptScriptThread(
            [In] IntPtr pThis,
            [In] uint scriptThreadID,
            [In] ref EXCEPINFO excepInfo,
            [In] ScriptInterruptFlags flags
        );

        private delegate uint RawEnumCodeContextsOfPosition(
            [In] IntPtr pThis,
            [In] ulong sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );

        public ActiveScriptWrapper64(string progID, WindowsScriptEngineFlags flags)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            pActiveScript = RawCOMHelpers.CreateInstance<IActiveScript>(progID);
            pActiveScriptParse = RawCOMHelpers.QueryInterface<IActiveScriptParse64>(pActiveScript);
            pActiveScriptDebug = RawCOMHelpers.QueryInterface<IActiveScriptDebug64>(pActiveScript);
            pActiveScriptGarbageCollector = RawCOMHelpers.QueryInterfaceNoThrow<IActiveScriptGarbageCollector>(pActiveScript);
            pDebugStackFrameSniffer = RawCOMHelpers.QueryInterfaceNoThrow<IDebugStackFrameSnifferEx64>(pActiveScript);

            activeScript = (IActiveScript)Marshal.GetObjectForIUnknown(pActiveScript);
            activeScriptParse = (IActiveScriptParse64)activeScript;
            activeScriptDebug = (IActiveScriptDebug64)activeScript;
            activeScriptGarbageCollector = activeScript as IActiveScriptGarbageCollector;
            debugStackFrameSniffer = activeScript as IDebugStackFrameSnifferEx64;

            if (flags.HasFlag(WindowsScriptEngineFlags.EnableStandardsMode))
            {
                var activeScriptProperty = activeScript as IActiveScriptProperty;
                if (activeScriptProperty != null)
                {
                    object name;
                    activeScriptProperty.GetProperty(ScriptProp.Name, IntPtr.Zero, out name);
                    if (Equals(name, "JScript"))
                    {
                        object value = ScriptLanguageVersion.Standards;
                        activeScriptProperty.SetProperty(ScriptProp.InvokeVersioning, IntPtr.Zero, ref value);
                    }
                }

                if (!flags.HasFlag(WindowsScriptEngineFlags.DoNotEnableVTablePatching) && MiscHelpers.IsX86InstructionSet())
                {
                    HostItem.EnableVTablePatching = true;
                }
            }

            // ReSharper restore SuspiciousTypeConversion.Global
        }

        public override void SetScriptSite(IActiveScriptSite site)
        {
            activeScript.SetScriptSite(site);
        }

        public override void SetScriptState(ScriptState state)
        {
            activeScript.SetScriptState(state);
        }

        public override void GetScriptState(out ScriptState state)
        {
            activeScript.GetScriptState(out state);
        }

        public override void InitNew()
        {
            activeScriptParse.InitNew();
        }

        public override void GetScriptDispatch(string itemName, out object dispatch)
        {
            activeScript.GetScriptDispatch(itemName, out dispatch);
        }

        public override void AddNamedItem(string name, ScriptItemFlags flags)
        {
            activeScript.AddNamedItem(name, flags);
        }

        public override void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo)
        {
            activeScriptParse.ParseScriptText(code, itemName, context, delimiter, sourceContext.ToUInt64(), startingLineNumber, flags, pVarResult, out excepInfo);
        }

        public override void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawInterruptScriptThread>(pActiveScript, 14);
            del(pActiveScript, scriptThreadID, ref excepInfo, flags);
        }

        public override void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawEnumCodeContextsOfPosition>(pActiveScriptDebug, 5);
            RawCOMHelpers.HResult.Check(del(pActiveScriptDebug, sourceContext.ToUInt64(), offset, length, out enumContexts));
        }

        public override void EnumStackFrames(out IEnumDebugStackFrames enumFrames)
        {
            if (debugStackFrameSniffer != null)
            {
                debugStackFrameSniffer.EnumStackFrames(out enumFrames);
            }
            else
            {
                enumFrames = new DummyEnumDebugStackFrames();
            }
        }

        public override void CollectGarbage(ScriptGCType type)
        {
            if (activeScriptGarbageCollector != null)
            {
                activeScriptGarbageCollector.CollectGarbage(type);
            }
        }

        public override void Close()
        {
            debugStackFrameSniffer = null;
            activeScriptGarbageCollector = null;
            activeScriptDebug = null;
            activeScriptParse = null;

            RawCOMHelpers.ReleaseAndEmpty(ref pDebugStackFrameSniffer);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptGarbageCollector);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptDebug);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptParse);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScript);

            activeScript.Close();
            Marshal.FinalReleaseComObject(activeScript);
            activeScript = null;
        }
    }

    #endregion

    #region ProcessDebugManagerWrapper

    internal abstract class ProcessDebugManagerWrapper
    {
        public static bool TryCreate(out ProcessDebugManagerWrapper wrapper)
        {
            if (Environment.Is64BitProcess)
            {
                return ProcessDebugManagerWrapper64.TryCreate(out wrapper);
            }

            return ProcessDebugManagerWrapper32.TryCreate(out wrapper);
        }

        public abstract void CreateApplication(out DebugApplicationWrapper applicationWrapper);

        public abstract bool TryAddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie);

        public abstract void RemoveApplication(uint cookie);
    }

    internal sealed class ProcessDebugManagerWrapper32 : ProcessDebugManagerWrapper
    {
        private readonly IProcessDebugManager32 processDebugManager;

        public new static bool TryCreate(out ProcessDebugManagerWrapper wrapper)
        {
            IProcessDebugManager32 processDebugManager;
            if (MiscHelpers.TryCreateCOMObject("ProcessDebugManager", null, out processDebugManager))
            {
                wrapper = new ProcessDebugManagerWrapper32(processDebugManager);
                return true;
            }

            wrapper = null;
            return false;
        }

        private ProcessDebugManagerWrapper32(IProcessDebugManager32 processDebugManager)
        {
            this.processDebugManager = processDebugManager;
        }

        public override void CreateApplication(out DebugApplicationWrapper applicationWrapper)
        {
            IDebugApplication32 debugApplication;
            processDebugManager.CreateApplication(out debugApplication);
            applicationWrapper = DebugApplicationWrapper.Create(debugApplication);
        }

        public override bool TryAddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie)
        {
            return RawCOMHelpers.HResult.Succeeded(processDebugManager.AddApplication(DebugApplicationWrapper32.Unwrap(applicationWrapper), out cookie));
        }

        public override void RemoveApplication(uint cookie)
        {
            processDebugManager.RemoveApplication(cookie);
        }
    }

    internal sealed class ProcessDebugManagerWrapper64 : ProcessDebugManagerWrapper
    {
        private readonly IProcessDebugManager64 processDebugManager;

        public new static bool TryCreate(out ProcessDebugManagerWrapper wrapper)
        {
            IProcessDebugManager64 processDebugManager;
            if (MiscHelpers.TryCreateCOMObject("ProcessDebugManager", null, out processDebugManager))
            {
                wrapper = new ProcessDebugManagerWrapper64(processDebugManager);
                return true;
            }

            wrapper = null;
            return false;
        }

        private ProcessDebugManagerWrapper64(IProcessDebugManager64 processDebugManager)
        {
            this.processDebugManager = processDebugManager;
        }

        public override void CreateApplication(out DebugApplicationWrapper applicationWrapper)
        {
            IDebugApplication64 debugApplication;
            processDebugManager.CreateApplication(out debugApplication);
            applicationWrapper = DebugApplicationWrapper.Create(debugApplication);
        }

        public override bool TryAddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie)
        {
            return RawCOMHelpers.HResult.Succeeded(processDebugManager.AddApplication(DebugApplicationWrapper64.Unwrap(applicationWrapper), out cookie));
        }

        public override void RemoveApplication(uint cookie)
        {
            processDebugManager.RemoveApplication(cookie);
        }
    }

    #endregion

    #region DebugApplicationWrapper

    internal abstract class DebugApplicationWrapper
    {
        public static DebugApplicationWrapper Create(IDebugApplication64 debugApplication)
        {
            return new DebugApplicationWrapper64(debugApplication);
        }

        public static DebugApplicationWrapper Create(IDebugApplication32 debugApplication)
        {
            return new DebugApplicationWrapper32(debugApplication);
        }

        public abstract void SetName(string name);

        public abstract void GetRootNode(out IDebugApplicationNode node);

        public abstract void CreateApplicationNode(out IDebugApplicationNode node);

        public abstract uint GetDebugger(out IApplicationDebugger debugger);

        public abstract void Close();
    }

    internal sealed class DebugApplicationWrapper32 : DebugApplicationWrapper
    {
        private readonly IDebugApplication32 debugApplication;

        public DebugApplicationWrapper32(IDebugApplication32 debugApplication)
        {
            this.debugApplication = debugApplication;
        }

        public static IDebugApplication32 Unwrap(DebugApplicationWrapper wrapper)
        {
            var wrapper32 = wrapper as DebugApplicationWrapper32;
            return (wrapper32 != null) ? wrapper32.debugApplication : null;
        }

        public override void SetName(string name)
        {
            debugApplication.SetName(name);
        }

        public override void GetRootNode(out IDebugApplicationNode node)
        {
            debugApplication.GetRootNode(out node);
        }

        public override void CreateApplicationNode(out IDebugApplicationNode node)
        {
            debugApplication.CreateApplicationNode(out node);
        }

        public override uint GetDebugger(out IApplicationDebugger debugger)
        {
            return debugApplication.GetDebugger(out debugger);
        }

        public override void Close()
        {
            debugApplication.Close();
        }
    }

    internal sealed class DebugApplicationWrapper64 : DebugApplicationWrapper
    {
        private readonly IDebugApplication64 debugApplication;

        public DebugApplicationWrapper64(IDebugApplication64 debugApplication)
        {
            this.debugApplication = debugApplication;
        }

        public static IDebugApplication64 Unwrap(DebugApplicationWrapper wrapper)
        {
            var wrapper64 = wrapper as DebugApplicationWrapper64;
            return (wrapper64 != null) ? wrapper64.debugApplication : null;
        }

        public override void SetName(string name)
        {
            debugApplication.SetName(name);
        }

        public override void GetRootNode(out IDebugApplicationNode node)
        {
            debugApplication.GetRootNode(out node);
        }

        public override void CreateApplicationNode(out IDebugApplicationNode node)
        {
            debugApplication.CreateApplicationNode(out node);
        }

        public override uint GetDebugger(out IApplicationDebugger debugger)
        {
            return debugApplication.GetDebugger(out debugger);
        }

        public override void Close()
        {
            debugApplication.Close();
        }
    }

    #endregion
}
