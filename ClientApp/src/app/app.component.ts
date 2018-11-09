/// <reference path="../../node_modules/monaco-editor/monaco.d.ts"/>
import { Component } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormGroup, FormBuilder } from '@angular/forms';
let monaco$ = new BehaviorSubject(null);
let ga$ = new BehaviorSubject(null);
let interval = setInterval(() => {
  if (window['monaco']) {
    monaco$.next(window['monaco']);
    clearInterval(interval);
  }
});
let intervalGa = setInterval(() => {
  if (window['ga']) {
    ga$.next(window['ga']);
    clearInterval(intervalGa);
  }
});
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  editor: monaco.editor.IStandaloneCodeEditor;
  editorResult: monaco.editor.IStandaloneCodeEditor;
  lastSelectionPath: string = null;
  result = { "files": [{ "path": "./typings\\myModel2.d.ts", "content": "declare interface MyModel2 {\r\n\tprop1: number;\r\n\tprop2: string;\r\n\tprop3: MyEnum;\r\n\tprop4: string;\r\n\tprop5: Array<string>;\r\n\tprop6: MyModel2;\r\n}\r\n" }, { "path": "./typings\\myModel1.d.ts", "content": "declare interface MyModel1 {\r\n}\r\n" }, { "path": "./typings\\myEnum.d.ts", "content": "declare const enum MyEnum {\r\n\tA = 0,\r\n\tB = 1,\r\n\tC = 2,\r\n\tD = 3,\r\n\tE = 4,\r\n}\r\n" }, { "path": "./my.service.ts", "content": "import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';\r\nimport { WebTypedClient } from '@guimabdo/webtyped-fetch';\r\nexport class MyService extends WebTypedClient {\r\n\tconstructor(baseUrl: string = WebTypedClient.baseUrl) {\r\n\t\tsuper(baseUrl, 'api/my');\r\n\t}\r\n\tsave: MyService.SaveFunction = (model: MyModel2) : Promise<void> => {\r\n\t\treturn this.invokePost({\r\n\t\t\t\tkind: 'Save',\r\n\t\t\t\tfunc: this.save,\r\n\t\t\t\tparameters: { model, _wtKind: 'Save' }\r\n\t\t\t},\r\n\t\t\t``,\r\n\t\t\tnull,\r\n\t\t\t{ model }\r\n\t\t);\r\n\t};\r\n}\r\nexport namespace MyService {\r\n\texport type SaveParameters = {model: MyModel2, _wtKind: 'Save' };\r\n\texport interface SaveCallInfo extends WebTypedCallInfo<SaveParameters, void> { kind: 'Save'; }\r\n\texport type SaveFunctionBase = (model: MyModel2) => Promise<void>;\r\n\texport interface SaveFunction extends WebTypedFunction<SaveParameters, void>, SaveFunctionBase {}\r\n}\r\n" }, { "path": "./index.ts", "content": "import * as mdl0 from './'\r\nexport * from './my.service';\r\nexport var serviceTypes = [\r\n\tmdl0.MyService\r\n]\r\n" }] };
  selectedItem = this.result.files[0];
  formGroup: FormGroup;
  transpiling = false;
  constructor(private http: HttpClient, fb: FormBuilder) {
    this.formGroup = fb.group({
      serviceMode: ['fetch']
    });
  }
  ngAfterViewInit() {
    ga$.subscribe(ga => {
      if (ga) {
        ga('create', 'UA-126693951-1', 'auto');
      }
    });
    monaco$.subscribe(m => {
      if (!m) { return; }
      
      this.editor = monaco.editor.create(document.getElementById('editor'), {
        value:
          `using System;
using System.Threading.Tasks;

public enum MyEnum{
    A, B, C, D, E
}

public class MyModel1{

}

public class MyModel2 {
    public int Prop1 {get;set;}
    public string Prop2 {get;set;}
    public MyEnum Prop3 {get;set;}
    public DateTime Prop4 {get;set;}
    public string[] Prop5{get;set;}
    public MyModel2 Prop6{get;set;}
}



[Route("api/[controller]")]
public class MyController {
    [HttpPost]
    public async Task Save(MyModel2 model){

    }
}
`,
        language: 'csharp',
        minimap: {
          enabled: false
        }
      });

      this.editorResult = monaco.editor.create(document.getElementById('result'), {
        value: ``,
        language: 'typescript',
        readOnly: true,
        minimap: {
          enabled: false
        }
      });
      this.select(this.result.files[0], false);
    });
  }
  select(ts, emitEvent: boolean = true) {
    console.log(emitEvent);
    if (!ts) { this.editorResult.setValue(''); return; }
    this.editorResult.setValue(ts.content);
    this.lastSelectionPath = ts.path;
    this.selectedItem = ts;
    if (emitEvent) {
      console.log('sending ga');
      ga('send', 'event', 'Playground', 'SelectFile', ts.path,);
    }
    return false;
  }
  async transpile() {
    
    ga('send', 'event', 'Playground', 'Transpile');
    //https://webtyped-functions.azurewebsites.net/api/HttpTrigger2?code=myGYx/xa0lpLhskRGlZWm7vnX2RIKv1GNWQXB38KQb0RHd9YGEcliQ==
    this.transpiling = true;
    var startDate = new Date();
    try {
      this.result = await this.http.post<any>(
        'https://webtyped-functions.azurewebsites.net/api/HttpTrigger2?code=myGYx/xa0lpLhskRGlZWm7vnX2RIKv1GNWQXB38KQb0RHd9YGEcliQ==',
        {
          files: [
            {
              path: 'Classes.cs',
              content: this.editor.getValue()
            }
          ],
          config: this.formGroup.value
        },
        {
          headers: new HttpHeaders({
            'Content-Type': 'application/json'
          })
        }
      ).toPromise();
      var endDate = new Date();
      ga('send', 'event', 'Playground', 'Transpiled', null, (endDate.getTime() - startDate.getTime()) / 1000);
      let file = this.result.files.find(f => f.path == this.lastSelectionPath);
      file = file || this.result.files[0];
      this.select(file, false);
    } finally {
      this.transpiling = false;
    }
  }
}
