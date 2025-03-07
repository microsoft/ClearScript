// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostObjectOperations"/></c> based on a private configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    public class V8FastHostObjectOperations<TObject> : IV8FastHostObjectOperations where TObject : IV8FastHostObject
    {
        private readonly Configuration configuration;

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostObjectOperations{TObject}"/></c> instance.
        /// </summary>
        public V8FastHostObjectOperations()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostObjectOperations{TObject}"/></c> instance with the specified configuration callback.
        /// </summary>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        public V8FastHostObjectOperations(V8FastHostObjectConfigurator<TObject> configurator)
        {
            configuration = new Configuration();
            if (configurator is not null)
            {
                Configure(configurator);
            }
        }

        /// <summary>
        /// Prepares the configuration associated with the <c><see cref="V8FastHostObjectOperations{TObject}"/></c> instance.
        /// </summary>
        /// <param name="configurator">A callback for preparing the configuration associated with the <c><see cref="V8FastHostObjectOperations{TObject}"/></c> instance.</param>
        /// <remarks>
        /// Once prepared, a fast host object configuration cannot be modified.
        /// </remarks>
        public void Configure(V8FastHostObjectConfigurator<TObject> configurator) => configuration.Configure(configurator);

        #region IV8FastHostObjectOperations implementation

        string IV8FastHostObjectOperations.GetFriendlyName(IV8FastHostObject instance) => $"FastHostObject:{typeof(TObject).GetFriendlyName()}";

        void IV8FastHostObjectOperations.GetProperty(IV8FastHostObject instance, string name, in V8FastResult value, out bool isCacheable) => configuration.GetProperty(instance, name, value, out isCacheable);

        void IV8FastHostObjectOperations.SetProperty(IV8FastHostObject instance, string name, in V8FastArg value) => configuration.SetProperty(instance, name, value);

        V8FastHostPropertyFlags IV8FastHostObjectOperations.QueryProperty(IV8FastHostObject instance, string name) => configuration.QueryProperty(instance, name);

        bool IV8FastHostObjectOperations.DeleteProperty(IV8FastHostObject instance, string name) => configuration.DeleteProperty(instance, name);

        IEnumerable<string> IV8FastHostObjectOperations.GetPropertyNames(IV8FastHostObject instance) => configuration.GetPropertyNames(instance);

        void IV8FastHostObjectOperations.GetProperty(IV8FastHostObject instance, int index, in V8FastResult value) => configuration.GetProperty(instance, index, value);

        void IV8FastHostObjectOperations.SetProperty(IV8FastHostObject instance, int index, in V8FastArg value) => configuration.SetProperty(instance, index, value);

        V8FastHostPropertyFlags IV8FastHostObjectOperations.QueryProperty(IV8FastHostObject instance, int index) => configuration.QueryProperty(instance, index);

        bool IV8FastHostObjectOperations.DeleteProperty(IV8FastHostObject instance, int index) => configuration.DeleteProperty(instance, index);

        IEnumerable<int> IV8FastHostObjectOperations.GetPropertyIndices(IV8FastHostObject instance) => configuration.GetPropertyIndices(instance);

        IV8FastEnumerator IV8FastHostObjectOperations.CreateEnumerator(IV8FastHostObject instance) => configuration.CreateEnumerator(instance);

        IV8FastAsyncEnumerator IV8FastHostObjectOperations.CreateAsyncEnumerator(IV8FastHostObject instance) => configuration.CreateAsyncEnumerator(instance);

        #endregion

        #region Nested type: Configuration

        private sealed class Configuration : V8FastHostObjectConfiguration<TObject>
        {
            private readonly Dictionary<string, (V8FastHostPropertyGetter<TObject> getter, V8FastHostPropertySetter<TObject> setter, V8FastHostPropertyFlags flags)> propertyMap = new();
            private readonly IEnumerable<string> propertyNames;
            private Func<TObject, IV8FastHostDynamicProperties> dynamicPropertiesGetter;
            private Func<TObject, IV8FastEnumerator> enumeratorFactory;
            private Func<TObject, IV8FastAsyncEnumerator> asyncEnumeratorFactory;
            private bool enumerateIndexedProperties;
            private int state;

            public Configuration()
            {
                propertyNames = propertyMap.Keys;
            }

            public void Configure(V8FastHostObjectConfigurator<TObject> configurator)
            {
                MiscHelpers.VerifyNonNullArgument(configurator, nameof(configurator));

                var currentState = Interlocked.CompareExchange(ref state, State.Configuring, State.Unconfigured);
                switch (currentState)
                {
                    case State.Configuring:
                        throw new InvalidOperationException("Configuration is already in progress");

                    case State.Configured:
                        throw new InvalidOperationException("Configuration is already complete");
                }

                try
                {
                    configurator(this);
                }
                catch (Exception)
                {
                    state = State.Unconfigured;
                    throw;
                }

                if ((enumeratorFactory is not null) && (asyncEnumeratorFactory is null))
                {
                    asyncEnumeratorFactory = instance =>
                    {
                        var enumerator = enumeratorFactory(instance);
                        return (enumerator is not null) ? new FastAsyncEnumeratorOnFastEnumerator(enumerator) : null;
                    };
                }

                state = State.Configured;
            }

            public void GetProperty(IV8FastHostObject instance, string name, in V8FastResult value, out bool isCacheable)
            {
                if ((state == State.Configured) && propertyMap.TryGetValue(name, out var entry))
                {
                    entry.getter((TObject)instance, value);
                    isCacheable = (entry.setter is null) && entry.flags.HasAllFlags(V8FastHostPropertyFlags.Cacheable);
                }
                else if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.GetDynamicProperty(name, value);
                    isCacheable = false;
                }
                else
                {
                    isCacheable = false;
                }
            }

            public void SetProperty(IV8FastHostObject instance, string name, in V8FastArg value)
            {
                if ((state == State.Configured) && propertyMap.TryGetValue(name, out var entry))
                {
                    if (entry.setter is {} setter)
                    {
                        setter((TObject)instance, value);
                    }
                    else
                    {
                        throw new NotSupportedException($"The '{name}' property is read-only");
                    }
                }
                else if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.SetDynamicProperty(name, value);
                }
                else
                {
                    throw new NotSupportedException("The object does not support dynamic properties");
                }
            }

            public V8FastHostPropertyFlags QueryProperty(IV8FastHostObject instance, string name)
            {
                if ((state == State.Configured) && propertyMap.TryGetValue(name, out var entry))
                {
                    return entry.flags;
                }

                if ((dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties) && dynamicProperties.HasDynamicProperty(name))
                {
                    return V8FastHostPropertyFlags.Available | V8FastHostPropertyFlags.Enumerable | V8FastHostPropertyFlags.Writable | V8FastHostPropertyFlags.Deletable;
                }

                return V8FastHostPropertyFlags.None;
            }

            public bool DeleteProperty(IV8FastHostObject instance, string name)
            {
                if ((state == State.Configured) && propertyMap.ContainsKey(name))
                {
                    return false;
                }

                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.DeleteDynamicProperty(name);
                }

                return true;
            }

            public IEnumerable<string> GetPropertyNames(IV8FastHostObject instance)
            {
                IEnumerable<string> names = null;

                if (state == State.Configured)
                {
                    names = propertyNames;
                }

                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    var dynamicPropertyNames = dynamicProperties.DynamicPropertyNames;
                    names = (names is null) ? dynamicPropertyNames : names.Concat(dynamicPropertyNames);
                }

                return names ?? Enumerable.Empty<string>();
            }

            public void GetProperty(IV8FastHostObject instance, int index, in V8FastResult value)
            {
                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.GetDynamicProperty(index, value);
                }
            }

            public void SetProperty(IV8FastHostObject instance, int index, in V8FastArg value)
            {
                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.SetDynamicProperty(index, value);
                }
                else
                {
                    throw new NotSupportedException("The object does not support indexed properties");
                }
            }

            public V8FastHostPropertyFlags QueryProperty(IV8FastHostObject instance, int index)
            {
                if ((dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties) && dynamicProperties.HasDynamicProperty(index))
                {
                    var flags = V8FastHostPropertyFlags.Available | V8FastHostPropertyFlags.Writable | V8FastHostPropertyFlags.Deletable;
                    if (EnumerateIndexedProperties)
                    {
                        flags |= V8FastHostPropertyFlags.Enumerable;
                    }

                    return flags;
                }

                return V8FastHostPropertyFlags.None;
            }

            public bool DeleteProperty(IV8FastHostObject instance, int index)
            {
                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    dynamicProperties.DeleteDynamicProperty(index);
                }

                return true;
            }

            public IEnumerable<int> GetPropertyIndices(IV8FastHostObject instance)
            {
                if (dynamicPropertiesGetter?.Invoke((TObject)instance) is {} dynamicProperties)
                {
                    return dynamicProperties.DynamicPropertyIndices;
                }

                return Enumerable.Empty<int>();
            }

            public IV8FastEnumerator CreateEnumerator(IV8FastHostObject instance)
            {
                return enumeratorFactory?.Invoke((TObject)instance);
            }

            public IV8FastAsyncEnumerator CreateAsyncEnumerator(IV8FastHostObject instance)
            {
                return asyncEnumeratorFactory?.Invoke((TObject)instance);
            }

            private void VerifyConfiguring()
            {
                if (state != State.Configuring)
                {
                    throw new InvalidOperationException("Configuration is not in progress");
                }
            }

            #region V8FastHostObjectConfiguration<TObject> overrides

            public override bool EnumerateIndexedProperties
            {
                get => enumerateIndexedProperties;
                set
                {
                    VerifyConfiguring();
                    enumerateIndexedProperties = value;
                }
            }

            public override void AddPropertyAccessors(string name, V8FastHostPropertyGetter<TObject> getter, V8FastHostPropertySetter<TObject> setter, V8FastHostPropertyFlags flags)
            {
                MiscHelpers.VerifyNonNullArgument(name, nameof(name));
                MiscHelpers.VerifyNonNullArgument(getter, nameof(getter));

                VerifyConfiguring();

                var addFlags = V8FastHostPropertyFlags.Available;
                var removeFlags = V8FastHostPropertyFlags.Deletable;

                if (setter is not null)
                {
                    addFlags |= V8FastHostPropertyFlags.Writable;
                }
                else
                {
                    removeFlags |= V8FastHostPropertyFlags.Writable;
                }

                flags |= addFlags;
                flags &= ~removeFlags;

                propertyMap.Add(name, (getter, setter, flags));
            }

            public override void AddMethodGetter(string name, int requiredArgCount, V8FastHostObjectConfigurator<V8FastHostMethod<TObject>> configurator, V8FastHostMethodInvoker<TObject> invoker)
            {
                MiscHelpers.VerifyNonNullArgument(invoker, nameof(invoker));
                AddPropertyGetter(name, (TObject instance, in V8FastResult result) => result.Set(new V8FastHostMethod<TObject>(instance, name, requiredArgCount, configurator, invoker)), V8FastHostPropertyFlags.Cacheable);
            }

            public override void SetDynamicPropertiesGetter(Func<TObject, IV8FastHostDynamicProperties> getter)
            {
                VerifyConfiguring();
                dynamicPropertiesGetter = getter;
            }

            public override void SetEnumeratorFactory(Func<TObject, IV8FastEnumerator> factory)
            {
                VerifyConfiguring();
                enumeratorFactory = factory;
            }

            public override void SetAsyncEnumeratorFactory(Func<TObject, IV8FastAsyncEnumerator> factory)
            {
                VerifyConfiguring();
                asyncEnumeratorFactory = factory;
            }

            #endregion

            #region Nested type: State

            private static class State
            {
                public const int Unconfigured = 0;
                public const int Configuring = 1;
                public const int Configured = 2;
            }

            #endregion

            #region Nested type: FastAsyncEnumeratorOnFastEnumerator

            private sealed class FastAsyncEnumeratorOnFastEnumerator : IV8FastAsyncEnumerator
            {
                private readonly IV8FastEnumerator enumerator;

                public FastAsyncEnumeratorOnFastEnumerator(IV8FastEnumerator enumerator) => this.enumerator = enumerator;

                public void GetCurrent(in V8FastResult item) => enumerator.GetCurrent(item);

                public ValueTask<bool> MoveNextAsync() => new(enumerator.MoveNext());

                public ValueTask DisposeAsync()
                {
                    enumerator.Dispose();
                    return default;
                }
            }

            #endregion
        }

        #endregion
    }
}
