using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTyped.Functions {
	public class Api {
		public async Task<ResultModel> Generate(GenerateModel model) {
			var result = new ResultModel();

			Console.WriteLine("Creating generator");
			var generator = new Generator(
				model.Files.Select(f => f.Content),
				model.Config.ToOptions()

			);
			Console.WriteLine("Generator created");
			Console.WriteLine("Generating Output");
			var output = await generator.GenerateOutputsAsync();
			Console.WriteLine("Output Generated");
			
			foreach (var f in output) {
				result.Files.Add(new FileModel { Path = f.Key, Content = f.Value });
			}

			return result;
		}
	}
}
