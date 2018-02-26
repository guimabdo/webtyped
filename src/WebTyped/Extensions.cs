using System;
using System.Collections.Generic;
using System.Text;

namespace WebTyped {
	public static class Extensions {
		public static StringBuilder AppendLine(this StringBuilder sb, int level, string text) {
			return sb
				.Append('\t', level)
				.AppendLine(text);
		}

		public static string ToCamelCase(this string str) {
			if (string.IsNullOrWhiteSpace(str)) { return str; }
			return str[0].ToString().ToLower() + str.Substring(1);
		}
	}
}
