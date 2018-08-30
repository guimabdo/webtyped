using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
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
		public Model(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) : base(modelType, typeResolver, options) { }



		public override (string file, string content)? GenerateOutput() {
			//If external
			if (this.ExternalType != null) { return null; }
			var hasConsts = false;
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();
			var hasModule = !string.IsNullOrEmpty(Module);
			var context = new ResolutionContext();
			var level = 0;
			if (hasModule) {
				sb.AppendLine($"declare module {Module} {{");
				level++;
			}
			if (!TypeSymbol.IsStatic) {
				
				string inheritance = "";

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
					if (m.IsStatic) {
						if (m.Kind == SymbolKind.Field && (m as IFieldSymbol).IsConst) {
							hasConsts = true;
						}
						continue;
					}
					if (m.Kind != SymbolKind.Property) { continue; }
					if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
					var prop = m as IPropertySymbol;
					var isNullable = TypeResolver.IsNullable(prop.Type);
					var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();
					sb.AppendLine(level + 1, $"{name}{(isNullable ? "?" : "")}: {TypeResolver.Resolve(prop.Type, context).Name};");
				}
				sb.AppendLine(level, "}");
				level--;
				if (hasConsts) {
					sb.AppendLine();
				}
			} else {
				hasConsts = true;
			}

			//Static part
			if (hasConsts) {
				//string modulePart = hasModule ? $"{Module}." : "";
				//https://github.com/Microsoft/TypeScript/issues/17372
				//Currently we can't merge namespaces and const enums.
				//This is supposed to be a bug.
				//Besides, even if this is fixed, I think enums will never be mergeable with interfaces,
				//so I think it will be always necessary to discriminate with something like $.
				//$ can't be used in C# classes, so will never happen a conflict.
				//We transpile C# Consts to TypeScript const enums because this is currently
				//the only way to inline values. Consts values direclty inside namespaces/modules are not inlined...
				sb.AppendLine(level, $"{(hasModule ? "" : "declare ")}const enum ${ClassName} {{");
				List<string> constants = new List<string>();
				foreach (var m in TypeSymbol.GetMembers()) {
					var fieldSymbol = (m as IFieldSymbol);
					if (fieldSymbol != null && fieldSymbol.IsConst) {
						//Consts names should not be changed, they are commonly uppercased both in client and server...
						// var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();
						var name = m.Name;
						constants.Add($"{name} = {JsonConvert.SerializeObject(fieldSymbol.ConstantValue)}");
						//sb.AppendLine(level + 1, $"{name} = {JsonConvert.SerializeObject(fieldSymbol.ConstantValue)};");
					}
				}
				
				sb.AppendLine(
					level + 1, 
					string.Join(
						$",{System.Environment.NewLine}{new StringBuilder().Append('\t', level + 1)}", 
						constants
					)
				);

				sb.AppendLine(level, "}");
			}

			if (hasModule) {
				sb.AppendLine("}");
			}

			sb.Insert(0, context.GetImportsText());
			string file = Path.Combine(Options.TypingsDir, Filename);
			string content = sb.ToString();
			return (file, content);
		}

		public static bool IsModel(INamedTypeSymbol t) {
			if (t.BaseType?.SpecialType == SpecialType.System_Enum) { return false; }
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}
	}
}
