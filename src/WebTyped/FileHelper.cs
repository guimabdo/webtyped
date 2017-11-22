using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebTyped {
	public static class FileHelper {
		public static async Task<string> WriteAsync(string file, string content) {
			if (!File.Exists(file) || File.ReadAllText(file) != content) {
				await File.WriteAllTextAsync(file, content);
			}
			return file;
		}
	}
}
