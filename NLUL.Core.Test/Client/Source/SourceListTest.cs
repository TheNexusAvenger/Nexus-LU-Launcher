using NLUL.Core.Client.Source;
using NUnit.Framework;

namespace NLUL.Core.Test.Client.Source
{
    [TestFixture]
    public class SourceListTest
    {
        [Test]
        public void TestDefaultList()
        {
            var sources = SourceList.GetSources(SourceList.GetLocalSources());
            foreach (var source in sources)
            {
                Assert.NotNull(source.Name, "Name is null.");
                Assert.NotNull(source.Method, "Method is null.");
                Assert.NotNull(source.Type, "Type is null.");
                Assert.NotNull(source.Url, "Url is null.");
                Assert.NotNull(source.Patches, "Patches is null.");
                foreach (var patch in source.Patches)
                {
                    Assert.NotNull(patch.Name, "Patch.Name is null.");
                }
            }
        }
    }
}