using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebTyped.Abstractions;

namespace WebTyped.Elements {
    public abstract class TsModelBase : ITsFile
    {
        public INamedTypeSymbol TypeSymbol { get; private set; }
        public TypeResolver TypeResolver { get; private set; }
        public Options Options { get; private set; }

        public string ClassName {
            get {
                if (TypeSymbol.TypeArguments.Any())
                {
                    return $"{TypeSymbol.Name}Of{TypeSymbol.TypeArguments.Count()}";
                }
                else
                {
                    return TypeSymbol.Name;
                }
            }
        }

        public string ModuleCamel {
            get {
                return Module.ToCamelCase();
            }
        }

        public string Module {
            get {
                var parent = TypeSymbol.ContainingType;
                if (parent != null && TypeResolver.TypeFiles.TryGetValue(parent, out var parentElement))
                {
                    return $"{parentElement.ClassName}";
                }

                //Module will be Namespace.{parent classes}
                var moduleName = "";
                if (!TypeSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    moduleName = TypeSymbol.ContainingNamespace.ToString();
                }
                var parents = new List<string>();
                while (parent != null)
                {
                    parents.Add(parent.Name);
                    parent = parent.ContainingType;
                }
                if (parents.Any())
                {
                    parents.Reverse();
                    moduleName += $".{string.Join(".", parents)}";
                }
                return Options.AdjustModule(moduleName);
            }
        }

        public bool HasCustomMap {
            get {
                if (Options.CustomMap == null) { return false; }
                return Options.CustomMap.ContainsKey(TypeSymbol.ConstructedFrom.ToString());
            }
        }

        public string OutputFilePath {
            get {
                return Path.Combine(Options.OutDir, ModuleCamel, Filename);
            }
        }

        //public string OutputFilePathWithoutExtension {
        //    get {
        //        return Path.Combine(Options.OutDir, ModuleCamel, FilenameWithoutExtenstion);
        //    }
        //}

        public string AbstractPath {
            get {
                return Path.Combine(ModuleCamel, FilenameWithoutExtenstion);
            }
        }

        public string FilenameWithoutExtenstion {
            get {
                return ClassName.ToCamelCase();
            }
        }

        protected string Filename {
            get {
                return $"{FilenameWithoutExtenstion}.ts";
            }
        }

        public TsModelBase(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options)
        {
            TypeSymbol = modelType;
            TypeResolver = typeResolver;
            Options = options;
        }

        protected void AppendKeysAndNames(StringBuilder sb)
        {
            var level = 0;
            sb.AppendLine();
            sb.AppendLine($"export class {ClassName}$ {{");
            level++;

            //Class name
            sb.AppendLine(level, $"static readonly $ = '{ClassName}';");

            //Members
            var currentTypeSymbol = TypeSymbol;
            var members = new List<ISymbol>();
            do
            {
                members.AddRange(currentTypeSymbol.GetMembers());
                currentTypeSymbol = currentTypeSymbol.BaseType;
            } while (currentTypeSymbol != null);

            HashSet<string> appendedMembers = new HashSet<string>();

            foreach (var m in members)
            {
                if (m.Kind != SymbolKind.Field && m.Kind != SymbolKind.Property)
                {
                    continue;
                }
                if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
                var name = m.Name;
                if (!Options.KeepPropsCase && !((m as IFieldSymbol)?.IsConst).GetValueOrDefault())
                {
                    name = name.ToCamelCase();
                }

                //Avoid dup
                if (appendedMembers.Contains(name)) { continue; }

                appendedMembers.Add(name);
                sb.AppendLine(level, $"static readonly ${name} = '{name}';");
            }

            level--;
            sb.AppendLine("}");
        }

        public abstract (string file, string content)? GenerateOutput();
        public abstract OutputFileAbstraction GetAbstraction();
    }
}
