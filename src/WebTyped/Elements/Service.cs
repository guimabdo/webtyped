using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTyped {
	public class Service : ITsFile {
		public string Module { get; private set; }
		string Filename { get { return $"{FilenameWithoutExtenstion}.ts"; } }
		public string FilenameWithoutExtenstion { get; private set; }
		public string FilePathWithoutExtension {
			get {
				return Path.Combine(Module, FilenameWithoutExtenstion);
			}
		}
		public string ControllerName { get; private set; }
		public string ClassName { get { return $"{ControllerName}Service"; } }
		public string FullName { get {
				if (string.IsNullOrEmpty(Module)) { return ClassName; }
				return $"{Module}.{ClassName}";
			}
		}
		public INamedTypeSymbol TypeSymbol { get; private set; }
		public TypeResolver TypeResolver { get; private set; }
		public Options Options { get; private set; }
		public Service(INamedTypeSymbol controllerType, TypeResolver typeResolver, Options options) {
			TypeSymbol = controllerType;
			ControllerName = controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length);
			TypeResolver = typeResolver;
			Module = controllerType.ContainingNamespace.ToString().Replace(".Controllers", ".Services");
			Module = options.TrimModule(Module);
			FilenameWithoutExtenstion = $"{ControllerName.ToCamelCase()}.service";
			Options = options;
			TypeResolver.Add(this);
		}

		public async Task<string> SaveAsync() {
			var sb = new StringBuilder();
			sb.AppendLine("import { Injectable, Inject, forwardRef } from '@angular/core';");
			sb.AppendLine("import { HttpClient } from '@angular/common/http';");
			sb.AppendLine("import { WebApiClient, WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';");
			sb.AppendLine("import { Observable } from 'rxjs';");

			var routeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");

			var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
			var path = arg.Replace("[controller]", ControllerName);
			int level = 0;
			//if (!string.IsNullOrEmpty(Module)) {
			//	sb.AppendLine($"export module {Module} {{");
			//	level++;
			//}
			sb.AppendLine(level, "@Injectable()");
			sb.AppendLine(level, $"export class {ClassName} extends WebApiClient {{");
			sb.AppendLine(level, $"	constructor(@Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {{");
			sb.AppendLine(level, $@"		super(baseUrl, ""{path}"", httpClient, eventEmmiter);");
			sb.AppendLine(level, "	}");

			foreach (var m in TypeSymbol.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					//subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if (m.Kind != SymbolKind.Method) { continue; }
				if (m.IsImplicitlyDeclared) { continue; }
				if (!m.IsDefinition) { continue; }
				var mtd = m as IMethodSymbol;
				var returnType = TypeResolver.Resolve(mtd.ReturnType as INamedTypeSymbol);
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
					mtd.Parameters.Select(p => $"{p.Name}: {TypeResolver.Resolve(p.Type as INamedTypeSymbol)}")
				);

				//Resolve how parameters are sent
				var pendingParameters = new List<string>();
				var bodyParam = "null";
				foreach (var p in mtd.Parameters) {
					//[FromRoute]
					if (action.Contains($"{{{p.Name}}}")) {
						continue;
					}
					//[FromBody]
					if (p.GetAttributes().Any(a => a.AttributeClass.Name == "FromBody")) {
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
					case "Put":
					case "Patch":
					case "Post":
						sb.AppendLine(level + 3, $"{bodyParam},");
						break;
					default: break;
				}
				var search = pendingParameters.Any() ? $"{{ {string.Join(", ", pendingParameters)} }}" : "undefined";
				sb.AppendLine(level + 3, $"{search}");
				sb.AppendLine(level + 2, ");");
				sb.AppendLine(level + 1, "};");
			}
			sb.AppendLine(level, "}");
			//if (!string.IsNullOrEmpty(Module)) {
			//	level--;
			//	sb.AppendLine(level, "}");
			//}

			//if (subClasses.Any()) {
			//	sb.AppendLine(level, $"export module {controller}Service {{");
			//	subClasses.ForEach(s => sb.AppendLine(CreateModelCode(s, level + 1)));
			//	sb.AppendLine(level, $"}}");
			//}
			var dir = Options.OutDir;
			if (!string.IsNullOrEmpty(Module)) {
				dir = Path.Combine(Options.OutDir, Module);
				Directory.CreateDirectory(dir);
			}
			var file = Path.Combine(dir, Filename);
			string content = sb.ToString();
			await FileHelper.WriteAsync(file, content);
			return file;
			//File.WriteAllText(Path.Combine(Options.ServicesDir, Filename), sb.ToString());
		}

		public static bool CanBeService(INamedTypeSymbol t) {
			if(t.ContainingType != null) { return false; }
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
	}
}
