using System;
using System.Collections.Generic;
using System.Text;

namespace WebTyped.Cli
{
	public class Config {
		public IEnumerable<string> Files { get; set; }
		public IEnumerable<string> Trims { get; set; }
		public string OutDir { get; set; }
		public bool Clear { get; set; } = true;
		public ServiceMode ServiceMode { get; set; } = ServiceMode.Fetch;
		public string BaseModule { get; set; }
		public bool KeepPropsCase { get; set; }
		public bool KeysAndNames { get; set; }
		public string ServiceSuffix { get; set; }

		public Options ToOptions() {
			return new Options(OutDir,
				Clear,
				ServiceMode,
				Trims ?? new string[0],
				BaseModule,
				KeepPropsCase,
				KeysAndNames,
				ServiceSuffix);
		}
	}
}
