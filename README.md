[![Build status](https://ci.appveyor.com/api/projects/status/github/guimabdo/webtyped?svg=true)](https://ci.appveyor.com/project/guimabdo/webtyped) [![Latest version](https://img.shields.io/npm/v/@guimabdo/webtyped-common.svg)](https://www.npmjs.com/search?q=@guimabdo/webtyped)

# WebTyped

 WebTyped is a tool for generating strongly typed TypeScript code from your http://ASP.NET or http://ASP.NET/core Web Apis.

## Quick Start

```
npm install @guimabdo/webtyped-generator
npm install @guimabdo/webtyped-[fetch|jquery|angular]

```

webpack.config.js:

```javascript
const WebTypedPlugin = require('@guimabdo/webtyped-generator').WebTypedPlugin;
module.exports = {
   plugins: [
		  new WebTypedPlugin({
			  sourceFiles: [
				   "./Controllers/Api/**/*.cs",
				   "./Models/**/*.cs"],
			  serviceMode: "fetch", //or "jquery", or "angular"
			  outDir: "./src/webtyped/",
			  clear: true
		})
	  ]
}
```

Run webpack and use generated services wherever you want:

```typescript
import { MyService } from './webtyped/<services-folder>';
let myService = new MyService(); //Generated from MyController.cs
myService.get().then(result => console.log(result));
```

### Angular? Import the generated module and inject services when needed:

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


