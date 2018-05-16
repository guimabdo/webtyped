using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;

namespace WebTyped.Elements {
	public abstract class TsModelBase : ITsFile {
		public INamedTypeSymbol TypeSymbol { get; private set; }
		public TypeResolver TypeResolver { get; private set; }
		public Options Options { get; private set; }
		//public string ClassName { get; set; }

		public string ClassName { get { return TypeSymbol.Name; } }

		public string FullName {
			get {
				if (string.IsNullOrEmpty(Module)) { return ClassName; }
				return $"{Module}.{ClassName}";
			}
		}


		public string Module {
			get {
				var parent = TypeSymbol.ContainingType;
				if (parent != null && TypeResolver.TypeFiles.TryGetValue(parent, out var parentElement)) {
					return $"{parentElement.FullName}";
				}

				//Module will be Namespace.{parent classes}
				//var moduleName = Options.TruncateNamespace(TypeSymbol.ContainingNamespace);
				var moduleName = "";
				if (!TypeSymbol.ContainingNamespace.IsGlobalNamespace) {
					moduleName = TypeSymbol.ContainingNamespace.ToString();
				}
				var parents = new List<string>();
				while (parent != null) {
					parents.Add(parent.Name);
					parent = parent.ContainingType;
				}
				if (parents.Any()) {
					parents.Reverse();
					moduleName += $".{string.Join(".", parents)}";
				}
				return Options.AdjustModule(moduleName);
				//return moduleName;
			}
		}

		public (string name, string module)? ExternalType {
			get {
				var clientTypeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == nameof(ClientTypeAttribute));

				if (clientTypeAttr == null) { return null; }
				var args = clientTypeAttr.ConstructorArguments.ToList();
				var name = (args[0].Value ?? TypeSymbol.Name).ToString();
				var module = args[1].Value != null ? args[1].Value.ToString() : null;
				return (name, module);
			}
		}


		//string FullClassName {
		//	get {
		//		if (string.IsNullOrEmpty(Module)) { return TypeSymbol.Name; }
		//		return $"{Module}.{TypeSymbol.Name}";
		//	}
		//}

		string FilenameWithoutExtenstion {
			get {
				var fileNameCore = TypeSymbol.Name.ToCamelCase();
				if (TypeSymbol.TypeArguments.Any()) {
					fileNameCore += $"_of_{string.Join('_', TypeSymbol.TypeArguments.Select(ta => ta.Name))}";
				}
				if (string.IsNullOrEmpty(Module)) { return $"{fileNameCore}.d"; }
				return $"{Module}.{fileNameCore}.d";
			}
		}

		protected string Filename {
			get {
				return $"{FilenameWithoutExtenstion}.ts";
				//if (this.TypeSymbol.TypeArguments.Any()) {
				//return $"{FilenameWithoutExtenstion}`.ts";
				//} else {
				//	return $"{FilenameWithoutExtenstion}.ts";
				//}
			}
		}

		public TsModelBase(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) {
			TypeSymbol = modelType;
			TypeResolver = typeResolver;
			Options = options;
		}

		//public abstract Task<string> SaveAsync();
		public abstract (string file, string content)? GenerateOutput();
	}
}
