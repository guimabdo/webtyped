using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebTyped.Annotations;

namespace WebTyped.Tests {

	[TestClass]
	public class UnitTest1 {
		Options GetCommonOptions() {
			return new Options(".\\", false, ServiceMode.Angular, new string[0], "", false, null);
		}

		async Task<Dictionary<string, string>> Generate(params string[] cs) {
			var generator = new Generator(cs, GetCommonOptions());
			return await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task DictionaryTest() {
			var cs =
@"
using System.Collections.Generic;
public class Dicts {
	public Dictionary<string, string> TsSupported1 { get; set; }
	public Dictionary<int, string> TsSupported2 { get; set; }
	public Dictionary<object, string> TsNotSupported1 { get; set; }
}";
			var outputs = await Generate(cs);


		}

		[TestMethod]
		public async Task TypingsScopeTest() {
			var cs = @"
using System;
using System.Threading.Tasks;
namespace Ns1{
	public class Model { 
		public string Prop1 { get; set; }
	}
}
namespace Ns2 {
	public class Model : Ns1.Model { 
		public string Prop2 { get; set; }
		public Ns3.MyEnum Prop3 { get; set; }
	}
}

public class Model : Ns2.Model { }

namespace Ns3{
	public enum MyEnum{
		A, B, C
	}
	public class LocalModel : Ns2.Model { }

	[Route(""api/[controller]"")]
	public class MyController {
		[HttpPost]
		public async Task<Ns1.Model> SomeMethod([FromBody]Ns2.Model val){ return null; }

		[HttpPost]
		public async Task<LocalModel> SomeMethod2([FromBody]Model val){ return null; }
	}
}
";
			var generator = new Generator(
	new string[] { cs },
	GetCommonOptions()
);
			var outputs = await generator.GenerateOutputsAsync();
		}

		[TestMethod]
		public async Task DateTimeOffsetShouldBeStringTest() {
			var cs = @"
using System;
public class Model { 
	public DateTimeOffset DateAt { get; set; }
}";
			var generator = new Generator(
	new string[] { cs },
	GetCommonOptions()
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
			var tr = new TypeResolver(GetCommonOptions());
			var kvpType = typeof(KeyValuePair<,>);
			var assembly = MetadataReference.CreateFromFile(kvpType.Assembly.Location);
			var compilation = CSharpCompilation.Create("CompTest", references: new[] { assembly });
			//kvpType = typeof(KeyValuePair<int, string>);
			var typeSymbol = compilation.GetTypeByMetadataName(kvpType.FullName);
			typeSymbol = typeSymbol.Construct(
				compilation.GetTypeByMetadataName(typeof(int).FullName),
				compilation.GetTypeByMetadataName(typeof(string).FullName)
			);
			var name = tr.Resolve(typeSymbol, new ResolutionContext(null)).Name;
			Assert.AreEqual(name, "{ key: number, value: string }");
		}


		[TestMethod]
		public async Task UnknowInheritanceTest() {
			var cs = @"public class Model: Interface {}";
			var generator = new Generator(
		new string[] { cs },
		GetCommonOptions()
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
	GetCommonOptions()
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
		GetCommonOptions()
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
				@"import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class MyService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/my', httpClient, eventEmitter);
	}
	getArray: MyService.GetArrayFunction = () : Observable<Array<number>> => {
		return this.invokeGet({
				kind: 'GetArray',
				func: this.getArray,
				parameters: { _wtKind: 'GetArray' }
			},
			``,
			undefined
		);
	};
}
export namespace MyService {
	export type GetArrayParameters = {_wtKind: 'GetArray' };
	export interface GetArrayCallInfo extends WebTypedCallInfo<GetArrayParameters, Array<number>> { kind: 'GetArray'; }
	export type GetArrayFunctionBase = () => Observable<Array<number>>;
	export interface GetArrayFunction extends WebTypedFunction<GetArrayParameters, Array<number>>, GetArrayFunctionBase {}
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

	[HttpGet(""sync"")]
	public byte[] GetArraySync(){ return null; }

}";
			var generator = new Generator(
		new string[] { cs },
		GetCommonOptions()
	);
			var outputs = await generator.GenerateOutputsAsync();
			Assert.AreEqual(
				@"import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class MyService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/my', httpClient, eventEmitter);
	}
	getArray: MyService.GetArrayFunction = () : Observable<string> => {
		return this.invokeGet({
				kind: 'GetArray',
				func: this.getArray,
				parameters: { _wtKind: 'GetArray' }
			},
			``,
			undefined
		);
	};
	getArraySync: MyService.GetArraySyncFunction = () : Observable<string> => {
		return this.invokeGet({
				kind: 'GetArraySync',
				func: this.getArraySync,
				parameters: { _wtKind: 'GetArraySync' }
			},
			`sync`,
			undefined
		);
	};
}
export namespace MyService {
	export type GetArrayParameters = {_wtKind: 'GetArray' };
	export interface GetArrayCallInfo extends WebTypedCallInfo<GetArrayParameters, string> { kind: 'GetArray'; }
	export type GetArrayFunctionBase = () => Observable<string>;
	export interface GetArrayFunction extends WebTypedFunction<GetArrayParameters, string>, GetArrayFunctionBase {}
	export type GetArraySyncParameters = {_wtKind: 'GetArraySync' };
	export interface GetArraySyncCallInfo extends WebTypedCallInfo<GetArraySyncParameters, string> { kind: 'GetArraySync'; }
	export type GetArraySyncFunctionBase = () => Observable<string>;
	export interface GetArraySyncFunction extends WebTypedFunction<GetArraySyncParameters, string>, GetArraySyncFunctionBase {}
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
		GetCommonOptions()
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
		GetCommonOptions()
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
			GetCommonOptions()
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

		[HttpGet(""get2"")]
		public void Get([FromUri]int id){}
    }
}
";

			var generator = new Generator(
				new string[] { cs },
				GetCommonOptions()
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
				GetCommonOptions()
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

		[TestMethod]
		public async Task GetQueryModelTest() {
			var cs =
@"
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public class QueryModel1{
	[FromRoute(Name = ""c"")]
	public string Prop1 { get; set; }
	[FromQuery(Name = ""$d"")]
	public string Prop2 { get; set; }
	public string Prop3 { get; set; }
}

public class QueryModel2{
	[FromRoute(Name = ""a"")]
	public string Prop1 { get; set; }
	[FromQuery(Name = ""$b"")]
	public string Prop2 { get; set; }
	public string Prop3 { get; set; }
}

[Route(""api/[controller]"")]
public class MyController {
	[HttpGet]
	public async Task<object> GetArray(QueryModel1 q1, QueryModel2 q2){ return null; }
}";
			var generator = new Generator(new string[] { cs }, GetCommonOptions());
			var outputs = await generator.GenerateOutputsAsync();

		}

		//[TestMethod]
		//public void RegexRouteTest() {
		//	string route = "teste/{count:regex(\\$count)?}{bla?}";
		//	var regex = new Regex(@"\{(?<paramName>\w+)(:\w+(\(.*?\))?)?\??}");
		//	string result = regex.Replace(route, m => {
		//		return $"{{{m.Groups["paramName"].Value}}}";
		//	});


		//}
	}
}
