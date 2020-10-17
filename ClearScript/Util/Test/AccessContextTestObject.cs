using System;

namespace Microsoft.ClearScript.Util.Test
{
    internal class AccessContextTestBase
    {
        #pragma warning disable 169 // The field 'abc' is never used
        #pragma warning disable 67 // Event 'abc' is never invoked
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        // ReSharper disable EventNeverSubscribedTo.Local
        // ReSharper disable UnusedType.Local

        public AccessContextTestBase() {}
        internal AccessContextTestBase(int arg) {}
        protected AccessContextTestBase(string arg) {}
        protected internal AccessContextTestBase(DateTime arg) {}
        private  AccessContextTestBase(TimeSpan arg) {}

        public event EventHandler PublicEvent;
        internal event EventHandler InternalEvent;
        protected  event EventHandler ProtectedEvent;
        protected internal event EventHandler ProtectedInternalEvent;
        private event EventHandler PrivateEvent;

        public string PublicField;
        internal string InternalField;
        protected string ProtectedField;
        protected internal string ProtectedInternalField;
        private string privateField;

        public void PublicMethod() {}
        internal void InternalMethod() {}
        protected void ProtectedMethod() {}
        protected internal void ProtectedInternalMethod() {}
        private void PrivateMethod() {}

        public string PublicProperty { get; set; }
        internal string InternalProperty { get; set; }
        protected string ProtectedProperty { get; set; }
        protected internal string ProtectedInternalProperty { get; set; }
        private string PrivateProperty { get; set; }

        public class PublicNestedType {}
        internal sealed class InternalNestedType {}
        protected class ProtectedNestedType {}
        protected internal sealed class ProtectedInternalNestedType {}
        private sealed class PrivateNestedType {}

        // ReSharper restore UnusedType.Local
        // ReSharper restore EventNeverSubscribedTo.Local
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local
        #pragma warning restore 67 // Event 'abc' is never invoked
        #pragma warning restore 169 // The field 'abc' is never used
    }

    internal sealed class AccessContextTestObject : AccessContextTestBase
    {
    }
}
