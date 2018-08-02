﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace WebTyped.Cli {

	class Program {

		const string CONFIG_FILE_NAME = "webtyped.json";
		//List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
		static async Task<int> Main(string[] args) {
			if (!File.Exists(CONFIG_FILE_NAME)) {
				Console.WriteLine($"WebTyped configuration file not found (\u001b[31m{CONFIG_FILE_NAME}\u001b[0m)");
				return 1;
			}

			if (args.Any() && args[0] == "watch") {
				Console.WriteLine($" \u001b[1;35m WebTyped is watching\u001b[0m");
				var config = await ReadConfigAsync();
				var matcher = new Matcher();
				foreach (var val in config.Files) {
					matcher.AddInclude(val);
				}
				var csFiles = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory());
				var directories = csFiles.Select(cs => Path.GetDirectoryName(cs)).Distinct();
				var observables = new List<IObservable<EventPattern<FileSystemEventArgs>>>();
				foreach (var d in directories) {
					var watcher = new FileSystemWatcher(d) {
						EnableRaisingEvents = true,
						IncludeSubdirectories = true
					};
					var obs = Observable
						.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
							h => watcher.Changed += h,
							h => watcher.Changed -= h);
					observables.Add(obs);
					obs = Observable
						.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
							h => watcher.Renamed += h,
							h => watcher.Renamed -= h);
					observables.Add(obs);
				}
				Observable.Merge(observables)
					.Where(e => {
						var files = GetFilePathsAsync().Result;
						return files.Contains(e.EventArgs.FullPath);
					})
					.Throttle(new TimeSpan(0, 0, 1))
					.Subscribe(async e => {
						Console.WriteLine($"{e.EventArgs.FullPath} changed");
						await Execute();
					});

				await Execute();
				while (true) { }
			}

			return await Execute();
		}

		private static void Watcher_Changed(object sender, FileSystemEventArgs e) {
			throw new NotImplementedException();
		}

		static async Task<Config> ReadConfigAsync() {
			var configText = await File.ReadAllTextAsync(CONFIG_FILE_NAME);
			var config = JsonConvert.DeserializeObject<Config>(configText);
			return config;
		}
		static async Task<HashSet<string>> GetFilePathsAsync() {
			var config = await ReadConfigAsync();
			var matcher = new Matcher();
			foreach (var val in config.Files) {
				matcher.AddInclude(val);
			}
			return matcher.GetResultsInFullPath(Directory.GetCurrentDirectory()).ToHashSet();
		}

		static async Task<int> Execute() {
			var dtInit = DateTime.Now;
			var config = await ReadConfigAsync();
			if (config.Files == null || !config.Files.Any()) {
				Console.WriteLine("Source files not provided.");
				return 1;
			}
			var matcher = new Matcher();
			foreach (var val in config.Files) {
				matcher.AddInclude(val);
			}
			var csFiles = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory());


			Console.WriteLine();
			Console.WriteLine($" \u001b[1;36mStarting WebTyped with these configs:\u001b[0m");
			config.GetType().GetProperties().ToList().ForEach(p => {
				var val = p.GetValue(config);
				Console.Write($"  \u001b[1;33m{p.Name}:");
				if (val is IEnumerable<string>) {
					Console.WriteLine();
					foreach (var x in (val as IEnumerable)) {
						Console.WriteLine($" \u001b[37m {x}");
					}
				} else {
					var strVal = "";
					if (val != null && val.GetType().IsEnum) {
						strVal = val.ToString();
					} else {
						strVal = JsonConvert.SerializeObject(p.GetValue(config));
					}

					Console.WriteLine($" \u001b[37m {strVal}");
				}
			});
			Console.WriteLine("\u001b[0m");


			var trees = new ConcurrentDictionary<string, SyntaxTree>();
			var tasks = new List<Task>();
			foreach (var csFile in csFiles) {
				tasks.Add(File.ReadAllTextAsync(csFile)
					.ContinueWith(tsk => {
						trees.TryAdd(csFile, CSharpSyntaxTree.ParseText(tsk.Result));
					}));
			}

			var options = new Options(config.OutDir,
				config.Clear,
				config.ServiceMode,
				config.Trims ?? new string[0],
				config.BaseModule,
				config.KeepPropsCase);

			foreach (var task in tasks) { await task; }
			var gen = new Generator(trees.Values, options);
			await gen.WriteFilesAsync();
			var dtEnd = DateTime.Now;
			Console.WriteLine($"Time: {Math.Truncate((dtEnd - dtInit).TotalMilliseconds)}ms");
			return 0;
		}
	}

	public class Config {
		public IEnumerable<string> Files { get; set; }
		public IEnumerable<string> Trims { get; set; }
		public string OutDir { get; set; }
		public bool Clear { get; set; } = true;
		public ServiceMode ServiceMode { get; set; } = ServiceMode.Fetch;
		public string BaseModule { get; set; }
		public bool KeepPropsCase { get; set; }
	}
}
