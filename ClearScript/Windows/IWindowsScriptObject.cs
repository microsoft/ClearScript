namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents a Windows Script object.
    /// </summary>
    public interface IWindowsScriptObject
    {
        /// <summary>
        /// Provides access to the underlying unmanaged COM object.
        /// </summary>
        /// <returns>An object that represents the underlying unmanaged COM object.</returns>
        object GetUnderlyingObject();
    }
}
