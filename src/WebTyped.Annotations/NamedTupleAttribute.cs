using System;

namespace WebTyped.Annotations {
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
	public class NamedTupleAttribute : Attribute {
		public NamedTupleAttribute() {}
	}
}
