﻿using Microsoft.CodeAnalysis;
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
            var type = p.Type;
            //it it is generic type, try get some constraint type
            var cTypes = (type as ITypeParameterSymbol)?.ConstraintTypes;
            if (cTypes.HasValue)
            {
                type = cTypes.Value.First();
            }

            var attrs = p.GetAttributes();
			var hasNamedTupleAttr = attrs.Any(a => a.AttributeClass.Name == nameof(NamedTupleAttribute));
			var res = typeResolver.Resolve(type, context, hasNamedTupleAttr);

			this.Name = p.Name;
			this.FromName = p.Name;
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
			if (type.IsReferenceType) {
				//var allProps = new List<string>();
				////When FromRoute is used in a model it should be ignored in queries
				//var ignoredQueryProps = new List<string>();
				////When FromQuery/FromUri is used it may be renamed
				//var renamedQueryProps = new Dictionary<string, string>();
				var props = new List<string>();
				var outProps = new List<string>();
				var hasModifications = false;
                List<ISymbol> members = new List<ISymbol>();
                var t = type;
                while(t != null)
                {
                    members.AddRange(t.GetMembers());
                    t = t.BaseType;
                }

				foreach (var m in members) {
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
									hasModifications = true;
									//ignoredQueryProps.Add(m.Name);
									break;
								case "FromUriAttribute":
								case "FromQueryAttribute":
									KeyValuePair<string, TypedConstant>? nameArg = propFromAttr.NamedArguments.ToList().FirstOrDefault(na => na.Key == "Name");
									if (nameArg.HasValue) {
										var tConst = nameArg.Value.Value;
										if (tConst.Value != null) {
											//renamedQueryProps.Add(p.Name, tConst.Value.ToString());
											outProps.Add($"{tConst.Value.ToString()}: {this.Name}.{name}");
											hasModifications = true;
										}
									}
									break;
								default:
									props.Add($"{name}: {this.Name}.{name}");
									break;
							}
						}
						else {
							props.Add($"{name}: {this.Name}.{name}");
						}
					}
				}

				if (hasModifications) {
					this.SearchRelayFormat = "";
					if (props.Any()) {
                        //Pensar melhor, no asp.net podemos colocar como parametros varias models com props de nomes iguais. 
                        //Por esse motivo fazemos o obj: {}
                        //Ao mesmo tempo, isso é uma modelagem esquisita de api. Talvez devemos dar preferencia mesmo para a segunda opção
                        //onde fica tudo na "raiz". Além disso, não testei ainda o comportamento do asp.net quando multiplos parametros
                        //que clasheiam sujas props
                        //this.SearchRelayFormat = $"...({this.Name} ? {{ {this.FromName}: {{ {string.Join(", ", props)} }} }} : {{}})";
                        this.SearchRelayFormat = $"...({this.Name} ? {{ {string.Join(", ", props)} }} : {{}})";
                    }
					if (outProps.Any()) {
						if (props.Any()) {
                            //Add comma
							this.SearchRelayFormat += ", ";
						}
						this.SearchRelayFormat += $"...({this.Name} ? {{{string.Join(", ", outProps)}}} : {{}})";

					}
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

			if (TsEnum.IsEnum(type)) {
				if (res.TsType != null && res.TsType is TsEnum) {
					var enumNames = string
						.Join(
							" | ",
							type.GetMembers()
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
