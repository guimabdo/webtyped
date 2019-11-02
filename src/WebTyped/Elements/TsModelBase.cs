using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped.Elements {
	public abstract class TsModelBase : ITsFile {
		public INamedTypeSymbol TypeSymbol { get; private set; }
		public TypeResolver TypeResolver { get; private set; }
		public Options Options { get; private set; }
		//public string ClassName { get; set; }

		public string ClassName {
			get {
				if (TypeSymbol.TypeArguments.Any()) {
					//if (TypeSymbol.TypeArguments.Any()) {
					//	// fileNameCore += $"_of_{string.Join('_', TypeSymbol.TypeArguments.Select(ta => ta.Name))}";
					//	fileNameCore += $"Of{TypeSymbol.TypeArguments.Count()}";
					//}
					return $"{TypeSymbol.Name}Of{TypeSymbol.TypeArguments.Count()}";
				}
				else {
					return TypeSymbol.Name;
				}
			}
		}

		public string FullName {
			get {
				return ClassName;
				//if(Options.TypingsScope == TypingsScope.Module) { return ClassName; }
				//if (string.IsNullOrEmpty(Module)) { return ClassName; }
				//return $"{Module}.{ClassName}";
			}
		}

		public string ModuleCamel {
			get {
				//return string.Join('.', Module.Split('.').Select(s => s.ToCamelCase()));
				return Module.ToCamelCase();
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

        public bool HasCustomMap {
            get {
                if(Options.CustomMap == null) { return false; }
                return Options.CustomMap.ContainsKey(TypeSymbol.ConstructedFrom.ToString());
            }
        }

		//public (string name, string module)? ExternalType {
		//	get {
  //              //if(Options.CustomMap == null) { return null; }
  //              //if (!Options.CustomMap.ContainsKey(TypeSymbol.ConstructedFrom.ToString())) { return null; }

  //              //OBSOLETE (WebTyped.Attributes)
		//		var clientTypeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == nameof(ClientTypeAttribute));

		//		if (clientTypeAttr == null) { return null; }
		//		var args = clientTypeAttr.ConstructorArguments.ToList();
		//		var name = (args[0].Value ?? TypeSymbol.Name).ToString();
		//		var module = args[1].Value != null ? args[1].Value.ToString() : null;
		//		return (name, module);
		//	}
		//}


		//string FullClassName {
		//	get {
		//		if (string.IsNullOrEmpty(Module)) { return TypeSymbol.Name; }
		//		return $"{Module}.{TypeSymbol.Name}";
		//	}
		//}
		public string OutputFilePath {
			get {
				//if (Options.TypingsScope == TypingsScope.Module) {
				//	return Path.Combine(Options.OutDir, ModuleCamel, Filename);
				//}
				//return Path.Combine(Options.TypingsDir, Filename);
				return Path.Combine(Options.OutDir, ModuleCamel, Filename);
			}
		}

		public string FilenameWithoutExtenstion {
			get {
				//if(Options.TypingsScope == TypingsScope.Module) {
				//	return ClassName.ToCamelCase();
				//}

				//var fileNamePart = ClassName.ToCamelCase();
				////if (TypeSymbol.TypeArguments.Any()) {
				////	// fileNameCore += $"_of_{string.Join('_', TypeSymbol.TypeArguments.Select(ta => ta.Name))}";
				////	fileNameCore += $"Of{TypeSymbol.TypeArguments.Count()}";
				////}
				//if (string.IsNullOrEmpty(Module)) { return $"{fileNamePart}.d"; }
				//return $"{Module}.{fileNamePart}.d";
				return ClassName.ToCamelCase();
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

		protected void AppendKeysAndNames(StringBuilder sb) {
			var level = 0;
			sb.AppendLine();
			sb.AppendLine($"export class {ClassName}$ {{");
			level++;

			//Class name
			sb.AppendLine(level, $"static readonly $ = '{ClassName}';");

			//Members
			var currentTypeSymbol = TypeSymbol;
			var members = new List<ISymbol>();
			do {
				members.AddRange(currentTypeSymbol.GetMembers());
				currentTypeSymbol = currentTypeSymbol.BaseType;
			} while (currentTypeSymbol != null);

            HashSet<string> appendedMembers = new HashSet<string>();

			foreach (var m in members) {
				if (m.Kind != SymbolKind.Field && m.Kind != SymbolKind.Property) {
					continue;
				}
				if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
				var name = m.Name;
				if (!Options.KeepPropsCase && !((m as IFieldSymbol)?.IsConst).GetValueOrDefault()) {
					name = name.ToCamelCase();
				}

                //Avoid dup
                if (appendedMembers.Contains(name)) { continue; }

                appendedMembers.Add(name);
				sb.AppendLine(level, $"static readonly ${name} = '{name}';");
				//sb.AppendLine(level, $"export namespace {name} {{");
				//level++;
				//sb.AppendLine(level, $"export const $nameof = '{name}';");
				//level--;
				//sb.AppendLine(level, "}");
				// sb.AppendLine(2, $@"{name} = ""{name}"",");
			}

			level--;
			sb.AppendLine("}");
		}

		public abstract (string file, string content)? GenerateOutput();
	}
}
