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
		private (string file, string content) GenerateOutputModule() {
			var hasConsts = false;
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();
			var context = new ResolutionContext(this);
			var level = 0;
			if (!TypeSymbol.IsStatic) {

				string inheritance = "";

				if (TypeSymbol.BaseType != null && TypeSymbol.BaseType.SpecialType != SpecialType.System_Object) {
					var inheritanceTypeResolution = TypeResolver.Resolve(TypeSymbol.BaseType, context);
					if (inheritanceTypeResolution.IsAny) {
						if (inheritanceTypeResolution.Name.Contains("/*")) {
							inheritance = $"/*extends {inheritanceTypeResolution.Name.Replace("any/*", "").Replace("*/", "")}*/";
						}

					}
					else {
						inheritance = $"extends {inheritanceTypeResolution.Name} ";
					}
				}

				string genericArguments = "";
				if (TypeSymbol.TypeArguments.Any()) {
					genericArguments = $"<{string.Join(", ", TypeSymbol.TypeArguments.Select(t => t.Name))}>";
				}

				sb.AppendLine(level, $"export interface {ClassName}{genericArguments} {inheritance}{{");
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
					var isNullable = TypeResolver.IsNullable(prop.Type) || !prop.Type.IsValueType;
					var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();

                    var summary = prop.GetDocumentationCommentXml();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summary = summary.Split("<summary>").Last().Split("</summary>").First().Trim();
                        sb.AppendLine(level + 1, "/**");
                        var lines = summary.Split(System.Environment.NewLine);
                        foreach(var l in lines)
                        {
                            sb.AppendLine(level + 1,$"*{l.Trim()}");
                        }
                        sb.AppendLine(level + 1, "**/");
                    }

					sb.AppendLine(level + 1, $"{name}{(isNullable ? "?" : "")}: {TypeResolver.Resolve(prop.Type, context).Name};");
				}
				sb.AppendLine(level, "}");

				if (hasConsts) {
					sb.AppendLine();
				}
			}
			else {
				hasConsts = true;
			}

			//Static part
			if (hasConsts
                //removing const support for now
                //If we enable it again, we should merge these members with KeysAndNames...
                && false 
                ) {
				//https://github.com/Microsoft/TypeScript/issues/17372
				//Currently we can't merge namespaces and const enums.
				//This is supposed to be a bug.
				//Besides, even if this is fixed, I think enums will never be mergeable with interfaces,
				//so I think it will be always necessary to discriminate with something like $.
				//$ can't be used in C# classes, so will never happen a conflict.
				//We transpile C# Consts to TypeScript const enums because this is currently
				//the only way to inline values. Consts values direclty inside namespaces/modules are not inlined...
				//---R: Now, supporting only scoped types, maybe values dont need to be inlined...

				sb.AppendLine(level, $"export class {ClassName} {{");
				level++;
				foreach (var m in TypeSymbol.GetMembers()) {
					var fieldSymbol = (m as IFieldSymbol);
					if (fieldSymbol != null && fieldSymbol.IsConst) {
						//Consts names should not be changed, they are commonly uppercased both in client and server...
						var name = m.Name;
						sb.AppendLine(level, $"static readonly {name} = {JsonConvert.SerializeObject(fieldSymbol.ConstantValue)};");
					}
				}
				level--;
				sb.AppendLine(level, "}");
			}
			
						
			level--;

			AppendKeysAndNames(sb);

			sb.Insert(0, context.GetImportsText());
			string content = sb.ToString();
			return (OutputFilePath, content);
		}

		public override (string file, string content)? GenerateOutput() {
			//If external
			if (this.ExternalType != null) { return null; }
			//if(Options.TypingsScope == TypingsScope.Global) {
			//	return GenerateOutputGlobal();
			//}
			return GenerateOutputModule();
		}

		public static bool IsModel(INamedTypeSymbol t) {
			if (t.BaseType?.SpecialType == SpecialType.System_Enum) { return false; }
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}
	}
}
