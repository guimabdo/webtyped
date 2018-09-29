using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;
using WebTyped.Elements;

namespace WebTyped {
	public class Generator {
		IEnumerable<SyntaxTree> _trees;
		Options _options;
		public Generator(IEnumerable<string> cSharps, Options options): this(cSharps.Select(s => CSharpSyntaxTree.ParseText(s)), options) {}
		public Generator(IEnumerable<SyntaxTree> trees, Options options) {
			this._trees = trees;
			this._options = options;
		}

		async Task<TypeResolver> PrepareAsync() {
			var trees = _trees.ToList();
			//TODO: Temp workaround.. investigate why referencing annotations assembly is nor working properly
			var attributes = CSharpSyntaxTree.ParseText(@"
using System;

namespace WebTyped.Annotations {
	[AttributeUsage(AttributeTargets.Class)]
	public class ClientTypeAttribute : Attribute {
		public ClientTypeAttribute(string typeName = null, string module = null) {}
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NamedTupleAttribute : Attribute {
		public NamedTupleAttribute() {}
	}
}
				
");
			//References
			var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
			//var webTypedAnnotations = MetadataReference.CreateFromFile(typeof(ClientTypeAttribute).Assembly.Location);
			var systemRuntime = MetadataReference.CreateFromFile(typeof(int).Assembly.Location);
			var linqExpressions = MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location);

			var compilation = CSharpCompilation.Create("Comp", trees.Union(new SyntaxTree[] { attributes }), new[] { mscorlib, systemRuntime, linqExpressions/*, webTypedAnnotations*/ });
			var typeResolver = new TypeResolver(_options);
			var tasks = new List<Task>();
			var namedTypeSymbols = new ConcurrentBag<INamedTypeSymbol>();
			var semanticModels = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));
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
					typeResolver.Add(new Service(s, typeResolver, _options));
					continue;
				}

				if (Model.IsModel(s)) {
					typeResolver.Add(new Model(s, typeResolver, _options));
					continue;
				}

				if (TsEnum.IsEnum(s)) {
					typeResolver.Add(new TsEnum(s, typeResolver, _options));
					continue;
				}
			}
			return typeResolver;
		}

		public async Task<Dictionary<string ,string>> GenerateOutputsAsync() {
			var typeResolver = await PrepareAsync();
			return typeResolver.GenerateOutputs();
		}

		public async Task WriteFilesAsync() {
			var typeResolver = await PrepareAsync();
			await typeResolver.SaveAllAsync();
		}
	}
}
