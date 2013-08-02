// 
// Copyright (c) Microsoft Corporation. All rights reserved.
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




using System;
using System.Reflection;

namespace Microsoft.ClearScript
{
    internal static partial class DelegateFactory
    {
        private const int maxArgCount = 16;

        
        private class ProcShim<TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget()
            {
                Invoke(() => target());
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1)
            {
                Invoke(() => target(a1));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2)
            {
                Invoke(() => target(a1, a2));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3)
            {
                Invoke(() => target(a1, a2, a3));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                Invoke(() => target(a1, a2, a3, a4));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                Invoke(() => target(a1, a2, a3, a4, a5));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDelegate> : ProcShim
        {
            private static readonly MethodInfo method = typeof(ProcShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public ProcShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public void InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                Invoke(() => target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        
        private class FuncShim<TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget()
            {
                return Invoke(() => (TResult)target());
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1)
            {
                return Invoke(() => (TResult)target(a1));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2)
            {
                return Invoke(() => (TResult)target(a1, a2));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3)
            {
                return Invoke(() => (TResult)target(a1, a2, a3));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private class FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult, TDelegate> : FuncShim<TResult>
        {
            private static readonly MethodInfo method = typeof(FuncShim<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult, TDelegate>).GetMethod("InvokeTarget");
            private readonly dynamic target;
            private readonly Delegate del;

            public FuncShim(ScriptEngine engine, object target)
                : base(engine)
            {
                this.target = GetCompatibleTarget(typeof(TDelegate), target);
                del = Delegate.CreateDelegate(typeof(TDelegate), this, method);
            }

            // ReSharper disable UnusedMember.Local

            public TResult InvokeTarget(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                return Invoke(() => (TResult)target(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
            }

            // ReSharper restore UnusedMember.Local

            #region DelegateShim overrides

            public override Delegate Delegate
            {
                get { return del; }
            }

            #endregion
        }
        
        private static readonly Type[] procTemplates =
        {
            
            typeof(Action),
            
            typeof(Action</*T1*/>),
            
            typeof(Action</*T1*/, /*T2*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/>),
            
            typeof(Action</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*T16*/>),
            
        };

        private static readonly Type[] funcTemplates =
        {
            
            typeof(Func</*TResult*/>),
            
            typeof(Func</*T1*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*TResult*/>),
            
            typeof(Func</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*T16*/, /*TResult*/>),
            
        };

        private static readonly Type[] procShimTemplates =
        {
            
            typeof(ProcShim</*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*TDelegate*/>),
            
            typeof(ProcShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*T16*/, /*TDelegate*/>),
            
        };

        private static readonly Type[] funcShimTemplates =
        {
            
            typeof(FuncShim</*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*TResult*/, /*TDelegate*/>),
            
            typeof(FuncShim</*T1*/, /*T2*/, /*T3*/, /*T4*/, /*T5*/, /*T6*/, /*T7*/, /*T8*/, /*T9*/, /*T10*/, /*T11*/, /*T12*/, /*T13*/, /*T14*/, /*T15*/, /*T16*/, /*TResult*/, /*TDelegate*/>),
            
        };
    }
}
