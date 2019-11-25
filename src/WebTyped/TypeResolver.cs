using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Abstractions;
using WebTyped.Elements;

namespace WebTyped
{
    public class TypeResolution
    {
        public string ConstructedFrom { get; set; }

        public string ModuleAlias { get; set; }

        /// <summary>
        /// TypeResolution or string with the generic argument name
        /// </summary>
        public List<object> GenericArguments { get; set; }

        public string Declaration { get; set; }

        //public ITsFile TsType { get; set; }
        public bool IsEnum { get; set; }

        public bool IsNullable { get; set; }

        public TypeResolution Inherits { get; set; }

        public bool IsKnown { get; set; }

        public bool IsAny {
            get {
                return this.Declaration == "any" || this.Declaration.StartsWith("any/*");
            }
        }
    }

    public class ResolutionContext
    {
        int counter = 0;
        Dictionary<string, string> _aliasByModule { get; set; } = new Dictionary<string, string>();
        public ITsFile Target { get; private set; }
        public ResolutionContext(ITsFile target)
        {
            Target = target;
        }
        public string GetAliasByModule(string externalModule)
        {
            if (!_aliasByModule.ContainsKey(externalModule))
            {
                string alias = $"extMdl{counter++}";
                _aliasByModule[externalModule] = alias;
            }

            return _aliasByModule[externalModule];
        }

        public IDictionary<string, string> GetImports()
        {
            return _aliasByModule;
        }

        public string GetImportsText()
        {
            var sb = new StringBuilder();
            foreach (var kvp in _aliasByModule)
            {
                sb.AppendLine($"import * as {kvp.Value} from '{kvp.Key}';");
            }
            return sb.ToString();
        }
    }

    public class TypeResolver
    {
        public IEnumerable<Service> Services { get; private set; } = new List<Service>();
        public IEnumerable<TsModelBase> Models { get; private set; } = new List<TsModelBase>();
        public Dictionary<ITypeSymbol, ITsFile> TypeFiles { get; private set; } = new Dictionary<ITypeSymbol, ITsFile>();
        Options Options { get; set; }
        public TypeResolver(Options options)
        {
            this.Options = options;
        }

        public void Add(ITsFile file)
        {
            this.TypeFiles[file.TypeSymbol] = file;
            if (file is TsModelBase)
            {
                (this.Models as List<TsModelBase>).Add(file as TsModelBase);
            }
            if (file is Service)
            {
                (this.Services as List<Service>).Add(file as Service);
            }
        }
        public TypeResolution Resolve(
            ITypeSymbol typeSymbol, 
            ResolutionContext context, 
            bool fillInheritance = false
        )
        {
            var type = typeSymbol as INamedTypeSymbol;

            var result = new TypeResolution();
            result.ConstructedFrom = type?.ConstructedFrom?.ToString();

            //Normalize type identifier
            if (!string.IsNullOrWhiteSpace(result.ConstructedFrom))
            {
                var splitted = result.ConstructedFrom.Split('<');
                var baseName = splitted[0];
                if (splitted.Length > 1)
                {
                    splitted = splitted[1].Split(',');
                    //Normalize constructedFrom. when analyzing compiled assemblies, generic args are omitted
                    result.ConstructedFrom = baseName + "<" + new StringBuilder().Append(',', splitted.Length - 1) + ">";
                }
            }

            //Get generics
            if (type != null && type.IsGenericType)
            {
                result.GenericArguments = type
                        .TypeArguments
                        .Select(t =>
                            t is INamedTypeSymbol ?
                                /* When type is defined */
                                (object)Resolve(t as INamedTypeSymbol, context)
                                //Just the argument name
                                : t.Name
                        )
                        .ToList();

            }

            //Base Type
            if (fillInheritance && type != null && type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
            {
                result.Inherits = Resolve(
                    type.BaseType, 
                    context, 
                    //Avoid creating unecessary extra import int the current context 
                    false);
            }

            //User custom type mapping
            if (Options.CustomMap != null && Options.CustomMap.ContainsKey(result.ConstructedFrom))
            {
                //Found definition with this full type name
                var clientType = Options.CustomMap[result.ConstructedFrom];

                var externalModule = clientType.Module;
                var externalName = clientType.Name;
                if (string.IsNullOrWhiteSpace(externalModule))
                {
                    result.Declaration = externalName;
                }
                else
                {
                    var alias = context.GetAliasByModule(externalModule);
                    result.ModuleAlias = alias;
                    result.Declaration = $"{alias}.{externalName}";
                }

                result.IsKnown = true;
                return result;
            }

            //Generic part of a generic class
            if (typeSymbol is ITypeParameterSymbol)
            {
                result.Declaration = typeSymbol.Name;
                return result;
            }

            result.IsNullable = IsNullable(typeSymbol);

            var tsType = TypeFiles.ContainsKey(typeSymbol) ?
                               TypeFiles[typeSymbol]
                                    //When inheriting from another generic model
                                    : TypeFiles.ContainsKey(typeSymbol.OriginalDefinition) ?
                                            TypeFiles[typeSymbol.OriginalDefinition] : null;
            if (tsType != null)
            {
                //result.TsType = tsType;
                result.IsEnum = tsType is TsEnum;
                result.IsKnown = true;
                result.Declaration = tsType.ClassName;
                if (tsType is TsModelBase
                    &&
                    tsType != context.Target //auto-reference
                )
                {
                    var tsModel = tsType as TsModelBase;

                    var c = new Uri("C:\\", UriKind.Absolute);
                    var uriOther = new Uri(c, new Uri(tsModel.OutputFilePath, UriKind.Relative));
                    var uriMe = new Uri(c, new Uri(context.Target.OutputFilePath, UriKind.Relative));


                    var module = uriMe.MakeRelativeUri(uriOther).ToString();
                    module = module.Substring(0, module.Length - 3);
                    if (module[0] != '.')
                    {
                        module = "./" + module;
                    }
                    var alias = context.GetAliasByModule(module);
                    result.ModuleAlias = alias;
                    result.Declaration = $"{alias}.{result.Declaration}";
                }
                //When inheriting from another generic model
                if (type.IsGenericType)
                {
                    var gp_ = GetGenericPart(type, result.Declaration, context);
                    result.Declaration = $"{result.Declaration}{gp_.genericPart}";
                }
                return result;
            }

            //Array
            if (typeSymbol is IArrayTypeSymbol)
            {
                var arrTypeSymbol = typeSymbol as IArrayTypeSymbol;

                //This generic type does not really exists, just the base type "System.Array", but it will help generators easily identify arrays
                result.ConstructedFrom = "System.Array<>";
                result.GenericArguments = new List<object>
                {
                    Resolve(arrTypeSymbol.ElementType, context)
                };


                if (arrTypeSymbol.ElementType.SpecialType == SpecialType.System_Byte)
                {
                    result.Declaration = "string";
                }
                else
                {
                    var elementTypeRes = Resolve(arrTypeSymbol.ElementType, context);
                    result.Declaration = $"Array<{elementTypeRes.Declaration}>";
                }
                return result;
            }
            //tuples
            if (type.IsTupleType)
            {
                var tupleElements = type.TupleElements
                    .Select(te => new
                    {
                        field = (Options.KeepPropsCase ? te.Name : te.Name.ToCamelCase()),
                        tupleField = (Options.KeepPropsCase ? te.CorrespondingTupleField.Name : te.CorrespondingTupleField.Name.ToCamelCase()),
                        type = Resolve(te.Type as INamedTypeSymbol, context).Declaration
                    });

                var tupleProps = tupleElements
                    .Select(te => $"/** field:{te.field} */{te.tupleField}: {te.type}");

                result.Declaration = $"{{{string.Join(", ", tupleProps)}}}";
                return result;
            }

            //string name = type.Name;
            var members = type.GetTypeMembers();
            string typeName = type.Name;
            //In case of nested types
            var parent = type.ContainingType;
            while (parent != null)
            {
                //Check if parent type is controller > service
                var parentName = parent.Name;
                //Adjust to check prerequisites
                if (Service.IsService(parent))
                {
                    //parentName = parentName.Replace("Controller", "Service");
                    parentName = parentName.Replace("Controller", Options.ServiceSuffix);
                }
                //For now, we'll just check if ends with "Controller" suffix
                //if (parentName.EndsWith("Controller")) {
                //	parentName = parentName.Replace("Controller", "Service");
                //}

                typeName = $"{parentName}.{typeName}";
                parent = parent.ContainingType;
            }
            //Change type to ts type
            var tsTypeName = ToTsTypeName(type, context);
            //If contains "{" or "}" then it was converted to anonymous type, so no need to do anything else.
            if (tsTypeName.Contains("{"))
            {
                result.Declaration = tsTypeName;
                return result;
            }

            var gp = GetGenericPart(type, tsTypeName, context);
            result.Declaration = $"{gp.modifiedTsTypeName}{gp.genericPart}";
            return result;
        }

        public (string genericPart, string modifiedTsTypeName) GetGenericPart(INamedTypeSymbol type, string tsTypeName, ResolutionContext context)
        {
            string genericPart = "";
            //Generic
            if (type.IsGenericType)
            {
                if (string.IsNullOrEmpty(tsTypeName))
                {
                    tsTypeName = Resolve(type.TypeArguments[0], context).Declaration;
                }
                else
                {
                    var types = type.TypeArguments
                        .Select(t =>
                            t is INamedTypeSymbol ?
                            /* When type is defined */
                            Resolve(t as INamedTypeSymbol, context).Declaration
                            : t.Name /* When it is a generic param reference */ );
                    genericPart = $"<{string.Join(", ", types)}>";
                }
            }

            if (tsTypeName == "any" || tsTypeName.StartsWith("any/*"))
            {
                genericPart = genericPart.Replace("*", "");
                if (!string.IsNullOrEmpty(genericPart))
                {
                    genericPart = $"/*{genericPart}*/";
                }
            }
            if (tsTypeName == "Array" && string.IsNullOrWhiteSpace(genericPart))
            {
                genericPart = "<any>";
            }
            return (genericPart, tsTypeName);
        }
        public bool IsNullable(ITypeSymbol t)
        {
            //return (t as INamedTypeSymbol)?.ConstructedFrom?.ToString() == "System.Nullable<T>";
            return ((t as INamedTypeSymbol)?.ConstructedFrom?.ToString().StartsWith("System.Nullable<")).GetValueOrDefault();
        }

        public class NormalizedType
        {
            public string Name { get; set; }

            public List<string> GenericArguments { get; set; }
        }

        string ToTsTypeName(INamedTypeSymbol original, ResolutionContext context)
        {
            if (IsNullable(original)) { return ""; }
            switch (original.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return "boolean";
                case SpecialType.System_Byte:
                case SpecialType.System_Decimal:
                case SpecialType.System_Double:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    return "number";
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_DateTime:
                    return "string";
                case SpecialType.System_Array:
                case SpecialType.System_Collections_Generic_ICollection_T:
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                case SpecialType.System_Collections_IEnumerable:
                    return "Array";
                case SpecialType.System_Void: return "void";
            }
            var constructedFrom = (original as INamedTypeSymbol).ConstructedFrom.ToString();

            switch (constructedFrom)
            {
                case "System.DateTimeOffset":
                    return "string";
                case "System.Collections.Generic.IList<T>":
                case "System.Collections.Generic.List<T>":
                case "System.Collections.Generic.IEnumerable<T>":
                case "System.Collections.Generic.ICollection<T>":
                case "System.Linq.IQueryable<T>":
                case "System.Collections.Generic.IList<>":
                case "System.Collections.Generic.List<>":
                case "System.Collections.Generic.IEnumerable<>":
                case "System.Collections.Generic.ICollection<>":
                case "System.Linq.IQueryable<>":
                    return "Array";
                case "System.Object":
                    return "any";
                case "System.Threading.Tasks.Task":
                    return "void";
                case "System.Threading.Tasks.Task<TResult>":
                    return "";
                case "System.Collections.Generic.KeyValuePair<TKey, TValue>":
                    {
                        var keyType = Resolve(original.TypeArguments[0], context).Declaration;
                        var valType = Resolve(original.TypeArguments[1], context).Declaration;
                        return Options.KeepPropsCase ?
                        $"{{ Key: {keyType}, Value: {valType} }}"
                        : $"{{ key: {keyType}, value: {valType} }}";
                    }
                case "System.Collections.Generic.Dictionary<TKey, TValue>":
                case "System.Collections.Generic.Dictionary<,>":
                    {
                        var keyType = Resolve(original.TypeArguments[0], context).Declaration;
                        var valType = Resolve(original.TypeArguments[1], context).Declaration;
                        if (keyType == "string" || keyType == "number")
                        {
                            return $"{{ [key: {keyType}]: {valType} }}";
                        }
                        goto default;
                    }
                default: return $"any/*{constructedFrom}*/";
            }
        }



        void Process(Action<string, string> outputManager)
        {
            foreach (var m in Models)
            {
                var output = m.GenerateOutput();
                if (!output.HasValue) { continue; }
                outputManager(output.Value.file, output.Value.content);
            }
            foreach (var s in Services)
            {
                var output = s.GenerateOutput();
                outputManager(output.file, output.content);
            }
            //Create indexes
            //Create root index
            var sbRootIndex = new StringBuilder();

            //Create index for each module folder
            IEnumerable<ITsFile> moduledFiles = Services;
            moduledFiles = moduledFiles.Union(Models);

            var typeModules = moduledFiles
                .GroupBy(s => s.Module)
                .Distinct()
                .OrderBy(g => g.Key);
            var counter = 0;
            var services = new List<string>();

            foreach (var tm in typeModules)
            {
                var isRoot = string.IsNullOrEmpty(tm.Key);
                var sbTypeIndex = isRoot ? sbRootIndex : new StringBuilder();
                var hasService = false;
                foreach (var t in tm.OrderBy(t => t.ClassName))
                {
                    if (t is Model m)
                    {
                        if (m.HasCustomMap)
                        {
                            continue;
                        }
                    }
                    sbTypeIndex.AppendLine($"export * from './{t.FilenameWithoutExtenstion}';");
                    if (t is Service)
                    {
                        if (!isRoot)
                        {
                            services.Add($"mdl{counter}.{t.ClassName}");
                        }
                        else
                        {
                            services.Add(t.ClassName);
                            sbRootIndex.AppendLine($"import {{ {t.ClassName} }} from './{t.FilenameWithoutExtenstion}';");
                        }
                        hasService = true;
                    }
                }

                if (!string.IsNullOrEmpty(tm.Key))
                {
                    var typesDir = Path.Combine(Options.OutDir, tm.Key.ToCamelCase());
                    var typesIndexFile = Path.Combine(typesDir, "index.ts");
                    outputManager(typesIndexFile, sbTypeIndex.ToString());
                }

                if (hasService)
                {
                    if (!isRoot)
                    {
                        sbRootIndex.AppendLine($"import * as mdl{counter} from './{tm.Key.ToCamelCase()}'");
                    }
                    counter++;
                }
            }
            sbRootIndex.AppendLine("export var serviceTypes = [");
            sbRootIndex.AppendLine(1, string.Join($",{System.Environment.NewLine}	", services));
            sbRootIndex.AppendLine("]");

            var rootIndexFile = Path.Combine(Options.OutDir, "index.ts");
            outputManager(rootIndexFile, sbRootIndex.ToString());
        }

        public Dictionary<string, string> GenerateOutputs()
        {
            var result = new Dictionary<string, string>();
            Process((file, content) => result.Add(file, content));
            return result;
        }

        public List<OutputFileAbstraction> GenerateAbstractions()
        {
            var result = new List<OutputFileAbstraction>();

            foreach (var m in Models)
            {
                var modelAbstraction = m.GetAbstraction();
                if (modelAbstraction != null)
                {
                    result.Add(modelAbstraction);
                }
            }

            foreach (var s in Services)
            {
                var serviceAbstraction = s.GetAbstraction();
                if (serviceAbstraction != null)
                {
                    result.Add(serviceAbstraction);
                }
            }

            return result;

        }

        public async Task SaveAllAsync()
        {
            var currentFiles =
                Directory.Exists(Options.OutDir) ?
                Directory.GetFiles(Options.OutDir, "*.ts", SearchOption.AllDirectories)
                : new string[0];
            List<Task<string>> tasks = new List<Task<string>>();
            Process((file, content) => tasks.Add(FileHelper.WriteAsync(file, content)));
            if (Options.Clear)
            {
                Console.WriteLine("Webtyped - Clearing invalid files");
                //currentFiles.ToList().ForEach(f => Console.WriteLine(f));
                var files = new HashSet<string>();
                foreach (var t in tasks)
                {
                    files.Add(await t);
                }
                //Console.Write("new files");
                //files.ToList().ForEach(f => Console.WriteLine(f));
                var delete = currentFiles.Except(files, StringComparer.InvariantCultureIgnoreCase);
                //delete.ToList().ForEach(f => Console.WriteLine(f));
                //Console.WriteLine($"celete: {string.Join(", ", delete)}");
                foreach (var f in delete)
                {
                    var line = File.ReadLines(f).FirstOrDefault();
                    var mark = FileHelper.Mark.Replace(System.Environment.NewLine, "");
                    //Console.WriteLine($"{line} == {FileHelper.Mark} - {line.CompareTo(FileHelper.Mark.Replace(System.Environment.NewLine, ""))}");
                    //if (line.ToUpper().Contains(FileHelper.Mark.ToUpper())) {
                    if (line == mark)
                    {
                        Console.WriteLine($"deleting {f}");
                        File.Delete(f);
                    }
                }
                DeleteEmptyDirs(Options.OutDir);
            }
        }

        static void DeleteEmptyDirs(string dir)
        {
            if (String.IsNullOrEmpty(dir))
                throw new ArgumentException(
                    "Starting directory is a null reference or an empty string",
                    "dir");

            try
            {
                foreach (var d in Directory.EnumerateDirectories(dir))
                {
                    DeleteEmptyDirs(d);
                }

                var entries = Directory.EnumerateFileSystemEntries(dir);

                if (!entries.Any())
                {
                    try
                    {
                        Directory.Delete(dir);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (DirectoryNotFoundException) { }
                }
            }
            catch (UnauthorizedAccessException) { }
        }
    }
}
