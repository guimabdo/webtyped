import { HttpClient, HttpParams } from '@angular/common/http';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
export class WebTypedClient {

    constructor(
        private baseUrl: string,
        private api: string,
        private httpClient: HttpClient,
        private eventEmitter: WebTypedEventEmitterService) { }

    invokeGet<T>(info: WebTypedCallInfo<T>, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebTypedCallInfo<T>, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private invoke<T>(info: WebTypedCallInfo<T>, action: string,
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
            params: undefined
        };

        if (search) {
            var params = new HttpParams();
            for (var p in search) {
                var val = search[p];
                if (val === undefined) { continue; }
                if (val === null) { val = ""; }
                if (Array.isArray(val)) {
                    for (let i = 0; i < val.length; i++) {
                        params = params.append(p, val[i]);
                    }
                } else {
                    if (typeof val === "object") {
                        for (let i in val) { //Currently just getting first level of fields. TODO: See how asp.net webapi manage nested objects from uri and then adjust here
                            params = params.set(i, val[i]);
                        }
                    } else {
                        params = params.set(p, val);
                    }

                }
            }
            options.params = params;
        }
        
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
            .do(data => {
                info.result = data;
                this.eventEmitter.emit(info);
            },
            r => {
                
            });
        return coreObs;
    }
}
