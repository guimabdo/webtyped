using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WebTyped.Example.Web.Binders {
	public class TupleJsonConverter : JsonConverter {
		IEnumerable<string> _transformNames;
		public TupleJsonConverter(IEnumerable<string> transformNames) {
			_transformNames = transformNames;
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			while (reader.Read()) {
				//reader.v
			}
			return null;
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			throw new NotImplementedException();
		}
		public override bool CanConvert(Type objectType) {
			return objectType.GetInterfaces().Any(i => i == typeof(ITuple));
		}
	}
	public class TupleBinder : IModelBinder {
		public Task BindModelAsync(ModelBindingContext bindingContext) {
			if (bindingContext.BindingSource.Id == "Body") {
				using (var reader = new StreamReader(bindingContext.ActionContext.HttpContext.Request.Body)) {
					var json = reader.ReadToEnd();
					var attr = ((ControllerActionDescriptor)bindingContext.ActionContext.ActionDescriptor).MethodInfo.GetParameters()[0].GetCustomAttributes(true).OfType<TupleElementNamesAttribute>().First();
					
					var obj = JsonConvert.DeserializeObject(json, bindingContext.ModelType, new TupleJsonConverter(attr.TransformNames));
					//var tuple = ValueTuple.Create<int, string>(2, "");
				}
			}
			return Task.CompletedTask;
		}
	}
	public class TupleBinderProvider : IModelBinderProvider {
		public TupleBinderProvider() {
				
		}
		
		public IModelBinder GetBinder(ModelBinderProviderContext context) {
			if(context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetInterfaces().Any(i => i == typeof(ITuple))) {
				return new TupleBinder();
			}
			return null;
		}
	}
}
