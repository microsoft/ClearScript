using System.Dynamic;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a script object.
    /// </summary>
    /// <remarks>
    /// Use this class in conjunction with C#'s
    /// <c><see href="http://msdn.microsoft.com/en-us/library/scekt9xw(VS.71).aspx">is</see></c>
    /// operator to identify script objects.
    /// </remarks>
    /// <seealso cref="ScriptEngine.Evaluate(string, bool, string)"/>
    public abstract class ScriptObject : DynamicObject
    {
        internal ScriptObject()
        {
        }

        /// <summary>
        /// Gets the script engine that owns the object.
        /// </summary>
        public abstract ScriptEngine Engine { get; }
    }
}
