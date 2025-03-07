// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    internal sealed class V8FastHostItem : IHostItem
    {
        private static readonly Func<IntPtr, int, string> getPropertyName = static (pName, length) => PropertyNamePool.GetOrAdd(pName, length);

        public V8ScriptEngine Engine { get; }

        public IV8FastHostObject Target { get; }

        public HostItemFlags Flags { get; }

        public bool IsInvocable => Target.Operations is IV8FastHostMethodOperations or IV8FastHostFunctionOperations;

        internal V8FastHostItem(V8ScriptEngine engine, IV8FastHostObject target, HostItemFlags flags)
        {
            Engine = engine;
            Target = target;
            Flags = flags;
        }

        public static object Wrap(V8ScriptEngine engine, IV8FastHostObject target, HostItemFlags flags)
        {
            return engine.GetOrCreateFastHostItem(target, flags);
        }

        public void GetProperty(StdString.Ptr pName, V8Value.FastResult.Ptr pValue, out bool isCacheable)
        {
            var ctx = (self: this, name: StdString.GetValue(pName, getPropertyName), pValue, isCacheable: false);

            Engine.HostInvoke(
                static pCtx =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    var value = new V8FastResult(ctx.self.Engine, ctx.self.Flags, ctx.pValue);

                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        operations.GetProperty(target, ctx.name, value, out ctx.isCacheable);
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support property retrieval");
                    }
                },
                StructPtr.FromRef(ref ctx)
            );

            isCacheable = ctx.isCacheable;
        }

        public void SetProperty(StdString.Ptr pName, V8Value.FastArg.Ptr pValue)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var value = new V8FastArg(ctx.self.Engine, ctx.pValue, V8FastArgKind.PropertyValue);

                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        operations.SetProperty(target, ctx.name, value);
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support property assignment");
                    }
                },
                (self: this, name: StdString.GetValue(pName, getPropertyName), pValue)
            );
        }

        public V8FastHostPropertyFlags QueryProperty(StdString.Ptr pName)
        {
            return Engine.HostInvoke(
                static ctx =>
                {
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        return operations.QueryProperty(target, ctx.name);
                    }

                    return V8FastHostPropertyFlags.None;
                },
                (self: this, name: StdString.GetValue(pName, getPropertyName))
            );
        }

        public bool DeleteProperty(StdString.Ptr pName)
        {
            return Engine.HostInvoke(
                static ctx =>
                {
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        return operations.DeleteProperty(target, ctx.name);
                    }

                    return true;
                },
                (self: this, name: StdString.GetValue(pName, getPropertyName))
            );
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return Engine.HostInvoke(
                static target =>
                {
                    if (target.Operations is {} operations)
                    {
                        return operations.GetPropertyNames(target);
                    }

                    return Enumerable.Empty<string>();
                },
                Target
            );
        }

        public void GetProperty(int index, V8Value.FastResult.Ptr pValue)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var value = new V8FastResult(ctx.self.Engine, ctx.self.Flags, ctx.pValue);

                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        operations.GetProperty(target, ctx.index, value);
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support property retrieval");
                    }
                },
                (self: this, index, pValue)
            );
        }

        public void SetProperty(int index, V8Value.FastArg.Ptr pValue)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var value = new V8FastArg(ctx.self.Engine, ctx.pValue, V8FastArgKind.PropertyValue);

                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        operations.SetProperty(target, ctx.index, value);
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support property assignment");
                    }
                },
                (self: this, index, pValue)
            );
        }

        public V8FastHostPropertyFlags QueryProperty(int index)
        {
            return Engine.HostInvoke(
                static ctx =>
                {
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        return operations.QueryProperty(target, ctx.index);
                    }

                    return V8FastHostPropertyFlags.None;
                },
                (self: this, index)
            );
        }

        public bool DeleteProperty(int index)
        {
            return Engine.HostInvoke(
                static ctx =>
                {
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        return operations.DeleteProperty(target, ctx.index);
                    }

                    return true;
                },
                (self: this, index)
            );
        }

        public IEnumerable<int> GetPropertyIndices()
        {
            return Engine.HostInvoke(
                static target =>
                {
                    if (target.Operations is {} operations)
                    {
                        return operations.GetPropertyIndices(target);
                    }

                    return Enumerable.Empty<int>();
                },
                Target
            );
        }

        public void Invoke(bool asConstructor, int argCount, V8Value.FastArg.Ptr pArgs, V8Value.FastResult.Ptr pResult)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var result = new V8FastResult(ctx.self.Engine, ctx.self.Flags, ctx.pResult);

                    var target = ctx.self.Target;
                    if (target.Operations is IV8FastHostMethodOperations methodOperations)
                    {
                        if (ctx.asConstructor)
                        {
                            throw new NotSupportedException("The object does not support constructor invocation");
                        }

                        var args = new V8FastArgs(ctx.self.Engine, ctx.pArgs.ToSpan(ctx.argCount), V8FastArgKind.MethodArg);

                        methodOperations.Invoke(args, result);
                        if (!result.IsSet)
                        {
                            result.SetUndefined();
                        }
                    }
                    else if (target.Operations is IV8FastHostFunctionOperations functionOperations)
                    {
                        var args = new V8FastArgs(ctx.self.Engine, ctx.pArgs.ToSpan(ctx.argCount), V8FastArgKind.FunctionArg);

                        functionOperations.Invoke(ctx.asConstructor, args, result);
                        if (!result.IsSet)
                        {
                            result.SetUndefined();
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support invocation");
                    }
                },
                (self: this, asConstructor, argCount, pArgs, pResult)
            );
        }

        public void CreateEnumerator(V8Value.FastResult.Ptr pResult)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var result = new V8FastResult(ctx.self.Engine, ctx.self.Flags, ctx.pResult); 
                    
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        var enumerator = operations.CreateEnumerator(target);
                        if (enumerator is not null)
                        {
                            result.Set(new ScriptableEnumerator(enumerator));
                            return;
                        }
                    }

                    throw new NotSupportedException("The object is not enumerable");
                },
                (self: this, pResult)
            );
        }

        public void CreateAsyncEnumerator(V8Value.FastResult.Ptr pResult)
        {
            Engine.HostInvoke(
                static ctx =>
                {
                    var result = new V8FastResult(ctx.self.Engine, ctx.self.Flags, ctx.pResult); 
                    
                    var target = ctx.self.Target;
                    if (target.Operations is {} operations)
                    {
                        var enumerator = operations.CreateAsyncEnumerator(target);
                        if (enumerator is not null)
                        {
                            result.Set(new ScriptableAsyncEnumerator(ctx.self.Engine, enumerator));
                            return;
                        }
                    }

                    throw new NotSupportedException("The object is not async-enumerable");
                },
                (self: this, pResult)
            );
        }

        #region Object overrides

        public override string ToString()
        {
            if ((Target.Operations is {} operations) && (operations.GetFriendlyName(Target) is {} friendlyName))
            {
                return $"[{friendlyName}]";
            }

            return $"[FastHostObject:{Target.GetFriendlyName(null)}]";
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        ScriptEngine IScriptMarshalWrapper.Engine => Engine;

        public object Unwrap() => Target;

        #endregion

        #region Nested type: PropertyNamePool

        private static class PropertyNamePool
        {
            private static readonly ConcurrentDictionary<Key, string> pool = new(new Key.Comparer());

            public static unsafe string GetOrAdd(IntPtr pName, int length)
            {
                if (!pool.TryGetValue(new Key(pName, length), out var name))
                {
                    name = new string((char*)pName, 0, length);
                    name = pool.GetOrAdd(new Key(name, length), name);
                }

                return name;
            }

            #region Nested type: Key

            private readonly struct Key
            {
                private readonly IntPtr pName;
                private readonly string name;
                private readonly int length;

                public Key(IntPtr pName, int length)
                {
                    this.pName = pName;
                    name = null;
                    this.length = length;
                }

                public Key(string name, int length)
                {
                    pName = IntPtr.Zero;
                    this.name = name;
                    this.length = length;
                }

                #region Nested type: Comparer

                public sealed class Comparer : IEqualityComparer<Key>
                {
                    private static unsafe bool Equals(char* pLeftName, char* pRightName, int length)
                    {
                        for (var index = 0; index < length; index++)
                        {
                            if (pRightName[index] != pLeftName[index])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    private static unsafe bool Equals(char* pLeftName, string rightName, int length)
                    {
                        for (var index = 0; index < length; index++)
                        {
                            if (rightName[index] != pLeftName[index])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    private static unsafe bool Equals(string leftName, char* pRightName, int length)
                    {
                        for (var index = 0; index < length; index++)
                        {
                            if (pRightName[index] != leftName[index])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    private static bool Equals(string leftName, string rightName, int length)
                    {
                        for (var index = 0; index < length; index++)
                        {
                            if (rightName[index] != leftName[index])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    #region IEqualityComparer<Key> implementation

                    unsafe bool IEqualityComparer<Key>.Equals(Key left, Key right)
                    {
                        if (right.length != left.length)
                        {
                            return false;
                        }

                        if (left.name is null)
                        {
                            return (right.name is null) ? Equals((char*)left.pName, (char*)right.pName, left.length) : Equals((char*)left.pName, right.name, left.length);
                        }

                        return (right.name is null) ? Equals(left.name, (char*)right.pName, left.length) : Equals(left.name, right.name, left.length);
                    }

                    unsafe int IEqualityComparer<Key>.GetHashCode(Key key)
                    {
                        // DJB2 hash algorithm

                        ulong hashCode = 5381;

                        var length = key.length;
                        if (key.name is {} name)
                        {
                            for (var index = 0; index < length; index++)
                            {
                                hashCode = unchecked(((hashCode << 5) + hashCode) + name[index]);
                            }
                        }
                        else
                        {
                            var pName = (char*)key.pName;
                            for (var index = 0; index < length; index++)
                            {
                                hashCode = unchecked(((hashCode << 5) + hashCode) + pName[index]);
                            }
                        }

                        return unchecked((int)hashCode);
                    }

                    #endregion
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: ScriptableEnumerator

        private sealed class ScriptableEnumerator : V8FastHostObject<ScriptableEnumerator>
        {
            private readonly IV8FastEnumerator enumerator;

            static ScriptableEnumerator()
            {
                Configure(configuration =>
                {
                    configuration.AddPropertyGetter("ScriptableCurrent", static (ScriptableEnumerator instance, in V8FastResult value) => instance.enumerator.GetCurrent(value));
                    configuration.AddMethodGetter("ScriptableMoveNext", static (ScriptableEnumerator instance, in V8FastArgs _, in V8FastResult result) => result.Set(instance.enumerator.MoveNext()));
                    configuration.AddMethodGetter("ScriptableDispose", static (ScriptableEnumerator instance, in V8FastArgs _, in V8FastResult _) => instance.enumerator.Dispose());
                });
            }

            public ScriptableEnumerator(IV8FastEnumerator enumerator) => this.enumerator = enumerator;
        }

        #endregion

        #region Nested type: ScriptableAsyncEnumerator

        private sealed class ScriptableAsyncEnumerator : V8FastHostObject<ScriptableAsyncEnumerator>
        {
            private readonly V8ScriptEngine engine;
            private readonly IV8FastAsyncEnumerator enumerator;

            static ScriptableAsyncEnumerator()
            {
                Configure(configuration =>
                {
                    configuration.AddPropertyGetter("ScriptableCurrent", static (ScriptableAsyncEnumerator instance, in V8FastResult value) => instance.enumerator.GetCurrent(value));
                    configuration.AddMethodGetter("ScriptableMoveNextAsync", static (ScriptableAsyncEnumerator instance, in V8FastArgs _, in V8FastResult result) => result.Set(instance.enumerator.MoveNextAsync().ToPromise(instance.engine)));
                    configuration.AddMethodGetter("ScriptableDisposeAsync", static (ScriptableAsyncEnumerator instance, in V8FastArgs _, in V8FastResult result) => result.Set(instance.enumerator.DisposeAsync().ToPromise(instance.engine)));
                });
            }

            public ScriptableAsyncEnumerator(V8ScriptEngine engine, IV8FastAsyncEnumerator enumerator)
            {
                this.engine = engine;
                this.enumerator = enumerator;
            }
        }

        #endregion
    }
}
