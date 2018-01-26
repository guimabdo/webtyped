using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebTyped {
	public enum ServiceMode {
		Angular, 
		Jquery,
		Fetch
	}
	public class Options {
		public string OutDir { get; private set; }
		public string TypingsDir { get; private set; }
		public bool Clear { get; private set; }
		public IEnumerable<string> ModuleTrims { get; private set; }
		public string BaseModule { get; private set; }
		public ServiceMode ServiceMode { get; private set; }
		public bool KeepPropsCase { get; private set; }
		public Options(string outDir,
			bool clear,
			ServiceMode serviceMode,
			IEnumerable<String> moduleTrims, string baseModule, bool keepPropsCase) {
			ModuleTrims = moduleTrims.OrderByDescending(m => m.Length);
			if (string.IsNullOrWhiteSpace(outDir)) { outDir = "./"; }
			OutDir = outDir;
			TypingsDir = Path.Combine(outDir, "typings");
			Directory.CreateDirectory(TypingsDir);
			Clear = clear;
			BaseModule = baseModule;
			ServiceMode = serviceMode;
			KeepPropsCase = keepPropsCase;
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
