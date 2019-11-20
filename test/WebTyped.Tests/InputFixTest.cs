using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	[TestClass]
	public class InputFixTest {
        [TestMethod]
        public async Task Bla()
        {
            //Find cs files
            var matcher = new Matcher();
            matcher.AddInclude("../Hypemov.MediaStore.Data/Enums/*.cs");
            matcher.AddInclude("../Hypemov.MediaStore.Business/**/*Model.cs");
            matcher.AddInclude("../Hypemov.MediaStore.Business/Validators/FileAttribute.cs");
            matcher.AddInclude("../Hypemov.MediaStore.Web/Controllers/*Controller.cs");
            var csFiles = matcher.GetResultsInFullPath(@"C:\Repos\Hypemov.MediaStore\Hypemov.MediaStore.Web");
            var codes = csFiles.Select(f => File.ReadAllText(f));

            var options = new Options(@"C:\Tests\WebTyped\");
            var generator = new Generator(
                codes,
                new string[0],
                new Package[] { 
                    new Package
                    {
                        Name = "Cblx.Backend",
                        Csproj = "C:/Repos/Hypemov.MediaStore/Hypemov.MediaStore.Data/Hypemov.MediaStore.Data.csproj",
                    }
                },
                new string[] {
                    "Cblx.Backend.Models.*"
                },
                options
            );

            var output = await generator.GenerateAbstractionsAsync();
        }



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


        [TestMethod]
        public async Task Input4ShouldExportExternalTypeInIndex()
        {
            string file = "Input4";
            var index = 2;
            //await AssertOutput("Input4", 2);
            var cs = Read($"{file}.cs");
            //var output = await TestHelpers.Generate(cs);

            var options = new Options(".\\")
            {
                GenericReturnType = new ClientType
                {
                    Name = "Observable",
                    Module = "rxjs"
                },
                CustomMap = new System.Collections.Generic.Dictionary<string, ClientType>
                {
                    { "Bla.LazyLoadEvent", new ClientType{ Module = "primeng/components/common/lazyloadevent", Name = "LazyLoadEvent" } }
                }
            };

            var generator = new Generator(
                new string[] { cs },
                new string[0],
                new Package[0],
                new string[0],
                options
            );
            var output = await generator.GenerateOutputsAsync();

            Assert.AreEqual(Read($"{file}-output.ts")
                .Trim(),
                output.ElementAt(index).Value
                .Trim());
        }

        [TestMethod]
        public async Task Input5GenericControllerShouldBeFixed()
        {
            await AssertOutput("Input5" ,2);
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
