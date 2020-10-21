using System.Threading.Tasks;

namespace Microsoft.ClearScript.JavaScript
{
    internal interface IJavaScriptEngine
    {
        uint BaseLanguageVersion { get; }

        void CompletePromiseWithResult<T>(Task<T> task, object resolve, object reject);
        void CompletePromise(Task task, object resolve, object reject);
    }
}
