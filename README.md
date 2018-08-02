[![Build status](https://ci.appveyor.com/api/projects/status/github/guimabdo/webtyped?svg=true)](https://ci.appveyor.com/project/guimabdo/webtyped)
# WebTyped [![Latest version](https://img.shields.io/npm/v/@guimabdo/webtyped-common.svg)](https://www.npmjs.com/search?q=@guimabdo/webtyped)

 WebTyped is a tool for generating strongly typed TypeScript code from your http://ASP.NET or http://ASP.NET/core Web Apis.

## Quick Start

```
npm install @guimabdo/webtyped
npm install @guimabdo/webtyped-[fetch|jquery|angular]

```

Create a webtyped.json configuration file in your project.
Example:

```javascript
{
	"files": [
		"../Controllers/**/*.cs",
		"../Models/**/*.cs"
	],
	"outDir": "./webtyped/", //optional, default: "./",
	"serviceMode": "angular", //optional, default: "fetch", current options: "fetch", "angular" or "jquery"
	"trims": ["My.Namespace"], //optional
	"baseModule": "WebApis", //optional
	"keepPropsCase": false, //options, default: false. May be useful with old versions of Asp.Net WebApi
	"clear": true // delete typescript files that are not part of the current generation
}

```

At the command line, run webtyped:

```batchfile
webtyped
```
Or use 'watch' option for generating typescript code and start watching cs files:

```batchfile
webtyped watch
```

Use generated services wherever you want:

```typescript
import { MyService } from './webtyped/<services-folder>';
let myService = new MyService(); //Generated from MyController.cs
myService.get().then(result => console.log(result));
```

### Angular(6+) Import the generated module and inject services when needed:

app.module.ts

```typescript
import { WebTypedGeneratedModule } from './webtyped';
@NgModule({
	imports: [WebTypedGeneratedModule.forRoot()]
})
export class AppModule {}
```

some.component.ts (for example)
```typescript
import { MyService } from './webtyped/<services-folder>';
@Component({...})
export class SomeComponent {
	constructor(myService: MyService){}
}
```

## Requirements

netcore 2.0 on dev machine

# WebTyped.Annotations [![Latest version](https://img.shields.io/nuget/v/WebTyped.Annotations.svg)](https://www.nuget.org/packages/WebTyped.Annotations/)

Attributes for changing services/models generator behaviour.

### ClientTypeAttribute

Use this attribute for mapping a Server Model to an existing Client Type so it won't be transpiled by the generator. 
- typeName: correspondent client type name, or empty if it has the same name as the server type.
- module: type module, leave it empty if the type is globally visible.

Generated API services will know how to resolve the type.

example:
```C#
[WebTyped.Annotations.ClientType(module: "primeng/components/common/selectitem")]
public class SelectItem { 
    public string Label { get; set; }
    public long Value { get; set; }
}
```

### NamedTupleAttribute

Sometimes your application have lots of multiparameted webapis. Instead of creating a Model for each webapi method, you may want to use Named Tuples like this:

```C#
[HttpPost("")]
public void Save([FromBody](name: string, birthdate: DateTime, somethingElse: number) parameters) {[
    ...
}
```

This will be transpiled to the client accordingly to .NET compiled tuple field names (Item1, Item2, Item3, ...), otherwise deserialization will not work when server receives the data. This will result in a non-friendly usage in client:

```typescript
myService.save({ item1: "John", item2: "2010-12-01", item3: 42});
```

Decorating the method parameter with NamedTuple attribute makes the generator create the client function parameter using the original field names. This function will change the parameter field names (to item1, item2...) before sending it to the server. So the usage becomes:

```typescript
myService.save({ name: "John", birthdate: "2010-12-01", somethingElse: 42});
```

