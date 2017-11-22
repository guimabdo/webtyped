using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebTyped {
	public static class FileHelper {
		public static void Write(string file, string content) {
			if (!File.Exists(file) || File.ReadAllText(file) != content) {
				File.WriteAllText(file, content);
			}
		}
	}
}
