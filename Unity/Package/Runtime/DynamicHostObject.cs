// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Dynamic;
using System.Linq;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Extends <c><see cref="DynamicObject"/></c> for enhanced behavior and performance in a scripting environment.
    /// </summary>
    public abstract class DynamicHostObject : DynamicObject
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="DynamicHostObject"/></c> instance.
        /// </summary>
        protected DynamicHostObject()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Determines whether the object has the specified named member.
        /// </summary>
        /// <param name="name">The member name for which to search.</param>
        /// <param name="ignoreCase"><c>True</c> to perform a case-insensitive search, <c>false</c> otherwise.</param>
        /// <returns><c>True</c> if the named member was found, <c>false</c> otherwise.</returns>
        public virtual bool HasMember(string name, bool ignoreCase)
        {
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            return GetDynamicMemberNames().Contains(name, comparer);
        }

        internal static bool HasMember(IDynamicMetaObjectProvider metaObjectProvider, DynamicMetaObject metaObject, string name, bool ignoreCase)
        {
            if (metaObjectProvider is DynamicHostObject hostObject)
            {
                return hostObject.HasMember(name, ignoreCase);
            }

            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            return metaObject.GetDynamicMemberNames().Contains(name, comparer);
        }
    }
}
