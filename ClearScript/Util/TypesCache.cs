using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Cache of host members
    /// </summary>
    public static class TypesCache
    {
        internal readonly struct MemberKey
        {
            public readonly Type Type;
            public readonly string Name;
            public readonly BindingFlags Flags;
            public readonly Type AccessContext;
            public readonly ScriptAccess Access;

            public MemberKey(Type type, string name, BindingFlags flags, Type accessContext, ScriptAccess access)
            {
                Type = type;
                Name = name;
                Flags = flags;
                AccessContext = accessContext;
                Access = access;
            }
        }

        private class MemberKeyEqualityComparer : IEqualityComparer<MemberKey>
        {
            public bool Equals(MemberKey x, MemberKey y)
            {
                return x.Type == y.Type && x.Name == y.Name && x.Flags == y.Flags && x.AccessContext == y.AccessContext && x.Access == y.Access;
            }

            public int GetHashCode(MemberKey obj)
            {
                unchecked
                {
                    var hashCode = (obj.Type != null ? obj.Type.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int)obj.Flags;
                    hashCode = (hashCode * 397) ^ (obj.AccessContext != null ? obj.AccessContext.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int)obj.Access;
                    return hashCode;
                }
            }
        }

        internal class PropertyCachedResult
        {
            internal readonly PropertyInfo PropertyInfo;

            public PropertyCachedResult(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo;
            }
        }
        
        internal class EventCachedResult
        {
            internal readonly EventInfo EventInfo;

            public EventCachedResult(EventInfo eventInfo)
            {
                EventInfo = eventInfo;
            }
        }

        private static readonly MemberKeyEqualityComparer memberKeyEqualityComparer = new MemberKeyEqualityComparer();

        private static readonly Dictionary<MemberKey, PropertyCachedResult> cachedProperties = new Dictionary<MemberKey, PropertyCachedResult>(memberKeyEqualityComparer);
        private static readonly Dictionary<MemberKey, EventCachedResult> cachedEvents = new Dictionary<MemberKey, EventCachedResult>(memberKeyEqualityComparer);

        internal static MemberKey GetMemberKey(Type type, string name, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            lock (cachedProperties)
            {
                return new MemberKey(type, name, bindFlags, accessContext, defaultAccess);
            }
        }

        internal static bool TryGetProperty(in MemberKey propertyKey, out PropertyCachedResult propertyCachedResult)
        {
            lock (cachedProperties)
            {
                return cachedProperties.TryGetValue(propertyKey, out propertyCachedResult);
            }
        }

        internal static void SetPropertyValue(in MemberKey propertyKey, PropertyInfo propertyInfo)
        {
            lock (cachedProperties)
            {
                cachedProperties[propertyKey] = new PropertyCachedResult(propertyInfo);
            }
        }

        internal static bool TryGetEvent(in MemberKey propertyKey, out EventCachedResult eventCachedResult)
        {
            lock (cachedEvents)
            {
                return cachedEvents.TryGetValue(propertyKey, out eventCachedResult);
            }
        }

        internal static void SetEventValue(in MemberKey eventKey, EventInfo eventInfo)
        {
            lock (cachedEvents)
            {
                cachedEvents[eventKey] = new EventCachedResult(eventInfo);
            }
        }
    }
}