import { HttpClient, HttpParams } from '@angular/common/http';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
export class WebApiClient {

    constructor(
        private baseUrl: string,
        private api: string,
        private httpClient: HttpClient,
        private eventEmitter: WebTypedEventEmitterService) { }

    invokeGet<T>(info: WebTypedCallInfo, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebTypedCallInfo, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private invoke<T>(info: WebTypedCallInfo, action: string,
        httpMethod: string, body?: any, search?: any): Observable<T> {
        var baseUrl = this.baseUrl || "";
        var httpClient = this.httpClient;
        if (baseUrl.endsWith('/')) { baseUrl = baseUrl.substr(0, baseUrl.length - 1); }
        var url = `${baseUrl}/${this.api}/${action}`;

        //var fData = "FormData";
        //var isFormData = body && typeof (body) === fData;

        ////Creating headers
        //var headers = new HttpHeaders();
        //if (!isFormData) { // multiplart header is resolved by the browser
        //    headers.set("Content-Type", "application/json");
        //    //If not formadata, stringify
        //    if (body) {
        //        body = JSON.stringify(body);
        //    }
        //}

        //Creating options
        var options: { params: undefined | HttpParams } = {
            //headers: headers,
            params: undefined
            //withCredentials: true //Cross-domain support? -- with new HttClient, I think this kind of things should be managed with interceptors
        };

        if (search) {
            var params = new HttpParams();
            for (var p in search) {
                var val = search[p];
                if (val === undefined) { continue; }
                params = params.set(p, search[p]);
            }
            options.params = params;
        }
        //var currentSearch = Object.assign({}, search);
        ////Se tiver querystring
        //if (currentSearch) {
        //    var s: URLSearchParams = new URLSearchParams();
        //    for (var p in currentSearch) {
        //        s.set(p, currentSearch[p]);
        //    }
        //    options.search = s;
        //}
        var httpObservable: Observable<T>;
        switch (httpMethod) {
            case 'get':
                httpObservable = httpClient.get<T>(url, options);
                break;
            case 'put':
                httpObservable = httpClient.put<T>(url, body, options);
                break;
            case 'patch':
                httpObservable = httpClient.patch<T>(url, body, options);
                break;
            case 'delete':
                httpObservable = httpClient.delete<T>(url, options);
                break;
            case 'post':
            default:
                httpObservable = httpClient.post<T>(url, body, options);
                break;
        }

        var coreObs = httpObservable //Emit completed event
            .do(() => {
                this.eventEmitter.emit(info);
            },
            r => {
                
            });
        return coreObs;
    }
}
