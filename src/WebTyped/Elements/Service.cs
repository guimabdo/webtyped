using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebTyped.Abstractions;

namespace WebTyped
{
    public enum ParameterFromKind
    {
        None,
        FromRoute,
        FromQuery,
        FromUri,
        FromBody,
    }

    public class Service : ITsFile
    {

        public string Module { get; private set; }
        string Filename { get { return $"{FilenameWithoutExtenstion}.ts"; } }
        public string FilenameWithoutExtenstion { get; private set; }
        public string FilePathWithoutExtension {
            get {
                return Path.Combine(Module, FilenameWithoutExtenstion);
            }
        }
        public string ControllerName { get; private set; }
        public string ClassName { get { return $"{ControllerName}{Options.ServiceSuffix}"; } }
        public string FullName {
            get {
                if (string.IsNullOrEmpty(Module)) { return ClassName; }
                return $"{Module}.{ClassName}";
            }
        }
        public INamedTypeSymbol TypeSymbol { get; private set; }
        public TypeResolver TypeResolver { get; private set; }
        public Options Options { get; private set; }

        public string OutputFilePath {
            get {
                var dir = Options.OutDir;
                if (!string.IsNullOrEmpty(Module))
                {
                    var moduleCamel = string.Join('.', Module.Split('.').Select(s => s.ToCamelCase()));
                    dir = Path.Combine(Options.OutDir, moduleCamel);
                }
                return Path.Combine(dir, Filename);
            }
        }

        public string AbstractPath {
            get {
                var dir ="";
                if (!string.IsNullOrEmpty(Module))
                {
                    var moduleCamel = string.Join('.', Module.Split('.').Select(s => s.ToCamelCase()));
                    dir = moduleCamel;
                }
                return Path.Combine(dir, FilenameWithoutExtenstion);
            }
        }

        public Service(INamedTypeSymbol controllerType, TypeResolver typeResolver, Options options)
        {
            TypeSymbol = controllerType;
            ControllerName = controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length);
            TypeResolver = typeResolver;
            Module = "";
            if (!controllerType.ContainingNamespace.IsGlobalNamespace)
            {
                Module = controllerType.ContainingNamespace.ToString().Replace(".Controllers", ".Services");
            }
            Module = options.AdjustModule(Module);
            FilenameWithoutExtenstion = $"{ControllerName.ToCamelCase()}.service";
            Options = options;
        }

        public (string file, string content) GenerateOutput()
        {
            var sb = new StringBuilder();
            var context = new ResolutionContext(this);
            sb.AppendLine("import { WebTypedCallInfo, WebTypedFunction, WebTypedInvoker } from '@guimabdo/webtyped-common';");
            string genericReturnType = "Promise";
            if (Options.GenericReturnType != null)
            {
                if (!string.IsNullOrWhiteSpace(Options.GenericReturnType.Name)) { genericReturnType = Options.GenericReturnType.Name; }
                if (!string.IsNullOrWhiteSpace(Options.GenericReturnType.Module))
                {
                    sb.AppendLine($"import {{ {Options.GenericReturnType.Name} }} from '{Options.GenericReturnType.Module}';");
                }
            }

            var routeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");

            var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
            var path = arg.Replace("[controller]", ControllerName.ToCamelCase());
            int level = 0;

            if (Options.Inject?.BeforeServiceClass != null)
            {
                foreach (var line in Options.Inject.BeforeServiceClass)
                {
                    sb.AppendLine(line);
                }
            }

            sb.AppendLine(level, $"export class {ClassName} {{");

            sb.AppendLine(level, $"	static readonly controllerName = '{TypeSymbol.Name}';");
            sb.AppendLine(level, $"	private api = '{path}';");

            sb.AppendLine(level, "	constructor(private invoker: WebTypedInvoker) {}");



            var existingMethods = new Dictionary<string, int>();

            List<string> typeAliases = new List<string>();

            foreach (var m in TypeSymbol.GetMembers())
            {
                if (m.Kind == SymbolKind.NamedType)
                {
                    continue;
                }
                if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
                if (m.IsStatic) { continue; }
                if (m.Kind != SymbolKind.Method) { continue; }
                if (m.IsImplicitlyDeclared) { continue; }
                if (!m.IsDefinition) { continue; }
                var mtd = m as IMethodSymbol;
                if (m.Name == ".ctor") { continue; }

                var mtdAttrs = mtd.GetAttributes();
                var returnType = TypeResolver.Resolve(mtd.ReturnType as ITypeSymbol, context);
                var returnTypeName = returnType.Name;
                //Not marked actions will accept posts
                var httpMethod = "Post";
                string action = "";

                //Get http method from method name pattern
                if (mtd.Name.StartsWith("Get")) { httpMethod = "Get"; }
                if (mtd.Name.StartsWith("Post")) { httpMethod = "Post"; }
                if (mtd.Name.StartsWith("Put")) { httpMethod = "Put"; }
                if (mtd.Name.StartsWith("Delete")) { httpMethod = "Delete"; }
                if (mtd.Name.StartsWith("Patch")) { httpMethod = "Patch"; }
                var methodName = mtd.Name.ToCamelCase();
                if (existingMethods.ContainsKey(methodName))
                {
                    existingMethods[methodName]++;
                    methodName = $"{methodName}_{existingMethods[methodName]}";
                }
                else
                {
                    existingMethods.Add(methodName, 0);
                }
                var httpAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Http"));
                var routeMethodAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Route"));
                //If has Http attribute
                if (httpAttr != null)
                {
                    httpMethod = httpAttr.AttributeClass.Name.Substring(4);
                    //Check if it contains route info
                    var args = (httpAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax)?.ArgumentList?.Arguments;
                    if (args != null && args.Value.Count > 0)
                    {
                        action = args.Value[0].ToString().Replace("\"", "");
                    }
                }
                //Check if contains route attr
                if (routeMethodAttr != null)
                {
                    var args = (routeMethodAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments;
                    if (args.Count > 0)
                    {
                        action = args[0].ToString().Replace("\"", "");
                    }
                }
                //Replace route variables
                action = action.Replace("[action]", methodName);

                var regex = new Regex(@"\{(?<paramName>\w+)(:\w+(\(.*?\))?)?\??}");
                action = regex.Replace(action, match =>
                {
                    return $"${{{match.Groups["paramName"].Value}}}";
                });

                //Resolve how parameters are sent
                var pendingParameters = new List<ParameterResolution>();
                var parameterResolutions = mtd.Parameters.Select(p => new ParameterResolution(p, TypeResolver, context, Options)).Where(p => !p.Ignore);
                var bodyParam = "null";
                foreach (var pr in parameterResolutions)
                {
                    //[FromRoute]
                    if (action.Contains($"{{{pr.Name}}}"))
                    {
                        //Casting to any because encodeURIComponent expects string
                        action = action.Replace($"{{{pr.Name}}}", $"{{encodeURIComponent(<any>{pr.Name})}}");
                        continue;
                    }

                    //[FromBody]
                    if (pr.From == ParameterFromKind.FromBody)
                    {
                        bodyParam = pr.Name;
                        continue;
                    }
                    pendingParameters.Add(pr);
                }
                var strParameters = string.Join(", ", parameterResolutions.Select(pr => pr.Signature));

                var upperMethodName = methodName[0].ToString().ToUpper() + methodName.Substring(1);
                typeAliases.Add($"export type {upperMethodName}Parameters = {{{strParameters}{(parameterResolutions.Any() ? ", " : "")}_wtKind: '{upperMethodName}' }};");
                typeAliases.Add($"export interface {upperMethodName}CallInfo extends WebTypedCallInfo<{upperMethodName}Parameters, {returnTypeName}> {{ kind: '{upperMethodName}'; }}");
                typeAliases.Add($"export type {upperMethodName}FunctionBase = ({strParameters}) => {genericReturnType}<{returnTypeName}>;");
                typeAliases.Add($"export interface {upperMethodName}Function extends WebTypedFunction<{upperMethodName}Parameters, {returnTypeName}>, {upperMethodName}FunctionBase {{}}");

                sb.AppendLine(level + 1, $"{methodName}: {ClassName}.{upperMethodName}Function = ({strParameters}) : {genericReturnType}<{returnTypeName}> => {{");
                sb.AppendLine(level + 2, $"return this.invoker.invoke({{");

                //info
                sb.AppendLine(level + 4, $"returnTypeName: '{returnTypeName}',");
                sb.AppendLine(level + 4, $"kind: '{upperMethodName}',");
                sb.AppendLine(level + 4, $"func: this.{methodName},");
                sb.AppendLine(level + 4, $"parameters: {{ {string.Join(", ", parameterResolutions.Select(p => p.Name))}{(parameterResolutions.Any() ? ", " : "")}_wtKind: '{upperMethodName}' }}");
                sb.AppendLine(level + 3, "},");

                //api
                sb.AppendLine(level + 3, $"this.api,");

                //action
                sb.AppendLine(level + 3, $"`{action}`,");

                //httpMethod
                sb.AppendLine(level + 3, $"`{httpMethod.ToLower()}`,");

                //Body
                switch (httpMethod)
                {
                    case "Put":
                    case "Patch":
                    case "Post":
                        sb.AppendLine(level + 3, $"{bodyParam},");
                        break;
                    default: sb.AppendLine(level + 3, "undefined,"); break;
                }

                //Search
                var search = pendingParameters.Any() ? $"{{ {string.Join(", ", pendingParameters.Select(pr => pr.SearchRelayFormat))} }}" : "undefined";
                sb.AppendLine(level + 3, $"{search},");

                //Expects
                var genericReturnTypeDescriptor = Options.GenericReturnType ?? new ClientType { Name = "Promise" };
                var genericReturnTypeDescriptorJson = JsonConvert.SerializeObject(genericReturnTypeDescriptor, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
                sb.AppendLine(level + 3, $"{genericReturnTypeDescriptorJson}");

                sb.AppendLine(level + 2, ");");
                sb.AppendLine(level + 1, "};");
            }
            sb.AppendLine(level, "}");
            sb.AppendLine(level, $"export declare namespace {ClassName} {{");
            typeAliases.ForEach(ta => sb.AppendLine(level + 1, ta));
            sb.AppendLine(level, "}");
            sb.Insert(0, context.GetImportsText());
            string content = sb.ToString();
            return (OutputFilePath, content);
        }

        public OutputFileAbstraction GetAbstraction()
        {
            var context = new ResolutionContext(this);
            var serviceAbstraction = new ServiceAbstraction();
            serviceAbstraction.Path = AbstractPath;
            serviceAbstraction.ClassName = ClassName;
            serviceAbstraction.ControllerName = TypeSymbol.Name;
            serviceAbstraction.Actions = new List<ActionAbstraction>();

            //Resolve endpoint
            var routeAttr = TypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "Route" || a.AttributeClass.Name.ToString() == "RoutePrefix");
            var arg = (routeAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments[0].ToString().Replace("\"", "");
            var path = arg.Replace("[controller]", ControllerName.ToCamelCase());
            serviceAbstraction.Endpoint = path;

            //Actions
            var existingMethods = new Dictionary<string, int>();
            foreach (var m in TypeSymbol.GetMembers())
            {
                if (m.Kind == SymbolKind.NamedType)
                {
                    continue;
                }
                if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
                if (m.IsStatic) { continue; }
                if (m.Kind != SymbolKind.Method) { continue; }
                if (m.IsImplicitlyDeclared) { continue; }
                if (!m.IsDefinition) { continue; }
                var mtd = m as IMethodSymbol;
                if (m.Name == ".ctor") { continue; }


                var actionAbstraction = new ActionAbstraction();
                serviceAbstraction.Actions.Add(actionAbstraction);


                var mtdAttrs = mtd.GetAttributes();
                var returnType = TypeResolver.Resolve(mtd.ReturnType as ITypeSymbol, context);
                var returnTypeName = returnType.Name;
                //Not marked actions will accept posts
                var httpMethod = "Post";
                string action = "";

                //Get http method from method name pattern
                if (mtd.Name.StartsWith("Get")) { httpMethod = "Get"; }
                if (mtd.Name.StartsWith("Post")) { httpMethod = "Post"; }
                if (mtd.Name.StartsWith("Put")) { httpMethod = "Put"; }
                if (mtd.Name.StartsWith("Delete")) { httpMethod = "Delete"; }
                if (mtd.Name.StartsWith("Patch")) { httpMethod = "Patch"; }
                var methodName = mtd.Name.ToCamelCase();
                if (existingMethods.ContainsKey(methodName))
                {
                    existingMethods[methodName]++;
                    methodName = $"{methodName}_{existingMethods[methodName]}";
                }
                else
                {
                    existingMethods.Add(methodName, 0);
                }

                actionAbstraction.FunctionName = methodName;

                var httpAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Http"));
                var routeMethodAttr = mtdAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Route"));
                //If has Http attribute
                if (httpAttr != null)
                {
                    httpMethod = httpAttr.AttributeClass.Name.Substring(4);
                    //Check if it contains route info
                    var args = (httpAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax)?.ArgumentList?.Arguments;
                    if (args != null && args.Value.Count > 0)
                    {
                        action = args.Value[0].ToString().Replace("\"", "");
                    }
                }
                //Check if contains route attr
                if (routeMethodAttr != null)
                {
                    var args = (routeMethodAttr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax).ArgumentList.Arguments;
                    if (args.Count > 0)
                    {
                        action = args[0].ToString().Replace("\"", "");
                    }
                }
                //Replace route variables
                action = action.Replace("[action]", methodName);

                var regex = new Regex(@"\{(?<paramName>\w+)(:\w+(\(.*?\))?)?\??}");
                action = regex.Replace(action, match =>
                {
                    return $"${{{match.Groups["paramName"].Value}}}";
                });

                //Resolve how parameters are sent
                var pendingParameters = new List<ParameterResolution>();
                var parameterResolutions = mtd.Parameters.Select(p => new ParameterResolution(p, TypeResolver, context, Options)).Where(p => !p.Ignore);
                foreach (var pr in parameterResolutions)
                {
                    //[FromRoute]
                    if (action.Contains($"{{{pr.Name}}}"))
                    {
                        //Casting to any because encodeURIComponent expects string
                        action = action.Replace($"{{{pr.Name}}}", $"{{encodeURIComponent(<any>{pr.Name})}}");
                        continue;
                    }

                    //[FromBody]
                    if (pr.From == ParameterFromKind.FromBody)
                    {
                        actionAbstraction.BodyParameterName = pr.Name;
                        continue;
                    }
                    pendingParameters.Add(pr);
                }
                var strParameters = string.Join(", ", parameterResolutions.Select(pr => pr.Signature));

                actionAbstraction.Parameters = parameterResolutions.Select(p => new ParameterAbstraction{ 
                    Name = p.Name,
                    IsOptional = p.IsOptional,
                    TypeDescription = p.TypeDescription
                }).ToList();

                actionAbstraction.ReturnTypeDescription = returnTypeName;
                //action
                actionAbstraction.ActionName = action;
                //httpMethod
                actionAbstraction.HttpMethod = httpMethod;

                actionAbstraction.SearchParametersNames = new List<string>();

                //Body
                switch (httpMethod)
                {
                    case "Put":
                    case "Patch":
                    case "Post":
                        break;
                    default:
                        actionAbstraction.BodyParameterName = null;
                        break;
                }

                //Search
                actionAbstraction.SearchParametersNames = pendingParameters.Select(pr => pr.Name).ToList();
            }

            return serviceAbstraction;
        }

        public static bool IsService(INamedTypeSymbol t)
        {
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
