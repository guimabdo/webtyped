using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	public static class TestHelpers {
		public static async Task<Dictionary<string, string>> Generate(params string[] cs) {
			var options = new Options(".\\", false, ServiceMode.Angular, new string[0], "", false, null);
			var generator = new Generator(cs, options);
			return await generator.GenerateOutputsAsync();
		}
	}
}
