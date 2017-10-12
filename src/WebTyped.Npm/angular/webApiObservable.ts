import { Observable } from 'rxjs';
import { Http, Headers, Response, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { WebApiEventEmmiterService } from './webApiEventEmmiter.service';
import { WebApiCallInfo } from '@guimabdo/webtyped-common';
export class WebApiObservable<T> extends Observable<T>{
    constructor(protected info: WebApiCallInfo,
                protected eventEmmiter: WebApiEventEmmiterService,
                protected http: Http,
                protected httpMethod: string,
                protected basePath: string,
                protected api: string,
                protected action: string,
                protected body: any,
                protected search: any){
        super(sub => {
            var url = `${basePath}/${api}/${action}`;
           
            var fData = "FormData";
            var isFormData = body && typeof (body) === fData;

            //Creating headers
            var headers = new Headers();
            if (!isFormData) { // multiplart header is resolved by the browser
                headers.set("Content-Type", "application/json");
                //If not formadata, stringify
                if (body) {
                    body = JSON.stringify(body);
                }
            }

            //Creating options
            var options: RequestOptionsArgs = {
                headers: headers,
                withCredentials: true //Cross-domain support?
            };

            var currentSearch = Object.assign({}, search);
            //Se tiver querystring
            if (currentSearch) {
                var s: URLSearchParams = new URLSearchParams();
                for (var p in currentSearch) {
                    s.set(p, currentSearch[p]);
                }
                options.search = s;
            }
            var httpObservable: Observable<Response>;
            switch (httpMethod) {
                case 'get':
                    httpObservable = http.get(url, options);
                    break;
                case 'put':
                    httpObservable = http.put(url, body, options);
                    break;
                case 'patch':
                    httpObservable = http.patch(url, body, options);
                    break;
                case 'delete':
                    httpObservable = http.delete(url, options);
                    break;
                case 'post':
                default:
                    httpObservable = http.post(url, body, options);
                    break;
            }
            var coreObs = httpObservable.map(r => {
                //Convert response
                var result: T;
                try {
                    result = r.json();
                } catch (err) { 
                    //plain text
                    var plainText:any = r.text();
                    result = plainText; 
                }
                return result;
            })
                //Emmit completed event
                .do(() => {
                    eventEmmiter.emit(info);
                },
                r => {
                   //Would be appropiate an option to centralize error handling here?
                });

            coreObs.subscribe(r => sub.next(r), err => sub.error(err), () => sub.complete());
        });
    }
}