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

		public string ClassName { get {
				if (TypeSymbol.TypeArguments.Any()) {
					//if (TypeSymbol.TypeArguments.Any()) {
					//	// fileNameCore += $"_of_{string.Join('_', TypeSymbol.TypeArguments.Select(ta => ta.Name))}";
					//	fileNameCore += $"Of{TypeSymbol.TypeArguments.Count()}";
					//}
					return $"{TypeSymbol.Name}Of{TypeSymbol.TypeArguments.Count()}";
				} else {
					return TypeSymbol.Name;
				}
			}
		}

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
				var fileNamePart = ClassName.ToCamelCase();
				//if (TypeSymbol.TypeArguments.Any()) {
				//	// fileNameCore += $"_of_{string.Join('_', TypeSymbol.TypeArguments.Select(ta => ta.Name))}";
				//	fileNameCore += $"Of{TypeSymbol.TypeArguments.Count()}";
				//}
				if (string.IsNullOrEmpty(Module)) { return $"{fileNamePart}.d"; }
				return $"{Module}.{fileNamePart}.d";
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
			var hasModule = !string.IsNullOrEmpty(Module);
			// bool shouldGenerateKeysAndNames = true;
			if (Options.KeysAndNames) {
				sb.AppendLine();

				#region names
				sb.AppendLine("//// If you need some kind of 'nameof':");
				//Name of the type
				string constName;
				if (hasModule) {
					var splitted = Module.Split('.').ToList();
					var last = splitted.Last();
					splitted.RemoveAt(splitted.Count - 1);
					sb.AppendLine($"declare namespace $wt.names{(splitted.Any() ? "." + string.Join('.', splitted) : "")} {{");
					constName = $"${last}";
				} else {
					sb.AppendLine($"declare namespace $wt {{");
					constName = "$names";
				}
				sb.AppendLine(1, $@"const enum {constName} {{ {ClassName} = ""{ClassName}"" }}");
				sb.AppendLine("}");

				//Members names
				sb.AppendLine();

				sb.AppendLine($"declare namespace $wt.names{(hasModule ? ("." + Module) : "")} {{");
				sb.AppendLine(1, $@"const enum ${ClassName} {{");
				var currentTypeSymbol = TypeSymbol;
				var members = new List<ISymbol>();
				do {
					members.AddRange(currentTypeSymbol.GetMembers());
					currentTypeSymbol = currentTypeSymbol.BaseType;
				} while (currentTypeSymbol != null);
				foreach (var m in members) {
					if (m.Kind != SymbolKind.Field && m.Kind != SymbolKind.Property) {
						continue;
					}
					if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
					var name = m.Name;
					if (!Options.KeepPropsCase && !((m as IFieldSymbol)?.IsConst).GetValueOrDefault()) {
						name = name.ToCamelCase();
					}
					sb.AppendLine(2, $@"{name} = ""{name}"",");
				}
				sb.AppendLine(1, "}");
				sb.AppendLine("}");
				#endregion

				#region keys
				sb.AppendLine("//// If you need type lookup");
				string interfaceName;
				if (hasModule) {
					var splitted = Module.Split('.').ToList();
					var last = splitted.Last();
					splitted.RemoveAt(splitted.Count - 1);
					sb.AppendLine($"declare namespace $wt.types{(splitted.Any() ? "." + string.Join('.', splitted) : "")} {{");
					interfaceName = $"{last}";
				} else {
					sb.AppendLine($"declare namespace $wt {{");
					interfaceName = "types";
				}
				sb.AppendLine(1, $@"interface {interfaceName} {{ {ClassName} }}");
				sb.AppendLine("}");
				#endregion
			}

		}

		//public abstract Task<string> SaveAsync();
		public abstract (string file, string content)? GenerateOutput();
	}
}
