using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	[TestClass]
	public class ExtrernalAssemblyTest
    {
		[TestMethod]
		public async Task GenerateFromAssemblyTest() {
            var options = new Options(".\\", 
                false, ServiceMode.Angular, 
                new string[0], "", 
                false, 
                null
            );
            var generator = new Generator(
                new string[] {
@"
using ExternalLib.Models;
public class Test : ExternalModel {
    public int Test2 {get;set;}
}
"
                }
                ,
                new string[] {
                    @"..\..\..\..\ExternalLib\bin\Debug\netstandard2.0\ExternalLib.dll"
                },
                new Package[] {
                    new Package
                    {
                        Name = "MSTest.TestFramework",
                        Csproj = @"..\..\..\WebTyped.Tests.csproj"
                        // Version = "1.1.18"
                    }
                },
                new string[] {
                    "ExternalLib.Models.*",
                    "Cblx.*Model"
                },
                options);
            var output = await generator.GenerateOutputsAsync();
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
