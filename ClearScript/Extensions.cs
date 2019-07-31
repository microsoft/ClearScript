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
            MiscHelpers.VerifyNonNullArgument(type, "type");
            MiscHelpers.VerifyNonNullArgument(engine, "engine");
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
            MiscHelpers.VerifyNonNullArgument(target, "target");
            MiscHelpers.VerifyNonNullArgument(engine, "engine");
            return HostItem.Wrap(engine, target, typeof(T));
        }
    }
}
