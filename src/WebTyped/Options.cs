using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped {
	public class Options {
		public string OutDir { get; private set; }
		public string TypingsDir { get; private set; }
		public bool Clear { get; private set; }
		//public IEnumerable<string> NamespaceTruncs { get; private set; }
		public IEnumerable<string> ModuleTrims { get; private set; }
		//public string ServicesDir { get; private set; }
		//public string ModelsDir { get; private set; }
		public Options(string outDir,
			bool clear,
			//IEnumerable<String> namespaceTruncs
			IEnumerable<String> moduleTrims
			/*, string servicesDir = "", string modelsDir = "typings"*/) {
			//NamespaceTruncs = namespaceTruncs;
			ModuleTrims = moduleTrims.OrderByDescending(m => m.Length);
			OutDir = outDir;
			TypingsDir = Path.Combine(outDir, "typings");
			Directory.CreateDirectory(TypingsDir);
			Clear = clear;
			//ServicesDir = Path.Combine(outDir, servicesDir);
			//ModelsDir = Path.Combine(outDir, modelsDir);
			//Directory.CreateDirectory(ServicesDir);
			//Directory.CreateDirectory(ModelsDir);
		}

		public string TrimModule(string module) {
			foreach(var mt in ModuleTrims) {
				if (module.StartsWith(mt)) {
					module = module.Replace(mt, "");
				}
			}
			while (module.StartsWith(".")) {
				module = module.Substring(1);
			}
			return module;
		}

		//public string TruncateNamespace(INamespaceSymbol ns) {
		//	return ns.ToString();
		//}
	}
}
