using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;
using WebTyped.Elements;

namespace WebTyped {
	public class Model : TsModelBase {
		public Model(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) : base(modelType, typeResolver, options) {}

	

		public override (string file, string content)? GenerateOutput() {
			//If external
			if(this.ExternalType != null) { return null; }

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
			var context = new ResolutionContext();
			if (TypeSymbol.BaseType != null && TypeSymbol.BaseType.SpecialType != SpecialType.System_Object) {
				var inheritanceTypeResolution = TypeResolver.Resolve(TypeSymbol.BaseType, context);
				if (inheritanceTypeResolution.IsAny) {
					if (inheritanceTypeResolution.Name.Contains("/*")) {
						inheritance = $"/*extends {inheritanceTypeResolution.Name.Replace("any/*", "").Replace("*/", "")}*/";
					}
					
				} else {
					inheritance = $"extends {inheritanceTypeResolution.Name} ";
				}
			}

			string genericArguments = "";
			if (TypeSymbol.TypeArguments.Any()) {
				genericArguments = $"<{string.Join(", ", TypeSymbol.TypeArguments.Select(t => t.Name))}>";
			}

			// sb.AppendLine(level, $"{(hasModule ? "" : "declare ")}interface {TypeSymbol.Name}{genericArguments} {inheritance}{{");
			sb.AppendLine(level, $"{(hasModule ? "" : "declare ")}interface {ClassName}{genericArguments} {inheritance}{{");
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
				sb.AppendLine(level + 1, $"{name}{(isNullable ? "?":"")}: {TypeResolver.Resolve(prop.Type, context).Name};");
			}
			sb.AppendLine(level, "}");
			if (!string.IsNullOrEmpty(Module)) {
				sb.AppendLine("}");
			}
			sb.Insert(0, context.GetImportsText());
			//File.WriteAllText(Path.Combine(Options.ModelsDir, Filename), sb.ToString());
			string file = Path.Combine(Options.TypingsDir, Filename);
			string content = sb.ToString();
			return (file, content);
			//await FileHelper.WriteAsync(file, content);
			//return file;
		}

		public static bool IsModel(INamedTypeSymbol t) {
			if(t.BaseType?.SpecialType == SpecialType.System_Enum) { return false; }
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}
	}
}
