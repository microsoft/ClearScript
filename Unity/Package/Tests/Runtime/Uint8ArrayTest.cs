using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public sealed class Uint8ArrayTest
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
        public void CopyTo()
        {
            engine.AddHostObject("receiveArray", new InvokeHostObject((args, _) =>
            {
                var uint8Array = args[0].GetUint8Array();
                var byteArray = new byte[5];
                uint8Array.CopyTo(byteArray);
                Assert.That(byteArray[0], Is.EqualTo(1));
                Assert.That(byteArray[1], Is.EqualTo(2));
                Assert.That(byteArray[2], Is.EqualTo(3));
                Assert.That(byteArray[3], Is.EqualTo(5));
                Assert.That(byteArray[4], Is.EqualTo(7));
            }));

            engine.Execute(@"{
                receiveArray(new Uint8Array([1, 2, 3, 5, 7]));
            }");
        }
    }
}
