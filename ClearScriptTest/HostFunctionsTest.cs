// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class HostFunctionsTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private HostFunctions host;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", host = new HostFunctions());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_asType()
        {
            var value = Enumerable.Range(1, 5).ToArray();
            VerifyAsType<IList>(value);
            VerifyAsType<Array>(value);
            VerifyAsType<Object>(value);
            Assert.IsNull(host.asType<IDictionary>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_asType_Scalar()
        {
            const int value = 123;
            VerifyAsType<IComparable>(value);
            VerifyAsType<ValueType>(value);
            VerifyAsType<Object>(value);
            Assert.IsNull(host.asType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_asType_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            VerifyAsType<IComparable>(value);
            VerifyAsType<Enum>(value);
            VerifyAsType<ValueType>(value);
            VerifyAsType<Object>(value);
            Assert.IsNull(host.asType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_asType_Struct()
        {
            var value = new DateTime(2007, 5, 22, 6, 15, 43);
            VerifyAsType<IComparable>(value);
            VerifyAsType<ValueType>(value);
            VerifyAsType<Object>(value);
            Assert.IsNull(host.asType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_cast()
        {
            var value = Enumerable.Range(1, 5).ToArray();
            CastVerifier<IList>.VerifySymmetric(host, value);
            CastVerifier<Array>.VerifySymmetric(host, value);
            CastVerifier<Object>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void HostFunctions_cast_Fail()
        {
            var value = Enumerable.Range(1, 5).ToArray();
            CastVerifier<IDictionary>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_cast_Scalar()
        {
            const double value = 123.75;
            CastVerifier<IComparable>.VerifySymmetric(host, value);
            CastVerifier<ValueType>.VerifySymmetric(host, value);
            CastVerifier<Object>.VerifySymmetric(host, value);
            CastVerifier<int>.VerifyAsymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void HostFunctions_cast_Scalar_Fail()
        {
            const double value = 123.75;
            CastVerifier<IList>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_cast_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            CastVerifier<IComparable>.VerifySymmetric(host, value);
            CastVerifier<Enum>.VerifySymmetric(host, value);
            CastVerifier<ValueType>.VerifySymmetric(host, value);
            CastVerifier<Object>.VerifySymmetric(host, value);
            CastVerifier<int>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void HostFunctions_cast_Enum_Fail()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            CastVerifier<IList>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_cast_Struct()
        {
            var value = new DateTime(2007, 5, 22, 6, 15, 43);
            CastVerifier<IComparable>.VerifySymmetric(host, value);
            CastVerifier<ValueType>.VerifySymmetric(host, value);
            CastVerifier<Object>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void HostFunctions_cast_Struct_Fail()
        {
            var value = new DateTime(2007, 5, 22, 6, 15, 43);
            CastVerifier<IList>.VerifySymmetric(host, value);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_cast_UserDefined()
        {
            Assert.AreEqual(intVal, CastVerifier<int>.VerifyCast(host, this));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Action()
        {
            var fooVal = Enumerable.Range(1, 5).ToArray();
            const double barVal = Math.PI;
            const DayOfWeek bazVal = DayOfWeek.Wednesday;
            var quxVal = new DateTime(2007, 5, 22, 6, 15, 43);

            int[] foo = null;
            var bar = 0d;
            var baz = DayOfWeek.Sunday;
            var qux = DateTime.UtcNow;

            Action<int[], double, DayOfWeek, DateTime> method = (fooArg, barArg, bazArg, quxArg) =>
            {
                foo = fooArg;
                bar = barArg;
                baz = bazArg;
                qux = quxArg;
            };

            host.del<Action<int[], double, DayOfWeek, DateTime>>(method)(fooVal, barVal, bazVal, quxVal);
            Assert.AreEqual(fooVal, foo);
            Assert.AreEqual(barVal, bar);
            Assert.AreEqual(bazVal, baz);
            Assert.AreEqual(quxVal, qux);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Action_0()
        {
            var methodInvoked = false;
            Action method = () => { methodInvoked = true; };
            host.del<Action>(method)();
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Action_1()
        {
            var methodInvoked = false;
            Action<bool> method = value => { methodInvoked = value; };
            host.del<Action<bool>>(method)(true);
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Func()
        {
            var fooVal = Enumerable.Range(1, 5).ToArray();
            const double barVal = Math.PI;
            const DayOfWeek bazVal = DayOfWeek.Wednesday;
            var quxVal = new DateTime(2007, 5, 22, 6, 15, 43);
            const int retVal = 42;

            int[] foo = null;
            var bar = 0d;
            var baz = DayOfWeek.Sunday;
            var qux = DateTime.UtcNow;

            Func<int[], double, DayOfWeek, DateTime, int> method = (fooArg, barArg, bazArg, quxArg) =>
            {
                foo = fooArg;
                bar = barArg;
                baz = bazArg;
                qux = quxArg;
                return retVal;
            };

            Assert.AreEqual(retVal, host.del<Func<int[], double, DayOfWeek, DateTime, int>>(method)(fooVal, barVal, bazVal, quxVal));
            Assert.AreEqual(fooVal, foo);
            Assert.AreEqual(barVal, bar);
            Assert.AreEqual(bazVal, baz);
            Assert.AreEqual(quxVal, qux);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Func_0()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<int> method = () => { methodInvoked = true; return retVal; };
            Assert.AreEqual(retVal, host.del<Func<int>>(method)());
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_Func_1()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<bool, int> method = value => { methodInvoked = value; return retVal; };
            Assert.AreEqual(retVal, host.del<Func<bool, int>>(method)(true));
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_del_CustomDelegate()
        {
            engine.EnableAutoHostVariables = true;

            var fooVal = Enumerable.Range(1, 5).ToArray();
            const double barVal = Math.PI;
            const DayOfWeek bazVal = DayOfWeek.Wednesday;
            var quxVal = new DateTime(2007, 5, 22, 6, 15, 43);
            const int retVal = 42;

            CustomDelegateHandler method = (fooVar, fooArg, barVar, barArg, bazVar, bazArg, quxVar, quxArg, retArg) =>
            {
                fooVar.Value = fooArg;
                barVar.Value = barArg;
                bazVar.Value = bazArg;
                quxVar.Value = quxArg;
                return retArg;
            };

            int[] foo;
            var bar = 0d;
            DayOfWeek baz;
            var qux = DateTime.UtcNow;

            Assert.AreEqual(retVal, host.del<CustomDelegate>(method)(out foo, fooVal, ref bar, barVal, out baz, bazVal, ref qux, quxVal, retVal));
            Assert.AreEqual(fooVal, foo);
            Assert.AreEqual(barVal, bar);
            Assert.AreEqual(bazVal, baz);
            Assert.AreEqual(quxVal, qux);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_0()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<int> method = () => { methodInvoked = true; return retVal; };
            Assert.AreEqual(retVal, ((Func<int>)host.func<int>(0, method))());
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_1()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<object, int> method = value => { methodInvoked = (bool)value; return retVal; };
            Assert.AreEqual(retVal, ((Func<object, int>)host.func<int>(1, method))(true));
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_2()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<object, object, int> method = (value1, value2) => { methodInvoked = (bool)value1 && (bool)value2; return retVal; };
            Assert.AreEqual(retVal, ((Func<object, object, int>)host.func<int>(2, method))(true, true));
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_0_NonGeneric()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<object> method = () => { methodInvoked = true; return retVal; };
            Assert.AreEqual(retVal, ((Func<object>)host.func(0, method))());
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_1_NonGeneric()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<object, object> method = value => { methodInvoked = (bool)value; return retVal; };
            Assert.AreEqual(retVal, ((Func<object, object>)host.func(1, method))(true));
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_func_2_NonGeneric()
        {
            var methodInvoked = false;
            const int retVal = 42;
            Func<object, object, object> method = (value1, value2) => { methodInvoked = (bool)value1 && (bool)value2; return retVal; };
            Assert.AreEqual(retVal, ((Func<object, object, object>)host.func(2, method))(true, true));
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isType()
        {
            var value = Enumerable.Range(1, 5).ToArray();
            Assert.IsTrue(host.isType<IList>(value));
            Assert.IsTrue(host.isType<Array>(value));
            Assert.IsTrue(host.isType<Object>(value));
            Assert.IsFalse(host.isType<IDictionary>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isType_Scalar()
        {
            const int value = 123;
            Assert.IsTrue(host.isType<IComparable>(value));
            Assert.IsTrue(host.isType<ValueType>(value));
            Assert.IsTrue(host.isType<Object>(value));
            Assert.IsFalse(host.isType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isType_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            Assert.IsTrue(host.isType<IComparable>(value));
            Assert.IsTrue(host.isType<Enum>(value));
            Assert.IsTrue(host.isType<ValueType>(value));
            Assert.IsTrue(host.isType<Object>(value));
            Assert.IsFalse(host.isType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isType_Struct()
        {
            var value = new DateTime(2007, 5, 22, 6, 15, 43);
            Assert.IsTrue(host.isType<IComparable>(value));
            Assert.IsTrue(host.isType<ValueType>(value));
            Assert.IsTrue(host.isType<Object>(value));
            Assert.IsFalse(host.isType<IList>(value));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isTypeObj_True_TypeArg()
        {
            engine.AddHostType("Int32", typeof(int));
            Assert.IsTrue((bool)engine.Evaluate("host.isTypeObj(Int32)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isTypeObj_True_NonTypeArg()
        {
            engine.AddHostType("List", typeof(List<>));
            engine.AddHostType("Console", typeof(Console));
            Assert.IsTrue((bool)engine.Evaluate("host.isTypeObj(List)"));
            Assert.IsTrue((bool)engine.Evaluate("host.isTypeObj(Console)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_isTypeObj_False()
        {
            Assert.IsFalse((bool)engine.Evaluate("host.isTypeObj(5)"));
            Assert.IsFalse((bool)engine.Evaluate("host.isTypeObj({})"));
            Assert.IsFalse((bool)engine.Evaluate("host.isTypeObj(host)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newArr()
        {
            VerifyNewArr<Random>(5);
            VerifyNewArr<Random>(5, 3);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newArr_Enum()
        {
            VerifyNewArr<DayOfWeek>(5);
            VerifyNewArr<DayOfWeek>(5, 3);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newArr_Scalar()
        {
            VerifyNewArr<double>(5);
            VerifyNewArr<double>(5, 3);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newArr_Struct()
        {
            VerifyNewArr<DateTime>(5);
            VerifyNewArr<DateTime>(5, 3);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newArr_NonGeneric()
        {
            VerifyNewArr(5);
            VerifyNewArr(5, 3);
        }

        [TestMethod, TestCategory("HostFunctions")]
        [ExpectedException(typeof(ArgumentException))]
        public void HostFunctions_newArr_Fail()
        {
            VerifyNewArr<Random>();
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newObj()
        {
            Assert.IsInstanceOfType(host.newObj<Random>(), typeof(Random));
            Assert.IsInstanceOfType(host.newObj<Random>(100), typeof(Random));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newObj_Struct()
        {
            Assert.AreEqual(new Point(), host.newObj<Point>());
            Assert.AreEqual(new Point(100, 200), host.newObj<Point>(100, 200));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newObj_PropertyBag()
        {
            Assert.IsInstanceOfType(host.newObj(), typeof(PropertyBag));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newVar()
        {
            VerifyNewVar<Random>();
            VerifyNewVar(new Random(100));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newVar_Enum()
        {
            VerifyNewVar<DayOfWeek>();
            VerifyNewVar(DayOfWeek.Wednesday);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newVar_Scalar()
        {
            VerifyNewVar<double>();
            VerifyNewVar(0.125);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_newVar_Struct()
        {
            VerifyNewVar<DateTime>();
            VerifyNewVar(DateTime.Now);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_proc_0()
        {
            var methodInvoked = false;
            Action method = () => methodInvoked = true;
            ((Action)host.proc(0, method))();
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_proc_1()
        {
            var methodInvoked = false;
            Action<object> method = value => methodInvoked = (bool)value;
            ((Action<object>)host.proc(1, method))(true);
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_proc_2()
        {
            var methodInvoked = false;
            Action<object, object> method = (value1, value2) => methodInvoked = (bool)value1 && (bool)value2;
            ((Action<object, object>)host.proc(2, method))(true, true);
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_typeOf_TypeArg()
        {
            engine.AllowReflection = true;
            engine.AddHostType("Int32", typeof(int));
            Assert.AreEqual(typeof(int), engine.Evaluate("host.typeOf(Int32)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_typeOf_NonTypeArg()
        {
            engine.AllowReflection = true;
            engine.AddHostType("List", typeof(List<>));
            engine.AddHostType("Console", typeof(Console));
            Assert.AreEqual(typeof(List<>), engine.Evaluate("host.typeOf(List)"));
            Assert.AreEqual(typeof(Console), engine.Evaluate("host.typeOf(Console)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_typeOf_TypeArg_Blocked()
        {
            engine.AddHostType("Int32", typeof(int));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("host.typeOf(Int32)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_typeOf_NonTypeArg_Blocked()
        {
            engine.AddHostType("Console", typeof(Console));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("host.typeOf(Console)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_typeOf_NonType()
        {
            engine.AllowReflection = true;
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("host.typeOf(5)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_flags_Unsigned()
        {
            engine.AddHostType("UnsignedFlags", HostItemFlags.PrivateAccess, typeof(UnsignedFlags));
            var result = engine.Evaluate("host.flags(UnsignedFlags.Second, UnsignedFlags.Fourth, UnsignedFlags.Sixth, UnsignedFlags.Eighth)");
            Assert.AreEqual(UnsignedFlags.Second | UnsignedFlags.Fourth | UnsignedFlags.Sixth | UnsignedFlags.Eighth, result);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_flags_Signed()
        {
            engine.AddHostType("SignedFlags", HostItemFlags.PrivateAccess, typeof(SignedFlags));
            // include Eighth member to force overflow path in host.flags()
            var result = engine.Evaluate("host.flags(SignedFlags.Second, SignedFlags.Fourth, SignedFlags.Sixth, SignedFlags.Eighth)");
            Assert.AreEqual(SignedFlags.Second | SignedFlags.Fourth | SignedFlags.Sixth | SignedFlags.Eighth, result);
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_flags_Mismatch()
        {
            engine.AddHostType("UnsignedFlags", HostItemFlags.PrivateAccess, typeof(UnsignedFlags));
            engine.AddHostType("SignedFlags", HostItemFlags.PrivateAccess, typeof(SignedFlags));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("host.flags(SignedFlags.Second, SignedFlags.Fourth, UnsignedFlags.Sixth, UnsignedFlags.Eighth)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_flags_NonFlags()
        {
            engine.AddHostType("NonFlags", HostItemFlags.PrivateAccess, typeof(NonFlags));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Execute("host.flags(NonFlags.Second, NonFlags.Fourth, NonFlags.Sixth, NonFlags.Eighth)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toSByte()
        {
            Assert.AreEqual(Convert.ToSByte(127), engine.Evaluate("host.toSByte(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toByte()
        {
            Assert.AreEqual(Convert.ToByte(127), engine.Evaluate("host.toByte(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt16()
        {
            Assert.AreEqual(Convert.ToInt16(127), engine.Evaluate("host.toInt16(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt16()
        {
            Assert.AreEqual(Convert.ToUInt16(127), engine.Evaluate("host.toUInt16(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toChar()
        {
            Assert.AreEqual(Convert.ToChar(127), engine.Evaluate("host.toChar(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt32()
        {
            Assert.AreEqual(Convert.ToInt32(127), engine.Evaluate("host.toInt32(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt32()
        {
            Assert.AreEqual(Convert.ToUInt32(127), engine.Evaluate("host.toUInt32(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt64()
        {
            Assert.AreEqual(Convert.ToInt64(127), engine.Evaluate("host.toInt64(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt64()
        {
            Assert.AreEqual(Convert.ToUInt64(127), engine.Evaluate("host.toUInt64(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toSingle()
        {
            Assert.AreEqual(Convert.ToSingle(127), engine.Evaluate("host.toSingle(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toDouble()
        {
            Assert.AreEqual(Convert.ToDouble(127), engine.Evaluate("host.toDouble(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toDecimal()
        {
            Assert.AreEqual(Convert.ToDecimal(127), engine.Evaluate("host.toDecimal(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toSByte_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions()); 
            Assert.AreEqual(Convert.ToSByte(127), engine.Evaluate("host.toSByte(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toByte_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToByte(127), engine.Evaluate("host.toByte(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt16_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToInt16(127), engine.Evaluate("host.toInt16(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt16_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToUInt16(127), engine.Evaluate("host.toUInt16(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toChar_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToChar(127), engine.Evaluate("host.toChar(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt32_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToInt32(127), engine.Evaluate("host.toInt32(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt32_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToUInt32(127), engine.Evaluate("host.toUInt32(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toInt64_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToInt64(127), engine.Evaluate("host.toInt64(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toUInt64_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToUInt64(127), engine.Evaluate("host.toUInt64(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toSingle_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToSingle(127), engine.Evaluate("host.toSingle(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toDouble_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToDouble(127), engine.Evaluate("host.toDouble(127)"));
        }

        [TestMethod, TestCategory("HostFunctions")]
        public void HostFunctions_toDecimal_V8()
        {
            engine.Dispose();
            engine = new V8ScriptEngine();
            engine.AddHostObject("host", new HostFunctions());
            Assert.AreEqual(Convert.ToDecimal(127), engine.Evaluate("host.toDecimal(127)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private void VerifyAsType<T>(object value) where T : class
        {
            var hostItem = host.asType<T>(value) as HostItem;
            Assert.IsNotNull(hostItem);
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(typeof(T), hostItem.Target.Type);
            Assert.AreEqual(value, hostItem.Unwrap());
        }

        private void VerifyNewArr<T>(params int[] lengths)
        {
            var value = (Array)host.newArr<T>(lengths);
            Assert.IsInstanceOfType(value, typeof(T).MakeArrayType(lengths.Length));
            lengths.ForEach((length, index) => Assert.AreEqual(lengths[index], value.GetLength(index)));
        }

        private void VerifyNewArr(params int[] lengths)
        {
            VerifyNewArr<object>(lengths);
        }

        private void VerifyNewVar<T>()
        {
            var variable = host.newVar<T>();
            Assert.IsInstanceOfType(variable, typeof(HostVariable<T>));
            if (typeof(T).IsValueType)
            {
                Assert.AreEqual(default(T), ((HostVariable<T>)variable).Value);
            }
            else
            {
                Assert.AreSame(default(T), ((HostVariable<T>)variable).Value);
            }
        }

        private void VerifyNewVar<T>(T initValue)
        {
            var variable = host.newVar(initValue);
            Assert.IsInstanceOfType(variable, typeof(HostVariable<T>));
            if (typeof(T).IsValueType)
            {
                Assert.AreEqual(initValue, ((HostVariable<T>)variable).Value);
            }
            else
            {
                Assert.AreSame(initValue, ((HostVariable<T>)variable).Value);
            }
        }

        private const int intVal = 12345;

        public static explicit operator int(HostFunctionsTest value)
        {
            return intVal;
        }

        private static class CastVerifier<TTarget>
        {
            public static void VerifySymmetric<TValue>(HostFunctions host, TValue value)
            {
                var result1 = VerifyCast<TTarget>(host, value);
                var result2 = VerifyCast<TValue>(host, result1);
                Assert.AreEqual(value, result2);
            }

            public static void VerifyAsymmetric<TValue>(HostFunctions host, TValue value)
            {
                var result1 = VerifyCast<TTarget>(host, value);
                var result2 = VerifyCast<TValue>(host, result1);
                Assert.AreEqual(value.DynamicCast<TTarget>().DynamicCast<TValue>(), result2);
            }

            public static object VerifyCast(HostFunctions host, object value)
            {
                return VerifyCast<TTarget>(host, value);
            }

            private static object VerifyCast<T>(HostFunctions host, object value)
            {
                var expectedResult = value.DynamicCast<T>();

                var result = host.cast<T>(value);
                var hostItem = result as HostItem;
                if (hostItem != null)
                {
                    Assert.AreEqual(typeof(T), hostItem.Target.Type);
                    result = hostItem.Unwrap();
                }

                Assert.AreEqual(expectedResult, result);
                return result;
            }
        }

        private delegate int CustomDelegate(
            out int[] foo, int[] fooVal,
            ref double bar, double barVal,
            out DayOfWeek baz, DayOfWeek bazVal,
            ref DateTime qux, DateTime quxVal,
            int retVal
        );

        private delegate int CustomDelegateHandler(
            OutArg<int[]> fooVar, int[] fooVal,
            RefArg<double> barVar, double barVal,
            OutArg<DayOfWeek> bazVar, DayOfWeek bazVal,
            RefArg<DateTime> quxVar, DateTime quxVal,
            int retVal
        );

        [Flags]
        public enum UnsignedFlags : byte
        {
            None = 0,
            First = 1 << 0,
            Second = 1 << 1,
            Third = 1 << 2,
            Fourth = 1 << 3,
            Fifth = 1 << 4,
            Sixth = 1 << 5,
            Seventh = 1 << 6,
            Eighth = 1 << 7,
        }

        [Flags]
        public enum SignedFlags : sbyte
        {
            None = 0,
            First = 1 << 0,
            Second = 1 << 1,
            Third = 1 << 2,
            Fourth = 1 << 3,
            Fifth = 1 << 4,
            Sixth = 1 << 5,
            Seventh = 1 << 6,
            Eighth = -128,
                // negative member forces overflow path in host.flags()
        }

        public enum NonFlags
        {
            None = 0,
            First = 1 << 0,
            Second = 1 << 1,
            Third = 1 << 2,
            Fourth = 1 << 3,
            Fifth = 1 << 4,
            Sixth = 1 << 5,
            Seventh = 1 << 6,
            Eighth = 1 << 7,
        }

        #endregion
    }
}
