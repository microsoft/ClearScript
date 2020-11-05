using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class TypeGuidMocks
    {
        [Guid(TypeGuids.Nothing)]
        public abstract class Nothing
        {
        }

        [Guid(TypeGuids.VBScriptEngine)]
        public abstract class VBScriptEngine
        {
        }

        [Guid(TypeGuids.WindowsScriptItem)]
        public abstract class WindowsScriptItem
        {
        }
    }
}
