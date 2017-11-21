using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped {
	public class Model : ITsFile {
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
				if (string.IsNullOrEmpty(Module)) { return TypeSymbol.Name.ToCamelCase(); }
				return $"{Module}.{TypeSymbol.Name.ToCamelCase()}.d";
			}
		}

		string Filename {
			get {
				return $"{FilenameWithoutExtenstion}.ts";
			}
		}

		public Model(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) {
			TypeSymbol = modelType;
			TypeResolver = typeResolver;
			Options = options;
			TypeResolver.Add(this);
		}

		public string Save() {
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();

			var level = 0;
			if (!string.IsNullOrEmpty(Module)) {
				sb.AppendLine($"declare module {Module} {{");
				level++;
			}

			sb.AppendLine(level, $"declare interface {TypeSymbol.Name} {{");
			foreach (var m in TypeSymbol.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if (m.Kind != SymbolKind.Property) { continue; }
				if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
				var prop = m as IPropertySymbol;
				sb.AppendLine(level + 1, $"{m.Name}: {TypeResolver.Resolve(prop.Type)};");
			}
			sb.AppendLine(level, "}");
			if (!string.IsNullOrEmpty(Module)) {
				sb.AppendLine("}");
			}

			//File.WriteAllText(Path.Combine(Options.ModelsDir, Filename), sb.ToString());
			string file = Path.Combine(Options.TypingsDir, Filename);
			string content = sb.ToString();
			if (!File.Exists(file) || File.ReadAllText(file) != content) {
				File.WriteAllText(file, content);
			}
			return file;
		}

		public static bool CanBeModel(INamedTypeSymbol t) {
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}
	}
}
