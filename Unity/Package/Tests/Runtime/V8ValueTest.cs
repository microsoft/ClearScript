using System;
using System.Globalization;
using System.Numerics;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class V8ValueTest
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
        public void SetAndGetBigInt()
        {
            var avogadro = BigInteger.Parse("602200000000000000000000");

            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetBigInt(avogadro);
                var decoded = value.Decode();
                Assert.That(decoded.GetBigInt(), Is.EqualTo(avogadro));
            });
        }

        [Test]
        public void SetAndGetBoolean()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetBoolean(true);
                var decoded = value.Decode();
                Assert.That(decoded.GetBoolean(), Is.True);
            });
        }

        [Test]
        public void SetAndGetDateTime()
        {
            var ponyEpoch = DateTime.Parse("2010-10-10T20:30:00Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);

            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetDateTime(ponyEpoch);
                var decoded = value.Decode();
                Assert.That(decoded.GetDateTime(), Is.EqualTo(ponyEpoch));
            });
        }

        [Test]
        public void SetAndGetHostObject()
        {
            var hostObject = new HostObject();

            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetHostObject(hostObject);
                var decoded = value.Decode();
                Assert.That(decoded.GetHostObject(), Is.EqualTo(hostObject));
            });
        }

        [Test]
        public void SetAndGetInt32()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetInt32(-273);
                var decoded = value.Decode();
                Assert.That(decoded.GetInt32(), Is.EqualTo(-273));
            });
        }

        [Test]
        public void SetAndGetNull()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetNull();
                var decoded = value.Decode();
                Assert.That(decoded.Type, Is.EqualTo(V8Value.Type.Null));
            });
        }

        [Test]
        public void SetAndGetNullHostObject()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetHostObject(null);
                var decoded = value.Decode();
                Assert.That(decoded.Type, Is.EqualTo(V8Value.Type.Null));
            });
        }

        [Test]
        public void SetAndGetNullString()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetString(null);
                var decoded = value.Decode();
                Assert.That(decoded.Type, Is.EqualTo(V8Value.Type.Null));
            });
        }

        [Test]
        public void SetAndGetNumber()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetNumber(Math.PI);
                var decoded = value.Decode();
                Assert.That(decoded.GetNumber(), Is.EqualTo(Math.PI));
            });
        }

        [Test]
        public void SetAndGetString()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetString("Bing bong!");
                var decoded = value.Decode();
                Assert.That(decoded.GetString(), Is.EqualTo("Bing bong!"));
            });
        }

        [Test]
        public void SetAndGetUInt32()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetUInt32(uint.MaxValue);
                var decoded = value.Decode();
                Assert.That(decoded.GetUInt32(), Is.EqualTo(uint.MaxValue));
            });
        }

        [Test]
        public void SetAndGetUndefined()
        {
            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetUndefined();
                var decoded = value.Decode();
                Assert.That(decoded.Type, Is.EqualTo(V8Value.Type.Undefined));
            });
        }

        [Test]
        public void SetAndGetV8Object()
        {
            var scriptObject = (ScriptObject)engine.Evaluate("({ })");

            engine.ScriptInvoke(() =>
            {
                using var value = V8Value.New();
                value.SetV8Object(scriptObject);
                using var decoded = value.Decode();

                Assert.That(decoded.GetV8Object().GetHashCode(),
                    Is.EqualTo(scriptObject.GetHashCode()));
            });
        }

        private sealed class HostObject : IV8HostObject
        {
        }
    }
}