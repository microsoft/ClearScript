// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class BindSignature : IEquatable<BindSignature>
    {
        private readonly Type context;
        private readonly BindingFlags flags;
        private readonly TargetInfo targetInfo;
        private readonly string name;
        private readonly Type[] typeArgs;
        private readonly ArgInfo[] argData;

        public BindSignature(Type context, BindingFlags flags, HostTarget target, string name, Type[] typeArgs, object[] args)
        {
            this.context = context;
            this.flags = flags;
            targetInfo = new TargetInfo(target);
            this.name = name;
            this.typeArgs = typeArgs;

            argData = new ArgInfo[args.Length];
            for (var index = 0; index < args.Length; index++)
            {
                argData[index] = new ArgInfo(args[index]);
            }
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as BindSignature);
        }

        public override int GetHashCode()
        {
            var accumulator = new HashAccumulator();

            accumulator.Update(context);
            accumulator.Update((int)flags);
            targetInfo.UpdateHash(ref accumulator);
            accumulator.Update(name);

            foreach (var type in typeArgs)
            {
                accumulator.Update(type);
            }

            foreach (var argInfo in argData)
            {
                argInfo.UpdateHash(ref accumulator);
            }

            return accumulator.HashCode;
        }

        #endregion

        #region IEquatable<BindSignature> implementation

        public bool Equals(BindSignature that)
        {
            if (that is null)
            {
                return false;
            }

            if (context != that.context)
            {
                return false;
            }

            if (flags != that.flags)
            {
                return false;
            }

            if (!targetInfo.Equals(that.targetInfo))
            {
                return false;
            }

            if (name != that.name)
            {
                return false;
            }

            if (typeArgs.Length != that.typeArgs.Length)
            {
                return false;
            }

            for (var index = 0; index < typeArgs.Length; index++)
            {
                if (typeArgs[index] != that.typeArgs[index])
                {
                    return false;
                }
            }

            if (argData.Length != that.argData.Length)
            {
                return false;
            }

            for (var index = 0; index < argData.Length; index++)
            {
                if (!argData[index].Equals(that.argData[index]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Nested type: TargetKind

        private enum TargetKind
        {
            Static,
            Null,
            Instance
        }

        #endregion

        #region Nested type: TargetInfo

        private sealed class TargetInfo : IEquatable<TargetInfo>
        {
            private readonly TargetKind kind;
            private readonly Type targetType;
            private readonly Type instanceType;

            public TargetInfo(HostTarget target)
            {
                if (target is HostType)
                {
                    kind = TargetKind.Static;
                    targetType = target.Type;
                }
                else if (target.InvokeTarget is null)
                {
                    kind = TargetKind.Null;
                    targetType = target.Type;
                }
                else
                {
                    kind = TargetKind.Instance;
                    targetType = target.Type;

                    var tempType = target.InvokeTarget.GetType();
                    if (tempType != targetType)
                    {
                        instanceType = tempType;
                    }
                }
            }

            public void UpdateHash(ref HashAccumulator accumulator)
            {
                accumulator.Update((int)kind);
                accumulator.Update(targetType);
                accumulator.Update(instanceType);
            }

            #region Object overrides

            public override bool Equals(object obj)
            {
                return Equals(obj as TargetInfo);
            }

            public override int GetHashCode()
            {
                var accumulator = new HashAccumulator();
                UpdateHash(ref accumulator);
                return accumulator.HashCode;
            }

            #endregion

            #region IEquatable<TargetInfo> implementation

            public bool Equals(TargetInfo that)
            {
                return (that is not null) && (kind == that.kind) && (targetType == that.targetType) && (instanceType == that.instanceType);
            }

            #endregion
        }

        #endregion

        #region Nested type: ArgKind

        private enum ArgKind
        {
            Null,
            Zero,
            ByValue,
            Out,
            Ref
        }

        #endregion

        #region Nested type: ArgInfo

        private sealed class ArgInfo : IEquatable<ArgInfo>
        {
            private readonly ArgKind kind;
            private readonly Type type;

            public ArgInfo(object arg)
            {
                if (arg is null)
                {
                    kind = ArgKind.Null;
                    return;
                }

                if (arg is IOutArg outArg)
                {
                    kind = ArgKind.Out;
                    type = outArg.Type;
                    return;
                }

                if (arg is IRefArg refArg)
                {
                    kind = ArgKind.Ref;
                    type = refArg.Type;
                    return;
                }

                if (arg is HostType)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostType);
                    return;
                }

                if (arg is HostMethod)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostMethod);
                    return;
                }

                if (arg is HostIndexedProperty)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostIndexedProperty);
                    return;
                }

                if (arg is ScriptMethod)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(ScriptMethod);
                    return;
                }

                if (arg is HostObject hostObject)
                {
                    kind = hostObject.Target.IsZero() ? ArgKind.Zero : ArgKind.ByValue;
                    type = hostObject.Type;
                    return;
                }

                if (arg is HostVariable hostVariable)
                {
                    kind = hostVariable.Target.IsZero() ? ArgKind.Zero : ArgKind.ByValue;
                    type = hostVariable.Type;
                    return;
                }

                Debug.Assert(arg is not HostTarget);
                kind = arg.IsZero() ? ArgKind.Zero : ArgKind.ByValue;
                type = arg.GetType();
            }

            public void UpdateHash(ref HashAccumulator accumulator)
            {
                accumulator.Update((int)kind);
                accumulator.Update(type);
            }

            #region Object overrides

            public override bool Equals(object obj)
            {
                return Equals(obj as ArgInfo);
            }

            public override int GetHashCode()
            {
                var accumulator = new HashAccumulator();
                UpdateHash(ref accumulator);
                return accumulator.HashCode;
            }

            #endregion

            #region IEquatable<ArgInfo> implementation

            public bool Equals(ArgInfo that)
            {
                return (that is not null) && (kind == that.kind) && (type == that.type);
            }

            #endregion
        }

        #endregion

        #region Nested type: HashAccumulator

        private ref struct HashAccumulator
        {
            public int HashCode { get; private set; }

            public void Update(int value)
            {
                HashCode = unchecked((HashCode * 31) + value);
            }

            public void Update(object obj)
            {
                HashCode = unchecked((HashCode * 31) + (obj?.GetHashCode() ?? 0));
            }
        }

        #endregion
    }
}
