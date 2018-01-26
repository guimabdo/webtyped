using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;
using WebTyped.Elements;

namespace WebTyped {
	public class Program {
		public static int Main(params string[] args) {
			var cmd = new CommandLineApplication() {
				Name = "WebTyped",
				Description = "Generate typescript services and model definitions based on your webApis"
			};
			cmd.VersionOption("--version", "0.1.0");
			cmd.HelpOption("-?|-h|--help");

			cmd.Command("generate", target => {
				var sourceFiles = target.Option("-sf | --sourceFiles", "C# source files (glob supported)", CommandOptionType.MultipleValue);
				var outDir = target.Option("-od | --outDir", "", CommandOptionType.SingleValue);
				var trims = target.Option("-t | --trim", "These module names will be removed when generating ts code", CommandOptionType.MultipleValue);
				var clear = target.Option("-c | --clear", "Clears folder", CommandOptionType.NoValue);
				var serviceMode = target.Option("-sm | --serviceMode", "Http connection fmwork (angular, jquery, fetch)", CommandOptionType.SingleValue);
				var baseModule = target.Option("-bm | --baseModule", "Base module for your types", CommandOptionType.SingleValue);
				var keepPropsCase = target.Option("--keepPropsCase", "Keep properties case", CommandOptionType.NoValue);

				target.HelpOption("-?|-h|--help");
				target.OnExecute(async () => {
					var matcher = new Matcher();
					foreach (var val in sourceFiles.Values) {
						matcher.AddInclude(val);
					}
					var csFiles = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory());
					var trees = new ConcurrentDictionary<string, SyntaxTree>();
					var tasks = new List<Task>();
					foreach (var csFile in csFiles) {
						tasks.Add(File.ReadAllTextAsync(csFile)
							.ContinueWith(tsk => {
								trees.TryAdd(csFile, CSharpSyntaxTree.ParseText(tsk.Result));
							}));
					}
					
					var svModeEnum = ServiceMode.Angular;
					if (serviceMode.HasValue()) {
						switch (serviceMode.Value()) {
							case "jquery":
								svModeEnum = ServiceMode.Jquery;
								break;
							case "fetch":
								svModeEnum = ServiceMode.Fetch;
								break;
						}
					}

					var options = new Options(outDir.Value(), clear.HasValue(), svModeEnum, trims.Values, baseModule.Value(), keepPropsCase.HasValue());

					var gen = new Generator(trees.Values, options);
					await gen.WriteFilesAsync();
					return 0;
				});
			});

			cmd.OnExecute(() => {
				cmd.ShowHelp();
				return 0;
			});

			return cmd.Execute(args);
		}
	}
}
