using System;

namespace WebTyped.Annotations {
	[AttributeUsage(AttributeTargets.Class)]
	public class ClientTypeAttribute : Attribute {
		public ClientTypeAttribute(string typeName = null, string module = null) {}
	}
}
