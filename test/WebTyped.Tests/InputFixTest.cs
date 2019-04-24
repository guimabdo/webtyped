using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	[TestClass]
	public class InputFixTest {
		[TestMethod]
		public async Task Input1ShouldBeFixed() {
			await AssertOutput("Input1");
		}


        [TestMethod]
        public async Task Input2ShouldBeFixed()
        {
            await AssertOutput("Input2", 2);
        }

        [TestMethod]
        public async Task Input3ShouldNotThrowException()
        {
            await AssertOutput("Input3");
        }

        string Read(string file) {
			return File.ReadAllText($"../../../Inputs/{file}");
		}
		async Task AssertOutput(string file, int index = 0) {
			var cs = Read($"{file}.cs");
			var output = await TestHelpers.Generate(cs);
			Assert.AreEqual(Read($"{file}-output.ts")
                .Trim(), 
                output.ElementAt(index).Value
                .Trim());
		}
	}
}
