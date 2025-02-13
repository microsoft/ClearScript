using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public sealed class V8ObjectTest
    {
        private V8ScriptEngine engine;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            engine = new V8ScriptEngine();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            engine.Dispose();
        }

        [Test]
        public new void GetHashCode()
        {
            engine.Execute("var sameObject = { }");
            var sameObject0 = (ScriptObject)engine.Evaluate("sameObject");
            var sameObject1 = (ScriptObject)engine.Evaluate("sameObject");
            Assume.That(!object.ReferenceEquals(sameObject0, sameObject1));

            engine.ScriptInvoke(() =>
            {
                var v8Object0 = new V8Object(sameObject0);
                var v8Object1 = new V8Object(sameObject1);
                Assume.That(v8Object0.ptr != v8Object1.ptr);
                Assert.That(v8Object0.GetHashCode() == v8Object1.GetHashCode());
            });
        }

        [Test]
        public void GetIndexedProperty()
        {
            var scriptObject = (ScriptObject)engine.Evaluate("[1, 2, 3]");

            engine.ScriptInvoke(() =>
            {
                var v8Object = new V8Object(scriptObject);
                using var value = V8Value.New();

                for (int i = 0; i < 3; i++)
                {
                    v8Object.GetIndexedProperty(i, value);
                    using var decoded = value.Decode();
                    Assert.That(decoded.GetNumber(), Is.EqualTo(i + 1.0));
                }
            });
        }

        [Test]
        public void GetNamedProperty()
        {
            var scriptObject = (ScriptObject)engine.Evaluate(@"({
                jinxies: 'Bing bong!'
            })");

            engine.ScriptInvoke(() =>
            {
                using var name = new StdString("jinxies");
                using var value = V8Value.New();
                new V8Object(scriptObject).GetNamedProperty(name, value);
                using var decoded = value.Decode();
                Assert.That(decoded.GetString(), Is.EqualTo("Bing bong!"));
            });
        }

        [Test]
        public void Invoke()
        {
            var scriptObject = (ScriptObject)engine.Evaluate(@"(function(a, b) {
                return a + b;
            })");

            engine.ScriptInvoke(() =>
            {
                using var args = new StdV8ValueArray(2);
                args[0].SetString("bing");
                args[1].SetString("bong");
                using var result = V8Value.New();
                new V8Object(scriptObject).Invoke(args, result);
                using var decoded = result.Decode();
                Assert.That(decoded.GetString(), Is.EqualTo("bingbong"));
            });
        }
    }
}
