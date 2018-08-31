﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace WebTyped.Elements
{
	public class TsEnum : TsModelBase {
		public TsEnum(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) : base(modelType, typeResolver, options) {}

		public override (string file, string content)? GenerateOutput() {
			//If external
			if (this.ExternalType != null) { return null; }
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();

			var level = 0;
			var hasModule = false;
			if (!string.IsNullOrEmpty(Module)) {
				hasModule = true;
				sb.AppendLine($"declare module {Module} {{");
				level++;
			}

			sb.AppendLine(level, $"{(hasModule ? "" : "declare ")}const enum {TypeSymbol.Name} {{");
			foreach (var m in TypeSymbol.GetMembers()) {
				if (m.Kind == SymbolKind.Field) {
					var f = m as IFieldSymbol;
					sb.AppendLine(level + 1, $"{f.Name} = {f.ConstantValue},");
				}
			}
			sb.AppendLine(level, "}");
			if (!string.IsNullOrEmpty(Module)) {
				sb.AppendLine("}");
			}

			//$wt.names and $wt.keys... Make this optional? 
			bool shouldGenerateKeysAndNames = true;
			if (shouldGenerateKeysAndNames) {
				sb.AppendLine();

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


			//File.WriteAllText(Path.Combine(Options.ModelsDir, Filename), sb.ToString());
			string file = Path.Combine(Options.TypingsDir, Filename);
			string content = sb.ToString();
			return (file, content);
			//await FileHelper.WriteAsync(file, content);
			//return file;
		}

		public static bool IsEnum(ITypeSymbol t) {
			if (t.BaseType?.SpecialType != SpecialType.System_Enum) { return false; }
			return true;
		}
	}
}
