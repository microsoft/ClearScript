// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast host object configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <remarks>
    /// This class provides a way to configure the scriptable capabilities of a fast host object or
    /// type. It is not a required part of the interface between V8 and fast host objects, but it
    /// is used extensively by ClearScript's fast host object implementation infrastructure.
    /// </remarks>
    public abstract class V8FastHostObjectConfiguration<TObject> where TObject : IV8FastHostObject
    {
        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostObjectConfiguration{TObject}"/></c> instance.
        /// </summary>
        protected V8FastHostObjectConfiguration()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        /// <summary>
        /// Controls whether indexed properties are enumerable.
        /// </summary>
        public abstract bool EnumerateIndexedProperties { get; set; }

        /// <summary>
        /// Adds a getter for a field-backed read-only property.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="name">The property name.</param>
        /// <param name="accessor">A callback that provides access to the field.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public void AddPropertyGetter<TField>(string name, V8FastHostFieldAccessor<TObject, TField> accessor) => AddPropertyGetter(name, accessor, V8FastHostPropertyFlags.None);

        /// <summary>
        /// Adds a getter for a field-backed read-only property with the specified attributes.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="name">The property name.</param>
        /// <param name="accessor">A callback that provides access to the field.</param>
        /// <param name="flags">The attributes of the property configured by this method.</param>
        /// <remarks>
        /// In the context of this method, the only effective property attributes are
        /// <c><see cref="V8FastHostPropertyFlags.Cacheable"/></c> and
        /// <c><see cref="V8FastHostPropertyFlags.Enumerable"/></c>. Properties configured by this
        /// method cannot be assigned or deleted.
        /// </remarks>
        public void AddPropertyGetter<TField>(string name, V8FastHostFieldAccessor<TObject, TField> accessor, V8FastHostPropertyFlags flags)
        {
            MiscHelpers.VerifyNonNullArgument(accessor, nameof(accessor));
            AddPropertyGetter(name, (TObject instance, in V8FastResult value) => value.Set(accessor(instance)), flags);
        }

        /// <summary>
        /// Adds a getter for a read-only property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="getter">A callback that gets the property value.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public void AddPropertyGetter(string name, V8FastHostPropertyGetter<TObject> getter) => AddPropertyGetter(name, getter, V8FastHostPropertyFlags.None);

        /// <summary>
        /// Adds a getter for a read-only property with the specified attributes.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="getter">A callback that gets the property value.</param>
        /// <param name="flags">The attributes of the property configured by this method.</param>
        /// <remarks>
        /// In the context of this method, the only effective property attributes are
        /// <c><see cref="V8FastHostPropertyFlags.Cacheable"/></c> and
        /// <c><see cref="V8FastHostPropertyFlags.Enumerable"/></c>. Properties configured by this
        /// method cannot be assigned or deleted.
        /// </remarks>
        public void AddPropertyGetter(string name, V8FastHostPropertyGetter<TObject> getter, V8FastHostPropertyFlags flags) => AddPropertyAccessors(name, getter, null, flags);

        /// <summary>
        /// Adds accessors for a field-backed property.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="name">The property name.</param>
        /// <param name="accessor">A callback that provides access to the field.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be deleted.
        /// </remarks>
        public void AddPropertyAccessors<TField>(string name, V8FastHostFieldAccessor<TObject, TField> accessor) => AddPropertyAccessors(name, accessor, V8FastHostPropertyFlags.None);

        /// <summary>
        /// Adds accessors for a field-backed property with the specified attributes.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="name">The property name.</param>
        /// <param name="accessor">A callback that provides access to the field.</param>
        /// <param name="flags">The attributes of the property configured by this method.</param>
        /// <remarks>
        /// In the context of this method, the only effective property attributes are
        /// <c><see cref="V8FastHostPropertyFlags.Cacheable"/></c> and
        /// <c><see cref="V8FastHostPropertyFlags.Enumerable"/></c>. Properties configured by this
        /// method cannot be deleted.
        /// </remarks>
        public void AddPropertyAccessors<TField>(string name, V8FastHostFieldAccessor<TObject, TField> accessor, V8FastHostPropertyFlags flags)
        {
            MiscHelpers.VerifyNonNullArgument(accessor, nameof(accessor));
            AddPropertyAccessors(name, (TObject instance, in V8FastResult value) => value.Set(accessor(instance)), (TObject instance, in V8FastArg value) => accessor(instance) = value.Get<TField>(name), flags);
        }

        /// <summary>
        /// Adds accessors for a property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="getter">A callback that gets the property value.</param>
        /// <param name="setter">A callback that sets the property value.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be deleted.
        /// </remarks>
        public void AddPropertyAccessors(string name, V8FastHostPropertyGetter<TObject> getter, V8FastHostPropertySetter<TObject> setter) => AddPropertyAccessors(name, getter, setter, V8FastHostPropertyFlags.None);

        /// <summary>
        /// Adds accessors for a property with the specified attributes.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="getter">A callback that gets the property value.</param>
        /// <param name="setter">A callback that sets the property value.</param>
        /// <param name="flags">The attributes of the property configured by this method.</param>
        /// <remarks>
        /// In the context of this method, the only effective property attributes are
        /// <c><see cref="V8FastHostPropertyFlags.Cacheable"/></c> and
        /// <c><see cref="V8FastHostPropertyFlags.Enumerable"/></c>. Properties configured by this
        /// method cannot be deleted.
        /// </remarks>
        public abstract void AddPropertyAccessors(string name, V8FastHostPropertyGetter<TObject> getter, V8FastHostPropertySetter<TObject> setter, V8FastHostPropertyFlags flags);

        /// <summary>
        /// Adds a getter for a method.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="invoker">The method invocation callback.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public void AddMethodGetter(string name, V8FastHostMethodInvoker<TObject> invoker) => AddMethodGetter(name, 0, invoker);

        /// <summary>
        /// Adds a getter for a method with the specified required argument count.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="invoker">The method invocation callback.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public void AddMethodGetter(string name, int requiredArgCount, V8FastHostMethodInvoker<TObject> invoker) => AddMethodGetter(name, requiredArgCount, null, invoker);

        /// <summary>
        /// Adds a getter for a method with the specified configuration callback.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="configurator">A callback for preparing the method's configuration.</param>
        /// <param name="invoker">The method invocation callback.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public void AddMethodGetter(string name, V8FastHostObjectConfigurator<V8FastHostMethod<TObject>> configurator, V8FastHostMethodInvoker<TObject> invoker) => AddMethodGetter(name, 0, configurator, invoker);

        /// <summary>
        /// Adds a getter for a method with the specified required argument count and configuration callback.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="configurator">A callback for preparing the method's configuration.</param>
        /// <param name="invoker">The method invocation callback.</param>
        /// <remarks>
        /// Properties configured by this method are not enumerable and cannot be assigned or
        /// deleted.
        /// </remarks>
        public abstract void AddMethodGetter(string name, int requiredArgCount, V8FastHostObjectConfigurator<V8FastHostMethod<TObject>> configurator, V8FastHostMethodInvoker<TObject> invoker);

        /// <summary>
        /// Sets a callback that gets the dynamic properties of a <typeparamref name="TObject"/> instance.
        /// </summary>
        /// <param name="getter">A callback that gets the dynamic properties of a <typeparamref name="TObject"/> instance.</param>
        public abstract void SetDynamicPropertiesGetter(Func<TObject, IV8FastHostDynamicProperties> getter);

        /// <summary>
        /// Sets a callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IEnumerator"/></c> form.
        /// </summary>
        /// <param name="factory">A callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IEnumerator"/></c> form.</param>
        public void SetEnumeratorFactory(Func<TObject, IEnumerator> factory)
        {
            MiscHelpers.VerifyNonNullArgument(factory, nameof(factory));
            SetEnumeratorFactory(instance =>
            {
                var enumerator = factory(instance);
                return (enumerator is not null) ? new FastEnumeratorOnEnumerator(enumerator) : null;
            });
        }

        /// <summary>
        /// Sets a callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IEnumerator{TItem}"/></c> form.
        /// </summary>
        /// <typeparam name="TItem">The enumerator's item type.</typeparam>
        /// <param name="factory">A callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IEnumerator{TItem}"/></c> form.</param>
        public void SetEnumeratorFactory<TItem>(Func<TObject, IEnumerator<TItem>> factory)
        {
            MiscHelpers.VerifyNonNullArgument(factory, nameof(factory));
            SetEnumeratorFactory(instance =>
            {
                var enumerator = factory(instance);
                return (enumerator is not null) ? new FastEnumeratorOnEnumerator<TItem>(enumerator) : null;
            });
        }

        /// <summary>
        /// Sets a callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IV8FastEnumerator"/></c> form.
        /// </summary>
        /// <param name="factory">A callback that creates an enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IV8FastEnumerator"/></c> form.</param>
        public abstract void SetEnumeratorFactory(Func<TObject, IV8FastEnumerator> factory);

        /// <summary>
        /// Sets a callback that creates an asynchronous enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IAsyncEnumerator{TItem}"/></c> form.
        /// </summary>
        /// <typeparam name="TItem">The asynchronous enumerator's item type.</typeparam>
        /// <param name="factory">A callback that creates an asynchronous enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IAsyncEnumerator{TItem}"/></c> form.</param>
        public void SetAsyncEnumeratorFactory<TItem>(Func<TObject, IAsyncEnumerator<TItem>> factory)
        {
            MiscHelpers.VerifyNonNullArgument(factory, nameof(factory));
            SetAsyncEnumeratorFactory(instance =>
            {
                var enumerator = factory(instance);
                return (enumerator is not null) ? new FastAsyncEnumeratorOnAsyncEnumerator<TItem>(enumerator) : null;
            });
        }

        /// <summary>
        /// Sets a callback that creates an asynchronous enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IV8FastAsyncEnumerator"/></c> form.
        /// </summary>
        /// <param name="factory">A callback that creates an asynchronous enumerator for a <typeparamref name="TObject"/> instance in <c><see cref="IV8FastAsyncEnumerator"/></c> form.</param>
        public abstract void SetAsyncEnumeratorFactory(Func<TObject, IV8FastAsyncEnumerator> factory);

        #region Nested type: FastEnumeratorOnEnumerator

        private sealed class FastEnumeratorOnEnumerator : IV8FastEnumerator
        {
            private readonly IEnumerator enumerator;

            public FastEnumeratorOnEnumerator(IEnumerator enumerator) => this.enumerator = enumerator;

            public void GetCurrent(in V8FastResult item) => item.Set(enumerator.Current);

            public bool MoveNext() => enumerator.MoveNext();

            public void Dispose() => (enumerator as IDisposable)?.Dispose();
        }

        #endregion

        #region Nested type: FastEnumeratorOnEnumerator<TItem>

        private sealed class FastEnumeratorOnEnumerator<TItem> : IV8FastEnumerator
        {
            private readonly IEnumerator<TItem> enumerator;

            public FastEnumeratorOnEnumerator(IEnumerator<TItem> enumerator) => this.enumerator = enumerator;

            public void GetCurrent(in V8FastResult item) => item.Set(enumerator.Current);

            public bool MoveNext() => enumerator.MoveNext();

            public void Dispose() => enumerator.Dispose();
        }

        #endregion

        #region Nested type: FastAsyncEnumeratorOnAsyncEnumerator<TItem>

        private sealed class FastAsyncEnumeratorOnAsyncEnumerator<TItem> : IV8FastAsyncEnumerator
        {
            private readonly IAsyncEnumerator<TItem> enumerator;

            public FastAsyncEnumeratorOnAsyncEnumerator(IAsyncEnumerator<TItem> enumerator) => this.enumerator = enumerator;

            public void GetCurrent(in V8FastResult item) => item.Set(enumerator.Current);

            public ValueTask<bool> MoveNextAsync() => enumerator.MoveNextAsync();

            public ValueTask DisposeAsync() => enumerator.DisposeAsync();
        }

        #endregion
    }
}
