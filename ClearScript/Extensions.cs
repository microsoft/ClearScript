// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines extension methods for use with all script engines.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a type to a host type for use with script code currently running on the
        /// calling thread.
        /// </summary>
        /// <param name="type">The type to convert to a host type.</param>
        /// <returns>A host type for use with script code.</returns>
        public static object ToHostType(this Type type)
        {
            return type.ToHostType(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a type to a host type for use with script code running in the specified
        /// script engine.
        /// </summary>
        /// <param name="type">The type to convert to a host type.</param>
        /// <param name="engine">The script engine in which the host type will be used.</param>
        /// <returns>A host type for use with script code.</returns>
        public static object ToHostType(this Type type, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(type, nameof(type));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));
            return HostItem.Wrap(engine, HostType.Wrap(type));
        }

        /// <summary>
        /// Converts an object to a host object with the specified type restriction, for use with
        /// script code currently running on the calling thread.
        /// </summary>
        /// <typeparam name="T">The type whose members are to be made accessible from script code.</typeparam>
        /// <param name="target">The object to convert to a host object for use with script code.</param>
        /// <returns>A host object with the specified type restriction.</returns>
        public static object ToRestrictedHostObject<T>(this T target)
        {
            return target.ToRestrictedHostObject(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts an object to a host object with the specified type restriction, for use with
        /// script code running in the specified script engine.
        /// </summary>
        /// <typeparam name="T">The type whose members are to be made accessible from script code.</typeparam>
        /// <param name="target">The object to convert to a host object for use with script code.</param>
        /// <param name="engine">The script engine in which the host object will be used.</param>
        /// <returns>A host object with the specified type restriction.</returns>
        public static object ToRestrictedHostObject<T>(this T target, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(target, nameof(target));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));
            return HostItem.Wrap(engine, target, typeof(T));
        }

        /// <summary>
        /// Converts an object to a host object with a type restriction specified as a
        /// <c><see cref="Type"/></c> instance, for use with script code currently running on the
        /// calling thread.
        /// </summary>
        /// <param name="target">The object to convert to a host object for use with script code.</param>
        /// <param name="type">The type whose members are to be made accessible from script code.</param>
        /// <returns>A host object with the specified type restriction.</returns>
        public static object ToRestrictedHostObject(this object target, Type type)
        {
            return target.ToRestrictedHostObject(type, ScriptEngine.Current);
        }

        /// <summary>
        /// Converts an object to a host object with a type restriction specified as a
        /// <c><see cref="Type"/></c> instance, for use with script code running in the specified
        /// script engine.
        /// </summary>
        /// <param name="target">The object to convert to a host object for use with script code.</param>
        /// <param name="type">The type whose members are to be made accessible from script code.</param>
        /// <param name="engine">The script engine in which the host object will be used.</param>
        /// <returns>A host object with the specified type restriction.</returns>
        public static object ToRestrictedHostObject(this object target, Type type, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(target, nameof(target));
            MiscHelpers.VerifyNonNullArgument(type, nameof(type));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            if (!MiscHelpers.Try(out var holder, static type => Activator.CreateInstance(typeof(Holder<>).MakeGenericType(type)), type))
            {
                throw new ArgumentException("The specified type is invalid", nameof(type));
            }

            if (!MiscHelpers.Try(static ctx => ((IHolder)ctx.holder).Value = ctx.target, (holder, target)))
            {
                throw new ArgumentException("The target object is incompatible with the specified type", nameof(target));
            }

            return HostItem.Wrap(engine, target, type);
        }
    }
}
