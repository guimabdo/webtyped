using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped {
	public enum ServiceMode {
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

	public class Options {
		public string OutDir { get; private set; }

		//public string TypingsDir { get; private set; }
		public bool Clear { get; private set; }

		public IEnumerable<string> ModuleTrims { get; private set; }

		public string BaseModule { get; private set; }

        public ClientType GenericReturnType { get; private set; }

        public Inject Inject { get; private set; }

		//public ServiceMode ServiceMode { get; private set; }

		//public TypingsScope TypingsScope { get; private set; }

		public string ServiceSuffix { get; private set; }
		//public bool KeysAndNames { get; set; }

		//public bool GenerateKeys { get;  }
		//public bool IsAngular { get {
		//		return ServiceMode == ServiceMode.Angular || ServiceMode == ServiceMode.Angular4;
		//	}
		//}

		public bool KeepPropsCase { get; private set; }
		public Options(string outDir,
			bool clear,
            ClientType genericReturnType,
			//ServiceMode serviceMode,
			//TypingsScope typingsScope,
			IEnumerable<String> moduleTrims, 
            string baseModule, 
			bool keepPropsCase, 
			//bool keysAndNames,
			string serviceSuffix, Inject inject) {
			this.ServiceSuffix = serviceSuffix ?? "Service";
			ModuleTrims = moduleTrims.OrderByDescending(m => m.Length);
			if (string.IsNullOrWhiteSpace(outDir)) {
				outDir = "./";//This works for linux and windows, .\\ will not work for linux 
			}
			OutDir = outDir;
			//TypingsDir = Path.Combine(outDir, "typings");
			//Directory.CreateDirectory(TypingsDir);
			Clear = clear;
			BaseModule = baseModule;
			//ServiceMode = serviceMode;
			//TypingsScope = typingsScope;
			KeepPropsCase = keepPropsCase;
            //KeysAndNames = keysAndNames;

            GenericReturnType = genericReturnType;

            Inject = inject;
		}

		public string AdjustModule(string module) {
			foreach(var mt in ModuleTrims) {
				if (module.StartsWith(mt)) {
					module = module.Replace(mt, "");
				}
			}
			module = module.Trim('.');
			if (!string.IsNullOrWhiteSpace(BaseModule)) {
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
