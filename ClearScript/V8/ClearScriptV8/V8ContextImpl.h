// 
// Copyright © Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

#pragma once

//-----------------------------------------------------------------------------
// V8 headers
//-----------------------------------------------------------------------------

#pragma warning(push, 3)

#include "v8.h"
#include "v8-debug.h"
using namespace v8;

#pragma warning(pop)

//-----------------------------------------------------------------------------
// V8ContextImpl
//-----------------------------------------------------------------------------

class V8ContextImpl: public V8Context
{
    PROHIBIT_COPY(V8ContextImpl)

public:

    V8ContextImpl(LPCWSTR pName, bool enableDebugging, bool disableGlobalMembers, DebugMessageDispatcher* pDebugMessageDispatcher, int debugPort);

    V8Value GetRootObject();
    void SetGlobalProperty(LPCWSTR pName, const V8Value& value, bool globalMembers);
    V8Value Execute(LPCWSTR pDocumentName, LPCWSTR pCode, bool discard);
    void CallWithLock(LockCallback* pCallback, LPVOID pvArg);
    void Interrupt();

    int IncrementDebugMessageDispatchCount();
    void ProcessDebugMessages();
    void DisableDebugAgent();

    LPVOID AddRefV8Object(LPVOID pvV8Object);
    void ReleaseV8Object(LPVOID pvV8Object);

    V8Value GetV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName);
    void SetV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName, const V8Value& value);
    bool DeleteV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName);
    void GetV8ObjectPropertyNames(LPVOID pvV8Object, vector<wstring>& names);

    V8Value GetV8ObjectProperty(LPVOID pvV8Object, int index);
    void SetV8ObjectProperty(LPVOID pvV8Object, int index, const V8Value& value);
    bool DeleteV8ObjectProperty(LPVOID pvV8Object, int index);
    void GetV8ObjectPropertyIndices(LPVOID pvV8Object, vector<int>& indices);

    V8Value InvokeV8Object(LPVOID pvV8Object, const vector<V8Value>& args, bool asConstructor);
    V8Value InvokeV8ObjectMethod(LPVOID pvV8Object, LPCWSTR pName, const vector<V8Value>& args);

    ~V8ContextImpl();

private:

    Handle<Value> Wrap();

    static Handle<Value> GetGlobalProperty(Local<String> hName, const AccessorInfo& info);

    static Handle<Value> GetHostObjectProperty(Local<String> hName, const AccessorInfo& info);
    static Handle<Value> SetHostObjectProperty(Local<String> hName, Local<Value> hValue, const AccessorInfo& info);
    static Handle<Integer> QueryHostObjectProperty(Local<String> hName, const AccessorInfo& info);
    static Handle<Boolean> DeleteHostObjectProperty(Local<String> hName, const AccessorInfo& info);
    static Handle<Array> GetHostObjectPropertyNames(const AccessorInfo& info);

    static Handle<Value> GetHostObjectProperty(unsigned __int32 index, const AccessorInfo& info);
    static Handle<Value> SetHostObjectProperty(unsigned __int32 index, Local<Value> hValue, const AccessorInfo& info);
    static Handle<Integer> QueryHostObjectProperty(unsigned __int32 index, const AccessorInfo& info);
    static Handle<Boolean> DeleteHostObjectProperty(unsigned __int32 index, const AccessorInfo& info);
    static Handle<Array> GetHostObjectPropertyIndices(const AccessorInfo& info);

    static Handle<Value> InvokeHostObject(const Arguments& args);
    static void DisposeWeakHandle(Isolate* pIsolate, Persistent<Value> hValue, void* parameter);

    Persistent<Integer> GetIntegerHandle(int value);
    Handle<Value> ImportValue(const V8Value& value);
    V8Value ExportValue(Handle<Value> hValue);
    void ImportValues(const vector<V8Value>& values, vector<Handle<Value>>& importedValues);

    Isolate* m_pIsolate;
    bool m_DebugAgentEnabled;
    long m_DebugMessageDispatchCount;
    Persistent<Context> m_hContext;
    Persistent<Object> m_hGlobal;
    vector<Persistent<Object>> m_GlobalMembersStack;
    Persistent<String> m_hHostObjectCookieName;
    Persistent<FunctionTemplate> m_hHostObjectTemplate;
    hash_map<int, Persistent<Integer>> m_IntegerCache;
};
