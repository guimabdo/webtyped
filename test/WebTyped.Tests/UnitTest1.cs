using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WebTyped.Tests {
	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public void TestMethod1() {
			var folder = Directory.GetCurrentDirectory();
			var result = Program.Main(new[] {
				"generate",
				"--sourceFiles", "../../../../WebTyped.Example.Web/Controllers/**/*.cs",
				"--sourceFiles", "../../../../WebTyped.Example.Web/Models/**/*.cs",
				"--sourceFiles", "../../../../WebTyped.Example.Web/OtherModels/**/*.cs",
				"--outDir", "../../../../WebTyped.Example.Web/ClientApp/app/webApiUnitTest/",
				"--trim", "WebTyped_Example_Web.Services",
				"--trim", "WebTyped.Example.Web",
				"--trim", "WebTyped.Example.Web.Models",
				"--baseModule", "UnitTest",
				"--clear"
				//"--controllers", "../../../UnitTest1.cs",
				//"--models", "../../../UnitTest1.cs",
			});
			Assert.AreEqual(0, result);
		}
	}
}
