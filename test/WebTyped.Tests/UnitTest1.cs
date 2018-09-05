using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebTyped.Annotations;

namespace WebTyped.Tests {
	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public async Task DateTimeOffsetShouldBeStringTest() {
			var cs = @"
using System;
public class Model { 
	public DateTimeOffset DateAt { get; set; }
}";
			var generator = new Generator(
	new string[] { cs },
	new Options(null, false, ServiceMode.Angular, new string[0], "", false)
);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
@"declare interface Model {
	dateAt: string;
}
", outputs[@".\typings\model.d.ts"]);
		}

		[TestMethod]
		public void KeyValuePairToTsTest() {
			var tr = new TypeResolver(new Options("", false, ServiceMode.Angular, new string[0], "", false));
			var kvpType = typeof(KeyValuePair<,>);
			var assembly = MetadataReference.CreateFromFile(kvpType.Assembly.Location);
			var compilation = CSharpCompilation.Create("CompTest", references: new[] { assembly });
			//kvpType = typeof(KeyValuePair<int, string>);
			var typeSymbol = compilation.GetTypeByMetadataName(kvpType.FullName);
			typeSymbol = typeSymbol.Construct(
				compilation.GetTypeByMetadataName(typeof(int).FullName),
				compilation.GetTypeByMetadataName(typeof(string).FullName)
			);
			var name = tr.Resolve(typeSymbol, new ResolutionContext()).Name;
			Assert.AreEqual(name, "{ key: number, value: string }");
		}


		[TestMethod]
		public async Task UnknowInheritanceTest() {
			var cs = @"public class Model: Interface {}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
@"declare interface Model /*extends Interface*/{
}
",
				outputs[@".\typings\model.d.ts"]);
		}

		[TestMethod]
		public async Task EnumSignatureTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
[Route(""api/[controller]"")]
public class MyController {
	[HttpPost]
	public async Task SomeMethod([FromBody]TestEnum val){ return null; }
}

public enum TestEnum{
	Cat = 1,
	Dog = 2
}
";
			var generator = new Generator(
	new string[] { cs },
	new Options(null, false, ServiceMode.Angular, new string[0], "", false)
);
			var outputs = await generator.GenerateOutputsAsync();
		}


		[TestMethod]
		public async Task ArrayResolutionTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
[Route(""api/[controller]"")]
public class MyController {
	[HttpGet]
	public async Task<int[]> GetArray(){ return null; }
}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
				@"import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs/Observable';
@Injectable()
export class MyService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/my', httpClient, eventEmitter);
	}
	getArray = () : Observable<Array<number>> => {
		return this.invokeGet<Array<number>>({
				func: this.getArray,
				parameters: {  }
			},
			``,
			undefined
		);
	};
}
",
				outputs[@".\my.service.ts"]);
		}

		[TestMethod]
		public async Task ByteArrayResolutionTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
[Route(""api/[controller]"")]
public class MyController {
	[HttpGet]
	public async Task<byte[]> GetArray(){ return null; }
}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
				@"import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs/Observable';
@Injectable()
export class MyService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/my', httpClient, eventEmitter);
	}
	getArray = () : Observable<string> => {
		return this.invokeGet<string>({
				func: this.getArray,
				parameters: {  }
			},
			``,
			undefined
		);
	};
}
",
				outputs[@".\my.service.ts"]);
		}

		[TestMethod]
		public async Task GenericClassTest() {
			var cs = @"public class GenericClass<T>{ 
	public T Id { get; set; } 
}";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.IsTrue(outputs.First().Value.Contains(
@"declare interface GenericClassOf1<T> {
	id: T;
}
"
));
		}

		[TestMethod]
		public async Task GenericAndInheritedClassWithSameNameTest() {
			var cs = @"public class GenericClass<T>{ 
	public T Id { get; set; } 
}

public class GenericClass : GenericClass<int>{}

";
			var generator = new Generator(
		new string[] { cs },
		new Options(null, false, ServiceMode.Angular, new string[0], "", false)
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.IsTrue(outputs[@".\typings\genericClass.d.ts"] ==
@"declare interface GenericClass extends GenericClassOf1<number> {
}
");

			Assert.IsTrue(outputs[@".\typings\genericClassOf1.d.ts"] ==
@"declare interface GenericClassOf1<T> {
	id: T;
}
");
		}

		//[TestMethod]
		//public void TestMethod1() {
		//	var folder = Directory.GetCurrentDirectory();
		//	var result = Program.Main(new[] {
		//		"generate",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/Controllers/**/*.cs",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/Models/**/*.cs",
		//		"--sourceFiles", "../../../../WebTyped.Example.Web/OtherModels/**/*.cs",
		//		"--outDir", "../../../../WebTyped.Example.Web/ClientApp/app/webApiUnitTest/",
		//		"--trim", "WebTyped_Example_Web.Services",
		//		"--trim", "WebTyped.Example.Web",
		//		"--trim", "WebTyped.Example.Web.Models",
		//		"--baseModule", "UnitTest",
		//		"--clear"
		//		//"--controllers", "../../../UnitTest1.cs",
		//		//"--models", "../../../UnitTest1.cs",
		//	});
		//	Assert.AreEqual(0, result);
		//}

		[TestMethod]
		public async Task ConstTest() {
			var cs =
@"
namespace Test {
	public static class Consts {
		public static class SomeModelMetadata{
			public static class SomeField1 {
				public const string DISPAY_NAME = ""Name of Field"";
				public const int MAX_LENGTH = 128;
			}

			public static class SomeField2 {
				public const string DISPAY_NAME = ""Name of Field"";
				public const int MAX_LENGTH = 128;
			}
		}
    }
}
";
			var generator = new Generator(
			new string[] { cs },
			new Options(null, false, ServiceMode.Angular, new string[0], "", false)
		);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task FromNamedUriQueryTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
using System.Web.Http;
namespace Test{
	[Route(""api/[controller]"")]
	public class TestController {
		[HttpGet]
		public void Get([FromUri(Name = ""$id"")]int id){}
    }
}
";

			var generator = new Generator(
				new string[] { cs },
				new Options(null, false, ServiceMode.Angular, new string[0], "", false)
			);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task TupleTest() {
			var cs =
@"
using System;
using WebTyped.Annotations;
using System.Threading.Tasks;
namespace Test{
	public class ModelWithTuples {
		public (int, string) Tuple1 { get; set; }
		public (int id, string name) Tuple2 { get; set; }
	}

	[Route(""api/[controller]"")]
	public class ApiWithTupleController {
		[HttpPost]
		public void Post([FromBody][NamedTuple](int id, string name) param){}

		[HttpGet]
		public void Get([NamedTuple](int id, string name) param){}

		[HttpPost]
		public void Post2([FromBody](int id, string name) param){}

		[HttpGet]
		public void Get2((int id, string name) param){}

		[HttpGet]
		public void GetErr([NamedTuple]string param){}
			
		[HttpGet]
		public (int id, string name) Get4([NamedTuple](int id, string name) param){}
    }
}
";

			var generator = new Generator(
				new string[] { cs },
				new Options(null, false, ServiceMode.Angular, new string[0], "", false)
			);
			var outputs = await generator.GenerateOutputsAsync();

			Assert.AreEqual(
				@"declare module Test {
	interface ModelWithTuples {
		tuple1: {/** field:item1 */item1: number, /** field:item2 */item2: string};
		tuple2: {/** field:id */item1: number, /** field:name */item2: string};
	}
}
",
				outputs[@".\typings\Test.modelWithTuples.d.ts"]);

			Assert.AreEqual(
			@"import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs/Observable';
@Injectable()
export class ApiWithTupleService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/apiWithTuple', httpClient, eventEmitter);
	}
	post = (param: {/** tuple field:item1 */id: number, /** tuple field:item2 */name: string}) : Observable<void> => {
		return this.invokePost<void>({
				func: this.post,
				parameters: { param }
			},
			``,
			function(__source) { return { item1: __source.id, item2: __source.name  } }(param),
			undefined
		);
	};
	get = (param: {/** tuple field:item1 */id: number, /** tuple field:item2 */name: string}) : Observable<void> => {
		return this.invokeGet<void>({
				func: this.get,
				parameters: { param }
			},
			``,
			{ param: function(__source) { return { item1: __source.id, item2: __source.name  } }(param) }
		);
	};
	post2 = (param: {/** field:id */item1: number, /** field:name */item2: string}) : Observable<void> => {
		return this.invokePost<void>({
				func: this.post2,
				parameters: { param }
			},
			``,
			param,
			undefined
		);
	};
	get2 = (param: {/** field:id */item1: number, /** field:name */item2: string}) : Observable<void> => {
		return this.invokeGet<void>({
				func: this.get2,
				parameters: { param }
			},
			``,
			{ param }
		);
	};
	getErr = (param: string) : Observable<void> => {
		return this.invokeGet<void>({
				func: this.getErr,
				parameters: { param }
			},
			``,
			{ [UNSUPPORTED - NamedTupleAttribute must be used only for tuple parameters] }
		);
	};
	get4 = (param: {/** tuple field:item1 */id: number, /** tuple field:item2 */name: string}) : Observable<{/** field:id */item1: number, /** field:name */item2: string}> => {
		return this.invokeGet<{/** field:id */item1: number, /** field:name */item2: string}>({
				func: this.get4,
				parameters: { param }
			},
			``,
			{ param: function(__source) { return { item1: __source.id, item2: __source.name  } }(param) }
		);
	};
}
",
			outputs[@".\test\apiWithTuple.service.ts"]);
		}

	}
}
