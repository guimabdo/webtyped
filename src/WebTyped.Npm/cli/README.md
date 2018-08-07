[![Build status](https://ci.appveyor.com/api/projects/status/github/guimabdo/webtyped?svg=true)](https://ci.appveyor.com/project/guimabdo/webtyped)
# WebTyped [![Latest version](https://img.shields.io/npm/v/@guimabdo/webtyped-common.svg)](https://www.npmjs.com/search?q=@guimabdo/webtyped)

 WebTyped is a tool for generating strongly typed TypeScript code from your http://ASP.NET or http://ASP.NET/core Web Apis.

## Quick Start

```
npm install @guimabdo/webtyped -g
npm install @guimabdo/webtyped-common
npm install @guimabdo/webtyped-[fetch|jquery|angular|angular4]

```

Create a webtyped.json configuration file in your project.
Example:

```javascript
{
	"files": [
		"../Controllers/**/*.cs",
		"../Models/**/*.cs"
	],
	"outDir": "./webtyped/", //optional, default: "webtyped",
	"serviceMode": "angular", //optional, default: "fetch", current options: "fetch", "angular", "angular4" or "jquery"
	"trims": ["My.Namespace"], //optional
	"baseModule": "WebApis", //optional
	"keepPropsCase": false, //optional, default: false. May be useful with old versions of Asp.Net WebApi
	"clear": true //optional, default: true. Delete typescript files that are not part of the current generation
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
