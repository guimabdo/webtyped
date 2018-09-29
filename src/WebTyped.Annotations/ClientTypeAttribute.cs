using System;

namespace WebTyped.Annotations {
	/// <summary>
	/// Maps a server type to a specific client type, so WebTyped generator will not generate the client type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ClientTypeAttribute : Attribute {
		public ClientTypeAttribute(string typeName = null, string module = null) {}
	}
}
