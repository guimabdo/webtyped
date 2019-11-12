using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	[TestClass]
	public class FileSystemTest {
		[TestMethod]
		public async Task IndexFolderCasingShouldBeAllCamel() {
			var output = await TestHelpers.Generate("namespace Some.NameSpace { public class MyClass {} }");
			Assert.IsTrue(output.ElementAt(0).Key.StartsWith(".\\some.nameSpace\\"));
		}

		//string Read(string file) {
		//	return File.ReadAllText($"../../../Inputs/{file}");
		//}

		//async Task AssertOutput(string file) {
		//	var cs = Read($"{file}.cs");
		//	var output = await TestHelpers.Generate(cs);
		//	Assert.AreEqual(Read($"{file}-output.ts"), output.First().Value);
		//}
	}
}
