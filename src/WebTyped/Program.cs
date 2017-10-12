using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebTyped {
	public class Program {
		public static int Main(params string[] args) {
			var cmd = new CommandLineApplication();

			cmd.Command("create", target => {
				var controllers = target.Option("-c | --controllers", "Controllers path (glob supported)", CommandOptionType.SingleValue);
				var models = target.Option("-m | --models", "Models path (glob supported)", CommandOptionType.MultipleValue);
				var output = target.Option("-o | --output", "", CommandOptionType.SingleValue);
				target.OnExecute(() => {
					var matcher = new Matcher();
					if (controllers.HasValue()) {
						matcher.AddInclude(controllers.Value());
					}
					foreach (var val in models.Values) {
						matcher.AddInclude(val);
					}
					var csFiles = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory());

					var trees = csFiles
							.Select(r => CSharpSyntaxTree.ParseText(File.ReadAllText(r)))
							.ToList();

					//References
					var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

					var compilation = CSharpCompilation.Create("Comp", trees, new[] { mscorlib });
					var semanticModels = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));

					foreach (var t in trees) {
						foreach (var @class in t.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()) {
							var sm = semanticModels[t];

							var dsClass = sm.GetDeclaredSymbol(@class);

							if (dsClass.ContainingType != null) { continue; }
							if (!dsClass.Name.EndsWith("Controller")) {
								continue;
							}

							if (!CheckServicePrerequisites(dsClass)) { continue; }

							var controller = dsClass.Name.Substring(0, dsClass.Name.Length - "Controller".Length);
							var camelController = controller[0].ToString().ToLower() + controller.Substring(1);
							var sb = new StringBuilder();
							sb.AppendLine("import { Injectable, Inject, forwardRef } from '@angular/core';");
							sb.AppendLine("import { Http } from '@angular/http';");
							sb.AppendLine("import { WebApiClient, WebApiEventEmmiterService, WebApiObservable } from '@guimabdo/webtyped-angular';");
							sb.AppendLine(CreateServiceCode(dsClass));

							//if (dsClass.Name.EndsWith("Controller")) {
							//	str = str.Replace("__Controller__", controller);
							//}
							//
							//var methods = new StringBuilder();
							//var subClasses = new StringBuilder();
							//foreach (var m in dsClass.GetMembers()) {
							//	if (m.Kind == SymbolKind.NamedType) {
							//		subClasses.AppendLine(CreateModelCode(m as INamedTypeSymbol));
							//		continue;
							//	}
							//	if (m.Kind != SymbolKind.Method) { continue; }
							//	if (m.IsImplicitlyDeclared) { continue; }
							//	if (!m.IsDefinition) { continue; }
							//	methods.AppendLine($"\t{m.Name} = () => null;");
							//}
							//str = str.Replace("__Methods__", methods.ToString());
							//if (subClasses.Length > 0) {
							//	str += $"export module {controller}Service {{";
							//	str += subClasses.ToString();
							//	str += "}}";
							//
							//}

							Directory.CreateDirectory(Path.GetDirectoryName(output.Value()));
							File.WriteAllText(Path.Combine(output.Value(), $"{camelController}.service.ts"), sb.ToString());
						}
					}




					return 0;
				});
			});
			return cmd.Execute(args);
		}

		
		static bool CheckServicePrerequisites(INamedTypeSymbol t) {
			var routeAttr = t.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name == "RoutePrefix");
			if(routeAttr == null) { return false; }

			//Bug: https://github.com/dotnet/roslyn/issues/22501
			//ConstructorArguments value is always 0
			//so we'll get the syntax node
			var node = routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;
			if (node.ArgumentList.Arguments.Count < 1) { return false; }
			return true;
		}

		static string CreateServiceCode(INamedTypeSymbol t, int level = 0) {
			var subClasses = new List<INamedTypeSymbol>();
			var controller = t.Name.Substring(0, t.Name.Length - "Controller".Length);
			var routeAttr = t.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");

			var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
			var path = arg.Replace("[controller]", controller);

			var sb = new StringBuilder();
			sb.AppendLine(level, "@Injectable()");
			sb.AppendLine(level, $"export class {controller}Service extends WebApiClient {{");
			sb.AppendLine(level, $"	constructor(http: Http, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {{");
			sb.AppendLine(level, $@"		super(""{path}"", http, eventEmmiter);");
			sb.AppendLine(level, $"	}}");

			foreach (var m in t.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if (m.Kind != SymbolKind.Method) { continue; }
				if (m.IsImplicitlyDeclared) { continue; }
				if (!m.IsDefinition) { continue; }
				var mtd = m as IMethodSymbol;
				var returnType = TranslateType(mtd.ReturnType as INamedTypeSymbol);
				var mtdAttrs = mtd.GetAttributes();
				//Not marked actions will accept posts
				var httpMethod = "Post";
				string action = "";
				string search = "undefined";

				var httpAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Http"));
				var routeMethodAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Route"));
				//If has Http attribute
				if(httpAttr != null) {
					httpMethod = httpAttr.AttributeClass.Name.Substring(4);
					//Check if it contains route info
					var args = (httpAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments;
					if (args.Count > 0) {
						action = args[0].ToString().Replace("\"", "");
					}
				}
				//Check if contains route attr
				if(routeMethodAttr != null) {
					var args = (routeMethodAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments;
					if (args.Count > 0) {
						action = args[0].ToString().Replace("\"", "");
					}
				}
				//Replace route variables
				action = action.Replace("[action]", m.Name);
				sb.AppendLine(level + 1, $"{m.Name} = () : WebApiObservable<{returnType}> => {{ return this.invoke{httpMethod}<{returnType}>({{ func: this.{m.Name}, parameters: {{}} }}, `{action}`, {search}); }};");
			}
			sb.AppendLine($"}}");
			if (subClasses.Any()) {
				sb.AppendLine(level, $"export module {controller}Service {{");
				subClasses.ForEach(s => sb.AppendLine(CreateModelCode(s, level + 1)));
				sb.AppendLine(level, $"}}");
			}
			return sb.ToString();
			//			@Injectable()
			//export class __Controller__Service extends WebApiClient {
			//    constructor(http: Http,
			//        @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
			//        super("", http, eventEmmiter);
			//    }
			//    __Methods__
			//}

		}
		static string CreateModelCode(INamedTypeSymbol t, int level = 0) {
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();
			sb.AppendLine(level, $"export class {t.Name} {{");
			foreach (var m in t.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if (m.Kind != SymbolKind.Property) { continue; }
				if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
				var prop = m as IPropertySymbol;
				sb.AppendLine(level + 1, $"{m.Name}: {TranslateType(prop.Type as INamedTypeSymbol)};");
			}
			sb.AppendLine(level, $"}}");
			if (subClasses.Any()) {
				sb.AppendLine(level, $"export module {t.Name} {{");
				subClasses.ForEach(s => sb.AppendLine(CreateModelCode(s, level + 1)));
				sb.AppendLine(level, $"}}");
			}
			return sb.ToString();
		}

		static string TranslateType(INamedTypeSymbol type) {
			string name = type.Name;
			var members = type.GetTypeMembers();
			string typeName = type.Name;
			//In case of nested types
			var parent = type.ContainingType;
			while (parent != null) {
				//Check if parent type is controller > service
				var parentName = parent.Name;
				//Adjust to check prerequisites
				//if(CheckServicePrerequisites(parent)) {	}
				//For now, we'll just check if ends with "Controller" suffix
				if (parentName.EndsWith("Controller")) {
					parentName = parentName.Replace("Controller", "Service");
				}
				
				typeName = $"{parentName}.{typeName}";
				parent = parent.ContainingType;
			}
			
			string genericPart = "";
			//Generic
			if (type.IsGenericType) {
				genericPart = $"<{string.Join(", ", type.TypeArguments.Select(t => TranslateType(t as INamedTypeSymbol)))}>";
			}

			//Change type to ts type
			typeName = ToTsTypeName(typeName);
			
			return $"{typeName}{genericPart}";
		}

		static string ToTsTypeName(string typeName) {
			switch (typeName) {
				case nameof(String):
					return "string";
				case nameof(Int32):
				case nameof(Int16):
				case nameof(Int64):
					return "number";
				case nameof(IEnumerable):
					return "Array";
				default: return typeName;
			}
		}

		//static string GetResourceString(string resource) {
		//	var assembly = typeof(Program).Assembly;
		//	var resourceStream = assembly.GetManifestResourceStream($"WebTyped.Resources.{resource}");

		//	using (var reader = new StreamReader(resourceStream, Encoding.UTF8)) {
		//		return reader.ReadToEnd();
		//	}
		//}
	}

	public static class Extensions {
		public static StringBuilder AppendLine(this StringBuilder sb, int level, string text) {
			return sb
				.Append('\t', level)
				.AppendLine(text);
		}

		public static string ToCamelCase(this string str) {
			return str[0].ToString().ToLower() + str.Substring(1);
		}
	}
}
