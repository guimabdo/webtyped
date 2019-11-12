using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped
{
    public enum ServiceMode
    {
        Angular,
        Angular4,
        Jquery,
        Fetch
    }

    public class Inject
    {
        public List<string> BeforeServiceClass { get; set; }
    }


    public class ClientType
    {
        public string Name { get; set; }

        public string Module { get; set; }
    }

    public class Package
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Csproj { get; set; }
    }

    //public enum TypingsScope {
    //	Global,
    //	Module
    //}

    //public class ExternalAssembly
    //{
    //    public string Path { get; set; }
    //    public List<string> Classes { get; set; }
    //}

    public class Options
    {
        public string OutDir { get; set; }

        public bool Clear { get; set; }

        public IEnumerable<string> ModuleTrims { get; set; }

        public string BaseModule { get; set; }

        public ClientType GenericReturnType { get; set; }

        public Inject Inject { get; set; }

        public Dictionary<string, ClientType> CustomMap { get; set; }

        public string ServiceSuffix { get; set; } = "Service";

        public bool KeepPropsCase { get;  set; }
        public Options(string outDir)
        {
            if (string.IsNullOrWhiteSpace(outDir))
            {
                outDir = "./";//This works for linux and windows, .\\ will not work for linux 
            }
            OutDir = outDir;
        }

        public string AdjustModule(string module)
        {
            if (ModuleTrims != null)
            {
                foreach (var mt in ModuleTrims.OrderByDescending(m => m.Length))
                {
                    if (module.StartsWith(mt))
                    {
                        module = module.Replace(mt, "");
                    }
                }
            }

            module = module.Trim('.');
            if (!string.IsNullOrWhiteSpace(BaseModule))
            {
                module = $"{BaseModule}.{module}";
            }
            //while (module.StartsWith(".")) {
            //	module = module.Substring(1);
            //}
            module = module.Trim('.');

            return module;
        }
    }
}
