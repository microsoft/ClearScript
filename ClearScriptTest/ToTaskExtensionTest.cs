using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    [DeploymentItem("v8-base-x64.dll")]
    [DeploymentItem("v8-base-ia32.dll")]
    [DeploymentItem("v8-zlib-x64.dll")]
    [DeploymentItem("v8-zlib-ia32.dll")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class ToTaskExtensionTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine _engine;
        private TestTasks _testTasks;

        [TestInitialize]
        public void TestInitialize()
        {
            _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            _engine.AddHostType(typeof(Extensions));
            _engine.AddHostType(typeof(JavaScriptExtensions));
            _engine.AddHostObject("Tracer", new Tracer());
            _engine.AddHostObject("GetInnerMostMessage", new Func<Exception, string>(GetInnerMostMessage));
            _testTasks = new TestTasks(_engine.Script.Tracer);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesException_ThrowBeforeReturningPromise_WithJavaScriptExceptionHandler()
        {
            var setupScript = @"
				var Test = {
					throwBeforePromise: async function() {
						try {
							throw 'Throw before promise error';
						} catch (error) {
							Tracer.Log('JS Exception: ' + error);
							throw error;
						}
					}
				}
			";
            var evaluateFunction = "Test.throwBeforePromise();";
            var expectedTrace = new[] {
                "JS Exception: Throw before promise error",
                "CLR Exception: Throw before promise error"
            };

            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesException_ThrowInNonAsyncFunction()
        {
            var setupScript = @"
				var Test = {
					throwBeforePromise: function() {
						throw 'Throw in non async function';
					}
				}
			";
            var evaluateFunction = "Test.throwBeforePromise();";
            var expectedTrace = new[] {
                "CLR Exception: Throw in non async function"
            };

            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesException_ThrowInNonAsyncFunction_WithJavaScriptExceptionHandler()
        {
            var setupScript = @"
				var Test = {
					throwBeforePromise: function() {
						try {
							throw 'Throw before promise error';
						} catch (error) {
							Tracer.Log('JS Exception: ' + error);
							throw error;
						}
					}
				}
			";
            var evaluateFunction = "Test.throwBeforePromise();";
            var expectedTrace = new[] {
                "JS Exception: Throw before promise error",
                "CLR Exception: Throw before promise error"
            };

            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesExceptions_IntTask()
        {
            await RunTestScript(_engine,
                _testTasks.GetIntTask(PointOfFailure.Canceled),
                new[] { "A", "CLR Exception: A task was canceled." });
            await RunTestScript(_engine,
                _testTasks.GetIntTask(PointOfFailure.A),
                new[] { "A", "CLR Exception: GetInt Fail A" });
            await RunTestScript(_engine,
                _testTasks.GetIntTask(PointOfFailure.B),
                new[] { "A", "B", "CLR Exception: GetInt Fail B" });
            await RunTestScript(_engine,
                _testTasks.GetIntTask(PointOfFailure.C),
                new[] { "A", "B", "C", "CLR Exception: GetInt Fail C" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesExceptions_IntTask_WithJavaScriptTryCatch()
        {
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetIntTask(PointOfFailure.Canceled),
                new[] { "A", "JS Exception: A task was canceled.", "CLR Exception: A task was canceled." });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetIntTask(PointOfFailure.A),
                new[] { "A", "JS Exception: GetInt Fail A", "CLR Exception: GetInt Fail A" });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetIntTask(PointOfFailure.B),
                new[] { "A", "B", "JS Exception: GetInt Fail B", "CLR Exception: GetInt Fail B" });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetIntTask(PointOfFailure.C),
                new[] { "A", "B", "C", "JS Exception: GetInt Fail C", "CLR Exception: GetInt Fail C" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesExceptions_VoidTask()
        {
            await RunTestScript(_engine,
                _testTasks.GetVoidTask(PointOfFailure.Canceled),
                new[] { "A", "CLR Exception: A task was canceled." });
            await RunTestScript(_engine,
                _testTasks.GetVoidTask(PointOfFailure.A),
                new[] { "A", "CLR Exception: GetVoid Fail A" });
            await RunTestScript(_engine,
                _testTasks.GetVoidTask(PointOfFailure.B),
                new[] { "A", "B", "CLR Exception: GetVoid Fail B" });
            await RunTestScript(_engine,
                _testTasks.GetVoidTask(PointOfFailure.C),
                new[] { "A", "B", "C", "CLR Exception: GetVoid Fail C" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_HandlesExceptions_VoidTask_WithJavaScriptTryCatch()
        {
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.Canceled),
                new[] { "A", "JS Exception: A task was canceled.", "CLR Exception: A task was canceled." });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.A),
                new[] { "A", "JS Exception: GetVoid Fail A", "CLR Exception: GetVoid Fail A" });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.B),
                new[] { "A", "B", "JS Exception: GetVoid Fail B", "CLR Exception: GetVoid Fail B" });
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.C),
                new[] { "A", "B", "C", "JS Exception: GetVoid Fail C", "CLR Exception: GetVoid Fail C" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue()
        {
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.None),
                new[] { "A", "B", "C", "CLR Result: [undefined]" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue_IntTask()
        {
            await RunTestScript(_engine,
                _testTasks.GetIntTask(PointOfFailure.None),
                new[] { "A", "B", "C", "CLR Result: 10" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue_IntTask_WithJavaScriptTryCatch()
        {
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetIntTask(PointOfFailure.None),
                new[] { "A", "B", "C", "CLR Result: 10" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue_InvokingNonAsyncFunction()
        {
            _engine.Script.testDelegate = _testTasks.GetIntTask(PointOfFailure.None);
            var setupScript = @"
				var Test = {
					notAsync: function() {
						return testDelegate().ToPromise();
					}
				}
			";
            var evaluateFunction = "Test.notAsync();";
            var expectedTrace = new[] { "A", "B", "C", "CLR Result: 10" };
            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue_VoidTask()
        {
            await RunTestScript(_engine,
                _testTasks.GetVoidTask(PointOfFailure.None),
                new[] { "A", "B", "C", "CLR Result: [undefined]" });
        }

        [TestMethod, TestCategory("ToTask")]
        public async Task ToTask_ReturnsValue_VoidTask_WithJavaScriptTryCatch()
        {
            await RunTestScriptWithTryCatch(_engine,
                _testTasks.GetVoidTask(PointOfFailure.None),
                new[] { "A", "B", "C", "CLR Result: [undefined]" });
        }

        private async Task RunTestScript(string setupScript, string evaluateFunction, string[] expectedTrace)
        {
            var expectedTraceString = string.Join(Environment.NewLine, expectedTrace);
            await RunTestScript(setupScript, evaluateFunction, expectedTraceString);
        }

        private async Task RunTestScript(string setupScript, string evaluateFunction, string expectedTraceString)
        {
            _engine.Script.Tracer.Clear();
            try
            {
                _engine.Execute(setupScript);
                var result = await _engine.Evaluate(evaluateFunction).ToTask();
                _engine.Script.Tracer.Log("CLR Result: " + result);
            }
            catch (Exception ex)
            {
                _engine.Script.Tracer.Log("CLR Exception: " + GetInnerMostMessage(ex));
            }

            Assert.AreEqual(expectedTraceString, _engine.Script.Tracer.GetTrace());
        }

        private async Task RunTestScript(ScriptEngine engine, Delegate testDelegate, string[] expectedTrace)
        {
            engine.Script.testDelegate = testDelegate;
            var setupScript = @"
				var Test = {
					getDelegate: async function() {
						return await testDelegate().ToPromise();
					},
				}
			";
            var evaluateFunction = "Test.getDelegate();";

            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        private async Task RunTestScriptWithTryCatch(ScriptEngine engine, Delegate testDelegate, string[] expectedTrace)
        {
            engine.Script.testDelegate = testDelegate;
            var setupScript = @"
				var Test = {
					getDelegate: async function() {
						try {
							return await testDelegate().ToPromise();
						} catch (error) {
							Tracer.Log('JS Exception: ' + GetInnerMostMessage(error));
							throw error;
						}
					},
				}
			";
            var evaluateFunction = "Test.getDelegate();";

            await RunTestScript(setupScript, evaluateFunction, expectedTrace);
        }

        #endregion

        #region miscelaneous

        public enum PointOfFailure { None, A, B, C, Canceled };

        public class Tracer
        {
            private List<string> _trace = new List<string>();

            public void Clear()
            {
                _trace.Clear();
            }

            public string GetTrace()
            {
                return string.Join(Environment.NewLine, _trace);
            }

            public void Log(object traceMessage)
            {
                _trace.Add(traceMessage.ToString());
            }
        }

        // The exception handling tests below are using these test tasks with three failure points named A,B,C
        //  The point of failures are as follows:
        //	A - As soon as the task is started
        //  ... simulated work ...
        //	B - In the middle of continuations
        //  ... simulated work ...
        //	C - At the end of the continuations
        //	+ an extra simulating When it's canceled
        // 	I did not split these into individual tests, as code changes will likely affect all at once. (or maybe I'm just too lazy)
        private class TestTasks
        {
            private Tracer _tracer;

            public TestTasks(Tracer tracer)
            {
                _tracer = tracer;
            }

            public async Task Delay()
            {
                await Task.Delay(5);
            }

            public async Task<int> GetInt(PointOfFailure pof)
            {
                _tracer.Log("A");
                if (pof == PointOfFailure.A) throw new Exception("GetInt Fail A");
                await Delay();
                if (pof == PointOfFailure.Canceled) throw new OperationCanceledException();
                _tracer.Log("B");
                if (pof == PointOfFailure.B) throw new Exception("GetInt Fail B");
                await Delay();
                _tracer.Log("C");
                if (pof == PointOfFailure.C) throw new Exception("GetInt Fail C");
                return 10;
            }

            public Delegate GetIntTask(PointOfFailure pof)
            {
                return new Func<Task<int>>(() => GetInt(pof));
            }

            public async Task GetVoid(PointOfFailure pof)
            {
                _tracer.Log("A");
                if (pof == PointOfFailure.A) throw new Exception("GetVoid Fail A");
                await Delay();
                if (pof == PointOfFailure.Canceled) throw new OperationCanceledException();
                _tracer.Log("B");
                if (pof == PointOfFailure.B) throw new Exception("GetVoid Fail B");
                await Delay();
                _tracer.Log("C");
                if (pof == PointOfFailure.C) throw new Exception("GetVoid Fail C");
            }

            public Delegate GetVoidTask(PointOfFailure pof)
            {
                return new Func<Task>(() => GetVoid(pof));
            }
        }

        public static string GetInnerMostMessage(Exception ex)
        {
            return ex.InnerException == null
                ? ex.Message
                : GetInnerMostMessage(ex.InnerException);
        }

        #endregion
    }
}