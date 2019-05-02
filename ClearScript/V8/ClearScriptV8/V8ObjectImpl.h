// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8ObjectImpl
    //-------------------------------------------------------------------------

    private ref class V8ObjectImpl sealed : IV8Object
    {
    public:

        V8ObjectImpl(V8ObjectHolder* pHolder, V8Value::Subtype subtype);

        virtual Object^ GetProperty(String^ gcName);
        virtual void SetProperty(String^ gcName, Object^ gcValue);
        virtual bool DeleteProperty(String^ gcName);
        virtual array<String^>^ GetPropertyNames();

        virtual Object^ GetProperty(int index);
        virtual void SetProperty(int index, Object^ gcValue);
        virtual bool DeleteProperty(int index);
        virtual array<int>^ GetPropertyIndices();

        virtual Object^ Invoke(bool asConstructor, array<Object^>^ gcArgs);
        virtual Object^ InvokeMethod(String^ gcName, array<Object^>^ gcArgs);

        virtual bool IsArrayBufferOrView();
        virtual V8ArrayBufferOrViewKind GetArrayBufferOrViewKind();
        virtual V8ArrayBufferOrViewInfo^ GetArrayBufferOrViewInfo();
        virtual void InvokeWithArrayBufferOrViewData(Action<IntPtr>^ gcAction);

        SharedPtr<V8ObjectHolder> GetHolder();
        V8Value::Subtype GetSubtype();

        ~V8ObjectImpl();
        !V8ObjectImpl();

    private:

        static void ImportValues(array<Object^>^ gcValues, std::vector<V8Value>& importedValues);

        Object^ m_gcLock;
        SharedPtr<V8ObjectHolder>* m_pspHolder;
        V8Value::Subtype m_Subtype;
    };

}}}
