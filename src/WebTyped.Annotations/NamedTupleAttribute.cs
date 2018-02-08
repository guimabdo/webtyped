using System;

namespace WebTyped.Annotations {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NamedTupleAttribute : Attribute {
		public NamedTupleAttribute() {}
	}
}
