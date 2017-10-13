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
				"--outDir", "../../../../WebTyped.Example.Web/ClientApp/app/webApi/"
				//"--controllers", "../../../UnitTest1.cs",
				//"--models", "../../../UnitTest1.cs",
			});
			Assert.AreEqual(0, result);
		}
	}
}
