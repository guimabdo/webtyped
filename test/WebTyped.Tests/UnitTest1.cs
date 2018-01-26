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
		public async Task SeilaTest() {
			var cs =
@"
using System;
using WebTyped.Annotations;
namespace Test{
	[ClientType(""SomeClientName"", module: ""SomeClientExternalModule"")]
	public class SeilaTestClass {
	}
	
	public class Seila2TestClass {
		public SeilaTestClass Prop { get; set; }
	}
}
";

			var generator = new Generator(
				new string[] { cs }, 
				new Options("", false, ServiceMode.Angular, new string[0], "", false)
			);

			var outputs = await generator.GenerateOutputsAsync();
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
