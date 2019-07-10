using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Annotations;
using WebTyped.Elements;

namespace WebTyped
{
    public class Generator
    {
        readonly IEnumerable<SyntaxTree> _trees;
        readonly Options _options;
        readonly IEnumerable<string> assemblies;
        readonly IEnumerable<Package> packages;
        readonly IEnumerable<string> referenceTypes;

        public Generator(
            IEnumerable<string> cSharps,
            IEnumerable<string> assemblies,
            IEnumerable<Package> packages,
            IEnumerable<string> assembliesClasses,
            Options options
        ) : this(cSharps.Select(s => CSharpSyntaxTree.ParseText(s)),
            assemblies,
            packages,
            assembliesClasses,
            options)
        { }


        //public Generator(
        //    IEnumerable<string> cSharps,
        //    IEnumerable<string> assemblies,
        //    Options options
        //): this(cSharps.Select(s => CSharpSyntaxTree.ParseText(s)), assemblies, new string[0], options) {}

        public Generator(
            IEnumerable<SyntaxTree> trees,
            IEnumerable<string> assemblies,
            IEnumerable<Package> packages,
            IEnumerable<string> assembliesClasses,
            Options options
        )
        {
            this._trees = trees;
            this._options = options;
            this.assemblies = assemblies ?? new string[0];
            this.referenceTypes = assembliesClasses ?? new string[0];
            this.packages = packages;
        }

        async Task<TypeResolver> PrepareAsync()
        {
            var trees = _trees.ToList();
            //TODO: Temp workaround.. investigate why referencing annotations assembly is nor working properly
            var attributes = CSharpSyntaxTree.ParseText(@"
using System;

namespace WebTyped.Annotations {
	[AttributeUsage(AttributeTargets.Class)]
	public class ClientTypeAttribute : Attribute {
		public ClientTypeAttribute(string typeName = null, string module = null) {}
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NamedTupleAttribute : Attribute {
		public NamedTupleAttribute() {}
	}
}

namespace System.Web.Http {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromUriAttribute : Attribute {
		public string Name { get; set; }
		public Type BinderType { get; set; }
		public bool SuppressPrefixCheck { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromBodyAttribute : Attribute {}
}

namespace Microsoft.AspNetCore.Mvc{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromQueryAttribute : Attribute {
		public string Name { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromRouteAttribute : Attribute {
		public string Name { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromBodyAttribute : Attribute {}
}
");
            //References
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            //var webTypedAnnotations = MetadataReference.CreateFromFile(typeof(ClientTypeAttribute).Assembly.Location);
            var systemRuntime = MetadataReference.CreateFromFile(typeof(int).Assembly.Location);
            var linqExpressions = MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location);
            var thisAssembly = MetadataReference.CreateFromFile(this.GetType().Assembly.Location);

            //External assemblies
            var externals = new List<PortableExecutableReference>();
            foreach (var path in assemblies)
            {
                if (File.Exists(path))
                {
                    externals.Add(MetadataReference.CreateFromFile(str));
                }
            }

            foreach (var pkg in packages)
            {
                var nugetGlobalPackages = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "%userprofile%/.nuget/packages"
                    : "~/.nuget/packages";
            }

            var compilation = CSharpCompilation.Create(
                "Comp",
                //Trees
                trees
                    .Union(new SyntaxTree[] { attributes })
                    ,
                //basic + external Assemblies
                new[] {
                        mscorlib,
                        systemRuntime,
                        linqExpressions,
                    //MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                    //thisAssembly
                    /*, webTypedAnnotations*/
                }
                    .Union(externals)
            );
            var typeResolver = new TypeResolver(_options);
            var tasks = new List<Task>();
            var namedTypeSymbols = new ConcurrentBag<INamedTypeSymbol>();
            var semanticModels = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));
            foreach (var t in trees)
            {
                tasks.Add(t.GetRootAsync().ContinueWith(tks =>
                {
                    var root = tks.Result;
                    foreach (var @type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
                    {
                        var sm = semanticModels[t];
                        namedTypeSymbols.Add(sm.GetDeclaredSymbol(@type));
                    }
                }));
            }

            //Types in referenced assemblies
            var assembliesTypesMatcher = new Matcher();
            referenceTypes.ToList().ForEach(t => assembliesTypesMatcher.AddInclude(t));
            //var allSymbols = compilation.GetSymbolsWithName((str) => true);
            foreach (var e in externals)
            {
                
                var ass = Assembly.LoadFile(Path.GetFullPath(e.FilePath));
                var assSymbol = compilation.GetAssemblyOrModuleSymbol(e) as IAssemblySymbol;
                var typeNames = ass.GetTypes().Select(t => t.FullName);
                
                foreach(var tn in typeNames)
                {
                    try
                    {
                        if (assembliesTypesMatcher.Match(tn).HasMatches)
                        {
                            namedTypeSymbols.Add(assSymbol.GetTypeByMetadataName(tn));
                        }
                    }
                    catch { }
                }
            }

           


            foreach (var tsk in tasks) { await tsk; }
            foreach (var s in namedTypeSymbols)
            {
                if (Service.IsService(s))
                {
                    typeResolver.Add(new Service(s, typeResolver, _options));
                    continue;
                }

                if (Model.IsModel(s))
                {
                    typeResolver.Add(new Model(s, typeResolver, _options));
                    continue;
                }

                if (TsEnum.IsEnum(s))
                {
                    typeResolver.Add(new TsEnum(s, typeResolver, _options));
                    continue;
                }
            }
            return typeResolver;
        }

        public async Task<Dictionary<string, string>> GenerateOutputsAsync()
        {
            var typeResolver = await PrepareAsync();
            return typeResolver.GenerateOutputs();
        }

        public async Task WriteFilesAsync()
        {
            var typeResolver = await PrepareAsync();
            await typeResolver.SaveAllAsync();
        }
    }
}
