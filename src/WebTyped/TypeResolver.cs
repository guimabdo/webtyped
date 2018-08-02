using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTyped.Elements;

namespace WebTyped {
	public class TypeResolution {
		public string OriginalName { get; set; }
		/// <summary>
		/// Currently used for named tuples...
		/// </summary>
		public string AltName { get; set; }

		public bool UseAltName { get; set; }
		public ITsFile TsType { get; set; }
		
		public string Name { get {
				if (UseAltName) {
					if (string.IsNullOrWhiteSpace(AltName)) { return "[This type doesn't have an alternative name. Are you using NamedTupleAttribute without a tuple type?]"; }
					return AltName;
				}
				return OriginalName;
			}
		}

		public string MapAltToOriginalFunc { get; set; }
		public bool IsNullable { get; set; }
		public bool IsTuple { get; set; }
		public bool IsAny {
			get {
				return this.Name == "any" || this.Name.StartsWith("any/*");
			}
		}
	}

	public class ResolutionContext {
		int counter = 0;
		Dictionary<string, string> _aliasByModule { get; set; } = new Dictionary<string, string>();
		public string GetAliasByModule(string externalModule) {
			if (!_aliasByModule.ContainsKey(externalModule)) {
				string alias = $"extMdl{counter++}";
				_aliasByModule[externalModule] = alias;
			}

			return _aliasByModule[externalModule];
		}

		public string GetImportsText() {
			var sb = new StringBuilder();
			foreach(var kvp in _aliasByModule) {
				sb.AppendLine($"import * as {kvp.Value} from '{kvp.Key}';");
			}
			return sb.ToString();
		}
	}

	public class TypeResolver {
		public IEnumerable<Service> Services { get; private set; } = new List<Service>();
		public IEnumerable<TsModelBase> Models { get; private set; } = new List<TsModelBase>();
		public Dictionary<ITypeSymbol, ITsFile> TypeFiles { get; private set; } = new Dictionary<ITypeSymbol, ITsFile>();
		Options Options { get; set; }
		public TypeResolver(Options options) {
			this.Options = options;
		}
	
		public void Add(ITsFile file) {
			this.TypeFiles[file.TypeSymbol] = file;
			if (file is TsModelBase) {
				(this.Models as List<TsModelBase>).Add(file as TsModelBase);
			}
			if (file is Service) {
				(this.Services as List<Service>).Add(file as Service);
			}
		}
		public TypeResolution Resolve(ITypeSymbol typeSymbol, ResolutionContext context, bool useTupleAltNames = false) {
			var type = typeSymbol as INamedTypeSymbol;

			var result = new TypeResolution();
			//Generic part of a generic class
			if (typeSymbol is ITypeParameterSymbol) {
				result.OriginalName = typeSymbol.Name;
				return result;
			}

			result.IsNullable = IsNullable(typeSymbol);
			
			var tsType = TypeFiles.ContainsKey(typeSymbol) ? TypeFiles[typeSymbol]
				//When inheriting from another generic model
				: TypeFiles.ContainsKey(typeSymbol.OriginalDefinition) ? TypeFiles[typeSymbol.OriginalDefinition] : null;
			if (tsType != null) {
				result.TsType = tsType;
				result.OriginalName = tsType.FullName;
				if(tsType is TsModelBase) {
					var tsModel = tsType as TsModelBase;
					//External types
					if (tsModel.ExternalType != null) {
						var externalModule = tsModel.ExternalType.Value.module;
						var externalName = tsModel.ExternalType.Value.name ?? result.OriginalName;
						if (string.IsNullOrWhiteSpace(externalModule)) {
							result.OriginalName = externalName;
						}
						else {
							var alias = context.GetAliasByModule(externalModule);
							result.OriginalName = $"{alias}.{externalName}";
						}
					}
				}
				//When inheriting from another generic model
				if (type.IsGenericType) {
					var gp_ = GetGenericPart(type, result.OriginalName, context, useTupleAltNames);
					result.OriginalName = $"{result.OriginalName}{gp_.genericPart}";
				}
				return result;
			}

			//Array
			if(typeSymbol is IArrayTypeSymbol) {
				var arrTypeSymbol = typeSymbol as IArrayTypeSymbol;
				if (arrTypeSymbol.ElementType.SpecialType == SpecialType.System_Byte) {
					result.OriginalName = "string";
				} else {
					var elementTypeRes = Resolve(arrTypeSymbol.ElementType, context, useTupleAltNames);
					result.OriginalName = $"Array<{elementTypeRes.Name}>";
				}
				return result;
			}
			//tuples
			if (type.IsTupleType) {
				//var tupleProps = type.TupleElements
				//	.Select(te => $"{(Options.KeepPropsCase ? te.Name : te.Name.ToCamelCase())}: {Resolve(te.Type as INamedTypeSymbol, context).Name}");
				var tupleElements = type.TupleElements
					.Select(te => new {
						field = (Options.KeepPropsCase ? te.Name : te.Name.ToCamelCase()),
						tupleField = (Options.KeepPropsCase ? te.CorrespondingTupleField.Name : te.CorrespondingTupleField.Name.ToCamelCase()),
						type = Resolve(te.Type as INamedTypeSymbol, context).OriginalName
					});
				
				var tupleProps = tupleElements
					.Select(te => $"/** field:{te.field} */{te.tupleField}: {te.type}");
				var tupleAltProps = tupleElements
					.Select(te => $"/** tuple field:{te.tupleField} */{te.field}: {te.type}");
				result.OriginalName = $"{{{string.Join(", ", tupleProps)}}}";
				result.AltName = $"{{{string.Join(", ", tupleAltProps)}}}";
				var mapParam = "__source";
				var mapping = string.Join(", ", tupleElements.Select(t => $"{t.tupleField}: {mapParam}.{t.field}"));
				result.MapAltToOriginalFunc = $"function({mapParam}) {{ return {{ {mapping}  }} }}";
				result.IsTuple = true;
				result.UseAltName = useTupleAltNames;
				return result;
			}

			//string name = type.Name;
			var members = type.GetTypeMembers();
			string typeName = type.Name;
			//In case of nested types
			var parent = type.ContainingType;
			while (parent != null) {
				//Check if parent type is controller > service
				var parentName = parent.Name;
				//Adjust to check prerequisites
				if (Service.IsService(parent)) {
					parentName = parentName.Replace("Controller", "Service");
				}
				//For now, we'll just check if ends with "Controller" suffix
				//if (parentName.EndsWith("Controller")) {
				//	parentName = parentName.Replace("Controller", "Service");
				//}

				typeName = $"{parentName}.{typeName}";
				parent = parent.ContainingType;
			}
			//Change type to ts type
			var tsTypeName = ToTsTypeName(type, context, useTupleAltNames);
			//If contains "{" or "}" then it was converted to anonymous type, so no need to do anything else.
			if (tsTypeName.Contains("{")) {
				result.OriginalName = tsTypeName;
				return result;
			}
			//if (tsTypeName == "any") {
			//	return $"any/* {type.ToString()} */";
			//}

			var gp = GetGenericPart(type, tsTypeName, context, useTupleAltNames);
			result.OriginalName = $"{gp.modifiedTsTypeName}{gp.genericPart}";
			return result;
		}

		public (string genericPart, string modifiedTsTypeName) GetGenericPart(INamedTypeSymbol type, string tsTypeName, ResolutionContext context, bool useTupleAltNames) {
			string genericPart = "";
			//Generic
			if (type.IsGenericType) {
				if (string.IsNullOrEmpty(tsTypeName)) {
					tsTypeName = Resolve(type.TypeArguments[0], context, useTupleAltNames).Name;
				} else {
					genericPart = $"<{string.Join(", ", type.TypeArguments.Select(t => Resolve(t as INamedTypeSymbol, context, useTupleAltNames).Name))}>";
				}
			}
			//genericPart = genericPart.Replace("*", "");

			if (tsTypeName == "any" || tsTypeName.StartsWith("any/*")) {
				genericPart = genericPart.Replace("*", "");
				if (!string.IsNullOrEmpty(genericPart)) {
					genericPart = $"/*{genericPart}*/";
				}
				//return $"{tsTypeName}{(string.IsNullOrEmpty(genericPart) ? "" : $"")}";
			}
			if (tsTypeName == "Array" && string.IsNullOrWhiteSpace(genericPart)) {
				genericPart = "<any>";
			}
			return (genericPart, tsTypeName);
		}
		public bool IsNullable(ITypeSymbol t) {
			return (t as INamedTypeSymbol)?.ConstructedFrom?.ToString() == "System.Nullable<T>";
		}

		string ToTsTypeName(INamedTypeSymbol original, ResolutionContext context, bool useTupleAltNames = false) {
			if (IsNullable(original)) { return ""; }
			switch (original.SpecialType) {
				case SpecialType.System_Boolean:
					return "boolean";
				case SpecialType.System_Byte:
				case SpecialType.System_Decimal:
				case SpecialType.System_Double:
				case SpecialType.System_Int16:
				case SpecialType.System_Int32:
				case SpecialType.System_Int64:
				case SpecialType.System_Single:
				case SpecialType.System_UInt16:
				case SpecialType.System_UInt32:
				case SpecialType.System_UInt64:
					return "number";
				case SpecialType.System_Char:
				case SpecialType.System_String:
				case SpecialType.System_DateTime:
					return "string";
				case SpecialType.System_Array:
				case SpecialType.System_Collections_Generic_ICollection_T:
				case SpecialType.System_Collections_Generic_IEnumerable_T:
				case SpecialType.System_Collections_Generic_IList_T:
				case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
				case SpecialType.System_Collections_Generic_IReadOnlyList_T:
				case SpecialType.System_Collections_IEnumerable:
					return "Array";
				case SpecialType.System_Void: return "void";
			}
			var constructedFrom = (original as INamedTypeSymbol).ConstructedFrom.ToString();

			//IQueryable<int> a;
			switch (constructedFrom) {
				//ase nameof(Boolean):
				//	return "boolean";
				//case nameof(DateTime):
				//case nameof(String):
				//	return "string";
				//case nameof(Int32):
				//case nameof(Int16):
				//case nameof(Int64):
				//	return "number";
				//case nameof(IEnumerable):
				case "System.DateTimeOffset":
					return "string";
				case "System.Collections.Generic.IList<T>":
				case "System.Collections.Generic.List<T>":
				case "System.Collections.Generic.IEnumerable<T>":
				case "System.Collections.Generic.ICollection<T>":
				case "System.Linq.IQueryable<T>":
					return "Array";
				case "System.Threading.Tasks.Task":
					return "void";
				case "System.Threading.Tasks.Task<TResult>":
					return "";
				case "System.Collections.Generic.KeyValuePair<TKey, TValue>":
					var keyType = Resolve(original.TypeArguments[0], context, useTupleAltNames).Name;
					var valType = Resolve(original.TypeArguments[1], context, useTupleAltNames).Name;
					return Options.KeepPropsCase ?
					$"{{ Key: {keyType}, Value: {valType} }}"
					: $"{{ key: {keyType}, value: {valType} }}";
				//default: return typeName;
				default: return $"any/*{constructedFrom}*/";
			}
		}



		void Process(Action<string, string> outputManager) {
			foreach (var m in Models) {
				var output = m.GenerateOutput();
				if (!output.HasValue) { continue; }
				outputManager(output.Value.file, output.Value.content);
			}
			foreach (var s in Services) {
				var output = s.GenerateOutput();
				outputManager(output.file, output.content);
			}
			//Create indexes
			//Create root index
			var sbRootIndex = new StringBuilder();
			if (Options.IsAngular) {
				sbRootIndex.AppendLine("import { NgModule, ModuleWithProviders } from '@angular/core';");
				sbRootIndex.AppendLine("import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';");
				var ngV = Options.ServiceMode == ServiceMode.Angular4 ? "4" : "";
				sbRootIndex.AppendLine($"import {{ WebTypedEventEmitterService, WebTypedInterceptor }} from '@guimabdo/webtyped-angular{ngV}';");
			}

			//Create index for each module folder
			var serviceModules = Services
				.GroupBy(s => s.Module)
				.Distinct()
				.OrderBy(g => g.Key);
			var counter = 0;
			var services = new List<string>();
			foreach (var sm in serviceModules) {
				sbRootIndex.AppendLine($"import * as mdl{counter} from './{sm.Key.ToCamelCase()}'");
				var sbServiceIndex = string.IsNullOrEmpty(sm.Key) ? sbRootIndex : new StringBuilder();
				foreach (var s in sm.OrderBy(s => s.ClassName)) {
					sbServiceIndex.AppendLine($"export * from './{s.FilenameWithoutExtenstion}';");
					services.Add($"mdl{counter}.{s.ClassName}");
				}

				if (!string.IsNullOrEmpty(sm.Key)) {
					var servicesDir = Path.Combine(Options.OutDir, sm.Key.ToCamelCase());
					var serviceIndexFile = Path.Combine(servicesDir, "index.ts");
					outputManager(serviceIndexFile, sbServiceIndex.ToString());
				}
				counter++;
			}
			sbRootIndex.AppendLine("export var serviceTypes = [");
			sbRootIndex.AppendLine(1, string.Join($",{System.Environment.NewLine}	", services));
			sbRootIndex.AppendLine("]");
			if (Options.IsAngular) {
				sbRootIndex.AppendLine("@NgModule({");
				sbRootIndex.AppendLine(1, "imports: [ HttpClientModule ]");
				sbRootIndex.AppendLine("})");
				sbRootIndex.AppendLine("export class WebTypedGeneratedModule {");
				sbRootIndex.AppendLine(1, "static forRoot(): ModuleWithProviders {");
				sbRootIndex.AppendLine(2, "return {");
				sbRootIndex.AppendLine(3, "ngModule: WebTypedGeneratedModule,");
				sbRootIndex.AppendLine(3, "providers: [");
				sbRootIndex.AppendLine(4, "{");
				sbRootIndex.AppendLine(5, "provide: HTTP_INTERCEPTORS,");
				sbRootIndex.AppendLine(5, "useClass: WebTypedInterceptor,");
				sbRootIndex.AppendLine(5, "multi: true");
				sbRootIndex.AppendLine(4, "},");
				sbRootIndex.AppendLine(4, "WebTypedEventEmitterService,");
				sbRootIndex.AppendLine(4, "...serviceTypes");
				sbRootIndex.AppendLine(3, "]");
				sbRootIndex.AppendLine(2, "};");
				sbRootIndex.AppendLine(1, "}");
				sbRootIndex.AppendLine("}");
			}

			var rootIndexFile = Path.Combine(Options.OutDir, "index.ts");
			outputManager(rootIndexFile, sbRootIndex.ToString());
		}

		public Dictionary<string, string> GenerateOutputs() {
			var result = new Dictionary<string, string>();
			Process((file, content) => result.Add(file, content));
			return result;
		}

		public async Task SaveAllAsync() {
			var currentFiles = Directory.GetFiles(Options.OutDir, "*.ts", SearchOption.AllDirectories);
			List<Task<string>> tasks = new List<Task<string>>();
			Process((file, content) => tasks.Add(FileHelper.WriteAsync(file, content)));
			if (Options.Clear) {
				Console.WriteLine("Webtyped - Clearing invalid files");
				//currentFiles.ToList().ForEach(f => Console.WriteLine(f));
				var files = new HashSet<string>();
				foreach (var t in tasks) {
					files.Add(await t);
				}
				//Console.Write("new files");
				//files.ToList().ForEach(f => Console.WriteLine(f));
				var delete = currentFiles.Except(files, StringComparer.InvariantCultureIgnoreCase);
				//delete.ToList().ForEach(f => Console.WriteLine(f));
				//Console.WriteLine($"celete: {string.Join(", ", delete)}");
				foreach (var f in delete) {
					var line = File.ReadLines(f).First();
					var mark = FileHelper.Mark.Replace(System.Environment.NewLine, "");
					//Console.WriteLine($"{line} == {FileHelper.Mark} - {line.CompareTo(FileHelper.Mark.Replace(System.Environment.NewLine, ""))}");
					//if (line.ToUpper().Contains(FileHelper.Mark.ToUpper())) {
					if (line == mark) {
						Console.WriteLine($"deleting {f}");
						File.Delete(f);
					}
				}
				DeleteEmptyDirs(Options.OutDir);
			}
		}

		static void DeleteEmptyDirs(string dir) {
			if (String.IsNullOrEmpty(dir))
				throw new ArgumentException(
					"Starting directory is a null reference or an empty string",
					"dir");

			try {
				foreach (var d in Directory.EnumerateDirectories(dir)) {
					DeleteEmptyDirs(d);
				}

				var entries = Directory.EnumerateFileSystemEntries(dir);

				if (!entries.Any()) {
					try {
						Directory.Delete(dir);
					} catch (UnauthorizedAccessException) { } catch (DirectoryNotFoundException) { }
				}
			} catch (UnauthorizedAccessException) { }
		}
	}
}
