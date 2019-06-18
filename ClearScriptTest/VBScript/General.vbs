' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT license.

set System = clr.System

set TestObjectT = host.type("Microsoft.ClearScript.Test.GeneralTestObject", "ClearScriptTest")
set tlist = host.newObj(System.Collections.Generic.List(TestObjectT))
call tlist.Add(host.newObj(TestObjectT, "Eóin", 20))
call tlist.Add(host.newObj(TestObjectT, "Shane", 16))
call tlist.Add(host.newObj(TestObjectT, "Cillian", 8))
call tlist.Add(host.newObj(TestObjectT, "Sasha", 6))
call tlist.Add(host.newObj(TestObjectT, "Brian", 3))

class VBTestObject
   public name
   public age
end class

function createTestObject(name, age)
   dim testObject
   set testObject = new VBTestObject
   testObject.name = name
   testObject.age = age
   set createTestObject = testObject
end function

set olist = host.newObj(System.Collections.Generic.List(System.Object))
call olist.Add(createTestObject("Brian", 3))
call olist.Add(createTestObject("Sasha", 6))
call olist.Add(createTestObject("Cillian", 8))
call olist.Add(createTestObject("Shane", 16))
call olist.Add(createTestObject("Eóin", 20))

set dict = host.newObj(System.Collections.Generic.Dictionary(System.String, System.String))
call dict.Add("foo", "bar")
call dict.Add("baz", "qux")
set value = host.newVar(System.String)
result = dict.TryGetValue("foo", value.out)

set expando = host.newObj(System.Dynamic.ExpandoObject)
set expandoCollection = host.cast(System.Collections.Generic.ICollection(System.Collections.Generic.KeyValuePair(System.String, System.Object)), expando)

set onEventRef = GetRef("onEvent")
sub onEvent(s, e)
    call System.Console.WriteLine("Property changed: {0}; new value: {1}", e.PropertyName, eval("s." + e.PropertyName))
end sub

set onStaticEventRef = GetRef("onStaticEvent")
sub onStaticEvent(s, e)
    call System.Console.WriteLine("Property changed: {0}; new value: {1} (static event)", e.PropertyName, e.PropertyValue)
end sub

set eventCookie = tlist.Item(0).Change.connect(onEventRef)
set staticEventCookie = TestObjectT.StaticChange.connect(onStaticEventRef)
tlist.Item(0).Name = "Jerry"
tlist.Item(1).Name = "Ellis"
tlist.Item(0).Name = "Eóin"
tlist.Item(1).Name = "Shane"

call eventCookie.disconnect()
call staticEventCookie.disconnect()
tlist.Item(0).Name = "Jerry"
tlist.Item(1).Name = "Ellis"
tlist.Item(0).Name = "Eóin"
tlist.Item(1).Name = "Shane"
