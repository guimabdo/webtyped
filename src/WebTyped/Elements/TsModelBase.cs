using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				var moduleName = TypeSymbol.ContainingNamespace.ToString();
				var parents = new List<string>();
				while (parent != null) {
					parents.Add(parent.Name);
					parent = parent.ContainingType;
				}
				if (parents.Any()) {
					parents.Reverse();
					moduleName += $".{string.Join(".", parents)}";
				}
				return Options.TrimModule(moduleName);
				//return moduleName;
			}
		}

		string FullClassName {
			get {
				if (string.IsNullOrEmpty(Module)) { return TypeSymbol.Name; }
				return $"{Module}.{TypeSymbol.Name}";
			}
		}

		string FilenameWithoutExtenstion {
			get {
				if (string.IsNullOrEmpty(Module)) { return $"{TypeSymbol.Name.ToCamelCase()}.d"; }
				return $"{Module}.{TypeSymbol.Name.ToCamelCase()}.d";
			}
		}

		protected string Filename {
			get {
				return $"{FilenameWithoutExtenstion}.ts";
			}
		}

		public TsModelBase(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) {
			TypeSymbol = modelType;
			TypeResolver = typeResolver;
			Options = options;
		}

		public abstract Task<string> SaveAsync();
	}
}
