using Microsoft.ClearScript.V8.SplitProxy;
using NUnit.Framework;

namespace Microsoft.ClearScript.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public sealed class StdStringTest
    {
        [Test]
        public void GetValueTest()
        {
            using var stdString = new StdString("Bing bong!");
            Assert.That(stdString.Equals("Bing bong!"));
            Assert.That(stdString.ToString() == "Bing bong!");
        }
    }
}
