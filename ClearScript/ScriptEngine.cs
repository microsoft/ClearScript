// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides the base implementation for all script engines.
    /// </summary>
    public abstract class ScriptEngine : IScriptEngine, IHostContext
    {
        #region data

        private Type accessContext;
        private ScriptAccess defaultAccess;
        private bool enforceAnonymousTypeAccess;
        private bool exposeHostObjectStaticMembers;
        private CustomAttributeLoader customAttributeLoader;

        private DocumentSettings documentSettings;
        private readonly DocumentSettings defaultDocumentSettings = new();

        private static readonly IUniqueNameManager nameManager = new UniqueNameManager();
        private static readonly object nullHostObjectProxy = new();
        [ThreadStatic] private static ScriptEngine currentEngine;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new script engine instance.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        [Obsolete("Use ScriptEngine(string name, string fileNameExtensions) instead.")]
        protected ScriptEngine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new script engine instance with the specified list of supported file name extensions.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        protected ScriptEngine(string name, string fileNameExtensions)
        {
            Name = nameManager.GetUniqueName(name, GetType().GetRootName());
            defaultDocumentSettings.FileNameExtensions = fileNameExtensions;
            extensionMethodTable = realExtensionMethodTable = new ExtensionMethodTable();
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets the script engine that is invoking a host member on the current thread.
        /// </summary>
        /// <remarks>
        /// If multiple script engines are invoking host members on the current thread, this
        /// property gets the one responsible for the most deeply nested invocation. If no script
        /// engines are invoking host members on the current thread, this property returns
        /// <c>null</c>.
        /// </remarks>
        public static ScriptEngine Current => currentEngine;

        #endregion

        #region IScriptEngine implementation

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public abstract string FileNameExtension { get; }

        /// <inheritdoc cref="IScriptEngine.AccessContext" />
        public Type AccessContext
        {
            get => accessContext;

            set
            {
                accessContext = value;
                OnAccessSettingsChanged();
            }
        }

        /// <inheritdoc cref="IScriptEngine.DefaultAccess" />
        public ScriptAccess DefaultAccess
        {
            get => defaultAccess;

            set
            {
                defaultAccess = value;
                OnAccessSettingsChanged();
            }
        }

        /// <inheritdoc/>
        public bool EnforceAnonymousTypeAccess
        {
            get => enforceAnonymousTypeAccess;

            set
            {
                enforceAnonymousTypeAccess = value;
                OnAccessSettingsChanged();
            }
        }

        /// <inheritdoc/>
        public bool ExposeHostObjectStaticMembers
        {
            get => exposeHostObjectStaticMembers;

            set
            {
                exposeHostObjectStaticMembers = value;
                OnAccessSettingsChanged();
            }
        }

        /// <inheritdoc/>
        public bool DisableExtensionMethods
        {
            get => extensionMethodTable == emptyExtensionMethodTable;

            set
            {
                var newExtensionMethodTable = value ? emptyExtensionMethodTable : realExtensionMethodTable;
                if (newExtensionMethodTable != extensionMethodTable)
                {
                    ScriptInvoke(
                        static ctx =>
                        {
                            if (ctx.newExtensionMethodTable != ctx.self.extensionMethodTable)
                            {
                                ctx.self.extensionMethodTable = ctx.newExtensionMethodTable;
                                ctx.self.ClearMethodBindCache();
                                ctx.self.OnAccessSettingsChanged();
                            }
                        },
                        (self: this, newExtensionMethodTable)
                    );
                }
            }
        }

        /// <inheritdoc/>
        public bool FormatCode { get; set; }

        /// <inheritdoc/>
        public bool AllowReflection { get; set; }

        /// <inheritdoc/>
        public bool DisableTypeRestriction { get; set; }

        /// <inheritdoc/>
        public bool DisableListIndexTypeRestriction { get; set; }

        /// <inheritdoc/>
        public bool EnableNullResultWrapping { get; set; }

        /// <inheritdoc/>
        public bool DisableFloatNarrowing { get; set; }

        /// <inheritdoc/>
        public bool DisableDynamicBinding { get; set; }

        /// <inheritdoc/>
        public bool UseReflectionBindFallback { get; set; }

        /// <inheritdoc/>
        public bool EnableAutoHostVariables { get; set; }

        /// <inheritdoc/>
        public object UndefinedImportValue { get; set; } = Undefined.Value;

        /// <inheritdoc/>
        public object NullImportValue { get; set; }

        /// <inheritdoc/>
        public object NullExportValue { get; set; }

        /// <inheritdoc/>
        public object VoidResultValue { get; set; } = VoidResult.Value;

        /// <inheritdoc/>
        public ContinuationCallback ContinuationCallback { get; set; }

        /// <inheritdoc/>
        public abstract dynamic Script { get; }

        /// <inheritdoc/>
        public abstract ScriptObject Global { get; }

        /// <inheritdoc/>
        public DocumentSettings DocumentSettings
        {
            get => documentSettings ?? defaultDocumentSettings;
            set => documentSettings = value;
        }

        /// <inheritdoc cref="IScriptEngine.CustomAttributeLoader" />
        public CustomAttributeLoader CustomAttributeLoader
        {
            get => customAttributeLoader;

            set
            {
                customAttributeLoader = value;
                OnAccessSettingsChanged();
            }
        }

        /// <inheritdoc/>
        public object HostData { get; set; }

        /// <inheritdoc/>
        public void AddHostObject(string itemName, object target)
        {
            AddHostObject(itemName, HostItemFlags.None, target);
        }

        /// <inheritdoc/>
        public void AddHostObject(string itemName, HostItemFlags flags, object target)
        {
            MiscHelpers.VerifyNonNullArgument(target, nameof(target));
            AddHostItem(itemName, flags, target);
        }

        /// <inheritdoc/>
        public void AddRestrictedHostObject<T>(string itemName, T target)
        {
            AddRestrictedHostObject(itemName, HostItemFlags.None, target);
        }

        /// <inheritdoc/>
        public void AddRestrictedHostObject<T>(string itemName, HostItemFlags flags, T target)
        {
            AddHostItem(itemName, flags, HostItem.Wrap(this, target, typeof(T)));
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, string progID)
        {
            AddCOMObject(itemName, HostItemFlags.None, progID);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, string progID, string serverName)
        {
            AddCOMObject(itemName, HostItemFlags.None, progID, serverName);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, HostItemFlags flags, string progID)
        {
            AddCOMObject(itemName, flags, progID, null);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, HostItemFlags flags, string progID, string serverName)
        {
            AddHostItem(itemName, flags, MiscHelpers.CreateCOMObject(progID, serverName));
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, Guid clsid)
        {
            AddCOMObject(itemName, HostItemFlags.None, clsid);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, Guid clsid, string serverName)
        {
            AddCOMObject(itemName, HostItemFlags.None, clsid, serverName);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid)
        {
            AddCOMObject(itemName, flags, clsid, null);
        }

        /// <inheritdoc/>
        public void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid, string serverName)
        {
            AddHostItem(itemName, flags, MiscHelpers.CreateCOMObject(clsid, serverName));
        }

        /// <inheritdoc/>
        public void AddHostType(Type type)
        {
            AddHostType(HostItemFlags.None, type);
        }

        /// <inheritdoc/>
        public void AddHostType(HostItemFlags flags, Type type)
        {
            AddHostType(type.GetRootName(), flags, type);
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, Type type)
        {
            AddHostType(itemName, HostItemFlags.None, type);
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, HostItemFlags flags, Type type)
        {
            MiscHelpers.VerifyNonNullArgument(type, nameof(type));
            AddHostItem(itemName, flags, HostType.Wrap(type));
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, string typeName, params Type[] typeArgs)
        {
            AddHostType(itemName, HostItemFlags.None, typeName, typeArgs);
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, HostItemFlags flags, string typeName, params Type[] typeArgs)
        {
            AddHostItem(itemName, flags, TypeHelpers.ImportType(typeName, null, false, typeArgs));
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddHostType(itemName, HostItemFlags.None, typeName, assemblyName, typeArgs);
        }

        /// <inheritdoc/>
        public void AddHostType(string itemName, HostItemFlags flags, string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddHostItem(itemName, flags, TypeHelpers.ImportType(typeName, assemblyName, true, typeArgs));
        }

        /// <inheritdoc/>
        public void AddHostTypes(params Type[] types)
        {
            if (types is not null)
            {
                foreach (var type in types)
                {
                    if (type is not null)
                    {
                        AddHostType(type);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, string progID)
        {
            AddCOMType(itemName, HostItemFlags.None, progID);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, string progID, string serverName)
        {
            AddCOMType(itemName, HostItemFlags.None, progID, serverName);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, HostItemFlags flags, string progID)
        {
            AddCOMType(itemName, flags, progID, null);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, HostItemFlags flags, string progID, string serverName)
        {
            AddHostItem(itemName, flags, HostType.Wrap(MiscHelpers.GetCOMType(progID, serverName)));
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, Guid clsid)
        {
            AddCOMType(itemName, HostItemFlags.None, clsid);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, Guid clsid, string serverName)
        {
            AddCOMType(itemName, HostItemFlags.None, clsid, serverName);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, HostItemFlags flags, Guid clsid)
        {
            AddCOMType(itemName, flags, clsid, null);
        }

        /// <inheritdoc/>
        public void AddCOMType(string itemName, HostItemFlags flags, Guid clsid, string serverName)
        {
            AddHostItem(itemName, flags, HostType.Wrap(MiscHelpers.GetCOMType(clsid, serverName)));
        }

        /// <inheritdoc/>
        public void Execute(string code)
        {
            Execute(null, code);
        }

        /// <inheritdoc/>
        public void Execute(string documentName, string code)
        {
            Execute(documentName, false, code);
        }

        /// <inheritdoc/>
        public void Execute(string documentName, bool discard, string code)
        {
            Execute(new DocumentInfo(documentName) { Flags = discard ? DocumentFlags.IsTransient : DocumentFlags.None }, code);
        }

        /// <inheritdoc/>
        public void Execute(DocumentInfo documentInfo, string code)
        {
            Execute(documentInfo.MakeUnique(this), code, false);
        }

        /// <inheritdoc/>
        public void ExecuteDocument(string specifier)
        {
            ExecuteDocument(specifier, null);
        }

        /// <inheritdoc/>
        public void ExecuteDocument(string specifier, DocumentCategory category)
        {
            ExecuteDocument(specifier, category, null);
        }

        /// <inheritdoc/>
        public void ExecuteDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            Execute(document.Info, document.GetTextContents());
        }

        /// <inheritdoc/>
        public virtual string ExecuteCommand(string command)
        {
            var documentInfo = new DocumentInfo("Command") { Flags = DocumentFlags.IsTransient };
            return GetCommandResultString(Evaluate(documentInfo.MakeUnique(this), command, false));
        }

        /// <inheritdoc/>
        public object Evaluate(string code)
        {
            return Evaluate(null, code);
        }

        /// <inheritdoc/>
        public object Evaluate(string documentName, string code)
        {
            return Evaluate(documentName, true, code);
        }

        /// <inheritdoc/>
        public object Evaluate(string documentName, bool discard, string code)
        {
            return Evaluate(new DocumentInfo(documentName) { Flags = discard ? DocumentFlags.IsTransient : DocumentFlags.None }, code);
        }

        /// <inheritdoc/>
        public object Evaluate(DocumentInfo documentInfo, string code)
        {
            return Evaluate(documentInfo.MakeUnique(this, DocumentFlags.IsTransient), code, true);
        }

        /// <inheritdoc/>
        public object EvaluateDocument(string specifier)
        {
            return EvaluateDocument(specifier, null);
        }

        /// <inheritdoc/>
        public object EvaluateDocument(string specifier, DocumentCategory category)
        {
            return EvaluateDocument(specifier, category, null);
        }

        /// <inheritdoc/>
        public object EvaluateDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            return Evaluate(document.Info, document.GetTextContents());
        }

        /// <inheritdoc/>
        public object Invoke(string funcName, params object[] args)
        {
            MiscHelpers.VerifyNonBlankArgument(funcName, nameof(funcName), "Invalid function name");
            return Global.InvokeMethod(funcName, args ?? ArrayHelpers.GetEmptyArray<object>());
        }

        /// <inheritdoc/>
        public abstract string GetStackTrace();

        /// <inheritdoc/>
        public abstract void Interrupt();

        /// <inheritdoc/>
        public abstract void CollectGarbage(bool exhaustive);

        #endregion

        #region internal members

        internal abstract IUniqueNameManager DocumentNameManager { get; }

        internal virtual bool EnumerateInstanceMethods => true;

        internal virtual bool EnumerateExtensionMethods => EnumerateInstanceMethods;

        internal virtual bool UseCaseInsensitiveMemberBinding => false;

        internal abstract void AddHostItem(string itemName, HostItemFlags flags, object item);

        internal object PrepareResult<T>(T result, ScriptMemberFlags flags, bool isListIndexResult)
        {
            return PrepareResult(result, typeof(T), flags, isListIndexResult);
        }

        internal virtual object PrepareResult(object result, Type type, ScriptMemberFlags flags, bool isListIndexResult)
        {
            var wrapNull = flags.HasAllFlags(ScriptMemberFlags.WrapNullResult) || EnableNullResultWrapping;
            if (wrapNull && (result is null))
            {
                return HostObject.WrapResult(null, type, true);
            }

            if (!flags.HasAllFlags(ScriptMemberFlags.ExposeRuntimeType) && !DisableTypeRestriction && (!isListIndexResult || !DisableListIndexTypeRestriction))
            {
                return HostObject.WrapResult(result, type, wrapNull);
            }

            return result;
        }

        internal abstract object MarshalToScript(object obj, HostItemFlags flags);

        internal object MarshalToScript(object obj)
        {
            var hostItem = obj as HostItem;
            return MarshalToScript(obj, hostItem?.Flags ?? HostItemFlags.None);
        }

        internal object[] MarshalToScript(object[] args)
        {
            return args.Select(MarshalToScript).ToArray();
        }

        internal abstract object MarshalToHost(object obj, bool preserveHostTarget);

        internal object[] MarshalToHost(object[] args, bool preserveHostTargets)
        {
            return args.Select(arg => MarshalToHost(arg, preserveHostTargets)).ToArray();
        }

        internal abstract object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate);

        internal abstract object ExecuteRaw(UniqueDocumentInfo documentInfo, string code, bool evaluate);

        internal object Evaluate(UniqueDocumentInfo documentInfo, string code, bool marshalResult)
        {
            var result = Execute(documentInfo, code, true);
            if (marshalResult)
            {
                result = MarshalToHost(result, false);
            }

            return result;
        }

        internal string GetCommandResultString(object result)
        {
            if (result is HostItem hostItem)
            {
                if (hostItem.Target is IHostVariable)
                {
                    return result.ToString();
                }
            }

            var marshaledResult = MarshalToHost(result, false);

            if (marshaledResult is VoidResult)
            {
                return null;
            }

            if (marshaledResult is null)
            {
                return "[null]";
            }

            if (marshaledResult is Undefined)
            {
                return marshaledResult.ToString();
            }

            if (marshaledResult is ScriptItem)
            {
                return "[ScriptObject]";
            }

            return result.ToString();
        }

        internal void RequestInterrupt()
        {
            // Some script engines don't support IActiveScript::InterruptScriptThread(). This
            // method provides an alternate mechanism based on IActiveScriptSiteInterruptPoll.

            var tempScriptFrame = CurrentScriptFrame;
            if (tempScriptFrame is not null)
            {
                tempScriptFrame.InterruptRequested = true;
            }
        }

        internal void CheckReflection()
        {
            if (!AllowReflection)
            {
                throw new UnauthorizedAccessException("Use of reflection is prohibited in this script engine");
            }
        }

        internal virtual void OnAccessSettingsChanged()
        {
            ClearConstructorBindCache();
            ClearPropertyBindCache();
        }

        #endregion

        #region host-side invocation

        internal virtual void HostInvoke(Action action)
        {
            action();
        }

        internal virtual void HostInvoke<TArg>(Action<TArg> action, in TArg arg)
        {
            action(arg);
        }

        internal virtual TResult HostInvoke<TResult>(Func<TResult> func)
        {
            return func();
        }

        internal virtual TResult HostInvoke<TArg, TResult>(Func<TArg, TResult> func, in TArg arg)
        {
            return func(arg);
        }

        #endregion

        #region script-side invocation

        internal ScriptFrame CurrentScriptFrame { get; private set; }

        internal ValueScope<ScriptEngine> CreateEngineScope()
        {
            return ScopeFactory.Create(static engine => MiscHelpers.Exchange(ref currentEngine, engine), static previousEngine => currentEngine = previousEngine, this);
        }

        internal virtual void ScriptInvoke(Action action)
        {
            using (CreateEngineScope())
            {
                ScriptInvokeInternal(action);
            }
        }

        internal virtual void ScriptInvoke<TArg>(Action<TArg> action, in TArg arg)
        {
            using (CreateEngineScope())
            {
                ScriptInvokeInternal(action, arg);
            }
        }

        internal virtual TResult ScriptInvoke<TResult>(Func<TResult> func)
        {
            using (CreateEngineScope())
            {
                return ScriptInvokeInternal(func);
            }
        }

        internal virtual TResult ScriptInvoke<TArg, TResult>(Func<TArg, TResult> func, in TArg arg)
        {
            using (CreateEngineScope())
            {
                return ScriptInvokeInternal(func, arg);
            }
        }

        internal void ScriptInvokeInternal(Action action)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                action();
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal void ScriptInvokeInternal<TArg>(Action<TArg> action, in TArg arg)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                action(arg);
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal TResult ScriptInvokeInternal<TResult>(Func<TResult> func)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                return func();
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal TResult ScriptInvokeInternal<TArg, TResult>(Func<TArg, TResult> func, in TArg arg)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                return func(arg);
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal void ThrowScriptError()
        {
            if (CurrentScriptFrame is not null)
            {
                ThrowScriptError(CurrentScriptFrame.ScriptError);
            }
        }

        internal static void ThrowScriptError(IScriptEngineException scriptError)
        {
            if (scriptError is not null)
            {
                if (scriptError is ScriptInterruptedException)
                {
                    throw new ScriptInterruptedException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptExceptionAsObject, scriptError.InnerException);
                }

                throw new ScriptEngineException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptExceptionAsObject, scriptError.InnerException);
            }
        }

        #endregion

        #region synchronized invocation

        internal virtual void SyncInvoke(Action action)
        {
            action();
        }

        internal virtual T SyncInvoke<T>(Func<T> func)
        {
            return func();
        }

        #endregion

        #region enumeration settings

        internal object EnumerationSettingsToken { get; private set; } = new();

        internal void OnEnumerationSettingsChanged()
        {
            EnumerationSettingsToken = new object();
        }

        #endregion

        #region extension method table

        private static readonly ExtensionMethodTable emptyExtensionMethodTable = new();

        private readonly ExtensionMethodTable realExtensionMethodTable;
        private ExtensionMethodTable extensionMethodTable;

        internal void ProcessExtensionMethodType(Type type)
        {
            if (extensionMethodTable != emptyExtensionMethodTable)
            {
                if (extensionMethodTable.ProcessType(this, type))
                {
                    ClearMethodBindCache();
                }
            }
        }

        internal ExtensionMethodSummary ExtensionMethodSummary => extensionMethodTable.Summary;

        internal void RebuildExtensionMethodSummary()
        {
            if (extensionMethodTable != emptyExtensionMethodTable)
            {
                extensionMethodTable.RebuildSummary(this);
            }
        }

        #endregion

        #region constructor bind cache

        private readonly Dictionary<BindSignature, ConstructorInfo> constructorBindCache = new();

        internal void CacheConstructorBindResult(BindSignature signature, ConstructorInfo result)
        {
            constructorBindCache[signature] = result;
        }

        internal bool TryGetCachedConstructorBindResult(BindSignature signature, out ConstructorInfo result)
        {
            return constructorBindCache.TryGetValue(signature, out result);
        }

        private void ClearConstructorBindCache()
        {
            constructorBindCache.Clear();
        }

        #endregion

        #region method bind cache

        private readonly Dictionary<BindSignature, object> methodBindCache = new();

        internal void CacheMethodBindResult(BindSignature signature, object result)
        {
            methodBindCache[signature] = result;
        }

        internal bool TryGetCachedMethodBindResult(BindSignature signature, out object result)
        {
            return methodBindCache.TryGetValue(signature, out result);
        }

        private void ClearMethodBindCache()
        {
            methodBindCache.Clear();
        }

        #endregion

        #region property bind cache

        private readonly Dictionary<BindSignature, MemberInfo> propertyGetBindCache = new();
        private readonly Dictionary<BindSignature, MemberInfo> propertySetBindCache = new();

        internal void CachePropertyGetBindResult(BindSignature signature, MemberInfo property)
        {
            propertyGetBindCache[signature] = property;
        }

        internal bool TryGetCachedPropertyGetBindResult(BindSignature signature, out MemberInfo property)
        {
            return propertyGetBindCache.TryGetValue(signature, out property);
        }

        internal void CachePropertySetBindResult(BindSignature signature, MemberInfo property)
        {
            propertySetBindCache[signature] = property;
        }

        internal bool TryGetCachedPropertySetBindResult(BindSignature signature, out MemberInfo property)
        {
            return propertySetBindCache.TryGetValue(signature, out property);
        }

        private void ClearPropertyBindCache()
        {
            propertyGetBindCache.Clear();
            propertySetBindCache.Clear();
        }

        #endregion

        #region host item cache

        private readonly ConditionalWeakTable<object, List<WeakReference<HostItem>>> hostObjectHostItemCache = new();
        private readonly ConditionalWeakTable<Type, List<WeakReference<HostItem>>> hostTypeHostItemCache = new();

        internal HostItem GetOrCreateHostItem(object target, Type type, bool isCanonicalRef, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            return GetOrCreateHostItemForHostObject(null, target, type, isCanonicalRef, flags, createHostItem);
        }

        internal HostItem GetOrCreateHostItem(HostTarget target, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            if (target is HostObject hostObject)
            {
                return GetOrCreateHostItemForHostObject(hostObject, hostObject.Target, hostObject.Type, true, flags, createHostItem);
            }

            if (target is HostType hostType)
            {
                return GetOrCreateHostItemForHostType(hostType, flags, createHostItem);
            }

            if (target is HostMethod hostMethod)
            {
                return GetOrCreateHostItemForHostObject(hostMethod, hostMethod, hostMethod.Type, true, flags, createHostItem);
            }

            if (target is HostVariable hostVariable)
            {
                return GetOrCreateHostItemForHostObject(hostVariable, hostVariable, hostVariable.Type, true, flags, createHostItem);
            }

            if (target is HostIndexedProperty hostIndexedProperty)
            {
                return GetOrCreateHostItemForHostObject(hostIndexedProperty, hostIndexedProperty, hostIndexedProperty.Type, true, flags, createHostItem);
            }

            return CreateHostItem(target, flags, createHostItem, null);
        }

        private HostItem GetOrCreateHostItemForHostObject(HostTarget hostTarget, object target, Type type, bool isCanonicalRef, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            var cacheEntry = hostObjectHostItemCache.GetOrCreateValue(target ?? nullHostObjectProxy);

            List<WeakReference<HostItem>> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                if (!weakRef.TryGetTarget(out var hostItem))
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if ((hostItem.Target.Type == type) && (hostItem.Flags == flags))
                    {
                        return hostItem;
                    }

                    if (activeWeakRefs is null)
                    {
                        activeWeakRefs = new List<WeakReference<HostItem>>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs is not null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            return CreateHostItem(hostTarget ?? HostObject.Wrap(target, type, isCanonicalRef), flags, createHostItem, cacheEntry);
        }

        private HostItem GetOrCreateHostItemForHostType(HostType hostType, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            if (hostType.Types.Length != 1)
            {
                return CreateHostItem(hostType, flags, createHostItem, null);
            }

            var cacheEntry = hostTypeHostItemCache.GetOrCreateValue(hostType.Types[0]);

            List<WeakReference<HostItem>> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                if (!weakRef.TryGetTarget(out var hostItem))
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if (hostItem.Flags == flags)
                    {
                        return hostItem;
                    }

                    if (activeWeakRefs is null)
                    {
                        activeWeakRefs = new List<WeakReference<HostItem>>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs is not null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            return CreateHostItem(hostType, flags, createHostItem, cacheEntry);
        }

        private HostItem CreateHostItem(HostTarget hostTarget, HostItemFlags flags, HostItem.CreateFunc createHostItem, List<WeakReference<HostItem>> cacheEntry)
        {
            var newHostItem = createHostItem(this, hostTarget, flags);

            if (cacheEntry is not null)
            {
                cacheEntry.Add(new WeakReference<HostItem>(newHostItem));
            }

            if (hostTarget.Target is IScriptableObject scriptableObject)
            {
                scriptableObject.OnExposedToScriptCode(this);
            }

            return newHostItem;
        }

        #endregion

        #region host item collateral

        internal abstract HostItemCollateral HostItemCollateral { get; }

        #endregion

        #region shared host target member data

        internal readonly HostTargetMemberData SharedHostMethodMemberData = new();
        internal readonly HostTargetMemberData SharedHostIndexedPropertyMemberData = new();
        internal readonly HostTargetMemberData SharedScriptMethodMemberData = new();

        private readonly ConditionalWeakTable<Type, List<WeakReference<HostTargetMemberDataWithContext>>> sharedHostObjectMemberDataCache = new();

        internal HostTargetMemberData GetSharedHostObjectMemberData(HostObject target, CustomAttributeLoader targetCustomAttributeLoader, Type targetAccessContext, ScriptAccess targetDefaultAccess, HostTargetFlags targetFlags)
        {
            var cacheEntry = sharedHostObjectMemberDataCache.GetOrCreateValue(target.Type);

            List<WeakReference<HostTargetMemberDataWithContext>> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                if (!weakRef.TryGetTarget(out var memberData))
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if ((memberData.CustomAttributeLoader == targetCustomAttributeLoader) && (memberData.AccessContext == targetAccessContext) && (memberData.DefaultAccess == targetDefaultAccess) && (memberData.TargetFlags == targetFlags))
                    {
                        return memberData;
                    }

                    if (activeWeakRefs is null)
                    {
                        activeWeakRefs = new List<WeakReference<HostTargetMemberDataWithContext>>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs is not null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            var newMemberData = new HostTargetMemberDataWithContext(targetCustomAttributeLoader, targetAccessContext, targetDefaultAccess, targetFlags);
            cacheEntry.Add(new WeakReference<HostTargetMemberDataWithContext>(newMemberData));
            return newMemberData;
        }

        #endregion

        #region event connections

        private readonly EventConnectionMap eventConnectionMap = new();

        internal EventConnection CreateEventConnection(Type handlerType, object source, EventInfo eventInfo, Delegate handler)
        {
            return eventConnectionMap.Create(this, handlerType, source, eventInfo, handler);
        }

        internal EventConnection<T> CreateEventConnection<T>(object source, EventInfo eventInfo, Delegate handler)
        {
            return eventConnectionMap.Create<T>(this, source, eventInfo, handler);
        }

        internal void BreakEventConnection(EventConnection connection)
        {
            eventConnectionMap.Break(connection);
        }

        private void BreakAllEventConnections()
        {
            eventConnectionMap.Dispose();
        }

        #endregion

        #region disposal / finalization

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the script engine and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// This method is called by the public <c><see cref="Dispose()"/></c> method and the
        /// <c><see cref="Finalize">Finalize</see></c> method. <c><see cref="Dispose()"/></c> invokes the
        /// protected <c>Dispose(Boolean)</c> method with the <paramref name="disposing"/>
        /// parameter set to <c>true</c>. <c><see cref="Finalize">Finalize</see></c> invokes
        /// <c>Dispose(Boolean)</c> with <paramref name="disposing"/> set to <c>false</c>.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                BreakAllEventConnections();
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the script engine is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method overrides <c><see cref="System.Object.Finalize"/></c>. Application code should not
        /// call this method; an object's <c>Finalize()</c> method is automatically invoked during
        /// garbage collection, unless finalization by the garbage collector has been disabled by a
        /// call to <c><see cref="System.GC.SuppressFinalize"/></c>.
        /// </remarks>
        ~ScriptEngine()
        {
            Dispose(false);
        }

        #endregion

        #region IHostContext implementation

        ScriptEngine IHostContext.Engine => this;

        #endregion

        #region Nested type: ScriptFrame

        internal sealed class ScriptFrame
        {
            public Exception HostException { get; set; }

            public IScriptEngineException ScriptError { get; set; }

            public IScriptEngineException PendingScriptError { get; set; }

            public bool InterruptRequested { get; set; }
        }

        #endregion

        #region Nested type: EventConnectionMap

        private sealed class EventConnectionMap : IDisposable
        {
            private readonly HashSet<EventConnection> map = new();
            private readonly InterlockedOneWayFlag disposedFlag = new();

            internal EventConnection Create(ScriptEngine engine, Type handlerType, object source, EventInfo eventInfo, Delegate handler)
            {
                var connection = (EventConnection)typeof(EventConnection<>).MakeGenericType(handlerType).CreateInstance(BindingFlags.NonPublic, engine, source, eventInfo, handler);
                Add(connection);
                return connection;
            }

            internal EventConnection<T> Create<T>(ScriptEngine engine, object source, EventInfo eventInfo, Delegate handler)
            {
                var connection = new EventConnection<T>(engine, source, eventInfo, handler);
                Add(connection);
                return connection;
            }

            private void Add(EventConnection connection)
            {
                if (!disposedFlag.IsSet)
                {
                    lock (map)
                    {
                        map.Add(connection);
                    }
                }
            }

            internal void Break(EventConnection connection)
            {
                var mustBreak = true;

                if (!disposedFlag.IsSet)
                {
                    lock (map)
                    {
                        mustBreak = map.Remove(connection);
                    }
                }

                if (mustBreak)
                {
                    connection.Break();
                }
            }

            public void Dispose()
            {
                if (disposedFlag.Set())
                {
                    var connections = new List<EventConnection>();

                    lock (map)
                    {
                        connections.AddRange(map);
                    }

                    connections.ForEach(connection => connection.Break());
                }
            }
        }

        #endregion
    }
}
