// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;

namespace Microsoft.ClearScript.Test
{
    public class GeneralTestObject
    {
        private string name;
        public string Name
        {
            get { return name; }

            set
            {
                name = value;
                InvokeChange("Name", name);
            }
        }

        private int age;
        public int Age
        {
            get { return age; }

            set
            {
                age = value;
                InvokeChange("Age", age);
            }
        }

        public event PropertyChangedEventHandler Change;

        public static event EventHandler<StaticEventArgs> StaticChange;

        public GeneralTestObject(string name, int age)
        {
            this.name = name;
            this.age = age;
        }

        private void InvokeChange(string propertyName, object propertyValue)
        {
            if (Change != null)
            {
                Change(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StaticChange != null)
            {
                StaticChange(null, new StaticEventArgs(propertyName, propertyValue));
            }
        }

        #region Nested type: StaticEventArgs

        public class StaticEventArgs : PropertyChangedEventArgs
        {
            public object PropertyValue { get; private set; }

            public StaticEventArgs(string name, object value)
                : base(name)
            {
                PropertyValue = value;
            }
        }

        #endregion
    }
}
