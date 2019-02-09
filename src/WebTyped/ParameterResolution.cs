using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebTyped.Annotations;
using WebTyped.Elements;

namespace WebTyped {
	public class ParameterResolution {
		public string Name { get; private set; }
		public string Signature { get; private set; }
		public string BodyRelayFormat { get; private set; }
		public string SearchRelayFormat { get; private set; }
		public ParameterFromKind From { get; private set; } = ParameterFromKind.None;
		public string FromName { get; set; }
		public bool Ignore { get; private set; }
		public ParameterResolution(IParameterSymbol parameterSymbol, TypeResolver typeResolver, ResolutionContext context, Options options) {
			var p = parameterSymbol;
			var attrs = p.GetAttributes();
			var hasNamedTupleAttr = attrs.Any(a => a.AttributeClass.Name == nameof(NamedTupleAttribute));
			var res = typeResolver.Resolve(parameterSymbol.Type, context, hasNamedTupleAttr);

			this.Name = parameterSymbol.Name;
			this.FromName = parameterSymbol.Name;
			this.BodyRelayFormat = this.Name;
			this.SearchRelayFormat = this.Name;
			var fromAttr = attrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("From"));

			//
			if (fromAttr != null) {
				switch (fromAttr.AttributeClass.Name) {
					case "FromUriAttribute":
					case "FromQueryAttribute":
					case "FromBodyAttribute":
					case "FromRouteAttribute":
						this.From = (ParameterFromKind)typeof(ParameterFromKind).GetField(fromAttr.AttributeClass.Name.Replace("Attribute", "")).GetValue(null);
						switch (fromAttr.AttributeClass.Name) {
							case "FromUriAttribute":
							case "FromQueryAttribute":
								KeyValuePair<string, TypedConstant>? nameArg = fromAttr.NamedArguments.ToList().FirstOrDefault(na => na.Key == "Name");
								if (nameArg.HasValue) {
									var tConst = nameArg.Value.Value;
									if (tConst.Value != null) {
										this.FromName = tConst.Value.ToString();
										this.SearchRelayFormat = $"{this.FromName}: {this.Name}";
									}
								}
								break;
						}

						//

						break;
				}
				//if (attrs.Any(a => a.AttributeClass.Name.StartsWith("FromBody"))) {
				//	this.From = ParameterFromKind.FromBody;
				//}
				//if (attrs.Any(a => a.AttributeClass.Name.StartsWith("FromUri"))) {
				//	this.From = ParameterFromKind.FromUri;

				//}
				//if (attrs.Any(a => a.AttributeClass.Name.StartsWith("FromQuery"))) {
				//	this.From = ParameterFromKind.FromQuery;
				//}
				//if (attrs.Any(a => a.AttributeClass.Name.StartsWith("FromRoute"))) {
				//	this.From = ParameterFromKind.FromRoute;
				//}

				//switch (From) {
				//	case ParameterFromKind.FromQuery:
				//	case ParameterFromKind.FromUri:
				//		break;
				//	default:
				//		break;
				//}
			}

			//Check if it is a Model being used to catch query/route parameters
			if (parameterSymbol.Type.IsReferenceType) {
				//var allProps = new List<string>();
				////When FromRoute is used in a model it should be ignored in queries
				//var ignoredQueryProps = new List<string>();
				////When FromQuery/FromUri is used it may be renamed
				//var renamedQueryProps = new Dictionary<string, string>();
				var props = new List<string>();
				foreach (var m in parameterSymbol.Type.GetMembers()) {
					if (m.Kind != SymbolKind.Field && m.Kind != SymbolKind.Property) {
						continue;
					}
					if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
					if (((m as IFieldSymbol)?.IsConst).GetValueOrDefault()) { continue; }
					var name = m.Name;
					if (!options.KeepPropsCase && !((m as IFieldSymbol)?.IsConst).GetValueOrDefault()) {
						name = name.ToCamelCase();
					}
					if (m is IPropertySymbol) {
						//allProps.Add(m.Name);
						var propAttrs = m.GetAttributes();
						var propFromAttr = propAttrs.FirstOrDefault(a => a.AttributeClass.Name.StartsWith("From"));
						if (propFromAttr != null) {
							switch (propFromAttr.AttributeClass.Name) {
								case "FromRouteAttribute":
									//ignoredQueryProps.Add(m.Name);
									break;
								case "FromUriAttribute":
								case "FromQueryAttribute":
									KeyValuePair<string, TypedConstant>? nameArg = propFromAttr.NamedArguments.ToList().FirstOrDefault(na => na.Key == "Name");
									if (nameArg.HasValue) {
										var tConst = nameArg.Value.Value;
										if (tConst.Value != null) {
											//renamedQueryProps.Add(p.Name, tConst.Value.ToString());
											props.Add($"{tConst.Value.ToString()}: {this.Name}.{name}");
										}
									}
									break;
								default:
									props.Add($"{this.Name}.{name}");
									break;
							}
						}
						else {
							props.Add($"{this.Name}.{name}");
						}
					}
				}

				if (props.Any()) {
					this.SearchRelayFormat = $"{this.FromName}: {{ {string.Join(", ", props)} }}";
					//this.SearchRelayFormat = $"{this.FromName}: {{ ";
					//foreach(var pName in allProps) {
					//	if (ignoredQueryProps.Contains(pName)) {
					//		
					//	}
					//}
					//this.SearchRelayFormat += " }";
				}

			}


			//var hasMapFunc = !string.IsNullOrWhiteSpace(res.MapAltToOriginalFunc);
			string unsupportedNamedTupleMessage = "[UNSUPPORTED - NamedTupleAttribute must be used only for tuple parameters]";
			string typeName = res.Name;

			if (hasNamedTupleAttr) {
				if (!res.IsTuple) {
					this.BodyRelayFormat = unsupportedNamedTupleMessage;
					this.SearchRelayFormat = unsupportedNamedTupleMessage;
					//typeName = unsupportedNamedTupleMessage;
				}
				else {
					this.BodyRelayFormat = $"{res.MapAltToOriginalFunc}({this.Name})";
					this.SearchRelayFormat = $"{this.Name}: {this.BodyRelayFormat}";
					//typeName = res.AltName;
				}
			}

			if (TsEnum.IsEnum(p.Type)) {
				if (res.TsType != null && res.TsType is TsEnum) {
					var enumNames = string
						.Join(
							" | ",
							p.Type.GetMembers()
							.Where(m => m.Kind == SymbolKind.Field)
							.Select(m => $"'{m.Name}'"));
					if (!string.IsNullOrEmpty(enumNames)) {
						typeName = $"{typeName} | {enumNames}";
					}
				}
			}

			this.Signature = $"{p.Name}{(p.IsOptional ? "?" : "")}: {typeName}" + (res.IsNullable ? " | null" : "");
			this.Ignore = p.GetAttributes().Any(a => a.AttributeClass.Name == "FromServices");
		}

	}

}
