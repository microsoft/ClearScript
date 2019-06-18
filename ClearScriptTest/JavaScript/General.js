// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

System = clr.System;

TestObject = host.type('Microsoft.ClearScript.Test.GeneralTestObject', 'ClearScriptTest');
tlist = host.newObj(System.Collections.Generic.List(TestObject));
tlist.Add(host.newObj(TestObject, 'Eóin', 20));
tlist.Add(host.newObj(TestObject, 'Shane', 16));
tlist.Add(host.newObj(TestObject, 'Cillian', 8));
tlist.Add(host.newObj(TestObject, 'Sasha', 6));
tlist.Add(host.newObj(TestObject, 'Brian', 3));

olist = host.newObj(System.Collections.Generic.List(System.Object));
olist.Add({ name: 'Brian', age: 3 });
olist.Add({ name: 'Sasha', age: 6 });
olist.Add({ name: 'Cillian', age: 8 });
olist.Add({ name: 'Shane', age: 16 });
olist.Add({ name: 'Eóin', age: 20 });

dict = host.newObj(System.Collections.Generic.Dictionary(System.String, System.String));
dict.Add('foo', 'bar');
dict.Add('baz', 'qux');
value = host.newVar(System.String);
result = dict.TryGetValue('foo', value.out);

bag = host.newObj();
bag.method = function (x) { System.Console.WriteLine(x * x); };
bag.proc = host.del(System.Action(System.Object), bag.method);

expando = host.newObj(System.Dynamic.ExpandoObject);
expandoCollection = host.cast(System.Collections.Generic.ICollection(System.Collections.Generic.KeyValuePair(System.String, System.Object)), expando);

function onChange(s, e) {
    System.Console.WriteLine('Property changed: {0}; new value: {1}', e.PropertyName, s[e.PropertyName]);
};
function onStaticChange(s, e) {
    System.Console.WriteLine('Property changed: {0}; new value: {1} (static event)', e.PropertyName, e.PropertyValue);
};
eventCookie = tlist.Item(0).Change.connect(onChange);
staticEventCookie = TestObject.StaticChange.connect(onStaticChange);
tlist.Item(0).Name = 'Jerry';
tlist.Item(1).Name = 'Ellis';
tlist.Item(0).Name = 'Eóin';
tlist.Item(1).Name = 'Shane';
eventCookie.disconnect();
staticEventCookie.disconnect();
tlist.Item(0).Name = 'Jerry';
tlist.Item(1).Name = 'Ellis';
tlist.Item(0).Name = 'Eóin';
tlist.Item(1).Name = 'Shane';

// ReSharper disable once WrongExpressionStatement
Math.round(Math.sin(Math.PI) * 1000e16);
