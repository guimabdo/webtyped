using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;

namespace WebTyped {
	public class ParameterResolution {
		public string Name { get; private set; }
		public string Signature { get; private set; }
		public string RelayFormat { get; private set; }
		public bool FromBody { get; private set; }
		public bool Ignore { get; private set; }
		public ParameterResolution(IParameterSymbol parameterSymbol, TypeResolver typeResolver, ResolutionContext context) {
			var res = typeResolver.Resolve(parameterSymbol.Type, context);
			this.Name = parameterSymbol.Name;
			this.RelayFormat = this.Name;
			var p = parameterSymbol;
			var hasNamedTupleAttr = p.GetAttributes().Any(a => a.AttributeClass.Name == nameof(NamedTupleAttribute));
			var hasMapFunc = !string.IsNullOrWhiteSpace(res.MapAltToOriginalFunc);
			string unsupportedNamedTupleMessage = "[UNSUPPORTED - NamedTupleAttribute must be used only for tuple parameters]";
			string typeName = res.Name;
			
			if (hasNamedTupleAttr) {
				if (!hasMapFunc) {
					this.RelayFormat = unsupportedNamedTupleMessage;
					typeName = unsupportedNamedTupleMessage;
				} else {
					this.RelayFormat = $"{res.MapAltToOriginalFunc}({this.Name})";
					typeName = res.AltName;
				}
			}
			this.Signature = $"{p.Name}{(p.IsOptional ? "?" : "")}: {typeName}" + (res.IsNullable ? " | null" : "");
			this.Ignore = p.GetAttributes().Any(a => a.AttributeClass.Name == "FromServices");
		}
		
	}

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
		public string FullName {
			get {
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
			Module = options.AdjustModule(Module);
			FilenameWithoutExtenstion = $"{ControllerName.ToCamelCase()}.service";
			Options = options;
			//TypeResolver.Add(this);
		}

		public (string file, string content) GenerateOutput() {
			var sb = new StringBuilder();
			var context = new ResolutionContext();
			switch (Options.ServiceMode) {
				case ServiceMode.Jquery:
					sb.AppendLine("import { WebTypedClient } from '@guimabdo/webtyped-jquery';");
					break;
				case ServiceMode.Fetch:
					sb.AppendLine("import { WebTypedClient } from '@guimabdo/webtyped-fetch';");
					break;
				case ServiceMode.Angular:
				default:
					sb.AppendLine("import { Injectable, Inject, forwardRef, Optional } from '@angular/core';");
					sb.AppendLine("import { HttpClient } from '@angular/common/http';");
					sb.AppendLine("import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';");
					sb.AppendLine("import { Observable } from 'rxjs';");
					break;
			}

			var routeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");

			var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
			var path = arg.Replace("[controller]", ControllerName.ToCamelCase());
			int level = 0;
			switch (Options.ServiceMode) {
				case ServiceMode.Angular:
					sb.AppendLine(level, "@Injectable()");
					break;
			}
			sb.AppendLine(level, $"export class {ClassName} extends WebTypedClient {{");
			switch (Options.ServiceMode) {
				case ServiceMode.Angular:
					sb.AppendLine(level, $"	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {{");
					sb.AppendLine(level, $@"		super(baseUrl, ""{path}"", httpClient, eventEmitter);");
					break;
				case ServiceMode.Fetch:
				case ServiceMode.Jquery:
					sb.AppendLine(level, $@"	constructor(baseUrl: string = WebTypedClient.baseUrl) {{");
					sb.AppendLine(level, $@"		super(baseUrl, ""{path}"");");
					break;
			}
			sb.AppendLine(level, "	}");
			var existingMethods = new Dictionary<string, int>();
			foreach (var m in TypeSymbol.GetMembers()) {
				if (m.Kind == SymbolKind.NamedType) {
					//subClasses.Add(m as INamedTypeSymbol);
					continue;
				}
				if(m.DeclaredAccessibility != Accessibility.Public) { continue; }
				if (m.IsStatic) { continue; }
				if (m.Kind != SymbolKind.Method) { continue; }
				if (m.IsImplicitlyDeclared) { continue; }
				if (!m.IsDefinition) { continue; }
				var mtd = m as IMethodSymbol;
				if(m.Name == ".ctor") { continue; }
				var returnType = TypeResolver.Resolve(mtd.ReturnType as INamedTypeSymbol, context).Name;
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
				var methodName = mtd.Name.ToCamelCase();
				if (existingMethods.ContainsKey(methodName)) {
					existingMethods[methodName]++;
					methodName = $"{methodName}_{existingMethods[methodName]}";
				} else {
					existingMethods.Add(methodName, 0);
				}
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
					.Replace("[action]", methodName)
					.Replace("{", "${");

				//Resolve how parameters are sent
				//var pendingParameters = new List<string>();
				var pendingParameters = new List<ParameterResolution>();
				var parameterResolutions = mtd.Parameters.Select(p => new ParameterResolution(p, TypeResolver, context)).Where(p => !p.Ignore);
				var bodyParam = "null";
				foreach(var pr in parameterResolutions) {
					//[FromRoute]
					if (action.Contains($"{{{pr.Name}}}")) {
						continue;
					}

					//[FromBody]
					if (pr.FromBody) {
						bodyParam = pr.RelayFormat;
						continue;
					}
					pendingParameters.Add(pr);
				}
				var strParameters = string.Join(", ", parameterResolutions.Select(pr => pr.Signature));

				//if(pendingParameters.Any() && bodyParam == "null" && new string[] {
				//	"Put", "Patch", "Post"
				//}.Contains(httpMethod)) {
				//	bodyParam = pendingParameters[0];
				//	pendingParameters.RemoveAt(0);
				//}
				string genericReturnType;
				switch (Options.ServiceMode) {
					case ServiceMode.Jquery: genericReturnType = "JQuery.jqXHR"; break;
					case ServiceMode.Fetch: genericReturnType = "Promise"; break;
					case ServiceMode.Angular:default: genericReturnType = "Observable"; break;
				}
				sb.AppendLine(level + 1, $"{methodName} = ({strParameters}) : {genericReturnType}<{returnType}> => {{");
				sb.AppendLine(level + 2, $"return this.invoke{httpMethod}<{returnType}>({{");
				sb.AppendLine(level + 4, $"func: this.{methodName},");
				sb.AppendLine(level + 4, $"parameters: {{ {string.Join(", ", parameterResolutions.Select(p => p.Name))} }}");
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
				var search = pendingParameters.Any() ? $"{{ {string.Join(", ", pendingParameters.Select(pr => pr.RelayFormat))} }}" : "undefined";
				sb.AppendLine(level + 3, $"{search}");
				sb.AppendLine(level + 2, ");");
				sb.AppendLine(level + 1, "};");
			}
			sb.AppendLine(level, "}");
			sb.Insert(0, context.GetImportsText());
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
				dir = Path.Combine(Options.OutDir, Module.ToCamelCase());
				Directory.CreateDirectory(dir);
			}
			var file = Path.Combine(dir, Filename);
			string content = sb.ToString();
			return (file, content);
			//await FileHelper.WriteAsync(file, content);
			//return file;
			//File.WriteAllText(Path.Combine(Options.ServicesDir, Filename), sb.ToString());
		}

		public static bool IsService(INamedTypeSymbol t) {
			if (t.SpecialType == SpecialType.System_Enum) { return false; }
			if (t.ContainingType != null) { return false; }
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
