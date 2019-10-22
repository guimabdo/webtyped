using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cblx.AspNetCore.ModelMetadataApi.Models {
	public class ModelMetadata {
		readonly Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata m;
		public ModelMetadata(
			Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata,
			ModelMetadataOptions options
		) {
			m = metadata;
			List<string> manual = new List<string>() {
				nameof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata.Properties),
				nameof(DefaultModelMetadata.Attributes)
			};
			foreach (var prop in this.GetType().GetProperties()) {
				if (manual.Contains(prop.Name)) { continue; }
				var sourceValue = m.GetType().GetProperty(prop.Name).GetValue(m);
				prop.SetValue(this, sourceValue);
			}

			if (options.Models.Values.Any(t => t == metadata.ModelType)) {
				this.Properties = new List<ModelMetadata>();
				foreach (var p in m.Properties) {
					this.Properties.Add(new ModelMetadata(p, options));
				}
			}

			if (metadata is DefaultModelMetadata defMeta) {
				this.Attributes = new List<object>();
				foreach (var attr in defMeta.Attributes.Attributes) {
					this.Attributes.Add(attr);
				}
			}
		}

        /// <summary>
        /// Summary should be copied to output
        /// Multi line
        /// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<Object> Attributes { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsEnum { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsFlagsEnum { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsReadOnly { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsRequired { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Order { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Placeholder { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string NullDisplayText { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool ShowForDisplay { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool ShowForEdit { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string SimpleDisplayProperty { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string TemplateHint { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsNullableValueType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsCollectionType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsEnumerableType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsReferenceOrNullableType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string> EnumNamesAndValues { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string PropertyName { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ModelMetadata> Properties { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string DisplayName { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Description { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string DataTypeName { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool ConvertEmptyStringToNull { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string DisplayFormatString { get; set; }
	}
}
