using System;
using System.Globalization;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Test
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    internal sealed class V8HostObjectTest
    {
        private V8ScriptEngine engine;
        private HostObject hostObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDateTimeConversion);
            hostObject = new HostObject();
            engine.AddHostObject("hostObject", hostObject);

            engine.AddHostObject("assert", new InvokeHostObject((args, _) =>
            {
                if (args.Length != 1)
                    throw new ArgumentException($"Expected 1 argument, but got {args.Length}");
                
                Assert.That(args[0].GetBoolean());
            }));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            engine.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            hostObject.GetNamedProperty = null;
            hostObject.SetNamedProperty = null;
            hostObject.DeleteNamedProperty = null;
            hostObject.GetIndexedProperty = null;
            hostObject.SetIndexedProperty = null;
            hostObject.DeleteIndexedProperty = null;
            hostObject.GetEnumerator = null;
            hostObject.GetAsyncEnumerator = null;
            hostObject.GetNamedPropertyNames = null;
            hostObject.GetIndexedPropertyIndices = null;
        }

        [Test]
        public void DeleteIndexedProperty()
        {
            bool wasCalled = false;

            hostObject.DeleteIndexedProperty = index =>
            {
                wasCalled = true;
                Assert.That(index, Is.EqualTo(42));
                return true;
            };
            
            engine.Execute(@"{
                delete hostObject[42];
            }");
            
            Assert.That(wasCalled);
        }
        
        [Test]
        public void DeleteNamedProperty()
        {
            bool wasCalled = false;
            
            hostObject.DeleteNamedProperty = name =>
            {
                wasCalled = true;
                Assert.That(name.Equals("deleteProperty"));
                return true;
            };
            
            engine.Execute(@"{
                delete hostObject.deleteProperty;
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void GetBoolean()
        {
            bool wasCalled = false;
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getBoolean"));
                value.SetBoolean(true);
            };

            object result = engine.Evaluate(@"{
                let value = hostObject.getBoolean;
                assert(value === true);
                value;
            }");
            
            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<bool>());
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetDateTime()
        {
            bool wasCalled = false;
            
            var ponyEpoch = DateTime.Parse("2010-10-10T20:30:00Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getDateTime"));
                value.SetDateTime(ponyEpoch);
            };
            
            object result = engine.Evaluate(@"{
                let value = hostObject.getDateTime;
                assert(value instanceof Date);
                let ponyEpoch = new Date('2010-10-10T20:30:00Z');
                assert(value.getTime() === ponyEpoch.getTime());
                value;
            }");
            
            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<DateTime>());
            Assert.That(result, Is.EqualTo(ponyEpoch));
        }

        [Test]
        public void GetHostObject()
        {
            bool wasCalled = false;
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getHostObject"));
                value.SetHostObject(hostObject);
            };

            object result = engine.Evaluate(@"{
                let value = hostObject.getHostObject;
                assert(value === hostObject);
                value;
            }");

            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<HostObject>());
            Assert.That(result, Is.EqualTo(hostObject));
        }

        [Test]
        public void GetIndexedProperty()
        {
            bool wasCalled = false;

            hostObject.GetIndexedProperty = (index, value) =>
            {
                wasCalled = true;
                Assert.That(index == 13);
                value.SetString("Bing bong!");
            };

            object result = engine.Evaluate(@"{
                let value = hostObject[13];
                assert(value === 'Bing bong!');
                value;
            }");

            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(result, Is.EqualTo("Bing bong!"));
        }
        
        [Test]
        public void GetInt32()
        {
            bool wasCalled = false;
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getInt32"));
                value.SetInt32(-273);
            };

            object result = engine.Evaluate(@"{
                let value = hostObject.getInt32;
                assert(value == -273);
                value;
            }");

            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<int>());
            Assert.That(result, Is.EqualTo(-273));
        }

        [Test]
        public void GetNumber()
        {
            bool wasCalled = false;
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getNumber"));
                value.SetNumber(Math.PI);
            };

            object result = engine.Evaluate(@"{
                let value = hostObject.getNumber;
                assert(value === 3.1415926535897931);
                value;
            }");
            
            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<double>());
            Assert.That(result, Is.EqualTo(Math.PI));
        }

        [Test]
        public void GetString()
        {
            bool wasCalled = false;
            
            hostObject.GetNamedProperty = (StdString name, V8Value value, out bool isConst) =>
            {
                wasCalled = true;
                isConst = false;
                Assert.That(name.Equals("getString"));
                value.SetString("Bing bong!");
            };

            object result = engine.Evaluate(@"{
                let value = hostObject.getString;
                assert(value ==='Bing bong!');
                value;
            }");
            
            Assert.That(wasCalled);
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(result, Is.EqualTo("Bing bong!"));
        }

        [Test]
        public void SetBoolean()
        {
            bool wasCalled = false;
            
            hostObject.SetNamedProperty = (name, value) =>
            {
                wasCalled = true;
                Assert.That(name.Equals("setBoolean"));
                Assert.That(value.GetBoolean(), Is.True);
            };
            
            engine.Execute(@"{
                hostObject.setBoolean = true;
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void SetDateTime()
        {
            bool wasCalled = false;
            
            var ponyEpoch = DateTime.Parse("2010-10-10T20:30:00Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);
            
            hostObject.SetNamedProperty = (name, value) =>
            {
                wasCalled = true;
                Assert.That(name.Equals("setDateTime"));
                Assert.That(value.GetDateTime(), Is.EqualTo(ponyEpoch));
            };
            
            engine.Execute(@"{
                hostObject.setDateTime = new Date('2010-10-10T20:30:00Z');
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void SetHostObject()
        {
            bool wasCalled = false;
            
            hostObject.SetNamedProperty = (name, value) =>
            {
                wasCalled = true;
                Assert.That(name.Equals("setHostObject"));
                Assert.That(value.GetHostObject(), Is.EqualTo(hostObject));
            };
            
            engine.Execute(@"{
                hostObject.setHostObject = hostObject;
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void SetIndexedProperty()
        {
            bool wasCalled = false;

            hostObject.SetIndexedProperty = (index, value) =>
            {
                wasCalled = true;
                Assert.That(index, Is.EqualTo(13));
                Assert.That(value.GetString(), Is.EqualTo("Bing bong!"));
            };
            
            engine.Execute(@"{
                hostObject[13] = 'Bing bong!';
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void SetNumber()
        {
            bool wasCalled = false;
            
            hostObject.SetNamedProperty = (name, value) =>
            {
                wasCalled = true;
                Assert.That(name.Equals("setNumber"));
                Assert.That(value.GetNumber(), Is.EqualTo(Math.PI));
            };
            
            engine.Execute(@"{
                hostObject.setNumber = 3.1415926535897931;
            }");
            
            Assert.That(wasCalled);
        }

        [Test]
        public void SetString()
        {
            bool wasCalled = false;
            
            hostObject.SetNamedProperty = (name, value) =>
            {
                wasCalled = true;
                Assert.That(name.Equals("setString"));
                Assert.That(value.GetString(), Is.EqualTo("Bing bong!"));
            };
            
            engine.Execute(@"{
                hostObject.setString = 'Bing bong!';
            }");
            
            Assert.That(wasCalled);
        }
        
        private sealed class HostObject : IV8HostObject
        {
            public GetNamedPropertyCallback GetNamedProperty;
            public SetNamedPropertyCallback SetNamedProperty;
            public DeleteNamedPropertyCallback DeleteNamedProperty;
            public GetIndexedPropertyCallback GetIndexedProperty;
            public SetIndexedPropertyCallback SetIndexedProperty;
            public DeleteIndexedPropertyCallback DeleteIndexedProperty;
            public GetEnumeratorCallback GetEnumerator;
            public GetAsyncEnumeratorCallback GetAsyncEnumerator;
            public GetNamedPropertyNamesCallback GetNamedPropertyNames;
            public GetIndexedPropertyIndicesCallback GetIndexedPropertyIndices;

            void IV8HostObject.GetNamedProperty(StdString name, V8Value value, out bool isConst) =>
                GetNamedProperty(name, value, out isConst);

            void IV8HostObject.SetNamedProperty(StdString name, V8Value.Decoded value) =>
                SetNamedProperty(name, value);

            bool IV8HostObject.DeleteNamedProperty(StdString name) =>
                DeleteNamedProperty(name);

            void IV8HostObject.GetIndexedProperty(int index, V8Value value) =>
                GetIndexedProperty(index, value);

            void IV8HostObject.SetIndexedProperty(int index, V8Value.Decoded value) =>
                SetIndexedProperty(index, value);

            bool IV8HostObject.DeleteIndexedProperty(int index) =>
                DeleteIndexedProperty(index);

            void IV8HostObject.GetEnumerator(V8Value result) =>
                GetEnumerator(result);

            void IV8HostObject.GetAsyncEnumerator(V8Value result) =>
                GetAsyncEnumerator(result);

            void IV8HostObject.GetNamedPropertyNames(StdStringArray names) =>
                GetNamedPropertyNames(names);

            void IV8HostObject.GetIndexedPropertyIndices(StdInt32Array indices) =>
                GetIndexedPropertyIndices(indices);
        }

        private delegate void GetNamedPropertyCallback(StdString name, V8Value value, out bool isConst);

        private delegate void SetNamedPropertyCallback(StdString name, V8Value.Decoded value);

        private delegate bool DeleteNamedPropertyCallback(StdString name);
        
        private delegate void GetIndexedPropertyCallback(int index, V8Value value);

        private delegate void SetIndexedPropertyCallback(int index, V8Value.Decoded value);

        private delegate bool DeleteIndexedPropertyCallback(int index);

        private delegate void GetEnumeratorCallback(V8Value result);

        private delegate void GetAsyncEnumeratorCallback(V8Value result);

        private delegate void GetNamedPropertyNamesCallback(StdStringArray names);

        private delegate void GetIndexedPropertyIndicesCallback(StdInt32Array indices);
    }
}