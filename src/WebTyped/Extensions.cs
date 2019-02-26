using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTyped {
	public static class Extensions {
		static readonly CamelCaseNamingStrategy camel = new CamelCaseNamingStrategy();
		public static StringBuilder AppendLine(this StringBuilder sb, int level, string text) {
			return sb
				.Append('\t', level)
				.AppendLine(text);
		}

		public static string ToCamelCase(this string str) {
			if (string.IsNullOrWhiteSpace(str)) { return str; }
			return string.Join('.', str.Split('.').Select(s => camel.GetPropertyName(s, false)));
		}
	}
}
