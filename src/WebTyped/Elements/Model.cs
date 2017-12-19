using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Elements;

namespace WebTyped {
	public class Model : TsModelBase {
		public Model(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) : base(modelType, typeResolver, options) {}

		public override async Task<string> SaveAsync() {
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();

			var level = 0;
			var hasModule = false;
			if (!string.IsNullOrEmpty(Module)) {
				hasModule = true;
				sb.AppendLine($"declare module {Module} {{");
				level++;
			}
			string inheritance = "";
			if(TypeSymbol.BaseType != null && TypeSymbol.BaseType.SpecialType != SpecialType.System_Object) {
				inheritance = $"extends {TypeResolver.Resolve(TypeSymbol.BaseType)} ";
			}
			sb.AppendLine(level, $"{(hasModule ? "" : "declare ")}interface {TypeSymbol.Name} {inheritance}{{");
			foreach (var m in TypeSymbol.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if (m.Kind != SymbolKind.Property) { continue; }
				if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
				var prop = m as IPropertySymbol;
				var isNullable = TypeResolver.IsNullable(prop.Type);
				var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();
				sb.AppendLine(level + 1, $"{name}{(isNullable ? "?":"")}: {TypeResolver.Resolve(prop.Type)};");
			}
			sb.AppendLine(level, "}");
			if (!string.IsNullOrEmpty(Module)) {
				sb.AppendLine("}");
			}

			//File.WriteAllText(Path.Combine(Options.ModelsDir, Filename), sb.ToString());
			string file = Path.Combine(Options.TypingsDir, Filename);
			string content = sb.ToString();
			await FileHelper.WriteAsync(file, content);
			return file;
		}

		public static bool IsModel(INamedTypeSymbol t) {
			if(t.BaseType?.SpecialType == SpecialType.System_Enum) { return false; }
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}
	}
}
