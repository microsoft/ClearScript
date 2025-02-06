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