using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class StdV8ValueArrayTest
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
        public void WriteReadInt32()
        {
            engine.ScriptInvoke(() =>
            {
                using var array = new StdV8ValueArray(5);

                for (int i = 0; i < 5; i++)
                    array[i].SetInt32(i);

                for (int i = 0; i < 5; i++)
                {
                    var decoded = array[i].Decode();
                    Assert.That(decoded.GetInt32(), Is.EqualTo(i));
                }
            });
        }
    }
}
