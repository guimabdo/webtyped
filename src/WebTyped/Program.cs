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
			var cmd = new CommandLineApplication() {
				Name = "WebTyped",
				Description = "Generate typescript services and model definitions based on your webApis"
			};
			cmd.VersionOption("--version", "0.1.0");
			cmd.HelpOption("-?|-h|--help");

			cmd.Command("generate", target => {
				//var controllers = target.Option("-c | --controllers", "Controllers path (glob supported)", CommandOptionType.SingleValue);
				//var models = target.Option("-m | --models", "Models path (glob supported)", CommandOptionType.MultipleValue);
				var sourceFiles = target.Option("-sf | --sourceFiles", "C# source files (glob supported)", CommandOptionType.MultipleValue);
				var outDir = target.Option("-od | --outDir", "", CommandOptionType.SingleValue);
				target.HelpOption("-?|-h|--help");
				target.OnExecute(() => {
					var matcher = new Matcher();
					//if (controllers.HasValue()) {
					//	matcher.AddInclude(controllers.Value());
					//}
					//foreach (var val in models.Values) {
					//	matcher.AddInclude(val);
					//}
					foreach (var val in sourceFiles.Values) {
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

					var servicesDir = Path.Combine(outDir.Value(), "services");
					var modelsDir = Path.Combine(outDir.Value(), "models");
					Directory.CreateDirectory(servicesDir);
					Directory.CreateDirectory(modelsDir);
					var sbServicesIndex = new StringBuilder();
					sbServicesIndex.AppendLine("import * as services from './';");

					
					var serviceNames = new List<string>();
					foreach (var t in trees) {
						foreach (var @class in t.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()) {
							var sm = semanticModels[t];

							var dsClass = sm.GetDeclaredSymbol(@class);
							//Only root types, subtypes are handled with its roots
							if (dsClass.ContainingType != null) { continue; }
							if (CheckServicePrerequisites(dsClass)) {
								var controller = dsClass.Name.Substring(0, dsClass.Name.Length - "Controller".Length);
								serviceNames.Add($"{controller}Service");
								var sb = new StringBuilder();
								sb.AppendLine("import { Injectable, Inject, forwardRef } from '@angular/core';");
								sb.AppendLine("import { HttpClient } from '@angular/common/http';");
								sb.AppendLine("import { WebApiClient, WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';");
								sb.AppendLine("import { Observable } from 'rxjs'");
								sb.AppendLine(CreateServiceCode(dsClass));

								var svcFileName = $"{controller.ToCamelCase()}.service";
								File.WriteAllText(Path.Combine(servicesDir, $"{svcFileName}.ts"), sb.ToString());
								sbServicesIndex.AppendLine($"export * from './{svcFileName}';");

								//Nested types
								var nestedTypes = dsClass.GetMembers().OfType<INamedTypeSymbol>();
								if (nestedTypes.Any()) {
									sb = new StringBuilder();
									sb.AppendLine($"declare module {controller}Service {{");
									nestedTypes.ToList().ForEach(s => sb.AppendLine(CreateModelCode(s, 1)));
									sb.AppendLine("}");
									File.WriteAllText(Path.Combine(modelsDir, $"{controller.ToCamelCase()}.service.d.ts"), sb.ToString());
								}
							} else if (CheckModelPrerequisites(dsClass)) {
								File.WriteAllText(Path.Combine(modelsDir, $"{dsClass.Name.ToCamelCase()}.d.ts"), CreateServiceCode(dsClass));
							}
						}
					}
					sbServicesIndex.AppendLine("export var all = [");
					sbServicesIndex.AppendLine(string.Join("," + System.Environment.NewLine, serviceNames.Select(s => $"services.{s}")));
					sbServicesIndex.AppendLine("];");

					File.WriteAllText(Path.Combine(servicesDir, $"index.ts"), sbServicesIndex.ToString());

					var sbMainIndex = new StringBuilder();
					sbMainIndex.AppendLine("import * as services from './services';");
					sbMainIndex.AppendLine("import { WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';");
					sbMainIndex.AppendLine("export var providers = [");
					sbMainIndex.AppendLine(1, "WebApiEventEmmiterService,");
					sbMainIndex.AppendLine(1, "...services.all");
					sbMainIndex.AppendLine("];");
					File.WriteAllText(Path.Combine(outDir.Value(), $"index.ts"), sbMainIndex.ToString());


					return 0;
				});
			});

			cmd.OnExecute(() => {
				cmd.ShowHelp();
				return 0;
			});

			return cmd.Execute(args);
		}

		static bool CheckModelPrerequisites(INamedTypeSymbol t) {
			if (t.Name.EndsWith("Controller")) { return false; }
			if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
			return true;
		}

		static bool CheckServicePrerequisites(INamedTypeSymbol t) {
			if (!t.Name.EndsWith("Controller")) { return false; }
			var routeAttr = t.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name == "RoutePrefix");
			if (routeAttr == null) { return false; }

			//Bug: https://github.com/dotnet/roslyn/issues/22501
			//ConstructorArguments value is always 0
			//so we'll get the syntax node
			var node = routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;
			if (node.ArgumentList.Arguments.Count < 1) { return false; }
			return true;
		}

		static string CreateServiceCode(INamedTypeSymbol t, int level = 0) {
			//var subClasses = new List<INamedTypeSymbol>();
			var controller = t.Name.Substring(0, t.Name.Length - "Controller".Length);
			var routeAttr = t.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");

			var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
			var path = arg.Replace("[controller]", controller);

			var sb = new StringBuilder();
			sb.AppendLine(level, "@Injectable()");
			sb.AppendLine(level, $"export class {controller}Service extends WebApiClient {{");
			sb.AppendLine(level, $"	constructor(@Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {{");
			sb.AppendLine(level, $@"		super(baseUrl, ""{path}"", httpClient, eventEmmiter);");
			sb.AppendLine(level, $"	}}");

			foreach (var m in t.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					//subClasses.Add(m as INamedTypeSymbol);
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
				//string search = "undefined";

				//Get http method from method name pattern
				if (mtd.Name.StartsWith("Get")) { httpMethod = "Get"; }
				if (mtd.Name.StartsWith("Post")) { httpMethod = "Post"; }
				if (mtd.Name.StartsWith("Put")) { httpMethod = "Put"; }
				if (mtd.Name.StartsWith("Delete")) { httpMethod = "Delete"; }
				if (mtd.Name.StartsWith("Patch")) { httpMethod = "Patch"; }

				var httpAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Http"));
				var routeMethodAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Route"));
				//If has Http attribute
				if (httpAttr != null) {
					httpMethod = httpAttr.AttributeClass.Name.Substring(4);
					//Check if it contains route info
					var args = (httpAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax)?.ArgumentList?.Arguments;
					if (args != null && args.Value.Count > 0) {
						action = args.Value[0].ToString().Replace("\"", "");
					}
				}
				//Check if contains route attr
				if (routeMethodAttr != null) {
					var args = (routeMethodAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments;
					if (args.Count > 0) {
						action = args[0].ToString().Replace("\"", "");
					}
				}
				//Replace route variables
				action = action
					.Replace("[action]", m.Name)
					.Replace("{", "${");

				//Resolve parameters
				var strParameters = string.Join(", ",
					mtd.Parameters.Select(p => $"{p.Name}: {TranslateType(p.Type as INamedTypeSymbol)}")
				);

				//Resolve how parameters are sent
				var pendingParameters = new List<string>();
				var bodyParam = "null";
				foreach(var p in mtd.Parameters) {
					//[FromRoute]
					if (action.Contains($"{{{p.Name}}}")) {
						continue;
					}
					//[FromBody]
					if(p.GetAttributes().Any(a => a.AttributeClass.Name == "FromBody")) {
						bodyParam = p.Name;
						continue;
					}
					pendingParameters.Add(p.Name);
				}

				//if(pendingParameters.Any() && bodyParam == "null" && new string[] {
				//	"Put", "Patch", "Post"
				//}.Contains(httpMethod)) {
				//	bodyParam = pendingParameters[0];
				//	pendingParameters.RemoveAt(0);
				//}

				sb.AppendLine(level + 1, $"{m.Name} = ({strParameters}) : Observable<{returnType}> => {{");
				sb.AppendLine(level + 2, $"return this.invoke{httpMethod}<{returnType}>({{");
				sb.AppendLine(level + 4, $"func: this.{m.Name},");
				sb.AppendLine(level + 4, $"parameters: {{ {string.Join(", ", mtd.Parameters.Select(p => p.Name))} }}");
				sb.AppendLine(level + 3, "},");
				sb.AppendLine(level + 3, $"`{action}`,");
				//Body
				switch (httpMethod) {
					case "Put": case "Patch": case "Post":
						sb.AppendLine(level + 3, $"{bodyParam},");
						break;
					default: break;
				}
				var search = pendingParameters.Any() ? $"{{ {string.Join(", ", pendingParameters)} }}" : "undefined";
				sb.AppendLine(level + 3, $"{search}");
				sb.AppendLine(level + 2, ");");
				sb.AppendLine(level + 1, "};");
			}
			sb.AppendLine($"}}");
			//if (subClasses.Any()) {
			//	sb.AppendLine(level, $"export module {controller}Service {{");
			//	subClasses.ForEach(s => sb.AppendLine(CreateModelCode(s, level + 1)));
			//	sb.AppendLine(level, $"}}");
			//}
			return sb.ToString();
		}
		static string CreateModelCode(INamedTypeSymbol t, int level = 0) {
			var subClasses = new List<INamedTypeSymbol>();
			var sb = new StringBuilder();
			sb.AppendLine(level, $"export interface {t.Name} {{");
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
			//tuples
			if (type.IsTupleType) {
				return $"{{{string.Join(", ", type.TupleElements.Select(te => $"{te.Name}: {TranslateType(te.Type as INamedTypeSymbol)}"))}}}";
			}

			string name = type.Name;
			var members = type.GetTypeMembers();
			string typeName = type.Name;
			//In case of nested types
			var parent = type.ContainingType;
			while (parent != null) {
				//Check if parent type is controller > service
				var parentName = parent.Name;
				//Adjust to check prerequisites
				if (CheckServicePrerequisites(parent)) {
					parentName = parentName.Replace("Controller", "Service");
				}
				//For now, we'll just check if ends with "Controller" suffix
				//if (parentName.EndsWith("Controller")) {
				//	parentName = parentName.Replace("Controller", "Service");
				//}

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
				case nameof(List<object>):
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
