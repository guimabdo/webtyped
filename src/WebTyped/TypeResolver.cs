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
	public class TypeResolver {
		public IEnumerable<Service> Services { get; private set; } = new List<Service>();
		public IEnumerable<TsModelBase> Models { get; private set; } = new List<TsModelBase>();
		public Dictionary<ITypeSymbol, ITsFile> TypeFiles { get; private set; } = new Dictionary<ITypeSymbol, ITsFile>();
		Options Options { get; set; }
		public TypeResolver(Options options) {
			this.Options = options;
		}
		//public void Add(Service service) {
		//	this.TypeFiles[service.TypeSymbol] = service;
		//	(this.Services as List<Service>).Add(service);
		//}
		//public void Add(Model model) {
		//	this.TypeFiles[model.TypeSymbol] = model;
		//	(this.Models as List<Model>).Add(model);
		//}

		public void Add(ITsFile file) {
			this.TypeFiles[file.TypeSymbol] = file;
			if(file is TsModelBase) {
				(this.Models as List<TsModelBase>).Add(file as TsModelBase);
			}
			if (file is Service) {
				(this.Services as List<Service>).Add(file as Service);
			}
		}
		public string Resolve(ITypeSymbol typeSymbol) {
			if (TypeFiles.TryGetValue(typeSymbol, out var tsType)) {
				return tsType.FullName;
			}

			var type = typeSymbol as INamedTypeSymbol;
			//tuples
			if (type.IsTupleType) {
				return $"{{{string.Join(", ", type.TupleElements.Select(te => $"{te.Name}: {Resolve(te.Type as INamedTypeSymbol)}"))}}}";
			}

			string name = type.Name;
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

			string genericPart = "";
			//Generic
			if (type.IsGenericType) {
				genericPart = $"<{string.Join(", ", type.TypeArguments.Select(t => Resolve(t as INamedTypeSymbol)))}>";
			}

			//Change type to ts type
			typeName = ToTsTypeName(typeName);

			return $"{typeName}{genericPart}";
		}

		string ToTsTypeName(string typeName) {
			switch (typeName) {
				case nameof(Boolean):
					return "boolean";
				case nameof(DateTime):
				case nameof(String):
					return "string";
				case nameof(Int32):
				case nameof(Int16):
				case nameof(Int64):
					return "number";
				case nameof(IEnumerable):
				case nameof(List<object>):
					return "Array";
				default: return typeName;
			}
		}

		public async Task SaveAllAsync() {
			//HashSet<string> files = new HashSet<string>();
			List<Task<string>> tasks = new List<Task<string>>();
			//foreach(var m in Models) { files.Add(m.Save()); }
			//foreach (var s in Services) { files.Add(s.Save()); }
			foreach (var m in Models) {
				tasks.Add(m.SaveAsync());
			}
			foreach (var s in Services) {
				tasks.Add(s.SaveAsync());
			}
			//Create indexes
			//Create root index
			var sbRootIndex = new StringBuilder();
			sbRootIndex.AppendLine("import { WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';");

			//Create index for each module folder
			var serviceModules = Services.GroupBy(s => s.Module).Distinct();
			var counter = 0;
			var services = new List<string>();
			services.Add("WebApiEventEmmiterService");
			foreach (var sm in serviceModules) {
				sbRootIndex.AppendLine($"import * as mdl{counter} from './{sm.Key}'");
				var sbServiceIndex = string.IsNullOrEmpty(sm.Key) ? sbRootIndex : new StringBuilder();
				foreach (var s in sm) {
					sbServiceIndex.AppendLine($"export * from './{s.FilenameWithoutExtenstion}';");
					services.Add($"mdl{counter}.{s.ClassName}");
				}

				if (!string.IsNullOrEmpty(sm.Key)) {
					var servicesDir = Path.Combine(Options.OutDir, sm.Key);
					var serviceIndexFile = Path.Combine(servicesDir, "index.ts");
					//files.Add(serviceIndexFile);
					//await FileHelper.WriteAsync(serviceIndexFile, sbServiceIndex.ToString());
					tasks.Add(FileHelper.WriteAsync(serviceIndexFile, sbServiceIndex.ToString()));
				}
				counter++;
			}
			sbRootIndex.AppendLine("export var providers = [");
			sbRootIndex.AppendLine(1, string.Join($",{System.Environment.NewLine}	", services));
			sbRootIndex.AppendLine("]");
			var rootIndexFile = Path.Combine(Options.OutDir, "index.ts");
			//files.Add(rootIndexFile);
			//await FileHelper.WriteAsync(rootIndexFile, sbRootIndex.ToString());
			tasks.Add(FileHelper.WriteAsync(rootIndexFile, sbRootIndex.ToString()));


			if (Options.Clear) {
				var currentFiles = Directory.GetFiles(Options.OutDir, "*.ts", SearchOption.AllDirectories);
				var files = new HashSet<string>();
				foreach (var t in tasks) {
					files.Add(await t);
				}
				//Console.WriteLine($"current: {string.Join(", ", currentFiles)}");
				//Console.WriteLine($"new: {string.Join(", ", files)}");
				var delete = currentFiles.Except(files);
				//Console.WriteLine($"celete: {string.Join(", ", delete)}");
				foreach (var f in delete) {
					if (File.ReadLines(f).First().StartsWith(FileHelper.Mark)) {
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
