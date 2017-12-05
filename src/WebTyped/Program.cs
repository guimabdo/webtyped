using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

				target.HelpOption("-?|-h|--help");
				target.OnExecute(async () => {
					var matcher = new Matcher();
					foreach (var val in sourceFiles.Values) {
						matcher.AddInclude(val);
					}
					var csFiles = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory());
					var trees = new List<SyntaxTree>();
					var tasks = new List<Task>();
					foreach (var csFile in csFiles) {
						tasks.Add(File.ReadAllTextAsync(csFile)
							.ContinueWith(tsk => {
								trees.Add(CSharpSyntaxTree.ParseText(tsk.Result));
							}));
						//trees.Add(CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(csFile)));
					}
					////Wait all task
					//while (tasks.Any()) {
					//	tasks.RemoveAll(t => t.IsCompleted);
					//	//var completed = tasks.FirstOrDefault(t => t.IsCompleted);
					//	//if (completed != null) {
					//	//	//trees.Add(CSharpSyntaxTree.ParseText(await completed));
					//	//	tasks.Remove(completed);
					//	//}
					//}

					//References
					var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
					var systemRuntime = MetadataReference.CreateFromFile(typeof(int).Assembly.Location);
					var linqExpressions = MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location);
					foreach (var task in tasks) { await task; }
					var compilation = CSharpCompilation.Create("Comp", trees, new[] { mscorlib, systemRuntime, linqExpressions });

					//1200ms

					var semanticModels = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));
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

					var options = new Options(outDir.Value(), clear.HasValue(), svModeEnum, trims.Values, baseModule.Value());
					var typeResolver = new TypeResolver(options);

					//1330ms
					tasks = new List<Task>();
					var namedTypeSymbols = new List<INamedTypeSymbol>();
					foreach (var t in trees) {
						tasks.Add(t.GetRootAsync().ContinueWith(tks => {
							var root = tks.Result;
							foreach (var @type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>()) {
								var sm = semanticModels[t];
								namedTypeSymbols.Add(sm.GetDeclaredSymbol(@type));
							}
						}));
					}
					foreach (var tsk in tasks) { await tsk; }
					foreach (var s in namedTypeSymbols) {
						if (Service.IsService(s)) {
							typeResolver.Add(new Service(s, typeResolver, options));
							continue;
						}

						if (Model.IsModel(s)) {
							typeResolver.Add(new Model(s, typeResolver, options));
							continue;
						}

						if (TsEnum.IsEnum(s)) {
							typeResolver.Add(new TsEnum(s, typeResolver, options));
							continue;
						}
					}
					//return 0;

					await typeResolver.SaveAllAsync();
					//var saveTask = typeResolver.SaveAllAsync();
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
