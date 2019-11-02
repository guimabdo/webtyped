[![Build status](https://ci.appveyor.com/api/projects/status/github/guimabdo/webtyped?svg=true)](https://ci.appveyor.com/project/guimabdo/webtyped)
# WebTyped [![Latest version](https://img.shields.io/npm/v/@guimabdo/webtyped.svg)](https://www.npmjs.com/search?q=@guimabdo/webtyped)

 WebTyped is a tool for generating strongly typed TypeScript code from your http://ASP.NET or http://ASP.NET/core Web Apis.

## Quick Start

```
npm install @guimabdo/webtyped -g
npm install @guimabdo/webtyped-common

```

Create a webtyped.json configuration file in your project.
Example:

```javascript
{
	"files": [
		"../Controllers/**/*.cs",
		"../Models/**/*.cs"
	]
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
import { WebTypedFetchInvoker } from "@guimabdo/webtyped-common";

let inv = new WebTypedFetchInvoker(<api-base-url>);
let myService = new MyService(inv); //Generated from MyController.cs
myService.get().then(result => console.log(result));
```

### Angular

```
npm install @guimabdo/webtyped-angular

```

webtyped.json

```javascript
{
	"files": [
		"../Controllers/**/*.cs",
		"../Models/**/*.cs"
	],
	"inject": {
		"beforeServiceClass": [
			"import { Injectable } from '@angular/core';",
			"@Injectable({ providedIn: 'root' })"
		]
	},
}
```

Import the generated module and inject services when needed:

app.module.ts

```typescript

import { WebTypedNgModule } from '@guimabdo/webtyped-angular';
@NgModule({
	imports: [
		WebTypedNgModule.forRoot()
	],
	//Optionally set api base. Default is './'
	providers: [
		{
			provide: 'API_BASE_URL',
			useValue: '<url>'
		}
	]
})
export class AppModule {}

```

Usage:

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