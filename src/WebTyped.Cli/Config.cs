using System;
using System.Collections.Generic;
using System.Text;

namespace WebTyped.Cli
{
	public class Config {
        public string Generator { get; set; }

		public IEnumerable<string> Files { get; set; }

        public IEnumerable<string> Assemblies { get; set; }

        public IEnumerable<Package> Packages { get; set; }

        public IEnumerable<string> ReferenceTypes { get; set; }

        public IEnumerable<string> Trims { get; set; }

        public Inject Inject { get; set; }

		public string OutDir { get; set; }

        public Dictionary<string, ClientType> CustomMap { get;set; }

		public bool Clear { get; set; } = true;

		//public ServiceMode ServiceMode { get; set; } = ServiceMode.Fetch;
        public ClientType GenericReturnType { get; set; }

		//public TypingsScope TypingsScope { get; set; } = TypingsScope.Global;
		public string BaseModule { get; set; }

		public bool KeepPropsCase { get; set; }

		//public bool KeysAndNames { get; set; }
		public string ServiceSuffix { get; set; }

		public Options ToOptions() {
			return new Options(OutDir) {
                Clear = Clear,
                GenericReturnType = GenericReturnType,
                ModuleTrims = Trims ?? new string[0],
                BaseModule = BaseModule,
                KeepPropsCase = KeepPropsCase,
				ServiceSuffix = ServiceSuffix,
                Inject = Inject
            };
		}
	}
}
