using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebTyped.Annotations;

namespace WebTyped.Tests {
	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public void KeyValuePairToTsTest() {
			var tr = new TypeResolver(new Options("", false, ServiceMode.Angular, new string[0], "", false));
			var kvpType = typeof(KeyValuePair<,>);
			var assembly = MetadataReference.CreateFromFile(kvpType.Assembly.Location);
			var compilation = CSharpCompilation.Create("CompTest", references: new[] { assembly });
			//kvpType = typeof(KeyValuePair<int, string>);
			var typeSymbol = compilation.GetTypeByMetadataName(kvpType.FullName);
			typeSymbol = typeSymbol.Construct(
				compilation.GetTypeByMetadataName(typeof(int).FullName),
				compilation.GetTypeByMetadataName(typeof(string).FullName)
			);
			var name = tr.Resolve(typeSymbol, new ResolutionContext());
			Assert.AreEqual(name, "{ key: number, value: string }");
		}

		[TestMethod]
		public async Task TupleTest() {
			var cs =
@"
using System;
using WebTyped.Annotations;
using System.Threading.Tasks;
namespace Test{
	public class ModelWithTuples {
		public (int, string) Tuple1 { get; set; }
		public (int id, string name) Tuple2 { get; set; }
	}

	[Route(""api/[controller]"")]
	public class ApiWithTupleController {
		[HttpPost]
		public void Post([FromBody][NamedTuple](int id, string name) param){}

		[HttpGet]
		public void Get([NamedTuple](int id, string name) param){}

		[HttpPost]
		public void Post2([FromBody](int id, string name) param){}

		[HttpGet]
		public void Get2((int id, string name) param){}

		[HttpGet]
		public void GetErr([NamedTuple]string param){}

		[HttpGet]
		[NamedTuple]
		public (int id, string name) Get3([NamedTuple](int id, string name) param){}

		
		[HttpGet]
		public (int id, string name) Get4([NamedTuple](int id, string name) param){}

		[HttpGet]
		[NamedTuple]
		public async Task<(int id, string name)> Get5([NamedTuple](int id, string name) param){}

		[HttpGet]
		[NamedTuple]
		public object GetErr2(){}
    }
}
";

			var generator = new Generator(
				new string[] { cs },
				new Options(null, false, ServiceMode.Angular, new string[0], "", false)
			);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task UnknowInheritanceTest() {
			var cs = @"public class Model: Interface {}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
		}


		[TestMethod]
		public async Task ArrayResolutionTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
[Route(""api/[controller]"")]
public class MyController {
	[HttpGet]
	public async Task<int[]> GetArray(){ return null; }
}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task ByteArrayResolutionTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
[Route(""api/[controller]"")]
public class MyController {
	[HttpGet]
	public async Task<byte[]> GetArray(){ return null; }
}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task SeilaTest() {
			var cs =
@"
using System;
using WebTyped.Annotations;
namespace Test{
	[ClientType]
	public class SeilaTestClass {
	}
	
	public class Seila2TestClass {
		public SeilaTestClass Prop { get; set; }
	}
}
";

			var generator = new Generator(
				new string[] { cs },
				new Options(null, false, ServiceMode.Angular, new string[0], "", false)
			);

			var outputs = await generator.GenerateOutputsAsync();
			//await generator.WriteFilesAsync();
		}


		//[TestMethod]
		//public void TestMethod1() {
		//	var folder = Directory.GetCurrentDirectory();
		//	var result = Program.Main(new[] {
		//		"generate",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/Controllers/**/*.cs",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/Models/**/*.cs",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/OtherModels/**/*.cs",
		//		"--outDir", "../../../../WebTyped.Example.Web/ClientApp/app/webApiUnitTest/",
		//		"--trim", "WebTyped_Example_Web.Services",
		//		"--trim", "WebTyped.Example.Web",
		//		"--trim", "WebTyped.Example.Web.Models",
		//		"--baseModule", "UnitTest",
		//		"--clear"
		//		//"--controllers", "../../../UnitTest1.cs",
		//		//"--models", "../../../UnitTest1.cs",
		//	});
		//	Assert.AreEqual(0, result);
		//}
	}
}
